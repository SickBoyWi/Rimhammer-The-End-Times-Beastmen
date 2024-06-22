using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public class JobDriver_JoinPitFight : JobDriver
    {
        private const TargetIndex PitIndex = TargetIndex.A;
        private const TargetIndex StandIndex = TargetIndex.B;

        protected Building_PitFightSpot PitRef
        {
            get
            {
                return (Building_PitFightSpot)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected IntVec3 StandPosition
        {
            get
            {
                return (IntVec3)this.job.GetTarget(TargetIndex.B);
            }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull<JobDriver_JoinPitFight>(TargetIndex.A);
            
            this.FailOn<JobDriver_JoinPitFight>((Func<bool>)(() =>
            {
                if (this.PitRef.DestroyedOrNull())
                {
                    return true;
                }
                return this.PitRef.currentState == Building_PitFightSpot.State.resting;
            }));

            this.AddFinishAction(jobCondition =>
            {
                try
                {
                    if (this.PitRef == null || this.pawn == null || this.PitRef.GetFighter(this.pawn) == null)
                        return;
                }
                catch
                {
                    return;
                }
                if (this.PitRef.GetFighter(this.pawn).isInFight)
                    return;
                this.PitRef.TryCancelFight("");
            });

            yield return new Toil()
            {
                initAction = (Action)(() =>
                {
                    if (this.PitRef.currentState != Building_PitFightSpot.State.scheduled)
                        return;
                    this.PitRef.currentState = Building_PitFightSpot.State.preparation;
                    this.PitRef.startTheFightWatching();
                })
            };

            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            
            // Show boat.
            Toil chantingTime = new Toil();
            chantingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            chantingTime.defaultDuration = 400;
            chantingTime.PlaySustainerOrSound(BeastmenDefOf.RH_TET_Beastmen_Braying);
            yield return chantingTime;

            yield return new Toil()
            {
                initAction = (Action)(() =>
                {
                    Building_PitFightSpot.State currState = this.PitRef.currentState;
                    if (this.PitRef.Destroyed)
                        return;
                    
                    this.PitRef.FighterReady(this.pawn);
                    
                    // Something makes this.PitRef null by the time it gets here. Every time.
                    if (currState != Building_PitFightSpot.State.fighting)
                    {
                        this.pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Wait)
                        {
                            count = 1
                        }, JobTag.Misc);
                    }
                }),
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
}
