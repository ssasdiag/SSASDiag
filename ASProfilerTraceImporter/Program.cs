using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
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
            // yes it's a hack to get started...
            if (AppDomain.CurrentDomain.BaseDirectory != Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll")
            {
                // Set file extension to the way we want to handle it
                RegistryKey r;
                if (Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.trc\\Shell\\Import\\Command") == null)
                    r = Registry.ClassesRoot.CreateSubKey("SystemFileAssociations").CreateSubKey(".trc").CreateSubKey("Shell").CreateSubKey("Import").CreateSubKey("Command", RegistryKeyPermissionCheck.ReadWriteSubTree);
                else
                    r = Registry.ClassesRoot.CreateSubKey("Software\\Classes\\ASProfilerTraceImporter\\shell\\import\\command", RegistryKeyPermissionCheck.ReadWriteSubTree);
                r.SetValue("", "\"" + Application.ExecutablePath + "\" \"%1\"");
                Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.trc\\shell\\import", true).SetValue("", "&Import profiler trace to SQL table");

                if (!Directory.Exists(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll"))
                    Directory.CreateDirectory(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll").Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                else
                    foreach (string f in Directory.EnumerateFiles(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll"))  File.Delete(f); 

                if (!File.Exists(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\Microsoft.Data.ConnectionUI.dll")) File.WriteAllBytes(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\Microsoft.Data.ConnectionUI.dll", Properties.Resources.Microsoft_Data_ConnectionUI);
                if (!File.Exists(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\Microsoft.Data.ConnectionUI.Dialog.dll")) File.WriteAllBytes(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\Microsoft.Data.ConnectionUI.Dialog.dll", Properties.Resources.Microsoft_Data_ConnectionUI_Dialog);
                if (!File.Exists(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\Microsoft.SqlServer.ConnectionInfo.dll")) File.WriteAllBytes(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\Microsoft.SqlServer.ConnectionInfo.dll", Properties.Resources.Microsoft_SqlServer_ConnectionInfo);
                if (!File.Exists(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\Microsoft.SqlServer.ConnectionInfoExtended.dll")) File.WriteAllBytes(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\Microsoft.SqlServer.ConnectionInfoExtended.dll", Properties.Resources.Microsoft_SqlServer_ConnectionInfoExtended);
                if (!File.Exists(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\AS Profiler Trace Importer.exe")) File.Copy(Application.ExecutablePath, Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\AS Profiler Trace Importer.exe");

                var currentAssembly = Assembly.GetExecutingAssembly();
                AppDomainSetup ads = new AppDomainSetup();
                ads.ApplicationBase = Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll";
                var otherDomain = AppDomain.CreateDomain("other domain", null, ads);

                var ret = otherDomain.ExecuteAssemblyByName(
                                            currentAssembly.FullName,
                                            new string[] { });

                Environment.ExitCode = ret;
                return;
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

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
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Oops!  Exception...");
        }
    }
}
