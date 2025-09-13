using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public class JobDriver_WatchPitFight : JobDriver
    {
        private const TargetIndex MySpotOrChairInd = TargetIndex.A;
        private const TargetIndex WatchTargetInd = TargetIndex.B;
        private bool gaveMood = false;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, (ReservationLayerDef)null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            bool haveChair = this.job.GetTarget(TargetIndex.A).HasThing;
            if (haveChair)
                this.EndOnDespawnedOrNull<JobDriver_WatchPitFight>(TargetIndex.A, JobCondition.Incompletable);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            yield return new Toil()
            {
                tickAction = (Action)(() =>
                {
                    if (!gaveMood)
                    {
                        gaveMood = true;
                        this.pawn.needs.mood.thoughts.memories.TryGainMemory(BeastmenDefOf.RH_TET_Beastmen_WatchedPitFight, (Pawn)null);
                    }
                    if (((Building_PitFightSpot)this.pawn.mindState.duty.focus.Thing).currentState == Building_PitFightSpot.State.fighting)
                        JoyUtility.JoyTickCheckEnd(this.pawn, 1,JoyTickFullJoyAction.None, 1f, (Building)null);
                    this.pawn.rotationTracker.FaceCell(this.job.GetTarget(TargetIndex.B).Cell);
                    this.pawn.GainComfortFromCellIfPossible(1);
                    if (!this.pawn.IsHashIntervalTick(100))
                        return;
                    this.pawn.jobs.CheckForJobOverride();
                }),
                defaultCompleteMode = ToilCompleteMode.Never,
                handlingFacing = true
            };
        }
    }
}
