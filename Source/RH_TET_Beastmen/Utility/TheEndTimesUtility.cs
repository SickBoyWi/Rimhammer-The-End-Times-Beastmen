using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld.Planet;
using RimWorld;
using System.Reflection;

namespace TheEndTimes_Beastmen
{
    public static class ModProps
    {
        public static string main = "TheEndTimes";
        public static string mod = "Beastmen";
    }

    public static class Utility
    {
        public static bool modCheck = false;
        
        public static bool IsMorning(Map map) =>  GenLocalDate.HourInteger(map) > 6 && GenLocalDate.HourInteger(map) < 10;
        public static bool IsEvening(Map map) => GenLocalDate.HourInteger(map) > 18 && GenLocalDate.HourInteger(map) < 22;
        public static bool IsNight(Map map) => GenLocalDate.HourInteger(map) > 22;
        
        public static bool IsActorAvailable(Pawn shaman, bool downedAllowed = false)
        {
            StringBuilder s = new StringBuilder();
            s.Append("ActorAvailble Checks Initiated");
            s.AppendLine();
            if (shaman == null)
                return ResultFalseWithReport(s);
            
            s.Append("ActorAvailble: Passed null Check");
            s.AppendLine();
            if (shaman.Dead)
                return ResultFalseWithReport(s);
            
            s.Append("ActorAvailble: Passed not-dead");
            s.AppendLine();
            if (shaman.Downed && !downedAllowed)
                return ResultFalseWithReport(s);
            
            s.Append("ActorAvailble: Passed downed check & downedAllowed = " + downedAllowed.ToString());
            s.AppendLine();
            if (shaman.Drafted)
                return ResultFalseWithReport(s);
            
            s.Append("ActorAvailble: Passed drafted check");
            s.AppendLine();
            if (shaman.InAggroMentalState)
                return ResultFalseWithReport(s);
            
            s.Append("ActorAvailble: Passed drafted check");
            s.AppendLine();
            if (shaman.InMentalState)
                return ResultFalseWithReport(s);
            
            s.Append("ActorAvailble: Passed InMentalState check");
            s.AppendLine();
            s.Append("ActorAvailble Checks Passed");
            TheEndTimes_Beastmen.Utility.DebugReport(s.ToString());
            
            return true;
        }

        public static bool ResultFalseWithReport(StringBuilder s)
        {
            s.Append("ActorAvailble: Result = Unavailable");
            TheEndTimes_Beastmen.Utility.DebugReport(s.ToString());
            return false;
        }
        
        public static BodyPartRecord GetHeart(HediffSet set)
        {
            foreach (BodyPartRecord current in set.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined))
            {
                for (int i = 0; i < current.def.tags.Count; i++)
                {
                    if (current.def.tags[i].defName == "BloodPumpingSource")
                    {
                        return current;
                    }
                }
            }
            return null;
        }
        
        public static void ChangeResearchProgress(ResearchProjectDef projectDef, float progressValue, bool deselectCurrentResearch = false)
        {
            FieldInfo researchProgressInfo = typeof(ResearchManager).GetField("progress", BindingFlags.Instance | BindingFlags.NonPublic);
            object researchProgress = researchProgressInfo.GetValue(Find.ResearchManager);
            PropertyInfo itemPropertyInfo = researchProgress.GetType().GetProperty("Item");
            itemPropertyInfo.SetValue(researchProgress, progressValue, new[] { projectDef });
            if (deselectCurrentResearch) Find.ResearchManager.SetCurrentProject(null);
            Find.ResearchManager.ReapplyAllMods();
        }
        
        public static int GetSocialSkill(Pawn p) => p.skills.GetSkill(SkillDefOf.Social).Level;
        
        public static string Prefix => ModProps.main + " :: " + ModProps.mod + " :: ";

        public static void DebugReport(string x)
        {
            if (Prefs.DevMode && DebugSettings.godMode)
            {
                Log.Message(Prefix + x);
            }
        }

        public static void ErrorReport(string x) => Log.Error(Prefix + x);
    }
}
