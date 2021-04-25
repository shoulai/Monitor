using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using WindowsDeskBand.DeskBand.BandParts;
using WPFBand;

namespace MonitorLibrary
{
    /// <summary>
    /// WPFDevelopersBandControl.xaml 的交互逻辑
    /// </summary>
    [ComVisible(true)]
    [Guid("eabd5a5b-4273-4fb8-a851-aa0d4b803534")]
    [BandRegistration(Name = "Monitor", ShowDeskBand = true)]
    public partial class TaskbarControl : WPFBandControl
    {

        [DllImport("shell32.dll")]
        private static extern IntPtr SHAppBarMessage(int msg, ref APPBARDATA data);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        private struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        private struct RECT
        {
            public int left, top, right, bottom;
        }
        private const int ABM_GETTASKBARPOS = 5;


        private SystemInfo systemInfo;
        private MonitorSetting monitorSetting;
        private MonitorAbout monitorAbout = new MonitorAbout();
        public TaskbarControl()
        {
            Options.MinHorizontalSize.Width = 70;
            //System.Drawing.Color taskBarColour = GetColourAt(GetTaskbarPosition().Location);
            //this.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(taskBarColour.R, taskBarColour.G, taskBarColour.B));
            InitializeComponent();

            if (IsLight())
                MonitorInfo.Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#FF3E3E3E"); 
            else
                MonitorInfo.Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#FFFFFF");
            StartMonitor();
            m_CPUCounter = new System.Diagnostics.PerformanceCounter();
            m_CPUCounter.CategoryName = "Processor";
            m_CPUCounter.CounterName = "% Processor Time";
            m_CPUCounter.InstanceName = "_Total";
            int x = (int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - 500);
            int y = (int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - 230);
            systemInfo = new SystemInfo();
            systemInfo.Left = x;
            systemInfo.Top = y;
            monitorSetting = new MonitorSetting(this);
            monitorSetting.Left = x;
            monitorSetting.Top = y;
            monitorSetting.Visibility = Visibility.Hidden;
            monitorAbout.Left = x;
            monitorAbout.Top = y;
            monitorAbout.Visibility = Visibility.Hidden;
        }



        private void StartMonitor()
        {
            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    SetMonitorInfo();
                    Thread.Sleep(800);
                }
            })).Start();
        }


        /// <summary>
        /// 委托设置内容
        /// </summary>
        /// <param name="msg"></param>
        private void SetMonitorInfo()
        {
            SetMonitorInfoCallback d = new SetMonitorInfoCallback(ExceMonitorInfo);
            Dispatcher.Invoke(d);
        }

        private void ExceMonitorInfo()

        {
            int cpu = GetCPU() + 1;
            int memory = GetMemory();
            //systemInfo.CPUInfo.Content = "CPU占用： " + (cpu < 10 ? "0" : "") + cpu + "%";
            //systemInfo.MemoryInfo.Content = "内存占用： " + memory + "%";
            systemInfo.CPUInfo.Text = (cpu < 10 ? "0" : "") + cpu+ "%";
            systemInfo.MemoryInfo.Text = +memory + "%";
            MonitorInfo.Text = string.Format("   CPU: " + (cpu < 10 ? "0" : "") + cpu + "%  \n  内存: " + memory + "%");
        }



        delegate void SetMonitorInfoCallback();






        private void ExitMonitor(object sender, RoutedEventArgs e)
        {
            this.OnClose();
            this.CloseDeskBand();
            ExecCMD("/nologo /unregister  \"{0}\"", "MonitorLibrary.dll");
        }

        public void ExecCMD(string args, string fileName)
        {
            try
            {
                string dllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                if (!File.Exists(dllPath))
                {
                    return;
                }
                string startArgs = string.Format(args, dllPath);

                Process p = new Process();
                p.StartInfo.FileName = "RegAsm";
                p.StartInfo.Arguments = startArgs;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
                p.Close();
                p.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        private System.Diagnostics.PerformanceCounter m_CPUCounter;


        public int GetCPU()
        {
            return (int)m_CPUCounter.NextValue();
        }

        public int GetMemory()
        {
            MemoryInfo MemInfo = new MemoryInfo();
            GlobalMemoryStatus(ref MemInfo);

            double totalMb = MemInfo.TotalPhysical / 1024 / 1024;
            double avaliableMb = MemInfo.AvailablePhysical / 1024 / 1024;

            return 100 - (int)Math.Round((avaliableMb / totalMb) * 100, 2);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MemoryInfo
        {
            public uint Length;
            public uint MemoryLoad;
            public ulong TotalPhysical;//总内存
            public ulong AvailablePhysical;//可用物理内存
            public ulong TotalPageFile;
            public ulong AvailablePageFile;
            public ulong TotalVirtual;
            public ulong AvailableVirtual;
        }

        [DllImport("kernel32")]
        public static extern void GlobalMemoryStatus(ref MemoryInfo meminfo);

        private void ShowSystemInfo(object sender, System.Windows.Input.MouseEventArgs e)
        {
            systemInfo.Visibility = Visibility.Visible;
        }

        private void HideSystemInfo(object sender, System.Windows.Input.MouseEventArgs e)
        {
            systemInfo.Visibility = Visibility.Hidden;
        }

        private void OpenSetting(object sender, RoutedEventArgs e)
        {
            monitorSetting.Visibility = Visibility.Visible;
        }

        private void MonitorAbout(object sender, RoutedEventArgs e)
        {
            monitorAbout.Visibility = Visibility.Visible;
        }

        private static Rectangle GetTaskbarPosition()
        {
            APPBARDATA data = new APPBARDATA();
            data.cbSize = Marshal.SizeOf(data);

            IntPtr retval = SHAppBarMessage(ABM_GETTASKBARPOS, ref data);
            if (retval == IntPtr.Zero)
            {
                throw new Win32Exception("error");
            }

            return new Rectangle(data.rc.left, data.rc.top, data.rc.right - data.rc.left, data.rc.bottom - data.rc.top);
        }

        private static System.Drawing.Color GetColourAt(System.Drawing.Point location)
        {
            using (Bitmap screenPixel = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }

                return screenPixel.GetPixel(0, 0);
            }
        }
        bool IsLight() 
        {
            bool isLightMode = true;
            try
            {
                var v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", "1");
                if (v != null && v.ToString() == "0")
                    isLightMode = false;
            }
            catch { }
            return isLightMode;
        }
    }

}
