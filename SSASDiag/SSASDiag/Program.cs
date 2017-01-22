using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace SSASDiag
{
    static class Program
    {
        public static string Case = "";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            string m_strPrivateTempBinPath = "";

            if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
                m_strPrivateTempBinPath = AppDomain.CurrentDomain.GetData("tempbinlocation") as string;
            else
                m_strPrivateTempBinPath =  new DirectoryInfo(Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\").FullName;

            // Setup custom app domain to launch real assembly from temp location, and act as singleton also...
            if (AppDomain.CurrentDomain.BaseDirectory != m_strPrivateTempBinPath)
            {
                bool bScheduleUpdateOfBin = false;
                string sNewBin = Environment.GetEnvironmentVariable("temp") + "\\ssasdiag_newbin_tmp";

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

                    // Check for new version but just spawn a new thread to do it without blocking...
                    new Thread(new ThreadStart(() =>
                        {
                            try
                            {
                                // This aspx page exposes the version number of the latest current build there to avoid having to download unnecessarily.
                                WebRequest req = HttpWebRequest.Create("http://jburchelsrv.southcentralus.cloudapp.azure.com/ssasdiagversion.aspx");
                                req.Method = "GET";
                                WebResponse wr = req.GetResponse();
                                string[] versionInfo = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd().Split('\n');
                                string version = "";
                                // We also return the case number cached per IP if any prior access to the server was made with QueryString value Case.
                                // We can use this to send a link to customers including a query string with case number.
                                // When they download the .exe, the case will be cached in session scope.
                                // Later when they run it and check version file, their local version will get case number to use locally in logging.
                                // For now only server guts implemented, and retrieval here, but not doing anything with it until tested further.
                                foreach (string v in versionInfo)
                                {
                                    if (v.Split('=')[0] == "Version") version = v.Split('=')[1];
                                    if (v.Split('=')[0] == "Case") Case = v.Split('=')[1];
                                }
                                if (version == "" || ServerFileIsNewer(FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileVersion, version))
                                {
                                    req = HttpWebRequest.Create("http://jburchelsrv.southcentralus.cloudapp.azure.com/ssasdiagdownload.aspx" + (Case == "" ? "" : "?Case=" + Case));
                                    req.Method = "GET";
                                    Stream newBin = File.OpenWrite(sNewBin);
                                    req.GetResponse().GetResponseStream().CopyTo(newBin);
                                    newBin.Close();
                                    FileVersionInfo fNew = FileVersionInfo.GetVersionInfo(sNewBin);
                                    FileVersionInfo fOld = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                                    if (fNew.FileMajorPart >  fOld.FileMajorPart || (fNew.FileMajorPart == fOld.FileMajorPart && fNew.FileMinorPart > fOld.FileMinorPart))
                                    {
                                        MessageBox.Show("SSASDiag has an update!  Restart to use the updated version.", "SSAS Diagnostics Collector Update Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        bScheduleUpdateOfBin = true;
                                    }
                                    else
                                        File.Delete(sNewBin);
                                }
                            }
                            catch (Exception ex) { Debug.WriteLine(ex); }
                        })).Start();
                    
                    AppDomainSetup ads = new AppDomainSetup();
                    ads.ApplicationBase = m_strPrivateTempBinPath;
                    AppDomain tempDomain = AppDomain.CreateDomain("SSASDiagTempDomain", null, ads);
                    tempDomain.SetData("tempbinlocation", m_strPrivateTempBinPath);
                    
                    tempDomain.SetData("originalbinlocation", currentAssembly.Location.Substring(0, currentAssembly.Location.LastIndexOf("\\")));
                    // Execute the domain.
                    ret = tempDomain.ExecuteAssemblyByName(currentAssembly.FullName, new string[] { "", "/templocation=" + m_strPrivateTempBinPath });

                    //  Finally unload our actual executing AppDomain after finished from temp directory, to delete the files there...
                    AppDomain.Unload(tempDomain);


                }
                catch (Exception ex)
                {
                    // This is the generic exception handler from the top level default AppDomain, 
                    // which catches any previously uncaught exceptions originating from the tempDomain.
                    // This should avoid any crashes of the app in theory, instead providing graceful error messaging.
                    //

                    string msg = "There was an unexpected error in the SSAS Diagnostics Collector and the application will close.  " 
                                    + "Details of the error are provided for debugging purposes, and copied on the clipboard.\r\n\r\n"
                                    + "An email will also be generated to the tool's author after you click OK.  Please paste the details there to report the issue.\r\n\r\n" 
                        + ex.Message + "\r\n at " + ex.TargetSite + ".";
                    if (ex.InnerException != null) msg += "\r\n\r\n Inner Exception: " + ex.InnerException.Message + "\r\n  at " + ex.InnerException.TargetSite + ".";

                    MessageBox.Show(msg, "Unexpected error in SSAS Diagnostics Collection", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Clipboard.SetData(DataFormats.StringFormat, "Error: " + ex.Message + " at " + ex.TargetSite + "\n"
                                        + ex.StackTrace
                                        + (ex.InnerException == null ? "" :
                                            "\n\n=====================\nInner Exception: " + ex.InnerException.Message + " at " + ex.InnerException.TargetSite + "\n"
                                            + ex.InnerException.StackTrace));

                    Process.Start("mailto:jon.burchel@mcirosoft.com?subject=" + "SSASDiag error");
                }


                try { Directory.Delete(m_strPrivateTempBinPath, true); } catch { /* ignore failures on cleanup */ }

                // Don't forget to kick off the file replacement copy to run 2s after we complete if there was a new version!  ;)
                if (bScheduleUpdateOfBin)
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.Arguments = "/c ping 127.0.0.1 -n 2 > nul & move /y \"" + sNewBin + "\" " + Assembly.GetEntryAssembly().Location;
                    p.Start();
                }

                // After the inner app domain exits
                Environment.ExitCode = ret;
                return;
            }

            // Launch application normally then...
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmSSASDiag());
        }

        private static bool ServerFileIsNewer(string clientFileVersion, string serverFile)
        {
            Version client = new Version(clientFileVersion);
            Version server = new Version(serverFile);
            return server > client;
        }
    }
}
