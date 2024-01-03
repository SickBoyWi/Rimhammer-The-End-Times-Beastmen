using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace TheEndTimes_Beastmen
{
    public class GenStep_Caravan : GenStep
    {
        private static List<CellRect> possibleRects = new List<CellRect>();
        private static readonly IntRange SettlementSizeRange = new IntRange(12, 32);

        public override int SeedPart
        {
            get
            {
                return 398638183;
            }
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            IntVec3 spawnCell;
            CellFinder.TryFindRandomSpawnCellForPawnNear(map.Center, map, out spawnCell);
            
            Faction faction = map.Parent.Faction;
            if (faction == null && !TryFindEnemyFaction(out faction))
            {
                return;
            }
            TraderKindDef traderKind = faction.def.caravanTraderKinds.RandomElementByWeight<TraderKindDef>((Func<TraderKindDef, float>)(traderDef => traderDef.CalculatedCommonality));
            
            Map target = map;
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = target;
            incidentParms.spawnCenter = spawnCell;
            incidentParms.points = 1500f;
            incidentParms.faction = faction;
            incidentParms.traderKind = traderKind;
            incidentParms.generateFightersOnly = false;
            incidentParms.raidStrategy = null;
            incidentParms.raidForceOneDowned = false;
            incidentParms.pawnGroupMakerSeed = SeedPart;
            
            List <Pawn> pawnList = this.SpawnPawns(incidentParms);
            if (pawnList.Count == 0)
                return;
            
            for (int index = 0; index < pawnList.Count; ++index)
            {
                if (pawnList[index].needs != null && pawnList[index].needs.food != null)
                    pawnList[index].needs.food.CurLevel = pawnList[index].needs.food.MaxLevel;
            }
            TraderKindDef traderKindDef = traderKind;
            
            IntVec3 result;
            RCellFinder.TryFindRandomSpotJustOutsideColony(pawnList[0], out result);
            LordJob_DefendAttackedTraderCaravan travelAndExit = new LordJob_DefendAttackedTraderCaravan(spawnCell);
            LordMaker.MakeNewLord(faction, (LordJob)travelAndExit, target, (IEnumerable<Pawn>)pawnList);
            
            return;
        }

        protected List<Pawn> SpawnPawns(IncidentParms parms)
        {
            Map target = (Map)parms.target;
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Trader, parms, true), false).ToList<Pawn>();
            foreach (Thing newThing in list)
                GenSpawn.Spawn(newThing, CellFinder.RandomClosewalkCellNear(parms.spawnCenter, target, 5, (Predicate<IntVec3>)null), target, WipeMode.Vanish);
            return list;
        }

        private bool TryFindEnemyFaction(out Faction enemyFac)
        {
            return Find.FactionManager.AllFactions.Where<Faction>((Func<Faction, bool>)(f =>
            {
                if (!f.def.hidden && !f.defeated && FactionCanBeGroupSource(f))
                    return f.HostileTo(Faction.OfPlayer);
                return false;
            })).TryRandomElement<Faction>(out enemyFac);
        }

        private bool FactionCanBeGroupSource(Faction f, bool desperate = false)
        {
            if (!f.IsPlayer && !f.defeated
                    && !f.def.hidden
                    && f.HostileTo(Faction.OfPlayer)
                    && !f.def.defName.Equals("RH_TET_Beastmen_GorFaction"))
                return f.def.caravanTraderKinds.Any<TraderKindDef>();
            return false;
        }
    }
}
