using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public class ThoughtWorker_ListeningToDrum : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
                return (ThoughtState)false;
            ThingDef def = BeastmenDefOf.RH_TET_Beastmen_Drum;
            return (ThoughtState)(GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(def), PathEndMode.ClosestTouch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), def.building.instrumentRange, (Predicate<Thing>)(thing =>
            {
                Building_Drum drum = thing as Building_Drum;
                if (drum != null && drum.IsBeingPlayed)
                { 
                    bool retVal = Building_Drum.IsAffectedByInstrument(def, drum.Position, p.Position, p.Map);

                    if (retVal)
                    {
                        Need_Violence need = (Need_Violence)p.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == BeastmenDefOf.RH_TET_Beastmen_NeedViolence));
                        if (need != null && !need.Disabled)
                        {
                            need.didViolence = true;
                        }
                    }

                    return retVal;
                }
                return false;
            }), (IEnumerable<Thing>)null, 0, -1, false, RegionType.Set_Passable, false) != null);
        }
    }
}