using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    internal class ITab_FightingPitManagerInterface
    {
        private static Listing_Standard listingStandard = new Listing_Standard();

        public static void DrawFightingPitTab(Rect rect, Building_PitFightSpot pit)
        {
        }

        private static void FillTabArea(Rect rect)
        {
            ITab_FightingPitManagerInterface.LabelWithTooltip("Fighting area", "Pit Fighting area");
        }

        private static void LabelWithTooltip(string label, string tooltip)
        {
            Rect rect = ITab_FightingPitManagerInterface.listingStandard.GetRect(Text.CalcHeight(label, ITab_FightingPitManagerInterface.listingStandard.ColumnWidth));
            Widgets.Label(rect, label);
            ITab_FightingPitManagerInterface.DoTooltip(rect, tooltip);
        }

        private static void DoTooltip(Rect rect, string tooltip)
        {
            if (tooltip.NullOrEmpty())
                return;
            if (Mouse.IsOver(rect))
                Widgets.DrawHighlight(rect);
            TooltipHandler.TipRegion(rect, (TipSignal)tooltip);
        }

        private static string FighterLabel(Building_PitFightSpot pit, int index)
        {
            if (index == 0 && pit.fighter1.p != null)
                return pit.fighter1.p.Name.ToStringShort;
            if (index == 1 && pit.fighter2.p != null)
                return pit.fighter2.p.Name.ToStringShort;
            return "Select";
        }
    }
}
