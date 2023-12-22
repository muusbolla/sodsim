using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Slamulator
{
    class BuffCollection
    {
        //Primary
        public Buff BonereaversEdge;
        public Buff EmpyreanDemolisher;
        public Buff EskhandarsRightClaw;
        public Buff Flurry;
        public Buff MHCrusader;
        public Buff OHCrusader;
        public Buff UntamedFury;
        public Buff Earthstrike;
        public Buff KissOfTheSpider;
        public Buff SlayersCrest;
        public Buff DeathWish;

        //Secondary
        public Buff BlessingOfKings;
        public Buff SpiritOfZandalar;

        //Final
        public Buff Felstriker;
        public Buff Recklessness;

        public BuffCollection(PlayerState p)
        {
            BonereaversEdge = new BonereaversEdgeBuff(p);
            EmpyreanDemolisher = new EmpyreanDemolisherBuff(p);
            EskhandarsRightClaw = new EskhandarsRightClawBuff(p);
            Flurry = new Flurry(p);
            MHCrusader = new Crusader(p);
            OHCrusader = new Crusader(p);
            UntamedFury = new UntamedFury(p);
            Earthstrike = new EarthstrikeBuff(p);
            KissOfTheSpider = new KissOfTheSpiderBuff(p);
            SlayersCrest = new SlayersCrestBuff(p);
            DeathWish = new DeathWishBuff(p);

            BlessingOfKings = new BlessingOfKings(p);
            SpiritOfZandalar = new SpiritOfZandalar(p);

            Felstriker = new Felstriker(p);
            Recklessness = new RecklessnessBuff(p);
        }
        public void ExpireAllBuffs()
        {
            FieldInfo[] fields = typeof(BuffCollection).GetFields();
            foreach (FieldInfo f in fields)
            {
                Type t = typeof(Buff);
                if (f.FieldType == t)
                {
                    Buff b = f.GetValue(this) as Buff;
                    if (b != null && b.IsActive)
                    {
                        b.Expire();
                    }
                }
            }

        }
    }
}
