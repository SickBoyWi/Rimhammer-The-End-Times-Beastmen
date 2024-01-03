using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using RimWorld.Planet;
using TheEndTimes_Magic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public static class HerdUtility
    {
        public static readonly int ritualDuration = 740; // 15 secs
        public static readonly int offeringAwaitDuration = 600; // 10 secs

        #region GetResults

        public enum SacrificeResult
        {
            none = 0,
            criticalfailure,
            failure,
            mixedsuccess,
            success
        }

        public enum SacrificeType
        {
            none,
            meat,
            plants,
            meals,
            animal,
            weapons,
            armor,
            artifact,
            humanlike
        }

        public enum OfferingSize
        {
            none = 0,
            weak = 5,
            ok = 10,
            nice = 20,
            solid = 50,
            great = 100
        }

        public static void SacrificeExecutionComplete(Building_MassiveHerdstone herdstone)
        {
            herdstone.ChangeState(Building_MassiveHerdstone.State.sacrificing,
                Building_MassiveHerdstone.SacrificeState.finishing);

            DarkGod g = herdstone.SacrificeData.God;

            if (herdstone.SacrificeData.Sacrifice.RaceProps.Animal)
            {
                g.ReceiveSacrifice(herdstone.SacrificeData.Sacrifice);

                RewardUtility.ProcessSacrificeAnimal(g, herdstone);
            }
            else
                RewardUtility.ProcessSacrificeHuman(g, herdstone);
            
            MapComponent_SacrificeTracker tracker = herdstone.Map.GetComponent<MapComponent_SacrificeTracker>();
            if (tracker != null)
            {
                tracker.lastUsedHerdstone = herdstone;
                if (tracker.lastSacrificeType == SacrificeType.humanlike)
                {
                    herdstone.sacrificedHumansCount++;

                    tracker.lastResult = SacrificeResult.success;
                    
                    var result = tracker.lastResult;
                    
                    List<Pawn> prisoners = herdstone.Map.mapPawns.PrisonersOfColonySpawned;
                    if (prisoners != null)
                    {
                        foreach (Pawn prisoner in prisoners)
                        {
                            if (prisoner != null)
                            {
                                if (prisoner.needs != null)
                                {
                                    prisoner.needs.mood.thoughts.memories.TryGainMemory(BeastmenDefOf
                                        .RH_TET_Beastmen_OtherPrisonerWasSacrificed);
                                }
                            }
                        }
                    }
                }
                else
                {
                    herdstone.sacrificedAnimalsCount++;
                    herdstone.newPawnEventBonus++;
                }

                MakeSacrificeThoughts(herdstone.SacrificeData.Executioner, herdstone.SacrificeData.Sacrifice, true);
                if (!herdstone.SacrificeData.Congregation.NullOrEmpty())
                {
                    foreach (Pawn pawn in herdstone.SacrificeData.Congregation)
                    {
                        if (pawn.Spawned && !pawn.Dead && pawn != herdstone.SacrificeData.Sacrifice)
                        {
                            if (pawn != herdstone.SacrificeData.Executioner)
                                MakeSacrificeThoughts(pawn, herdstone.SacrificeData.Sacrifice);
                        }
                    }
                }

                herdstone.tempSacrifice = null;
                tracker.GenerateSacrificeMessage(herdstone);
                herdstone.ChangeState(Building_MassiveHerdstone.State.notinuse,
                    Building_MassiveHerdstone.SacrificeState.finished);
            }
        }
        public static void ConversionComplete(Building_MassiveHerdstone herdstone)
        {
            herdstone.ChangeState(Building_MassiveHerdstone.State.converting,
                Building_MassiveHerdstone.SacrificeState.finishing);

            MapComponent_SacrificeTracker tracker = herdstone.Map.GetComponent<MapComponent_SacrificeTracker>();
            if (tracker != null)
            {
                tracker.lastUsedHerdstone = herdstone;
                if (tracker.lastSacrificeType == SacrificeType.humanlike)
                {
                    tracker.lastResult = SacrificeResult.success;

                    var result = tracker.lastResult;

                    List<Pawn> prisoners = herdstone.Map.mapPawns.PrisonersOfColonySpawned;
                    if (prisoners != null)
                    {
                        foreach (Pawn prisoner in prisoners)
                        {
                            if (prisoner != null)
                            {
                                if (prisoner.needs != null)
                                {
                                    prisoner.needs.mood.thoughts.memories.TryGainMemory(BeastmenDefOf
                                        .RH_TET_Beastmen_OtherPrisonerWasSacrificed);
                                }
                            }
                        }
                    }
                }

                MakeConversionThoughts(herdstone.ConversionData.Executioner, herdstone.ConversionData.Victim, true);
                if (!herdstone.ConversionData.Congregation.NullOrEmpty())
                {
                    foreach (Pawn pawn in herdstone.ConversionData.Congregation)
                    {
                        if (pawn.Spawned && !pawn.Dead && pawn != herdstone.ConversionData.Victim)
                        {
                            if (pawn != herdstone.ConversionData.Executioner)
                                MakeConversionThoughts(pawn, herdstone.ConversionData.Victim);
                        }
                    }
                }

                herdstone.tempSacrifice = null;
                tracker.GenerateConversionMessage(herdstone);
                herdstone.ChangeState(Building_MassiveHerdstone.State.notinuse,
                    Building_MassiveHerdstone.SacrificeState.finished);
            }
        }

        public static void OfferingComplete(Pawn offerer, Building_MassiveHerdstone herdstone, DarkGod god,
            List<Thing> offering)
        {
            herdstone.ChangeState(Building_MassiveHerdstone.State.notinuse,
                Building_MassiveHerdstone.OfferingState.finished);

            ReceiveOffering(offerer, herdstone, offering, god);

            if (Utility.IsActorAvailable(offerer))
            {
                Job job = new Job(BeastmenDefOf.RH_TET_Beastmen_WaitThroughOffering);
                job.targetA = herdstone;
                offerer.jobs.TryTakeOrderedJob(job);
            }
            Settlement factionBase = (Settlement)herdstone.Map.info.parent;

            Messages.Message(("RH_TET_Beastmen_OfferingFinished".Translate(offerer.Name) + "RH_TET_Beastmen_HerdstoneFavorTotal".Translate(god.Label, god.PlayerFavor)), TargetInfo.Invalid,
                MessageTypeDefOf.PositiveEvent);
        }

        public static void ReceiveOffering(Pawn offerer, Building_MassiveHerdstone herdstone, List<Thing> offerings, DarkGod god)
        {
            foreach (Thing offering in offerings)
            {
                god.ConsumeOfferings(offerer, offering);
                RewardUtility.ProcessOffering(god, herdstone);
            }
        }

        #endregion GetResults

        #region Bools

        public static bool AreHerdstonesAvailable(Map map)
        {
            if (map.listerBuildings.AllBuildingsColonistOfClass<Building_MassiveHerdstone>() != null)
            {
                foreach (Building_MassiveHerdstone herdstone in map.listerBuildings
                    .AllBuildingsColonistOfClass<Building_MassiveHerdstone>())
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsExecutioner(Pawn p)
        {
            List<Thing> list =
                p.Map.listerThings.AllThings.FindAll(s => s.GetType() == typeof(Building_MassiveHerdstone));
            foreach (Building_MassiveHerdstone b in list)
            {
                if (b != null && b.SacrificeData != null && b.SacrificeData.Executioner != null)
                    if (b?.SacrificeData?.Executioner == p) return true;
                if (b?.ConversionData?.Executioner == p) return true;
            }
            return false;
        }

        public static bool IsSacrifice(Pawn p)
        {
            List<Thing> list =
                p.Map.listerThings.AllThings.FindAll(s => s.GetType() == typeof(Building_MassiveHerdstone));
            foreach (Building_MassiveHerdstone b in list)
            {
                if (b != null && b.SacrificeData != null && b.SacrificeData.Sacrifice != null)
                    if (b?.SacrificeData?.Sacrifice == p) return true;
                if (b?.ConversionData?.Victim == p) return true;
            }
            return false;
        }

        public static bool IsBrayShamanAvailable(Pawn pawn)
        {
            if (!Utility.IsActorAvailable(pawn)) return false;
            if (!IsBrayShaman(pawn)) return false;
            return true;
        }

        public static bool IsBrayShaman(Pawn pawn)
        {
            if (pawn == null)
            {
                Utility.DebugReport("IsBrayShaman :: Pawn Null Exception");
                return false;
            }
            
            if (pawn.needs == null)
            {
                Utility.DebugReport("IsBrayShaman :: Pawn Needs Null Exception");
                return false;
            }
            
            if (pawn.kindDef.defName.Equals("RH_TET_Beastmen_BrayShaman") 
                || pawn.kindDef.defName.Equals("RH_TET_Beastmen_GreatBrayShaman")
                || pawn.kindDef.defName.Equals("RH_TET_Beastmen_BrayScenario"))
            {
                CompMagicUser compMagicUser = pawn.TryGetComp<CompMagicUser>();
                if (compMagicUser != null && compMagicUser.IsMagicUser)
                {
                    return true;
                }
            }
            
            return false;
        }

        #endregion Bools
         
        #region ThoughtGivers

        public static void MakeSacrificeThoughts(Pawn attendee, Pawn other = null, bool isExcutioner = false)
        {
            if (attendee != null)
            {
                SacrificeType lastSacrifice =
                    attendee.Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType;

                //Human sacrifices
                if (lastSacrifice == SacrificeType.humanlike)
                {
                    //Sacrifice Thought
                    ThoughtDef resultThought = null;
                    switch (attendee.Map.GetComponent<MapComponent_SacrificeTracker>().lastResult)
                    {
                        case SacrificeResult.mixedsuccess:
                        case SacrificeResult.success:
                            resultThought = BeastmenDefOf.RH_TET_Beastmen_AttendedSuccessfulSacrifice;
                            break;
                        case SacrificeResult.failure:
                        case SacrificeResult.criticalfailure:
                            resultThought = BeastmenDefOf.RH_TET_Beastmen_AttendedFailedSacrifice;
                            break;
                        case SacrificeResult.none:
                            break;
                    }
                    attendee.needs.mood.thoughts.memories.TryGainMemory(resultThought);

                    //Relationship Thoughts
                    if (other != null)
                    {
                        //Family

                        ThoughtDef familyThought = null;
                        if (attendee.relations.FamilyByBlood.Contains(other))
                        {
                            if (isExcutioner) familyThought = BeastmenDefOf.RH_TET_Beastmen_ExecutedFamily;
                            else familyThought = BeastmenDefOf.RH_TET_Beastmen_SacrificedFamily;
                        }
                        if (familyThought != null) attendee.needs.mood.thoughts.memories.TryGainMemory(familyThought);

                        //Friends and Rivals
                        ThoughtDef relationThought = null;
                        int num = attendee.relations.OpinionOf(other);
                        if (num >= 20)
                        {
                            if (isExcutioner) relationThought = ThoughtDefOf.KilledMyFriend;
                            else relationThought = BeastmenDefOf.RH_TET_Beastmen_SacrificedFriend;
                        }
                        else if (num <= -20)
                        {
                            if (isExcutioner) relationThought = ThoughtDefOf.KilledMyRival;
                            else relationThought = BeastmenDefOf.RH_TET_Beastmen_SacrificedRival;
                        }
                        if (relationThought != null)
                            attendee.needs.mood.thoughts.memories.TryGainMemory(relationThought);

                        //Bloodlust
                        if (attendee.story.traits.HasTrait(TraitDefOf.Bloodlust))
                        {
                            if (isExcutioner)
                                attendee.needs.mood.thoughts.memories.TryGainMemory(
                                    ThoughtDefOf.KilledHumanlikeBloodlust, other);
                            else
                                attendee.needs.mood.thoughts.memories.TryGainMemory(
                                    ThoughtDefOf.WitnessedDeathBloodlust, other);
                        }
                    }
                }

                //Animal Sacrifices
                else if (lastSacrifice == SacrificeType.animal)
                {
                    if (other != null && other.RaceProps != null)
                    {
                        if (other.RaceProps.Animal)
                        {
                            //Pet checker
                            if (other.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) == attendee)
                            {
                                if (isExcutioner)
                                    attendee.needs.mood.thoughts.memories.TryGainMemory(BeastmenDefOf.RH_TET_Beastmen_ExecutedPet,
                                        other);
                                else
                                    attendee.needs.mood.thoughts.memories.TryGainMemory(BeastmenDefOf.RH_TET_Beastmen_SacrificedPet,
                                        other);
                            }
                        }
                    }
                }
            }
        }

        public static void MakeConversionThoughts(Pawn attendee, Pawn other = null, bool isExcutioner = false)
        {
            if (attendee != null)
            {
                //Sacrifice Thought
                ThoughtDef resultThought = null;
                switch (attendee.Map.GetComponent<MapComponent_SacrificeTracker>().lastResult)
                {
                    case SacrificeResult.mixedsuccess:
                    case SacrificeResult.success:
                        resultThought = BeastmenDefOf.RH_TET_Beastmen_AttendedSuccessfulConversion;
                        break;
                    case SacrificeResult.failure:
                    case SacrificeResult.criticalfailure:
                        resultThought = BeastmenDefOf.RH_TET_Beastmen_AttendedFailedConversion;
                        break;
                    case SacrificeResult.none:
                        break;
                }
                attendee.needs.mood.thoughts.memories.TryGainMemory(resultThought);

                //Relationship Thoughts
                if (other != null)
                {
                    //Family

                    ThoughtDef familyThought = null;
                    if (attendee.relations.FamilyByBlood.Contains(other))
                    {
                        if (isExcutioner) familyThought = BeastmenDefOf.RH_TET_Beastmen_ConvertedFamily;
                        else familyThought = BeastmenDefOf.RH_TET_Beastmen_ConvertedFamily;
                    }
                    if (familyThought != null) attendee.needs.mood.thoughts.memories.TryGainMemory(familyThought);

                    //Friends and Rivals
                    ThoughtDef relationThought = null;
                    int num = attendee.relations.OpinionOf(other);
                    if (num >= 20)
                    {
                        if (isExcutioner) relationThought = BeastmenDefOf.RH_TET_Beastmen_ConvertedFriend;
                        else relationThought = BeastmenDefOf.RH_TET_Beastmen_ConvertedFriend;
                    }
                    else if (num <= -20)
                    {
                        if (isExcutioner) relationThought = BeastmenDefOf.RH_TET_Beastmen_ConvertedRival;
                        else relationThought = BeastmenDefOf.RH_TET_Beastmen_ConvertedRival;
                    }

                    if (relationThought != null)
                        attendee.needs.mood.thoughts.memories.TryGainMemory(relationThought);
                }
            }
        }

        public static void GiveMoodToPawns(ThoughtDef thought, IEnumerable<Pawn> pawns, Pawn otherpawn = null)
        {
            if (pawns != null && thought != null)
            {
                foreach (Pawn podsAliveColonist in pawns)
                {
                    if (otherpawn == null || !podsAliveColonist.Equals(otherpawn))
                    {
                        if (podsAliveColonist != null && podsAliveColonist.needs != null && podsAliveColonist.needs.mood != null && podsAliveColonist.needs.mood.thoughts != null && podsAliveColonist.needs.mood.thoughts.memories != null)
                            podsAliveColonist.needs.mood.thoughts.memories.TryGainMemory(thought, otherpawn);
                    }
                }
            }
        }

        public static void RemoveMoodFromPawns(ThoughtDef thought, IEnumerable<Pawn> pawns)
        {
            if (pawns != null && thought != null)
            {
                foreach (Pawn podsAliveColonist in pawns)
                {
                    if (podsAliveColonist != null && podsAliveColonist.needs != null && podsAliveColonist.needs.mood != null && podsAliveColonist.needs.mood.thoughts != null && podsAliveColonist.needs.mood.thoughts.memories != null)
                        podsAliveColonist.needs.mood.thoughts.memories.RemoveMemoriesOfDef(thought);
                }
            }
        }

        #endregion ThoughtGivers

        #region HerdstoneJobs

        #region Sacrifice

        public static void GiveAttendSacrificeJob(Building_MassiveHerdstone herdstone, Pawn attendee)
        {
            if (IsExecutioner(attendee)) return;
            if (IsSacrifice(attendee)) return;
            if (!Utility.IsActorAvailable(attendee)) return;
            if (attendee.jobs.curJob.def == BeastmenDefOf.RH_TET_Beastmen_ReflectOnResult) return;
            if (attendee.jobs.curJob.def == BeastmenDefOf.RH_TET_Beastmen_AttendSacrifice) return;
            if (attendee.Drafted) return;
            if (attendee.IsPrisoner) return;

            IntVec3 result;
            Building chair;

            if (!WatchBuildingUtility.TryFindBestWatchCell(herdstone, attendee, true, out result, out chair))
            {
                if (!WatchBuildingUtility.TryFindBestWatchCell(herdstone, attendee, false, out result, out chair))
                {
                    return;
                }
            }

            int dir = herdstone.Rotation.Opposite.AsInt;

            if (chair != null)
            {
                IntVec3 newPos = chair.Position + GenAdj.CardinalDirections[dir];

                Job J = new Job(BeastmenDefOf.RH_TET_Beastmen_AttendSacrifice, herdstone, newPos, chair);
                J.playerForced = true;
                J.ignoreJoyTimeAssignment = true;
                J.expiryInterval = 9999;
                J.ignoreDesignations = true;
                J.ignoreForbidden = true;
                attendee.jobs.TryTakeOrderedJob(J);
            }
            else
            {
                IntVec3 newPos = result + GenAdj.CardinalDirections[dir];

                Job J = new Job(BeastmenDefOf.RH_TET_Beastmen_AttendSacrifice, herdstone, newPos, result);
                J.playerForced = true;
                J.ignoreJoyTimeAssignment = true;
                J.expiryInterval = 9999;
                J.ignoreDesignations = true;
                J.ignoreForbidden = true;
                attendee.jobs.TryTakeOrderedJob(J);
            }
        }

        #endregion Sacrifice

        #region Gather

        public static void AbortCongregation(Building_MassiveHerdstone herdstone)
        {
            if (herdstone != null) herdstone.ChangeState(Building_MassiveHerdstone.State.notinuse);
        }

        public static void AbortCongregation(Building_MassiveHerdstone herdstone, String reason)
        {
            if (herdstone != null) herdstone.ChangeState(Building_MassiveHerdstone.State.notinuse);
            Messages.Message(reason + " " + "RH_TET_Beastmen_AbortHerdGathering".Translate(), MessageTypeDefOf.NegativeEvent);
        }

        #endregion Gather

        #endregion HerdstoneJobs

        #region GeneralUtility

        public static bool IsBeastman(Pawn p)
        {
            if (p == null)
                return false;
            if (p.RaceProps.Humanlike && (p.def.race.body.defName.Equals("RH_TET_Beastmen_Ungor")|| p.def.race.body.defName.Equals("RH_TET_Beastmen_Gor")))
            {
                return true;
            }
            return false;
        }


        public static bool finalDestructionQuestExposedYet(List<Thing> herdstones)
        {
            bool retVal = false;

            foreach (Building_MassiveHerdstone herdy in herdstones)
            {
                if (herdy != null && herdy.finalDestructionQuestExposed)
                {
                    retVal = true;
                    break;
                }
            }

            return retVal;
        }

        public static bool destructionQuestExposedYet(List<Thing> herdstones)
        {
            bool retVal = false;

            foreach (Building_MassiveHerdstone herdy in herdstones)
            {
                if (herdy != null && herdy.destructionQuestExposed)
                {
                    retVal = true;
                    break;
                }
            }

            return retVal;
        }

        public static List<Thing> getAllHerdstones()
        {
            List<Thing> herdstones = new List<Thing>();

            foreach (Map map in Find.Maps)
            {
                herdstones.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone));
                herdstones.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone_UpgradeOne));
                herdstones.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone_UpgradeTwo));
                herdstones.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone_UpgradeThree));
            }

            return herdstones;
        }

        public static bool IsHumanlikeMeat(ThingDef def)
        {
            return def.ingestible.sourceDef != null && def.ingestible.sourceDef.race != null && def.ingestible.sourceDef.race.Humanlike;
        }

        #endregion GeneralUtility
    }
}