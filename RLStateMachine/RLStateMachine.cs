using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Drawing;

namespace RLStateMachine
{
    public enum StateType { entry, end, transition, error, idle }

    public class RLSM
    {
        private string _CurrentState;
        private Graph G;

        public string Name { get; }
        public Action FirstAction { get; set; }
        public Action LastAction { get; set; }
        public Dictionary<string, SMState> States { get; } = new Dictionary<string, SMState>();

        public string CurrentState
        {
            get
            {
                if (_CurrentState is null || _CurrentState == "") return "<Unknown>";
                else return _CurrentState;
            }
        }

        public Action<string> StateChanged { get; set; }

        public RLSM(string name)
        {
            Name = name;
            G = new Graph();
        }

        public void Reset()
        {
            try
            {
                _CurrentState = States.Values.Where(p => p.Type == StateType.entry).First().Name;
            }
            catch (Exception e)
            {
                throw new Exception($"The {Name} state machine cannot be reset as it does not have a single unique entry state");
            }
        }

        public SMState AddState(Enum stateName, List<Transition> transitions, Action alwaysAction = null, StateType type = StateType.idle)
        {
            if (stateName.ToString() == "" || stateName is null)
            {
                throw new Exception("The name of a node cannot be null or empty");
            }
            SMState N = new SMState(stateName, alwaysAction, transitions, type);
            States.Add(stateName.ToString(), N);
            return N;
        }

        public SMState GetState(string stateName)
        {
            if (States.ContainsKey(stateName)) return States[stateName];
            else return null;
        }

        public void Run()
        {
            FirstAction?.Invoke();
            string oldState = null;
            
            //keep runnng the nodes until the state does not change
            while (oldState != _CurrentState)
            {
                oldState = _CurrentState;
                if (States.ContainsKey(_CurrentState))
                {
                    string newState = States[_CurrentState]?.RunNode();
                    if (newState != "")
                    {
                        _CurrentState = newState;
                        StateChanged?.Invoke(newState);
                    }
                }
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
                NewNode.Attr.Color = NodeColor;
                NewNode.Attr.Shape = NodeShape;
                NewNode.Attr.LineWidth = lineWidth;
                NewNode.Attr.LabelMargin = lableMargin;
                if (N.Value.Transitions.Count == 0 && N.Value.Type != StateType.end) NewNode.Attr.FillColor = Color.Red;
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
         private string _StateName;
        private StateType _Type;
        public string Name { get { return _StateName; } }
        public StateType Type { get { return _Type; } }

        public Action AlwaysAction { get; set; }

        public List<Transition> Transitions { get; private set; } = new List<Transition>();

        public SMState(Enum nodename, Action alwaysAction, List<Transition> transitions, StateType type = StateType.idle)
        {
            _StateName = nodename.ToString();
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
            return "";
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

        public Transition(string name, Func<bool> condition, Action operationsiftrue, Enum newstate)
        {
            _Name = name;
            _NewState = newstate.ToString();
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
