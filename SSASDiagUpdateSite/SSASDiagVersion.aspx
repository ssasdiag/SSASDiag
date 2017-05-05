<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SSASDiagVersion.aspx.cs" Inherits="SSASDiagVersion" EnableEventValidation="true" %>
<%
    System.Diagnostics.FileVersionInfo f = System.Diagnostics.FileVersionInfo.GetVersionInfo(Server.MapPath("ssasdiag.exe"));
    Response.Write("Version=" + f.FileVersion);
 %>
