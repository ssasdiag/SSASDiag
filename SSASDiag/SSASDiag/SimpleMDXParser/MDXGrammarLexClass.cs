using SimpleMDXParser;
using SSVParseLib;
using System;
using System.Collections.Generic;
using System.Drawing;

public class MDXGrammarLexClass : SSLex
{
    internal ColorCoding m_ColorCoding;
    private List<string> m_Comments;
    public Source m_Source;
    public const int MDXGrammarLexExprETKMultiLineComment = 1;
    public const int MDXGrammarLexExprMain = 0;
    public const int MDXGrammarLexTokenKALTER = 0x77;
    public const int MDXGrammarLexTokenKAmp = 0x3b;
    public const int MDXGrammarLexTokenKAND = 0x58;
    public const int MDXGrammarLexTokenKAS = 0x55;
    public const int MDXGrammarLexTokenKBang = 60;
    public const int MDXGrammarLexTokenKBraceClose = 0x41;
    public const int MDXGrammarLexTokenKBraceOpen = 0x40;
    public const int MDXGrammarLexTokenKCALCULATE = 0x75;
    public const int MDXGrammarLexTokenKCALCULATION = 0x6a;
    public const int MDXGrammarLexTokenKCASE = 0x70;
    public const int MDXGrammarLexTokenKCComment = 0x37;
    public const int MDXGrammarLexTokenKCELL = 0x67;
    public const int MDXGrammarLexTokenKColon = 0x3a;
    public const int MDXGrammarLexTokenKComma = 0x39;
    public const int MDXGrammarLexTokenKCOUNT = 0x69;
    public const int MDXGrammarLexTokenKCPPComment = 0x35;
    public const int MDXGrammarLexTokenKCREATE = 0x76;
    public const int MDXGrammarLexTokenKCUBE = 120;
    public const int MDXGrammarLexTokenKDIMENSION = 0x66;
    public const int MDXGrammarLexTokenKDISTINCT = 0x6f;
    public const int MDXGrammarLexTokenKDivide = 0x45;
    public const int MDXGrammarLexTokenKDot = 0x38;
    public const int MDXGrammarLexTokenKDRILLTHROUGH = 0x7e;
    public const int MDXGrammarLexTokenKDYNAMIC = 0x6d;
    public const int MDXGrammarLexTokenKELSE = 0x73;
    public const int MDXGrammarLexTokenKEMPTY = 0x63;
    public const int MDXGrammarLexTokenKEND = 0x74;
    public const int MDXGrammarLexTokenKEqual = 0x49;
    public const int MDXGrammarLexTokenKEXISTING = 110;
    public const int MDXGrammarLexTokenKFIRSTROWSET = 0x80;
    public const int MDXGrammarLexTokenKFOR = 0x6b;
    public const int MDXGrammarLexTokenKFREEZE = 0x7c;
    public const int MDXGrammarLexTokenKFROM = 0x60;
    public const int MDXGrammarLexTokenKGO = 130;
    public const int MDXGrammarLexTokenKGreater = 0x4b;
    public const int MDXGrammarLexTokenKGreaterEqual = 0x4c;
    public const int MDXGrammarLexTokenKHash = 0x4e;
    public const int MDXGrammarLexTokenKHAVING = 0x65;
    public const int MDXGrammarLexTokenKHIDDEN = 100;
    public const int MDXGrammarLexTokenKIF = 0x7d;
    public const int MDXGrammarLexTokenKIS = 0x56;
    public const int MDXGrammarLexTokenKLess = 0x47;
    public const int MDXGrammarLexTokenKLessEqual = 0x48;
    public const int MDXGrammarLexTokenKMAXROWS = 0x7f;
    public const int MDXGrammarLexTokenKMEMBER = 0x5c;
    public const int MDXGrammarLexTokenKMinus = 0x43;
    public const int MDXGrammarLexTokenKMult = 0x44;
    public const int MDXGrammarLexTokenKNON = 0x62;
    public const int MDXGrammarLexTokenKNOT = 90;
    public const int MDXGrammarLexTokenKNotEqual = 0x4a;
    public const int MDXGrammarLexTokenKON = 0x5f;
    public const int MDXGrammarLexTokenKOR = 0x57;
    public const int MDXGrammarLexTokenKParClose = 0x3f;
    public const int MDXGrammarLexTokenKParOpen = 0x3e;
    public const int MDXGrammarLexTokenKPlus = 0x42;
    public const int MDXGrammarLexTokenKPower = 70;
    public const int MDXGrammarLexTokenKPROPERTIES = 0x68;
    public const int MDXGrammarLexTokenKRETURN = 0x81;
    public const int MDXGrammarLexTokenKSCOPE = 0x7b;
    public const int MDXGrammarLexTokenKSELECT = 0x5e;
    public const int MDXGrammarLexTokenKSemiColon = 0x3d;
    public const int MDXGrammarLexTokenKSESSION = 0x6c;
    public const int MDXGrammarLexTokenKSET = 0x5d;
    public const int MDXGrammarLexTokenKShtrudel = 0x4d;
    public const int MDXGrammarLexTokenKSQLComment = 0x36;
    public const int MDXGrammarLexTokenKSUBCUBE = 0x79;
    public const int MDXGrammarLexTokenKTHEN = 0x72;
    public const int MDXGrammarLexTokenKUPDATE = 0x7a;
    public const int MDXGrammarLexTokenKWHEN = 0x71;
    public const int MDXGrammarLexTokenKWHERE = 0x61;
    public const int MDXGrammarLexTokenKWITH = 0x5b;
    public const int MDXGrammarLexTokenKXOR = 0x59;
    public const int MDXGrammarLexTokenTGProperty = 0x83;
    public const int MDXGrammarLexTokenTKFloat = 0x54;
    public const int MDXGrammarLexTokenTKID = 0x51;
    public const int MDXGrammarLexTokenTKParam = 0x52;
    public const int MDXGrammarLexTokenTKScalar = 0x53;
    public const int MDXGrammarLexTokenTKString1 = 80;
    public const int MDXGrammarLexTokenTKString2 = 0x4f;

