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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Set file extension to the way we want to handle it
            RegistryKey r;
            if (Registry.CurrentUser.OpenSubKey("Software\\Classes\\ASProfilerTraceImporter.Program") == null)
                r = Registry.CurrentUser.CreateSubKey("Software\\Classes\\ASProfilerTraceImporter.Program").CreateSubKey("shell").CreateSubKey("Open").CreateSubKey("Command");
            else
                r = Registry.CurrentUser.CreateSubKey("Software\\Classes\\ASProfilerTraceImporter.Program\\shell\\open\\command", RegistryKeyPermissionCheck.ReadWriteSubTree);
            r.SetValue("", "\"" + Application.ExecutablePath + "\" \"%1\"");
            if (Registry.CurrentUser.OpenSubKey("Software\\Classes\\.trc") == null)
                r = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.trc", RegistryKeyPermissionCheck.ReadWriteSubTree);
            else
                r = Registry.CurrentUser.OpenSubKey("Software\\Classes\\.trc", true);
            if (r.OpenSubKey("OpenWithProgids") == null)
                r = r.CreateSubKey("OpenWithProgids");
            else
                r = r.OpenSubKey("OpenWithProgids", true);
            r.SetValue("ASProfilerTraceImporter.Program", "");

            Application.Run(new frmProfilerTraceImporter());
        }
    }
}
