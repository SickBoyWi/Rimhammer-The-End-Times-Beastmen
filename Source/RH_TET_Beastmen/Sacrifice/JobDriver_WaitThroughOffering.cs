using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    class JobDriver_WaitThroughOffering : JobDriver
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
                if (this.pawn == herdstone.shaman)
                {
                    yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
                }
            }

            //Toil 1 Celebrate or recoil
            yield return new Toil
            {
                initAction = delegate
                {
                    //Do something? Ia ia!
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            //Toil 2 Reflect on worship
            Toil reflectingTime = new Toil();
            reflectingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            reflectingTime.defaultDuration = HerdUtility.offeringAwaitDuration;
            yield return reflectingTime;

            //Toil 3 Reset the altar and clear variables.
            yield return new Toil
            {
                initAction = delegate
                {
                    if (herdstone != null)
                    {
                        if (herdstone.currentOfferingState == Building_MassiveHerdstone.OfferingState.offering)
                        {
                            herdstone.ChangeState(Building_MassiveHerdstone.State.offering, Building_MassiveHerdstone.OfferingState.finished);
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

    }
}
