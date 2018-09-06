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
        private int _CurrentState;
        private string _Name;
        private Graph G;
        private Dictionary<int, SMState> Nodes = new Dictionary<int, SMState>();

        public int CurrentState  { get { return _CurrentState; } }
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

        public void AddState(int nodenr, string nodename, List<Transition> transitions, Action alwaysAction = null)
        {
            G.AddNode(nodename);
            SMState N = new SMState(nodenr, nodename, alwaysAction, transitions);
            Nodes.Add(nodenr, N);
        }

        public SMState GetState(int stateNr)
        {
            if (Nodes.ContainsKey(stateNr)) return Nodes[stateNr];
            else return null;
        }

        public SMState GetState(string stateName)
        {
            return Nodes.Values.Where(p => p.Name == stateName).FirstOrDefault();
        }

        public void Run()
        {
            FirstAction?.Invoke();

            int? newstate = null;
            if (Nodes.ContainsKey(_CurrentState)) newstate = Nodes[_CurrentState].RunNode();

            LastAction?.Invoke();
        }

        public void SaveGraph(string filename)
        {
            G.Write(filename);
        }
    }

    public class SMState
    {
        private string _Name;
        private int _StateNum;

        public int Number { get { return _StateNum; } }
        public string Name { get { return _Name; } }

        public Action AlwaysAction { get; set; }

        public List<Transition> Transitions { get; private set; } = new List<Transition>();

        public SMState(int nodenr, string nodename, Action alwaysAction, List<Transition> transitions)
        {
            _StateNum = nodenr;
            _Name = nodename;
            AlwaysAction = alwaysAction;

            //copy transistion list;
            Transitions = new List<Transition>();
            foreach (var T in transitions) Transitions.Add(T.Copy());
        }

        public int? RunNode()
        {
            AlwaysAction?.Invoke();
            foreach (var T in Transitions)
            {
                if (T != null)
                {
                    var newState = T.Check();
                    if (newState != null) return newState;
                }
            }
            return null;
        }
    }

    public class Transition
    {
        private string Name;
        private Func<bool> Condition;
        private Action OperationsIfTrue;
        private int NewState;

        public Transition()
        {
            Name = "";
            Condition = () => false;
            OperationsIfTrue = () => { };
            NewState = 0;            
        }

        public Transition(string name, Func<bool> condition, Action operationsiftrue, int newstate)
        {
            Name = name;
            Condition = condition;
            OperationsIfTrue = operationsiftrue;
            NewState = newstate;
        }

        public int? Check()
        {
            if (Condition.Invoke())
            {
                OperationsIfTrue?.Invoke();
                return NewState;
            }
            else return null;
        }

        public Transition Copy()
        {
            var T = new Transition();
            T.Name = this.Name;
            T.Condition = this.Condition;
            T.OperationsIfTrue = this.OperationsIfTrue;
            T.NewState = this.NewState;

            return T;
        }
    }
}
