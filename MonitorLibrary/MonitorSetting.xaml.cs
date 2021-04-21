using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MonitorLibrary
{
    /// <summary>
    /// MonitorSetting.xaml 的交互逻辑
    /// </summary>
    public partial class MonitorSetting : Window
    {
        private TaskbarControl monitor;

        public MonitorSetting()
        {
        }

        public MonitorSetting(TaskbarControl monitor)
        {
            this.monitor = monitor;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void SettingFontColor_Click(object sender, RoutedEventArgs e)
        {
            string taskFontColor = this.taskFontColor.Text;
            Regex r = new Regex("#[a-fA-F0-9]{6}");
            if (!r.IsMatch(taskFontColor))
            {
                Err.Content = "格式错误。例(#F5FFFA)";
                return;
            }
            Err.Content = "";
            monitor.MonitorInfo.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(taskFontColor));
        }
    }

    public class RegAsmHelpers
    {
        private static readonly Lazy<RegAsmHelpers> lazy =
      new Lazy<RegAsmHelpers>(() => new RegAsmHelpers());

        public static RegAsmHelpers Instance { get { return lazy.Value; } }
        private RegAsmHelpers()
        {
        }
        public bool InstallRegAsm(string args, string fileName)
        {
            bool result = true;
            try
            {
                string dllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                if (!File.Exists(dllPath))
                {
                    return false;
                }
                string startArgs = string.Format(args, dllPath);

                Process p = new Process();
                p.StartInfo.FileName = "RegAsm";
                p.StartInfo.Arguments = startArgs;
                p.StartInfo.CreateNoWindow = true;

                WindowsIdentity winIdentity = WindowsIdentity.GetCurrent();
                WindowsPrincipal winPrincipal = new WindowsPrincipal(winIdentity);
                if (!winPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    p.StartInfo.Verb = "runas";
                }
                p.Start();
                p.WaitForExit();
                p.Close();
                p.Dispose();
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public bool StrartExplorer()
        {
            var result = false;
            string explorer = string.Format("{0}\\{1}", Environment.GetEnvironmentVariable("WINDIR"), "explorer.exe");
            using (var p = new Process())
            {
                p.StartInfo.FileName = explorer;
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
                p.Close();
                result = true;
            }

            return result;
        }

        public bool CloseExplorer(string str)
        {
            var result = false;
            string cmdline = $"{str}";
            using (var p = new Process())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;

                p.Start();
                p.StandardInput.AutoFlush = true;
                p.StandardInput.WriteLine(cmdline + " &exit");

                p.WaitForExit();
                p.Close();
                result = true;
            }
            return result;
        }
        }
    }
