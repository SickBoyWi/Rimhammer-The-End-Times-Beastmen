using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public class MentalState_PitFighter : MentalState
    {
        public Pawn otherPawn;
        public Building_PitFightSpot pitFightingSpot;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Building_PitFightSpot>(ref this.pitFightingSpot, "pitFightingSpot", false);
            Scribe_References.Look<Pawn>(ref this.otherPawn, "otherPawn", false);
        }

        private bool ShouldStop
        {
            get
            {
                return this.otherPawn == null || this.otherPawn.Dead || this.otherPawn.Downed || this.pitFightingSpot == null || this.pitFightingSpot.currentState == Building_PitFightSpot.State.resting;
            }
        }

        public override void MentalStateTick(int delta)
        {
            if (this.ShouldStop)
            {
                this.RecoverFromState();
                if (this.otherPawn != null && !this.otherPawn.Downed && !this.otherPawn.Dead)
                    return;
                this.pitFightingSpot.EndFight(this.pawn, false);
            }
            else
                base.MentalStateTick(delta);
        }
    }
}
