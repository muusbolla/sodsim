using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class DWSimulator
    {
        public ThreadedSimulationContext MyContext;
        public PlayerState PlayerOne;

        public DWSimulator(ThreadedSimulationContext _myContext)
        {
            MyContext = _myContext;
            PlayerOne = new PlayerState(MyContext);
        }
        

        public void Run(double duration)
        {
            //INIT
            MyContext.Server.ResetServer();
            PlayerOne.BeginCombat();

            //RUN
            while (MyContext.Server.Time < duration)
            {
                MyContext.Server.DoNext();
            }

            //CLEANUP
            PlayerOne.Buffs.ExpireAllBuffs();

            //if(bre)
            //{
            //    breWeightedUptime = breWeightedUptime / duration;
            //}

            if (PlayerOne.logging)
            {
                string[] log = PlayerOne.GetLog();
                string wd = System.IO.Directory.GetCurrentDirectory();
                bool exists = System.IO.Directory.Exists(wd + @"\Logs");

                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(wd + @"\Logs");
                }
                System.IO.File.WriteAllLines(String.Format(@"{0}\Logs\{1}.txt",
                    wd, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture)), log);
            }

        }
        
    }
}
