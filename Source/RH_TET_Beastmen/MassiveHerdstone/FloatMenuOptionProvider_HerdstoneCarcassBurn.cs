using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public class FloatMenuOptionProvider_HerdstoneCarcassBurn : FloatMenuOptionProvider
    {
        protected override bool Drafted
        {
            get
            {
                return false;
            }
        }

        protected override bool Undrafted
        {
            get
            {
                return true;
            }
        }

        protected override bool Multiselect
        {
            get
            {
                return false;
            }
        }

        protected override bool RequiresManipulation
        {
            get
            {
                return true;
            }
        }

        protected override FloatMenuOption GetSingleOptionFor(
          Thing clickedThing,
          FloatMenuContext context)
        {
            Corpse p = clickedThing as Corpse;
            if (!this.CanBeBurnedByColony(clickedThing))
                return (FloatMenuOption)null;

            if (!context.FirstSelectedPawn.CanReach((LocalTargetInfo)clickedThing, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                return new FloatMenuOption((string)("RH_TET_Beastmen_CannotCarryToMassiveHerdstone".Translate((NamedArgument)clickedThing.LabelCap, (NamedArgument)clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst()), (Action)null, MenuOptionPriority.Default, (Action<Rect>)null, (Thing)null, 0.0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0);

            if (p != null)
            {
                return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"RH_TET_Beastmen_CarryToMassiveHerdstone".Translate((NamedArgument)clickedThing.LabelCap, (NamedArgument)clickedThing), 
                            (Action)(() =>
                            {
                                clickedThing.SetForbidden(false, false);
                                Job job1 = JobMaker.MakeJob(BeastmenDefOf.RH_TET_Beastmen_CarryToMassiveHerdstone, (LocalTargetInfo)clickedThing, Building_MassiveHerdstone.FindMassiveHerdstoneFor(p, context.FirstSelectedPawn, true, false));
                                job1.count = 1;
                                context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job1, new JobTag?(JobTag.Misc), false);
                                StrippableUtility.CheckSendStrippingImpactsGoodwillMessage(clickedThing);
                            }), 
                            MenuOptionPriority.Default, (Action<Rect>)null, (Thing)null, 0.0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0), context.FirstSelectedPawn, (LocalTargetInfo)clickedThing, "ReservedBy", (ReservationLayerDef)null);
            }
            return (FloatMenuOption)null;
        }

        private bool CanBeBurnedByColony(Thing th)
        {
            if (!(th is Corpse p))
                return false;
            if (p.InnerPawn.RaceProps.IsMechanoid)
                return false;
            if (p.InnerPawn.RaceProps.Humanlike)
                return true;
            return false;
        }
    }
}
