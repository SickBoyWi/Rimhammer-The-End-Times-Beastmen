using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    class JobDriver_ReflectOnResult : JobDriver
    {

        protected Building_MassiveHerdstone herdstone
        {
            get
            {
                return (Building_MassiveHerdstone)base.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Toil 0 -- Go here first to reflect.
            if (herdstone != null)
            {
                if (herdstone?.SacrificeData?.Executioner != null)
                {
                    if (this.pawn == herdstone.SacrificeData.Executioner)
                    {
                        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
                    }
                }
            }

            //Toil 9 Celebrate or recoil
            yield return new Toil
            {
                initAction = delegate
                {
                    if (Map.GetComponent<MapComponent_SacrificeTracker>().lastResult == HerdUtility.SacrificeResult.success)
                    {
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            //Toil 10 Reflect on result
            Toil reflectingTime = new Toil();
            reflectingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            reflectingTime.defaultDuration = HerdUtility.offeringAwaitDuration;
            yield return reflectingTime;

            //Toil 11 Reset the herdstone and clear variables.
            yield return new Toil
            {
                initAction = delegate
                {
                    if (herdstone != null)
                    {
                        if (herdstone.currentSacrificeState != Building_MassiveHerdstone.SacrificeState.finished)
                        {
                            herdstone.ChangeState(Building_MassiveHerdstone.State.sacrificing, Building_MassiveHerdstone.SacrificeState.finished);
                            Map.GetComponent<MapComponent_SacrificeTracker>().ClearSacrificeVariables();
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
