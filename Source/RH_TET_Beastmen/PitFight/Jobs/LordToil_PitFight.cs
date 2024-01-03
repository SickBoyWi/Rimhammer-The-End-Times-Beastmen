using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Beastmen
{
    public class LordToil_PitFight : LordToil
    {
        public static readonly IntVec3 OtherFighterNoFightSpotCellOffset = new IntVec3(-1, 0, 0);
        private IntVec3 spot;
        private Building_PitFightSpot pitFightingSpot;

        public LordToil_PitFight(IntVec3 spot, Building_PitFightSpot pitFightingSpotP)
        {
            this.spot = spot;
            this.pitFightingSpot = pitFightingSpotP;
        }

        public override void Init()
        {
            base.Init();
        }

        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
        {
            return BeastmenDefOf.RH_TET_Beastmen_WatchPitFight.hook;
        }

        public override void UpdateAllDuties()
        {
            for (int index = 0; index < this.lord.ownedPawns.Count; ++index)
                this.lord.ownedPawns[index].mindState.duty = new PawnDuty(BeastmenDefOf.RH_TET_Beastmen_WatchPitFight)
                {
                    spectateRect = this.CalculateSpectateRect(),
                    focus = (LocalTargetInfo)((Thing)this.pitFightingSpot)
                };
        }

        private CellRect CalculateSpectateRect()
        {
            return CellRect.CenteredOn(this.pitFightingSpot.Position, Mathf.RoundToInt(this.pitFightingSpot.GetComp<CompFightingPit>().radius - 1f));
        }
    }
}
