using System.DirectoryServices.AccountManagement;
using System.ServiceProcess;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO.Compression;
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
using System.Linq;

namespace SSASDiag
{
    static class Program
    {
        public static Guid RunID;
        public static frmSSASDiag MainForm;
        public static string TempPath = "";

        /// <summary> /// 
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if (!Environment.UserInteractive)
            {
                // In service mode we wipe out command line of startup config after we complete.
                // If somebody tries to restart the service then, we want to immediately shutdown.
                // They only way to launch is through the UI.
                // UI is intended mechanism to shutdown too, but if user shuts down service, we trigger stop of collection properly too.
                if (Environment.GetCommandLineArgs().Length == 3)
                {
                    if (Environment.GetCommandLineArgs()[1] == "/instance")
                    {
                        ProcessStartInfo p = new ProcessStartInfo("cmd.exe", "/c ping 1.1.1.1 -n 1 -w 1500 > nul & net stop SSASDiag_" + Environment.GetCommandLineArgs()[2]);
                        p.WindowStyle = ProcessWindowStyle.Hidden;
                        p.UseShellExecute = true;
                        p.Verb = "runas";
                        p.CreateNoWindow = true;
                        Process.Start(p);
                        return;
                    }
                }
            }

            // Assign a unique Run ID used to anonymously track usage if user allows
            RunID = Guid.NewGuid();

