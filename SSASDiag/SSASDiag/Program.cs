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
        public static string LaunchingUser = "";
        public static bool InitializationComplete = false;
       

        /// <summary> /// 
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            SetupDebugTraceAndDumps();

            if (!Environment.UserInteractive)
            {
                // In service mode we wipe out command line of startup config after we complete.
                // If somebody tries to restart the service then, we want to immediately shutdown.
                // They only way to launch is through the UI.
                // UI is intended mechanism to shutdown too, but if user shuts down service, we trigger stop of collection properly too.
                if (Environment.GetCommandLineArgs().Length == 3)
                {
                    Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Started service mode.");
                    if (Environment.GetCommandLineArgs()[1] == "/instance")
                    {
                        ProcessStartInfo p = new ProcessStartInfo("cmd.exe", "/c ping 1.1.1.1 -n 2 -w 1000 > nul & net stop SSASDiag_" + Environment.GetCommandLineArgs()[2]);
                        p.WindowStyle = ProcessWindowStyle.Hidden;
                        p.UseShellExecute = true;
                        p.Verb = "runas";
                        p.CreateNoWindow = true;
                        ShutdownDebugTrace();
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
                Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ":  .NET 4.5 was missing, process stopping.");
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
                frmStatusFloater SplashScreen = new frmStatusFloater();
                SplashScreen.Left = Screen.PrimaryScreen.Bounds.Width / 2 - SplashScreen.Width / 2;
                SplashScreen.Top = Screen.PrimaryScreen.Bounds.Height / 2 - SplashScreen.Height / 2;
                SplashScreen.closeSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "SSASDiagSplashscreenInitializedEvent");
                SplashScreen.lblStatus.Top -= 8;
                SplashScreen.lblStatus.Font = new System.Drawing.Font(SplashScreen.lblSubStatus.Font.Name, 16);
                SplashScreen.lblStatus.Text = "Initializing SSAS Diagnostics Tool";
                SplashScreen.Controls.Add(new Label() { Text = "v" + Application.ProductVersion, Font = new System.Drawing.Font(SplashScreen.lblStatus.Font.Name, 8), Top = SplashScreen.lblStatus.Top + 15, Left = SplashScreen.lblStatus.Left + SplashScreen.lblStatus.Width, ForeColor = SplashScreen.lblStatus.ForeColor });

                SplashScreen.Show();
                SplashScreen.imgAnimation.Image = Properties.Resources.DatabaseLogo;
                SplashScreen.imgAnimation.Size = new System.Drawing.Size(52, 52);
                SplashScreen.imgAnimation.Top = SplashScreen.Height / 2 - SplashScreen.imgAnimation.Height / 2;
                SplashScreen.imgAnimation.Left = 30;
                SplashScreen.imgAnimation.SizeMode = PictureBoxSizeMode.StretchImage;
                SplashScreen.Refresh();


                Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Initializing temp location at " + TempPath);
                int ret = 0;

                // Extract all embedded file type (byte[]) resource assemblies and copy self into temp location
                ResourceManager rm = Properties.Resources.ResourceManager;
                ResourceSet rs = rm.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
                IDictionaryEnumerator de = rs.GetEnumerator();
                Directory.CreateDirectory(TempPath);
                SplashScreen.lblSubStatus.Text = "Extracting resources...";
                SplashScreen.Refresh();
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
                                Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Extracted resources zip.");
                                break;
                            }
                        } catch { } // may fail if file is in use, fine...

                de.Reset();
                while (de.MoveNext() == true)
                    if (!File.Exists(TempPath + de.Key.ToString().Replace('_', '.') + ".exe")
                        && de.Entry.Value is byte[] && de.Key.ToString() != "ResourcesZip")
                    {
                        File.WriteAllBytes(TempPath + de.Key.ToString().Replace('_', '.') + ".exe", de.Entry.Value as byte[]);
                        Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Extracted temp file " + de.Key.ToString().Replace('_', '.') + ".exe");

                    }

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
                        {
                            SplashScreen.lblSubStatus.Text = "Setting up temporary application domain...";
                            SplashScreen.Refresh();
                            File.Copy(Application.ExecutablePath, Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\SSASDiag.exe", true);
                            Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Copied SSASDiag.exe to temp location.");
                        }
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Exception copying binary to temp cache: " + e.Message);
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
                            SplashScreen.lblSubStatus.Text = "Extracting dependent binaries...";
                            SplashScreen.Refresh();

                            // Extract any dependencies required for initial form display on main thread...
                            ZipArchive za = ZipFile.OpenRead(f);
                            if (!File.Exists(TempPath + za.GetEntry("FastColoredTextBox.dll").Name) ||
                                   (File.Exists(TempPath + za.GetEntry("FastColoredTextBox.dll").Name) &&
                                    new FileInfo(TempPath + za.GetEntry("FastColoredTextBox.dll").Name).Length != za.GetEntry("FastColoredTextBox.dll").Length)
                                )
                            {
                                za.GetEntry("FastColoredTextBox.dll").ExtractToFile(TempPath + "FastColoredTextBox.dll", true);
                                Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Extracted FastColoredTextBox.dll to temp path.");
                            }


                            foreach (ZipArchiveEntry ze in za.Entries)
                            {
                                try
                                {
                                    if (!File.Exists(TempPath + ze.Name) ||
                                            (File.Exists(TempPath + ze.Name) && new FileInfo(TempPath + ze.Name).Length != ze.Length)
                                        )
                                    {
                                        ze.ExtractToFile(TempPath + ze.Name, true);
                                        Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Extracted " + ze.Name + " to temp path.");
                                    }
                                }
                                catch (Exception ex2)
                                {
                                    Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Extraction Exception: " + ex2.Message);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Extraction Exception: " + ex.Message);
                        }
                    }
                try
                {
                    // Initialize the new app domain from temp location...
                    var currentAssembly = Assembly.GetExecutingAssembly();

                    SplashScreen.lblSubStatus.Text = "Starting application...";
                    SplashScreen.Refresh();
                    
                    AppDomainSetup ads = new AppDomainSetup();
                    ads.ApplicationBase = TempPath;
                    AppDomain tempDomain = AppDomain.CreateDomain("SSASDiagTempDomain", null, ads);
                    tempDomain.SetData("tempbinlocation", TempPath);                    
                    tempDomain.SetData("originalbinlocation", currentAssembly.Location.Substring(0, currentAssembly.Location.LastIndexOf("\\")));
                    Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Preparing to launch executable from temp domain.");
                    Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Launch location: " + currentAssembly.Location);
                    ShutdownDebugTrace();
                    // Execute the domain.
                    ret = tempDomain.ExecuteAssemblyByName(currentAssembly.FullName);
                }
                catch (AppDomainUnloadedException ex)
                {
                    /* This happens normally if we terminate due to update process... */
                    Trace.WriteLine(Program.CurrentFormattedLocalDateTime() + ": AppDomainUnloaded Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
                }
                catch (Exception ex)
                {
                    // This is the generic exception handler from the top level default AppDomain, 
                    // which catches any previously uncaught exceptions originating from the tempDomain.
                    // This should avoid any crashes of the app in theory, instead providing graceful error messaging.
                    //
                    Trace.WriteLine(Program.CurrentFormattedLocalDateTime() + ": AppDomain Initialization Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
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
                Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Starting SSASDiag UI for RunID " + RunID);
                Application.Run(MainForm);  
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": General Exception:\r\n" + ex.Message + "\r\n at stack:\r\n" + ex.StackTrace);
                if (Environment.UserInteractive)
                    MessageBox.Show("SSASDiag encountered an unexpected exception:\n\t" + ex.Message + "\n\tat\n" + ex.StackTrace, "SSASDiag Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Exiting SSASDiag.");
            File.Delete("SSASDiag.exe.config");
        }

        public static void SetupDebugTraceAndDumps()
        {
            string binlocation = AppDomain.CurrentDomain.GetData("originalbinlocation") as string;
            if (binlocation == null) binlocation = Environment.CurrentDirectory;

            if (Environment.GetCommandLineArgs().Select(s=>s.ToLower()).Contains("/debug") || Registry.LocalMachine.CreateSubKey(@"SOFTWARE\SSASDiag").GetValue("LoggingEnabled", "False") as string == "True")
            {
                bool DebugListener = false;
                foreach (TraceListener l in Trace.Listeners)
                    if (l.Name == "debuglistener") DebugListener = true;
                if (!DebugListener)
                {
                    if (Environment.GetCommandLineArgs().Select(s => s.ToLower()).Contains("/debug") || Registry.LocalMachine.CreateSubKey(@"Software\SSASDiag").GetValue("LoggingEnabled", "True") as string == "True")
                        Trace.Listeners.Add(new TextWriterTraceListener(binlocation + (Environment.UserInteractive ? "\\SSASDiagDebugTrace.log" : "\\SSASDiagServiceDebugTrace.log"), "debuglistener"));
                    Trace.AutoFlush = true;
                }
            }

            RegistryKey dumpkey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps\\SSASDiag.exe");
            dumpkey.SetValue("DumpFolder", binlocation, RegistryValueKind.String);
            dumpkey.SetValue("DumpCount", 3, RegistryValueKind.DWord);
            dumpkey.SetValue("DumpType", 2, RegistryValueKind.DWord);
            dumpkey.Close();
            dumpkey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps\\SSASDiagServiceWorker.exe");
            dumpkey.SetValue("DumpFolder", binlocation, RegistryValueKind.String);
            dumpkey.SetValue("DumpCount", 3, RegistryValueKind.DWord);
            dumpkey.SetValue("DumpType", 2, RegistryValueKind.DWord);
            dumpkey.Close();
        }

        public static void ShutdownDebugTrace()
        {
            if (Trace.Listeners["debuglistener"] != null) Trace.Listeners["debuglistener"].Close();
            Trace.Listeners.Remove("debuglistener");
        }

        static private void InstallCDB()
        {
            Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Installing CDB in the background for debugger analysis.");
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
                        Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": CDB zip downloaded.");
                    }
                    if (File.Exists(TempPath + "\\cdb.zip"))
                    {
                        try
                        {
                            ZipArchive zf = ZipFile.OpenRead(TempPath + "\\cdb.zip");
                            zf.ExtractToDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\CDB");
                            Debug.WriteLine(Program.CurrentFormattedLocalDateTime() + ": CDB extracted and installed.");
                        }
                        catch { }  // low priority action.  ignore all failures.
                    }
                }
            }
            )).Start();
        }

        public static string CurrentFormattedLocalDateTime()
        {
            return DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz");
        }


    }
}
