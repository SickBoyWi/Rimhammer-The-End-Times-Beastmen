using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace TheEndTimes_Beastmen
{
    [StaticConstructorOnStartup]
    public class RH_TET_BeastmenMod : Mod
    {
        public static System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
        public static bool questInEffect = false;
        public static bool anyOneCanBeastActive = false;

        public static List<ThingDef> getSarsenList()
        {
            List<ThingDef> sarsens = new List<ThingDef>();

            sarsens.Add(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Psychic);
            sarsens.Add(BeastmenDefOf.RH_TET_Beastmen_Sarsen_Gifter);
            sarsens.Add(BeastmenDefOf.RH_TET_Beastmen_SarsenDouble_Conversion);
            sarsens.Add(BeastmenDefOf.RH_TET_Beastmen_Sarsen_Tutor);
            sarsens.Add(BeastmenDefOf.RH_TET_Beastmen_Sarsen_Healer);
            sarsens.Add(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Pulse);
            sarsens.Add(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Gas);
            sarsens.Add(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Beacon);

            return sarsens;
        }

        public RH_TET_BeastmenMod(ModContentPack content) : base(content)
        {
            if (ModsConfig.IsActive("SickBoyWi.TheEndTimes.BeastsAnyOne"))
            {
                anyOneCanBeastActive = true;
            }
            else
                anyOneCanBeastActive = false;

            var harmony = new Harmony("rimworld.theendtimes.beastmen");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}