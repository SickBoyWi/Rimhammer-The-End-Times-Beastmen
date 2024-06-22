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
    public class JobDriver_HoldConversion : JobDriver
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
            chantingTime.initAction = delegate
            {
                DropHerdstone.ChangeState(Building_MassiveHerdstone.State.converting,
                    Building_MassiveHerdstone.SacrificeState.sacrificing);
            };

            yield return chantingTime;

            //Toil 8: Conversion.
            yield return new Toil
            {
                initAction = delegate
                {
                    int age = this.Takee.ageTracker.AgeBiologicalYears;
                    PawnGenerationRequest request = new PawnGenerationRequest(BeastmenDefOf.RH_TET_Beastmen_UngorBeastmenPlayer, null, PawnGenerationContext.NonPlayer, -1,
                        true, false, false,
                        false, false,  0.00f, 
                        true, true, false,
                        false, true, false,
                        false, false, false,
                        0.0f, 0.0f, null, 0.0f,
                        null, null, null,
                        null, null, age,
                        age, null, null,
                        null, null, null);

                    Pawn newPawn = PawnGenerator.GeneratePawn(request);
                    try
                    {
                        pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
                    }
                    catch
                    {
                        // Ignore if no ideos present.
                    }

                    IntVec3 position = this.Takee.Position;
                    Map map = this.Takee.Map;

                    // TODO JEH Move injuries from humanlike pawn to new ungor.
                    //if (newPawn.health.hediffSet != null)
                    //    newPawn.health.hediffSet.Clear();

                    //this.Takee.health.hediffSet.
                    //foreach (Hediff hediff in this.Takee.health.hediffSet.GetHediffs<Hediff>())
                    //    newPawn.health.AddHediff(hediff);THIS NEEDS A BODY PART, IT IS ERRORING.

                    newPawn.equipment.DestroyAllEquipment();
                    newPawn.apparel.DestroyAll();

                    this.Map.weatherManager.eventHandler.AddEvent((WeatherEvent)new WeatherEvent_LightningStrikeNoDamage(map, position));

                    // Kill target.
                    DamageInfo dinfo = new DamageInfo(RH_TET_MagicDefOf.RH_TET_MagicalInjury, 99999f, 999f, -1f, null, this.Takee.health.hediffSet.GetBrain(), (ThingDef)null, DamageInfo.SourceCategory.Collapse, (Thing)null);
                    //this.Takee.TakeDamage(dinfo);
                    if (!this.Takee.Dead)
                        this.Takee.Kill(new DamageInfo?(dinfo), (Hediff)null);

                    FleckMaker.Static(this.pawn.Position, this.pawn.Map, FleckDefOf.PsycastAreaEffect, 5f);

                    // Find target corpse and destroy.
                    this.Takee.Corpse.Destroy(DestroyMode.KillFinalize);

                    // Set new pawn faction.
                    newPawn.SetFaction(Faction.OfPlayer);

                    // Spawn new pawn.
                    GenSpawn.Spawn((Thing)newPawn, position, map, Rot4.Random, WipeMode.Vanish, false);

                    this.Map.weatherManager.eventHandler.AddEvent((WeatherEvent)new WeatherEvent_LightningStrikeNoDamage(map, position));

                    TaleRecorder.RecordTale(BeastmenDefOf.RH_TET_Beastmen_ConvertedPrisoner, new object[]
                    {
                        this.pawn,
                        this.Takee
                    });

                    HerdUtility.ConversionComplete(DropHerdstone);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            this.AddFinishAction(jobCondition =>
            {
                TaleDef taleToAdd = TaleDef.Named("RH_TET_MadeConversion");
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