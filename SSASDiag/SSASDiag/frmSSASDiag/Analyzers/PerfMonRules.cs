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

            RuleCounter AvailableMB = new RuleCounter("Memory\\Available MBytes", true, false, Color.Blue);
            RuleCounter WorkingSet = new RuleCounter("Process\\Working Set\\_Total", false);
            Rule r = new Rule();
            r.Category = "Memory";
            r.Name = "Server Available Memory";
            r.Description = "Checks to ensure sufficient free memory.";
            r.Counters.Add(AvailableMB);
            r.Counters.Add(WorkingSet);
            r.RuleFunction = new Action(() =>
            {
                r.RuleResult = RuleResultEnum.Pass;
                // Create an expression
                double totalMem = ((double)AvailableMB.ChartSeries.Points[0].Tag) + (((double)WorkingSet.ChartSeries.Points[0].Tag) / 1024.0 / 1024.0);
                // Add custom series
                Series TotalMemory = r.AddCustomSeriesAtY("Total Physical Memory MB", totalMem, Color.Black);
                Series MaximumUtilization = r.AddCustomSeriesAtY("97% Memory Utilization", totalMem * .97, Color.Red);
                Series WarnUtilization = r.AddCustomSeriesAtY("95% Memory Utilization", totalMem * .95, Color.Yellow);
                // Perform validation with a standard rule function and update rule result descriptions
                r.ValidateThresholdRule(AvailableMB.ChartSeries, totalMem * .95, totalMem * .97);
                if (r.RuleResult == RuleResultEnum.Fail) r.ResultDescription = "Memory usage rose above 97%.";
                if (r.RuleResult == RuleResultEnum.Warn) r.ResultDescription = "Memory usage rose above 95%.";
                if (r.RuleResult == RuleResultEnum.Pass) r.ResultDescription = "Sufficient memory available at all times.";
            });
            Rules.Add(r);
        }

        private enum RuleResultEnum
        {
            NotRun, Fail, Warn, Pass, CountersUnavailable, Other
        }

        private class RuleCounter
        {
            public string Path;
            public bool ShowInChart;
            public bool HighlightInChart = true;
            public Series ChartSeries = null;
            public Color? CounterColor = null;
            public RuleCounter(string Path, bool ShowInChart = true, bool HighlightInChart = false, Color? CounterColor = null)
            {
                this.Path = Path;
                this.ShowInChart = ShowInChart;
                this.HighlightInChart = HighlightInChart;
                this.CounterColor = CounterColor;
            }
        }

        private class Rule : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string Category { get; set; } = "";
            public string ResultDescription { get; set; } = "";

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

            public List<RuleCounter> Counters = new List<RuleCounter>();
            public List<Series> CustomSeries = new List<Series>();

            private RuleResultEnum ruleResult = RuleResultEnum.NotRun;
            [Browsable(false)]
            public RuleResultEnum RuleResult { get { return ruleResult; } set { OnPropertyChanged("RuleResultImg"); ruleResult = value; } }

            private Action ruleFunction = null;
            [Browsable(false)]
            public Action RuleFunction { get { return ruleFunction; } set { ruleFunction = value; } }

            public Series AddCustomSeriesAtY(string name, double y, Color color)
            {
                Series s = new Series(name);
                s.ChartType = SeriesChartType.Line;
                s.XValueType = ChartValueType.DateTime;
                s.EmptyPointStyle.BorderWidth = 0;
                s.BorderColor = s.Color = color;

                DataPointCollection p1 = Counters[0].ChartSeries.Points;
                for (int i = 0; i < p1.Count; i++)
                    s.Points.Add(new DataPoint(p1[i].XValue, y));

                CustomSeries.Add(s);
                return s;
            }
            public void ValidateThresholdRule(Series s, double WarnY, double ErrorY, Color? WarnColor = null, Color? ErrorColor = null, bool Below = true)
            {
                if (!WarnColor.HasValue)
                    WarnColor = Color.Yellow;
                if (!ErrorColor.HasValue)
                    ErrorColor = Color.Red;

                foreach (DataPoint p in s.Points)
                {
                    double val = 0;
                    if (p.Tag == null)
                        val = p.YValues[0];
                    else
                        val = (double)p.Tag;

                    if (Below)
                    {
                        if (val >= ErrorY)
                        {
                            p.Color = ErrorColor.Value;
                            RuleResult = RuleResultEnum.Fail;
                        }
                        else if (val >= WarnY)
                        {
                            p.Color = WarnColor.Value;
                            if (RuleResult != RuleResultEnum.Fail)
                                RuleResult = RuleResultEnum.Warn;
                        }
                    }
                    else
                    {
                        if (val <= ErrorY)
                        {
                            p.Color = ErrorColor.Value;
                            RuleResult = RuleResultEnum.Fail;
                        }
                        else if (val <= WarnY)
                        {
                            p.Color = WarnColor.Value;
                            if (RuleResult != RuleResultEnum.Fail)
                                RuleResult = RuleResultEnum.Warn;
                        }
                    }
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
