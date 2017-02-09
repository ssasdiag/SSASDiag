using Microsoft.SqlServer.Management.Trace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// TraceFile binaries for Microsoft.SqlServer.Management.Trace are only available in x86.
// The SSASDiag process needs to be X64 to optimally work with large files.
// Workaround here, costs a few extra seconds to invoke at stop time but worth it
// Call this simple X86 process from SSASDiag.  

namespace ExtractDbNamesFromTrace
{
    static class ExtractDbNamesFromTrace
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Environment.GetCommandLineArgs().Length > 2)
            {
                TraceFile dbs = new TraceFile();
                dbs.InitializeAsReader(Environment.GetCommandLineArgs()[1]);
                List<string> r = new List<string>();
                while (dbs.Read())
                    if (dbs["DatabaseName"] != null && (dbs["DatabaseName"] as string).Trim() != "")
                        r.Add(dbs["DatabaseName"] as string);
                dbs.Close();
                if (r.Count > 0)
                    File.WriteAllLines(Environment.GetCommandLineArgs()[2], r.Distinct());
            }
        }
    }
}