    public MDXGrammarLexClass(SSLexTable q_table, SSLexConsumer q_consumer) : base(q_table, q_consumer)
    {
        this.m_Comments = new List<string>();
    }

    public override bool complete(SSLexLexeme q_lexeme)
    {
        int num = q_lexeme.token();
        if (((0x35 == num) || (0x36 == num)) || (0x37 == num))
        {
            this.m_ColorCoding.ApplyColor(q_lexeme, Color.Green);
            this.m_Comments.Add(new string(q_lexeme.lexeme()));
            return false;
        }
        if ((0x4f == num) || (80 == num))
        {
            this.m_ColorCoding.ApplyColor(q_lexeme, Color.Red);
        }
        else if ((0x53 == num) || (0x54 == num))
        {
            this.m_ColorCoding.ApplyColor(q_lexeme, Color.Silver);
        }
        else if ((((((0x55 == num) || (0x56 == num)) || ((0x57 == num) || (0x58 == num))) || (((0x59 == num) || (90 == num)) || ((0x5b == num) || (0x5c == num)))) || ((((0x5d == num) || (0x5e == num)) || ((0x5f == num) || (0x60 == num))) || (((0x61 == num) || (0x62 == num)) || ((0x63 == num) || (100 == num))))) || ((((((0x65 == num) || (0x66 == num)) || ((0x67 == num) || (0x68 == num))) || (((0x69 == num) || (0x6a == num)) || ((0x6b == num) || (0x6c == num)))) || ((((110 == num) || (0x6f == num)) || ((0x70 == num) || (0x71 == num))) || (((0x72 == num) || (0x73 == num)) || ((0x74 == num) || (0x75 == num))))) || (((((0x76 == num) || (0x77 == num)) || ((120 == num) || (0x79 == num))) || (((0x7a == num) || (0x7b == num)) || ((0x7c == num) || (0x7d == num)))) || (((0x7e == num) || (0x7f == num)) || ((0x80 == num) || (0x81 == num))))))
        {
            this.m_ColorCoding.ApplyColor(q_lexeme, Color.Blue);
        }
        if (this.m_Comments.Count > 0)
        {
            q_lexeme.Comments = this.m_Comments;
            this.m_Comments = new List<string>();
        }
        return true;
    }

