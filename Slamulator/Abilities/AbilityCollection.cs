using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class AbilityCollection
    {
        public Ability WhiteHitMH;
        public Ability WhiteHitOH;
        public Ability Bloodthirst;
        public Ability Whirlwind;
        public Ability Overpower;
        public Ability Hamstring;
        public Ability Slam;
        public Ability Execute;

        public Ability BerserkerStance;
        public Ability BattleStance;

        public Ability DeathWish;
        public Ability Recklessness;

        public Ability HOJHitMH;
        public Ability IronfoeHitMH;
        public Ability FlurryAxeHitMH;
        public Ability WindfuryTotemHitMH;
        public Ability DMCMaelstrom;
        public Ability HeartOfWyrmthalak;

        public Ability Earthstrike;
        public Ability KissOfTheSpider;
        public Ability SlayersCrest;

        public AbilityCollection(PlayerState p)
        {
            WhiteHitMH = new WhiteHitMH(p);
            WhiteHitOH = new WhiteHitOH(p);
            Bloodthirst = new Bloodthirst(p);
            Whirlwind = new Whirlwind(p);
            Overpower = new Overpower(p);
            Hamstring = new Hamstring(p);
            Slam = new Slam(p);
            Execute = new Execute(p);

            BerserkerStance = new BerserkerStance(p);
            BattleStance = new BattleStance(p);

            DeathWish = new DeathWish(p);
            Recklessness = null;

            HOJHitMH = new HOJHitMH(p);
            IronfoeHitMH = new IronfoeHitMH(p);
            FlurryAxeHitMH = new FlurryAxeHitMH(p);
            WindfuryTotemHitMH = new WindfuryTotemHitMH(p);
            DMCMaelstrom = new DMCMaelstrom(p);
            HeartOfWyrmthalak = new HeartOfWyrmthalak(p);

            Earthstrike = new Earthstrike(p);
            KissOfTheSpider = new KissOfTheSpider(p);
            SlayersCrest = new SlayersCrest(p);
        }
    }
}
