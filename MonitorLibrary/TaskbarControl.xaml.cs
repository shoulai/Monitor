using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
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
        private SystemInfo systemInfo;
        private MonitorSetting monitorSetting;
        public TaskbarControl()
        {
            Options.MinHorizontalSize.Width = 200;
            InitializeComponent();
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
            systemInfo.CPUInfo.Content = "CPU占用： " + (cpu < 10 ? "0" : "") + cpu + "%";
            systemInfo.MemoryInfo.Content = "内存占用： " + memory + "%";
            MonitorInfo.Text = "CPU: " + (cpu < 10 ? "0" : "") + cpu + "%  内存: " + memory + "%";
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
    }

}