    public string tokenToString(int q_token)
    {
        switch (q_token)
        {
            case -2:
                return "%error";

            case -1:
                return "eof";

            case 0x35:
                return "KCPPComment";

            case 0x36:
                return "KSQLComment";

            case 0x37:
                return "KCComment";

            case 0x38:
                return ".";

            case 0x39:
                return ",";

            case 0x3a:
                return ":";

            case 0x3b:
                return "&";

            case 60:
                return "!";

            case 0x3d:
                return ";";

            case 0x3e:
                return "(";

            case 0x3f:
                return ")";

            case 0x40:
                return "{";

            case 0x41:
                return "}";

            case 0x42:
                return "+";

            case 0x43:
                return "-";

            case 0x44:
                return "*";

            case 0x45:
                return "/";

            case 70:
                return "^";

            case 0x47:
                return "<";

            case 0x48:
                return "<=";

            case 0x49:
                return "=";

            case 0x4a:
                return "<>";

            case 0x4b:
                return ">";

            case 0x4c:
                return ">=";

            case 0x4d:
                return "@";

            case 0x4e:
                return "#";

            case 0x4f:
                return "TKString2";

            case 80:
                return "TKString1";

            case 0x51:
                return "TKID";

            case 0x52:
                return "TKParam";

            case 0x53:
                return "TKScalar";

            case 0x54:
                return "TKFloat";

            case 0x55:
                return "KAS";

            case 0x56:
                return "KIS";

            case 0x57:
                return "KOR";

            case 0x58:
                return "KAND";

            case 0x59:
                return "KXOR";

            case 90:
                return "KNOT";

            case 0x5b:
                return "KWITH";

            case 0x5c:
                return "KMEMBER";

            case 0x5d:
                return "KSET";

            case 0x5e:
                return "KSELECT";

            case 0x5f:
                return "KON";

            case 0x60:
                return "KFROM";

            case 0x61:
                return "KWHERE";

            case 0x62:
                return "KNON";

            case 0x63:
                return "KEMPTY";

            case 100:
                return "KHIDDEN";

            case 0x65:
                return "KHAVING";

            case 0x66:
                return "KDIMENSION";

            case 0x67:
                return "KCELL";

            case 0x68:
                return "KPROPERTIES";

            case 0x69:
                return "KCOUNT";

            case 0x6a:
                return "KCALCULATION";

            case 0x6b:
                return "KFOR";

            case 0x6c:
                return "KSESSION";

            case 0x6d:
                return "KDYNAMIC";

            case 110:
                return "KEXISTING";

            case 0x6f:
                return "KDISTINCT";

            case 0x70:
                return "KCASE";

            case 0x71:
                return "KWHEN";

            case 0x72:
                return "KTHEN";

            case 0x73:
                return "KELSE";

            case 0x74:
                return "KEND";

            case 0x75:
                return "KCALCULATE";

            case 0x76:
                return "KCREATE";

            case 0x77:
                return "KALTER";

            case 120:
                return "KCUBE";

            case 0x79:
                return "KSUBCUBE";

            case 0x7a:
                return "KUPDATE";

            case 0x7b:
                return "KSCOPE";

            case 0x7c:
                return "KFREEZE";

            case 0x7d:
                return "KIF";

            case 0x7e:
                return "KDRILLTHROUGH";

            case 0x7f:
                return "KMAXROWS";

            case 0x80:
                return "KFIRSTROWSET";

            case 0x81:
                return "KRETURN";

            case 130:
                return "KGO";

            case 0x83:
                return "TGProperty";
        }
        return "Token Not Found";
    }

    public List<string> Comments
    {
        get
        {
            return this.m_Comments;
        }
        set
        {
            this.m_Comments = value;
        }
    }
}

