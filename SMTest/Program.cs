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

            SM.AddState(States.Entry.ToString(), new List<Transition>
            {
                new Transition("cmdIdle", () => cmd == 1, () => Console.WriteLine("Going to the idle state"), States.Idle.ToString()),
            }, null, SMState.StateType.entry);

            SM.AddState(States.Idle.ToString(), new List<Transition>
            {
                new Transition("cmdStart", () => cmd == 2, null, States.Starting.ToString()),
                new Transition("error"  , () => error == true, null, States.Error.ToString())
            }, () => Console.WriteLine("Running in the idle state"), SMState.StateType.idle);

            SM.AddState(States.Starting.ToString(), new List<Transition>
            {
                new Transition("Started", () => false, null, States.Started.ToString())
            }, null, SMState.StateType.transition);

            SM.AddState(States.Started.ToString(), new List<Transition>
            {
               new Transition("CmdStop", ()=>false, null, States.Stopping.ToString())
            }, null, SMState.StateType.idle);

            SM.AddState(States.Error.ToString(), new List<Transition>
            {
                new Transition("cmdReset", () => cmd == 2, null, States.Starting.ToString()),
            }, null, SMState.StateType.error);

            SM.AddState(States.Stopping.ToString(), new List<Transition>
            {
               new Transition("Stopped", ()=>false, null, States.Stopped.ToString()),
               new Transition("error"  , () => error == true, null, States.Error.ToString())
            }, null, SMState.StateType.transition);

            SM.AddState(States.Stopped.ToString(), new List<Transition>
            {  }, null, SMState.StateType.end);

            SM.SaveGraph(@"C:\temp\sampleGraph");

            ShowGraph();

            SM.Reset();
            Console.WriteLine(SM.CurrentState);

            SM.Run();
            Console.WriteLine(SM.CurrentState);

            cmd = 1;
            SM.Run();
            Console.WriteLine(SM.CurrentState);
            SM.Run();

            cmd = 2;
            SM.Run();
            Console.WriteLine(SM.CurrentState);
            SM.Run();

            Console.ReadLine();
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
