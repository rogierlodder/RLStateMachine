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
        private Dictionary<string, SMState> Nodes = new Dictionary<string, SMState>();

        public string CurrentState  { get { return _CurrentState; } }
        public string Name { get { return _Name; } }
        public Action FirstAction { get; set; }
        public Action LastAction { get; set; }
        public string CurrentStateName
        {
            get
            {
                if (Nodes.ContainsKey(_CurrentState)) return Nodes[_CurrentState].Name;
                else return "<Unknown>";
            }
        }

        public RLSM(string name)
        {
            _Name = name;
            G = new Graph();
        }

        public void AddState(string nodename, List<Transition> transitions, Action alwaysAction = null)
        {
            SMState N = new SMState(nodename, alwaysAction, transitions);
            Nodes.Add(nodename, N);
        }

        public SMState GetState(string stateName)
        {
            return Nodes.Values.Where(p => p.Name == stateName).FirstOrDefault();
        }

        public void Run()
        {
            FirstAction?.Invoke();

            string newstate = null;
            if (Nodes.ContainsKey(_CurrentState)) newstate = Nodes[_CurrentState].RunNode();

            LastAction?.Invoke();
        }

        public void SaveGraph(string filename)
        {
            foreach (var N in Nodes) G.AddNode(N.Value.Name);

            foreach (var N in Nodes)
            {
                foreach (var T in N.Value.Transitions)
                {
                    string newState = "";
                    newState = T.NewState;
                    if (!Nodes.ContainsKey(T.NewState))
                    {
                        Node NewNode = new Node(newState);
                        NewNode.Attr.Color = Color.Red;
                        NewNode.Attr.FillColor = Color.Red;
                        G.AddNode(NewNode);
                    }

                    G.AddEdge(N.Value.Name, T.Name, newState);
                }
            }

            G.Write(filename);
        }
    }

    public class SMState
    {
        private string _StateName;
        public string Name { get { return _StateName; } }

        public Action AlwaysAction { get; set; }

        public List<Transition> Transitions { get; private set; } = new List<Transition>();

        public SMState(string nodename, Action alwaysAction, List<Transition> transitions)
        {
            _StateName = nodename;
            AlwaysAction = alwaysAction;

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
