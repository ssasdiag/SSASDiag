using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;
using System.Globalization;
using System.Collections;
using System.IO;
using System.Reflection;

namespace SSASDiag
{
    static class Program
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string m_strPrivateTempBinPath = Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\";

            // Setup custom app domain to launch real assembly from temp location, and act as singleton also...
            if (AppDomain.CurrentDomain.BaseDirectory != m_strPrivateTempBinPath || AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                int ret = 0;

                // Extract all embedded file type (byte[]) resource assemblies and copy self into temp location
                ResourceManager rm = Properties.Resources.ResourceManager;
                ResourceSet rs = rm.GetResourceSet(new CultureInfo("en-US"), true, true);
                IDictionaryEnumerator de = rs.GetEnumerator();
                Directory.CreateDirectory(m_strPrivateTempBinPath);
                while (de.MoveNext() == true)
                    if (de.Entry.Value is byte[])
                        try { File.WriteAllBytes(m_strPrivateTempBinPath + de.Key.ToString().Replace('_', '.') + ".dll", de.Entry.Value as byte[]); } catch { } // may fail if file is in use, fine...
                try { File.Copy(Application.ExecutablePath, Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\SSASDiag.exe", true); } catch { } // may fail if file is in use, fine...

                try
                {
                    // Initialize the new app domain from temp location...
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    AppDomainSetup ads = new AppDomainSetup();
                    ads.ApplicationBase = m_strPrivateTempBinPath;
                    AppDomain tempDomain = AppDomain.CreateDomain("SSASDiagTempDomain", null, ads);

                    // Execute the domain.
                    ret = tempDomain.ExecuteAssemblyByName(currentAssembly.FullName, new string[] { });

                    //  Finally unload our actual executing AppDomain after finished from temp directory, to delete the files there...
                    AppDomain.Unload(tempDomain);
                }
                catch
                {
                    // This is the generic exception handler from the top level default AppDomain, 
                    // which catches any previously uncaught exceptions originating from the tempDomain.
                    // This should avoid any crashes of the app in theory, instead providing graceful error messaging.
                    //

                    // not implemented now, not sure it is needed either...

                    // MessageBox.Show("Unexpected error in SSAS Diagnostics Collection", e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


                try { Directory.Delete(m_strPrivateTempBinPath, true); } catch { /* ignore failures on cleanup */ }

                // After the inner app domain exits
                Environment.ExitCode = ret;
                return;
            }

            // Launch application normally then...
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmSSASDiag());
        }
    }
}
