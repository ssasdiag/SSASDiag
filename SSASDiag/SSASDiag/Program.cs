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
            // Setup custom app domain to launch real assembly from temp location, and act as singleton also...
            if (AppDomain.CurrentDomain.BaseDirectory != Environment.GetEnvironmentVariable("temp") + "\\SSASDiag" || AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                // Extract all embedded file type (byte[]) resource assemblies and copy self into temp location
                ResourceManager rm = Properties.Resources.ResourceManager;
                ResourceSet rs = rm.GetResourceSet(new CultureInfo("en-US"), true, true);
                IDictionaryEnumerator de = rs.GetEnumerator();
                Directory.CreateDirectory(Environment.GetEnvironmentVariable("temp") + "\\SSASDiag");
                while (de.MoveNext() == true)
                    if (de.Entry.Value is byte[])
                        File.WriteAllBytes(Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\" + de.Key.ToString().Replace('_', '.') + ".dll", de.Entry.Value as byte[]);
                File.Copy(Application.ExecutablePath, Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\SSASDiag.exe", true);

                // Initialize the new app domain from temp location...
                var currentAssembly = Assembly.GetExecutingAssembly();
                AppDomainSetup ads = new AppDomainSetup();
                ads.ApplicationBase = Environment.GetEnvironmentVariable("temp") + "\\SSASDiag";
                AppDomain tempDomain = AppDomain.CreateDomain("SSASDiagTempDomain", null, ads);
                int ret = tempDomain.ExecuteAssemblyByName(currentAssembly.FullName, new string[] { });

                //  Finally unload our actual executing AppDomain after finished from temp directory, to delete the files there...
                AppDomain.Unload(tempDomain);
                try { Directory.Delete(Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\", true); } catch { /* ignore failures on cleanup */ }

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
