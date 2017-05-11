<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SSASDiagUsageStats.aspx.cs" Inherits="SSASDiagUsageStats"  EnableSessionState="True" %>
<%
    if (Request.QueryString["UsageVersion"] != null && Request.QueryString["FeatureName"] != null && Request.QueryString["RunID"] != null)
    {
        System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection("Server=.;Database=SSASDiag;User=WebUsr;Password=Password1;");
        conn.Open();
        System.Data.SqlClient.SqlCommand cmd =
            new System.Data.SqlClient.SqlCommand("insert into SSASDiagUsageStats Values('" + Request.UserHostAddress + "', '" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff") +
                                                 "', '" + HttpUtility.UrlDecode(Request.QueryString["RunID"]) + "', '" + HttpUtility.UrlDecode(Request.QueryString["UsageVersion"]) + "', '" +
                                                 HttpUtility.UrlDecode(Request.QueryString["FeatureName"]) + "', '" + HttpUtility.UrlDecode(Request.QueryString["FeatureDetail"]) + "', '" + (Request.QueryString["MicrosoftInternal"] ) + "')", conn);
        cmd.ExecuteNonQuery();
        conn.Close();
    }
%>