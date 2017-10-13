using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;


namespace SSASMagicNumberLookupService
{
    class Program
    {
        static ManualResetEvent DebuggerResultReady = new ManualResetEvent(false);
        static string LastResponse = "";
        static string path = @"w:\magicnumbers.data";
        static Timer tmr = new Timer(new TimerCallback(PollForAwaitingRequests), null, 0, 1000);

        static void Main(string[] args)
        {
            Console.WriteLine("SSASMagicNumberLookupService started at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ".");
            Console.WriteLine("Press q to quit monitoring for new magic number lookup requests...");
            while (Console.ReadKey().KeyChar != 'q')
                ;
            Console.WriteLine("\r\nShutting down SSASMagicNumberLookupService at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ".");
            tmr.Change(Timeout.Infinite, 0);
            bClosing = true;
            if (InExtraction)
                Console.Write("Extraction of magic numbers in progress.  Waiting for completion to shutdown...");
            while (InExtraction)
            {
                Thread.Sleep(1000);
                Console.Write(".");
            }
            Console.WriteLine("\r\nShutdown complete.");
        }
        static bool bClosing = false;

        static private void PollForAwaitingRequests(object o)
        {
            tmr.Change(Timeout.Infinite, 0);

            if (File.Exists(path))
            {
                List<string> syms = null;
                while (syms == null)
                {
                    try
                    {
                        syms = File.ReadAllLines(path).ToList();
                    }
                    catch { }
                }
                bool bNewNums = false;
                for (int i = 0; i < syms.Count(); i++)
                {
                    if (syms[i].Trim().EndsWith("=") && !bClosing)
                    {
                        InExtraction = true;
                        bNewNums = true;
                        string sym = syms[i];
                        Console.WriteLine("\r\n" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " -  Performing magic number lookup on binary with time/size stamp: " + sym.TrimEnd('=') + "...");
                        string magicnums = ExtractMagicNumbers(sym.TrimEnd('='));
                        Console.WriteLine("\r\n" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " - Magic numbers discovered: " + magicnums);
                        syms[syms.IndexOf(sym.Trim())] = sym.Trim() + magicnums;
                        InExtraction = false;
                    }
                }
                if (bNewNums)
                {
                    bool bWritten = false;
                    while (!bWritten)
                    {
                        try
                        {
                            File.WriteAllLines(path, syms);
                            bWritten = true;
                        }
                        catch { }
                    }
                }
            }
            tmr.Change(0, 1000);
        }

        static bool InExtraction = false;
        static private string ExtractMagicNumbers(string sym)
        {
            string localcache = "C:\\MagicNumberLookupSymbolCache\\";

            HttpWebRequest wr = WebRequest.CreateHttp("http://symweb/msmdsrv.exe/" + sym + "/file.ptr");
            string sympath = new StreamReader(wr.GetResponse().GetResponseStream()).ReadToEnd().Replace("PATH:", "");
            string localsymbol = localcache + sym + sympath.Substring(sympath.LastIndexOf('\\'));
            if (!Directory.Exists(localcache + sym))
            {
                Directory.CreateDirectory(localcache + sym);
                File.Copy(sympath, localsymbol);
            }
                

            Process p = new Process();
            p.OutputDataReceived += P_OutputDataReceived;
            p.ErrorDataReceived += P_ErrorDataReceived;
            p.Exited += P_Exited;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.EnableRaisingEvents = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c \"\"" + Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\CDB\\cdb.exe\" -z \"" + localsymbol + "\"\"";
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.StandardInput.WriteLine(".echo \"EndOfData\""); // Ensures we can detect end of output, when this is processed and input prompt is displayed in console output...

            DebuggerResultReady.Reset();
            DebuggerResultReady.WaitOne();
            DebuggerResultReady.Reset();
            string res = SubmitDebuggerCommand(".reload /i \"" + localcache + sym + "\"", p);

            res = SubmitDebuggerCommand("x msmdsrv!PXSession::InternalExecuteCommand", p).Trim();
            string PXSessionExecuteCommandAddress = Int32.Parse(res.Substring(0, res.IndexOf(" ")).Split('`')[1].TrimStart('0'), System.Globalization.NumberStyles.HexNumber).ToString();
            string[] lines = SubmitDebuggerCommand("uf msmdsrv!PXSession::InternalExecuteCommand", p).Split(new char[] { '\r', '\n' });
            res = lines[lines.Length - 3];
            string PXSessionExecuteCommandEndAddress = Int32.Parse(res.Substring(0, res.IndexOf(' ')).Split('`')[1].TrimStart('0'), System.Globalization.NumberStyles.HexNumber).ToString();

            res = SubmitDebuggerCommand("dt msmdsrv!PXSession", p);
            string LastRequestOffset = res.Split(new char[] { '\r', '\n' }).Where(s => s.Contains("m_strLastRequest")).First().Trim();
            LastRequestOffset = Int32.Parse(LastRequestOffset.Substring(0, LastRequestOffset.IndexOf(" ")).Replace("+0x", ""), System.Globalization.NumberStyles.HexNumber).ToString();
            string User = res.Split(new char[] { '\r', '\n' }).Where(s => s.Contains("m_strUserName")).First().Trim();
            User = Int32.Parse(User.Substring(0, User.IndexOf(" ")).Replace("+0x", ""), System.Globalization.NumberStyles.HexNumber).ToString();
            string Roles = res.Split(new char[] { '\r', '\n' }).Where(s => s.Contains("m_strActiveRoles")).First().Trim();
            Roles = Int32.Parse(Roles.Substring(0, Roles.IndexOf(" ")).Replace("+0x", ""), System.Globalization.NumberStyles.HexNumber).ToString();
            string StartTime = res.Split(new char[] { '\r', '\n' }).Where(s => s.Contains("m_timeLastRequestStart")).First().Trim();
            StartTime = Int32.Parse(StartTime.Substring(0, StartTime.IndexOf(" ")).Replace("+0x", ""), System.Globalization.NumberStyles.HexNumber).ToString();
            string Database = res.Split(new char[] { '\r', '\n' }).Where(s => s.Contains("m_strSPDatabaseID")).First().Trim();
            Database = Int32.Parse(Database.Substring(0, Database.IndexOf(" ")).Replace("+0x", ""), System.Globalization.NumberStyles.HexNumber).ToString();

            res = SubmitDebuggerCommand("x msmdsrv!PFThreadPool::ExecuteJob", p).Trim();
            string PFThreadPoolExecuteJobAddress = Int32.Parse(res.Substring(0, res.IndexOf(" ")).Split('`')[1].TrimStart('0'), System.Globalization.NumberStyles.HexNumber).ToString();
            lines = SubmitDebuggerCommand("uf msmdsrv!PFThreadPool::ExecuteJob", p).Split(new char[] { '\r', '\n' });
            res = lines[lines.Length - 3];
            string PFThreadPoolExecuteJobEndAddress = Int32.Parse(res.Substring(0, res.IndexOf(' ')).Split('`')[1].TrimStart('0'), System.Globalization.NumberStyles.HexNumber).ToString();

            res = SubmitDebuggerCommand("dt msmdsrv!PFThreadContext", p);
            string ParentContextOffset = res.Split(new char[] { '\r', '\n' }).Where(s => s.Contains("m_pParentEC")).First().Trim();
            ParentContextOffset = Int32.Parse(ParentContextOffset.Substring(0, ParentContextOffset.IndexOf(" ")).Replace("+0x", ""), System.Globalization.NumberStyles.HexNumber).ToString();
            res = SubmitDebuggerCommand("dt msmdsrv!PCExecutionContext", p);
            string SessionOffset = res.Split(new char[] { '\r', '\n' }).Where(s => s.Contains("m_spSession")).First().Trim();
            SessionOffset = Int32.Parse(SessionOffset.Substring(0, SessionOffset.IndexOf(" ")).Replace("+0x", ""), System.Globalization.NumberStyles.HexNumber).ToString();

            string output = PXSessionExecuteCommandAddress + "," +
                            PXSessionExecuteCommandEndAddress + "," +
                            LastRequestOffset + "," +
                            User + "," +
                            Roles + "," +
                            StartTime + "," +
                            Database + "," +
                            PFThreadPoolExecuteJobAddress + "," +
                            PFThreadPoolExecuteJobEndAddress + "," +
                            ParentContextOffset + "," +
                            SessionOffset;
            return output;
        }
        
        static private string SubmitDebuggerCommand(string cmd, Process p)
        {
            LastResponse = "";
            if (p.StartInfo.RedirectStandardInput)
            {
                p.StandardInput.WriteLine(cmd);
                if (cmd != "q")
                {
                    p.StandardInput.WriteLine(".echo \"EndOfData\"");
                    DebuggerResultReady.Reset();
                    DebuggerResultReady.WaitOne();
                }
            }
            return LastResponse;
        }

        static private void P_Exited(object sender, EventArgs e)
        {

        }

        static private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {

        }

        static string CurrentPrompt = "";
        static private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.EndsWith("EndOfData"))  // this signals the input prompt has been shown
                {
                    CurrentPrompt = e.Data.Replace("EndOfData", "");
                    LastResponse += CurrentPrompt.Substring(CurrentPrompt.IndexOf("> ") + "> ".Length);
                    CurrentPrompt = CurrentPrompt.Substring(0, CurrentPrompt.IndexOf("> ") + 1);
                    DebuggerResultReady.Set();
                }
                else
                {
                    // trim the current prompt from the start of data if both are not zero length strings
                    string output = e.Data.Length > 0 && CurrentPrompt.Length > 0 ? e.Data.Replace(CurrentPrompt, "") : e.Data;
                    LastResponse += output + "\r\n";
                }
            }
        }

    }


}
