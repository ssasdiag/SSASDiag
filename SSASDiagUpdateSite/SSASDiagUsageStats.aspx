<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SSASDiagUsageStats.aspx.cs" Inherits="SSASDiagUsageStats"  EnableSessionState="True" %>
<%
    if (Request.Form["UsageVersion"] != null && Request.Form["FeatureName"] != null && Request.Form["RunID"] != null)
    {
        System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection("Server=.;Database=SSASDiag;User=WebUsr;Password=Password1;");
        conn.Open();
        System.Data.SqlClient.SqlCommand cmd =
            new System.Data.SqlClient.SqlCommand("insert into SSASDiagUsageStats Values('" + Request.UserHostAddress + "', '" + 
                                                 DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff") +
                                                 "', '" + HttpUtility.UrlDecode(Request.Form["RunID"]) + "', '" + 
                                                 HttpUtility.UrlDecode(Request.Form["UsageVersion"]) + "', '" +
                                                 HttpUtility.UrlDecode(Request.Form["FeatureName"]) + "', '" + 
                                                 HttpUtility.UrlDecode(Request.Form["FeatureDetail"]) + "', '" + 
                                                 HttpUtility.UrlDecode(Request.Form["MicrosoftInternal"]) + "', '" +
                                                 HttpUtility.UrlDecode(Request.Form["UpnSuffix"]) + "')", conn);
        cmd.ExecuteNonQuery();
        conn.Close();
    }
%>