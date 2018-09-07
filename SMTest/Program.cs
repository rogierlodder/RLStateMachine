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

            SM.AddState((int)States.none, "None", new List<Transition>
            {
                new Transition("cmdIdle", () => cmd == 1, null, (int)States.Idle),
                new Transition("error"  , () => error == true, null, (int)States.Error)
            });

            SM.AddState((int)States.Idle, "Idle", new List<Transition>
            {
                new Transition("cmdStart", () => cmd == 2, null, (int)States.Starting),
                new Transition("error"  , () => error == true, null, (int)States.Error)
            });

            SM.SaveGraph(@"C:\temp\sampleGraph");
        }
    }
}
