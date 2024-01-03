using RimWorld;
using System;
using Verse;
using Verse.AI.Group;

namespace TheEndTimes_Beastmen
{
    public class LordJob_Joinable_PitFight : LordJob_VoluntarilyJoinable
    {
        private IntVec3 spot;
        private Building_PitFightSpot pitFightingSpot;

        public LordJob_Joinable_PitFight()
        {
        }

        public LordJob_Joinable_PitFight(IntVec3 spot, Building_PitFightSpot pitFightSpot)
        {
            this.spot = spot;
            this.pitFightingSpot = pitFightSpot;
        }

        public override bool AllowStartNewGatherings
        {
            get
            {
                return false;
            }
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_PitFight toilFightingMatch = new LordToil_PitFight(this.spot, this.pitFightingSpot);
            stateGraph.AddToil((LordToil)toilFightingMatch);
            LordToil_End lordToilEnd = new LordToil_End();
            stateGraph.AddToil((LordToil)lordToilEnd);
            Transition transition = new Transition((LordToil)toilFightingMatch, (LordToil)lordToilEnd, false, true);
            transition.AddTrigger((Trigger)new Trigger_TickCondition((Func<bool>)(() => this.pitFightingSpot.currentState == Building_PitFightSpot.State.resting), 1));
            stateGraph.AddTransition(transition, false);
            return stateGraph;
        }

        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            if (this.IsInvited(p))
                return VoluntarilyJoinableLordJobJoinPriorities.MarriageCeremonyGuest;
            return 0.0f;
        }

        public override string GetReport(Pawn pawn)
        {
            return (string)"RH_TET_Beastmen_HavingFun".Translate();
        }

        private bool IsInvited(Pawn p)
        {
            if (this.lord.faction != null && (!p.Equals(pitFightingSpot.fighter1.p) && !p.Equals(pitFightingSpot.fighter2.p)))
                return p.Faction == this.lord.faction;
            return false;
        }

        public override void ExposeData()
        {
            Scribe_References.Look<Building_PitFightSpot>(ref this.pitFightingSpot, "pitFightingSpot", false);
            Scribe_Values.Look<IntVec3>(ref this.spot, "spot", new IntVec3(), false);
        }
    }
}
