using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheEndTimes_Magic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Beastmen
{
    public class JobDriver_HoldSacrifice : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private const TargetIndex TakeeIndex = TargetIndex.A;
        private const TargetIndex herdstoneIndex = TargetIndex.B;

        protected Pawn Takee
        {
            get { return (Pawn)base.job.GetTarget(TargetIndex.A).Thing; }
        }

        protected Building_MassiveHerdstone DropHerdstone
        {
            get { return (Building_MassiveHerdstone)base.job.GetTarget(TargetIndex.B).Thing; }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalState(TargetIndex.A);

            yield return Toils_Reserve.Reserve(TakeeIndex, 1);
            yield return Toils_Reserve.Reserve(herdstoneIndex, Building_MassiveHerdstone.LyingSlotsCount);

            yield return new Toil
            {
                initAction = delegate
                {
                    DropHerdstone.ChangeState(Building_MassiveHerdstone.State.sacrificing,
                        Building_MassiveHerdstone.SacrificeState.gathering);
                }
            };

            //Toil 1: Get victim.
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOn(() => this.job.def == JobDefOf.Arrest && !this.Takee.CanBeArrestedBy(this.pawn))
                .FailOn(() =>
                    !this.pawn.CanReach(this.DropHerdstone, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    if (this.job.def.makeTargetPrisoner)
                    {
                        Pawn pawn = (Pawn)this.job.targetA.Thing;
                        Lord lord = pawn.GetLord();
                        if (lord != null)
                        {
                            lord.Notify_PawnAttemptArrested(pawn);
                        }
                        GenClamor.DoClamor(pawn, 10f, ClamorDefOf.Harm);
                        if (this.job.def == JobDefOf.Arrest && !pawn.CheckAcceptArrest(this.pawn))
                        {
                            this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        }
                    }
                }
            };

            //Toil 2: Carry victim.
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            //Toil 3: Go to herdstone.
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);

            //Toil 4: Release victim.
            yield return Toils_Reserve.Release(TargetIndex.B);

            //Toil 5: Restrain victim.
            yield return new Toil
            {
                initAction = delegate
                {
                    //In-case this fails...
                    IntVec3 position = this.DropHerdstone.Position;
                    Thing thing;
                    this.pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out thing, null);
                    if (!this.DropHerdstone.Destroyed && (this.DropHerdstone.AnyUnoccupiedLyingSlot))
                    {
                        this.Takee.Position = DropHerdstone.GetLyingSlotPos();
                        this.Takee.Notify_Teleported(false);
                        this.Takee.stances.CancelBusyStanceHard();
                        Job job = new Job(BeastmenDefOf.RH_TET_Beastmen_AwaitDeath, DropHerdstone);
                        this.Takee.jobs.StartJob(job);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            //Toil 6: Bray.
            Toil chantingTime = new Toil();
            chantingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            chantingTime.defaultDuration = HerdUtility.ritualDuration;
            chantingTime.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            chantingTime.PlaySustainerOrSound(BeastmenDefOf.RH_TET_Beastmen_Braying);
            Texture2D godSymbol = ((DarkGodDef)DropHerdstone.SacrificeData.God.def).Symbol;
            chantingTime.initAction = delegate
            {
                if (godSymbol != null)
                    MoteMaker.MakeInteractionBubble(this.pawn, null, ThingDefOf.Mote_Speech, godSymbol);
                
                DropHerdstone.ChangeState(Building_MassiveHerdstone.State.sacrificing,
                    Building_MassiveHerdstone.SacrificeState.sacrificing);
            };

            yield return chantingTime;

            //Toil 8: Execution.
            yield return new Toil
            {
                initAction = delegate
                {
                    this.Takee.TakeDamage(new DamageInfo(DamageDefOf.ExecutionCut, 99999, 0f, -1f, this.pawn,
                        TheEndTimes_Beastmen.Utility.GetHeart(this.Takee.health.hediffSet)));
                    if (!this.Takee.Dead)
                    {
                        this.Takee.Kill(null);
                    }

                    TaleRecorder.RecordTale(TaleDefOf.ExecutedPrisoner, new object[]
                    {
                        this.pawn,
                        this.Takee
                    });
                    HerdUtility.SacrificeExecutionComplete(DropHerdstone);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            this.AddFinishAction(() =>
            {
                TaleDef taleToAdd = TaleDef.Named("RH_TET_MadeSacrifice");
                if ((this.pawn.IsColonist || this.pawn.HostFaction == Faction.OfPlayer) && taleToAdd != null)
                {
                    TaleRecorder.RecordTale(taleToAdd, new object[]
                    {
                        this.pawn,
                    });
                }
                
                DropHerdstone.ChangeState(Building_MassiveHerdstone.State.notinuse,
                    Building_MassiveHerdstone.SacrificeState.finished);
            });
        }
    }
}