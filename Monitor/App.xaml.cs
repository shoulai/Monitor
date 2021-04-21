using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Monitor
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ExecCMD("/nologo /unregister  \"{0}\"", "MonitorLibrary.dll");
            ExecCMD("/nologo /codebase   \"{0}\"", "MonitorLibrary.dll");
            Thread.Sleep(1000);
            Environment.Exit(0);
        }
     

        public static void ExecCMD(string args, string fileName)
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


    }

}
