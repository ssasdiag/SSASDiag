<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SSASMagicNumberService.aspx.cs" Inherits="SSASMagicNumberService"  EnableSessionState="False" %>
<%@ Import Namespace="System.IO"  %>
<%@ Import Namespace="System.Threading" %>
<%
    string SignalFile = "c:\\magic\\magicnumbers.data";

    if (Request.QueryString["c"] != null)
    {
        string SymbolTimeSizeStamp = "";
        SymbolTimeSizeStamp = Request.QueryString["c"];
        bool bFileAccessed = false;
        List<string> lines = new List<string>();
        while (!bFileAccessed)
        {
            try
            {
                lines = File.ReadAllLines(SignalFile).ToList();
                bFileAccessed = true;
            }
            catch
            {
                Thread.Sleep(250);
            }
        }
        if (lines.Where(s=>s.StartsWith(SymbolTimeSizeStamp)).Count() > 0 && lines.Find(s=>s.StartsWith(SymbolTimeSizeStamp)).Replace(SymbolTimeSizeStamp + "=", "") != "")
            Response.Write(lines.Find(s => s.StartsWith(SymbolTimeSizeStamp)).Replace(SymbolTimeSizeStamp + "=", ""));
        else
        {
            bFileAccessed = false;
            while (!bFileAccessed)
            {
                try
                {
                    File.AppendAllText(SignalFile, "\r\n" + SymbolTimeSizeStamp + "=");
                    bFileAccessed = true;
                }
                catch
                {
                    Thread.Sleep(250);
                }
            }
            while (true)
            {
                Thread.Sleep(2000);
                bFileAccessed = false;
                while (!bFileAccessed)
                {
                    try
                    {
                        lines = File.ReadAllLines(SignalFile).ToList();
                        bFileAccessed = true;
                    }
                    catch
                    {
                        Thread.Sleep(250);
                    }
                }
                if (!lines.Find(s => s.StartsWith(SymbolTimeSizeStamp)).Trim().EndsWith("="))
                {
                    Response.Write(lines.Find(s => s.StartsWith(SymbolTimeSizeStamp)).Replace(SymbolTimeSizeStamp + "=", ""));
                    return;
                }
            }
        }
    }
%>