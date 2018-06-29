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

            RuleCounter AvailableBytes = new RuleCounter("Memory\\Available Bytes");
            RuleCounter WorkingSet = new RuleCounter("Process\\Working Set\\_Total", false);
            Rule r = new Rule();
            r.Category = "Memory";
            r.Name = "Server Available Memory";
            r.Description = "Checks to ensure there is sufficient free memory.";
            r.Counters.Add(AvailableBytes);
            r.Counters.Add(WorkingSet);
            r.RuleFunction = new Func<Rule, RuleResultEnum>((rr) =>
            {
                r.RuleResult = RuleResultEnum.Other;
                return RuleResultEnum.Other;
            });
            r.AnnotationFunction = new Action<Rule>((rr) =>
            {
                Series TotalMemory = new Series("Total System Memory");
                TotalMemory.ChartType = SeriesChartType.Line;
                TotalMemory.XValueType = ChartValueType.DateTime;
                TotalMemory.EmptyPointStyle.BorderWidth = 0;
                TotalMemory.BorderColor = TotalMemory.Color = Color.DarkRed;
                DataPointCollection p1 = AvailableBytes.ChartSeries.Points, p2 = WorkingSet.ChartSeries.Points;
                double totalMem = ((double)p1[0].Tag) + ((double)p2[0].Tag);
                for (int i = 0; i < p1.Count; i++)
                    TotalMemory.Points.Add(new DataPoint(p1[i].XValue, totalMem));
                AddCustomSeries(TotalMemory);
            });
            Rules.Add(r);
        }

        private class RuleCounter
        {
            public string Path;
            public bool ShowInChart;
            public bool HighlightInChart = true;
            public Series ChartSeries = null;

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
