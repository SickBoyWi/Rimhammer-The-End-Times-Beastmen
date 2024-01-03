using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheEndTimes_Beastmen
{
    public static class GettingSoftUtility
    {
        private static List<GettingSoftDef> expectationsInOrder = null;

        public static void Reset()
        {
            GettingSoftUtility.expectationsInOrder = DefDatabase<GettingSoftDef>.AllDefs.OrderBy<GettingSoftDef, float>((Func<GettingSoftDef, float>)(ed => ed.maxMapWealth)).ToList<GettingSoftDef>();
        }

        public static GettingSoftDef CurrentExpectationFor(Pawn p)
        {
            if (expectationsInOrder == null)
                GettingSoftUtility.Reset();

            if (Current.ProgramState != ProgramState.Playing)
                return (GettingSoftDef)null;
            if (!Settings.UseHerdWealthMoodModifier || !HerdUtility.IsBeastman(p))
                return (GettingSoftDef)null;
            if (p.Faction != Faction.OfPlayer && !p.IsPrisonerOfColony)
                return BeastmenDefOf.RH_TET_Beastmen_ExtremelyLow;
            if (p.MapHeld != null)
                return GettingSoftUtility.CurrentExpectationFor(p.MapHeld, p);
            return BeastmenDefOf.RH_TET_Beastmen_VeryLow;
        }

        public static GettingSoftDef CurrentExpectationFor(Map m, Pawn p)
        {
            float wealthTotal = m.wealthWatcher.WealthTotal - m.wealthWatcher.WealthPawns;
            wealthTotal -= CalculateWealthWeapons(m);
            wealthTotal -= GetEquipmentApparelAndInventoryWealth(p);

            for (int index = 0; index < GettingSoftUtility.expectationsInOrder.Count; ++index)
            {
                GettingSoftDef expectationDef = GettingSoftUtility.expectationsInOrder[index];
                if ((double)wealthTotal < (double)expectationDef.maxMapWealth)
                    return expectationDef;
            }
            return GettingSoftUtility.expectationsInOrder[GettingSoftUtility.expectationsInOrder.Count - 1];
        }

        public static float GetEquipmentApparelAndInventoryWealth(Pawn p)
        {
            float num = 0.0f;
            if (p.equipment != null)
            {
                List<ThingWithComps> equipmentListForReading = p.equipment.AllEquipmentListForReading;
                for (int index = 0; index < equipmentListForReading.Count; ++index)
                { 
                    if (equipmentListForReading[index].def != null && equipmentListForReading[index].def.IsWeapon)
                        num += equipmentListForReading[index].MarketValue * (float)equipmentListForReading[index].stackCount;
                }
            }
            if (p.inventory != null)
            {
                ThingOwner<Thing> innerContainer = p.inventory.innerContainer;
                for (int index = 0; index < innerContainer.Count; ++index)
                {
                    if (innerContainer[index].def != null && innerContainer[index].def.IsWeapon)
                        num += innerContainer[index].MarketValue * (float)innerContainer[index].stackCount;
                }
            }
            return num;
        }

        private static float CalculateWealthWeapons(Map m)
        {
            List<Thing> tmpThings = new List<Thing>();
            ThingOwnerUtility.GetAllThingsRecursively<Thing>(m, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), tmpThings, false, (Predicate<IThingHolder>)(x =>
            {
                Pawn p = null;
                switch (x)
                {
                    case PassingShip _:
                    case MapComponent _:
                        return false;
                    case Pawn pp:
                        p = pp;
                        if (pp.Faction != Faction.OfPlayer)
                            return false;
                        break;
                }
                return p == null || !p.IsQuestLodger();
            }), true);
            float num = 0.0f;
            for (int index = 0; index < tmpThings.Count; ++index)
            {
                if (tmpThings[index].SpawnedOrAnyParentSpawned && !tmpThings[index].PositionHeld.Fogged(m) && tmpThings[index].def != null && tmpThings[index].def.IsWeapon)
                    num += tmpThings[index].MarketValue * (float)tmpThings[index].stackCount;
            }
            return num;
        }
    }
}
