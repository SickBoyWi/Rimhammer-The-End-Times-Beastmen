using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using TheEndTimes_Magic;
using RimWorld.QuestGen;
using RimWorld.Planet;
using Verse.Sound;

namespace TheEndTimes_Beastmen
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static PawnKindDef Slave;

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("rimworld.theendtimes.beastmen");

            harmony.Patch(original: AccessTools.Method(
                    type: typeof(IncidentWorker_WandererJoin),
                    name: "TryExecuteWorker"),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(IncidentWorker_WandererJoin_TryExecuteWorker_PreFix)),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(IncidentWorker_WandererJoin_TryExecuteWorker_PostFix)));

            harmony.Patch(original: AccessTools.Method(
                    type: typeof(QuestNode_GeneratePawn),
                    name: "RunInt"),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(QuestNode_GeneratePawn_RunInt_PreFix)),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(QuestNode_GeneratePawn_RunInt_PostFix)));

            harmony.Patch(original: AccessTools.Method(
                    type: typeof(DownedRefugeeQuestUtility),
                    name: "GenerateRefugee"),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DownedRefugeeQuestUtility_GenerateRefugee_PreFix)),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DownedRefugeeQuestUtility_GenerateRefugee_PostFix)));

            harmony.Patch(original: AccessTools.Method(
                    type: typeof(PrisonerWillingToJoinQuestUtility),
                    name: "GeneratePrisoner"),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PrisonerWillingToJoinQuestUtility_GeneratePrisoner_PreFix)),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PrisonerWillingToJoinQuestUtility_GeneratePrisoner_PostFix)));

            harmony.Patch(original: AccessTools.Method(
                    type: typeof(Faction),
                    name: "TryMakeInitialRelationsWith"),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Faction_TryMakeInitialRelationsWith_PreFix)),
                    postfix: null);

            harmony.Patch(original: AccessTools.Method(
                    type: typeof(QuestUtility),
                    name: "GenerateQuestAndMakeAvailable",
                    parameters: new Type[] { typeof(QuestScriptDef), typeof(Slate) }),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(QuestUtility_GenerateQuestAndMakeAvailable_PreFix)),
                    postfix: null);

            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(FloatMenuMakerMap_AddHumanlikeOrders_PostFix)), null);
            harmony.Patch(AccessTools.Method(typeof(Pawn_EquipmentTracker), "EquipmentTrackerTick"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_EquipmentTracker_EquipmentTrackerTick_PostFix)), null);
            harmony.Patch(AccessTools.Method(typeof(WildManUtility), "IsWildMan"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(WildManUtility_IsWildMan_PostFix)), null);
            harmony.Patch(AccessTools.Method(typeof(WildManUtility), "AnimalOrWildMan"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(WildManUtility_AnimalOrWildMan_PostFix)), null);
            harmony.Patch(AccessTools.Method(typeof(WildManUtility), "NonHumanlikeOrWildMan"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(WildManUtility_NonHumanlikeOrWildMan_PostFix)), null);
            harmony.Patch(AccessTools.Method(typeof(Pawn), "PostApplyDamage"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_PostApplyDamage_PostFix)), null);
            harmony.Patch(AccessTools.Method(typeof(Verse.Pawn), nameof(Verse.Pawn.ButcherProducts)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ButcherProductsBeastmen_PostFix)), null);
            harmony.Patch(AccessTools.Method(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(GenRecipe_MakeRecipeProducts_PostFix)), null);
            harmony.Patch(AccessTools.Method(typeof(LordToil_PanicFlee), nameof(LordToil_PanicFlee.Init)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(LordToil_PanicFlee_LordToil_PanicFlee_PostFix)), null);

            harmony.Patch(AccessTools.Method(typeof(SettlementDefeatUtility), nameof(SettlementDefeatUtility.CheckDefeated)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(SettlementDefeatUtility_CheckDefeated_PostFix)), null);

        }

        static void SettlementDefeatUtility_CheckDefeated_PostFix(Settlement factionBase)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;
            if (ofPlayer == null)
                return;
            else if (!(ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                        || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction")) && !RH_TET_BeastmenMod.anyOneCanBeastActive)
                return;

            if (factionBase.Destroyed && factionBase.Faction.def != null && factionBase.Faction.def.defName != null)
            {
                String factionName = factionBase.Faction.def.defName;
                int totalSarsenCount = 0;

                if (factionName.Equals("RH_TET_Dwarf_KarakMountain") || factionName.Equals("RH_TET_EmpireOfMan"))
                {
                    List<Thing> herdstoneList = HerdUtility.getAllHerdstones();

                    Thing giftedHerdstone = null;
                    bool otherHerdstonesPresent = false;

                    if (herdstoneList != null && herdstoneList.Count > 0)
                    {
                        if (herdstoneList.Count > 1)
                        {
                            int rando = RH_TET_BeastmenMod.random.Next(0, herdstoneList.Count);
                            giftedHerdstone = herdstoneList[rando];
                            otherHerdstonesPresent = true;
                        }
                        else
                        {
                            giftedHerdstone = herdstoneList.First<Thing>();
                        }
                    }

                    if (giftedHerdstone != null)
                    {
                        Map map = giftedHerdstone.Map;

                        Dictionary<ThingDef, int> sarsenCounts = new Dictionary<ThingDef, int>();
                        sarsenCounts.Add(BeastmenDefOf.RH_TET_Beastmen_SarsenDouble_Conversion, 0);
                        sarsenCounts.Add(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Beacon, 0);
                        sarsenCounts.Add(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Gas, 0);
                        sarsenCounts.Add(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Psychic, 0);
                        sarsenCounts.Add(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Pulse, 0);
                        sarsenCounts.Add(BeastmenDefOf.RH_TET_Beastmen_Sarsen_Gifter, 0);
                        sarsenCounts.Add(BeastmenDefOf.RH_TET_Beastmen_Sarsen_Healer, 0);
                        sarsenCounts.Add(BeastmenDefOf.RH_TET_Beastmen_Sarsen_Tutor, 0);

                        List<Thing> sarsensOnMapList = new List<Thing>();
                        sarsensOnMapList.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_SarsenDouble_Conversion));
                        sarsensOnMapList.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Beacon));
                        sarsensOnMapList.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Gas));
                        sarsensOnMapList.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Psychic));
                        sarsensOnMapList.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_SarsenLintel_Pulse));
                        sarsensOnMapList.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_Sarsen_Gifter));
                        sarsensOnMapList.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_Sarsen_Healer));
                        sarsensOnMapList.AddRange(map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_Sarsen_Tutor));

                        if (sarsensOnMapList != null && sarsensOnMapList.Count > 0)
                        {
                            foreach (Thing sarsenOnMap in sarsensOnMapList)
                            {
                                if (sarsenOnMap != null)
                                {
                                    sarsenCounts.SetOrAdd(sarsenOnMap.def, (sarsenCounts.TryGetValue(sarsenOnMap.def) + 1));
                                    totalSarsenCount++;
                                }
                            }
                        }

                        // Order sarsenCounts by value(count).
                        Dictionary<ThingDef, int> sortedSarsensOnMap = sarsenCounts.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                        int minVal = 1000;
                        int maxVal = -1;

                        foreach (int sarsenCount in sortedSarsensOnMap.Values)
                        {
                            if (sarsenCount > maxVal)
                                maxVal = sarsenCount;
                            if (sarsenCount < minVal)
                                minVal = sarsenCount;
                        }

                        List<ThingDef> sarsensToChooseFrom = RH_TET_BeastmenMod.getSarsenList();

                        if (minVal != maxVal)
                        {
                            // Remove all sarsens that have a count = to maxVal in sortedSarsensOnMap from sarsensToChooseFrom.
                            foreach (ThingDef sarsenOnMapFromSorted in sortedSarsensOnMap.Keys)
                            {
                                if (sortedSarsensOnMap.TryGetValue(sarsenOnMapFromSorted) >= maxVal)
                                    sarsensToChooseFrom.Remove(sarsenOnMapFromSorted);
                            }
                        }

                        // Sarsens to choose from should have the available sarsens to give to the player.
                        if (sarsensToChooseFrom.Count > 0)
                        {
                            int rando = RH_TET_BeastmenMod.random.Next(0, sarsensToChooseFrom.Count);

                            // Creat it, spawn it, send a message.
                            ThingDef sarsenToGiftPlayer = sarsensToChooseFrom.ElementAt(rando);

                            IntVec3 spawnCell;
                            CellRect cellRectAroundHerdstone = new CellRect(giftedHerdstone.InteractionCell.x - 4, giftedHerdstone.InteractionCell.z - 4, 10, 12);
                            RewardUtility.TryFindSpawnCellForItem(cellRectAroundHerdstone, giftedHerdstone.Map, out spawnCell);

                            Thing madeSarsen = ThingMaker.MakeThing(sarsenToGiftPlayer);
                            GenSpawn.Spawn(madeSarsen.MakeMinified(), spawnCell, map);
                            totalSarsenCount++;

                            // Make it dramatic.
                            Find.CameraDriver.shaker.DoShake(2f);
                            SoundInfo info = SoundInfo.InMap(new TargetInfo(giftedHerdstone.InteractionCell, map, false), MaintenanceType.None);
                            SoundDefOf.Thunder_OnMap.PlayOneShot(info);

                            Find.LetterStack.ReceiveLetter("RH_TET_Beastmen_HerdstoneSarsenGiftLabel".Translate(), "RH_TET_Beastmen_HerdstoneSarsenGift".Translate(madeSarsen.Label), LetterDefOf.PositiveEvent, (string)null);

                            // Much simpler than below commented out code, and should be equally as effective.
                            if (totalSarsenCount >= 8 && !HerdUtility.finalDestructionQuestExposedYet(herdstoneList))
                            {
                                EventMakerUtility.SpawnFinalDestructionEndGameQuest(giftedHerdstone as Building_MassiveHerdstone);

                                (giftedHerdstone as Building_MassiveHerdstone).finalDestructionQuestExposed = true;
                            }
                        }
                        else
                        {
                            Log.Error("Something went wrong. No sarsens to choose from.");
                        }
                    }
                }
            }
        }

        static void LordToil_PanicFlee_LordToil_PanicFlee_PostFix(LordToil_PanicFlee __instance)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;
            if (ofPlayer == null)
                return;
            else if (!(ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                        || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction")) && !RH_TET_BeastmenMod.anyOneCanBeastActive)
                return;

            for (int index = 0; index < __instance.lord.ownedPawns.Count; ++index)
            {
                Pawn ownedPawn = __instance.lord.ownedPawns[index];

                if (ownedPawn != null && ownedPawn.Faction != null && !ownedPawn.Faction.def.defName.Equals("RH_TET_Beastmen_GorFaction"))
                    return;

                if (ownedPawn.Map.IsPlayerHome)
                    return;

                ownedPawn.mindState.mentalStateHandler.Reset();
                    ownedPawn.SetFaction(ofPlayer);
                    String letterLabel = (string)("LetterLabelMessageRecruitSuccess".Translate() + ": " + ownedPawn.LabelShortCap);
                    Find.LetterStack.ReceiveLetter((TaggedString)letterLabel, "RH_TET_Beastmen_MessageRecruitSuccess".Translate((NamedArgument)((Thing)ownedPawn), ownedPawn.Named("RECRUITEE")), LetterDefOf.PositiveEvent, (LookTargets)(ownedPawn), null, null, null, null);
            }
        }

        static void GenRecipe_MakeRecipeProducts_PostFix(ref IEnumerable<Thing> __result,
              RecipeDef recipeDef,
              Pawn worker,
              List<Thing> ingredients,
              Thing dominantIngredient,
              IBillGiver billGiver)
        {
            if (recipeDef.Equals(BeastmenDefOf.RH_TET_Beastmen_SpikeHead))
            {
                foreach (Thing t in ingredients)
                {
                    if (t.def != null && t.def.defName != null && t.def.defName.StartsWith("Corpse_"))
                    {
                        String thingToMakeDefName = null;
                        switch (t.def.defName)
                        {
                            case "Corpse_Human":
                                // Do nothing. It's already human. Exit.
                                return;
                            case "Corpse_RH_TET_Dwarf_Race_Standard":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeDawi";
                                break;
                            case "Corpse_RH_TET_Dwarf_Slayer_Race_Standard":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeSlayer";
                                break;
                            case "Corpse_RH_TET_Beastmen_Alien_BrayGor":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeBrayGor";
                                break;
                            case "Corpse_RH_TET_Beastmen_Alien_Gor":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeGor";
                                break;
                            case "Corpse_RH_TET_Beastmen_Alien_Ungor":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeUngor";
                                break;
                            case "Corpse_RH_TET_Beastmen_Alien_Bullgor":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeBullgor";
                                break;
                            case "Corpse_RH_TET_Beastmen_Alien_Cygor":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeCygor";
                                break;
                            case "Corpse_RH_TET_Beastmen_Alien_Khornegor":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeKhorngor";
                                break;
                            case "Corpse_RH_TET_Beastmen_Alien_Pestigor":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikePestigor";
                                break;
                            case "Corpse_RH_TET_Beastmen_Alien_Tzaangor":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeTzaangor";
                                break;
                            case "Corpse_RH_TET_Beastmen_Alien_Slaangor":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeSlaangor";
                                break;
                            case "Corpse_RH_TET_SkavenSlaveNaked":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeSkaven";
                                break;
                            case "Corpse_RH_TET_SkavenSlave":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeSkaven";
                                break;
                            case "Corpse_RH_TET_SkavenClanRat":
                                thingToMakeDefName = "RH_TET_Beastmen_HeadSpikeSkaven";
                                break;
                            default:
                                // Do nothing. It's a human or unknown, let it roll as human.
                                // Add new pawns here.
                                break;
                        }

                        if (thingToMakeDefName != null)
                        {
                            List<Thing> newThingList = new List<Thing>();
                            Thing headOnSpike = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(thingToMakeDefName));
                            Thing minifiredHeadOnSpike = headOnSpike.MakeMinified();
                            newThingList.Add(minifiredHeadOnSpike);
                            __result = newThingList;
                        }
                    }
                }
            }
        }

        static void ButcherProductsBeastmen_PostFix(Verse.Pawn __instance, ref IEnumerable<Thing> __result, Pawn butcher, float efficiency)
        {
            if (HerdUtility.IsBeastman(butcher))
            { 
                foreach (Thing thing in __result)
                {
                    if (thing != null && thing.def != null && thing.def.IsMeat)
                    {
                        thing.stackCount = thing.stackCount * 2;
                    }
                }
            }
        }

        private static void QuestUtility_GenerateQuestAndMakeAvailable_PreFix(ref Quest __result, ref QuestScriptDef root, ref Slate vars)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;
            if (ofPlayer != null && root != null && root.defName != null)
            { 
                // Should only happen to this mod factions.
                if (root.defName.Equals("RH_TET_Beastmen_CaravanNearby"))
                {
                    if (!(ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                        || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction")))
                    {
                        // For non this mod factions, swap it out with a different quest.
                        root = QuestScriptDefOf.OpportunitySite_ItemStash;
                    }
                }
                else if (root.defName.Equals("RH_TET_Beastmen_Homestead"))
                {
                    if (!(ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                        || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction")))
                    {
                        // For non this mod factions, swap it out with a different quest.
                        root = DefDatabase<QuestScriptDef>.GetNamed("OpportunitySite_PrisonerWillingToJoin");
                    }
                }
            }

            __result = null;
            return;
        }

        private static void Pawn_PostApplyDamage_PostFix(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            Pawn instigator = null;
            try
            {
                instigator = dinfo.Instigator as Pawn;
            }
            catch
            {
                // Ignore. Not a pawn, so do nothing.
            }

            try
            {
                if (instigator != null)
                {
                    Need_Violence need = (Need_Violence)instigator.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == BeastmenDefOf.RH_TET_Beastmen_NeedViolence));
                    if (need != null && !need.Disabled)
                    {
                        need.didViolence = true;
                    }
                }
            }
            catch
            {
                // Ignore again. Only beasts will have the need.
            }
        }

        private static void Faction_TryMakeInitialRelationsWith_PreFix(Faction __instance, Faction other)
        {
            Faction playerFaction = __instance;

            // Makes all factions hostile to beastmen.
            if (playerFaction.IsPlayer && !other.IsPlayer && (playerFaction.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                || playerFaction.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction")))
            {
                if (other.def.defName.Equals("RH_TET_Beastmen_GorFaction") 
                    || other.def.defName.Equals("RH_TET_Skaven_UnderEmpireFaction")) // Could be friendly with beastmen or skaven.
                    other.def.permanentEnemy = false;
                else
                    other.def.permanentEnemy = true;
            }
        }

        private static void PrisonerWillingToJoinQuestUtility_GeneratePrisoner_PreFix(ref Pawn __result, int tile, Faction hostFaction)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;

            if (ofPlayer != null)
            {
                if (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
                {
                    Slave = PawnKindDefOf.Slave;
                    int rando = RH_TET_BeastmenMod.random.Next(0, 10);

                    if (rando == 0 || rando == 4)
                    {
                        PawnKindDefOf.Slave = PawnKindDef.Named("RH_TET_Beastmen_Ravager");
                    }
                    else if (rando == 5 || rando == 7)
                    {
                        PawnKindDefOf.Slave = PawnKindDef.Named("RH_TET_Beastmen_Raider");
                    }
                    else
                    {
                        PawnKindDefOf.Slave = PawnKindDef.Named("RH_TET_Beastmen_RaiderMelee");
                    }
                }
            }
        }

        private static void PrisonerWillingToJoinQuestUtility_GeneratePrisoner_PostFix(ref Pawn __result, int tile, Faction hostFaction)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;

            if (ofPlayer != null)
            {
                if (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
                {
                    PawnKindDefOf.Slave = Slave;
                }
            }
        }

        private static void DownedRefugeeQuestUtility_GenerateRefugee_PreFix(int tile)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;

            if (ofPlayer != null)
            {
                if (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
                {
                    int rando = RH_TET_BeastmenMod.random.Next(0, 100);

                    if (rando < 3)
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_Beastmen_BrayShaman");
                    }
                    else if (rando < 40)
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_Beastmen_Ravager");
                    }
                    else if (rando < 70)
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_Beastmen_Raider");
                    }
                    else
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_Beastmen_RaiderMelee");
                    }
                }
            }
        }

        private static void DownedRefugeeQuestUtility_GenerateRefugee_PostFix(int tile)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;

            if (ofPlayer != null)
            {
                if (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
                {
                    PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("SpaceRefugee");
                }
            }
        }

        private static void QuestNode_GeneratePawn_RunInt_PreFix(QuestNode_GeneratePawn __instance)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;

            if (ofPlayer != null)
            {
                if (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
                {
                    int rando = RH_TET_BeastmenMod.random.Next(0, 100);

                    if (rando < 3)
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_Beastmen_BrayShaman");
                    }
                    else if (rando < 40)
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_Beastmen_Ravager");
                    }
                    else if (rando < 70)
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_Beastmen_Raider");
                    }
                    else
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_Beastmen_RaiderMelee");
                    }
                }
            }
        }

        private static void QuestNode_GeneratePawn_RunInt_PostFix(QuestNode_GeneratePawn __instance)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;

            if (ofPlayer != null)
            {
                if (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                    || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
                {
                    PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("SpaceRefugee");
                }
            }
        }

        private static void IncidentWorker_WandererJoin_TryExecuteWorker_PreFix(IncidentWorker_WandererJoin __instance, ref bool __result, IncidentParms parms)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;

            if (ofPlayer != null)
            {
                if (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
                {
                    if (__instance.def.defName.Equals("StrangerInBlackJoin"))
                    {
                        __instance.def.pawnKind = PawnKindDef.Named("RH_TET_Beastmen_Minotaur");
                        __instance.def.letterText = "Sensing your plight, and lured by the dark gods, a mysterious [PAWN_kind] has arrived.\n\nWill [PAWN_pronoun] be able to sway the battle?";
                        __instance.def.letterLabel = "Minotaur";
                    }
                    else if (__instance.def.defName.Equals("WandererJoin"))
                    {
                        int rando = RH_TET_BeastmenMod.random.Next(0, 100);

                        // Give the player a bray shaman if they don't have one.
                        Map map = (Map)parms.target;
                        bool hasBray = false;
                        foreach (Pawn candidate in map.mapPawns.FreeColonistsSpawned)
                        {
                            if (HerdUtility.IsBrayShaman(candidate) && !candidate.Dead)
                            {
                                hasBray = true;
                                break;
                            }
                        }

                        if (!hasBray || rando < 3)
                        {
                            __instance.def.pawnKind = PawnKindDef.Named("RH_TET_Beastmen_BrayShaman");
                        }
                        else if (rando < 5)
                        {
                            __instance.def.pawnKind = PawnKindDef.Named("RH_TET_Beastmen_GreatBrayShaman");
                        }
                        else if (rando < 25)
                        {
                            __instance.def.pawnKind = PawnKindDef.Named("RH_TET_Beastmen_Minotaur");
                        }
                        else if (rando < 45)
                        {
                            __instance.def.pawnKind = PawnKindDef.Named("RH_TET_Beastmen_Wargor");
                        }
                        else if (rando < 60)
                        {
                            __instance.def.pawnKind = PawnKindDef.Named("RH_TET_Beastmen_Ravager");
                        }
                        else if (rando < 75)
                        {
                            __instance.def.pawnKind = PawnKindDef.Named("RH_TET_Beastmen_Raider");
                        }
                        else if (rando < 95)
                        {
                            __instance.def.pawnKind = PawnKindDef.Named("RH_TET_Beastmen_RaiderMelee");
                        }
                        else
                        {
                            __instance.def.pawnKind = PawnKindDef.Named("RH_TET_Beastmen_CygorBeastmenPlayer");
                        }
                    }
                }
            }

            __result = true;
        }

        private static void IncidentWorker_WandererJoin_TryExecuteWorker_PostFix(IncidentWorker_WandererJoin __instance, ref bool __result, IncidentParms parms)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;

            if (ofPlayer != null)
            {
                if (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
                {
                    PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("SpaceRefugee");
                }
            }
        }

        private static void WildManUtility_NonHumanlikeOrWildMan_PostFix(IncidentWorker_WandererJoin __instance, ref bool __result, Pawn p)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;

            if (ofPlayer != null)
            {
                if (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                    || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
                {
                    if (p.kindDef == PawnKindDef.Named("RH_TET_Beastmen_WildGor")
                        || p.kindDef == PawnKindDef.Named("RH_TET_Beastmen_WildUngor")
                        || p.kindDef == PawnKindDef.Named("RH_TET_Beastmen_WildCygor"))
                    {
                        __result = true;
                    }
                }
            }
        }


        private static void WildManUtility_AnimalOrWildMan_PostFix(IncidentWorker_WandererJoin __instance, ref bool __result, Pawn p)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;

            if (ofPlayer != null && (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
              || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction")))
            {
                if (p.kindDef == PawnKindDef.Named("RH_TET_Beastmen_WildGor")
                    || p.kindDef == PawnKindDef.Named("RH_TET_Beastmen_WildUngor")
                    || p.kindDef == PawnKindDef.Named("RH_TET_Beastmen_WildCygor"))
                {
                    __result = true;
                }
            }
        }

        private static void WildManUtility_IsWildMan_PostFix(ref bool __result, Pawn p)
        {
            Faction ofPlayer = Faction.OfPlayerSilentFail;

            if (ofPlayer != null && (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
              || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction")))
            {
                if (p.kindDef == PawnKindDef.Named("RH_TET_Beastmen_WildGor")
                    || p.kindDef == PawnKindDef.Named("RH_TET_Beastmen_WildUngor")
                    || p.kindDef == PawnKindDef.Named("RH_TET_Beastmen_WildCygor"))
                {
                    __result = true;
                }
            }
        }

        private static void Pawn_EquipmentTracker_EquipmentTrackerTick_PostFix(Pawn_EquipmentTracker __instance)
        {
            List<ThingWithComps> equipmentListForReading = __instance.AllEquipmentListForReading;
            for (int index = 0; index < equipmentListForReading.Count; ++index)
            {
                if (equipmentListForReading[index].def.defName.Equals("RH_TET_MeleeWeapon_KhorneAxe")
                    || equipmentListForReading[index].def.defName.Equals("RH_TET_MeleeWeapon_NurgleStar")
                    || equipmentListForReading[index].def.defName.Equals("RH_TET_MeleeWeapon_SlaaneshWhip")
                    || equipmentListForReading[index].def.defName.Equals("RH_TET_MeleeWeapon_TzeentchStaff"))
                {
                    equipmentListForReading[index].Tick();
                }
            }
        }

        public static void FloatMenuMakerMap_AddHumanlikeOrders_PostFix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);

            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                foreach (LocalTargetInfo current5 in GenUI.TargetsAt(clickPos, GetTargetingParameters(), true))
                {
                    LocalTargetInfo localTargetInfo = current5;
                    Corpse victim = (Corpse)localTargetInfo.Thing;
                    Building_MassiveHerdstone herdstoneMaybeUnfueled = Building_MassiveHerdstone.FindMassiveHerdstoneFor(victim, pawn, false, true);
                    Building_MassiveHerdstone herdstone = Building_MassiveHerdstone.FindMassiveHerdstoneFor(victim, pawn, true, true);
                    if (victim.InnerPawn.RaceProps.Humanlike && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true) && herdstoneMaybeUnfueled != null)
                    {
                        if (herdstone != null)
                        {
                            string text4 = "RH_TET_Beastmen_CarryToMassiveHerdstone".Translate(localTargetInfo.Thing.LabelCap, localTargetInfo.Thing);
                            JobDef jDef = BeastmenDefOf.RH_TET_Beastmen_CarryToMassiveHerdstone;
                            Action action3 = delegate
                            {
                                Building_MassiveHerdstone building_MassiveHerdstone = Building_MassiveHerdstone.FindMassiveHerdstoneFor(victim, pawn, true, false);
                                // It was unfueled.
                                if (building_MassiveHerdstone == null)
                                {
                                    building_MassiveHerdstone = Building_MassiveHerdstone.FindMassiveHerdstoneFor(victim, pawn, true, true);
                                }
                                if (building_MassiveHerdstone == null)
                                {
                                    Messages.Message("RH_TET_Beastmen_CannotCarryToMassiveHerdstone".Translate() + ": " + "RH_TET_Beastmen_NoMassiveHerdstone".Translate(), victim, MessageTypeDefOf.RejectInput, false);
                                    return;
                                }
                                Job job = new Job(jDef, victim, building_MassiveHerdstone);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            };
                            string label = text4;
                            Action action2 = action3;
                            Corpse revalidateClickTarget = victim;
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2, MenuOptionPriority.Default, null, revalidateClickTarget, 0f, null, null), pawn, victim, "ReservedBy"));
                        }
                        else
                        {
                            // Only valid herdstone is unfueled.
                            opts.Add(new FloatMenuOption("RH_TET_Beastmen_NoMassiveHerdstone".Translate() + " (" + "RH_TET_Beastmen_HerdstoneRequiresFuel".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                    }
                }
            }
        }

        private static TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = false,
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = ((TargetInfo x) => x.Thing is Corpse && BaseTargetValidator(x.Thing))
            };
        }

        // Requires humanlike corpses allows dessicated.
        private static bool BaseTargetValidator(Thing t)
        {
            Corpse corpse = t as Corpse;
            if (corpse != null && !corpse.InnerPawn.RaceProps.IsFlesh)
            {
                return false;
            }

            return true;
        }
    }
}