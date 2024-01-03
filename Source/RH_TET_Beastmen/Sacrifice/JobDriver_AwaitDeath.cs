using System.Collections.Generic;
using Verse.AI;
using Verse;
using RimWorld;

namespace TheEndTimes_Beastmen
{
    public class JobDriver_AwaitDeath : JobDriver_Wait
    {

        protected Building_MassiveHerdstone DropHerdstone
        {
            get
            {
                return (Building_MassiveHerdstone)base.job.GetTarget(TargetIndex.A).Thing;
            }
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {

            yield return new Toil
            {
                initAction = delegate
                {
                    this.pawn.Reserve(this.pawn.Position, this.job);// De ReserveDestinationFor(this.pawn, this.pawn.Position);
                    this.pawn.pather.StopDead();
                    JobDriver curDriver = this.pawn.jobs.curDriver;
                    pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
                    curDriver.asleep = false;
                },
                tickAction = delegate
                {
                    if (this.job.expiryInterval == -1 && this.job.def == JobDefOf.Wait_Combat && !this.pawn.Drafted)
                    {
                        Log.Error(this.pawn + " in eternal WaitCombat without being drafted.");
                        this.ReadyForNextToil();
                        return;
                    }
                    if ((Find.TickManager.TicksGame + this.pawn.thingIDNumber) % 4 == 0)
                    {
                    }

                },
                defaultCompleteMode = ToilCompleteMode.Never
            };
        }
    }
}