            // Check for .NET 4.6.1 or later.
            object ReleaseVer = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full").GetValue("Release");
            if (ReleaseVer == null || Convert.ToInt32(ReleaseVer) <= 378389)
            {
                if (Environment.UserInteractive && MessageBox.Show("SSASDiag requires .NET 4.5 or later and will exit now.\r\nInstall the latest .NET release now?", ".NET Update Required", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    Process.Start("https://download.microsoft.com/download/F/9/4/F942F07D-F26F-4F30-B4E3-EBD54FABA377/NDP462-KB3151800-x86-x64-AllOS-ENU.exe");
                return;
            }

            // Setup private temp bin location...
            if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
                TempPath = AppDomain.CurrentDomain.GetData("tempbinlocation") as string;
            else
                TempPath =  new DirectoryInfo(Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\").FullName;

            // Setup custom app domain to launch real assembly from temp location, and act as singleton also...
            if (AppDomain.CurrentDomain.BaseDirectory != TempPath)
            {
                int ret = 0;

                // Extract all embedded file type (byte[]) resource assemblies and copy self into temp location
                ResourceManager rm = Properties.Resources.ResourceManager;
                ResourceSet rs = rm.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
                IDictionaryEnumerator de = rs.GetEnumerator();
                Directory.CreateDirectory(TempPath);
                while (de.MoveNext() == true)
                    if (de.Entry.Value is byte[])
                        try {
                            
                            if (de.Key.ToString() == "ResourcesZip")
                            {
                                byte[] b = de.Entry.Value as byte[];
                                if (!File.Exists(TempPath + "Resources.zip") ||
                                        (File.Exists(TempPath + "Resources.zip") &&
                                         new FileInfo(TempPath + "Resources.zip").Length != b.Length) 
                                    )
                                    File.WriteAllBytes(TempPath + "Resources.zip", b);
                                break;
                            }
                        } catch { } // may fail if file is in use, fine...

                // Do non-immediately-essential out-of-proc exe resource extractions off thread to speed startup...
                de.Reset();
                new Thread(new ThreadStart(() =>
                {
                    while (de.MoveNext() == true)
                        if (!File.Exists(TempPath + de.Key.ToString().Replace('_', '.') + ".exe") 
                            && de.Entry.Value is byte[] && de.Key.ToString() != "ResourcesZip")
                                File.WriteAllBytes(TempPath + de.Key.ToString().Replace('_', '.') + ".exe", de.Entry.Value as byte[]);
                })).Start();

                // Symbolic debugger binaries required for dump parsing. 
                // Silent, 'impactless' (completely asynchronously off main thread), minimal subset of the standard debugging tools required for cdb.exe.
                // Copied simply into %Program Files%\CDB.
                InstallCDB();

                int iCopyTries = 0;
                while (iCopyTries < 3)
                {
                    try
                    {
                        if (!File.Exists(TempPath + "SSASDiag.exe")
                            || (File.Exists(TempPath + "SSASDiag.exe") && !File.ReadAllBytes(TempPath + "SSASDiag.exe").SequenceEqual(File.ReadAllBytes(Application.ExecutablePath)))
                            || Debugger.IsAttached
                            || !Environment.UserInteractive)
                            File.Copy(Application.ExecutablePath, Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\SSASDiag.exe", true);
                        break;
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("Exception copying binary to temp cache: " + e.Message);
                        Thread.Sleep(100);
                        iCopyTries++;
                    } 
                }

                // Now decompress any compressed files we include.  This lets us cram more dependencies in as we add features and still not excessively bloat!  :D
                // Although in our real compression work in assembling files for upload we will use the more flexible open source Ionic.Zip library included in our depenencies,
                // these may not be loaded initially when are launching the first time outside the sandbox.  So here we will use .NET built in compression, at least always there.
                foreach (string f in Directory.GetFiles(TempPath))
                    if (f.EndsWith(".zip"))
                    {
                        try
                        {
                            // Extract any dependencies required for initial form display on main thread...
                            ZipArchive za = ZipFile.OpenRead(f);
                            if (!File.Exists(TempPath + za.GetEntry("FastColoredTextBox.dll").Name) || 
                                   (File.Exists(TempPath + za.GetEntry("FastColoredTextBox.dll").Name) && 
                                    new FileInfo(TempPath + za.GetEntry("FastColoredTextBox.dll").Name).Length != za.GetEntry("FastColoredTextBox.dll").Length)
                                )
                                za.GetEntry("FastColoredTextBox.dll").ExtractToFile(TempPath + "FastColoredTextBox.dll", true);

                            // Extract remainig non-immediate depencies off main thread to improve startup time...
                            new Thread(new ThreadStart(() =>
                            {
                                foreach (ZipArchiveEntry ze in za.Entries)
                                {
                                    try
                                    {
                                        if (!File.Exists(TempPath + ze.Name) || 
                                                (File.Exists(TempPath + ze.Name) && new FileInfo(TempPath + ze.Name).Length != ze.Length)
                                            )
                                            ze.ExtractToFile(TempPath + ze.Name, true);
                                    }
                                    catch (Exception ex2)
                                    {
                                        Trace.WriteLine("Extraction Exception: " + ex2.Message);
                                    }
                                }
                            }
                            )).Start();
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Extraction Exception: " + ex.Message);
                        }
                    }
                try
                {
                    // Initialize the new app domain from temp location...
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    

                    AppDomainSetup ads = new AppDomainSetup();
                    ads.ApplicationBase = TempPath;
                    AppDomain tempDomain = AppDomain.CreateDomain("SSASDiagTempDomain", null, ads);
                    tempDomain.SetData("tempbinlocation", TempPath);

                    if (Properties.Settings.Default.AutoUpdate == "true" && Environment.UserInteractive)
                        CheckForUpdates(tempDomain);
                    
                    tempDomain.SetData("originalbinlocation", currentAssembly.Location.Substring(0, currentAssembly.Location.LastIndexOf("\\")));
                    // Execute the domain.
                    ret = tempDomain.ExecuteAssemblyByName(currentAssembly.FullName);
                }
                catch (AppDomainUnloadedException ex)
                {
                    /* This happens normally if we terminate due to update process... */
                    Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
                }
                catch (Exception ex)
                {
                    // This is the generic exception handler from the top level default AppDomain, 
                    // which catches any previously uncaught exceptions originating from the tempDomain.
                    // This should avoid any crashes of the app in theory, instead providing graceful error messaging.
                    //
                    Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
                    string msg = "There was an unexpected error in the SSAS Diagnostics Collector and the application will close.  " 
                                    + "Details of the error are provided for debugging purposes, and copied on the clipboard.\r\n\r\n"
                                    + "An email will also be generated to the tool's author after you click OK.  Please paste the details there to report the issue.\r\n\r\n" 
                        + ex.Message + "\r\n at " + ex.TargetSite + ".";
                    if (ex.InnerException != null) msg += "\r\n\r\n Inner Exception: " + ex.InnerException.Message + "\r\n  at " + ex.InnerException.TargetSite + ".";

                    if (Environment.UserInteractive)
                        MessageBox.Show(msg, "Unexpected error in SSAS Diagnostics Collection", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (Environment.UserInteractive)
                        Clipboard.SetData(DataFormats.StringFormat, "Error: " + ex.Message + " at " + ex.TargetSite + "\n"
                                        + ex.StackTrace
                                        + (ex.InnerException == null ? "" :
                                            "\n\n=====================\nInner Exception: " + ex.InnerException.Message + " at " + ex.InnerException.TargetSite + "\n"
                                            + ex.InnerException.StackTrace));

                    if (Environment.UserInteractive)
                        Process.Start("mailto:jon.burchel@mcirosoft.com?subject=" + WebUtility.UrlEncode("SSASDiag error at " + ex.TargetSite));
                }

                // After the inner app domain exits
                Environment.ExitCode = ret;
                return;
            }

            // Launch application normally then...
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                MainForm = new frmSSASDiag();
                Application.ApplicationExit += Application_ApplicationExit;
                Application.Run(MainForm);  
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
                if (Environment.UserInteractive)
                    MessageBox.Show("SSASDiag encountered an unexpected exception:\n\t" + ex.Message + "\n\tat\n" + ex.StackTrace, "SSASDiag Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            System.Diagnostics.Trace.WriteLine("Exiting SSASDiag.");
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (!Environment.UserInteractive)
            {
                if (MainForm != null && MainForm.btnCapture.Image.Tag as string == "Stop")
                {
                    MainForm.btnCapture.PerformClick();
                    while (MainForm.btnCapture.Image.Tag as string != "Start")
                        Thread.Sleep(5000);
                }
                // Reinitialize service config to start with only option to specify instance, which will trigger stop of service hereafter in service mode then, until UI configures new settings, should someone try to start manually.
                string svcIniPath = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\services\\SSASDiag_" + (MainForm.cbInstances.SelectedIndex == 0 ? "MSSQLSERVER" : MainForm.cbInstances.Text)).GetValue("ImagePath") as string;
                svcIniPath = svcIniPath.Substring(0, svcIniPath.IndexOf(".exe")) + ".ini";
                List<string> svcConfig = new List<string>();
                if (File.Exists(svcIniPath))
                    svcConfig = new List<string>(File.ReadAllLines(svcIniPath));
                svcConfig[svcConfig.FindIndex(s => s.StartsWith("CommandLine="))] = "CommandLine=" + (AppDomain.CurrentDomain.GetData("originalbinlocation") as string) + "\\SSASDiag.exe /instance " + (MainForm.cbInstances.SelectedIndex == 0 ? "MSSQLSERVER" : MainForm.cbInstances.Text);
                File.WriteAllLines(svcIniPath, svcConfig.ToArray());
            }
        }

        public static void CheckForUpdates(AppDomain tempDomain)
        {
            string sNewBin = Program.TempPath + "newbin_tmp";

            // Check for new version but just spawn a new thread to do it without blocking...
            new Thread(new ThreadStart(() =>
            {
                try
                {
                    // This aspx page exposes the version number of the latest current build there to avoid having to download unnecessarily.
                    WebRequest req = HttpWebRequest.Create(Uri.EscapeUriString("http://jburchelsrv.southcentralus.cloudapp.azure.com/ssasdiagversion.aspx"));
                    req.Method = "GET";
                    WebResponse wr = req.GetResponse();
                    string[] versionInfo = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd().Split('\n');
                    string version = "";
                    foreach (string v in versionInfo)
                        if (v.Split('=')[0] == "Version") version = v.Split('=')[1];

                    
                    if (version == "" || ServerFileIsNewer(FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileVersion, version))
                    {
                        req = HttpWebRequest.Create(Uri.EscapeUriString("http://jburchelsrv.southcentralus.cloudapp.azure.com/ssasdiagdownload.aspx"));
                        req.Method = "GET";
                        Stream newBin = File.OpenWrite(sNewBin);
                        req.GetResponse().GetResponseStream().CopyTo(newBin);
                        newBin.Close();
                        if (MessageBox.Show("SSASDiag has an update!  Restart the tool to use the updated version?", "SSAS Diagnostics Collector Update Available", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                        {
                            Process p = new Process();
                            p.StartInfo.UseShellExecute = true;
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            p.StartInfo.FileName = "cmd.exe";
                            string AssemblyLocation = (string)(AppDomain.CurrentDomain.GetData("originalbinlocation") as string != "" ? AppDomain.CurrentDomain.GetData("originalbinlocation") : Assembly.GetExecutingAssembly().Location);
                            p.StartInfo.Arguments = "/c ping 1.1.1.1 -n 1 -w 1500 > nul & move /y \"" + sNewBin + "\" \"" +
                                                     AssemblyLocation + "\\SSASDiag.exe\" & " +
                                                     "del /q \"" + TempPath + "SSASDiag.exe\" & " + 
                                                     AssemblyLocation + "\\SSASDiag.exe\"";
                            p.Start();
                            Application.Exit();
                            return;
                        }
                    }
                }
                catch (Exception ex) { Trace.WriteLine("Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace); }
            })).Start();
        }

        static private void InstallCDB()
        {
            new Thread(new ThreadStart(() =>
            {
                if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\CDB\\cdb.exe"))
                {
                    if (!File.Exists(TempPath + "\\cdb.zip"))
                    {
                        WebRequest req = HttpWebRequest.Create(Uri.EscapeUriString("http://jburchelsrv.southcentralus.cloudapp.azure.com/cdb.zip"));
                        req.Method = "GET";
                        Stream newBin = File.OpenWrite(TempPath + "\\cdb.zip");
                        req.GetResponse().GetResponseStream().CopyTo(newBin);
                    }
                    ZipArchive zf = ZipFile.OpenRead(TempPath + "\\cdb.zip");
                    zf.ExtractToDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\CDB");
                }
            }
            )).Start();
        }

        private static bool ServerFileIsNewer(string clientFileVersion, string serverFile)
        {
            Version client = new Version(clientFileVersion);
            Version server = new Version(serverFile);
            return server > client;
        }
    }
}
