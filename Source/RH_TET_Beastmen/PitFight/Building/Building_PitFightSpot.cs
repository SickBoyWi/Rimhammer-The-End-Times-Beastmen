using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Beastmen
{
    public class Building_PitFightSpot : Building, IBillGiver
    {
        public Building_PitFightSpot.State currentState = Building_PitFightSpot.State.resting;
        public PitFighter fighter1 = new PitFighter();
        public PitFighter fighter2 = new PitFighter();
        protected int wickTicksLeft = 0;
        public bool winnerGetsFreedom = false;
        public bool toDeath = false;
        private bool destroyedFlag = false;
        private Area fightingArea_int;

        public Area FightingArea
        {
            get
            {
                if (this.fightingArea_int != null && this.fightingArea_int.Map != this.MapHeld)
                    return (Area)null;
                return this.fightingArea_int;
            }
            set
            {
                this.fightingArea_int = value;
            }
        }

        public IEnumerable<IntVec3> IngredientStackCells
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BillStack BillStack
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Building_PitFightSpot.State>(ref this.currentState, "currentState", Building_PitFightSpot.State.resting, false);
            Scribe_References.Look<Pawn>(ref this.fighter1.p, "fighter1p", false);
            Scribe_References.Look<Pawn>(ref this.fighter2.p, "fighter2p", false);
            Scribe_Values.Look<bool>(ref this.fighter1.isInFight, "fighter2f", false, false);
            Scribe_Values.Look<bool>(ref this.fighter2.isInFight, "fighter2f", false, false);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            this.destroyedFlag = true;
            base.Destroy(mode);
        }

        public void Fight()
        {
            if (this.IsBusy())
            {
                switch (this.currentState)
                {
                    case Building_PitFightSpot.State.preparation:
                        Messages.Message("RH_TET_Beastmen_FightAlreadyHeld".Translate(), MessageTypeDefOf.RejectInput, true);
                        return;
                    case Building_PitFightSpot.State.fighting:
                        Messages.Message("RH_TET_Beastmen_FightAlreadyProcess".Translate(), MessageTypeDefOf.RejectInput, true);
                        return;
                }
            }
            
            if (this.fighter1.p == null || this.fighter2.p == null)
                Messages.Message("RH_TET_Beastmen_SelectTwoFighters".Translate(), MessageTypeDefOf.RejectInput, true);
            else if (this.fighter1.p == this.fighter2.p)
                Messages.Message("RH_TET_Beastmen_FightSelf".Translate(), MessageTypeDefOf.RejectInput, true);
            else if (!this.fightCapable(this.fighter1.p))
                Messages.Message(this.fighter1.p.Name.ToStringShort + " " + "RH_TET_Beastmen_FighterCantFight".Translate(), MessageTypeDefOf.RejectInput, true);
            else if (!this.fightCapable(this.fighter2.p))
                Messages.Message(this.fighter2.p.Name.ToStringShort + " " +  "RH_TET_Beastmen_FighterCantFight".Translate(), MessageTypeDefOf.RejectInput, true);
            else
            { 
                this.currentState = Building_PitFightSpot.State.scheduled;
                
                this.fighter1.p.jobs.ClearQueuedJobs();
                this.fighter2.p.jobs.ClearQueuedJobs();

                this.fighter1.p.jobs.TryTakeOrderedJob(new Job(BeastmenDefOf.RH_TET_Beastmen_JoinPitFightJob, (LocalTargetInfo)((Thing)this), (LocalTargetInfo)this.getFighterStandPoint(this.fighter1.p))
                {
                    count = 1
                }, JobTag.Misc);
                this.fighter2.p.jobs.TryTakeOrderedJob(new Job(BeastmenDefOf.RH_TET_Beastmen_JoinPitFightJob, (LocalTargetInfo)((Thing)this), (LocalTargetInfo)this.getFighterStandPoint(this.fighter2.p))
                {
                    count = 1
                }, JobTag.Misc);
            }
        }

        public void TryCancelFight(string reason = "")
        {
            this.currentState = Building_PitFightSpot.State.resting;
            Messages.Message("RH_TET_Beastmen_FightCancelled".Translate() + reason, MessageTypeDefOf.NegativeEvent, true);
            this.fighter1 = new PitFighter();
            this.fighter2 = new PitFighter();
        }

        private void startFightingState(PitFighter f)
        {
            f.p.mindState.enemyTarget = (Thing)this.getOtherFighter(f).p;
            f.p.jobs.StopAll(false);
            f.p.mindState.mentalStateHandler.Reset();

            MentalStateHandler mentalStateHandler = f.p.mindState.mentalStateHandler;
            MentalStateDef fighterState = BeastmenDefOf.RH_TET_Beastmen_PitFighter;

            Pawn p1 = f.p;
            MentalStateDef stateDef = fighterState;
            Pawn p2 = this.getOtherFighter(f).p;
            mentalStateHandler.TryStartMentalState(stateDef, "", false, false, (Pawn)null, true);
            
            // This will tell the pawn to quit when the opponent is downed or dead.
            MentalState_PitFighter mentalState = f.p.MentalState as MentalState_PitFighter;
            mentalState.otherPawn = this.getOtherFighter(f).p;
            mentalState.pitFightingSpot = this;

        }

        public void EndFight(Pawn pawn = null, bool suspended = false)
        {
            this.currentState = Building_PitFightSpot.State.resting;
            Pawn pawn1 = (Pawn)null;
            Pawn pawn2 = (Pawn)null;
            if (suspended)
            {
                Messages.Message("RH_TET_Beastmen_FightSuspended".Translate(), MessageTypeDefOf.RejectInput, true);
            }
            else
            {
                Messages.Message("RH_TET_Beastmen_FightWinner".Translate() + " " + pawn.Name.ToStringShort + "!", MessageTypeDefOf.RejectInput, true);
                if (pawn != null && pawn == this.fighter1.p)
                {
                    pawn1 = this.fighter1.p;
                    if (this.fighter2 != null)
                        pawn2 = this.fighter2.p;
                }
                else if (pawn != null && pawn == this.fighter2.p)
                {
                    pawn1 = this.fighter2.p;
                    if (this.fighter1 != null)
                        pawn2 = this.fighter1.p;
                }
                
                if (pawn1 != null && !pawn1.Dead)
                {
                    pawn1.jobs.ClearQueuedJobs();
                    pawn1.needs.mood.thoughts.memories.TryGainMemory(BeastmenDefOf.RH_TET_Beastmen_PitFightWinner, (Pawn)null);
                }
                if (pawn2 != null && !pawn2.Dead)
                {
                    pawn2.jobs.ClearQueuedJobs();
                    pawn2.needs.mood.thoughts.memories.TryGainMemory(BeastmenDefOf.RH_TET_Beastmen_PitFightLoser, (Pawn)null);
                }
            }

            this.fighter1 = new PitFighter();
            this.fighter2 = new PitFighter();
        }

        public void FighterReady(Pawn p)
        {
            this.GetFighter(p).isInFight = true;
            //this.startFightingState(this.GetFighter(p));
            
            if (!this.fighter1.isInFight || !this.fighter2.isInFight)
                return;
            Messages.Message("RH_TET_Beastmen_FightStarting".Translate(), MessageTypeDefOf.RejectInput, true);
            this.currentState = Building_PitFightSpot.State.fighting;
            this.startFightingState(this.fighter1);
            this.startFightingState(this.fighter2);
        }

        public void startTheFightWatching()
        {
            LordMaker.MakeNewLord(this.Faction, (LordJob)new LordJob_Joinable_PitFight(this.Position, this), this.Map, (IEnumerable<Pawn>)null);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.Append("Current state: ");
            stringBuilder.Append(this.currentState.ToString());
            return stringBuilder.ToString();
        }

        public bool CurrentlyUsableForBills()
        {
            throw new NotImplementedException();
        }

        public bool UsableForBillsAfterFueling()
        {
            throw new NotImplementedException();
        }

        private bool fightCapable(Pawn p)
        {
            return p.health.capacities.CapableOf(PawnCapacityDefOf.Moving) && !p.health.InPainShock;
        }

        private bool IsBusy()
        {
            if (!this.IsInFight())
                return this.IsPreparing();
            return true;
        }

        private bool IsInFight()
        {
            return this.currentState == Building_PitFightSpot.State.fighting;
        }

        private bool IsPreparing()
        {
            return this.currentState == Building_PitFightSpot.State.preparation;
        }

        private bool IsFighter1(Pawn p)
        {
            return this.fighter1.p.Equals(p);
        }

        public IntVec3 getFighterStandPoint(Pawn p)
        {
            CompFightingPit comp = this.GetComp<CompFightingPit>();
            CellRect cellRect = CellRect.CenteredOn(this.Position, 1);
            cellRect = cellRect.ExpandedBy(Mathf.RoundToInt(comp.radius - 1f));
            IEnumerable<IntVec3> corners = cellRect.Corners;
            
            if (this.IsFighter1(p))
                return corners.First<IntVec3>();
            return corners.Last<IntVec3>();
        }

        private IntVec3 getCorner(bool nearest)
        {
            if (nearest)
                return this.fightingArea_int.ActiveCells.First<IntVec3>();
            return this.fightingArea_int.ActiveCells.Last<IntVec3>();
        }

        public PitFighter GetFighter(Pawn p)
        {
            if (p == this.fighter1.p)
                return this.fighter1;
            if (p == this.fighter2.p)
                return this.fighter2;
            return (PitFighter)null;
        }

        public Pawn getFighterForHaul()
        {
            if (!this.fighter1.isInFight)
                return this.fighter1.p;
            return this.fighter2.p;
        }

        public PitFighter getOtherFighter(PitFighter f)
        {
            if (f.p == this.fighter1.p)
                return this.fighter2;
            return this.fighter1;
        }

        public enum State
        {
            resting,
            preparation,
            fighting,
            scheduled,
        }
        public virtual void Notify_BillDeleted(Bill bill)
        {
        }
    }
}
