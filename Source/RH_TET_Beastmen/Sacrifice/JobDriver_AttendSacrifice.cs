using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Beastmen
{
    public class JobDriver_AttendSacrifice : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private TargetIndex Build = TargetIndex.A;
        private TargetIndex Facing = TargetIndex.B;
        private TargetIndex Spot = TargetIndex.C;

        private Pawn setExecutioner = null;

        protected Building_MassiveHerdstone Herdstone
        {
            get
            {
                return (Building_MassiveHerdstone)base.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Pawn ExecutionerPawn
        {
            get
            {
                if (setExecutioner != null) return setExecutioner;

                if (Herdstone != null && Herdstone.SacrificeData != null && Herdstone.SacrificeData.Executioner != null)
                { 
                    setExecutioner = Herdstone.SacrificeData.Executioner; 
                    return Herdstone.SacrificeData.Executioner;
                }
                else if (Herdstone != null && Herdstone.ConversionData != null && Herdstone.ConversionData.Executioner != null)
                {
                    setExecutioner = Herdstone.ConversionData.Executioner;
                    return Herdstone.ConversionData.Executioner;
                }
                else
                {
                    foreach (Pawn pawn in this.pawn.Map.mapPawns.FreeColonistsSpawned)
                    {
                        if (pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_HoldSacrifice || pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_HoldConversion) { setExecutioner = pawn; return pawn; }
                    }
                }
                return null;
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look<Pawn>(ref this.setExecutioner, "setExecutioner");
            base.ExposeData();
        }


        private string report = "";
        public override string GetReport()
        {
            if (report != "")
            {
                return base.ReportStringProcessed(report);
            }
            return base.GetReport();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            rotateToFace = Facing;

            this.AddEndCondition(delegate
            {
                if (ExecutionerPawn == null)
                {
                    return JobCondition.Incompletable;
                }
                if (ExecutionerPawn.CurJob == null)
                {
                    return JobCondition.Incompletable;
                }

                if (ExecutionerPawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_WaitThroughOffering)
                {
                    return JobCondition.Succeeded;
                }
                else if (ExecutionerPawn.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_HoldSacrifice && ExecutionerPawn.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_HoldConversion)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });

            this.EndOnDespawnedOrNull(Spot, JobCondition.Incompletable);
            this.EndOnDespawnedOrNull(Build, JobCondition.Incompletable);


            yield return Toils_Reserve.Reserve(Spot, 1, -1);

            //Toil 1: Go to the locations
            Toil gotoExecutioner;
            if (this.TargetC.HasThing)
            {
                gotoExecutioner = Toils_Goto.GotoThing(Spot, PathEndMode.OnCell);
            }
            else
            {
                gotoExecutioner = Toils_Goto.GotoCell(Spot, PathEndMode.OnCell);
            }
            yield return gotoExecutioner;

            //Toil 2: 'Attend'
            var herdstoneToil = new Toil();
            herdstoneToil.defaultCompleteMode = ToilCompleteMode.Delay;
            herdstoneToil.defaultDuration = HerdUtility.ritualDuration;
            herdstoneToil.AddPreTickAction(() =>
            {
                this.pawn.GainComfortFromCellIfPossible();
                this.pawn.rotationTracker.FaceCell(TargetB.Cell);
                if (report == "") report = "RH_TET_Beastmen_AttendingSacrifice".Translate();
                if (ExecutionerPawn != null)
                {
                    if (ExecutionerPawn.CurJob != null)
                    {
                        if (ExecutionerPawn.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_HoldSacrifice && ExecutionerPawn.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_HoldConversion)
                        {
                            this.ReadyForNextToil();
                        }
                    }
                }
            });
            herdstoneToil.JumpIf(() => (ExecutionerPawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_HoldSacrifice || ExecutionerPawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_HoldConversion), herdstoneToil);
            yield return herdstoneToil;
            
            yield return new Toil
            {
                initAction = delegate
                {
                    //Do something?
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            //Toil 3 Reflect on worship
            Toil reflectingTime = new Toil();
            reflectingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            reflectingTime.defaultDuration = HerdUtility.offeringAwaitDuration;
            reflectingTime.AddPreTickAction(() =>
            {
                report = "RH_TET_Beastmen_ReflectingOnSacrifice".Translate();
            });
            yield return reflectingTime;

            //Toil 3 Reset the herdstone and clear variables.
            yield return new Toil
            {
                initAction = delegate
                {
                    if (Herdstone != null)
                    {
                        if (Herdstone.currentSacrificeState != Building_MassiveHerdstone.SacrificeState.finished)
                        {
                            Herdstone.ChangeState(Building_MassiveHerdstone.State.sacrificing, Building_MassiveHerdstone.SacrificeState.finished);
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };


            this.AddFinishAction(jobCondition =>
            {
                if (this.TargetC.Cell.GetEdifice(this.pawn.Map) != null)
                {
                    if (this.pawn.Map.reservationManager.ReservedBy(this.TargetC.Cell.GetEdifice(this.pawn.Map), this.pawn))
                        this.pawn.ClearAllReservations();
                }
                else
                {
                    if (this.pawn.Map.reservationManager.ReservedBy(this.TargetC.Cell.GetEdifice(this.pawn.Map), this.pawn))
                        this.pawn.ClearAllReservations(); 
                }
            });
        }
    }
}
