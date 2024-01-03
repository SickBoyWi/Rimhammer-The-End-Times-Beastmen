using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace TheEndTimes_Beastmen.RandomHarmonyStuff
{
    class Harmony_Patch_Cauldron
    {
        [HarmonyPatch(typeof(RimWorld.Building_NutrientPasteDispenser))]
        [HarmonyPatch("CanDispenseNow", MethodType.Getter)]
        static class Patch_Building_NutrientPasteDispenser_CanDispenseNow_Getter
        {
            static void Postfix(Building_NutrientPasteDispenser __instance, ref bool __result)
            {
                if (__instance.def.defName == "RH_TET_Beastmen_CauldronSlopDispenser")
                {
                    if (!__result)
                    {
                        if (__instance.GetComp<CompRefuelable>().HasFuel)
                            __result = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.Building_NutrientPasteDispenser))]
        [HarmonyPatch("AdjacentReachableHopper")]
        static class Patch_Building_NutrientPasteDispenser_AdjacentReachableHopper
        {
            static void Postfix(Building_NutrientPasteDispenser __instance, ref Building __result, Pawn reacher)
            {
                if (__instance.def.defName == "RH_TET_Beastmen_CauldronSlopDispenser" && __result == null)
                {
                    for (int index = 0; index < __instance.AdjCellsCardinalInBounds.Count; ++index)
                    {
                        Building edifice = __instance.AdjCellsCardinalInBounds[index].GetEdifice(__instance.Map);
                        if (edifice != null && edifice.def == BeastmenDefOf.RH_TET_Beastmen_SlopBucket && reacher.CanReach((LocalTargetInfo)((Thing)edifice), PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                        { 
                            __result = edifice;
                            break;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.Alert_PasteDispenserNeedsHopper))]
        [HarmonyPatch("BadDispensers", MethodType.Getter)]
        static class Patch_Alert_PasteDispenserNeedsHopper_BadDispensers_Get
        {
            static void Postfix(Alert_PasteDispenserNeedsHopper __instance, ref List<Thing> __result)
            {
                if (__result.Count > 0)
                {
                    List<Thing> cauldronsToRemoveFromList = new List<Thing>();

                    foreach (Thing foodBuilding in __result)
                    {
                        if (foodBuilding.def != null && foodBuilding.def.Equals(BeastmenDefOf.RH_TET_Beastmen_CauldronSlopDispenser))
                        {
                            bool flag = false;
                            ThingDef slopBucket = BeastmenDefOf.RH_TET_Beastmen_SlopBucket;
                            foreach (IntVec3 cellsCardinalInBound in ((Building_Cauldron)foodBuilding).AdjCellsCardinalInBounds)
                            {
                                Thing edifice = (Thing)cellsCardinalInBound.GetEdifice(foodBuilding.Map);
                                if (edifice != null && edifice.def == slopBucket)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                                cauldronsToRemoveFromList.Add(foodBuilding);
                        }
                    }

                    foreach(Thing thing in cauldronsToRemoveFromList)
                    {
                        if (__result.Contains(thing))
                            __result.Remove(thing);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.Building_NutrientPasteDispenser))]
        [HarmonyPatch("TryDispenseFood")]
        static class Patch_Building_NutrientPasteDispenser_TryDispenseFood
        {
            static void Postfix(Building_NutrientPasteDispenser __instance, ref Thing __result)
            {
                if (__instance.def.defName == "RH_TET_Beastmen_CauldronSlopDispenser" && __result == null)
                {
                    Building_Cauldron cauldron = __instance as Building_Cauldron;

                    if (!cauldron.CanDispenseNow)
                        return;
                    float num = __instance.def.building.nutritionCostPerDispense - 0.0001f;
                    List<ThingDef> thingDefList = new List<ThingDef>();
                    do
                    {
                        Thing feedInAnyHopper = cauldron.FindFeedInAnyHopper();
                        if (feedInAnyHopper == null)
                        {
                            return;
                        }
                        int count = Mathf.Min(feedInAnyHopper.stackCount, Mathf.CeilToInt(num / feedInAnyHopper.GetStatValue(StatDefOf.Nutrition, true)));
                        num -= (float)count * feedInAnyHopper.GetStatValue(StatDefOf.Nutrition, true);
                        thingDefList.Add(feedInAnyHopper.def);
                        feedInAnyHopper.SplitOff(count);
                    }
                    while ((double)num > 0.0);
                    __instance.def.building.soundDispense.PlayOneShot((SoundInfo)new TargetInfo(__instance.Position, __instance.Map, false));
                    Thing thing = ThingMaker.MakeThing(cauldron.DispensableDef, (ThingDef)null);
                    CompIngredients comp = thing.TryGetComp<CompIngredients>();
                    for (int index = 0; index < thingDefList.Count; ++index)
                        comp.RegisterIngredient(thingDefList[index]);
                    __result = thing;
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.Building_NutrientPasteDispenser))]
        [HarmonyPatch("FindFeedInAnyHopper")]
        static class Patch_Building_NutrientPasteDispenser_FindFeedInAnyHopper
        {
            static void Postfix(Building_NutrientPasteDispenser __instance, ref Thing __result)
            {
                if (__instance.def.defName == "RH_TET_Beastmen_CauldronSlopDispenser" && __result == null)
                {
                    for (int index1 = 0; index1 < __instance.AdjCellsCardinalInBounds.Count; ++index1)
                    {
                        Thing thing1 = (Thing)null;
                        Thing thing2 = (Thing)null;
                        List<Thing> thingList = __instance.AdjCellsCardinalInBounds[index1].GetThingList(__instance.Map);
                        for (int index2 = 0; index2 < thingList.Count; ++index2)
                        {
                            Thing thing3 = thingList[index2];
                            if (Building_Cauldron.IsAcceptableFeedstock(thing3.def))
                                thing1 = thing3;
                            if (thing3.def == BeastmenDefOf.RH_TET_Beastmen_SlopBucket)
                                thing2 = thing3;
                        }
                        if (thing1 != null && thing2 != null)
                        {
                            __result = thing1;
                            return;
                        }
                    }
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.Building_NutrientPasteDispenser))]
        [HarmonyPatch("HasEnoughFeedstockInHoppers")]
        static class Patch_Building_NutrientPasteDispenser_HasEnoughFeedstockInHoppers
        {
            static void Postfix(Building_NutrientPasteDispenser __instance, ref bool __result)
            {
                if (__instance.def.defName == "RH_TET_Beastmen_CauldronSlopDispenser" && __result == false)
                {
                    float num = 0.0f;
                    Building_Cauldron cauldron = __instance as Building_Cauldron;
                    for (int index1 = 0; index1 < __instance.AdjCellsCardinalInBounds.Count; ++index1)
                    {
                        IntVec3 cellsCardinalInBound = __instance.AdjCellsCardinalInBounds[index1];
                        Thing thing1 = (Thing)null;
                        Thing thing2 = (Thing)null;
                        Map map = __instance.Map;
                        List<Thing> thingList = cellsCardinalInBound.GetThingList(map);
                        for (int index2 = 0; index2 < thingList.Count; ++index2)
                        {
                            Thing thing3 = thingList[index2];
                            if (Building_Cauldron.IsAcceptableFeedstock(thing3.def))
                                thing1 = thing3;
                            if (thing3.def == BeastmenDefOf.RH_TET_Beastmen_SlopBucket)
                                thing2 = thing3;
                        }
                        if (thing1 != null && thing2 != null)
                            num += (float)thing1.stackCount * thing1.GetStatValue(StatDefOf.Nutrition, true);
                        if ((double)num >= (double)cauldron.def.building.nutritionCostPerDispense)
                        {
                            __result = true;
                            return;
                        }
                    }
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.FoodUtility))]
        [HarmonyPatch("BestFoodSourceOnMap_NewTemp")]
        static class Patch_FoodUtility_BestFoodSourceOnMap
        {
            static void Postfix(ref Thing __result,
                  Pawn getter,
                  Pawn eater,
                  bool desperate,
                  ref ThingDef foodDef,
                  FoodPreferability maxPref = FoodPreferability.MealLavish,
                  bool allowPlant = true,
                  bool allowDrug = true,
                  bool allowCorpse = true,
                  bool allowDispenserFull = true,
                  bool allowDispenserEmpty = true,
                  bool allowForbidden = false,
                  bool allowSociallyImproper = false,
                  bool allowHarvest = false,
                  bool forceScanWholeMap = false,
                  bool ignoreReservations = false,
                  FoodPreferability minPrefOverride = FoodPreferability.Undefined)
            {
                if (__result == null && getter.RaceProps.Humanlike)
                {
                    bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
                    if (!getterCanManipulate && getter != eater)
                    {
                        return;
                    }
                    FoodPreferability minPref = minPrefOverride != FoodPreferability.Undefined ? minPrefOverride : (!eater.NonHumanlikeOrWildMan() ? (!desperate ? (eater.needs.food.CurCategory >= HungerCategory.UrgentlyHungry ? FoodPreferability.RawBad : FoodPreferability.MealAwful) : FoodPreferability.DesperateOnly) : FoodPreferability.NeverForNutrition);
                    Predicate<Thing> foodValidator = (Predicate<Thing>)(t =>
                    {
                        Building_Cauldron cauldron = t as Building_Cauldron;
                        if (cauldron != null)
                        {
                            if (!allowDispenserFull || !getterCanManipulate || (BeastmenDefOf.RH_TET_Beastmen_SlopMeal.ingestible.preferability < minPref || BeastmenDefOf.RH_TET_Beastmen_SlopMeal.ingestible.preferability > maxPref) || (!eater.WillEat(BeastmenDefOf.RH_TET_Beastmen_SlopMeal, getter, true) || t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter) || !cauldron.refuelableComp.HasFuel || (!allowDispenserEmpty && !cauldron.HasEnoughFeedstockInHoppers() || (!t.InteractionCell.Standable(t.Map)))) || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map, false), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false)))
                                return false;
                        }
                        return true;
                    });
                    Thing bestThing = null;
                    if (getter.RaceProps.Humanlike)
                    {
                        bestThing = Harmony_Patch_Cauldron.SpawnedFoodSearchInnerScan(eater, getter.Position, getter.Map.listerThings.ThingsMatching(ThingRequest.ForDef(BeastmenDefOf.RH_TET_Beastmen_CauldronSlopDispenser)), PathEndMode.ClosestTouch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, foodValidator);
                        if (allowHarvest & getterCanManipulate)
                        {
                            int searchRegionsMax = !forceScanWholeMap || bestThing != null ? 30 : -1;
                            Thing foodSource = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForGroup(ThingRequestGroup.HarvestablePlant), PathEndMode.Touch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, (Predicate<Thing>)(x =>
                            {
                                return false;
                            }), (IEnumerable<Thing>)null, 0, searchRegionsMax, false, RegionType.Set_Passable, false);
                            if (foodSource != null)
                            {
                                bestThing = foodSource;
                                foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, true);
                            }
                        }
                        if (foodDef == null && bestThing != null)
                            foodDef = FoodUtility.GetFinalIngestibleDef(bestThing, false);
                    }
                    __result = bestThing;
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.FoodUtility))]
        [HarmonyPatch("GetMaxAmountToPickup")]
        static class Patch_FoodUtility_GetMaxAmountToPickup
        {
            static void Postfix(ref int __result, Thing food, Pawn pawn, int wantedCount)
            {
                if (food is Building_Cauldron)
                    __result = !pawn.CanReserve((LocalTargetInfo)food, 1, -1, (ReservationLayerDef)null, false) ? 0 : -1;
            }
        }

        [HarmonyPatch(typeof(RimWorld.FoodUtility))]
        [HarmonyPatch("ThoughtsFromIngesting")]
        static class Patch_FoodUtility_ThoughtsFromIngesting
        {
            static void Postfix(
              ref List<FoodUtility.ThoughtFromIngesting> __result,
              List<FoodUtility.ThoughtFromIngesting> ___ingestThoughts,
              List<ThoughtDef> ___extraIngestThoughtsFromTraits,
              Pawn ingester,
              Thing foodSource,
              ThingDef foodDef)
            {
                if (ingester.needs == null || ingester.needs.mood == null || !ingester.RaceProps.Humanlike)
                    return;
                Building_Cauldron cauldron = foodSource as Building_Cauldron;

                if (cauldron != null)
                {
                    Thing feedInAnyHopper = cauldron.FindFeedInAnyHopper();
                    if (feedInAnyHopper != null)
                    {
                        Harmony_Patch_Cauldron.AddIngestThoughtsFromIngredient(feedInAnyHopper.def, ingester, ___ingestThoughts, ___extraIngestThoughtsFromTraits, out bool _, out bool _);

                        MeatSourceCategory meatSourceCategory = FoodUtility.GetMeatSourceCategory(feedInAnyHopper.def);

                        if (meatSourceCategory == MeatSourceCategory.Humanlike)
                        {
                            if (HerdUtility.IsBeastman(ingester))
                            {
                                FoodUtility.ThoughtFromIngesting thoughtFromIngesting = new FoodUtility.ThoughtFromIngesting()
                                {
                                    thought = BeastmenDefOf.RH_TET_Beastmen_GorAteHumanlikeMeatAsIngredient
                                };
                                ___ingestThoughts.Add(thoughtFromIngesting);

                                // TODO JEH 1.3 THIS IS A HACK. Otherwise, they never get the thought/memory.
                                ingester.needs?.mood?.thoughts.memories.TryGainMemory(BeastmenDefOf.RH_TET_Beastmen_GorAteHumanlikeMeatAsIngredient, (Pawn)null, (Precept)null);
                            }
                        }

                        if (___ingestThoughts != null && ___ingestThoughts.Count > 0)
                        {
                            __result.AddRange(___ingestThoughts);
                        }
                    }
                }
                return;
            }
        }

        [HarmonyPatch(typeof(RimWorld.JobDriver_Ingest))]
        [HarmonyPatch("GetReport")]
        static class Patch_JobDriver_Ingest_GetReport
        {
            static void Postfix(
              JobDriver_Ingest __instance,
              ref string __result)
            {
                if (__instance.job.targetA.Thing != null && (__instance.job.targetA.Thing.def.Equals(BeastmenDefOf.RH_TET_Beastmen_CauldronSlopDispenser) || __instance.job.targetA.Thing.def.Equals(BeastmenDefOf.RH_TET_Beastmen_SlopMeal)))
                {
                    __result = JobUtility.GetResolvedJobReportRaw(__instance.job.def.reportString, BeastmenDefOf.RH_TET_Beastmen_SlopMeal.label, (object)BeastmenDefOf.RH_TET_Beastmen_SlopMeal, "", "", "", "");
                }
                return;
            }
        }

        [HarmonyPatch(typeof(RimWorld.JobDriver_FoodDeliver))]
        [HarmonyPatch("GetReport")]
        static class Patch_JobDriver_FoodDeliver_GetReport
        {
            static void Postfix(
              JobDriver_Ingest __instance,
              ref string __result)
            {
                if (__instance.job.targetA.Thing != null && (__instance.job.targetA.Thing.def.Equals(BeastmenDefOf.RH_TET_Beastmen_CauldronSlopDispenser) || __instance.job.targetA.Thing.def.Equals(BeastmenDefOf.RH_TET_Beastmen_SlopMeal)) && __instance.job.targetB != null && __instance.job.targetB.Thing != null)
                {
                    __result = JobUtility.GetResolvedJobReportRaw(__instance.job.def.reportString, BeastmenDefOf.RH_TET_Beastmen_SlopMeal.label, (object)BeastmenDefOf.RH_TET_Beastmen_SlopMeal, __instance.job.targetB.Thing.LabelShort, (object)__instance.job.targetB.Thing, "", (object)"");
                }
                return;
            }
        }

        [HarmonyPatch(typeof(RimWorld.JobDriver_FoodFeedPatient))]
        [HarmonyPatch("GetReport")]
        static class Patch_JobDriver_FoodFeedPatient_GetReport
        {
            static void Postfix(
              JobDriver_Ingest __instance,
              ref string __result)
            {
                if (__instance.job.targetA.Thing != null && (__instance.job.targetA.Thing.def.Equals(BeastmenDefOf.RH_TET_Beastmen_CauldronSlopDispenser) || __instance.job.targetA.Thing.def.Equals(BeastmenDefOf.RH_TET_Beastmen_SlopMeal)) && __instance.job.targetB != null && __instance.job.targetB.Thing != null)
                {
                    __result = JobUtility.GetResolvedJobReportRaw(__instance.job.def.reportString, BeastmenDefOf.RH_TET_Beastmen_SlopMeal.label, (object)BeastmenDefOf.RH_TET_Beastmen_SlopMeal, __instance.job.targetB.Thing.LabelShort, (object)__instance.job.targetB.Thing, "", (object)"");
                }
                return;
            }
        }

        // UTILS: pulled from core source because they're private there.
        private static Thing SpawnedFoodSearchInnerScan(
          Pawn eater,
          IntVec3 root,
          List<Thing> searchSet,
          PathEndMode peMode,
          TraverseParms traverseParams,
          float maxDistance = 9999f,
          Predicate<Thing> validator = null)
        {
            if (searchSet == null)
                return (Thing)null;
            Pawn pawn = traverseParams.pawn ?? eater;
            int num1 = 0;
            int num2 = 0;
            Thing thing = (Thing)null;
            float num3 = float.MinValue;
            for (int index = 0; index < searchSet.Count; ++index)
            {
                Thing search = searchSet[index];
                ++num2;
                float lengthManhattan = (float)(root - search.Position).LengthManhattan;
                if ((double)lengthManhattan <= (double)maxDistance)
                {
                    float num4 = FoodUtility.FoodOptimality(eater, search, FoodUtility.GetFinalIngestibleDef(search, false), lengthManhattan, false);
                    if ((double)num4 >= (double)num3 && pawn.Map.reachability.CanReach(root, (LocalTargetInfo)search, peMode, traverseParams) && search.Spawned && (validator == null || validator(search)))
                    {
                        thing = search;
                        num3 = num4;
                        ++num1;
                    }
                }
            }
            return thing;
        }

        private static void AddIngestThoughtsFromIngredient(
          ThingDef ingredient,
          Pawn ingester,
          List<FoodUtility.ThoughtFromIngesting> ingestThoughts,
          List<ThoughtDef> extraIngestThoughtsFromTraits,
          out bool ateFungus,
          out bool ateNonFungusRawPlant)
        {
            ateFungus = false;
            ateNonFungusRawPlant = false;
            if (ingredient.ingestible == null)
                return;
            MeatSourceCategory meatSourceCategory = FoodUtility.GetMeatSourceCategory(ingredient);
            extraIngestThoughtsFromTraits.Clear();
            ingester.story?.traits?.GetExtraThoughtsFromIngestion(extraIngestThoughtsFromTraits, ingredient, meatSourceCategory, false);
            for (int index = 0; index < extraIngestThoughtsFromTraits.Count; ++index)
                TryAddIngestThought(ingester, extraIngestThoughtsFromTraits[index], (Precept)null, ingestThoughts, ingredient, meatSourceCategory);
            if (ingredient.ingestible.specialThoughtAsIngredient != null)
                TryAddIngestThought(ingester, ingredient.ingestible.specialThoughtAsIngredient, (Precept)null, ingestThoughts, ingredient, meatSourceCategory);
            if (meatSourceCategory == MeatSourceCategory.Humanlike)
            {
                AddThoughtsFromIdeo(HistoryEventDefOf.AteHumanMeat, ingester, ingredient, meatSourceCategory, ingestThoughts);
                AddThoughtsFromIdeo(HistoryEventDefOf.AteHumanMeatAsIngredient, ingester, ingredient, meatSourceCategory, ingestThoughts);
            }
            else if (FoodUtility.IsVeneratedAnimalMeatOrCorpse(ingredient, ingester, (Thing)null))
                AddThoughtsFromIdeo(HistoryEventDefOf.AteVeneratedAnimalMeat, ingester, ingredient, meatSourceCategory, ingestThoughts);
            if (meatSourceCategory == MeatSourceCategory.Insect)
                AddThoughtsFromIdeo(HistoryEventDefOf.AteInsectMeatAsIngredient, ingester, ingredient, meatSourceCategory, ingestThoughts);
            if (!ModsConfig.IdeologyActive || ingredient.thingCategories == null || !ingredient.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw))
                return;
            if (ingredient.IsFungus)
            {
                AddThoughtsFromIdeo(HistoryEventDefOf.AteFungusAsIngredient, ingester, ingredient, meatSourceCategory, ingestThoughts);
                ateFungus = true;
            }
            else
                ateNonFungusRawPlant = true;
        }

        private static void TryAddIngestThought(
          Pawn ingester,
          ThoughtDef def,
          Precept fromPrecept,
          List<FoodUtility.ThoughtFromIngesting> ingestThoughts,
          ThingDef foodDef,
          MeatSourceCategory meatSourceCategory)
        {
            FoodUtility.ThoughtFromIngesting thoughtFromIngesting = new FoodUtility.ThoughtFromIngesting()
            {
                thought = def,
                fromPrecept = fromPrecept
            };
            if (ingester.story != null && ingester.story.traits != null)
            {
                if (ingester.story.traits.IsThoughtFromIngestionDisallowed(def, foodDef, meatSourceCategory))
                    return;
                ingestThoughts.Add(thoughtFromIngesting);
            }
            else
                ingestThoughts.Add(thoughtFromIngesting);
        }

        private static void AddThoughtsFromIdeo(
             HistoryEventDef eventDef,
             Pawn ingester,
             ThingDef foodDef,
             MeatSourceCategory meatSourceCategory,
             List<FoodUtility.ThoughtFromIngesting> ingestThoughts)
        {
            if (ingester.Ideo == null)
                return;
            List<Precept> preceptsListForReading = ingester.Ideo.PreceptsListForReading;
            for (int index1 = 0; index1 < preceptsListForReading.Count; ++index1)
            {
                List<PreceptComp> comps = preceptsListForReading[index1].def.comps;
                for (int index2 = 0; index2 < comps.Count; ++index2)
                {
                    if (comps[index2] is PreceptComp_SelfTookMemoryThought tookMemoryThought && tookMemoryThought.eventDef == eventDef)
                        TryAddIngestThought(ingester, tookMemoryThought.thought, preceptsListForReading[index1], ingestThoughts, foodDef, meatSourceCategory);
                }
            }
        }
    }
}
