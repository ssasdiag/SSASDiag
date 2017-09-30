namespace SimpleMDXParser
{
    using SSVParseLib;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;

    internal class MDXGrammarYaccClass : SSYacc
    {
        public bool m_Cancel;
        public bool m_ColorCodeOnly;
        internal ColorCoding m_ColorCoding;
        public CubeInfo m_CubeInfo;
        public MessageCollection m_Errors;
        public string m_MdxText;
        private MDXNode m_Node;
        public MDXParamParser m_ParamParser;
        internal Source m_Source;
        public Stopwatch m_StopWatch;
        public const int MDXGrammarYaccProdRAlter = 0x16;
        public const int MDXGrammarYaccProdRAlterStmt = 9;
        public const int MDXGrammarYaccProdRAndOp = 0x63;
        public const int MDXGrammarYaccProdRAssignment = 0x18;
        public const int MDXGrammarYaccProdRAssignmentStmt = 14;
        public const int MDXGrammarYaccProdRAxesEmpty = 0x2b;
        public const int MDXGrammarYaccProdRAxesList = 0x2c;
        public const int MDXGrammarYaccProdRAxesList1 = 0x2d;
        public const int MDXGrammarYaccProdRAxesListN = 0x2e;
        public const int MDXGrammarYaccProdRAxis = 0x2f;
        public const int MDXGrammarYaccProdRAxisAxis = 0x37;
        public const int MDXGrammarYaccProdRAxisName = 0x36;
        public const int MDXGrammarYaccProdRAxisNumber = 0x35;
        public const int MDXGrammarYaccProdRCalcProps = 0x2a;
        public const int MDXGrammarYaccProdRCalcProps0 = 0x29;
        public const int MDXGrammarYaccProdRCalculateStmt = 12;
        public const int MDXGrammarYaccProdRCellProps = 0x41;
        public const int MDXGrammarYaccProdRCellProps0 = 0x40;
        public const int MDXGrammarYaccProdRConst = 0x4d;
        public const int MDXGrammarYaccProdRConstFloat = 0x7d;
        public const int MDXGrammarYaccProdRConstInteger = 0x7c;
        public const int MDXGrammarYaccProdRCountFunc = 0x67;
        public const int MDXGrammarYaccProdRCountProperty = 0x6b;
        public const int MDXGrammarYaccProdRCreate = 0x13;
        public const int MDXGrammarYaccProdRCreateNew = 0x15;
        public const int MDXGrammarYaccProdRCreateNew0 = 20;
        public const int MDXGrammarYaccProdRCreateStmt = 8;
        public const int MDXGrammarYaccProdRCreateSubcube = 11;
        public const int MDXGrammarYaccProdRCreateSubcubeP = 0x1d;
        public const int MDXGrammarYaccProdRCreateSubcubeS = 0x1c;
        public const int MDXGrammarYaccProdRDimProps0 = 0x30;
        public const int MDXGrammarYaccProdRDimPropsDP = 0x31;
        public const int MDXGrammarYaccProdRDimPropsP = 50;
        public const int MDXGrammarYaccProdRDistinct = 0x55;
        public const int MDXGrammarYaccProdRDivOp = 0x59;
        public const int MDXGrammarYaccProdRDrillThrough = 10;
        public const int MDXGrammarYaccProdRDynamic = 40;
        public const int MDXGrammarYaccProdRElse0 = 0x75;
        public const int MDXGrammarYaccProdRElse1 = 0x76;
        public const int MDXGrammarYaccProdREnumSet = 110;
        public const int MDXGrammarYaccProdREQOp = 0x5e;
        public const int MDXGrammarYaccProdRExisting = 0x54;
        public const int MDXGrammarYaccProdRExpList1 = 0x79;
        public const int MDXGrammarYaccProdRExpListEmpty = 0x77;
        public const int MDXGrammarYaccProdRExpListN = 0x7a;
        public const int MDXGrammarYaccProdRExpListSkip = 0x7b;
        public const int MDXGrammarYaccProdRExpListSome = 120;
        public const int MDXGrammarYaccProdRExpOp = 90;
        public const int MDXGrammarYaccProdRFirstRowSet = 0x45;
        public const int MDXGrammarYaccProdRFirstRowSet0 = 0x44;
        public const int MDXGrammarYaccProdRFreeze0 = 0x1a;
        public const int MDXGrammarYaccProdRFreeze1 = 0x1b;
        public const int MDXGrammarYaccProdRFreezeStmt = 0x10;
        public const int MDXGrammarYaccProdRFromCube = 0x38;
        public const int MDXGrammarYaccProdRFromSelect = 0x39;
        public const int MDXGrammarYaccProdRFunction = 0x66;
        public const int MDXGrammarYaccProdRGEOp = 0x5f;
        public const int MDXGrammarYaccProdRGTOp = 0x60;
        public const int MDXGrammarYaccProdRHaving = 0x3d;
        public const int MDXGrammarYaccProdRHaving0 = 60;
        public const int MDXGrammarYaccProdRHidden = 0x34;
        public const int MDXGrammarYaccProdRId1 = 0x48;
        public const int MDXGrammarYaccProdRIdN = 0x49;
        public const int MDXGrammarYaccProdRIfStmt = 0x11;
        public const int MDXGrammarYaccProdRISOp = 0x62;
        public const int MDXGrammarYaccProdRLEOp = 0x5d;
        public const int MDXGrammarYaccProdRLTOp = 0x5c;
        public const int MDXGrammarYaccProdRMaxRows = 0x43;
        public const int MDXGrammarYaccProdRMaxRows0 = 0x42;
        public const int MDXGrammarYaccProdRMethod = 0x68;
        public const int MDXGrammarYaccProdRMinusOp = 0x57;
        public const int MDXGrammarYaccProdRMulOp = 0x58;
        public const int MDXGrammarYaccProdRNEOp = 0x61;
        public const int MDXGrammarYaccProdRNoHidden = 0x33;
        public const int MDXGrammarYaccProdRNonEmpty = 0x3b;
        public const int MDXGrammarYaccProdRNonEmpty0 = 0x3a;
        public const int MDXGrammarYaccProdRNotOp = 0x51;
        public const int MDXGrammarYaccProdRObject = 0x6c;
        public const int MDXGrammarYaccProdROrOp = 100;
        public const int MDXGrammarYaccProdRParameter = 80;
        public const int MDXGrammarYaccProdRParExp = 0x4a;
        public const int MDXGrammarYaccProdRPlusOp = 0x56;
        public const int MDXGrammarYaccProdRPropAssign = 0x19;
        public const int MDXGrammarYaccProdRPropAssignStmt = 15;
        public const int MDXGrammarYaccProdRProperties = 0x69;
        public const int MDXGrammarYaccProdRProperty = 0x6a;
        public const int MDXGrammarYaccProdRRangeOp = 0x5b;
        public const int MDXGrammarYaccProdRReturn = 0x47;
        public const int MDXGrammarYaccProdRReturn0 = 70;
        public const int MDXGrammarYaccProdRScope = 0x17;
        public const int MDXGrammarYaccProdRScopeStmt = 13;
        public const int MDXGrammarYaccProdRScript1 = 3;
        public const int MDXGrammarYaccProdRScriptGo = 6;
        public const int MDXGrammarYaccProdRScriptN = 4;
        public const int MDXGrammarYaccProdRScriptS = 5;
        public const int MDXGrammarYaccProdRSearchCase = 0x71;
        public const int MDXGrammarYaccProdRSelect = 0x12;
        public const int MDXGrammarYaccProdRSelectStmt = 7;
        public const int MDXGrammarYaccProdRSession = 0x27;
        public const int MDXGrammarYaccProdRSession0 = 0x26;
        public const int MDXGrammarYaccProdRSetAlias = 0x6f;
        public const int MDXGrammarYaccProdRSimpleCase = 0x70;
        public const int MDXGrammarYaccProdRStartExp = 1;
        public const int MDXGrammarYaccProdRStartScript = 2;
        public const int MDXGrammarYaccProdRString1 = 0x4e;
        public const int MDXGrammarYaccProdRString2 = 0x4f;
        public const int MDXGrammarYaccProdRSubselectStar = 0x4c;
        public const int MDXGrammarYaccProdRTuple = 0x4b;
        public const int MDXGrammarYaccProdRUnaryMinus = 0x52;
        public const int MDXGrammarYaccProdRUnaryPlus = 0x53;
        public const int MDXGrammarYaccProdRVBA = 0x6d;
        public const int MDXGrammarYaccProdRWhen = 0x74;
        public const int MDXGrammarYaccProdRWhenList1 = 0x72;
        public const int MDXGrammarYaccProdRWhenListN = 0x73;
        public const int MDXGrammarYaccProdRWhere = 0x3f;
        public const int MDXGrammarYaccProdRWhereEmpty = 0x3e;
        public const int MDXGrammarYaccProdRWithCellCalc = 0x25;
        public const int MDXGrammarYaccProdRWithEmpty = 30;
        public const int MDXGrammarYaccProdRWithList = 0x1f;
        public const int MDXGrammarYaccProdRWithList0 = 0x20;
        public const int MDXGrammarYaccProdRWithListN = 0x21;
        public const int MDXGrammarYaccProdRWithMember = 0x22;
        public const int MDXGrammarYaccProdRWithSet = 0x23;
        public const int MDXGrammarYaccProdRWithSetNew = 0x24;
        public const int MDXGrammarYaccProdRXorOp = 0x65;

        public MDXGrammarYaccClass(MDXParamParser paramparser, SSYaccTable q_table, SSLex q_lex) : base(q_table, q_lex)
        {
            this.m_ParamParser = paramparser;
            this.m_Errors = new MessageCollection();
            this.m_StopWatch = new Stopwatch();
            this.m_Cancel = false;
        }

        private MDXExpNode ChildExpNode(int in_iChild)
        {
            return (MDXExpNode) this.ChildNode(in_iChild);
        }

        private MDXNode ChildNode(int in_iChild)
        {
            return ((MDXStackElem) base.elementFromProduction(in_iChild)).GetNode();
        }

        private SSYaccStackElement ColorCodeReduce(int q_prod, int q_size)
        {
            switch (q_prod)
            {
                case 0x68:
                {
                    MDXObject obj3;
                    string key = this.LexChildString(2);
                    if (MDXParserObjects.s_ObjectsMap.TryGetValue(key, out obj3) && ((obj3.SyntaxForm == MDXSyntaxForm.Method) || ((obj3.SyntaxForm == MDXSyntaxForm.Property) && (obj3.ReturnType == MDXDataType.Set))))
                    {
                        this.ColorKeyword(2);
                    }
                    break;
                }
                case 0x6a:
                {
                    MDXObject obj2;
                    string str = this.LexChildString(2);
                    if (!MDXParserObjects.s_ObjectsMap.TryGetValue(str, out obj2) || (obj2.SyntaxForm != MDXSyntaxForm.Property))
                    {
                        if (str.StartsWith("[") && str.EndsWith("]"))
                        {
                            this.ColorSome(2, Color.Maroon);
                        }
                        break;
                    }
                    this.ColorKeyword(2);
                    break;
                }
                case 0x6c:
                {
                    string flag = this.LexChildString(0);
                    if (!MDXParserObjects.IsFlag(flag))
                    {
                        if (flag.Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.ColorKeyword(0);
                        }
                        else if (flag.StartsWith("[") && flag.EndsWith("]"))
                        {
                            this.ColorSome(0, Color.Maroon);
                        }
                        break;
                    }
                    this.ColorKeyword(0);
                    break;
                }
                case 0x38:
                    this.ColorSome(0, Color.Maroon);
                    break;
            }
            return this.stackElement();
        }

        private void ColorKeyword(int i)
        {
            SSLexLexeme lexeme = base.elementFromProduction(i).lexeme();
            this.m_ColorCoding.ApplyColor(lexeme, Color.Blue);
        }

        private void ColorSome(int i, Color color)
        {
            SSLexLexeme lexeme = base.elementFromProduction(i).lexeme();
            this.m_ColorCoding.ApplyColor(lexeme, color);
        }

        public override bool error(int State, SSLexLexeme LookaheadToken)
        {
            Message item = new Message(LookaheadToken) {
                Type = MessageType.Error,
                Source = this.m_Source
            };
            item.Location.Adjust(item.Source.StartLocation);
            if (-1 == LookaheadToken.token())
            {
                item.Text = "End of data reached";
            }
            else
            {
                item.Text = "Syntax error while parsing '" + new string(LookaheadToken.lexeme()) + "'";
                this.m_Source.DrawWigglyLine((LookaheadToken.index() - LookaheadToken.length()) - 1, LookaheadToken.length());
            }
            this.m_Errors.Add(item);
            return base.syncErr();
        }

        public string GetMdxText()
        {
            return this.m_MdxText;
        }

        public MDXNode GetNode()
        {
            return this.m_Node;
        }

        private string LexChildString(int in_iChild)
        {
            return new string(base.elementFromProduction(in_iChild).lexeme().lexeme());
        }

        public override SSYaccStackElement reduce(int q_prod, int q_size)
        {
            string canonicalName;
            MDXObject obj2;
            bool flag;
            if (this.m_Cancel)
            {
                base.doError();
                return null;
            }
            if (this.m_ColorCodeOnly)
            {
                return this.ColorCodeReduce(q_prod, q_size);
            }
            MDXStackElem elem = (MDXStackElem) this.stackElement();
            switch (q_prod)
            {
                case 1:
                    this.m_Node = this.ChildNode(1);
                    return elem;

                case 2:
                    this.m_Node = this.ChildNode(0);
                    return elem;

                case 3:
                {
                    MDXScriptNode node64 = new MDXScriptNode();
                    MDXStatementNode stmt = (MDXStatementNode) this.ChildNode(0);
                    node64.Add(stmt);
                    elem.SetNode(node64);
                    return elem;
                }
                case 4:
                case 6:
                {
                    MDXScriptNode node66 = (MDXScriptNode) this.ChildNode(0);
                    MDXStatementNode node67 = (MDXStatementNode) this.ChildNode(2);
                    if (q_prod == 6)
                    {
                        node66.Add(new MDXGoNode());
                    }
                    node66.Add(node67);
                    elem.SetNode(node66);
                    return elem;
                }
                case 5:
                    elem.SetNode(this.ChildNode(0));
                    return elem;

                case 7:
                    elem.SetNode(this.ChildNode(0));
                    this.SetSource(elem.GetNode(), 1);
                    return elem;

                case 8:
                    elem.SetNode(this.ChildNode(0));
                    this.SetSource(elem.GetNode(), 1);
                    return elem;

                case 9:
                    elem.SetNode(null);
                    return elem;

                case 10:
                {
                    this.ColorKeyword(0);
                    MDXSelectNode select = this.ChildNode(3) as MDXSelectNode;
                    MDXExpListNode returnattrs = this.ChildNode(4) as MDXExpListNode;
                    MDXDrillThroughNode node81 = new MDXDrillThroughNode(select, returnattrs);
                    this.SetSource(node81, 5);
                    elem.SetNode(node81);
                    return elem;
                }
                case 11:
                    elem.SetNode(this.ChildNode(0));
                    return elem;

                case 12:
                {
                    this.ColorKeyword(0);
                    MDXCalculateNode node76 = new MDXCalculateNode();
                    this.SetSource(node76, 1);
                    elem.SetNode(node76);
                    return elem;
                }
                case 13:
                    elem.SetNode(this.ChildNode(0));
                    this.SetSource(elem.GetNode(), 1);
                    return elem;

                case 14:
                    elem.SetNode(this.ChildNode(0));
                    this.SetSource(elem.GetNode(), 1);
                    return elem;

                case 15:
                    elem.SetNode(this.ChildNode(0));
                    this.SetSource(elem.GetNode(), 1);
                    return elem;

                case 0x10:
                {
                    MDXFreezeNode node95 = new MDXFreezeNode();
                    elem.SetNode(node95);
                    this.SetSource(elem.GetNode(), 1);
                    return elem;
                }
                case 0x11:
                    this.ColorKeyword(0);
                    this.ColorKeyword(2);
                    this.ColorKeyword(4);
                    this.ColorKeyword(5);
                    elem.SetNode(null);
                    return elem;

                case 0x12:
                {
                    this.ColorKeyword(1);
                    this.ColorKeyword(3);
                    MDXWithListNode withs = (MDXWithListNode) this.ChildNode(0);
                    MDXAxesListNode axes = (MDXAxesListNode) this.ChildNode(2);
                    MDXWhereNode where = (MDXWhereNode) this.ChildNode(5);
                    MDXSelectNode subselect = null;
                    MDXStringConstNode cube = null;
                    if (this.ChildNode(4).GetType() != typeof(MDXSelectNode))
                    {
                        cube = this.ChildNode(4) as MDXStringConstNode;
                    }
                    else
                    {
                        subselect = this.ChildNode(4) as MDXSelectNode;
                    }
                    MDXExpListNode cellprops = this.ChildNode(6) as MDXExpListNode;
                    MDXSelectNode node42 = new MDXSelectNode(withs, axes, where, subselect, cube, cellprops);
                    this.SetSource(node42, 7);
                    elem.SetNode(node42);
                    return elem;
                }
                case 0x13:
                {
                    this.ColorKeyword(0);
                    MDXWithListNode node77 = this.ChildNode(2) as MDXWithListNode;
                    MDXCreateNode node78 = new MDXCreateNode(node77, this.ChildNode(1) != null);
                    this.SetSource(node78, 3);
                    elem.SetNode(node78);
                    return elem;
                }
                case 20:
                {
                    this.ColorKeyword(0);
                    MDXIDNode name = this.ChildNode(2) as MDXIDNode;
                    MDXIDNode exp = new MDXIDNode("NULL");
                    MDXWithMemberNode t = new MDXWithMemberNode(name, exp, null, true);
                    MDXWithListNode node107 = new MDXWithListNode();
                    node107.Add(t);
                    MDXCreateNode node108 = new MDXCreateNode(node107, this.ChildNode(1) != null);
                    elem.SetNode(node108);
                    this.SetSource(elem.GetNode(), 3);
                    return elem;
                }
                case 0x15:
                {
                    this.ColorKeyword(0);
                    MDXWithMemberNode node101 = new MDXWithMemberNode(this.ChildExpNode(2), this.ChildExpNode(4), null, true);
                    MDXWithListNode node102 = new MDXWithListNode();
                    node102.Add(node101);
                    MDXCreateNode node103 = new MDXCreateNode(node102, this.ChildNode(1) != null);
                    elem.SetNode(node103);
                    this.SetSource(elem.GetNode(), 5);
                    return elem;
                }
                case 0x16:
                    this.ColorKeyword(0);
                    this.ColorKeyword(1);
                    this.ColorKeyword(3);
                    this.ColorKeyword(4);
                    return elem;

                case 0x17:
                {
                    this.ColorKeyword(0);
                    this.ColorKeyword(4);
                    this.ColorKeyword(5);
                    MDXScriptNode script = this.ChildNode(3) as MDXScriptNode;
                    MDXEndScopeNode node97 = new MDXEndScopeNode();
                    this.SetSource(node97, 4, 5);
                    MDXScopeNode node98 = new MDXScopeNode(this.ChildExpNode(1), script, node97);
                    elem.SetNode(node98);
                    this.SetSource(elem.GetNode(), 6);
                    return elem;
                }
                case 0x18:
                {
                    MDXAssignmentNode node99 = new MDXAssignmentNode(null, this.ChildExpNode(0), this.ChildExpNode(2));
                    elem.SetNode(node99);
                    this.SetSource(elem.GetNode(), 3);
                    return elem;
                }
                case 0x19:
                {
                    MDXAssignmentNode node100 = new MDXAssignmentNode(this.LexChildString(0), this.ChildExpNode(2), this.ChildExpNode(5));
                    elem.SetNode(node100);
                    this.SetSource(elem.GetNode(), 6);
                    return elem;
                }
                case 0x1a:
                    this.ColorKeyword(0);
                    return elem;

                case 0x1b:
                    this.ColorKeyword(0);
                    return elem;

                case 0x1c:
                {
                    this.ColorKeyword(0);
                    this.ColorKeyword(1);
                    this.ColorKeyword(3);
                    string str10 = this.LexChildString(2);
                    MDXSelectNode node82 = this.ChildNode(4) as MDXSelectNode;
                    MDXCreateSubcubeNode node83 = new MDXCreateSubcubeNode(str10, node82);
                    this.SetSource(node83, 5);
                    elem.SetNode(node83);
                    return elem;
                }
                case 0x1d:
                {
                    this.ColorKeyword(0);
                    this.ColorKeyword(1);
                    this.ColorKeyword(3);
                    string str11 = this.LexChildString(2);
                    MDXSelectNode node84 = this.ChildNode(5) as MDXSelectNode;
                    MDXCreateSubcubeNode node85 = new MDXCreateSubcubeNode(str11, node84);
                    this.SetSource(node85, 7);
                    elem.SetNode(node85);
                    return elem;
                }
                case 30:
                {
                    MDXWithListNode node43 = new MDXWithListNode();
                    this.SetSource(node43, 0);
                    elem.SetNode(node43);
                    return elem;
                }
                case 0x1f:
                {
                    this.ColorKeyword(0);
                    MDXWithListNode node44 = (MDXWithListNode) this.ChildNode(1);
                    this.SetSource(node44, 2);
                    elem.SetNode(node44);
                    return elem;
                }
                case 0x20:
                {
                    MDXWithListNode node45 = new MDXWithListNode();
                    elem.SetNode(node45);
                    return elem;
                }
                case 0x21:
                {
                    MDXWithListNode node46 = (MDXWithListNode) this.ChildNode(0);
                    MDXWithNode node47 = (MDXWithNode) this.ChildNode(1);
                    node46.Add(node47);
                    this.SetSource(node46, 0, 1);
                    elem.SetNode(node46);
                    return elem;
                }
                case 0x22:
                {
                    this.ColorKeyword(0);
                    this.ColorKeyword(2);
                    MDXListNode<MDXCalcPropNode> calcprops = this.ChildNode(4) as MDXListNode<MDXCalcPropNode>;
                    MDXWithMemberNode node49 = new MDXWithMemberNode(this.ChildExpNode(1), this.ChildExpNode(3), calcprops, false);
                    this.SetSource(node49, 5);
                    node49.HandleSingleQuotes();
                    elem.SetNode(node49);
                    return elem;
                }
                case 0x23:
                case 0x24:
                {
                    this.ColorKeyword(1);
                    this.ColorKeyword(3);
                    MDXWithSetNode node50 = new MDXWithSetNode(this.ChildExpNode(2), this.ChildExpNode(4), null, 0x24 == q_prod);
                    this.SetSource(node50, 5);
                    node50.HandleSingleQuotes();
                    elem.SetNode(node50);
                    return elem;
                }
                case 0x25:
                {
                    this.ColorKeyword(0);
                    this.ColorKeyword(1);
                    this.ColorKeyword(3);
                    this.ColorKeyword(5);
                    string scope = this.LexChildString(4);
                    MDXWithCalcCellNode node51 = new MDXWithCalcCellNode(this.ChildExpNode(2), scope, this.ChildExpNode(6), null);
                    this.SetSource(node51, 8);
                    elem.SetNode(node51);
                    return elem;
                }
                case 0x26:
                case 0x30:
                case 0x33:
                case 0x3a:
                case 60:
                case 0x40:
                case 0x42:
                case 0x44:
                case 70:
                    return elem;

                case 0x27:
                    this.ColorKeyword(0);
                    return elem;

                case 40:
                    this.ColorKeyword(0);
                    return elem;

                case 0x29:
                {
                    MDXListNode<MDXCalcPropNode> node68 = new MDXListNode<MDXCalcPropNode>();
                    elem.SetNode(node68);
                    return elem;
                }
                case 0x2a:
                {
                    MDXExpNode node69 = this.ChildExpNode(4);
                    MDXCalcPropNode node70 = new MDXCalcPropNode(this.LexChildString(2), node69);
                    this.SetSource(node70, 2, 4);
                    MDXListNode<MDXCalcPropNode> node71 = this.ChildNode(0) as MDXListNode<MDXCalcPropNode>;
                    node71.Add(node70);
                    this.SetSource(node71, 5);
                    elem.SetNode(node71);
                    return elem;
                }
                case 0x2b:
                {
                    MDXAxesListNode node52 = new MDXAxesListNode();
                    elem.SetNode(node52);
                    return elem;
                }
                case 0x2c:
                    elem.SetNode(this.ChildNode(0));
                    this.SetSource(elem.GetNode(), 1);
                    return elem;

                case 0x2d:
                {
                    MDXAxesListNode node53 = new MDXAxesListNode();
                    MDXAxisNode node54 = (MDXAxisNode) this.ChildNode(0);
                    node53.Add(node54);
                    this.SetSource(node53, 1);
                    elem.SetNode(node53);
                    return elem;
                }
                case 0x2e:
                {
                    MDXAxesListNode node55 = (MDXAxesListNode) this.ChildNode(0);
                    MDXAxisNode node56 = (MDXAxisNode) this.ChildNode(2);
                    node55.Add(node56);
                    this.SetSource(node55, 3);
                    elem.SetNode(node55);
                    return elem;
                }
                case 0x2f:
                {
                    this.ColorKeyword(4);
                    MDXExpNode set = this.ChildExpNode(1);
                    MDXNonEmptyNode node58 = null;
                    if (this.ChildNode(0) != null)
                    {
                        node58 = this.ChildNode(0) as MDXNonEmptyNode;
                        this.SetSource(node58, 2);
                        node58.SetSet(set);
                        set = node58;
                    }
                    MDXExpListNode dimprops = this.ChildNode(3) as MDXExpListNode;
                    MDXStringConstNode node60 = (MDXStringConstNode) this.ChildNode(5);
                    MDXAxisNode node61 = new MDXAxisNode(node60.GetString(), set, dimprops, this.ChildExpNode(2));
                    this.SetSource(node61, 6);
                    elem.SetNode(node61);
                    return elem;
                }
                case 0x31:
                    this.ColorKeyword(0);
                    this.ColorKeyword(1);
                    elem.SetNode(this.ChildNode(2));
                    return elem;

                case 50:
                    this.ColorKeyword(0);
                    elem.SetNode(this.ChildNode(1));
                    return elem;

                case 0x34:
                {
                    this.ColorKeyword(0);
                    MDXEmptyNode node109 = new MDXEmptyNode();
                    elem.SetNode(node109);
                    return elem;
                }
                case 0x35:
                {
                    MDXStringConstNode node72 = new MDXStringConstNode(this.LexChildString(0));
                    this.SetSource(node72, 1);
                    elem.SetNode(node72);
                    return elem;
                }
                case 0x36:
                {
                    MDXStringConstNode node73 = new MDXStringConstNode(this.LexChildString(0));
                    this.SetSource(node73, 1);
                    elem.SetNode(node73);
                    return elem;
                }
                case 0x37:
                {
                    MDXStringConstNode node74 = new MDXStringConstNode(this.LexChildString(2));
                    this.SetSource(node74, 4);
                    elem.SetNode(node74);
                    return elem;
                }
                case 0x38:
                {
                    MDXStringConstNode node62 = new MDXStringConstNode(this.LexChildString(0));
                    this.SetSource(node62, 1);
                    elem.SetNode(node62);
                    return elem;
                }
                case 0x39:
                    elem.SetNode(this.ChildNode(1));
                    return elem;

                case 0x3b:
                {
                    this.ColorKeyword(0);
                    this.ColorKeyword(1);
                    MDXNonEmptyNode node111 = new MDXNonEmptyNode();
                    elem.SetNode(node111);
                    return elem;
                }
                case 0x3d:
                    this.ColorKeyword(0);
                    elem.SetNode(this.ChildExpNode(1));
                    return elem;

                case 0x3e:
                    elem.SetNode(null);
                    return elem;

                case 0x3f:
                {
                    this.ColorKeyword(0);
                    MDXWhereNode node63 = new MDXWhereNode();
                    node63.Set(this.ChildExpNode(1));
                    this.SetSource(node63, 2);
                    elem.SetNode(node63);
                    return elem;
                }
                case 0x41:
                    this.ColorKeyword(0);
                    this.ColorKeyword(1);
                    elem.SetNode(this.ChildNode(2));
                    return elem;

                case 0x43:
                    this.ColorKeyword(0);
                    return elem;

                case 0x45:
                    this.ColorKeyword(0);
                    return elem;

                case 0x47:
                    elem.SetNode(this.ChildNode(1));
                    return elem;

                case 0x48:
                {
                    MDXIDNode node114 = new MDXIDNode(this.LexChildString(0));
                    elem.SetNode(node114);
                    this.SetSource(elem.GetNode(), 1);
                    return elem;
                }
                case 0x49:
                {
                    string term = this.LexChildString(2);
                    MDXNode node115 = this.ChildNode(0);
                    node115.CheckType(typeof(MDXIDNode));
                    MDXIDNode node116 = (MDXIDNode) node115;
                    node116.AppendName(term);
                    elem.SetNode(node116);
                    this.SetSource(elem.GetNode(), 3);
                    return elem;
                }
                case 0x4a:
                    elem.SetNode(this.ChildNode(1));
                    this.SetSource(elem.GetNode(), 3);
                    return elem;

                case 0x4b:
                {
                    MDXExpListNode list = (MDXExpListNode) this.ChildNode(1);
                    MDXTupleNode node29 = new MDXTupleNode(list);
                    this.SetSource(node29, 3);
                    elem.SetNode(node29);
                    return elem;
                }
                case 0x4c:
                {
                    MDXExpListNode node30 = (MDXExpListNode) this.ChildNode(1);
                    MDXTupleNode node31 = new MDXTupleNode(node30);
                    this.SetSource(node31, 5);
                    elem.SetNode(node31);
                    return elem;
                }
                case 0x4d:
                    elem.SetNode(this.ChildNode(0));
                    this.SetSource(elem.GetNode(), 1);
                    return elem;

                case 0x4e:
                case 0x4f:
                {
                    string str = this.LexChildString(0).Remove(0, 1);
                    MDXStringConstNode node = new MDXStringConstNode(str.Remove(str.Length - 1, 1)) {
                        SingleQuote = q_prod == 0x4e
                    };
                    elem.SetNode(node);
                    this.SetSource(elem.GetNode(), 1);
                    return elem;
                }
                case 80:
                {
                    MDXParamNode node110 = new MDXParamNode(this.LexChildString(0));
                    elem.SetNode(node110);
                    this.SetSource(elem.GetNode(), 1);
                    return elem;
                }
                case 0x51:
                case 0x52:
                case 0x53:
                case 0x54:
                case 0x55:
                {
                    MDXExpNode node2 = this.ChildExpNode(1);
                    MDXUnaryOpNode node3 = new MDXUnaryOpNode(this.LexChildString(0), node2);
                    elem.SetNode(node3);
                    this.SetSource(elem.GetNode(), 2);
                    return elem;
                }
                case 0x56:
                case 0x57:
                case 0x58:
                case 0x59:
                case 90:
                case 0x5b:
                case 0x5c:
                case 0x5d:
                case 0x5e:
                case 0x5f:
                case 0x60:
                case 0x61:
                case 0x62:
                case 0x63:
                case 100:
                case 0x65:
                {
                    MDXExpNode node4 = this.ChildExpNode(0);
                    MDXExpNode node5 = this.ChildExpNode(2);
                    MDXBinOpNode node6 = new MDXBinOpNode(this.LexChildString(1), node4, node5);
                    this.SetSource(node6, 3);
                    elem.SetNode(node6);
                    return elem;
                }
                case 0x66:
                case 0x67:
                    this.ColorSome(0, Color.Maroon);
                    //this.ColorKeyword(0);
                    canonicalName = this.LexChildString(0);
                    flag = MDXParserObjects.s_ObjectsMap.TryGetValue(canonicalName, out obj2);
                    if (!flag)
                    {
                        canonicalName = StringUtil.CamelCase(canonicalName);
                        break;
                    }
                    canonicalName = obj2.CanonicalName;
                    break;

                case 0x68:
                case 0x69:
                {
                    MDXObject obj4;
                    MDXExpNode node14 = this.ChildExpNode(0);
                    string key = this.LexChildString(2);
                    MDXExpListNode node15 = (MDXExpListNode) this.ChildNode(4);
                    if (!MDXParserObjects.s_ObjectsMap.TryGetValue(key, out obj4) || (((obj4.SyntaxForm != MDXSyntaxForm.Method) && ((obj4.SyntaxForm != MDXSyntaxForm.Property) || (obj4.ReturnType != MDXDataType.Set))) && ((obj4.SyntaxForm != MDXSyntaxForm.Property) || (obj4.ReturnType != MDXDataType.Level))))
                    {
                        if (node14.GetType() != typeof(MDXIDNode))
                        {
                            Message m = new Message(base.elementFromProduction(2).lexeme()) {
                                Text = string.Format("Unrecognized MDX method '{0}'", key)
                            };
                            throw new MDXParserException(m);
                        }
                        key = StringUtil.CamelCase(key);
                        MDXIDNode node17 = node14 as MDXIDNode;
                        MDXFunctionNode node18 = new MDXFunctionNode(string.Format("{0}.{1}", node17.GetLabel(), key), node15, null, false);
                        this.SetSource(node18, 6);
                        elem.SetNode(node18);
                        return elem;
                    }
                    node15.Insert(0, node14);
                    MDXPropertyNode node16 = new MDXPropertyNode(obj4.CanonicalName, node15, obj4);
                    this.SetSource(node16, 6);
                    elem.SetNode(node16);
                    return elem;
                }
                case 0x6a:
                case 0x6b:
                {
                    MDXObject obj3;
                    string str3 = this.LexChildString(2);
                    MDXExpNode node10 = this.ChildExpNode(0);
                    if (!MDXParserObjects.s_ObjectsMap.TryGetValue(str3, out obj3) || (obj3.SyntaxForm != MDXSyntaxForm.Property))
                    {
                        node10.CheckType(typeof(MDXIDNode));
                        MDXIDNode node13 = (MDXIDNode) node10;
                        node13.AppendName(str3);
                        MDXDataType dt = this.m_CubeInfo.DetermineType(node13.GetMDX(-1));
                        node13.SetMDXType(dt);
                        elem.SetNode(node13);
                        this.SetSource(node13, 3);
                        return elem;
                    }
                    this.ColorKeyword(2);
                    MDXExpListNode args = new MDXExpListNode();
                    args.Add(node10);
                    MDXPropertyNode node12 = new MDXPropertyNode(obj3.CanonicalName, args, obj3);
                    elem.SetNode(node12);
                    this.SetSource(node12, 3);
                    SSLexLexeme lexeme = base.elementFromProduction(2).lexeme();
                    node12.Locator.Line = lexeme.line() + 1;
                    node12.Locator.Column = lexeme.offset() + 1;
                    return elem;
                }
                case 0x6c:
                {
                    string str6 = this.LexChildString(0);
                    if (!MDXParserObjects.IsFlag(str6))
                    {
                        MDXIDNode node20 = new MDXIDNode(str6);
                        elem.SetNode(node20);
                    }
                    else
                    {
                        this.ColorKeyword(0);
                        MDXFlagNode node19 = new MDXFlagNode(str6.ToUpper());
                        elem.SetNode(node19);
                    }
                    this.SetSource(elem.GetNode(), 1);
                    return elem;
                }
                case 0x6d:
                {
                    string str7 = this.LexChildString(0).ToUpper();
                    string str8 = StringUtil.CamelCase(this.LexChildString(2));
                    MDXExpListNode node32 = (MDXExpListNode) this.ChildNode(4);
                    MDXFunctionNode node33 = new MDXFunctionNode(string.Format("{0}!{1}", str7, str8), node32, null, false);
                    this.SetSource(node33, 6);
                    elem.SetNode(node33);
                    return elem;
                }
                case 110:
                {
                    MDXExpListNode node34 = (MDXExpListNode) this.ChildNode(1);
                    MDXEnumSetNode node35 = new MDXEnumSetNode(node34);
                    this.SetSource(node35, 3);
                    elem.SetNode(node35);
                    return elem;
                }
                case 0x6f:
                {
                    this.ColorKeyword(1);
                    MDXExpNode node112 = this.ChildExpNode(0);
                    string alias = this.LexChildString(2);
                    MDXAliasNode node113 = new MDXAliasNode(node112, alias);
                    elem.SetNode(node113);
                    this.SetSource(node113, 3);
                    return elem;
                }
                case 0x70:
                {
                    this.ColorKeyword(0);
                    this.ColorKeyword(4);
                    MDXEmptyNode node86 = new MDXEmptyNode();
                    elem.SetNode(node86);
                    return elem;
                }
                case 0x71:
                {
                    this.ColorKeyword(0);
                    this.ColorKeyword(3);
                    MDXListNode<MDXWhenNode> whenlist = this.ChildNode(1) as MDXListNode<MDXWhenNode>;
                    MDXExpNode elseexp = this.ChildExpNode(2);
                    MDXCaseNode node89 = new MDXCaseNode(whenlist, elseexp);
                    elem.SetNode(node89);
                    this.SetSource(elem.GetNode(), 4);
                    return elem;
                }
                case 0x72:
                {
                    MDXListNode<MDXWhenNode> node90 = new MDXListNode<MDXWhenNode>();
                    MDXWhenNode node91 = this.ChildNode(0) as MDXWhenNode;
                    node90.Add(node91);
                    elem.SetNode(node90);
                    this.SetSource(elem.GetNode(), 1);
                    return elem;
                }
                case 0x73:
                {
                    MDXListNode<MDXWhenNode> node92 = this.ChildNode(0) as MDXListNode<MDXWhenNode>;
                    MDXWhenNode node93 = this.ChildNode(1) as MDXWhenNode;
                    node92.Add(node93);
                    elem.SetNode(node92);
                    this.SetSource(elem.GetNode(), 2);
                    return elem;
                }
                case 0x74:
                {
                    this.ColorKeyword(0);
                    this.ColorKeyword(2);
                    MDXWhenNode node94 = new MDXWhenNode(this.ChildExpNode(1), this.ChildExpNode(3));
                    elem.SetNode(node94);
                    this.SetSource(elem.GetNode(), 4);
                    return elem;
                }
                case 0x75:
                    elem.SetNode(null);
                    return elem;

                case 0x76:
                    this.ColorKeyword(0);
                    elem.SetNode(this.ChildNode(1));
                    return elem;

                case 0x77:
                {
                    MDXExpListNode node21 = new MDXExpListNode();
                    elem.SetNode(node21);
                    return elem;
                }
                case 120:
                    elem.SetNode(this.ChildNode(0));
                    this.SetSource(elem.GetNode(), 1);
                    return elem;

                case 0x79:
                {
                    MDXExpListNode node22 = new MDXExpListNode();
                    MDXExpNode node23 = this.ChildExpNode(0);
                    node22.Add(node23);
                    this.SetSource(node22, 1);
                    elem.SetNode(node22);
                    return elem;
                }
                case 0x7a:
                {
                    MDXExpListNode node24 = (MDXExpListNode) this.ChildNode(0);
                    MDXExpNode node25 = this.ChildExpNode(2);
                    node24.Add(node25);
                    this.SetSource(node24, 3);
                    elem.SetNode(node24);
                    return elem;
                }
                case 0x7b:
                {
                    MDXExpListNode node26 = (MDXExpListNode) this.ChildNode(0);
                    MDXExpNode node27 = this.ChildExpNode(3);
                    node26.Add(new MDXEmptyNode());
                    node26.Add(node27);
                    this.SetSource(node26, 4);
                    elem.SetNode(node26);
                    return elem;
                }
                case 0x7c:
                {
                    MDXIntegerConstNode node7 = new MDXIntegerConstNode(this.LexChildString(0));
                    elem.SetNode(node7);
                    this.SetSource(elem.GetNode(), 1);
                    return elem;
                }
                case 0x7d:
                {
                    MDXFloatConstNode node75 = new MDXFloatConstNode(this.LexChildString(0));
                    this.SetSource(node75, 1);
                    elem.SetNode(node75);
                    return elem;
                }
                default:
                    return elem;
            }
            MDXExpListNode expList = (MDXExpListNode) this.ChildNode(2);
            MDXFunctionNode node9 = new MDXFunctionNode(canonicalName, expList, obj2, flag);
            this.SetSource(node9, 4);
            elem.SetNode(node9);
            return elem;
        }

        public void SetMdxText(string mdx)
        {
            this.m_MdxText = mdx;
        }

        public void SetMetadataInfo(CubeInfo cb)
        {
            this.m_CubeInfo = cb;
        }

        private void SetSource(MDXNode node, int count)
        {
            SSLex lex = base.m_lex;
            this.SetSource(node, 0, count - 1);
        }

        private void SetSource(MDXNode node, int start, int end)
        {
            try
            {
                int line = -1;
                int column = -1;
                int position = -1;
                int num4 = -1;
                int num5 = start;
                int num6 = end;
                while (num5 <= num6)
                {
                    SSLexLexeme lexeme = base.elementFromProduction(num5).lexeme();
                    if (lexeme != null)
                    {
                        line = lexeme.line() + 1;
                        column = lexeme.offset() + 1;
                        position = (lexeme.index() - lexeme.length()) - 1;
                    }
                    else
                    {
                        MDXNode node2 = this.ChildNode(num5);
                        if ((node2 != null) && (node2.Locator.Length != 0))
                        {
                            position = node2.Locator.Position;
                            line = node2.Locator.Line;
                            column = node2.Locator.Column;
                        }
                    }
                    if (position != -1)
                    {
                        break;
                    }
                    num5++;
                }
                node.Locator.Line = line;
                node.Locator.Column = column;
                while (num6 >= num5)
                {
                    SSLexLexeme lexeme2 = base.elementFromProduction(num6).lexeme();
                    if (lexeme2 != null)
                    {
                        num4 = lexeme2.index() - 1;
                    }
                    else
                    {
                        MDXNode node3 = this.ChildNode(num6);
                        if ((node3 != null) && (node3.Locator.Length != 0))
                        {
                            num4 = node3.Locator.Position + node3.Locator.Length;
                        }
                    }
                    if (num4 != -1)
                    {
                        break;
                    }
                    num6--;
                }
                for (num5 = start; num5 <= end; num5++)
                {
                    SSLexLexeme lexeme3 = base.elementFromProduction(num5).lexeme();
                    if ((lexeme3 != null) && (lexeme3.Comments != null))
                    {
                        if (node.Comments == null)
                        {
                            node.Comments = new List<string>();
                        }
                        foreach (string str in lexeme3.Comments)
                        {
                            node.Comments.Add(str);
                        }
                        lexeme3.Comments.Clear();
                    }
                }
                if ((position != -1) && (num4 != -1))
                {
                    node.Locator.Position = position;
                    node.Locator.Length = num4 - position;
                    node.Source = this.m_Source;
                }
            }
            catch (Exception)
            {
            }
        }

        public override SSYaccStackElement stackElement()
        {
            return new MDXStackElem();
        }
    }
}

