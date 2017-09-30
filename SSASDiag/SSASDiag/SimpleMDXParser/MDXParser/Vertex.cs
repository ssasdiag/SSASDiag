namespace SimpleMDXParser
{
    //using Microsoft.NetMap.Core;
    using System;
    using System.Collections.Generic;

    public class Vertex
    {
        private List<Vertex> m_Incoming;
        private string m_Name;
        private MDXNode m_Node;
        private List<Vertex> m_Outgoing;
        private DependencyGraph m_parentGraph;
        //private Microsoft.NetMap.Core.Vertex m_VisualVertex;

        internal Vertex(string name, DependencyGraph g)
        {
            this.m_parentGraph = g;
            this.m_Name = name;
            this.m_Outgoing = new List<Vertex>();
            this.m_Incoming = new List<Vertex>();
        }

        internal List<Vertex> Incoming
        {
            get
            {
                return this.m_Incoming;
            }
        }

        public string Name
        {
            get
            {
                return this.m_Name;
            }
        }

        internal MDXNode Node
        {
            get
            {
                return this.m_Node;
            }
            set
            {
                this.m_Node = value;
            }
        }

        internal List<Vertex> Outgoing
        {
            get
            {
                return this.m_Outgoing;
            }
        }

        public DependencyGraph ParentGraph
        {
            get
            {
                return this.m_parentGraph;
            }
        }

        //internal Microsoft.NetMap.Core.Vertex VisualVertex
        //{
        //    get
        //    {
        //        return this.m_VisualVertex;
        //    }
        //    set
        //    {
        //        this.m_VisualVertex = value;
        //    }
        //}
    }
}

