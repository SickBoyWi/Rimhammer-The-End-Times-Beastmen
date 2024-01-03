using RimWorld;
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
    public class ITab_HerdstoneHumanSacrificeCardUtil
    {

        public static void DrawHerdstoneCard(Rect rect, Building_MassiveHerdstone herdstone)
        {
            GUI.BeginGroup(rect);
            Rect rect3 = new Rect(2f, 0f, ITab_HerdstoneSacrificeCardUtil.ColumnSize, ITab_HerdstoneSacrificeCardUtil.ButtonSize);
            Widgets.Label(rect3, "RH_TET_Beastmen_God".Translate() + ": ");
            rect3.xMin = rect3.center.x - 15f;
            string label2 = ITab_MassiveHerdstoneCardUtil.GodLabel(herdstone, ITab_MassiveHerdstoneCardUtil.GodType.SacrificeGod);
            if (Widgets.ButtonText(rect3, label2, true, false, true))
            {
                ITab_MassiveHerdstoneCardUtil.OpenGodSelectMenu(herdstone, ITab_MassiveHerdstoneCardUtil.GodType.SacrificeGod);
            }
            TooltipHandler.TipRegion(rect3, "RH_TET_Beastmen_GodDesc".Translate());

            ITab_MassiveHerdstoneCardUtil.DrawGod(herdstone.tempCurrentSacrificeGod, rect3);

            Rect rect4 = rect3;
            rect4.y += ITab_HerdstoneSacrificeCardUtil.ButtonSize + 15f;
            rect4.width = ITab_HerdstoneSacrificeCardUtil.ColumnSize;
            rect4.x -= (rect3.x);
            rect4.x += 2f;
            Widgets.Label(rect4, "RH_TET_Beastmen_Executioner".Translate() + ": ");
            rect4.xMin = rect4.center.x - 15f;
            string label3 = ITab_MassiveHerdstoneCardUtil.ExecutionerLabel(herdstone);
            if (Widgets.ButtonText(rect4, label3, true, false, true))
            {
                ITab_MassiveHerdstoneCardUtil.OpenActorSelectMenu(herdstone, ITab_MassiveHerdstoneCardUtil.ActorType.executioner);
            }
            TooltipHandler.TipRegion(rect4, "RH_TET_Beastmen_ExecutionerDesc".Translate());

            Rect rect5 = rect4;
            rect5.y += ITab_HerdstoneSacrificeCardUtil.ButtonSize + 15f;
            rect5.x -= (rect4.x);
            rect5.x += 2f;
            rect5.width = ITab_HerdstoneSacrificeCardUtil.ColumnSize;
            Widgets.Label(rect5, "RH_TET_Beastmen_Sacrifice".Translate() + ": ");
            rect5.xMin = rect5.center.x - 15f;
            string label4 = ITab_MassiveHerdstoneCardUtil.SacrificeLabel(herdstone);
            if (Widgets.ButtonText(rect5, label4, true, false, true))
            {
                ITab_MassiveHerdstoneCardUtil.OpenActorSelectMenu(herdstone, ITab_MassiveHerdstoneCardUtil.ActorType.prisoner);
            }
            TooltipHandler.TipRegion(rect5, "RH_TET_Beastmen_SacrificeDesc".Translate());

            GUI.EndGroup();
        }
    }
}
