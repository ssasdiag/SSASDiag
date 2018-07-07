using System;
using System.Collections.Generic;

using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SSASDiag
{
    public partial class ucASPerfMonAnalyzer : UserControl
    {
        void DefineRules()
        {
            Rules.Clear();

            // Rule 0
            Rule r0 = new Rule("Server Available Memory", "Memory", "Checks to ensure sufficient free memory.");
            RuleCounter AvailableMB = RuleCounter.CountersFromPath("Memory\\Available MBytes", true, false, false, Color.Blue).First();
            RuleCounter WorkingSet = RuleCounter.CountersFromPath("Process\\Working Set\\_Total", false).First();
            if (AvailableMB == null || WorkingSet == null)
                r0.RuleResult = RuleResultEnum.CountersUnavailable;
            else
            {
                r0.Counters.Add(AvailableMB);
                r0.Counters.Add(WorkingSet);
                r0.RuleFunction = new Action(() =>
                {
                    r0.RuleResult = RuleResultEnum.Pass;
                    double totalMem = ((double)AvailableMB.ChartSeries.Points[0].YValues[0]) + (((double)WorkingSet.ChartSeries.Points[0].YValues[0]) / 1024.0 / 1024.0);
                    r0.AddStripLine("Total Physical Memory MB", totalMem, totalMem, Color.Black);
                    r0.ValidateThresholdRule(AvailableMB.ChartSeries, totalMem * .05, totalMem * .03, "5% available memory", "3% available memory", "> 5% available memory", false);
                    if (r0.RuleResult == RuleResultEnum.Fail) r0.ResultDescription = "Fail: Less than 3% free memory.";
                    if (r0.RuleResult == RuleResultEnum.Warn) r0.ResultDescription = "Warning: Less than 5% free memory.";
                    if (r0.RuleResult == RuleResultEnum.Pass) r0.ResultDescription = "Pass: Sufficient memory available at all times.";
                });
            }
            Rules.Add(r0);

            Rule r1 = new Rule("Server Available Memory2", "Memory", "Checks to ensure **LOTS** of free memory.");
            RuleCounter AvailableMB2 = RuleCounter.CountersFromPath("Memory\\Available MBytes", true, false, false, Color.Blue).First();
            RuleCounter WorkingSet2 = RuleCounter.CountersFromPath("Process\\Working Set\\_Total", false).First();
            r1.Counters.Add(AvailableMB2);
            r1.Counters.Add(WorkingSet2);
            if (AvailableMB2 == null || WorkingSet2 == null)
                r1.RuleResult = RuleResultEnum.CountersUnavailable;
            else
            {
                r1.RuleFunction = new Action(() =>
                {
                    r1.RuleResult = RuleResultEnum.Pass;
                    double totalMem = ((double)AvailableMB2.ChartSeries.Points[0].YValues[0]) + (((double)WorkingSet2.ChartSeries.Points[0].YValues[0]) / 1024.0 / 1024.0);
                    r1.AddStripLine("Total Physical Memory MB", totalMem, totalMem + 1, Color.Black);
                    r1.ValidateThresholdRule(AvailableMB2.ChartSeries, totalMem * .89, totalMem * .80, "89% available memory", "80% available memory", "> 89% available memory", false);
                    if (r1.RuleResult == RuleResultEnum.Fail) r1.ResultDescription = "Fail: Less than 80% free memory.";
                    if (r1.RuleResult == RuleResultEnum.Warn) r1.ResultDescription = "Warning: Less than 89% free memory.";
                    if (r1.RuleResult == RuleResultEnum.Pass) r1.ResultDescription = "Pass: Sufficient memory available at all times.";
                });
            }
            Rules.Add(r1);

            Rule r2 = new Rule("Disk Read Time", "IO", "Checks to ensure healthy disk read speed.");
            List<RuleCounter> DiskSecsPerRead = RuleCounter.CountersFromPath("PhysicalDisk\\Avg. Disk sec/Read\\-*", true, false);
            if (DiskSecsPerRead.Count == 0)
                r2.RuleResult = RuleResultEnum.CountersUnavailable;
            else
            {
                List<Series> ruleCheckSeries = new List<Series>();
                r2.Counters.AddRange(DiskSecsPerRead);
                r2.RuleFunction = new Action(() =>
                {
                    r2.RuleResult = RuleResultEnum.Pass;
                    foreach (RuleCounter rc in DiskSecsPerRead)
                        ruleCheckSeries.Add(rc.ChartSeries);
                    r2.ValidateThresholdRule(ruleCheckSeries, .010, .015, ">10ms Disk secs/Read time", ">15ms Disk secs/Read time", "<10ms Disk secs/Read time", true, true, false, false, 15);
                    if (r2.RuleResult == RuleResultEnum.Fail) r2.ResultDescription = "Fail: More than 15% of disk reads taking longer than 15ms.";
                    if (r2.RuleResult == RuleResultEnum.Warn) r2.ResultDescription = "Warning: More than 15% of disk reads taking longer than 10ms.";
                    if (r2.RuleResult == RuleResultEnum.Pass) r2.ResultDescription = "Pass: Disk read speed healthy at all times.";
                });
            }
            Rules.Add(r2);

            Rule r3 = new Rule("Missing Counters Example", "Test", "Demonstrates UI behavior if counters are missing for a rule.");
            if (RuleCounter.CountersFromPath("BadCounterName").Count == 0)
                r3.RuleResult = RuleResultEnum.CountersUnavailable; // Path not found will return an empty list.  Rule is responsible to determine if essential counters are missing, and set result.
            else
                r3.RuleResult = RuleResultEnum.Fail; // if BadCounterName was a path found in the counters, something must have failed somewhere!
            // No rule function even necessary if we already set the result before the function is needed.  We will never run it when counters are unavailable.
            Rules.Add(r3);
        }

        public enum RuleResultEnum
        {
            NotRun, Fail, Warn, Pass, CountersUnavailable, Other
        }

        public class RuleCounter
        {
            public string Path;
            public string WildcardPath;
            public bool ShowInChart;
            public bool HighlightInChart = true;
            public bool Include_TotalSeriesInWildcard = false;
            public Series ChartSeries = null;
            public Color? CounterColor = null;
            private RuleCounter(string Path, string WildcardPath = "", bool ShowInChart = true, bool HighlightInChart = false, bool Include_TotalSeriesInWildcard = true, Color? CounterColor = null)
            {
                this.Path = Path;
                this.WildcardPath = WildcardPath;
                this.ShowInChart = ShowInChart;
                this.HighlightInChart = HighlightInChart;
                this.CounterColor = CounterColor;
                this.Include_TotalSeriesInWildcard = Include_TotalSeriesInWildcard;
            }

            public static List<RuleCounter> CountersFromPath(string Path, bool ShowInChart = true, bool HighlightInChart = false, bool Include_TotalSeriesInWildcard = true,  Color? CounterColor = null)
            {
                ucASPerfMonAnalyzer HostControl = ((Program.MainForm.tcCollectionAnalysisTabs.TabPages[1].Controls["tcAnalysis"] as TabControl).TabPages["Performance Logs"].Controls[0] as ucASPerfMonAnalyzer);
                List<RuleCounter> counters = new List<RuleCounter>();
                string[] parts = Path.Split('\\');
                if (parts.Length > 1)
                {
                    string counter = parts[0] + "\\" + parts[1];
                    if (parts.Length > 2)
                    {
                        TreeNode node = HostControl.tvCounters.FindNodeByPath(counter);
                        if (node == null)
                        {
                            parts = ucASPerfMonAnalyzer.FullPathAlternateHierarchy(Path).Split('\\');
                            counter = parts[0] + "\\" + parts[1];
                            node = HostControl.tvCounters.FindNodeByPath(counter);
                        }
                        if (node != null)
                        {
                            if (parts[2].Contains("*"))
                            {
                                foreach (TreeNode child in node.Nodes)
                                    if ((parts[2].StartsWith("-") && child.Text != "_Total") ||
                                        !parts[2].StartsWith("-"))
                                    {
                                        if (parts.Length > 3)
                                        {
                                            if (parts[3].Contains("*"))
                                            {
                                                foreach (TreeNode grandchild in child.Nodes)
                                                {
                                                    if (Include_TotalSeriesInWildcard || parts[3] != "_Total")
                                                        counters.Add(new RuleCounter(counter + "\\" + parts[3] + "\\" + grandchild.Name, Path, ShowInChart, HighlightInChart, Include_TotalSeriesInWildcard, CounterColor));
                                                }
                                            }
                                            else
                                            if (Include_TotalSeriesInWildcard || parts[3] != "_Total")
                                                counters.Add(new RuleCounter(counter + "\\" + parts[3], Path, ShowInChart, HighlightInChart, Include_TotalSeriesInWildcard, CounterColor));
                                        }
                                        if (Include_TotalSeriesInWildcard || child.Name != "_Total")
                                            counters.Add(new RuleCounter(counter + "\\" + child.Name, Path, ShowInChart, HighlightInChart, Include_TotalSeriesInWildcard, CounterColor));
                                    }
                            }
                            else
                            if (Include_TotalSeriesInWildcard || parts[2] != "_Total")
                                counters.Add(new RuleCounter(counter + "\\" + parts[2], Path, ShowInChart, HighlightInChart, Include_TotalSeriesInWildcard, CounterColor));
                        }
                    }
                    else
                    if (Include_TotalSeriesInWildcard || Path != "_Total")
                        counters.Add(new RuleCounter(counter, Path, ShowInChart, HighlightInChart, Include_TotalSeriesInWildcard, CounterColor));
                }
                return counters;
            }
        }

        public class Rule : INotifyPropertyChanged
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
            public string ResultDescription { get; set; } = "Rule has not been run.";

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
            public RuleResultEnum RuleResult
            {
                get { return ruleResult; }
                set
                {
                    OnPropertyChanged("RuleResultImg");
                    if (value == RuleResultEnum.CountersUnavailable)
                        ResultDescription = "Missing required counter(s) to run rule.";
                    if (value == RuleResultEnum.NotRun)
                        ResultDescription = "Rule has not been run.";
                    OnPropertyChanged("ResultDescription");
                    ruleResult = value;
                }
            }

            private Action ruleFunction = null;
            [Browsable(false)]
            public Action RuleFunction
            {
                get { return ruleFunction; } set { ruleFunction = value; }
            }

            public double MaxValueForRule()
            {
                double max = double.MinValue;
                foreach (RuleCounter rc in Counters)
                    if (rc.ChartSeries.Tag != null)
                    {
                        if ((double)rc.ChartSeries.Tag > max && rc.ShowInChart)
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
                s.BorderColor = Color.FromArgb(128, color);
                s.ForeColor = Color.Transparent;
                s.IntervalOffset = y > y2 ? y2 : y;
                double stripWidth = Math.Abs(y2 - y);
                s.StripWidth = Math.Abs(y2 - y);
                s.BorderWidth = 1;

                CustomStripLines.Add(s);
                return s;
            }

            public void ValidateThresholdRule(double ScalarComparison, double WarnY, double ErrorY, string WarningLineText = "", string ErrorLineText = "", string PassLineText = "", bool CheckIfValueBelowWarnError = true, bool CheckAvg = false, bool IncludeZerosInAvg = true, bool IncludeNullsInAvg = false, int PctValuesToTriggerWarnFail = 0)
            {
                Series s = new Series();
                foreach (DataPoint p in Counters[0].ChartSeries.Points)
                    s.Points.AddXY(p.XValue, ScalarComparison);
                ValidateThresholdRule(s, WarnY, ErrorY, WarningLineText, ErrorLineText, PassLineText, CheckIfValueBelowWarnError, CheckAvg, IncludeZerosInAvg, IncludeNullsInAvg, PctValuesToTriggerWarnFail);
            }
            public void ValidateThresholdRule(Series series, double WarnY, double ErrorY, string WarningLineText = "", string ErrorLineText = "", string PassLineText = "", bool CheckIfValueBelowWarnError = true, bool CheckAvg = false, bool IncludeZerosInAvg = true, bool IncludeNullsInAvg = false, int PctValuesToTriggerWarnFail = 0)
            {
                List<Series> s = new List<Series>();
                s.Add(series);
                ValidateThresholdRule(s, WarnY, ErrorY, WarningLineText, ErrorLineText, PassLineText, CheckIfValueBelowWarnError, CheckAvg, IncludeZerosInAvg, IncludeNullsInAvg, PctValuesToTriggerWarnFail);
            }
            public void ValidateThresholdRule(List<Series> series, double WarnY, double ErrorY, string WarningLineText = "", string ErrorLineText= "", string PassLineText = "", bool FailIfValueBelowWarnError = true, bool CheckAvg = false, bool IncludeZerosInAvg = true, bool IncludeNullsInAvg = false, int PctValuesToTriggerWarnFail = 0)
            {
                Color WarnColor = Color.Khaki;
                Color ErrorColor = Color.Pink;
                Color PassColor = Color.LightGreen;

                StripLine WarnRegion = null;
                StripLine ErrorRegion = null;
                StripLine PassRegion = null;
                if (FailIfValueBelowWarnError)
                {
                    if (ErrorLineText != "")
                    {
                        PassRegion = AddStripLine(PassLineText, (WarnY > 0 ? WarnY : ErrorY), MaxValueForRule() * 1.05, PassColor);
                        CustomStripLines.Add(PassRegion);
                        ErrorRegion = AddStripLine(ErrorLineText, 0, ErrorY, ErrorColor);
                        if (WarningLineText != "")
                            WarnRegion = AddStripLine(WarningLineText, WarnY > 0 ? WarnY : 0, ErrorY, WarnColor);
                    }
                }
                else
                {
                    if (ErrorLineText != "")
                    {
                        ErrorRegion = AddStripLine(ErrorLineText, MaxValueForRule() * 1.05, ErrorY, ErrorColor);
                        PassRegion = AddStripLine(PassLineText, 0, WarnY > 0 ? WarnY : ErrorY, PassColor);
                    }
                    if (WarningLineText != "")
                        WarnRegion = AddStripLine(WarningLineText, WarnY, ErrorY, WarnColor);
                }

                foreach (Series s in series)
                {
                    double comparisonValue = 0;
                    if (PctValuesToTriggerWarnFail > 0)
                    {
                        double dCount = 0;
                        if (FailIfValueBelowWarnError)
                            dCount = s.Points.Where(p => p.YValues[0] <= ErrorY).Count();
                        else
                            dCount = s.Points.Where(p => p.YValues[0] >= ErrorY).Count();
                        if ((double)dCount / (double)s.Points.Count * 100.0> PctValuesToTriggerWarnFail / 100.0)
                        {
                            RuleResult = RuleResultEnum.Fail;
                            return;
                        }
                        if (FailIfValueBelowWarnError)
                            dCount = s.Points.Where(p => p.YValues[0] <= WarnY).Count();
                        else
                            dCount = s.Points.Where(p => p.YValues[0] >= WarnY).Count();
                        if ((double)dCount / (double)s.Points.Count * 100.0 > PctValuesToTriggerWarnFail / 100.0)
                        {
                            RuleResult = RuleResultEnum.Warn;
                            return;
                        }
                    }
                    else
                    {
                        comparisonValue = CheckAvg ? s.Points.AverageValue(true, IncludeZerosInAvg) : FailIfValueBelowWarnError ? s.Points.FindMaxByValue().YValues[0] : s.Points.FindMinByValue().YValues[0];
                        if (FailIfValueBelowWarnError)
                        {
                            if (comparisonValue <= ErrorY)
                                RuleResult = RuleResultEnum.Fail;
                            else if (comparisonValue <= WarnY)
                                RuleResult = RuleResultEnum.Warn;
                        }
                        else
                        {
                            if (comparisonValue >= ErrorY)
                                RuleResult = RuleResultEnum.Fail;
                            else if (comparisonValue >= WarnY)
                                RuleResult = RuleResultEnum.Warn;
                        }
                    }
                    if (RuleResult != RuleResultEnum.Pass)
                        return;
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

    static class Extensions
    {
        public static double AverageValue(this IEnumerable<DataPoint> points, bool IncludeNull = false, bool IncludeZero = true)
        {
            double sumY = 0, count = 0;
            foreach (var pt in points)
            {
                if ((pt.YValues[0] != null || !IncludeNull) && (pt.YValues[0] != 0 || !IncludeZero))
                {
                    sumY += pt.YValues[0];
                    count++;
                }
            }
            // also calc average time?
            if (count == 0)
                return 0;
            return sumY / count;
        }

        public static bool In<T>(this T t, params T[] values)
        {
            foreach (T value in values)
            {
                if (t.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }
       
    }
}
