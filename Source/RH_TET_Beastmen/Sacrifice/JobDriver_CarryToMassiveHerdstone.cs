using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

/**
 * Original code from core. CarryToCryptoSleepBlahBlah.
 * */
namespace TheEndTimes_Beastmen
{
    public class JobDriver_CarryToMassiveHerdstone : JobDriver
    {
        private const TargetIndex TakeeInd = TargetIndex.A;
        private const TargetIndex MassiveHerdstoneInd = TargetIndex.B;

        protected Corpse Takee
        {
            get
            {
                return (Corpse)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Building_MassiveHerdstone MassiveHerdstoneBurnHole
        {
            get
            {
                return (Building_MassiveHerdstone)this.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.Takee;
            Job job = this.job;
            bool returnBool;
            if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                pawn = this.pawn;
                target = this.MassiveHerdstoneBurnHole;
                job = this.job;
                returnBool = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
            }
            else
            {
                returnBool = false;
            }
            return returnBool;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalState(TargetIndex.A);
            this.FailOn(() => !this.MassiveHerdstoneBurnHole.Accepts(this.Takee));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).
                    FailOnDespawnedNullOrForbidden(TargetIndex.B)
                        .FailOn(() => this.MassiveHerdstoneBurnHole.GetDirectlyHeldThings().Count > 0)
                        .FailOn(() => !this.pawn.CanReach(this.Takee, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
                        .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            Toil prepare = Toils_General.Wait(500, TargetIndex.None);
            prepare.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
            prepare.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            yield return prepare;
            yield return new Toil
            {
                initAction = delegate
                {
                    this.MassiveHerdstoneBurnHole.TryAcceptThing(this.Takee, true);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        public override object[] TaleParameters()
        {
            return new object[]
            {
                this.pawn,
                this.Takee.InnerPawn
            };
        }
    }
}
