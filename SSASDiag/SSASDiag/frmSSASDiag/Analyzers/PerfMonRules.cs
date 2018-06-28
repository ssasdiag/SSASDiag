using Microsoft.Win32;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Management;
using System.ServiceProcess;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Data.SqlClient;
using System.IO;
using SimpleMDXParser;
using FastColoredTextBoxNS;
using System.Windows.Forms.DataVisualization.Charting;
using Ionic.Zip;

namespace SSASDiag
{
    public partial class ucASPerfMonAnalyzer : UserControl
    {
        void DefineRules()
        {
            Rules.Clear();

            // Rule 0
            Rule r = new Rule();
            r.Category = "Memory";
            r.Name = "Server Available Memory";
            r.Description = "Checks to ensure there is sufficient free memory.";
            r.Counters.Add("Memory\\Available Bytes");
            r.Counters.Add("Process\\Working Set\\_Total");
            r.RuleFunction = new Func<Rule, RuleResultEnum>((rr) =>
            {
                r.RuleResult = RuleResultEnum.Other;
                return RuleResultEnum.Other;
            });
            r.AnnotationFunction = new Action<Rule>((rr) =>
            {
                new Thread(new ThreadStart(() =>
                {
                    tvCounters.SelectedNodes.Clear();

                    chartPerfMon.Invoke(new Action(()=>chartPerfMon.Series.Clear()));
                    tvCounters.AfterCheck -= TvCounters_AfterCheck;
                    TreeNode n = null;
                    foreach (string c in r.Counters)
                    {
                        n = tvCounters.FindNodeByPath(c);
                        if (n == null)
                        {
                            string[] pathParts = c.Split('\\');
                            string sAlternatePath = pathParts[0] + "\\";
                            for (int i = 1; i < pathParts.Length - 1; i++)
                                sAlternatePath += (pathParts[i + 1] + "\\");
                            sAlternatePath += (pathParts[1]);
                            n = tvCounters.FindNodeByPath(sAlternatePath);
                        }
                        if (n == null)
                            return;
                        tvCounters.Invoke(new Action(() =>
                        {
                            n.Checked = true;
                            while (n.Parent != null)
                            {
                                n = n.Parent;
                                n.Expand();
                            }
                        }));
                    }
                    tvCounters.AfterCheck += TvCounters_AfterCheck;
                    AddCounters();
                })).Start();
            });
            Rules.Add(r);
        }
    }
}
