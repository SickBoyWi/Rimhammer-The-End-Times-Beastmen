using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Beastmen
{
    internal class ITab_FightingPitManager : ITab
    {
        private static Listing_Standard listingStandard = new Listing_Standard();
        protected static readonly Vector2 buttonSize = new Vector2(120f, 30f);
        public static ITab_FightingPitManager.PitCardTab tab = ITab_FightingPitManager.PitCardTab.Manager;

        protected Building_PitFightSpot SelectPit
        {
            get
            {
                return (Building_PitFightSpot)this.SelThing;
            }
        }

        public ITab_FightingPitManager()
        {
            this.size = new Vector2(550f, 215f);
            this.labelKey = "RH_TET_Beastmen_TabManage";
        }

        protected override void FillTab()
        {
            Rect rect1 = new Rect(0.0f, 0.0f, this.size.x, this.size.y).ContractedBy(5f);
            Rect rect2 = new Rect(rect1)
            {
                yMin = rect1.yMax,
                height = 550f
            };
            List<TabRecord> tabRecordList = new List<TabRecord>()
            {
                new TabRecord("Manage", (Action) (() => ITab_FightingPitManager.tab = ITab_FightingPitManager.PitCardTab.Manager), ITab_FightingPitManager.tab == ITab_FightingPitManager.PitCardTab.Manager),
                new TabRecord("Leaderboard", (Action) (() => ITab_FightingPitManager.tab = ITab_FightingPitManager.PitCardTab.Leaderboard), ITab_FightingPitManager.tab == ITab_FightingPitManager.PitCardTab.Leaderboard)
            };
            if (ITab_FightingPitManager.tab == ITab_FightingPitManager.PitCardTab.Manager)
            {
                this.FillTabManager(rect1);
            }
            else
            {
                if (ITab_FightingPitManager.tab != ITab_FightingPitManager.PitCardTab.Leaderboard)
                    return;
                this.FillTabLeaderboard(rect1);
            }
        }

        private void FillTabManager(Rect rect)
        {
            ITab_FightingPitManager.listingStandard.Begin(rect);
            float num1 = GUI.skin.label.CalcSize(new GUIContent("Organize_a_pit_fight.")).x / 2f;
            ITab_FightingPitManager.centeredText("Organize a pit fight.", new Vector2((float)((double)rect.xMax / 2.0 - (double)num1 - 2.0), 10f), 100, 0);
            Widgets.DrawLineHorizontal(rect.x - 0.0f, 35f, rect.width - 15f);
            ITab_FightingPitManager.DrawButton((Action)(() => ITab_FightingPitManagerUtility.OpenActor1SelectMenu(this.SelectPit)), ITab_FightingPitManager.FighterLabel(this.SelectPit, 0), new Vector2((float)((double)rect.xMax - (double)ITab_FightingPitManager.buttonSize.x - 10.0), 75f), "Fighter #2", true);
            ITab_FightingPitManager.DrawButton((Action)(() => ITab_FightingPitManagerUtility.OpenActor2SelectMenu(this.SelectPit)), ITab_FightingPitManager.FighterLabel(this.SelectPit, 1), new Vector2(rect.xMin - 10f, 75f), "Fighter #1", true);
            float num2 = GUI.skin.label.CalcSize(new GUIContent("Vs.")).x / 2f;
            ITab_FightingPitManager.centeredText("Vs.", new Vector2(rect.xMax / 2f - num2, 75f), 0, 0);
            if (this.SelectPit.currentState == Building_PitFightSpot.State.resting)
                ITab_FightingPitManager.DrawButton((Action)(() => this.SelectPit.Fight()), "Fight!", new Vector2((float)((double)rect.xMax / 2.0 - (double)ITab_FightingPitManager.buttonSize.x / 2.0), 135f), "Let the pit fight begin!", true);
            else if (this.SelectPit.currentState == Building_PitFightSpot.State.preparation || this.SelectPit.currentState == Building_PitFightSpot.State.scheduled)
                ITab_FightingPitManager.DrawButton((Action)(() => this.SelectPit.TryCancelFight("")), "Cancel", new Vector2((float)((double)rect.xMax / 2.0 - (double)ITab_FightingPitManager.buttonSize.x / 2.0), 135f), "Cancel the pit fight", true);
            else if (this.SelectPit.currentState == Building_PitFightSpot.State.fighting)
                ITab_FightingPitManager.DrawButton((Action)(() => this.SelectPit.EndFight((Pawn)null, true)), "Suspend the fight", new Vector2((float)((double)rect.xMax / 2.0 - (double)ITab_FightingPitManager.buttonSize.x / 2.0), 135f), "Suspend the pit fight", true);
            ITab_FightingPitManager.listingStandard.End();
        }

        private void FillTabLeaderboard(Rect rect)
        {
            ITab_FightingPitManager.listingStandard.Begin(rect);
            ITab_FightingPitManager.listingStandard.Gap(50f);
            float num = GUI.skin.label.CalcSize(new GUIContent("Winners")).x / 2f;
            ITab_FightingPitManager.centeredText("Winners", new Vector2(rect.xMax / 2f - num, 165f), 0, 0);
            ITab_FightingPitManager.listingStandard.Gap(50f);
            ITab_FightingPitManager.listingStandard.End();
        }

        private void FillTabArea(Rect rect)
        {
            ITab_FightingPitManager.LabelWithTooltip("Fighting Area", "Area in which pit fight will occur.", true);
            this.DoAreaRestriction(ITab_FightingPitManager.listingStandard, (Area)this.SelectPit.Map.areaManager.Home, new Action<Area>(this.SetAreaRestriction), new Func<Area, string>(AreaUtility.AreaAllowedLabel_Area));
        }

        private static void LabelWithTooltip(string label, string tooltip, bool centered = false)
        {
            Rect rect1 = ITab_FightingPitManager.listingStandard.GetRect(Text.CalcHeight(label, ITab_FightingPitManager.listingStandard.ColumnWidth));
            Vector2 vector2 = new Vector2();
            if (centered)
                vector2.x = (float)((double)rect1.xMax / 2.0 - (double)GUI.skin.label.CalcSize(new GUIContent(label)).x / 2.0);
            Rect rect2 = rect1;
            rect2.x = vector2.x;
            Widgets.Label(rect2, label);
            ITab_FightingPitManager.DoTooltip(rect1, tooltip);
        }

        private static void DrawButton(
          Action action,
          string text,
          Vector2 pos,
          string tooltip = null,
          bool active = true)
        {
            Rect rect = new Rect(pos.x, pos.y, ITab_FightingPitManager.buttonSize.x, ITab_FightingPitManager.buttonSize.y);
            if (!tooltip.NullOrEmpty())
                TooltipHandler.TipRegion(rect, (TipSignal)tooltip);
            if (!Widgets.ButtonText(rect, text, true, false, Color.white, active))
                return;
            SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera((Map)null);
            action();
        }

        private static void centeredText(string label, Vector2 pos, int offX = 0, int offY = 0)
        {
            Widgets.Label(new Rect(pos.x, pos.y, ITab_FightingPitManager.buttonSize.x + (float)offX, ITab_FightingPitManager.buttonSize.y + (float)offY), label);
        }

        private static void DoTooltip(Rect rect, string tooltip)
        {
            if (tooltip.NullOrEmpty())
                return;
            if (Mouse.IsOver(rect))
                Widgets.DrawHighlight(rect);
            TooltipHandler.TipRegion(rect, (TipSignal)tooltip);
        }

        private void SetAreaRestriction(Area area)
        {
            this.SelectPit.FightingArea = area;
        }

        private void DoAreaRestriction(
          Listing_Standard listing,
          Area area,
          Action<Area> setArea,
          Func<Area, string> getLabel)
        {
            this.DoAllowedAreaSelectors(listing.GetRect(48f), this.SelectPit, getLabel);
            Area fightingArea = this.SelectPit.FightingArea;
            this.SelectPit.FightingArea = (Area)null;
            Text.Anchor = TextAnchor.UpperLeft;
            if (fightingArea == area)
                return;
            setArea(fightingArea);
        }

        public void DoAllowedAreaSelectors(Rect rect, Building_PitFightSpot p, Func<Area, string> getLabel)
        {
            if (Find.CurrentMap == null)
                return;
            Area[] array = ITab_FightingPitManager.GetAreas().ToArray<Area>();
            int num1 = array.Length + 1;
            float width = rect.width / (float)num1;
            Text.WordWrap = false;
            Text.Font = GameFont.Tiny;
            ITab_FightingPitManager.DoAreaSelector(new Rect(rect.x, rect.y, width, rect.height), p, (Area)null, getLabel);
            int num2 = 1;
            foreach (Area area in array)
            {
                if (area != this.SelectPit.Map.areaManager.Home)
                {
                    float num3 = (float)num2 * width;
                    ITab_FightingPitManager.DoAreaSelector(new Rect(rect.x + num3, rect.y, width, rect.height), p, area, getLabel);
                    ++num2;
                }
            }
            Text.WordWrap = true;
            Text.Font = GameFont.Small;
        }

        public static IEnumerable<Area> GetAreas()
        {
            return Find.CurrentMap.areaManager.AllAreas.Where<Area>((Func<Area, bool>)(a => a.AssignableAsAllowed()));
        }

        private static void DoAreaSelector(
          Rect rect,
          Building_PitFightSpot p,
          Area area,
          Func<Area, string> getLabel)
        {
            rect = rect.ContractedBy(1f);
            GUI.DrawTexture(rect, area == null ? (Texture)BaseContent.GreyTex : (Texture)area.ColorTexture);
            Text.Anchor = TextAnchor.MiddleLeft;
            string label = getLabel(area);
            Rect rect1 = rect;
            rect1.xMin += 3f;
            rect1.yMin += 2f;
            Widgets.Label(rect1, label);
            if (p.FightingArea == area)
                Widgets.DrawBox(rect, 2);
            if (Mouse.IsOver(rect))
            {
                area?.MarkForDraw();
                if (Input.GetMouseButton(0) && p.FightingArea != area)
                {
                    p.FightingArea = area;
                    SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera((Map)null);
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, (TipSignal)label);
        }

        private static string FighterLabel(Building_PitFightSpot pit, int index)
        {
            if (index == 0 && pit.fighter1.p != null)
                return pit.fighter1.p.Name.ToStringShort;
            if (index == 1 && pit.fighter2.p != null)
                return pit.fighter2.p.Name.ToStringShort;
            return "Select";
        }

        public enum PitCardTab : byte
        {
            Manager,
            Leaderboard,
        }
    }
}
