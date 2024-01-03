using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheEndTimes_Beastmen
{
    class BiomeWorker_OldGrowthForest : BiomeWorker_BorealForest
    {
        public override float GetScore(Tile tile, int id)
        {
            if (tile.biome != null && tile.biome.defName != null && tile.biome.defName.Equals("RH_TET_ChaosWastes"))
                return -100f;
            if (tile.WaterCovered)
            {
                return -100f;
            }
            if (tile.temperature < -10f || tile.temperature > 10f)
            {
                return 0f;
            }
            if (tile.rainfall < 1100f)
            {
                return 0f;
            }
            return 40f;
        }
    }
}
