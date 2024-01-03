using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public class JobGiver_WatchPitFight : JobGiver_SpectateDutySpectateRect
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            PawnDuty duty = pawn.mindState.duty;
            IntVec3 cell;
            if (duty == null || !SpectatorCellFinder.TryFindSpectatorCellFor(pawn, duty.spectateRect, pawn.Map, out cell, duty.spectateRectAllowedSides, 1, (List<IntVec3>)null))
                return (Job)null;
            IntVec3 centerCell = duty.spectateRect.CenterCell;
            Building edifice = cell.GetEdifice(pawn.Map);
            if (edifice != null && edifice.def.category == ThingCategory.Building && (edifice.def.building.isSittable && pawn.CanReserve((LocalTargetInfo)((Thing)edifice), 1, -1, (ReservationLayerDef)null, false)))
                return new Job(BeastmenDefOf.RH_TET_Beastmen_WatchPitFightJob, (LocalTargetInfo)((Thing)edifice), (LocalTargetInfo)centerCell);
            return new Job(BeastmenDefOf.RH_TET_Beastmen_WatchPitFightJob, (LocalTargetInfo)cell, (LocalTargetInfo)centerCell);
        }
    }
}
