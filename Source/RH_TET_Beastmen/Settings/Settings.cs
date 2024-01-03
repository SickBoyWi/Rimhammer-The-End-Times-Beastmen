using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class SettingsController : Mod
    {
        public SettingsController(ModContentPack content) : base(content)
        {
            base.GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "Rimhammer - The End Times - Beastmen";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }

    public class Settings : ModSettings
    {
        public static bool UseHerdWealthMoodModifier = true;
        public static bool MakePawnsRequireViolence = true;
        public static bool UseHerdstoneEvents = true;
        public static int HerdstoneRaidFrequencySliderSetting = 0;
        private static Vector2 scrollPosition = Vector2.zero;
        private const int GAP_SIZE = 24;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref UseHerdWealthMoodModifier, "RH_TET_UseHerdWealthMoodModifier", true, false);
            Scribe_Values.Look<bool>(ref MakePawnsRequireViolence, "RH_TET_MakePawnsRequireViolence", true, false);
            Scribe_Values.Look<bool>(ref UseHerdstoneEvents, "RH_TET_UseHerdstoneEvents", true, false);
            Scribe_Values.Look<int>(ref HerdstoneRaidFrequencySliderSetting, "RH_TET_HerdstoneRaidFrequencySliderSetting", 0, false);
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            Rect scroll = new Rect(5f, 45f, 430, rect.height - 40);
            Rect view = new Rect(0, 45, 400, 1200);

            Widgets.BeginScrollView(scroll, ref scrollPosition, view, true);
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(view);

            ls.CheckboxLabeled("RH_TET_Beastmen_UseHerdWealthMoodModifier".Translate(), ref UseHerdWealthMoodModifier);
            ls.Label("RH_TET_Beastmen_HerdWealthMoodModifierInfo".Translate());
            ls.Gap(GAP_SIZE);

            ls.CheckboxLabeled("RH_TET_Beastmen_MakePawnsRequireViolence".Translate(), ref MakePawnsRequireViolence);
            ls.Label("RH_TET_Beastmen_MakePawnsRequireViolenceInfo".Translate());
            ls.Gap(GAP_SIZE);

            ls.Label("RH_TET_Beastmen_Herdstone_Settings".Translate());
            ls.Gap(GAP_SIZE);

            ls.CheckboxLabeled("RH_TET_Beastmen_Herdstone_UseEvents".Translate(), ref UseHerdstoneEvents);
            ls.Gap(GAP_SIZE);

            if (UseHerdstoneEvents)
            {
                ls.Label("RH_TET_Beastmen_Herdstone_EventFrequency".Translate() + ": " + HerdstoneRaidFrequencySliderSetting);
                HerdstoneRaidFrequencySliderSetting = (int)ls.Slider(HerdstoneRaidFrequencySliderSetting, -10, 10);
                ls.Gap(GAP_SIZE);

                if (ls.ButtonText("RH_TET_Beastmen_Herdstone_Default".Translate()))
                {
                    HerdstoneRaidFrequencySliderSetting = 0;
                }
                ls.Gap(GAP_SIZE);
            }

            //Log.Error("" + GetHerdstoneRaidFrequency());

            ls.End();
            Widgets.EndScrollView();
        }

        public static float GetHerdstoneRaidFrequency()
        {
            if (HerdstoneRaidFrequencySliderSetting == 0)
                return 1;

            if (HerdstoneRaidFrequencySliderSetting < 0)
            {
                // Less frequent raids.
                int numSave = HerdstoneRaidFrequencySliderSetting * -1;

                float multiplier = (float)numSave;// * .1f;

                return multiplier;
            }
            else
            {
                // More frequent raids.
                int numSave = HerdstoneRaidFrequencySliderSetting;

                if (numSave == 10)
                    return .05f;

                float multiplier = (float)numSave * .1f;
                return 1f - multiplier;
            }
        }
    }
}