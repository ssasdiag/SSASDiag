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

            Rule r0 = new Rule("Server Available Memory", "Memory", "Checks to ensure sufficient free memory.");
            RuleCounter AvailableMB = new RuleCounter("Memory\\Available MBytes", true, false, Color.Blue);
            RuleCounter WorkingSet = new RuleCounter("Process\\Working Set\\_Total", false);
            r0.Counters.Add(AvailableMB);
            r0.Counters.Add(WorkingSet);
            r0.RuleFunction = new Action(() =>
            {
                r0.RuleResult = RuleResultEnum.Pass;
                double totalMem = ((double)AvailableMB.ChartSeries.Points[0].Tag) + (((double)WorkingSet.ChartSeries.Points[0].Tag) / 1024.0 / 1024.0);
                r0.AddStripLine("Total Physical Memory MB", totalMem, totalMem, Color.Black);
                r0.ValidateThresholdRule(AvailableMB.ChartSeries, totalMem * .05, totalMem * .03, "5% available memory", "3% available memory", null, null, true);
                if (r0.RuleResult == RuleResultEnum.Fail) r0.ResultDescription = "Fail: Less than 3% free memory.";
                if (r0.RuleResult == RuleResultEnum.Warn) r0.ResultDescription = "Warning: Less than 5% free memory.";
                if (r0.RuleResult == RuleResultEnum.Pass) r0.ResultDescription = "Pass: Sufficient memory available at all times.";
            });
            Rules.Add(r0);

            Rule r1 = new Rule("Server Available Memory2", "Memory", "Checks to ensure LOTS of sufficient free memory.");
            RuleCounter AvailableMB2 = new RuleCounter("Memory\\Available MBytes", true, false, Color.Blue);
            RuleCounter WorkingSet2 = new RuleCounter("Process\\Working Set\\_Total", false);
            r1.Counters.Add(AvailableMB2);
            r1.Counters.Add(WorkingSet2);
            r1.RuleFunction = new Action(() =>
            {
                r1.RuleResult = RuleResultEnum.Pass;
                double totalMem = ((double)AvailableMB2.ChartSeries.Points[0].Tag) + (((double)WorkingSet2.ChartSeries.Points[0].Tag) / 1024.0 / 1024.0);
                r1.AddStripLine("Total Physical Memory MB", totalMem, totalMem, Color.Black);
                r1.ValidateThresholdRule(AvailableMB2.ChartSeries, totalMem * .89, totalMem * .80, "89% of Total Memory", "80% of Total Memory", null, null, true);
                if (r1.RuleResult == RuleResultEnum.Fail) r1.ResultDescription = "Fail: Less than 80% free memory.";
                if (r1.RuleResult == RuleResultEnum.Warn) r1.ResultDescription = "Warning: Less than 89% free memory.";
                if (r1.RuleResult == RuleResultEnum.Pass) r1.ResultDescription = "Pass: Sufficient memory available at all times.";
            });
            Rules.Add(r1);

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
            public Rule(string Name, string Category, string Description)
            {
                this.Name = Name;
                this.Category = Category;
                this.Description = Description;
            }

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
            public List<StripLine> CustomStripLines = new List<StripLine>();

            private RuleResultEnum ruleResult = RuleResultEnum.NotRun;
            [Browsable(false)]
            public RuleResultEnum RuleResult { get { return ruleResult; } set { OnPropertyChanged("RuleResultImg"); ruleResult = value; } }

            private Action ruleFunction = null;
            [Browsable(false)]
            public Action RuleFunction { get { return ruleFunction; } set { ruleFunction = value; } }

            public double MaxValueForRule()
            {
                double max = double.MinValue;
                foreach (RuleCounter rc in Counters)
                    if (rc.ChartSeries.Tag != null)
                    {
                        if ((double)rc.ChartSeries.Tag > max)
                            max = (double)rc.ChartSeries.Tag;
                    }
                    else
                    {
                        if (rc.ChartSeries.Points.FindMaxByValue().YValues[0] > max)
                            max = rc.ChartSeries.Points.FindMaxByValue().YValues[0];
                    }
                foreach (Series s in CustomSeries)
                    if (s.Tag != null)
                    {
                        if ((double)s.Tag > max)
                            max = (double)s.Tag;
                    }
                    else
                    {
                        if (s.Points.FindMaxByValue().YValues[0] > max)
                            max = s.Points.FindMaxByValue().YValues[0];
                    }
                foreach (StripLine s in CustomStripLines)
                    if (s.IntervalOffset > max)
                        max = s.IntervalOffset;
                return max;
            }

            public StripLine AddStripLine(string name, double y, double y2, Color color)
            {
                StripLine s = new StripLine();
                s.Interval = 0;
                s.Text = name;
                if (y != y2)
                    s.BackColor = Color.FromArgb(128, color);
                else
                    s.BackColor = color;
                s.BorderColor = color;
                s.ForeColor = Color.Transparent;
                s.IntervalOffset = y > y2 ? y2 : y;
                s.StripWidth = Math.Abs(y2 - y);
                s.BorderWidth = 1;

                CustomStripLines.Add(s);
                return s;
            }
            public void ValidateThresholdRule(Series s, double WarnY, double ErrorY, string WarningLineText = "", string ErrorLineText= "", Color? WarnColor = null, Color? ErrorColor = null, bool Below = true)
            {
                if (!WarnColor.HasValue)
                    WarnColor = Color.Yellow;
                if (!ErrorColor.HasValue)
                    ErrorColor = Color.DarkRed;

                StripLine WarnRegion = null;
                StripLine ErrorRegion = null;
                if (Below)
                {
                    if (WarningLineText != "")
                        WarnRegion = AddStripLine(WarningLineText, WarnY, ErrorY, WarnColor.Value);
                    if (ErrorLineText != "")
                        ErrorRegion = AddStripLine(ErrorLineText, 0, ErrorY, ErrorColor.Value);
                }
                else
                {
                    if (WarningLineText != "")
                        WarnRegion = AddStripLine(WarningLineText, 0, WarnY, WarnColor.Value);
                    if (ErrorLineText != "")
                        ErrorRegion = AddStripLine(ErrorLineText, WarnY > 0 ? WarnY : 0, ErrorY, ErrorColor.Value);
                }
                foreach (DataPoint p in s.Points)
                {
                    double val = 0;
                    if (p.Tag == null)
                        val = p.YValues[0];
                    else
                        val = (double)p.Tag;

                    if (Below)
                    {
                        if (val <= ErrorY)
                            RuleResult = RuleResultEnum.Fail;
                        else if (val <= WarnY)
                            if (RuleResult != RuleResultEnum.Fail)
                                RuleResult = RuleResultEnum.Warn;
                    }
                    else
                    {
                        if (val >= ErrorY)
                            RuleResult = RuleResultEnum.Fail;
                        else if (val >= WarnY)
                            if (RuleResult != RuleResultEnum.Fail)
                                RuleResult = RuleResultEnum.Warn;
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
