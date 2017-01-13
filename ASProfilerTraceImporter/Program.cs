using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;
using System.IO;

namespace ASProfilerTraceImporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ApplicationExit += Application_ApplicationExit;

            if (!Directory.Exists(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll")) Directory.CreateDirectory(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll");
            Environment.CurrentDirectory = Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter";

            AppDomainSetup ads = new AppDomainSetup();
            ads.PrivateBinPath = "dll";           

            if (!Directory.Exists(Environment.CurrentDirectory + "\\dll")) Directory.CreateDirectory(Environment.CurrentDirectory + "\\dll");
            if (!File.Exists(Environment.CurrentDirectory + "\\dll\\Microsoft.Data.ConnectionUI.dll")) File.WriteAllBytes(Environment.CurrentDirectory + "\\dll\\Microsoft.Data.ConnectionUI.dll", Properties.Resources.Microsoft_Data_ConnectionUI);
            if (!File.Exists(Environment.CurrentDirectory + "\\dll\\Microsoft.Data.ConnectionUI.Dialog.dll")) File.WriteAllBytes(Environment.CurrentDirectory + "\\dll\\Microsoft.Data.ConnectionUI.Dialog.dll", Properties.Resources.Microsoft_Data_ConnectionUI_Dialog);
            if (!File.Exists(Environment.CurrentDirectory + "\\dll\\Microsoft.SqlServer.ConnectionInfo.dll")) File.WriteAllBytes(Environment.CurrentDirectory + "\\dll\\SqlServer.ConnectionInfo.dll", Properties.Resources.Microsoft_SqlServer_ConnectionInfo);
            if (!File.Exists(Environment.CurrentDirectory + "\\dll\\Microsoft.SqlServer.ConnectionInfoExtended.dll")) File.WriteAllBytes(Environment.CurrentDirectory + "\\dll\\Microsoft.SqlServer.ConnectionInfoExtended.dll", Properties.Resources.Microsoft_SqlServer_ConnectionInfoExtended);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Set file extension to the way we want to handle it
            RegistryKey r;
            if (Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.trc\\Shell\\Import\\Command") == null)
                r = Registry.ClassesRoot.CreateSubKey("SystemFileAssociations").CreateSubKey(".trc").CreateSubKey("Shell").CreateSubKey("Import").CreateSubKey("Command", RegistryKeyPermissionCheck.ReadWriteSubTree);
            else
                r = Registry.ClassesRoot.CreateSubKey("Software\\Classes\\ASProfilerTraceImporter\\shell\\import\\command", RegistryKeyPermissionCheck.ReadWriteSubTree);
            r.SetValue("", "\"" + Application.ExecutablePath + "\" \"%1\"");
            Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.trc\\shell\\import", true).SetValue("", "&Import profiler trace to SQL table");

            Application.ThreadException += Application_ThreadException;
            try
            {
                Application.Run(new frmProfilerTraceImporter());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Oops!  Exception...");
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            Environment.CurrentDirectory = Environment.GetEnvironmentVariable("temp");
            Directory.Delete(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter", true);
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Oops!  Exception...");
        }
    }
}
