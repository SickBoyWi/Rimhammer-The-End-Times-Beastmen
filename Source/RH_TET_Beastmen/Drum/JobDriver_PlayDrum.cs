using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public class JobDriver_PlayDrum : JobDriver_SitFacingBuilding
    {
        public Building_Drum Drum
        {
            get
            {
                return (Building_Drum)(Thing)this.job.GetTarget(TargetIndex.A);
            }
        }

        protected override void ModifyPlayToil(Toil toil)
        {
            if (!this.pawn.CanReserve(this.Drum))
                return;
            base.ModifyPlayToil(toil);
            toil.AddPreInitAction((Action)(() => this.Drum.StartPlaying(this.pawn)));
            toil.AddFinishAction((Action)(() => this.Drum.StopPlaying()));
        }
    }
}