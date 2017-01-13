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
            AppDomainSetup ads = new AppDomainSetup();
            ads.PrivateBinPath = "dll";


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

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Oops!  Exception...");
        }
    }
}
