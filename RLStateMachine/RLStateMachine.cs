using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;

namespace RLStateMachine
{
    public class RLSM
    {
        private string _CurrentState;
        private string _Name;
        private Graph G;
        private Dictionary<string, SMState> States = new Dictionary<string, SMState>();

        public string Name { get { return _Name; } }
        public Action FirstAction { get; set; }
        public Action LastAction { get; set; }
        public string CurrentState
        {
            get
            {
                if (_CurrentState is null) return "<Unknown>";
                else return _CurrentState;
            }
        }

        public RLSM(string name)
        {
            _Name = name;
            G = new Graph();
        }

        public void Reset()
        {
            try
            {
                _CurrentState = States.Values.Where(p => p.Type == SMState.StateType.entry).First().Name;
            }
            catch (Exception e)
            {
                throw new Exception("The state machine cannot be reset as it does not have a single unique entry state");
            }
        }

        public SMState AddState(string nodename, List<Transition> transitions, Action alwaysAction = null, SMState.StateType type = SMState.StateType.idle)
        {
            SMState N = new SMState(nodename, alwaysAction, transitions, type);
            States.Add(nodename, N);
            return N;
        }

        public SMState GetState(string stateName)
        {
            return States.Values.Where(p => p.Name == stateName).FirstOrDefault();
        }

        public void Run()
        {
            FirstAction?.Invoke();

            if (States.ContainsKey(_CurrentState))
            {
                string newState = States[_CurrentState].RunNode();
                if (newState != "") _CurrentState = newState;
            }

            LastAction?.Invoke();
        }

        public void SaveGraph(string filename)
        {
            Reset(); 
             
            //add all nodes and assign the color and shape
            foreach (var N in States)
            {
                Node NewNode = new Node(N.Value.Name);
                Color NodeColor = new Color();
                Shape NodeShape = new Shape();
                int lineWidth = 2;
                NodeShape = Shape.Box;
                int lableMargin = 1;
                switch (N.Value.Type)
                {
                    case SMState.StateType.entry:
                        NodeColor = Color.Green;
                        NodeShape = Shape.Circle;
                        lableMargin = 2;
                        break;
                    case SMState.StateType.end:
                        NodeColor = Color.DarkBlue;
                        lableMargin = 3;
                        break;
                    case SMState.StateType.transition:
                        NodeColor = Color.CadetBlue;
                        NodeShape = Shape.Ellipse;
                        break;
                    case SMState.StateType.error:
                        NodeColor = Color.Red;
                        NodeShape = Shape.Ellipse;
                        lableMargin = 0;
                        lineWidth = 3;
                        break;
                    case SMState.StateType.idle:
                        NodeColor = Color.Black;
                        NodeShape = Shape.Diamond;
                        break;
                    default:
                        break;
                }
                NewNode.Attr.Color = NodeColor;
                NewNode.Attr.Shape = NodeShape;
                NewNode.Attr.LineWidth = lineWidth;
                NewNode.Attr.LabelMargin = lableMargin;
                if (N.Value.Transitions.Count == 0 && N.Value.Type != SMState.StateType.end) NewNode.Attr.FillColor = Color.Red;
                G.AddNode(NewNode);
            }

            //add transitions
            foreach (var N in States)
            {
                foreach (var T in N.Value.Transitions)
                {
                    string newState = "";
                    newState = T.NewState;
                    if (!States.ContainsKey(T.NewState))
                    {
                        Node NewNode = new Node(newState);
                        NewNode.Attr.Color = Color.Red;
                        NewNode.Attr.FillColor = Color.Red;
                        G.AddNode(NewNode);
                    }

                    G.AddEdge(N.Value.Name, $" {T.Name} ", newState);
                }
            }

            G.Write(filename);
        }
    }

    public class SMState
    {
        public enum StateType { entry, end, transition, error, idle}

        private string _StateName;
        private StateType _Type;
        public string Name { get { return _StateName; } }
        public StateType Type { get { return _Type; } }

        public Action AlwaysAction { get; set; }

        public List<Transition> Transitions { get; private set; } = new List<Transition>();

        public SMState(string nodename, Action alwaysAction, List<Transition> transitions, StateType type = StateType.idle)
        {
            _StateName = nodename;
            AlwaysAction = alwaysAction;
            _Type = type;
            //copy transistion list;
            Transitions = new List<Transition>();
            foreach (var T in transitions) Transitions.Add(T.Copy());
        }

        public string RunNode()
        {
            AlwaysAction?.Invoke();
            foreach (var T in Transitions)
            {
                if (T != null) return T.Check();                    
            }
            return null;
        }
    }

    public class Transition
    {
        private string _Name;
        private string _NewState;

        private Func<bool> Condition;
        private Action OperationsIfTrue;

        public string NewState { get { return _NewState; } }
        public string Name { get { return _Name; } }

        public Transition()
        {
            _Name = "";
            Condition = () => false;
            OperationsIfTrue = () => { };
            _NewState = "";            
        }

        public Transition(string name, Func<bool> condition, Action operationsiftrue, string newstate)
        {
            _Name = name;
            _NewState = newstate;
            Condition = condition;
            OperationsIfTrue = operationsiftrue;
        }

        public string Check()
        {
            if (Condition.Invoke())
            {
                OperationsIfTrue?.Invoke();
                return _NewState;
            }
            else return "";
        }

        public Transition Copy()
        {
            var T = new Transition
            {
                _Name = this._Name,
                Condition = this.Condition,
                OperationsIfTrue = this.OperationsIfTrue,
                _NewState = this._NewState
            };
            return T;
        }
    }
}
