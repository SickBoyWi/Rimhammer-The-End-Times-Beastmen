using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public class JobGiver_PitFighter : JobGiver_AIFightEnemy
    {
        private const float MaxAttackDistance = 900f;
        private const float WaitChance = 0.5f;
        private const int WaitTicks = 1;
        private const int MinMeleeChaseTicks = 420;
        private const int MaxMeleeChaseTicks = 900;

        protected override Job TryGiveJob(Pawn pawn)
        {
            MentalState_PitFighter mentalState = (MentalState_PitFighter)pawn.MentalState;
            Pawn otherPawn = mentalState.otherPawn;
            Building_PitFightSpot pitRef = mentalState.pitFightingSpot;

            if ((double)Rand.Value < 0.5)
                return new Job(JobDefOf.Wait_Combat)
                {
                    expiryInterval = 10
                };
            if (pawn.TryGetAttackVerb((Thing)null, false) == null)
                return (Job)null;

            if (otherPawn == null 
                    || !pawn.CanReach((LocalTargetInfo)((Thing)otherPawn), PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                return new Job(JobDefOf.Wait);

            pawn.mindState.enemyTarget = (Thing)otherPawn;
            this.UpdateEnemyTarget(pawn);

            Thing enemyTarget = pawn.mindState.enemyTarget;
            if (enemyTarget == null)
                return (Job)null;

            // Melee only for pit fight.
            Verb attackVerb = pawn.meleeVerbs.TryGetMeleeVerb(enemyTarget);

            if (attackVerb == null)
                return (Job)null;

            if (attackVerb.verbProps.IsMeleeAttack)
                return this.MeleeAttackJob(pawn, enemyTarget);

            return new Job(JobDefOf.Wait);
        }

        protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
        {
            dest = IntVec3.Invalid;
            return false;
        }

        protected override void UpdateEnemyTarget(Pawn pawn)
        {
            Pawn otherPawn = ((MentalState_PitFighter)pawn.MentalState).otherPawn;
            pawn.mindState.enemyTarget = (Thing)otherPawn;
        }
    }
}
