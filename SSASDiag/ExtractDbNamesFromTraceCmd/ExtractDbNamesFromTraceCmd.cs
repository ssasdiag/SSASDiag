using Microsoft.SqlServer.Management.Trace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExtractDbNamesFromTraceCmd
{
    class ExtractDbNamesFromTraceCmd
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                TraceFile dbs = new TraceFile();
                dbs.InitializeAsReader(args[0]);
                List<string> r = new List<string>();
                while (dbs.Read())
                    if (dbs["DatabaseName"] != null && (dbs["DatabaseName"] as string).Trim() != "")
                        r.Add(dbs["DatabaseName"] as string);
                dbs.Close();
                if (r.Count > 0)
                    Console.WriteLine(String.Join("\r\n", r.Distinct()).TrimEnd(new char[] { '\r', '\n'}));
                    //File.WriteAllLines(args[1], r.Distinct());
            }
            Console.WriteLine("ExtractDbNamesFromTraceCmd finished.");
        }
    }
}
