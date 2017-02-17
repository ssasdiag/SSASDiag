using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.Win32;
using System.IO;
using System.Resources;
using System.Collections;
using System.Globalization;
using System.IO.Compression;
using System.Diagnostics;

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


                ResourceManager rm = Properties.Resources.ResourceManager;
                ResourceSet rs = rm.GetResourceSet(new CultureInfo("en-US"), true, true);
                IDictionaryEnumerator de = rs.GetEnumerator();
                while (de.MoveNext() == true)
                    if (de.Entry.Value is byte[])
                        try
                        {
                            File.WriteAllBytes(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\"
                                          + de.Key.ToString().Replace('_', '.') + (de.Key.ToString() == "Resources" ? ".zip" : ".exe"),
                                          de.Entry.Value as byte[]);
                        }
                        catch { } // may fail if file is in use, fine...

                foreach (string f in Directory.GetFiles(Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\"))
                    if (f.EndsWith(".zip"))
                        try
                        {
                            System.IO.Compression.ZipFile.ExtractToDirectory(f, Environment.GetEnvironmentVariable("temp") + "\\ASProfilerTraceImporter\\dll\\");
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message); } // I deliberately ignore this exception.  I may occasionally fail to extract if files are already there due to some prior crashed run or something.  Just move on from it.  If it is truly unrecoverable we will blow up later then.  :)           

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
