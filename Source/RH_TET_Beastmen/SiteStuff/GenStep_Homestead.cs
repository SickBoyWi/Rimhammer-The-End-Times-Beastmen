using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class GenStep_Homestead : GenStep
    {
        private static List<CellRect> possibleRects = new List<CellRect>();
        private static readonly IntRange SettlementSizeRange = new IntRange(12, 32);
        public FloatRange defaultPawnGroupPointsRange = new FloatRange(750f, 900f);

        public override int SeedPart
        {
            get
            {
                return 398638182;
            }
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            CellRect var;
            if (!MapGenerator.TryGetVar<CellRect>("RectOfInterest", out var))
                var = CellRect.SingleCell(map.Center);
            Faction faction = map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer ? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined) : map.ParentFaction;

            ResolveParams resolveParams = new ResolveParams();
            resolveParams.rect = this.GetOutpostRect(var, map);
            resolveParams.faction = faction;
            resolveParams.edgeDefenseWidth = new int?(0);
            resolveParams.edgeDefenseTurretsCount = new int?(0);
            resolveParams.edgeDefenseMortarsCount = new int?(0);

            if (parms.sitePart != null)
            {
                resolveParams.settlementPawnGroupPoints = new float?(parms.sitePart.parms.threatPoints);
                resolveParams.settlementPawnGroupSeed = new int?(OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms));
            }
            else
                resolveParams.settlementPawnGroupPoints = new float?(this.defaultPawnGroupPointsRange.RandomInRange);

            RimWorld.BaseGen.BaseGen.globalSettings.map = map;
            RimWorld.BaseGen.BaseGen.globalSettings.minBuildings = 1;
            RimWorld.BaseGen.BaseGen.globalSettings.minBarracks = 1;
            RimWorld.BaseGen.BaseGen.symbolStack.Push("RH_TET_Beastmen_Homestead", resolveParams);
            RimWorld.BaseGen.BaseGen.Generate();
        }

        private CellRect GetOutpostRect(CellRect rectToDefend, Map map)
        {
            int randomInRange1 = SettlementSizeRange.RandomInRange;
            int randomInRange2 = SettlementSizeRange.RandomInRange;

            GenStep_Homestead.possibleRects.Add(new CellRect(rectToDefend.minX - 1 - randomInRange1, rectToDefend.CenterCell.z - randomInRange2 / 2, randomInRange1, randomInRange2));
            GenStep_Homestead.possibleRects.Add(new CellRect(rectToDefend.maxX + 1, rectToDefend.CenterCell.z - randomInRange1 / 2, randomInRange1, randomInRange2));
            GenStep_Homestead.possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - randomInRange1 / 2, rectToDefend.minZ - 1 - randomInRange2, randomInRange1, randomInRange2));
            GenStep_Homestead.possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - randomInRange1 / 2, rectToDefend.maxZ + 1, randomInRange1, randomInRange2));
            CellRect mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
            GenStep_Homestead.possibleRects.RemoveAll((Predicate<CellRect>)(x => !x.FullyContainedWithin(mapRect)));
            if (GenStep_Homestead.possibleRects.Any<CellRect>())
                return GenStep_Homestead.possibleRects.RandomElement<CellRect>();
            return rectToDefend;
        }
    }
}
