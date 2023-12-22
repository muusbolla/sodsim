using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Slamulator
{
    class ThreadedSimulationContext
    {
        private Thread _thread;

        public Random RNG;
        public ServerSide Server;
        public DWSimulator Simulator;
        public double _duration;
        public ThreadedSimulationContext()
        {
            RNG = null;
            Server = new ServerSide();
            Simulator = new DWSimulator(this);
        }

        public void ThreadedSimulate(double duration)
        {
            lock(this)
            {
                if(_thread == null)
                {
                    _duration = duration;
                    _thread = new Thread(this.Simulate);
                    _thread.Start();
                }
            }
        }

        public void Join()
        {
            _thread.Join();
            lock(this)
            {
                _thread = null;
            }
        }

        private void Simulate()
        {
            RNG = ThreadSafeRandom.GetThreadSafeRandom();
            Simulator.Run(_duration);
        }

        public double RollRange(double min, double max)
        {
            double roll = RNG.NextDouble();
            return (roll * (max - min) + min);
        }

        public bool RollChance(double odds)
        {
            double roll = RNG.NextDouble();// 0 <= roll < 1
            if (roll < odds)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
