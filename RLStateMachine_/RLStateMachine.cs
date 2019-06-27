using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
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
            var writer = new System.Xml.Serialization.XmlSerializer(typeof(RLSM));
            var file = new System.IO.StreamWriter(filename);

            writer.Serialize(file, this);
            file.Close();
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
