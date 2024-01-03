using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TheEndTimes_Magic;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    [StaticConstructorOnStartup]
    public class ITab_HerdstoneFoodSacrificeCardUtil
    {
        public static Vector2 HerdstoneCardSize = new Vector2(500f, 320f);

        public static void DrawHerdstoneCard(Rect rect, Building_MassiveHerdstone herdstone)
        {
            GUI.BeginGroup(rect);
            Rect rect3 = new Rect(2f, 0f, ITab_HerdstoneSacrificeCardUtil.ColumnSize, ITab_HerdstoneSacrificeCardUtil.ButtonSize);
            
            Rect rectx = new Rect(2f, 0f, ITab_HerdstoneSacrificeCardUtil.ColumnSize, ITab_HerdstoneSacrificeCardUtil.ButtonSize);
            rectx.xMin = rect.center.x - 15f;
            ITab_MassiveHerdstoneCardUtil.DrawGodsFavors(new List<DarkGod>(Find.World.GetComponent<WorldComponent_DarkGods>().GodCache.Keys), rectx);

            Rect rect4 = rect3;
            Widgets.Label(rect4, "RH_TET_Beastmen_Offerer".Translate() + ": ");
            rect4.xMin = rect4.center.x - 15f;
            string label3 = OffererLabel(herdstone);
            if (Widgets.ButtonText(rect4, label3, true, false, true))
            {
                ITab_MassiveHerdstoneCardUtil.OpenActorSelectMenu(herdstone, ITab_MassiveHerdstoneCardUtil.ActorType.offerer);
            }
            TooltipHandler.TipRegion(rect4, "RH_TET_Beastmen_OffererDesc".Translate());

            Rect rect5 = rect4;
            rect5.y += ITab_HerdstoneSacrificeCardUtil.ButtonSize + 15f;
            rect5.x -= (rect4.x - 2);
            rect5.width = ITab_HerdstoneSacrificeCardUtil.ColumnSize;
            Widgets.Label(rect5, "RH_TET_Beastmen_Offering".Translate() + ": ");
            rect5.xMin = rect5.center.x - 15f;
            string label4 = OfferingLabel(herdstone);
            if (Widgets.ButtonText(rect5, label4, true, false, true))
            {
                ITab_HerdstoneFoodSacrificeCardUtil.OpenOfferingSelectMenu(herdstone);
            }
            TooltipHandler.TipRegion(rect5, "RH_TET_Beastmen_OfferingDesc".Translate());

            Rect rect6 = rect5;
            rect6.y += ITab_HerdstoneSacrificeCardUtil.ButtonSize + 15f;
            rect6.x -= (rect5.x - 2);
            rect6.width = ITab_HerdstoneSacrificeCardUtil.ColumnSize;
            Widgets.Label(rect6, "RH_TET_Beastmen_Amount".Translate() + ": ");
            rect6.xMin = rect6.center.x - 15f;
            string label5 = AmountLabel(herdstone);
            if (Widgets.ButtonText(rect6, label5, true, false, true))
            {
                ITab_HerdstoneFoodSacrificeCardUtil.OpenAmountSelectMenu(herdstone);
            }
            TooltipHandler.TipRegion(rect6, "RH_TET_Beastmen_AmountDesc".Translate());

            GUI.EndGroup();
        }
        
        private static string OfferingLabel(Building_MassiveHerdstone herdstone)
        {
            if (herdstone.tempOfferingType == HerdUtility.SacrificeType.none)
            {
                return "None";
            }
            else
            {
                return herdstone.tempOfferingType.ToString().CapitalizeFirst();
            }
        }

        private static string OffererLabel(Building_MassiveHerdstone herdstone)
        {
            if (herdstone.tempOfferer == null)
            {
                return "None";
            }
            else
            {
                return herdstone.tempOfferer.Name.ToStringShort;
            }
        }

        private static string GodLabel(Building_MassiveHerdstone herdstone)
        {
            if (herdstone.tempCurrentOfferingGod == null)
            {
                return "None";
            }
            else
            {
                return herdstone.tempCurrentOfferingGod.LabelCap;
            }
        }


        private static string AmountLabel(Building_MassiveHerdstone herdstone)
        {
            if (herdstone.tempOfferingSize == HerdUtility.OfferingSize.none)
            {
                return "None";
            }
            else
            {
                return herdstone.tempOfferingSize.ToString().CapitalizeFirst();
            }
        }
        
        public static void OpenAmountSelectMenu(Building_MassiveHerdstone herdstone)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                herdstone.tempOfferingSize = HerdUtility.OfferingSize.none;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            Action action;
            action = delegate
            {
                herdstone.tempOfferingSize = HerdUtility.OfferingSize.weak;
            };
            list.Add(new FloatMenuOption("RH_TET_Beastmen_Meagre".Translate(), action, MenuOptionPriority.Default, null, null, 0f, null));

            Action action2;
            action2 = delegate
            {
                herdstone.tempOfferingSize = HerdUtility.OfferingSize.ok;
            };
            list.Add(new FloatMenuOption("RH_TET_Beastmen_Decent".Translate(), action2, MenuOptionPriority.Default, null, null, 0f, null));

            Action action3;
            action3 = delegate
            {
                herdstone.tempOfferingSize = HerdUtility.OfferingSize.nice;
            };
            list.Add(new FloatMenuOption("RH_TET_Beastmen_Sizable".Translate(), action3, MenuOptionPriority.Default, null, null, 0f, null));

            Action action4;
            action4 = delegate
            {
                herdstone.tempOfferingSize = HerdUtility.OfferingSize.solid;
            };
            list.Add(new FloatMenuOption("RH_TET_Beastmen_Worthy".Translate(), action4, MenuOptionPriority.Default, null, null, 0f, null));

            Action action5;
            action5 = delegate
            {
                herdstone.tempOfferingSize = HerdUtility.OfferingSize.great;
            };
            list.Add(new FloatMenuOption("RH_TET_Beastmen_Impressive".Translate(), action5, MenuOptionPriority.Default, null, null, 0f, null));

            Find.WindowStack.Add(new FloatMenu(list));
        }

        public static void OpenOfferingSelectMenu(Building_MassiveHerdstone herdstone)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                herdstone.tempOfferingType = HerdUtility.SacrificeType.none;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            Action action;
            action = delegate
            {
                herdstone.tempOfferingType = HerdUtility.SacrificeType.plants;
            };
            list.Add(new FloatMenuOption("RH_TET_Beastmen_Plants".Translate(), action, MenuOptionPriority.Default, null, null, 0f, null));

            Action action2;
            action2 = delegate
            {
                herdstone.tempOfferingType = HerdUtility.SacrificeType.meat;
            };
            list.Add(new FloatMenuOption("RH_TET_Beastmen_Meat".Translate(), action2, MenuOptionPriority.Default, null, null, 0f, null));
            
            Find.WindowStack.Add(new FloatMenu(list));
        }

    }
}
