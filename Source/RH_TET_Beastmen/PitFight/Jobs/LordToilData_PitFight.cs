using RimWorld;
using Verse;
using Verse.AI.Group;

namespace TheEndTimes_Beastmen
{
    public class LordToilData_PitFight : LordToilData
    {
        public SpectateRectSide watchRectAllowedSides = SpectateRectSide.All;
        public CellRect watchRect;

        public override void ExposeData()
        {
            Scribe_Values.Look<CellRect>(ref this.watchRect, "watchRect", new CellRect(), false);
            Scribe_Values.Look<SpectateRectSide>(ref this.watchRectAllowedSides, "watchRectAllowedSides", SpectateRectSide.None, false);
        }
    }
}
