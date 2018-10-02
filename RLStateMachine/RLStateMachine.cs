using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Drawing;

namespace RLStateMachine
{
    public enum StateType { entry, end, transition, error, idle }

    public class RLSM
    {
        private Graph G;

        public string Name { get; private set; }
        public Action FirstAction { get; set; }
        public Action LastAction { get; set; }
        public Dictionary<Enum, SMState> States { get; private set; } = new Dictionary<Enum, SMState>();
        public Enum CurrentState { get; private set; }
        public Action<Enum> StateChanged { get; set; }

        public RLSM(string name)
        {
            Name = name;
            G = new Graph();
        }

        public void Reset()
        {
            try
            {
                CurrentState = States.Where(p => p.Value.Type == StateType.entry).First().Key;
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
            States.Add(stateName, N);
            return N;
        }

        public SMState GetState(Enum stateName)
        {
            if (States.ContainsKey(stateName)) return States[stateName];
            else return null;
        }

        public void Run()
        {
            FirstAction?.Invoke();
            Enum oldState = null;

            do
            {
                oldState = CurrentState;
                if (States.ContainsKey(CurrentState))
                {
                    Enum newState = States[CurrentState]?.RunNode();
                    if (newState != null)
                    {
                        CurrentState = newState;
                        StateChanged?.Invoke(newState);
                    }
                }
            } while (oldState != CurrentState);

            LastAction?.Invoke();
        }

        public void SaveGraph(string filename)
        {
            Reset(); 
             
            //add all nodes and assign the color and shape
            foreach (var N in States)
            {
                Color NodeColor = new Color();
                Shape NodeShape = new Shape();
                NodeShape = Shape.Box;

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
            var AllTrans = new List<Transition>();

            //add transitions
            foreach (var N in States)
            {
                foreach (var T in N.Value.Transitions)
                {
                    AllTrans.Add(T);

                    Enum newState = null;
                    newState = T.NewState;
                    if (!States.ContainsKey(T.NewState))
                    {
                        Node NewNode = new Node(newState.ToString());
                        NewNode.Attr.Color = Color.Red;
                        NewNode.Attr.FillColor = Color.Red;
                        G.AddNode(NewNode);
                    }

                    G.AddEdge(N.Value.Name, $" {T.Name} ", newState.ToString());
                }
            }

            //Check is there are states that have no ingoing transitions
            foreach (var N in States.Values)
            {
                if (N.Type != StateType.entry && AllTrans.Count(p => p.NewState.ToString() == N.Name) == 0)
                {
                    G.Nodes.Where(q => q.Id == N.Name).First().Attr.FillColor = Color.Magenta;
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

        public Enum RunNode()
        {
            AlwaysAction?.Invoke();
            foreach (var T in Transitions)
            {
                if (T == null) return null;

                var newState = T.CheckTransition();
                if (newState != null) return newState;                    
            }
            return null;
        }
    }

    public class Transition
    {
        private string _Name;
        private Enum _NewState;

        private Func<bool> Condition;
        private Action OperationsIfTrue;

        public Enum NewState { get { return _NewState; } }
        public string Name { get { return _Name; } }

        public Transition()
        {
            _Name = "";
            Condition = () => false;
            OperationsIfTrue = () => { };
            _NewState = null;            
        }

        public Transition(string name, Func<bool> condition, Action operationsiftrue, Enum newstate)
        {
            _Name = name;
            _NewState = newstate;
            Condition = condition;
            OperationsIfTrue = operationsiftrue;
        }

        public Enum CheckTransition()
        {
            if (Condition.Invoke())
            {
                OperationsIfTrue?.Invoke();
                return _NewState;
            }
            else return null;
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
