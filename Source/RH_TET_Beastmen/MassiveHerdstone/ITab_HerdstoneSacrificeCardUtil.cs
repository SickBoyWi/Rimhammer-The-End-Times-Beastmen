using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    [StaticConstructorOnStartup]
    public class ITab_HerdstoneSacrificeCardUtil
    {
        public static Vector2 SacrificeCardSize = new Vector2(550f, 415f);

        public static float ButtonSize = 40f;

        public static float SpacingOffset = 15f;

        public static float ColumnSize = 245f;

        public enum SacrificeCardTab : byte
        {
            Offering,
            Animal,
            Humanlike
        }
        public enum ConversionCardTab : byte
        {
            Conversion
        }

        public static SacrificeCardTab tabSacrifice = SacrificeCardTab.Offering;
        public static ConversionCardTab tabConversion = ConversionCardTab.Conversion;

        public static void DrawSacrificeCard(Rect inRect, Building_MassiveHerdstone herdstone)
        {
            GUI.BeginGroup(inRect);
            
            float herdstoneLabelWidth = Text.CalcSize("Herdstone").x;

            Rect rect = new Rect(inRect);
            rect = rect.ContractedBy(14f);
            rect.height = 30f;

            Text.Font = GameFont.Medium;
            Widgets.Label(rect, herdstone.HerdstoneName);
            Text.Font = GameFont.Small;
            
            if ((herdstone.newPawnEventBonus + herdstone.sarsenBonus) > 0)
            { 
                Rect rect2 = new Rect(rect);
                rect2.y = rect.yMax;
                StringBuilder sb = new StringBuilder();
                sb.Append("RH_TET_Beastmen_Renown".Translate() + ": " + (herdstone.newPawnEventBonus + herdstone.sarsenBonus));
                if (herdstone.sarsenBonus > 0)
                    sb.Append(" (" + "RH_TET_Beastmen_SarsenInfo".Translate() + ")");
                Widgets.Label(rect2, sb.ToString());
            }
            
            Rect rect3 = new Rect(inRect);
            rect3.yMin = rect.yMax + 65f;
            rect3.height = 550f;
            List<TabRecord> list = new List<TabRecord>();

            if (herdstone.currentFunction >= Building_MassiveHerdstone.Function.Level2)
            {
                TabRecord item = new TabRecord("RH_TET_Beastmen_Offering".Translate(), delegate
                {
                    tabSacrifice = SacrificeCardTab.Offering;
                }, tabSacrifice == SacrificeCardTab.Offering);
                list.Add(item);
            }
            if (herdstone.currentFunction >= Building_MassiveHerdstone.Function.Level3)
            {
                TabRecord item2 = new TabRecord("RH_TET_Beastmen_Animal".Translate(), delegate
                {
                    tabSacrifice = SacrificeCardTab.Animal;
                }, tabSacrifice == SacrificeCardTab.Animal);
                list.Add(item2);
            }
            if (herdstone.currentFunction >= Building_MassiveHerdstone.Function.Level4)
            {
                TabRecord item3 = new TabRecord("RH_TET_Beastmen_Humanlike".Translate(), delegate
                {
                    tabSacrifice = SacrificeCardTab.Humanlike;
                }, tabSacrifice == SacrificeCardTab.Humanlike);
                list.Add(item3);
            }
            TabDrawer.DrawTabs(rect3, list);
            FillSacrificeCard(rect3.ContractedBy(10f), herdstone);
            
            GUI.EndGroup();
        }

        public static void DrawConversionCard(Rect inRect, Building_MassiveHerdstone herdstone)
        {
            GUI.BeginGroup(inRect);

            float herdstoneLabelWidth = Text.CalcSize("Herdstone").x;

            Rect rect = new Rect(inRect);
            rect = rect.ContractedBy(14f);
            rect.height = 30f;

            Text.Font = GameFont.Medium;
            Widgets.Label(rect, herdstone.HerdstoneName);
            Text.Font = GameFont.Small;

            Rect rect3 = new Rect(inRect);
            rect3.yMin = rect.yMax + 65f;
            rect3.height = 550f;
            List<TabRecord> list = new List<TabRecord>();

            // End Game
            if (herdstone.HasConversionSarsen())
            {
                TabRecord item = new TabRecord("RH_TET_Beastmen_CommandConversion".Translate(), delegate
                {
                    tabConversion = ConversionCardTab.Conversion;
                }, tabConversion == ConversionCardTab.Conversion);
                list.Add(item);
            }
            TabDrawer.DrawTabs(rect3, list);
            FillConversionCard(rect3.ContractedBy(10f), herdstone);

            GUI.EndGroup();
        }

        public static SacrificeCardTab Tab
        {
            get => tabSacrifice;
            set => tabSacrifice = value;
        }

        protected static void FillSacrificeCard(Rect cardRect, Building_MassiveHerdstone herdstone)
        {
            if (tabSacrifice == SacrificeCardTab.Offering)
            {
                ITab_HerdstoneFoodSacrificeCardUtil.DrawHerdstoneCard(cardRect, herdstone);
            }
            else if (tabSacrifice == SacrificeCardTab.Animal)
            {
                ITab_HerdstoneAnimalSacrificeCardUtil.DrawHerdstoneCard(cardRect, herdstone);
            }
            else if (tabSacrifice == SacrificeCardTab.Humanlike)
            {
                ITab_HerdstoneHumanSacrificeCardUtil.DrawHerdstoneCard(cardRect, herdstone);
            }
            else
            {
                ITab_HerdstoneFoodSacrificeCardUtil.DrawHerdstoneCard(cardRect, herdstone);
            }
        }

        protected static void FillConversionCard(Rect cardRect, Building_MassiveHerdstone herdstone)
        {
            if (tabConversion == ConversionCardTab.Conversion)
            {
                ITab_HerdstoneHumanConversionCardUtil.DrawHerdstoneCard(cardRect, herdstone);
            }
            else
            {
                ITab_HerdstoneHumanConversionCardUtil.DrawHerdstoneCard(cardRect, herdstone);
            }
        }
    }
}
