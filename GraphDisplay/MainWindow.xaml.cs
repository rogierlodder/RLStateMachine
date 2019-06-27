using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using RLStateMachine;
using Shape = Microsoft.Msagl.Drawing.Shape;

namespace GraphDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DockPanel Panel = new DockPanel();

        public MainWindow()
        {
            InitializeComponent();
            this.Content = Panel;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.GetCommandLineArgs().Length >= 1)
            {
                string fileName = Environment.GetCommandLineArgs()[1];
                GraphViewer graphViewer = new GraphViewer();

                if (File.Exists(fileName))
                {
                    graphViewer.BindToPanel(Panel);
                    Graph graph = new Graph();

                    graph = LoadGraphFromFile(fileName);
                    graph.Attr.LayerDirection = LayerDirection.TB;
                    graphViewer.Graph = graph;
                }
            }
        }



        public Graph LoadGraphFromFile(string filename)
        {
            Graph G = new Graph();

            //load graph file
            var States = new Dictionary<string, StateNode>();
            XElement root;
            try { root = XElement.Load(filename); }
            catch
            {
                throw new System.ArgumentException($"The Recipe file ({filename}) could not be opened");
            }

            //parse graph file
            var states = root.Elements().Where(p => p.Name == "States").Elements().ToList();
            foreach (var state in states)
            {
                var name = state.Attribute("Name").Value;
                var type = state.Attribute("Type").Value;

                StateType ST;
                Enum.TryParse(type, out ST);
                var sn = new StateNode { Name = name, Type = ST };

                var tnodes = state.Elements().Where(p => p.Name == "Transitions").Elements().ToList();
                foreach (var tn in tnodes)
                {
                    var ev = tn.Attribute("Event").Value;
                    var tp = tn.Attribute("TargetState").Value;
                    sn.Transitions.Add(new TransitionNode { Event = ev, NewState = tp });
                }
                States.Add(name, sn);
                
            }

            //add all nodes and assign the color and shape
            foreach (var N in States)
            {
                Color NodeColor = new Color();
                var NodeShape = new Microsoft.Msagl.Drawing.Shape();
                NodeShape = Shape.Box;
                var d = StateType.entry.ToString();
                int lineWidth = 2;
                int lableMargin = 1;
                switch (N.Value.Type)
                {
                    case StateType.entry:
                        NodeColor = Color.Green;
                        NodeShape = Shape.Circle;
                        lableMargin = 2;
                        break;
                    case StateType.end:
                        NodeColor = Color.DarkBlue;
                        lableMargin = 3;
                        break;
                    case StateType.transition:
                        NodeColor = Color.CadetBlue;
                        NodeShape = Shape.Ellipse;
                        break;
                    case StateType.error:
                        NodeColor = Color.Red;
                        NodeShape = Shape.Ellipse;
                        lableMargin = 0;
                        lineWidth = 3;
                        break;
                    case StateType.idle:
                        NodeColor = Color.Black;
                        NodeShape = Shape.Diamond;
                        break;
                    default:
                        break;
                }
                Node NewNode = new Node(N.Value.Name);
                NewNode.Attr.Color = NodeColor;
                NewNode.Attr.Shape = NodeShape;
                NewNode.Attr.LineWidth = lineWidth;
                NewNode.Attr.LabelMargin = lableMargin;

                //check if the state has outgoing transitions
                if (N.Value.Transitions.Count == 0 && N.Value.Type != StateType.end) NewNode.Attr.FillColor = Color.Red;

                G.AddNode(NewNode);
            }
            //list of all transitions
            var AllTrans = new List<TransitionNode>();

            ////add transitions
            foreach (var N in States)
            {
                foreach (var T in N.Value.Transitions)
                {
                    AllTrans.Add(T);

                    var newState = T.NewState;
                    if (!States.ContainsKey(T.NewState))
                    {
                        Node NewNode = new Node(newState.ToString());
                        NewNode.Attr.Color = Color.Red;
                        NewNode.Attr.FillColor = Color.Red;
                        G.AddNode(NewNode);
                    }

                    G.AddEdge(N.Value.Name, $" {T.Event} ", newState.ToString());
                }
            }

            ////Check is there are states that have no ingoing transitions
            foreach (var N in States.Values)
            {
                if (N.Type != StateType.entry && AllTrans.Count(p => p.NewState.ToString() == N.Name) == 0)
                {
                    G.Nodes.Where(q => q.Id == N.Name).First().Attr.FillColor = Color.Magenta;
                }
            }

            return G;
        }
    }

    public class StateNode
    {
        public string Name {get; set;}
        public StateType Type { get; set; }
        public List<TransitionNode> Transitions { get; set; } = new List<TransitionNode>();
    }

    public class TransitionNode
    {
        public string Event { get; set; }
        public string NewState { get; set; }
    }
}

