using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLStateMachine;

namespace SMTest
{
    class Program
    {
        enum States {Entry, Idle, Starting, Started, Stopping, Stopped, Error}

        static void Main(string[] args)
        {
            RLSM SM = new RLSM("Initializer");
            int cmd = 0;
            bool error = false;
            //SM.FirstAction = () => Console.WriteLine("We always to this first");
            //SM.LastAction = () => Console.WriteLine("And finally we do this\n");

            SM.StateChanged = (p) => { Console.WriteLine($"New State : {p}"); };

            SM.AddState(States.Entry.ToString(), new List<Transition>
            {
                new Transition("cmdIdle", () => cmd == 1, () => { }, States.Idle.ToString()),
            }, null, SMState.StateType.entry);

            SM.AddState(States.Idle.ToString(), new List<Transition>
            {
                new Transition("cmdStart", () => true, null, States.Starting.ToString()),
                new Transition("error"  , () => error == true, null, States.Error.ToString())
            }, () => { }, SMState.StateType.idle);

            SM.AddState(States.Starting.ToString(), new List<Transition>
            {
                new Transition("Started", () => true, null, States.Started.ToString())
            }, null, SMState.StateType.transition);

            SM.AddState(States.Started.ToString(), new List<Transition>
            {
               new Transition("CmdStop", () => true, null, States.Stopping.ToString())
            }, null, SMState.StateType.idle);

            SM.AddState(States.Error.ToString(), new List<Transition>
            {
                new Transition("cmdReset", () => true, null, States.Starting.ToString()),
            }, null, SMState.StateType.error);

            SM.AddState(States.Stopping.ToString(), new List<Transition>
            {
               new Transition("Stopped", () => true, null, States.Stopped.ToString()),
               new Transition("error"  , () => error == true, null, States.Error.ToString())
            }, null, SMState.StateType.transition);

            SM.AddState(States.Stopped.ToString(), new List<Transition>
            {  }, null, SMState.StateType.end);

            SM.SaveGraph(@"C:\temp\sampleGraph");

            ShowGraph();

            SM.Reset();

            SM.Run();

            cmd = 1;
            SM.Run();

            //cmd = 2;
            //SM.Run();

            //SM.Run();

            Console.ReadLine();
        }

        private static void useEnum(Enum E, int s)
        {

        }

        private static void ShowGraph()
        {
            Process proc = new Process();
            proc.StartInfo.FileName = @"GraphDisplay.exe";
            proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(@"..\..\..\GraphDisplay\bin\debug\");
            proc.StartInfo.Arguments = @"c:\Temp\sampleGraph.msagl";
            proc.Start();
        }
    }
}
