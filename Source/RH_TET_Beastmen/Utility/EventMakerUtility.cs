using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    public static class EventMakerUtility
    {
        public static void SpawnRaidEvent (Building_MassiveHerdstone herdstone, FloatRange strengthRange)
        {
            Map map = herdstone.Map;
            Faction enemyFac;
            IntVec3 spawnSpot;

            if (!TryFindSpawnSpot(map, out spawnSpot) || !TryFindEnemyFaction(out enemyFac))
                return;

            int num = Rand.Int;
            float raidStrengthFactor = strengthRange.RandomInRange;
            IncidentCategoryDef cat = IncidentCategoryDefOf.ThreatSmall;
            if (raidStrengthFactor >= 1)
            {
                cat = IncidentCategoryDefOf.ThreatBig;
            }
            
            IncidentParms incidentParams = StorytellerUtility.DefaultParmsNow(cat, (IIncidentTarget)map);
            incidentParams.forced = true;
            incidentParams.faction = enemyFac;
            incidentParams.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            incidentParams.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            incidentParams.spawnCenter = spawnSpot;
            incidentParams.points = Mathf.Max(StorytellerUtility.DefaultThreatPointsNow(herdstone.Map ?? (IIncidentTarget)Find.World) * raidStrengthFactor, enemyFac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
            incidentParams.pawnGroupMakerSeed = new int?(num);

            PawnGroupMakerParms pawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParams, false);
            pawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(pawnGroupMakerParms.points, incidentParams.raidArrivalMode, incidentParams.raidStrategy, pawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat);
            
            Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, (StorytellerComp)null, incidentParams), Find.TickManager.TicksGame + 1000, 0));
        }

        public static void SpawnChasedRefugeeEvent (Building_MassiveHerdstone herdstone)
        {
            if (RH_TET_BeastmenMod.random.Next(0, 1) == 0)
            {
                QuestScriptDef refugeeChased = DefDatabase<QuestScriptDef>.GetNamed("ThreatReward_Raid_Joiner", true);
                Slate vars = new Slate();
                vars.Set<float>("points", StorytellerUtility.DefaultThreatPointsNow(herdstone.Map ?? (IIncidentTarget)Find.World), false);
                vars.Set<Faction>("enemyFaction", Find.FactionManager.RandomEnemyFaction(), false);
                Quest andMakeAvailable = QuestUtility.GenerateQuestAndMakeAvailable(refugeeChased, vars);
                Find.LetterStack.ReceiveLetter((TaggedString)andMakeAvailable.name, andMakeAvailable.description, LetterDefOf.NeutralEvent, (LookTargets)null, (Faction)null, andMakeAvailable, (List<ThingDef>)null, (string)null);
            }
            else
            { 
                IncidentDef incident = BeastmenDefOf.RH_TET_Beastmen_RefugeesChased;
     
                Map map = herdstone.Map;
                IncidentParms incidentParams = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, (IIncidentTarget)map);
                IntVec3 spawnSpot;

                if (!TryFindSpawnSpot(map, out spawnSpot))
                    return;

                incidentParams.spawnCenter = spawnSpot;

                Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(incident, (StorytellerComp)null, incidentParams), Find.TickManager.TicksGame + 500, 0));
            }
        }

        public static void SpawnWandererJoin (Building_MassiveHerdstone herdstone)
        {
            Map map = herdstone.Map;
            IncidentParms incidentParams = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, (IIncidentTarget)map);
            IntVec3 spawnSpot;

            if (!TryFindSpawnSpot(map, out spawnSpot))
                return;

            incidentParams.spawnCenter = spawnSpot;

            Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(BeastmenDefOf.RH_TET_Beastmen_WandererJoin, (StorytellerComp)null, incidentParams), Find.TickManager.TicksGame + 500, 0));
        }

        public static void SpawnAnimalsJoin(Building_MassiveHerdstone herdstone)
        {
            IncidentDef incident = BeastmenDefOf.RH_TET_Beastmen_MonsterJoin;

            Map map = herdstone.Map;
            IncidentParms incidentParams = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, (IIncidentTarget)map);
            IntVec3 spawnSpot;

            if (!TryFindSpawnSpot(map, out spawnSpot))
                return;

            incidentParams.spawnCenter = spawnSpot;

            Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(incident, (StorytellerComp)null, incidentParams), Find.TickManager.TicksGame + 500, 0));
        }

        private static bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot)
        {
            return CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)(c =>
            {
                if (map.reachability.CanReachColony(c))
                    return !c.Fogged(map);
                return false;
            }), map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }

        private static bool TryFindEnemyFaction(out Faction enemyFac)
        {
            return Find.FactionManager.AllFactions.Where<Faction>((Func<Faction, bool>)(f =>
            {
                if (!f.def.hidden && !f.defeated)
                    return f.HostileTo(Faction.OfPlayer);
                return false;
            })).TryRandomElement<Faction>(out enemyFac);
        }

        public static void SpawnFinalDestructionEndGameQuest(Building_MassiveHerdstone herdstone)
        {
            Map map = herdstone.Map;
            Faction enemyFac = null;
            enemyFac = Find.FactionManager.AllFactions.Where<Faction>((Func<Faction, bool>)(f =>
            {
                if (!f.def.hidden && f.def.Equals(BeastmenDefOf.RH_TET_EmpireOfMan))
                    return f.HostileTo(Faction.OfPlayer);
                return false;
            })).First();

            if (enemyFac == null)
                return;

            IncidentCategoryDef cat = IncidentCategoryDefOf.ThreatBig;

            IncidentParms incidentParams = StorytellerUtility.DefaultParmsNow(cat, (IIncidentTarget)map);
            incidentParams.forced = true;
            incidentParams.faction = enemyFac;

            StorytellerComp_FinalDestruction stc = new StorytellerComp_FinalDestruction();

            Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(BeastmenDefOf.RH_TET_Beastmen_FinalDestruction, stc, incidentParams), Find.TickManager.TicksGame + 100, 0));
        }
    }
}
