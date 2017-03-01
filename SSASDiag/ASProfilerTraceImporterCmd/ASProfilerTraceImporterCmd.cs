using System;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Trace;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime;
namespace ASProfilerTraceImporterCmd
{
    class ASProfilerTraceImporterCmd
    {
        static string TraceFilePath = "";
        static string ConnStr = "";
        static string Table = "";
        static public string cols = "";
        static bool bCancel = false;
        static public SqlConnectionInfo2 cib = new SqlConnectionInfo2();
        static public List<TraceFileProcessor> tfps = new List<TraceFileProcessor>();
        static public int FileCount = 0;

        static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                TraceFilePath = args[0];
                ConnStr = args[1];
                cib.SetConnectionString(ConnStr);
                Table = args[2];

                EventWaitHandle waitForSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "ASProfilerTraceImporterCmdCancelSignal");
                Thread thMonitorCancelFlag = new Thread(new ThreadStart(() =>
                {
                    waitForSignal.WaitOne();
                    SetText("Import of profiler trace cancelled.");
                    bCancel = true;
                }));
                thMonitorCancelFlag.Start();
                PerformImport();
                thMonitorCancelFlag.Abort();
            }
            else
                return;
        }
       
        public static void SetText(string text)
        {
                Console.WriteLine(text);
        }

        public class SqlConnectionInfo2 : SqlConnectionInfo
        {
            public void SetConnectionString(string conn)
            {
                System.Security.SecureString s = new System.Security.SecureString();
                foreach (char c in conn) s.AppendChar(c);
                ConnectionStringInternal = s;
                foreach (string prop in conn.Split(';'))
                {
                    string[] kv = prop.Split('=');
                    if (kv[0].ToLower() == "database" || kv[0].ToLower() == "initial catalog") DatabaseName = kv[1];
                    if (kv[0].ToLower() == "server" || kv[0].ToLower() == "data source") ServerName = kv[1];
                    if (kv[0].ToLower() == "integrated security") UseIntegratedSecurity = Convert.ToBoolean(kv[1]);
                    if (kv[0].ToLower() == "user" || kv[0].ToLower() == "user id" || kv[0].ToLower() == "username") UserName = kv[1];
                    if (kv[0].ToLower() == "password") Password = kv[1];
                }
                RebuildConnectionStringInternal = false;
            }
        }

        static SqlCommand SqlCommandInfiniteConstructor(string c, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand(c, conn);
            cmd.CommandTimeout = 0;
            return cmd;
        }

        public delegate void TraceUpdateRowCountsDelegate();

        public static void UpdateRowCounts()
        {
            SetText("Loaded " + String.Format("{0:#,##0}", RowCount) + " rows from " + tfps.Count + " file" + (tfps.Count == 1 ? "" : "s in parallel") + "...");
        }

        public static int RowCount
        {
            get
            {
                int TotalRowCounts = 0;
                for (int i = 0; i < tfps.Count; i++) TotalRowCounts += tfps[i].RowCount;
                return TotalRowCounts;
            }
        }

        static int ExtractNumberPartFromText(string text)
        {
            Match match = Regex.Match(text, @"(\d+)");
            if (match == null) return 0;
            int value;
            if (!int.TryParse(match.Value, out value)) return 0;
            return value;
        }

        private static void PerformImport()
        {
            try
            {
                DateTime startTime = DateTime.Now;

                string path = TraceFilePath.Substring(0, TraceFilePath.LastIndexOf('\\') + 1);
                string trcbase = TraceFilePath.Substring(TraceFilePath.LastIndexOf('\\') + 1);
                // Remove numbers trailing from trace file name (if they are there) as well as file extension...
                trcbase = trcbase.Substring(0, trcbase.Length - 4); int j = trcbase.Length - 1; while (Char.IsDigit(trcbase[j])) j--; trcbase = trcbase.Substring(0, j + 1);
                FileInfo fi = new FileInfo(TraceFilePath);
                var fileInfos = Directory.GetFiles(path, trcbase + "*.trc", SearchOption.TopDirectoryOnly).Select(p => new FileInfo(p))
                    .OrderBy(x => x.CreationTime).Where(x => x.CreationTime >= fi.CreationTime.Subtract(new TimeSpan(0, 0, 45))).ToList();
                List<string> files = fileInfos.Select(x => x.Name.Substring(trcbase.Length)).ToList();
                files.Sort((x, y) => ExtractNumberPartFromText(x).CompareTo(ExtractNumberPartFromText(y)));
                if (TraceFilePath != path + trcbase + files[0])
                    files.RemoveRange(0, files.IndexOf(TraceFilePath.Substring((path + trcbase).Length)));
                FileCount = files.Count;

                List<Thread> workers = new List<Thread>();
                int CurFile = -1;
                cols = "";
                tfps = new List<TraceFileProcessor>();

                Semaphore s = new Semaphore(1, System.Environment.ProcessorCount * 2); // throttles simultaneous threads to number of processors, starts with just 1 free thread until cols are initialized
                foreach (string f in files)
                {
                    if (!bCancel)
                    {
                        CurFile++;
                        TraceFileProcessor tfp = new TraceFileProcessor(CurFile, path + trcbase + f, Table, s, new TraceFile(), new TraceTable());
                        tfps.Add(tfp);
                        workers.Add(new Thread(() => tfp.ProcessTraceFile()));
                        workers.Last().Start();
                    }
                }

                if (!bCancel)
                {
                    SqlConnection conn2 = new System.Data.SqlClient.SqlConnection(ConnStr);
                    conn2.Open();
                    for (int i = 0; i <= CurFile; i++)
                    {
                        workers[i].Join();
                        if (i == 1)
                            SetText("Merging file 1...");
                        if (i > 0 && !bCancel)
                        {
                            try
                            {
                                SqlCommandInfiniteConstructor("delete from [##" + Table + "_" + i + "] where eventclass = 65528", conn2).ExecuteNonQuery();
                                SqlCommandInfiniteConstructor("insert into [" + Table + "] (" + cols + ") select " + cols + " from [##" + Table + "_" + i + "]", conn2).ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Trace.WriteLine("Exception in trace import: \r\n" + ex.Message);
                            }
                            SetText("Merging file " + (i + 1) + "...");
                        }
                        tfps[i].tIn.Close();
                        tfps[i].tOut.Close();
                        tfps[i].tIn = null;
                        tfps[i].tOut = null;
                    }
                    if (!bCancel)
                    {
                        SetText("Building index and adding views...");
                        SetText("Done loading " + String.Format("{0:#,##0}", (RowCount)) + " rows in " + Math.Round((DateTime.Now - startTime).TotalSeconds, 1) + "s.");
                        cols = SqlCommandInfiniteConstructor("SELECT SUBSTRING((SELECT ', t.' + QUOTENAME(COLUMN_NAME) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + Table + "' AND COLUMN_NAME <> 'RowNumber' ORDER BY ORDINAL_POSITION FOR XML path('')), 3, 200000);", conn2).ExecuteScalar() as string;
                        try
                        {
                            SqlCommandInfiniteConstructor(Properties.Resources.EventClassSubClassTablesScript, conn2).ExecuteNonQuery();
                            SqlCommandInfiniteConstructor("if exists(select * from sys.views where name = '" + Table + "_v') drop view [" + Table + "_v];", conn2).ExecuteNonQuery();
                            SqlCommandInfiniteConstructor("create view [" + Table + "_v] as select t.[RowNumber], " + cols.Replace("t.[EventClass]", "c.[Name] as EventClassName, t.EventClass").Replace("t.[EventSubclass]", "s.[Name] as EventSubclassName, t.EventSubclass").Replace("t.[TextData]", "convert(nvarchar(max), t.[TextData]) TextData") + " from [" + Table + "]  t left outer join ProfilerEventClass c on c.EventClassID = t.EventClass left outer join ProfilerEventSubclass s on s.EventClassID = t.EventClass and s.EventSubclassID = t.EventSubclass;", conn2).ExecuteNonQuery();
                            SqlCommandInfiniteConstructor("if exists(select * from sys.views where name = '" + Table + "_QueriesAndCommandsIncludingIncomplete') drop view [" + Table + "_IncompleteQueriesAndCommands];", conn2).ExecuteNonQuery();
                            SqlCommandInfiniteConstructor("create view [" + Table + "_QueriesAndCommandsIncludingIncomplete] as select StartRow, EndRow, Duration, EventClass, case when EventClass in (-10, -16) then 0 else 1 end as RequestCompleted, ConnectionID, StartTime from (select min(RowNumber) StartRow, max(RowNumber) EndRow, sum(Duration) Duration, max(EventClass) EventClass, ConnectionID, StartTime from [" + Table + "] where EventClass in (9, 10, 15, 16) group by ConnectionID, StartTime having count(*) = 2 union select StartRow, EndRow, a.[Time Captured in Trace] Duration, -EventClass - (case when EventClass % 2 = 1 then 1 else -1 end) EventClass, ConnectionID, StartTime from(select case when max(EventClass) in (9, 15) then max(RowNumber) else null end StartRow, case when max(EventClass) in (10, 16) then max(RowNumber) else null end EndRow, datediff(\"ms\", max(StartTime),(select max(CurrentTime) from[" + Table + "])) [Time Captured in Trace] from [" + Table + "] a where EventClass in (9, 10, 15, 16) and not StartTime is null group by a.StartTime, ConnectionID having count(*) = 1) a, [" + Table + "] b where b.RowNumber = a.StartRow) a", conn2).ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            SetText("Exception creating event class/subclass view:\r\n" + ex.Message);
                            Trace.WriteLine("Can safely ignore since view creation will fail if necessary EvenClass and/or EventSubclass events are missing.");
                        }
                        SqlCommandInfiniteConstructor("if exists(select * from sys.views where name = '" + Table + "EventClassIndex') drop view [" + Table + "EventClassIndex];", conn2).ExecuteNonQuery();
                        SqlCommandInfiniteConstructor("create index [" + Table + "EventClassIndex] on [" + Table + "] (EventClass)", conn2).ExecuteNonQuery();
                        SqlCommandInfiniteConstructor("if exists(select * from sys.views where name = '" + Table + "ConnectionStarTimeIndex') drop view [" + Table + "ConnectionStarTimeIndex];", conn2).ExecuteNonQuery();
                        SqlCommandInfiniteConstructor("create nonclustered index [" + Table + "ConnectionStarTimeIndex] on [" + Table + "] (ConnectionID, StartTime)", conn2).ExecuteNonQuery();
                        string strQry = "";
                        try
                        {
                            SqlCommandInfiniteConstructor("if exists(select * from sys.views where name = '" + Table + "_QueryStats') drop view [" + Table + "_QueryStats];", conn2).ExecuteNonQuery();
                            int? QuerySubcubeEvents = SqlCommandInfiniteConstructor("select count(*) from [" + Table + "] where eventclass = 11", conn2).ExecuteScalar() as int?;
                            if (QuerySubcubeEvents > 0)
                            {
                                strQry = "create view [" + Table + "_QueryStats] as select e.QueryDuration, e.StorageEngineTime, e.QueryDuration - e.StorageEngineTime FormulaEngineTime, iif(e.QueryDuration > 0, cast(((1.0 * e.StorageEngineTime) / e.QueryDuration) * 100 as decimal(18,2)), 0) SEPct, s.RowNumber StartRow, e.EndRow, s.StartTime, e.EndTime, s.ConnectionID, s.DatabaseName, convert(nvarchar(max), s.Textdata) TextData, s.RequestParameters, s.RequestProperties, s.SPID, s.NTUserName, s.NTDomainName from [" + Table + "] s, (select max(duration) StorageEngineTime, startrow, endrow, d.EndTime, QueryDuration from (select c.Duration QueryDuration, c.StartRow, c.EndRow, d.RowNumber, d.EventClass, d.EventSubClass, d.TextData, d.ConnectionID, d.DatabaseName, d.StartTime, d.CurrentTime, c.EndTime, d.Duration from [" + Table + "] d, (select b.EndRow, b.ConnectionID, b.Duration, a.RowNumber as StartRow, b.EndTime from [" + Table + "] a, (select RowNumber EndRow, connectionid, textdata, starttime, currenttime endtime, duration from [" + Table + "] where eventclass = 10) b where a.eventclass = 9 and a.connectionid = b.connectionid and a.currenttime = b.starttime and convert(nvarchar(max), a.textdata) = convert(nvarchar(max), b.textdata)) c where d.ConnectionID = c.ConnectionID and d.RowNumber >= c.StartRow and d.RowNumber <= c.EndRow) d where eventclass = 11 group by startrow, endrow, connectionid, endtime, QueryDuration) e where s.rownumber = e.StartRow";
                                SqlCommandInfiniteConstructor(strQry, conn2).ExecuteNonQuery();
                            }
                            System.Diagnostics.Trace.WriteLine("QuerySubcube count: " + QuerySubcubeEvents);
                        }
                        catch (Exception ex)
                        {
                            SetText("Exception creating query stats view:\r\n" + ex.Message);
                            SetText("Query was: " + strQry);
                        }
                        SetText("Merged " + (CurFile + 1).ToString() + " files.");
                        SetText("Created view including incomplete commands and queries at [" + Table + "_QueriesAndCommandsIncludingIncomplete].\r\nCreated event names view [" + Table + "_v].");
                    }
                    conn2.Close();
                    if (!bCancel)
                        SetText("Database prepared for analysis.");
                }
                if (bCancel)
                    SetText("Cancelled loading after reading " + String.Format("{0:#,##0}", (RowCount + 1)) + " rows.");
            }
            catch (Exception exc)
            {
                if (!bCancel)
                    Console.WriteLine(exc.ToString(), "Oops!  Exception...");
            }
        }

        public class TraceFileProcessor
        {
            public int RowCount = 0;
            public string Table;
            public int CurFile = 0;
            public string FileName = "";
            public Semaphore Sem;
            public TraceFile tIn;
            public TraceTable tOut;

            public TraceFileProcessor(int curFile, string fileName, string table, Semaphore sem, TraceFile tin, TraceTable tout)
            { CurFile = curFile; FileName = fileName; Table = table; Sem = sem; tIn = tin; tOut = tout; }

            public void ProcessTraceFile()
            {
                try
                {
                    if (!bCancel)
                    {
                        Sem.WaitOne();  // throttle execution based on semaphore
                        tIn.InitializeAsReader(FileName);
                        tOut.InitializeAsWriter(tIn, ASProfilerTraceImporterCmd.cib, CurFile == 0 ? Table : "##" + Table + "_" + CurFile);
                        SqlConnection conn = new System.Data.SqlClient.SqlConnection(ASProfilerTraceImporterCmd.ConnStr);
                        conn.Open();
                        bool bFirstFile = false;
                        if (cols == "")
                        {
                            // get column list from first trace file...
                            cols = new SqlCommand("SELECT SUBSTRING((SELECT ', ' + QUOTENAME(COLUMN_NAME) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + Table + "' AND COLUMN_NAME <> 'RowNumber' ORDER BY ORDINAL_POSITION FOR XML path('')), 3, 200000);", conn).ExecuteScalar() as string;
                            bFirstFile = true;
                            Sem.Release(System.Environment.ProcessorCount * 2);  // We blocked everything until we got initial cols, now we release them all to run...
                        }
                        else
                            if (cols != new SqlCommand("SELECT SUBSTRING((SELECT ', ' + QUOTENAME(COLUMN_NAME) FROM tempdb.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '##" + Table + "_" + CurFile + "' AND COLUMN_NAME <> 'RowNumber' ORDER BY ORDINAL_POSITION FOR XML path('')), 3, 200000);", conn).ExecuteScalar() as string)
                            return;  // only happens if there is column mismatch between files - we won't read the whole file, just skip over...
                        conn.Close();
                        try
                        {
                            while (tOut.Write() && !bCancel)
                                if (RowCount++ % (384 * ((Environment.ProcessorCount > ASProfilerTraceImporterCmd.FileCount / 2) ? ASProfilerTraceImporterCmd.FileCount : Environment.ProcessorCount)) == 0) global::ASProfilerTraceImporterCmd.ASProfilerTraceImporterCmd.UpdateRowCounts();
                        }
                        catch (SqlTraceException ste)
                        {
                            string s = ste.Message;
                        }
                        if (!bFirstFile) Sem.Release(); // release this code for next thread waiting on the semaphore 
                    }
                }
                catch (Exception e)
                {
                    if (!bCancel)
                        SetText(e.ToString() + "\r\n\r\nException occurred while loading file " + FileName + ".");
                }
            }
        }
        

    }
}
