namespace SimpleMDXParser
{
    using System;
    using System.Collections.Generic;

    public class MDXParserObjects
    {
        public static string[] s_Flags = new string[] { "SELF", "BEFORE", "AFTER", "BEFORE_AND_AFTER", "SELF_AND_BEFORE", "SELF_AND_AFTER", "SELF_BEFORE_AFTER", "LEAVES", "ALL", "CONSTRAINED", "DESC", "BDESC", "ASC", "BASC", "EXCLUDEEMPTY", "NULL" };
        internal static Dictionary<string, string> s_FlagsMap;
        public static string[] s_Keywords = new string[] { 
            "AS", "IS", "OR", "AND", "XOR", "NOT", "WITH", "SELECT", "ON", "COLUMNS", "ROWS", "PAGES", "FROM", "WHERE", "NON EMPTY", "HAVING", 
            "DIMENSION PROPERTIES", "PROPERTIES", "CELL PROPERTIES", "FOR", "EXISTING", "DISTINCT", "CASE", "WHEN", "THEN", "ELSE", "END", "END SCOPE", "END IF", "CALCULATE", "CREATE", "ALTER CUBE", 
            "UPDATE CUBE", "SCOPE", "FREEZE", "IF", "DRILLTHROUGH", "MAXROWS", "FIRSTROWSET", "RETURN", "MEMBER", "SET"
         };
        internal static Dictionary<string, string> s_KeywordsMap;
        public static MDXObject[] s_Objects = new MDXObject[] { 
            new MDXObject("CurrentMember", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Unknown), 
            new MDXObject("NextMember", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Member), 
            new MDXObject("PrevMember", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Member), 
            new MDXObject("Current", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Set), 
            new MDXObject("Parent", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Member), new MDXObject("FirstChild", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Member), new MDXObject("LastChild", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Member), new MDXObject("FirstSibling", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Member), new MDXObject("LastSibling", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Member), new MDXObject("DefaultMember", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Hierarchy), new MDXObject("UnknownMember", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Hierarchy), new MDXObject("DataMember", MDXSyntaxForm.Property, MDXDataType.Member, MDXFunctionOpt.Optimized, MDXDataType.Member), new MDXObject("MEMBERS", MDXSyntaxForm.Property, MDXDataType.Set, MDXDataType.Hierarchy), new MDXObject("ALLMEMBERS", MDXSyntaxForm.Property, MDXDataType.Set, MDXDataType.Hierarchy), new MDXObject("Children", MDXSyntaxForm.Property, MDXDataType.Set, MDXDataType.Member), new MDXObject("Siblings", MDXSyntaxForm.Property, MDXDataType.Set, MDXDataType.Member), 
            new MDXObject("Value", MDXSyntaxForm.Property, MDXDataType.Number, MDXFunctionOpt.Optimized), 
            new MDXObject("MemberValue", MDXSyntaxForm.Property, MDXDataType.Number, MDXDataType.Member), 
            new MDXObject("Member_Caption", MDXSyntaxForm.Property, MDXDataType.String, MDXDataType.Member), 
            new MDXObject("Member_Value", MDXSyntaxForm.Property, MDXDataType.String, MDXDataType.Member),
            new MDXObject("Member_Key", MDXSyntaxForm.Property, MDXDataType.Number, MDXDataType.Member), 
            new MDXObject("Name", MDXSyntaxForm.Property, MDXDataType.String, MDXFunctionOpt.Optimized, MDXDataType.Member), 
            new MDXObject("UniqueName", MDXSyntaxForm.Property, MDXDataType.String, MDXFunctionOpt.Optimized, MDXDataType.Member), 
            new MDXObject("Key0", MDXSyntaxForm.Property, MDXDataType.Number, MDXDataType.Member), 
            new MDXObject("Count", MDXSyntaxForm.Property, MDXDataType.Number, MDXFunctionOpt.Optimized), new MDXObject("CurrentOrdinal", MDXSyntaxForm.Property, MDXDataType.Number), new MDXObject("Ordinal", MDXSyntaxForm.Property, MDXDataType.Number), new MDXObject("CalculationCurrentPass", MDXSyntaxForm.Property, MDXDataType.Number), new MDXObject("Hierarchy", MDXSyntaxForm.Property, MDXDataType.Hierarchy), new MDXObject("Dimension", MDXSyntaxForm.Property, MDXDataType.Hierarchy, MDXFunctionOpt.Deprecated), new MDXObject("Level", MDXSyntaxForm.Property, MDXDataType.Level), new MDXObject("AddCalculatedMembers", MDXSyntaxForm.Function, MDXDataType.Set, "set"), new MDXObject("Aggregate", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Optimized, "set [,expr]"), 
            new MDXObject("Ancestor", MDXSyntaxForm.Function, MDXDataType.Set, MDXFunctionOpt.Optimized), 
            new MDXObject("Ancestors", MDXSyntaxForm.Function, MDXDataType.Set, MDXFunctionOpt.Optimized), new MDXObject("Ascendants", MDXSyntaxForm.Function, MDXDataType.Set, MDXFunctionOpt.Optimized, "set"), new MDXObject("Avg", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Optimized, "set [,expr]"), new MDXObject("Axis", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("BottomCount", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("BottomPercent", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("BottomSum", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("CalculationPassValue", MDXSyntaxForm.Function, MDXDataType.Set, MDXFunctionOpt.Optimized), new MDXObject("ClosingPeriod", MDXSyntaxForm.Function, MDXDataType.Set, MDXFunctionOpt.Optimized), new MDXObject("CoalesceEmpty", MDXSyntaxForm.Function, MDXDataType.Set, MDXFunctionOpt.Optimized), new MDXObject("Correlation", MDXSyntaxForm.Function, MDXDataType.Number), new MDXObject("Cousin", MDXSyntaxForm.Function, MDXDataType.Member, MDXFunctionOpt.Optimized), new MDXObject("Covariance", MDXSyntaxForm.Function, MDXDataType.Number), new MDXObject("CovarianceN", MDXSyntaxForm.Function, MDXDataType.Number), new MDXObject("CrossJoin", MDXSyntaxForm.Function, MDXDataType.Set, "set1 ,set2 [ ,set3 ...]"), 
            new MDXObject("CustomData", MDXSyntaxForm.Function, MDXDataType.String), new MDXObject("Descendants", MDXSyntaxForm.Function, MDXDataType.Set, "member or set , level or distance [, flags]"), new MDXObject("Distinct", MDXSyntaxForm.Function, MDXDataType.Set, "set"), new MDXObject("DistinctCount", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Bad), new MDXObject("DrillDownLevel", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("DrillDownLevelBottom", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("DrillDownLevelTop", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("DrillDownMember", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("DrillDownMemberBottom", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("DrillDownMemberTop", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("DrillUpLevel", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("DrillUpMember", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("Except", MDXSyntaxForm.Function, MDXDataType.Set, "set, exceptset"), new MDXObject("Exists", MDXSyntaxForm.Function, MDXDataType.Set, "set [,filterset] [,measuregroup_name]"), new MDXObject("Extract", MDXSyntaxForm.Function, MDXDataType.Set, "set, hierarchy"), new MDXObject("Filter", MDXSyntaxForm.Function, MDXDataType.Set, "set ,condition"), 
            new MDXObject("Generate", MDXSyntaxForm.Function, MDXDataType.Unknown, "set ,generator_exp"), new MDXObject("Head", MDXSyntaxForm.Function, MDXDataType.Set, "set, count_exp"), new MDXObject("Hierarchize", MDXSyntaxForm.Function, MDXDataType.Set, "set [,flag]"), 
            new MDXObject("IIF", MDXSyntaxForm.Function, MDXDataType.Unknown, MDXFunctionOpt.Optimized, "condition, then_exp, else_exp"), 
            new MDXObject("Intersect", MDXSyntaxForm.Function, MDXDataType.Set, "set1, set2"), new MDXObject("IsAnsector", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("IsEmpty", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Optimized, "exp"), new MDXObject("IsGeneration", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Bad), new MDXObject("IsLeaf", MDXSyntaxForm.Function, MDXDataType.Number), new MDXObject("IsSibling", MDXSyntaxForm.Function, MDXDataType.Number), new MDXObject("Item", MDXSyntaxForm.Method, MDXDataType.Tuple, MDXFunctionOpt.Optimized, "index or name", MDXDataType.Set), new MDXObject("KPICurrentMember", MDXSyntaxForm.Function, MDXDataType.Member, "kpiname"), new MDXObject("KPIGoal", MDXSyntaxForm.Function, MDXDataType.Member, MDXFunctionOpt.Optimized, "kpiname"), new MDXObject("KPIStatus", MDXSyntaxForm.Function, MDXDataType.Member, MDXFunctionOpt.Optimized, "kpiname"), new MDXObject("KPITrend", MDXSyntaxForm.Function, MDXDataType.Member, MDXFunctionOpt.Optimized, "kpiname"), new MDXObject("KPIValue", MDXSyntaxForm.Function, MDXDataType.Member, MDXFunctionOpt.Optimized, "kpiname"), 
            new MDXObject("KPIWeight", MDXSyntaxForm.Function, MDXDataType.Member, MDXFunctionOpt.Optimized, "kpiname"), new MDXObject("LastPeriods", MDXSyntaxForm.Function, MDXDataType.Set, MDXFunctionOpt.Optimized), new MDXObject("Leaves", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("Levels", MDXSyntaxForm.Property, MDXDataType.Level), new MDXObject("LinkMember", MDXSyntaxForm.Function, MDXDataType.Member, MDXFunctionOpt.Bad), new MDXObject("LinRegIntercept", MDXSyntaxForm.Function, MDXDataType.Number), new MDXObject("LinRegPoint", MDXSyntaxForm.Function, MDXDataType.Number), new MDXObject("LinRegR2", MDXSyntaxForm.Function, MDXDataType.Number), new MDXObject("LinRegSlope", MDXSyntaxForm.Function, MDXDataType.Number), new MDXObject("LinRegVariance", MDXSyntaxForm.Function, MDXDataType.Number), new MDXObject("LookupCube", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Bad), new MDXObject("Max", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Optimized, "set [,exp]"), new MDXObject("Median", MDXSyntaxForm.Function, MDXDataType.Number, "set [,exp]"), new MDXObject("MemberToStr", MDXSyntaxForm.Function, MDXDataType.String, MDXFunctionOpt.Bad), new MDXObject("Min", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Optimized, "set [,exp]"), new MDXObject("MTD", MDXSyntaxForm.Function, MDXDataType.Set), 
            new MDXObject("NameToSet", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("NonEmpty", MDXSyntaxForm.Function, MDXDataType.Set, "set [,filterset]"), new MDXObject("NonEmptyCrossJoin", MDXSyntaxForm.Function, MDXDataType.Set, MDXFunctionOpt.Deprecated), new MDXObject("OpeningPeriod", MDXSyntaxForm.Function, MDXDataType.Member, MDXFunctionOpt.Optimized), new MDXObject("Order", MDXSyntaxForm.Function, MDXDataType.Set, "set, sortexp [,flag]"), new MDXObject("ParallelPeriod", MDXSyntaxForm.Function, MDXDataType.Member, MDXFunctionOpt.Optimized), new MDXObject("PeriodsToDate", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("Predict", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Bad), new MDXObject("QTD", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("Rank", MDXSyntaxForm.Function, MDXDataType.Number, "tuple, set [,rankexp]"), new MDXObject("RollupChildren", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Deprecated), new MDXObject("Root", MDXSyntaxForm.Function, MDXDataType.Tuple), new MDXObject("SetToArray", MDXSyntaxForm.Function, MDXDataType.Unknown, MDXFunctionOpt.Deprecated), new MDXObject("SetToStr", MDXSyntaxForm.Function, MDXDataType.String, MDXFunctionOpt.Bad), new MDXObject("StdDev", MDXSyntaxForm.Function, MDXDataType.Number, "set [,exp]"), new MDXObject("StdDevP", MDXSyntaxForm.Function, MDXDataType.Number, "set [,exp]"), 
            new MDXObject("StDev", MDXSyntaxForm.Function, MDXDataType.Number, "set [,exp]"), new MDXObject("StDevP", MDXSyntaxForm.Function, MDXDataType.Number, "set [,exp]"), new MDXObject("StripCalculatedMembers", MDXSyntaxForm.Function, MDXDataType.Set, "set"), new MDXObject("StrToMember", MDXSyntaxForm.Function, MDXDataType.Member, MDXFunctionOpt.Optimized), new MDXObject("StrToSet", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("StrToTuple", MDXSyntaxForm.Function, MDXDataType.Tuple), new MDXObject("StrToValue", MDXSyntaxForm.Function, MDXDataType.Number), new MDXObject("SubSet", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("Sum", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Optimized, "set [,exp]"), new MDXObject("Tail", MDXSyntaxForm.Function, MDXDataType.Set, "set, countexp"), new MDXObject("ToggleDrillState", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("TopCount", MDXSyntaxForm.Function, MDXDataType.Set, "set, count, exp"), new MDXObject("TopPercent", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("TopSum", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("TupleToStr", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Bad), new MDXObject("Union", MDXSyntaxForm.Function, MDXDataType.Set, "set1, set2"), 
            new MDXObject("UnOrder", MDXSyntaxForm.Function, MDXDataType.Set, "set"), new MDXObject("Username", MDXSyntaxForm.Function, MDXDataType.String), new MDXObject("ValidMeasure", MDXSyntaxForm.Function, MDXDataType.Number, MDXFunctionOpt.Optimized, "tuple"), new MDXObject("Var", MDXSyntaxForm.Function, MDXDataType.Number, "set [,exp]"), new MDXObject("Variance", MDXSyntaxForm.Function, MDXDataType.Number, "set [,exp]"), new MDXObject("VarianceP", MDXSyntaxForm.Function, MDXDataType.Number, "set [,exp]"), new MDXObject("VarP", MDXSyntaxForm.Function, MDXDataType.Number, "set [,exp]"), new MDXObject("VisualTotals", MDXSyntaxForm.Function, MDXDataType.Set, "set"), new MDXObject("WTD", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("YTD", MDXSyntaxForm.Function, MDXDataType.Set), new MDXObject("Lag", MDXSyntaxForm.Method, MDXDataType.Member, MDXFunctionOpt.Optimized, "offsetexp", MDXDataType.Member), new MDXObject("Lead", MDXSyntaxForm.Method, MDXDataType.Member, MDXFunctionOpt.Optimized, "offsetexp", MDXDataType.Member), new MDXObject("Properties", MDXSyntaxForm.Method, MDXDataType.String, MDXFunctionOpt.Optimized, "propertyname", MDXDataType.Member)
         };
        internal static Dictionary<string, MDXObject> s_ObjectsMap = new Dictionary<string, MDXObject>(StringComparer.InvariantCultureIgnoreCase);

        static MDXParserObjects()
        {
            foreach (MDXObject obj2 in s_Objects)
            {
                s_ObjectsMap.Add(obj2.CanonicalName, obj2);
            }
            s_FlagsMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string str in s_Flags)
            {
                s_FlagsMap.Add(str, str);
            }
            s_KeywordsMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string str2 in s_Keywords)
            {
                s_KeywordsMap.Add(str2, str2);
            }
        }

        public static MDXObject FindMDXObject(string objname)
        {
            MDXObject obj2;
            if (s_ObjectsMap.TryGetValue(objname, out obj2))
            {
                return obj2;
            }
            return null;
        }

        public static bool IsFlag(string flag)
        {
            return s_FlagsMap.ContainsKey(flag);
        }

        public static bool IsKeyword(string Keyword)
        {
            return s_KeywordsMap.ContainsKey(Keyword);
        }
    }
}

