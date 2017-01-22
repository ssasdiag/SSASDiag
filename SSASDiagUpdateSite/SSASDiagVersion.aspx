<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SSASDiagVersion.aspx.cs" Inherits="SSASDiagVersion" EnableEventValidation="true" %>
<%
    System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection("Server=.;Database=SSASDiag;User=WebUsr;Password=Password1;");
    conn.Open();
    System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select CaseNum from CaseForIp where IP = '" + Request.UserHostAddress + "'", conn);
    string existingCaseForThisIP = cmd.ExecuteScalar() as string;
    if (Request.QueryString["Case"] != null)
    {
        if (existingCaseForThisIP != null && Request.QueryString["Case"] != existingCaseForThisIP)
        {
            cmd = new System.Data.SqlClient.SqlCommand("update CaseForIp set CaseNum = '" + Request.QueryString["Case"] + "' where IP = '" + Request.UserHostAddress + "'", conn);
            cmd.ExecuteNonQuery();
            existingCaseForThisIP = Request.QueryString["Case"];
        }
        else if (existingCaseForThisIP == null)
        {
            cmd = new System.Data.SqlClient.SqlCommand("insert into CaseForIp values ('" + Request.UserHostAddress + "', '" + Request.QueryString["Case"] + "', '" + DateTime.Now + "')", conn);
            cmd.ExecuteNonQuery();
            existingCaseForThisIP = Request.QueryString["Case"];
        }
    }

    System.Diagnostics.FileVersionInfo f = System.Diagnostics.FileVersionInfo.GetVersionInfo(Server.MapPath("ssasdiag.exe"));
    Response.Write("Version=" + f.FileVersion);
    if (existingCaseForThisIP != null) Response.Write("\nCase=" + existingCaseForThisIP.Trim());
 %>
