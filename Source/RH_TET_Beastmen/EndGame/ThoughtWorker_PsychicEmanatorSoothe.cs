using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class ThoughtWorker_PsychicEmanatorSoothe : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned || (!HerdUtility.IsBeastman(p) && !RH_TET_BeastmenMod.anyOneCanBeastActive))
                return (ThoughtState)false;
            List<Thing> thingList = p.Map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Psychic);
            if (thingList != null && thingList.Count > 0)
            {
                return (ThoughtState)true;
            }
            return (ThoughtState)false;
        }
    }
}