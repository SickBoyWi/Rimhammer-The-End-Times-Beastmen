﻿using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Text;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class QuestNode_GetSiteTileFinalDestruction : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;
        public SlateRef<bool> preferCloserTiles;
        public SlateRef<bool> allowCaravans;

        protected override bool TestRunInt(Slate slate)
        {
            int tile;
            if (!this.TryFindTile(slate, out tile))
                return false;
            slate.Set<int>(this.storeAs.GetValue(slate), tile, false);
            return true;
        }

        protected override void RunInt()
        {
            Slate slate = RimWorld.QuestGen.QuestGen.slate;
            int tile;
            if (!this.TryFindTile(RimWorld.QuestGen.QuestGen.slate, out tile))
                return;
            RimWorld.QuestGen.QuestGen.slate.Set<int>(this.storeAs.GetValue(slate), tile, false);
        }

        private bool TryFindTile(Slate slate, out int tile)
        {
            Map map = slate.Get<Map>("map", (Map)null, false) ?? Find.RandomPlayerHomeMap;
            int nearThisTile1 = map != null ? map.Tile : -1;
            IntRange var;
            if (slate.TryGet<IntRange>("siteDistRange", out var, false))
                return QuestNode_GetSiteTileFinalDestruction.TryFindNewSiteTile(out tile, var.min, var.max, this.allowCaravans.GetValue(slate), this.preferCloserTiles.GetValue(slate), nearThisTile1);

            bool flag = this.preferCloserTiles.GetValue(slate);
            int num1 = this.allowCaravans.GetValue(slate) ? 1 : 0;
            int num2 = flag ? 1 : 0;
            int nearThisTile2 = nearThisTile1;
            return QuestNode_GetSiteTileFinalDestruction.TryFindNewSiteTile(out tile, 10, 40, num1 != 0, num2 != 0, nearThisTile2);
        }

        public static bool TryFindNewSiteTile(out int tile, int minDist = 10, int maxDist = 40,
            bool allowCaravans = false, bool preferCloserTiles = true, int nearThisTile = -1)
        {
            Func<int, int> findTile = delegate (int root)
            {
                int minDist2 = minDist;
                int maxDist2 = maxDist;
                Predicate<int> validator = (int x) =>
                    !Find.WorldObjects.AnyWorldObjectAt(x)
                    && Find.WorldGrid[x].hilliness == Hilliness.Flat
                    && IsValidTileForNewSettlement(x, null);
                bool preferCloserTiles2 = preferCloserTiles;
                int result;
                if (TileFinder.TryFindPassableTileWithTraversalDistance(root, minDist2, maxDist2, out result, validator,
                    false, TileFinderMode.Random, false, false))
                {
                    return result;
                }
                return -1;
            };

            int arg;
            if (nearThisTile != -1)
            {
                arg = nearThisTile;
            }
            else if (!TileFinder.TryFindRandomPlayerTile(out arg, allowCaravans, (int x) => findTile(x) != -1))
            {
                tile = -1;
                return false;
            }
            tile = findTile(arg);
            return tile != -1;
        }

        public static bool IsValidTileForNewSettlement(int tile, StringBuilder reason = null)
        {
            Tile tile1 = Find.WorldGrid[tile];
            if (!tile1.biome.canBuildBase)
            {
                reason?.Append("CannotLandBiome".Translate((NamedArgument)tile1.biome.LabelCap));
                return false;
            }
            if (!tile1.biome.implemented)
            {
                reason?.Append("BiomeNotImplemented".Translate() + ": " + tile1.biome.LabelCap);
                return false;
            }
            Settlement settlementBase = Find.WorldObjects.SettlementBaseAt(tile);
            if (settlementBase != null)
            {
                if (reason != null)
                {
                    if (settlementBase.Faction == null)
                        reason.Append("TileOccupied".Translate());
                    else if (settlementBase.Faction == Faction.OfPlayer)
                        reason.Append("YourBaseAlreadyThere".Translate());
                    else
                        reason.Append("BaseAlreadyThere".Translate((NamedArgument)settlementBase.Faction.Name));
                }
                return false;
            }
            if (Find.WorldObjects.AnySettlementBaseAtOrAdjacent(tile))
            {
                reason?.Append("FactionBaseAdjacent".Translate());
                return false;
            }
            if (!Find.WorldObjects.AnyMapParentAt(tile) && Current.Game.FindMap(tile) == null && !Find.WorldObjects.AnyWorldObjectOfDefAt(WorldObjectDefOf.AbandonedSettlement, tile))
                return true;
            reason?.Append("TileOccupied".Translate());
            return false;
        }
    }
}
