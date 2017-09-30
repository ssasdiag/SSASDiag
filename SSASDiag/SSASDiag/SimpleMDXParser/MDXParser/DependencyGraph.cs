namespace SimpleMDXParser
{
    //using Microsoft.NetMap.ApplicationUtil;
    //using Microsoft.NetMap.Core;
    //using Microsoft.NetMap.Visualization;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;

    public class DependencyGraph
    {
        private Vertex m_Root;
        private Dictionary<string, Vertex> m_Vertices = new Dictionary<string, Vertex>();

        internal void AddEdge(Vertex from, Vertex to)
        {
            from.Outgoing.Add(to);
            to.Incoming.Add(from);
        }

        public Vertex AddVertex(MDXNode node, string name)
        {
            Vertex vertex;
            name = this.NormalizeName(name);
            string key = name.ToLower();
            if (!this.m_Vertices.TryGetValue(key, out vertex))
            {
                if (this.m_Vertices.Count == 0)
                {
                    this.m_Root = vertex;
                }
                vertex = new Vertex(name, this)
                {
                    Node = node
                };
                this.m_Vertices.Add(key, vertex);
            }
            return vertex;
        }

        internal void FindLoops(Analyzer analyzer)
        {
            foreach (Vertex vertex in this.m_Vertices.Values)
            {
                bool flag = false;
                foreach (Vertex vertex2 in this.GetOutgoingTransitiveClosure(vertex))
                {
                    foreach (Vertex vertex3 in vertex2.Outgoing)
                    {
                        if (vertex3.Name.Equals(vertex.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        break;
                    }
                }
                if (flag)
                {
                    Message m = new Message(vertex.Node)
                    {
                        Severity = 3,
                        Text = string.Format("Calculation {0} recursively refers to itself. Recursion disables block optimization mode", vertex.Name),
                        Id = 0x2d
                    };
                    analyzer.Add(m);
                }
            }
        }

        private ICollection<Vertex> GetIncomingTransitiveClosure(Vertex root)
        {
            bool flag;
            Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
            dictionary.Add(root.Name.ToLower(), false);
            do
            {
                flag = true;
                string[] array = new string[dictionary.Keys.Count];
                dictionary.Keys.CopyTo(array, 0);
                foreach (string str in array)
                {
                    if (!dictionary[str])
                    {
                        Vertex vertex = this.m_Vertices[str];
                        foreach (Vertex vertex2 in vertex.Incoming)
                        {
                            if (!dictionary.ContainsKey(vertex2.Name.ToLower()))
                            {
                                dictionary.Add(vertex2.Name.ToLower(), false);
                                flag = false;
                            }
                        }
                        dictionary[str] = true;
                        flag = false;
                    }
                }
            }
            while (!flag);
            List<Vertex> list = new List<Vertex>();
            foreach (string str2 in dictionary.Keys)
            {
                list.Add(this.m_Vertices[str2]);
            }
            return list;
        }

        private ICollection<Vertex> GetOutgoingTransitiveClosure(Vertex root)
        {
            bool flag;
            Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
            dictionary.Add(root.Name.ToLower(), false);
            do
            {
                flag = true;
                string[] array = new string[dictionary.Keys.Count];
                dictionary.Keys.CopyTo(array, 0);
                foreach (string str in array)
                {
                    if (!dictionary[str])
                    {
                        Vertex vertex = this.m_Vertices[str];
                        foreach (Vertex vertex2 in vertex.Outgoing)
                        {
                            if (!dictionary.ContainsKey(vertex2.Name.ToLower()))
                            {
                                dictionary.Add(vertex2.Name.ToLower(), false);
                                flag = false;
                            }
                        }
                        dictionary[str] = true;
                        flag = false;
                    }
                }
            }
            while (!flag);
            List<Vertex> list = new List<Vertex>();
            foreach (string str2 in dictionary.Keys)
            {
                list.Add(this.m_Vertices[str2]);
            }
            return list;
        }

        public Vertex GetRoot()
        {
            return this.m_Root;
        }

        private string NormalizeName(string name)
        {
            if (name.StartsWith("CurrentCube.", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Remove(0, "CurrentCube.".Length);
            }
            string[] strArray = name.Split(new char[] { '.' });
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (string str in strArray)
            {
                if (!flag)
                {
                    builder.Append('.');
                }
                flag = false;
                if (!str.StartsWith("["))
                {
                    builder.AppendFormat("[{0}]", str);
                }
                else
                {
                    builder.Append(str);
                }
            }
            return builder.ToString();
        }

        public void UpdateStats(DataGridView dgv)
        {
            dgv.Tag = this;
            DataGridViewRow dataGridViewRow = new DataGridViewRow();
            dataGridViewRow.CreateCells(dgv);
            dataGridViewRow.Cells[0].Value = "(Total)";
            dataGridViewRow.Cells[1].Value = this.m_Vertices.Values.Count;
            dataGridViewRow.Cells[2].Value = this.m_Vertices.Values.Count;
            dgv.Rows.Add(dataGridViewRow);
            foreach (Vertex vertex in this.m_Vertices.Values)
            {
                ICollection<Vertex> outgoingTransitiveClosure = this.GetOutgoingTransitiveClosure(vertex);
                ICollection<Vertex> incomingTransitiveClosure = this.GetIncomingTransitiveClosure(vertex);
                Dictionary<string, Vertex> dictionary = new Dictionary<string, Vertex>(outgoingTransitiveClosure.Count + incomingTransitiveClosure.Count);
                foreach (Vertex vertex2 in outgoingTransitiveClosure)
                {
                    dictionary.Add(vertex2.Name, vertex2);
                }
                foreach (Vertex vertex3 in incomingTransitiveClosure)
                {
                    if (!dictionary.ContainsKey(vertex3.Name))
                    {
                        dictionary.Add(vertex3.Name, vertex3);
                    }
                }
                dataGridViewRow = new DataGridViewRow
                {
                    Tag = vertex
                };
                dataGridViewRow.CreateCells(dgv);
                dataGridViewRow.Cells[0].Value = vertex.Name;
                dataGridViewRow.Cells[0].Tag = dictionary.Values;
                dataGridViewRow.Cells[1].Value = outgoingTransitiveClosure.Count - 1;
                dataGridViewRow.Cells[1].Tag = outgoingTransitiveClosure;
                dataGridViewRow.Cells[2].Value = incomingTransitiveClosure.Count - 1;
                dataGridViewRow.Cells[2].Tag = incomingTransitiveClosure;
                dgv.Rows.Add(dataGridViewRow);
            }
            dgv.Sort(dgv.Columns[2], ListSortDirection.Descending);
        }

        //public void Visualize(NetMapControl netMapControl, ICollection<Vertex> showVertices, Vertex root)
        //{
        //    netMapControl.BeginUpdate();
        //    new LayoutManager { Layout = LayoutType.FruchtermanReingold }.ApplyLayoutToNetMapControl(netMapControl, 10);
        //    PerVertexWithLabelDrawer drawer = new PerVertexWithLabelDrawer();
        //    netMapControl.VertexDrawer = drawer;
        //    netMapControl.ShowToolTips = true;
        //    Graph graph = new Graph(GraphDirectedness.Directed, GraphRestrictions.None);
        //    netMapControl.Graph = graph;
        //    IVertexCollection vertices = graph.Vertices;
        //    IEdgeCollection edges = graph.Edges;
        //    ICollection<Vertex> outgoingTransitiveClosure = null;
        //    if (showVertices != null)
        //    {
        //        outgoingTransitiveClosure = showVertices;
        //    }
        //    else if (root != null)
        //    {
        //        outgoingTransitiveClosure = this.GetOutgoingTransitiveClosure(root);
        //    }
        //    else
        //    {
        //        outgoingTransitiveClosure = this.m_Vertices.Values;
        //    }
        //    foreach (Vertex vertex in outgoingTransitiveClosure)
        //    {
        //        if ((vertex.Incoming.Count != 0) || (vertex.Outgoing.Count != 0))
        //        {
        //            Microsoft.NetMap.Core.Vertex vertex2 = new Microsoft.NetMap.Core.Vertex();
        //            vertices.Add(vertex2);
        //            vertex2.SetValue(ReservedMetadataKeys.PerVertexPrimaryLabel, vertex.Name);
        //            vertex.VisualVertex = vertex2;
        //        }
        //    }
        //    foreach (Vertex vertex3 in outgoingTransitiveClosure)
        //    {
        //        foreach (Vertex vertex4 in vertex3.Outgoing)
        //        {
        //            bool flag = false;
        //            foreach (Vertex vertex5 in outgoingTransitiveClosure)
        //            {
        //                if (vertex4.Name == vertex5.Name)
        //                {
        //                    flag = true;
        //                    break;
        //                }
        //            }
        //            if (flag)
        //            {
        //                edges.Add(vertex3.VisualVertex, vertex4.VisualVertex, true);
        //            }
        //        }
        //    }
        //    EdgeDrawer edgeDrawer = (EdgeDrawer)netMapControl.EdgeDrawer;
        //    edgeDrawer.Color = Color.Black;
        //    edgeDrawer.DrawArrowOnDirectedEdge = true;
        //    edgeDrawer.Width = 3;
        //    netMapControl.EndUpdate();
        //}
    }
}
