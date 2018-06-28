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
            r.Counters.Add(new RuleCounter("Memory\\Available Bytes"));
            r.Counters.Add(new RuleCounter("Process\\Working Set\\_Total"));
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
                    foreach (RuleCounter c in r.Counters)
                    {
                        n = tvCounters.FindNodeByPath(c.Path);
                        if (n == null)
                        {
                            string[] pathParts = c.Path.Split('\\');
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

        private class RuleCounter
        {
            public string Path;
            public bool ShowInChart = true;
            public bool HighlightInChart = true;
            public DataPointCollection Points
            {
                get { return null; }
            }

            public RuleCounter(string Path, bool ShowInChart = true, bool HighlightInChart = false)
            {
                this.Path = Path;
                this.ShowInChart = ShowInChart;
                this.HighlightInChart = HighlightInChart;
            }
        }

        private class Rule : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string Category { get; set; } = "";
            public string ResultDescription { get; set; } = "";
            private RuleResultEnum ruleResult = RuleResultEnum.NotRun;
            [Browsable(false)]
            public RuleResultEnum RuleResult { get { return ruleResult; } set { OnPropertyChanged("RuleResultImg"); ruleResult = value; } }
            public List<RuleCounter> Counters { get; set; } = new List<RuleCounter>();
            public List<object> Annotations { get; set; } = new List<object>(); // List of rule annotation variables, which RuleFunction should set and AnnotationFunction should use to draw its annotations however it needs to.
            public Action<Rule> AnnotationFunction = null;
            private Func<Rule, RuleResultEnum> ruleFunction = null;
            [Browsable(false)]
            public Func<Rule, RuleResultEnum> RuleFunction { get { return ruleFunction; } set { ruleFunction = value; } }
            Image ruleResultImg = null;
            public Image RuleResultImg
            {
                get
                {                   
                    switch (RuleResult)
                    {
                        case RuleResultEnum.CountersUnavailable:
                            return Properties.Resources.RuleCountersUnavailable;
                        case RuleResultEnum.Fail:
                            return Properties.Resources.RuleFail;
                        case RuleResultEnum.NotRun:
                            return Properties.Resources.RuleNotRun;
                        case RuleResultEnum.Other:
                            return Properties.Resources.RuleOther;
                        case RuleResultEnum.Pass:
                            return Properties.Resources.RulePass;
                        case RuleResultEnum.Warn:
                            return Properties.Resources.RuleWarn;
                    }
                    return null;
                }
            }

            protected void OnPropertyChanged(string name)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }
    }
}
