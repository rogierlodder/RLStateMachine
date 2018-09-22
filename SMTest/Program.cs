using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLStateMachine;

namespace SMTest
{
    class Program
    {
        enum States {none, Idle, Starting, Started, Stopping, Stopped, Error}

        static void Main(string[] args)
        {
            RLSM SM = new RLSM("Initializer");
            int cmd = 0;
            bool error = false;

            SM.AddState(States.none.ToString(), new List<Transition>
            {
                new Transition("cmdIdle", () => cmd == 1, null, States.Idle.ToString()),
                new Transition("error"  , () => error == true, null, States.Error.ToString())
            });

            SM.AddState(States.Idle.ToString(), new List<Transition>
            {
                new Transition("cmdStart", () => cmd == 2, null, States.Starting.ToString()),
                new Transition("error"  , () => error == true, null, States.Error.ToString())
            });

            SM.AddState(States.Starting.ToString(), new List<Transition>
                { });

            SM.SaveGraph(@"C:\temp\sampleGraph");
        }
    }
}
