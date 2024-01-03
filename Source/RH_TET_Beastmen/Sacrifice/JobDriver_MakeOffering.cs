using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheEndTimes_Magic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public class JobDriver_MakeOffering : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        public const TargetIndex BillGiverInd = TargetIndex.A;

        public const TargetIndex IngredientInd = TargetIndex.B;

        public const TargetIndex IngredientPlaceCellInd = TargetIndex.C;

        public List<Thing> offerings = null;

        public float workLeft;

        public int billStartTick;

        public int ticksSpentDoingRecipeWork;

        public IBillGiver BillGiver
        {
            get
            {
                IBillGiver billGiver = this.pawn.jobs.curJob.GetTarget(TargetIndex.A).Thing as IBillGiver;
                if (billGiver == null)
                {
                    throw new InvalidOperationException("DoBill on non-Billgiver.");
                }
                return billGiver;
            }
        }

        public Building_MassiveHerdstone DropHerdstone
        {
            get
            {
                Building_MassiveHerdstone result = this.pawn.jobs.curJob.GetTarget(TargetIndex.A).Thing as Building_MassiveHerdstone;
                if (result == null)
                {
                    throw new InvalidOperationException("Herdstone is missing.");
                }
                return result;
            }
        }

        public override string GetReport()
        {
            return this.pawn.jobs.curJob.RecipeDef != null ? base.ReportStringProcessed(this.pawn.jobs.curJob.RecipeDef.jobString) : base.GetReport();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
            Scribe_Collections.Look(ref this.offerings, "offerings", LookMode.Reference);
            Scribe_Values.Look<int>(ref this.billStartTick, "billStartTick", 0, false);
            Scribe_Values.Look<int>(ref this.ticksSpentDoingRecipeWork, "ticksSpentDoingRecipeWork", 0, false);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.AddEndCondition(delegate
            {
                Thing thing = this.GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
                if (thing is Building && !thing.Spawned)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });

            this.FailOnBurningImmobile(TargetIndex.A);
            
            Toil toil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return new Toil { initAction = delegate { offerings = new List<Thing>(); } };
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Reserve.ReserveQueue(TargetIndex.B, 1);
            yield return new Toil
            {
                initAction = delegate
                {
                    if (this.job.targetQueueB != null && this.job.targetQueueB.Count == 1)
                    {
                        UnfinishedThing unfinishedThing = this.job.targetQueueB[0].Thing as UnfinishedThing;
                        if (unfinishedThing != null)
                        {
                            unfinishedThing.BoundBill = (Bill_ProductionWithUft)this.job.bill;
                        }
                    }
                }
            };
            yield return Toils_Jump.JumpIf(toil, () => this.job.GetTargetQueue(TargetIndex.B).NullOrEmpty<LocalTargetInfo>());

            Toil toil2 = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B, false);
            yield return toil2;

            Toil toil3 = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return toil3;
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, true, false);
            yield return JumpToCollectNextIntoHandsForBill(toil3, TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDestroyedOrNull(TargetIndex.B);

            Toil toil4 = Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
            yield return toil4;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, toil4, false);
            yield return new Toil
            {
                initAction = delegate {
                    if (offerings.Count > 0)
                    {
                        offerings.RemoveAll(x => x.DestroyedOrNull());
                    }
                    offerings.Add(TargetB.Thing);
                }
            };

            yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, toil2);
            yield return toil;
            
            Toil chantingTime = new Toil();
            chantingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            chantingTime.defaultDuration = HerdUtility.ritualDuration;
            chantingTime.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            chantingTime.PlaySustainerOrSound(BeastmenDefOf.RH_TET_Beastmen_Braying);
            Texture2D godSymbol = ((DarkGodDef)DropHerdstone.currentOfferingGod.def).Symbol;
            chantingTime.initAction = delegate
            {
                if (godSymbol != null)
                    MoteMaker.MakeInteractionBubble(this.pawn, null, ThingDefOf.Mote_Speech, godSymbol);
            };
            yield return chantingTime;
            
            //Toil 8: Execution of Prisoner
            yield return new Toil
            {
                initAction = delegate
                {
                    HerdUtility.OfferingComplete(this.pawn, DropHerdstone, DropHerdstone.currentOfferingGod, offerings);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            
            yield break;
        }

        private static Toil ToilLogMessage(string message)
        {
            return new Toil { initAction = delegate { Log.Message(message); } };
        }

        private static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                if (actor.carryTracker.CarriedThing == null)
                {
                    Log.Error("JumpToAlsoCollectTargetInQueue run on " + actor + " who is not carrying something.");
                    return;
                }
                if (actor.carryTracker.Full)
                {
                    return;
                }
                Job curJob = actor.jobs.curJob;
                List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
                if (targetQueue.NullOrEmpty<LocalTargetInfo>())
                {
                    return;
                }
                for (int i = 0; i < targetQueue.Count; i++)
                {
                    if (GenAI.CanUseItemForWork(actor, targetQueue[i].Thing))
                    {
                        if (targetQueue[i].Thing.CanStackWith(actor.carryTracker.CarriedThing))
                        {
                            if ((actor.Position - targetQueue[i].Thing.Position).LengthHorizontalSquared <= 64f)
                            {
                                int num = (actor.carryTracker.CarriedThing != null) ? actor.carryTracker.CarriedThing.stackCount : 0;
                                int num2 = curJob.countQueue[i];
                                num2 = Mathf.Min(num2, targetQueue[i].Thing.def.stackLimit - num);
                                num2 = Mathf.Min(num2, actor.carryTracker.AvailableStackSpace(targetQueue[i].Thing.def));
                                if (num2 > 0)
                                {
                                    curJob.count = num2;
                                    curJob.SetTarget(ind, targetQueue[i].Thing);
                                    List<int> countQueue;
                                    List<int> expr_1B2 = countQueue = curJob.countQueue;
                                    int num3;
                                    int expr_1B6 = num3 = i;
                                    num3 = countQueue[num3];
                                    expr_1B2[expr_1B6] = num3 - num2;
                                    if (curJob.countQueue[i] == 0)
                                    {
                                        curJob.countQueue.RemoveAt(i);
                                        targetQueue.RemoveAt(i);
                                    }
                                    actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
                                    return;
                                }
                            }
                        }
                    }
                }
            };
            return toil;
        }
    }
}
