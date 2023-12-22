using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class ProcCollection
    {
        public Proc BonereaversEdge;
        public Proc EmpyreanDemolisher;
        public Proc EskhandarsRightClaw;
        public Proc Felstriker;
        public Proc FlurryAxe;
        public Proc Ironfoe;
        public Proc UntamedBlade;

        public Proc DMCMaelstrom;
        public Proc HandOfJustice;
        public Proc HeartOfWyrmthalak;

        public Proc Crusader;

        public Proc WindfuryTotem;

        public ProcCollection(PlayerState p)
        {
            BonereaversEdge = new BonereaversEdgeProc(p);
            EmpyreanDemolisher = new EmpyreanDemolisherProc(p);
            EskhandarsRightClaw = new EskhandarsRightClawProc(p);
            Felstriker = new FelstrikerProc(p);
            FlurryAxe = new FlurryAxeProc(p);
            Ironfoe = new IronfoeProc(p);
            UntamedBlade = new UntamedBladeProc(p);

            DMCMaelstrom = new DMCMaelstromProc(p);
            HandOfJustice = new HandOfJusticeProc(p);
            HeartOfWyrmthalak = new HeartOfWyrmthalakProc(p);

            Crusader = new CrusaderProc(p);

            WindfuryTotem = new WindfuryTotemProc(p);
        }
    }
}
