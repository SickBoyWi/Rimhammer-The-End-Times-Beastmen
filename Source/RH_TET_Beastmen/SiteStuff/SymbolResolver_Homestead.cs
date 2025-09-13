using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace TheEndTimes_Beastmen
{
    public class SymbolResolver_Homestead : SymbolResolver
    {
        public static readonly FloatRange DefaultPawnsPoints = new FloatRange(150f, 300f);

        public override void Resolve(ResolveParams rp)
        {
            Map map = RimWorld.BaseGen.BaseGen.globalSettings.map;
            Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
            int dist = 0;

            if (rp.edgeDefenseWidth.HasValue)
                dist = rp.edgeDefenseWidth.Value;

            float f = (float)((double)rp.rect.Area / 144.0 * 0.170000001788139);

            RimWorld.BaseGen.BaseGen.globalSettings.minEmptyNodes = (double)f >= 1.0 ? GenMath.RoundRandom(f) : 0;
            Lord lord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, (LordJob)new LordJob_DefendBase(faction, rp.rect.CenterCell, 25000, false), map, (IEnumerable<Pawn>)null);
            TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);

            ResolveParams resolveParams1 = rp;
            resolveParams1.rect = rp.rect;
            resolveParams1.faction = faction;
            resolveParams1.singlePawnLord = lord;
            resolveParams1.pawnGroupKindDef = rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement;
            resolveParams1.singlePawnSpawnCellExtraPredicate = rp.singlePawnSpawnCellExtraPredicate ?? (Predicate<IntVec3>)(x => map.reachability.CanReachMapEdge(x, traverseParms));

            if (resolveParams1.pawnGroupMakerParams == null)
            {
                resolveParams1.pawnGroupMakerParams = new PawnGroupMakerParms();
                resolveParams1.pawnGroupMakerParams.tile = map.Tile;
                resolveParams1.pawnGroupMakerParams.faction = faction;
                PawnGroupMakerParms groupMakerParams = resolveParams1.pawnGroupMakerParams;
                float? settlementPawnGroupPoints = rp.settlementPawnGroupPoints;
                double num = (double)DefaultPawnsPoints.RandomInRange;
                groupMakerParams.points = (float)num;
                resolveParams1.pawnGroupMakerParams.inhabitants = true;
                resolveParams1.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
            }

            RimWorld.BaseGen.BaseGen.symbolStack.Push("pawnGroup", resolveParams1);
            RimWorld.BaseGen.BaseGen.symbolStack.Push("outdoorLighting", rp);

            if (faction.def.techLevel >= TechLevel.Industrial)
            {
                int num = !Rand.Chance(0.75f) ? 0 : GenMath.RoundRandom((float)rp.rect.Area / 400f);
                for (int index = 0; index < num; ++index)
                {
                    ResolveParams resolveParams2 = rp;
                    resolveParams2.faction = faction;
                    RimWorld.BaseGen.BaseGen.symbolStack.Push("firefoamPopper", resolveParams2);
                }
            }

            if (dist > 0)
            {
                ResolveParams resolveParams2 = rp;
                resolveParams2.faction = faction;
                resolveParams2.edgeDefenseWidth = new int?(dist);
                RimWorld.BaseGen.BaseGen.symbolStack.Push("edgeDefense", resolveParams2);
            }

            ResolveParams resolveParams3 = rp;
            resolveParams3.rect = rp.rect.ContractedBy(dist);
            resolveParams3.faction = faction;
            RimWorld.BaseGen.BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams3);
            ResolveParams resolveParams4 = rp;
            resolveParams4.rect = rp.rect.ContractedBy(dist);
            resolveParams4.faction = faction;
            resolveParams4.stockpileMarketValue = 250f;
            resolveParams4.wallStuff = ThingDefOf.WoodLog;
            RimWorld.BaseGen.BaseGen.symbolStack.Push("basePart_outdoors", resolveParams4);
            ResolveParams resolveParams5 = rp;
            resolveParams5.floorDef = TerrainDefOf.Bridge;
            ref ResolveParams local = ref resolveParams5;
            bool? ifTerrainSupports = rp.floorOnlyIfTerrainSupports;
            bool? nullable = new bool?(!ifTerrainSupports.HasValue || ifTerrainSupports.Value);
            local.floorOnlyIfTerrainSupports = nullable;
            RimWorld.BaseGen.BaseGen.symbolStack.Push("floor", resolveParams5);
        }
    }
}
