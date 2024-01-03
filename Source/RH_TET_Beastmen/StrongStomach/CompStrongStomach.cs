using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class CompStrongStomach : ThingComp
    {
        public int tickCounter = 0;
        public int healatonce = 3;

        public CompProperties_StrongStomach Props
        {
            get
            {
                return (CompProperties_StrongStomach)this.props;
            }
        }

        protected int rateInTicks
        {
            get
            {
                return this.Props.rateInTicks;
            }
        }

        public override void CompTick()
        {
            ++this.tickCounter;
            if (this.tickCounter < this.rateInTicks)
                return;
            Pawn parent = this.parent as Pawn;
            if (parent.health != null)
            {
                Hediff firstHediffOfDef = parent.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FoodPoisoning, false);
                if (firstHediffOfDef != null)
                    firstHediffOfDef.Severity = Math.Max(0.0f, firstHediffOfDef.Severity - 1f);
            }
            this.tickCounter = 0;
        }
    }
}
