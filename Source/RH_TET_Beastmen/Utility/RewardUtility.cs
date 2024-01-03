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
using Verse.Sound;

namespace TheEndTimes_Beastmen
{
    public static class RewardUtility
    {
        internal static void ProcessBurnedPawn(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            // One sixth chance that there will bo no gift. TODO:This made players mad, lol.
            //int randomMaybe = random.Next(0, 6);
            int randomMaybe = 3;

            if ((darkGod.PlayerFavor == 8 || darkGod.PlayerFavor == 64) || (randomMaybe > 0 && darkGod.PlayerFavor % 8 == 0))
            { 
                ProcessKhorneRewards(darkGod, herdstone);
            }
            else
            { 
                // Track the burned pawns count.
                Messages.Message("RH_TET_Beastmen_HerdstoneBodyCount".Translate(darkGod.PlayerFavor), new TargetInfo(herdstone.Position, herdstone.Map, false), MessageTypeDefOf.PositiveEvent, true);
            }
        }

        internal static void ProcessBurnedDessicatedRottenPawn(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            // One sixth chance that there will bo no gift. TODO:This made players mad, lol.
            //int randomMaybe = random.Next(0, 6);
            int randomMaybe = 3;

            if ((darkGod.PlayerFavor == 7 || darkGod.PlayerFavor == 49) || (randomMaybe > 0 && darkGod.PlayerFavor % 7 == 0))
            {
                ProcessNurgleRewards(darkGod, herdstone);
            }
            else
            {
                // Track the burned dessicated pawns count.
                Messages.Message("RH_TET_Beastmen_HerdstoneDessicatedMessage".Translate(darkGod.PlayerFavor), new TargetInfo(herdstone.Position, herdstone.Map, false), MessageTypeDefOf.PositiveEvent, true);
            }
        }

        public static bool TryFindSpawnCellForItem(CellRect rect, Map map, out IntVec3 result)
        {
            return CellFinder.TryFindRandomCellInsideWith(rect, (Predicate<IntVec3>)(c =>
            {
                if (c.GetFirstItem(map) != null)
                    return false;
                if (!c.Standable(map))
                {
                    switch (c.GetSurfaceType(map))
                    {
                        case SurfaceType.Item:
                        case SurfaceType.Eat:
                            break;
                        default:
                            return false;
                    }
                }
                return true;
            }), out result);
        }

        internal static bool PleasesPapaNurgle(RotStage inRotStage)
        {
            if (inRotStage == RotStage.Rotting || inRotStage == RotStage.Dessicated)
            {
                return true;
            }
            return false;
        }

        private static void ProcessKhorneRewards(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            float corpseCount = darkGod.PlayerFavor;
            bool success = false;
            
            // Letter
            LetterDef letterDef = BeastmenDefOf.RH_TET_Beastmen_KhorneMessage;
            string textLabel = "RH_TET_Beastmen_FavorOf".Translate() + " " + "RH_TET_Beastmen_Khorne".Translate();
            StringBuilder s = new StringBuilder();

            s.Append("\n\n" + "RH_TET_Beastmen_HerdstoneBodyCount".Translate(corpseCount));
            s.Append("\n\n" + "RH_TET_Beastmen_KhorneInfo".Translate());

            // This one is for the first khorne weapon. It'll keep people motivated to keep burning corpses.
            if (corpseCount == 8f)
            {
                // Weapon: axe of khorne.
                IntVec3 spawnCell;
                CellRect cellRectAroundHerdstone = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                TryFindSpawnCellForItem(cellRectAroundHerdstone, herdstone.Map, out spawnCell);

                Thing axeOfKhorne = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_MeleeWeapon_KhorneAxe);
                axeOfKhorne.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                GenSpawn.Spawn(axeOfKhorne, spawnCell, herdstone.Map);
                
                // Unique.
                s.Append("\n\n" + "RH_TET_Beastmen_Khorne".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(axeOfKhorne.Label));

                success = true;
            }
            // This one is for the new god gor pawn.
            else if (rewardThresholdSquaredCrossed(darkGod))
            {
                // Consider adding in a chance that this fails, for now, let it happen every time. If OP, add random chance on the times after the first (that one will always work).
                // Spawn khornegor.
                IntVec3 spawnCell4;
                CellRect cellRectAroundHerdstone4 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                TryFindSpawnCellForItem(cellRectAroundHerdstone4, herdstone.Map, out spawnCell4);
                PawnGenerationRequest request = new PawnGenerationRequest(BeastmenDefOf.RH_TET_Beastmen_Khornegor, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, 
                    true, false, 
                    false, true, true, 
                    0.05f, true, true,
                    true,
                    false, false, false, 
                    false, false, false, 
                    0.0f, 0.0f, null, 0.0f,
                    null, null, null, 
                    null, null, null, 
                    null, null, null,
                    null, null, null);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                try
                {
                    pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
                }
                catch
                {
                    // Ignore if no ideos present.
                }
                pawn.skills.skills.Where(w => w.def.defName == "Melee").First().passion = Passion.Major;
                GenSpawn.Spawn(pawn, spawnCell4, herdstone.Map, WipeMode.Vanish);

                // Unique.
                s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextPawn".Translate("RH_TET_Beastmen_Khorne".Translate(), pawn.def.label));

                success = true;
            }
            else
            {
                int rando = RH_TET_BeastmenMod.random.Next(0, 100);

                // Remaining gifts from Khorne: (even odds on each)
                //      Healing Potion, Human Meat x 64, Horn of Khorne, Coal/Steel x 128, Legendary Bestigor Axe, Beer x 16, melee skill increase for random pawn
                if (rando < 17)
                {
                    // Healing Potion.
                    IntVec3 spawnCell7;
                    CellRect cellRectAroundHerdstone7 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone7, herdstone.Map, out spawnCell7);

                    Thing reward = ThingMaker.MakeThing(ThingDef.Named("RH_TET_Potion_Healing"));
                    reward.stackCount = 1;
                    GenSpawn.Spawn(reward, spawnCell7, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Khorne".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(reward.stackCount, reward.def.label));

                    success = true;
                }
                else if (rando < 31)
                {
                    // Beer.
                    IntVec3 spawnCell7;
                    CellRect cellRectAroundHerdstone7 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone7, herdstone.Map, out spawnCell7);

                    Thing beer = ThingMaker.MakeThing(ThingDefOf.Beer);
                    beer.stackCount = 16;
                    GenSpawn.Spawn(beer, spawnCell7, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Khorne".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(beer.stackCount, beer.def.label));

                    success = true;
                }
                else if (rando < 46)
                {
                    // Human meat.
                    IntVec3 spawnCell2;
                    CellRect cellRectAroundHerdstone2 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone2, herdstone.Map, out spawnCell2);

                    Thing humanMeat = ThingMaker.MakeThing(ThingDefOf.Meat_Human);
                    humanMeat.stackCount = 64;
                    GenSpawn.Spawn(humanMeat, spawnCell2, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Khorne".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(humanMeat.stackCount, humanMeat.def.label));
                    
                    success = true;
                }
                else if (rando < 61)
                {
                    // Artiact: Khorne Horn
                    IntVec3 spawnCell3;
                    CellRect cellRectAroundHerdstone3 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone3, herdstone.Map, out spawnCell3);

                    Thing khorneHorn = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_KhorneHorn);
                    GenSpawn.Spawn(khorneHorn, spawnCell3, herdstone.Map);

                    // Unique.
                    s.Append("\n\n" + "RH_TET_Beastmen_Khorne".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(khorneHorn.Label));
                    
                    success = true;
                }
                else if (rando < 76)
                {
                    // Coal - default to steel if coal isn't in the game.
                    // Over 75, so do two stacks.
                    IntVec3 spawnCell5;
                    CellRect cellRectAroundHerdstone5 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone5, herdstone.Map, out spawnCell5);

                    Thing coalOrSteel = null;

                    // Player may not be using the other Rimhammer mods that add coal.
                    try
                    {
                        coalOrSteel = ThingMaker.MakeThing(ThingDef.Named("RH_TET_Coal"));
                    }
                    catch
                    {
                        coalOrSteel = ThingMaker.MakeThing(ThingDefOf.Steel);
                    }

                    coalOrSteel.stackCount = 75;

                    GenSpawn.Spawn(coalOrSteel, spawnCell5, herdstone.Map);

                    IntVec3 spawnCell55;
                    CellRect cellRectAroundHerdstone55 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone55, herdstone.Map, out spawnCell55);

                    Thing coalOrSteel2 = null;

                    // Player may not be using the other Rimhammer mods that add coal.
                    try
                    {
                        coalOrSteel2 = ThingMaker.MakeThing(ThingDef.Named("RH_TET_Coal"));
                    }
                    catch
                    {
                        coalOrSteel2 = ThingMaker.MakeThing(ThingDefOf.Steel);
                    }

                    coalOrSteel2.stackCount = 53;

                    GenSpawn.Spawn(coalOrSteel2, spawnCell55, herdstone.Map);

                    // Unique
                    s.Append("\n\n" + "RH_TET_Beastmen_Khorne".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate((coalOrSteel.stackCount + coalOrSteel2.stackCount), coalOrSteel2.def.label));
                    
                    success = true;
                }
                else if (rando < 91)
                {
                    // Weapon: legendary great axe.
                    IntVec3 spawnCell6;
                    CellRect cellRectAroundHerdstone6 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone6, herdstone.Map, out spawnCell6);

                    Thing beastmenGreatAxe = ThingMaker.MakeThing(BeastmenDefOf.RH_TET_Beastmen_MeleeWeapon_GreatAxe, ThingDefOf.Steel);
                    beastmenGreatAxe.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                    GenSpawn.Spawn(beastmenGreatAxe, spawnCell6, herdstone.Map);

                    // Unique.
                    s.Append("\n\n" + "RH_TET_Beastmen_Khorne".Translate() + " " + "RH_TET_Beastmen_BoonQualityStuffItemTextSingle".Translate(QualityCategory.Legendary.GetLabel(), beastmenGreatAxe.Stuff.label, beastmenGreatAxe.Label));
                    
                    success = true;
                }
                else
                {
                    // Skill Increase
                    // Find random pawn that is capable of violence.
                    int experiencePoints = 8000;
                    IEnumerable<Pawn> colonists = herdstone.Map.mapPawns.FreeColonists;
                    Pawn p = null;
                    if (colonists != null && colonists.Count() > 0)
                    {
                        p = FindRandomCapablePawn(colonists, RH_TET_BeastmenMod.random.Next(0, colonists.Count()), WorkTags.Violent);
                        if (p != null)
                        {
                            p.skills.GetSkill(SkillDefOf.Melee).Learn(experiencePoints, true);
                        }
                        else
                        {
                            Log.Warning("Khorne attempted to grant a pawn additional melee skill, but there was no available pawn.");
                            success = false;
                            return;
                        }
                    }
                    
                    if (p != null)
                    { 
                        // Unique
                        s.Append("\n\n" + "RH_TET_Beastmen_Khorne".Translate() + " " + "RH_TET_Beastmen_GenericStatIncreaseText".Translate(p.Name, experiencePoints, SkillDefOf.Melee.label));
                        
                        success = true;
                    }
                }
            }

            if (success)
            {
                // Wrap up info letter, and send.
                s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextEnd".Translate());
                Find.LetterStack.ReceiveLetter(textLabel, s.ToString(), letterDef, new TargetInfo(herdstone.InteractionCell, herdstone.Map));
                
                // Make it dramatic!
                Find.CameraDriver.shaker.DoShake(2f);
                SoundInfo info = SoundInfo.InMap(new TargetInfo(herdstone.InteractionCell, herdstone.Map, false), MaintenanceType.None);
                SoundDefOf.Thunder_OnMap.PlayOneShot(info);
            }
        }

        internal static void ProcessOffering(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            // One sixth chance that there will bo no gift.
            DarkGodDef currentDef = darkGod.def as DarkGodDef;
            if (rewardThresholdCrossed(darkGod))
            {
                // One sixth chance of failure.
                //int randomMaybe = random.Next(0, 6); TODO:This made players mad, lol.
                if (currentDef.Equals(RH_TET_MagicDefOf.RH_TET_Slaanesh))
                    ProcessSlaaneshRewards(darkGod, herdstone);
                else if (currentDef.Equals(RH_TET_MagicDefOf.RH_TET_Tzeentch))
                    ProcessTzeentchRewards(darkGod, herdstone);
            }
        }

        internal static void ProcessSacrificeAnimal(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            DarkGodDef currentDef = darkGod.def as DarkGodDef;
            if (rewardThresholdCrossed(darkGod))
            {
                if (currentDef.Equals(RH_TET_MagicDefOf.RH_TET_Slaanesh))
                    ProcessSlaaneshRewards(darkGod, herdstone);
                else if (currentDef.Equals(RH_TET_MagicDefOf.RH_TET_Tzeentch))
                    ProcessTzeentchRewards(darkGod, herdstone);
                else if (currentDef.Equals(RH_TET_MagicDefOf.RH_TET_Khorne))
                    ProcessKhorneRewards(darkGod, herdstone);
                else if (currentDef.Equals(RH_TET_MagicDefOf.RH_TET_Nurgle))
                    ProcessNurgleRewards(darkGod, herdstone);
            }
        }

        internal static void ProcessSacrificeHuman(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            DarkGodDef currentDef = darkGod.def as DarkGodDef;
            darkGod.humanlikesSacrificedCount++;
            herdstone.newPawnEventBonus += 5;

            if ((darkGod.humanlikesSacrificedCount == 1) || (darkGod.humanlikesSacrificedCount % currentDef.SacredNumber == 0))
            { 
                if (currentDef.Equals(RH_TET_MagicDefOf.RH_TET_Slaanesh))
                    ProcessSlaaneshHumanSacrificeRewards(darkGod, herdstone);
                else if (currentDef.Equals(RH_TET_MagicDefOf.RH_TET_Tzeentch))
                    ProcessTzeentchHumanSacrificeRewards(darkGod, herdstone);
                else if (currentDef.Equals(RH_TET_MagicDefOf.RH_TET_Khorne))
                    ProcessKhorneHumanSacrificeRewards(darkGod, herdstone);
                else if (currentDef.Equals(RH_TET_MagicDefOf.RH_TET_Nurgle))
                    ProcessNurgleHumanSacrificeRewards(darkGod, herdstone);
            }
        }

        private static void ProcessTzeentchHumanSacrificeRewards(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            // Letter
            LetterDef letterDef = BeastmenDefOf.RH_TET_Beastmen_TzeentchMessage;
            string textLabel = "RH_TET_Beastmen_BoonFrom".Translate() + " " + "RH_TET_Beastmen_Tzeentch".Translate();
            StringBuilder s = new StringBuilder();

            s.Append("\n\n" + "RH_TET_Beastmen_SacrificedHumansCount".Translate("RH_TET_Beastmen_Tzeentch".Translate(), darkGod.humanlikesSacrificedCount));
            s.Append("\n\n" + "RH_TET_Beastmen_TzeentchBoonInfo".Translate());

            IntVec3 spawnCell;
            CellRect cellRectAroundHerdstone = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
            TryFindSpawnCellForItem(cellRectAroundHerdstone, herdstone.Map, out spawnCell);

            Thing apparel = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_Magic_Apparel_TzeentchRibbon);
            apparel.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            GenSpawn.Spawn(apparel, spawnCell, herdstone.Map);

            // Unique.
            StringBuilder sb = new StringBuilder("\n\n" + "RH_TET_Beastmen_HerdstoneRenown".Translate(herdstone.newPawnEventBonus + herdstone.sarsenBonus));
            sb.Append(" (" + "RH_TET_Beastmen_SarsenInfo".Translate() + ")");
            s.Append(sb.ToString());

            s.Append("\n\n" + "RH_TET_Beastmen_Tzeentch".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(apparel.Label));

            // Wrap up info letter, and send.
            s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextEnd".Translate());
            Find.LetterStack.ReceiveLetter(textLabel, s.ToString(), letterDef, new TargetInfo(herdstone.InteractionCell, herdstone.Map));

            // Make it dramatic!
            Find.CameraDriver.shaker.DoShake(2f);
            SoundInfo info = SoundInfo.InMap(new TargetInfo(herdstone.InteractionCell, herdstone.Map, false), MaintenanceType.None);
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
        }

        private static void ProcessSlaaneshHumanSacrificeRewards(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {            
            // Letter
            LetterDef letterDef = BeastmenDefOf.RH_TET_Beastmen_SlaaneshMessage;
            string textLabel = "RH_TET_Beastmen_BoonFrom".Translate() + " " + "RH_TET_Beastmen_Slaanesh".Translate();
            StringBuilder s = new StringBuilder();

            s.Append("RH_TET_Beastmen_SacrificedHumansCount".Translate("RH_TET_Beastmen_Slaanesh".Translate(), darkGod.humanlikesSacrificedCount));
            s.Append("\n\n" + "RH_TET_Beastmen_SlaaneshBoonInfo".Translate());
            
            IntVec3 spawnCell;
            CellRect cellRectAroundHerdstone = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
            TryFindSpawnCellForItem(cellRectAroundHerdstone, herdstone.Map, out spawnCell);

            Thing apparel = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_Magic_Apparel_SlaaneshSash);
            apparel.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            GenSpawn.Spawn(apparel, spawnCell, herdstone.Map);

            // Unique.
            s.Append("\n\n" + "RH_TET_Beastmen_HerdstoneRenown".Translate(herdstone.newPawnEventBonus));

            s.Append("\n\n" + "RH_TET_Beastmen_Slaanesh".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(apparel.Label));

            // Wrap up info letter, and send.
            s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextEnd".Translate());
            Find.LetterStack.ReceiveLetter(textLabel, s.ToString(), letterDef, new TargetInfo(herdstone.InteractionCell, herdstone.Map));

            // Make it dramatic!
            Find.CameraDriver.shaker.DoShake(2f);
            SoundInfo info = SoundInfo.InMap(new TargetInfo(herdstone.InteractionCell, herdstone.Map, false), MaintenanceType.None);
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
        }

        private static void ProcessKhorneHumanSacrificeRewards(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            // Letter
            LetterDef letterDef = BeastmenDefOf.RH_TET_Beastmen_KhorneMessage;
            string textLabel = "RH_TET_Beastmen_BoonFrom".Translate() + " " + "RH_TET_Beastmen_Khorne".Translate();
            StringBuilder s = new StringBuilder();

            s.Append("RH_TET_Beastmen_SacrificedHumansCount".Translate("RH_TET_Beastmen_Khorne".Translate(), darkGod.humanlikesSacrificedCount));
            s.Append("\n\n" + "RH_TET_Beastmen_KhorneBoonInfo".Translate());

            IntVec3 spawnCell;
            CellRect cellRectAroundHerdstone = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
            TryFindSpawnCellForItem(cellRectAroundHerdstone, herdstone.Map, out spawnCell);

            Thing apparel = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_Magic_Apparel_KhorneBelt);
            apparel.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            GenSpawn.Spawn(apparel, spawnCell, herdstone.Map);

            // Unique.
            s.Append("\n\n" + "RH_TET_Beastmen_HerdstoneRenown".Translate(herdstone.newPawnEventBonus));

            s.Append("\n\n" + "RH_TET_Beastmen_Khorne".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(apparel.Label));

            // Wrap up info letter, and send.
            s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextEnd".Translate());
            Find.LetterStack.ReceiveLetter(textLabel, s.ToString(), letterDef, new TargetInfo(herdstone.InteractionCell, herdstone.Map));

            // Make it dramatic!
            Find.CameraDriver.shaker.DoShake(2f);
            SoundInfo info = SoundInfo.InMap(new TargetInfo(herdstone.InteractionCell, herdstone.Map, false), MaintenanceType.None);
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
        }

        private static void ProcessNurgleHumanSacrificeRewards(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            // Letter
            LetterDef letterDef = BeastmenDefOf.RH_TET_Beastmen_NurgleMessage;
            string textLabel = "RH_TET_Beastmen_BoonFrom".Translate() + " " + "RH_TET_Beastmen_Nurgle".Translate();
            StringBuilder s = new StringBuilder();

            s.Append("RH_TET_Beastmen_SacrificedHumansCount".Translate("RH_TET_Beastmen_Nurgle".Translate(), darkGod.humanlikesSacrificedCount));
            s.Append("\n\n" + "RH_TET_Beastmen_NurgleBoonInfo".Translate());

            IntVec3 spawnCell;
            CellRect cellRectAroundHerdstone = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
            TryFindSpawnCellForItem(cellRectAroundHerdstone, herdstone.Map, out spawnCell);

            Thing apparel = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_Magic_Apparel_NurgleBand);
            apparel.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            GenSpawn.Spawn(apparel, spawnCell, herdstone.Map);

            // Unique.
            s.Append("\n\n" + "RH_TET_Beastmen_HerdstoneRenown".Translate(herdstone.newPawnEventBonus));

            s.Append("\n\n" + "RH_TET_Beastmen_Nurgle".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(apparel.Label));

            // Wrap up info letter, and send.
            s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextEnd".Translate());
            Find.LetterStack.ReceiveLetter(textLabel, s.ToString(), letterDef, new TargetInfo(herdstone.InteractionCell, herdstone.Map));

            // Make it dramatic!
            Find.CameraDriver.shaker.DoShake(2f);
            SoundInfo info = SoundInfo.InMap(new TargetInfo(herdstone.InteractionCell, herdstone.Map, false), MaintenanceType.None);
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
        }

        private static void ProcessTzeentchRewards(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            float lastFavor = darkGod.LastFavor;
            float favor = darkGod.PlayerFavor;
            bool success = false;

            // Letter
            LetterDef letterDef = BeastmenDefOf.RH_TET_Beastmen_TzeentchMessage;
            string textLabel = "RH_TET_Beastmen_FavorOf".Translate() + " " + "RH_TET_Beastmen_Tzeentch".Translate();
            StringBuilder s = new StringBuilder();

            s.Append("\n\n" + "RH_TET_Beastmen_HerdstoneFavorCount".Translate("RH_TET_Beastmen_Tzeentch".Translate(), favor));
            s.Append("\n\n" + "RH_TET_Beastmen_TzeentchInfo".Translate());

            // This one is for the first god weapon. It'll keep people motivated to keep making offerings.
            if (lastFavor < 9f && favor >= 9f)
            {
                // Weapons
                IntVec3 spawnCell;
                CellRect cellRectAroundHerdstone = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                TryFindSpawnCellForItem(cellRectAroundHerdstone, herdstone.Map, out spawnCell);

                Thing weapon = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_MeleeWeapon_TzeentchStaff);
                weapon.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                GenSpawn.Spawn(weapon, spawnCell, herdstone.Map);

                // Unique.
                s.Append("\n\n" + "RH_TET_Beastmen_Tzeentch".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(weapon.Label));

                success = true;
            }
            // This one is for the god gor pawn.
            else if (rewardThresholdSquaredCrossed(darkGod))
            {
                // Consider adding in a chance that this fails, for now, let it happen every time. If OP, add random chance on the times after the first (that one will always work).
                // Spawn new god gor.
                IntVec3 spawnCell4;
                CellRect cellRectAroundHerdstone4 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                TryFindSpawnCellForItem(cellRectAroundHerdstone4, herdstone.Map, out spawnCell4);
                PawnGenerationRequest request = new PawnGenerationRequest(BeastmenDefOf.RH_TET_Beastmen_Tzaangor, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1,
                    true, false,
                    false, true, true,
                    0.05f, true, true,
                    true,
                    false, false, false,
                    false, false, false,
                    0.0f, 0.0f, null, 0.0f,
                    null, null, null,
                    null, null, null,
                    null, null, null,
                    null, null, null);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                try
                {
                    pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
                }
                catch
                {
                    // Ignore if no ideos present.
                }
                pawn.skills.skills.Where(w => w.def.defName == "Intellectual").First().passion = Passion.Major;
                GenSpawn.Spawn(pawn, spawnCell4, herdstone.Map, WipeMode.Vanish);

                // Unique.
                s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextPawn".Translate("RH_TET_Beastmen_Tzeentch".Translate(), pawn.def.label));

                success = true;
            }
            else
            {
                int rando = RH_TET_BeastmenMod.random.Next(0, 100);

                if (rando < 17)
                {
                    // Healing Potion.
                    IntVec3 spawnCell7;
                    CellRect cellRectAroundHerdstone7 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone7, herdstone.Map, out spawnCell7);

                    Thing reward = ThingMaker.MakeThing(ThingDef.Named("RH_TET_Potion_Healing"));
                    reward.stackCount = 1;
                    GenSpawn.Spawn(reward, spawnCell7, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Tzeentch".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(reward.stackCount, reward.def.label));

                    success = true;
                }
                else if (rando < 31)
                {
                    // Drug.
                    IntVec3 spawnCell7;
                    CellRect cellRectAroundHerdstone7 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone7, herdstone.Map, out spawnCell7);

                    Thing drug = ThingMaker.MakeThing(ThingDefOf.SmokeleafJoint);
                    drug.stackCount = 27;
                    GenSpawn.Spawn(drug, spawnCell7, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Tzeentch".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(drug.stackCount, drug.def.label));

                    success = true;
                }
                else if (rando < 46)
                {
                    // Food
                    IntVec3 spawnCell2;
                    CellRect cellRectAroundHerdstone2 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone2, herdstone.Map, out spawnCell2);

                    // Default stack size is 10.
                    Thing food = ThingMaker.MakeThing(ThingDefOf.MealFine);
                    food.stackCount = 10;
                    GenSpawn.Spawn(food, spawnCell2, herdstone.Map);

                    IntVec3 spawnCell22;
                    CellRect cellRectAroundHerdstone22 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone22, herdstone.Map, out spawnCell22);

                    Thing food2 = null;
                    food2 = ThingMaker.MakeThing(ThingDefOf.MealFine);
                    food2.stackCount = 8;

                    GenSpawn.Spawn(food2, spawnCell22, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Tzeentch".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(food.stackCount + food2.stackCount, food.def.label));

                    success = true;
                }
                else if (rando < 61)
                {
                    // Artiact
                    IntVec3 spawnCell3;
                    CellRect cellRectAroundHerdstone3 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone3, herdstone.Map, out spawnCell3);

                    Thing artifact = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_TzeentchCoin);
                    GenSpawn.Spawn(artifact, spawnCell3, herdstone.Map);

                    // Unique.
                    s.Append("\n\n" + "RH_TET_Beastmen_Tzeentch".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(artifact.Label));

                    success = true;
                }
                else if (rando < 76)
                {
                    // Resource
                    // Over 75, so do two stacks. Three actually
                    IntVec3 spawnCell5;
                    CellRect cellRectAroundHerdstone5 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone5, herdstone.Map, out spawnCell5);

                    Thing resource = null;

                    resource = ThingMaker.MakeThing(ThingDefOf.WoodLog);
                    resource.stackCount = 75;

                    GenSpawn.Spawn(resource, spawnCell5, herdstone.Map);

                    IntVec3 spawnCell55;
                    CellRect cellRectAroundHerdstone55 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone55, herdstone.Map, out spawnCell55);

                    Thing resource2 = null;
                    resource2 = ThingMaker.MakeThing(ThingDefOf.WoodLog);
                    resource2.stackCount = 75;

                    GenSpawn.Spawn(resource2, spawnCell55, herdstone.Map);

                    IntVec3 spawnCell555;
                    CellRect cellRectAroundHerdstone555 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone555, herdstone.Map, out spawnCell555);

                    Thing resource3 = null;
                    resource3 = ThingMaker.MakeThing(ThingDefOf.WoodLog);
                    resource3.stackCount = 12;

                    GenSpawn.Spawn(resource3, spawnCell555, herdstone.Map);

                    // Unique
                    s.Append("\n\n" + "RH_TET_Beastmen_Tzeentch".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate((resource.stackCount + resource2.stackCount + resource3.stackCount), resource2.def.label));

                    success = true;
                }
                else if (rando < 91)
                {
                    // Weapon: legendary 
                    IntVec3 spawnCell6;
                    CellRect cellRectAroundHerdstone6 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone6, herdstone.Map, out spawnCell6);

                    Thing weapon = ThingMaker.MakeThing(BeastmenDefOf.RH_TET_Beastmen_RangedWeapon_UngorBow);
                    weapon.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                    GenSpawn.Spawn(weapon, spawnCell6, herdstone.Map);

                    // Unique.
                    s.Append("\n\n" + "RH_TET_Beastmen_Tzeentch".Translate() + " " + "RH_TET_Beastmen_BoonQualityStuffItemTextSingle".Translate(QualityCategory.Legendary.GetLabel(), "wooden", weapon.Label));

                    success = true;
                }
                else
                {
                    // Skill Increase/Magic Level Increase

                    // Find random pawn that is capable of X.
                    int experiencePoints = 18000;
                    int magicLevelIncrease = 3;
                    bool wizardFound = false;
                    Pawn p = null;

                    IEnumerable<Pawn> colonists = herdstone.Map.mapPawns.FreeColonists;

                    if (colonists != null && colonists.Count() > 0)
                    {
                        // Try to give wizard pawn new magic levels.
                        p = FindRandomMagicUserPawn(colonists, RH_TET_BeastmenMod.random.Next(0, colonists.Count()));
                        p = null;
                        if (p != null)
                        {
                            CompMagicUser compMagicUser = ((p != null) ? p.TryGetComp<CompMagicUser>() : null);
                            compMagicUser.MagicData.AbilityPoints += 3;
                            wizardFound = true;
                        }
                        else
                        { 
                            // If no magic users, then give a skill increase intellectual.
                            p = FindRandomCapablePawn(colonists, RH_TET_BeastmenMod.random.Next(0, colonists.Count()), WorkTags.Intellectual);
                            if (p != null)
                            {
                                p.skills.GetSkill(SkillDefOf.Intellectual).Learn(experiencePoints, true);
                            }
                            else
                            {
                                Log.Warning("Tzeentch attempted to grant a pawn additional magic levels or intelligence, but there was no available pawn.");
                                success = false;
                                return;
                            }
                        }
                    }

                    if (p != null)
                    {
                        // Unique
                        if (!wizardFound)
                            s.Append("\n\n" + "RH_TET_Beastmen_Tzeentch".Translate() + " " + "RH_TET_Beastmen_GenericStatIncreaseText".Translate(p.Name, experiencePoints, SkillDefOf.Intellectual.label));
                        else
                            s.Append("\n\n" + "RH_TET_Beastmen_Tzeentch".Translate() + " " + "RH_TET_Beastmen_MagicSkillIncreaseText".Translate(p.Name, 3));

                        success = true;
                    }
                }
            }

            if (success)
            {
                // Wrap up info letter, and send.
                s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextEnd".Translate());
                Find.LetterStack.ReceiveLetter(textLabel, s.ToString(), letterDef, new TargetInfo(herdstone.InteractionCell, herdstone.Map));

                // Make it dramatic!
                Find.CameraDriver.shaker.DoShake(2f);
                SoundInfo info = SoundInfo.InMap(new TargetInfo(herdstone.InteractionCell, herdstone.Map, false), MaintenanceType.None);
                SoundDefOf.Thunder_OnMap.PlayOneShot(info);
            }
        }

        private static void ProcessSlaaneshRewards(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            float lastFavor = darkGod.LastFavor;
            float favor = darkGod.PlayerFavor;
            bool success = false;

            // Letter
            LetterDef letterDef = BeastmenDefOf.RH_TET_Beastmen_SlaaneshMessage;
            string textLabel = "RH_TET_Beastmen_FavorOf".Translate() + " " + "RH_TET_Beastmen_Slaanesh".Translate();
            StringBuilder s = new StringBuilder();

            s.Append("\n\n" + "RH_TET_Beastmen_HerdstoneFavorCount".Translate("RH_TET_Beastmen_Slaanesh".Translate(), favor));
            s.Append("\n\n" + "RH_TET_Beastmen_SlaaneshInfo".Translate());

            // This one is for the first god weapon. It'll keep people motivated to keep making offerings.
            if (lastFavor < 6f && favor >= 6f)
            {
                // Weapons
                IntVec3 spawnCell;
                CellRect cellRectAroundHerdstone = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                TryFindSpawnCellForItem(cellRectAroundHerdstone, herdstone.Map, out spawnCell);

                Thing weapon = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_MeleeWeapon_SlaaneshWhip);
                weapon.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                GenSpawn.Spawn(weapon, spawnCell, herdstone.Map);

                // Unique.
                s.Append("\n\n" + "RH_TET_Beastmen_Slaanesh".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(weapon.Label));

                success = true;
            }
            // This one is for the god gor pawn.
            else if (rewardThresholdSquaredCrossed(darkGod))
            {
                // Consider adding in a chance that this fails, for now, let it happen every time. If OP, add random chance on the times after the first (that one will always work).
                // Spawn new god gor.
                IntVec3 spawnCell4;
                CellRect cellRectAroundHerdstone4 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                TryFindSpawnCellForItem(cellRectAroundHerdstone4, herdstone.Map, out spawnCell4);

                PawnGenerationRequest request = new PawnGenerationRequest(BeastmenDefOf.RH_TET_Beastmen_Slaangor, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1,
                    true,  false,
                    false, true, true,
                    0.05f, true, true,
                    true,
                    false, false, false,
                    false, false, false,
                    0.0f, 0.0f, null, 0.0f,
                    null, null, null,
                    null, null, null,
                    null, null, null,
                    null, null, null);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                try
                {
                    pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
                }
                catch
                {
                    // Ignore if no ideos present.
                }
                pawn.skills.skills.Where(w => w.def.defName == "Social").First().passion = Passion.Major;
                GenSpawn.Spawn(pawn, spawnCell4, herdstone.Map, WipeMode.Vanish);

                // Unique.
                s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextPawn".Translate("RH_TET_Beastmen_Slaanesh".Translate(), pawn.def.label));

                success = true;
            }
            else
            {
                int rando = RH_TET_BeastmenMod.random.Next(0, 100);

                if (rando < 17)
                {
                    // Healing Potion.
                    IntVec3 spawnCell7;
                    CellRect cellRectAroundHerdstone7 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone7, herdstone.Map, out spawnCell7);

                    Thing reward = ThingMaker.MakeThing(ThingDef.Named("RH_TET_Potion_Healing"));
                    reward.stackCount = 1;
                    GenSpawn.Spawn(reward, spawnCell7, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Slaanesh".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(reward.stackCount, reward.def.label));

                    success = true;
                }
                else if (rando < 31)
                {
                    // Drug.
                    IntVec3 spawnCell7;
                    CellRect cellRectAroundHerdstone7 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone7, herdstone.Map, out spawnCell7);

                    Thing drug = ThingMaker.MakeThing(ThingDef.Named("PsychiteTea"));
                    drug.stackCount = 24;
                    GenSpawn.Spawn(drug, spawnCell7, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Slaanesh".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(drug.stackCount, drug.def.label));

                    success = true;
                }
                else if (rando < 46)
                {
                    // Food
                    IntVec3 spawnCell2;
                    CellRect cellRectAroundHerdstone2 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone2, herdstone.Map, out spawnCell2);

                    Thing food = ThingMaker.MakeThing(ThingDef.Named("MealLavish"));
                    food.stackCount = 6;
                    GenSpawn.Spawn(food, spawnCell2, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Slaanesh".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(food.stackCount, food.def.label));

                    success = true;
                }
                else if (rando < 61)
                {
                    // Artiact
                    IntVec3 spawnCell3;
                    CellRect cellRectAroundHerdstone3 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone3, herdstone.Map, out spawnCell3);

                    Thing artifact = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_SlaaneshChalice);
                    GenSpawn.Spawn(artifact, spawnCell3, herdstone.Map);

                    // Unique.
                    s.Append("\n\n" + "RH_TET_Beastmen_Slaanesh".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(artifact.Label));

                    success = true;
                }
                else if (rando < 76)
                {
                    // Resource
                    // Over 75, so do two stacks. Three actually
                    IntVec3 spawnCell5;
                    CellRect cellRectAroundHerdstone5 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone5, herdstone.Map, out spawnCell5);

                    Thing resource = null;

                    resource = ThingMaker.MakeThing(ThingDef.Named("BlocksMarble"));
                    resource.stackCount = 75;

                    GenSpawn.Spawn(resource, spawnCell5, herdstone.Map);

                    IntVec3 spawnCell55;
                    CellRect cellRectAroundHerdstone55 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone55, herdstone.Map, out spawnCell55);

                    Thing resource2 = null;
                    resource2 = ThingMaker.MakeThing(ThingDef.Named("BlocksMarble"));
                    resource2.stackCount = 33;

                    GenSpawn.Spawn(resource2, spawnCell55, herdstone.Map);

                    // Unique
                    s.Append("\n\n" + "RH_TET_Beastmen_Slaanesh".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate((resource.stackCount + resource2.stackCount), resource2.def.label));

                    success = true;
                }
                else if (rando < 91)
                {
                    // Weapon: legendary 
                    IntVec3 spawnCell6;
                    CellRect cellRectAroundHerdstone6 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone6, herdstone.Map, out spawnCell6);

                    Thing weapon = ThingMaker.MakeThing(BeastmenDefOf.RH_TET_Beastmen_MeleeWeapon_Sword, ThingDefOf.Steel);
                    weapon.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                    GenSpawn.Spawn(weapon, spawnCell6, herdstone.Map);

                    // Unique.
                    s.Append("\n\n" + "RH_TET_Beastmen_Slaanesh".Translate() + " " + "RH_TET_Beastmen_BoonQualityStuffItemTextSingle".Translate(QualityCategory.Legendary.GetLabel(), weapon.Stuff.label, weapon.Label));

                    success = true;
                }
                else
                {
                    // Skill Increase
                    // Find random pawn that is capable of art.
                    int experiencePoints = 12000;
                    IEnumerable<Pawn> colonists = herdstone.Map.mapPawns.FreeColonists;
                    Pawn p = null;
                    if (colonists != null && colonists.Count() > 0)
                    {
                        p = FindRandomCapablePawn(colonists, RH_TET_BeastmenMod.random.Next(0, colonists.Count()), WorkTags.Artistic);
                        if (p != null)
                        {
                            p.skills.GetSkill(SkillDefOf.Artistic).Learn(experiencePoints, true);
                        }
                        else
                        {
                            Log.Warning("Slaanesh attempted to grant a pawn additional art skill, but there was no available pawn.");
                            success = false;
                            return;
                        }
                    }

                    if (p != null)
                    {
                        // Unique
                        s.Append("\n\n" + "RH_TET_Beastmen_Slaanesh".Translate() + " " + "RH_TET_Beastmen_GenericStatIncreaseText".Translate(p.Name, experiencePoints, SkillDefOf.Artistic.label));

                        success = true;
                    }
                }
            }

            if (success)
            {
                // Wrap up info letter, and send.
                s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextEnd".Translate());
                Find.LetterStack.ReceiveLetter(textLabel, s.ToString(), letterDef, new TargetInfo(herdstone.InteractionCell, herdstone.Map));

                // Make it dramatic!
                Find.CameraDriver.shaker.DoShake(2f);
                SoundInfo info = SoundInfo.InMap(new TargetInfo(herdstone.InteractionCell, herdstone.Map, false), MaintenanceType.None);
                SoundDefOf.Thunder_OnMap.PlayOneShot(info);
            }
        }

        private static bool rewardThresholdSquaredCrossed(DarkGod darkGod)
        {
            DarkGodDef currentDef = darkGod.def as DarkGodDef;
            
            if (darkGod.LastFavor == 0 && darkGod.PlayerFavor >= (currentDef.SacredNumber * currentDef.SacredNumber))
                return true;
            else if((darkGod.PlayerFavor - darkGod.LastFavor) >= (currentDef.SacredNumber * currentDef.SacredNumber))
                return true;
            else if (darkGod.LastFavor % (currentDef.SacredNumber * currentDef.SacredNumber) 
                    >= darkGod.PlayerFavor % (currentDef.SacredNumber * currentDef.SacredNumber))
                return true;
            else return false;
        }

        private static bool rewardThresholdCrossed(DarkGod darkGod)
        {
            DarkGodDef currentDef = darkGod.def as DarkGodDef;
            
            if (darkGod.LastFavor == 0 && darkGod.PlayerFavor >= currentDef.SacredNumber)
                return true;
            else if ((darkGod.PlayerFavor - darkGod.LastFavor) >= currentDef.SacredNumber)
                return true;
            else if (darkGod.LastFavor % currentDef.SacredNumber >= darkGod.PlayerFavor % currentDef.SacredNumber)
                return true;
            else return false;
        }

        private static Pawn FindRandomMagicUserPawn(IEnumerable<Pawn> colonists, int randoPawn)
        {
            Pawn returnPawn = null;

            int i = 0;
            foreach (Pawn p in colonists)
            {
                if (i >= randoPawn)
                {
                    CompMagicUser compMagicUser = ((p != null) ? p.TryGetComp<CompMagicUser>() : null);
                    if (compMagicUser != null && compMagicUser.IsMagicUser)
                    {
                        returnPawn = p;
                        break;
                    }
                }

                i++;
            }

            if (returnPawn == null && randoPawn == 0)
            {
                return returnPawn;
            }
            else if (returnPawn == null)
            {
                returnPawn = FindRandomMagicUserPawn(colonists, 0);
            }

            return returnPawn;
        }

        private static Pawn FindRandomCapablePawn(IEnumerable<Pawn> colonists, int randoPawn, WorkTags capableOf)
        {
            Pawn returnPawn = null;
            
            int i = 0;
            foreach (Pawn p in colonists)
            {
                if (i >= randoPawn)
                { 
                    if (!((p.CombinedDisabledWorkTags & capableOf) == capableOf))
                    {
                        returnPawn = p;
                        break;
                    }
                }

                i++;
            }

            if (returnPawn == null && randoPawn == 0)
            {
                return returnPawn;
            }
            else if(returnPawn == null)
            {
                returnPawn = FindRandomCapablePawn(colonists, 0, capableOf);
            }

            return returnPawn;
        }

        private static void ProcessNurgleRewards(DarkGod darkGod, Building_MassiveHerdstone herdstone)
        {
            float corpseCount = darkGod.PlayerFavor;
            bool success = false;

            // Letter
            LetterDef letterDef = BeastmenDefOf.RH_TET_Beastmen_NurgleMessage;
            string textLabel = "RH_TET_Beastmen_FavorOf".Translate() + " " + "RH_TET_Beastmen_Nurgle".Translate();
            StringBuilder s = new StringBuilder();

            s.Append("\n\n" + "RH_TET_Beastmen_HerdstoneDessicatedMessage".Translate(corpseCount));
            s.Append("\n\n" + "RH_TET_Beastmen_NurgleInfo".Translate());

            // This one is for the first nurgle weapon. It'll keep people motivated to keep burning corpses.
            if (corpseCount == 7f)
            {
                // Weapon: morning star of nurgle.
                IntVec3 spawnCell;
                CellRect cellRectAroundHerdstone = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                TryFindSpawnCellForItem(cellRectAroundHerdstone, herdstone.Map, out spawnCell);

                Thing godlyWeapon = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_MeleeWeapon_NurgleStar);
                godlyWeapon.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                GenSpawn.Spawn(godlyWeapon, spawnCell, herdstone.Map);

                // Unique.
                s.Append("\n\n" + "RH_TET_Beastmen_Nurgle".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(godlyWeapon.Label));

                success = true;
            }
            // This one is for the new god gor pawn.
            else if (rewardThresholdSquaredCrossed(darkGod))
            {
                // Consider adding in a chance that this fails, for now, let it happen every time. If OP, add random chance on the times after the first (that one will always work).
                // Spawn god gor.
                IntVec3 spawnCell4;
                CellRect cellRectAroundHerdstone4 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                TryFindSpawnCellForItem(cellRectAroundHerdstone4, herdstone.Map, out spawnCell4);
                PawnGenerationRequest request = new PawnGenerationRequest(BeastmenDefOf.RH_TET_Beastmen_Pestigor, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1,
                    true, false,
                    false, true, true,
                    0.05f, true, true,
                    true,
                    false, false, false,
                    false, false, false,
                    0.0f, 0.0f, null, 0.0f,
                    null, null, null,
                    null, null, null,
                    null, null, null,
                    null, null, null);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                try
                {
                    pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
                }
                catch
                {
                    // Ignore if no ideos present.
                }
                GenSpawn.Spawn(pawn, spawnCell4, herdstone.Map, WipeMode.Vanish);

                // Unique.
                s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextPawn".Translate("RH_TET_Beastmen_Nurgle".Translate(), pawn.def.label));

                success = true;
            }
            else
            {
                int rando = RH_TET_BeastmenMod.random.Next(0, 100);

                // Remaining gifts from nurgle: (even odds on each)
                //      rice x 49, Charm of Nurgle, Wood x 128, Gor Armor Legendary Steel, Ambrosia x 21, medical skill increase for random pawn
                if (rando < 17)
                {
                    // Healing Potion.
                    IntVec3 spawnCell7;
                    CellRect cellRectAroundHerdstone7 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone7, herdstone.Map, out spawnCell7);

                    Thing reward = ThingMaker.MakeThing(ThingDef.Named("RH_TET_Potion_Healing"));
                    reward.stackCount = 1;
                    GenSpawn.Spawn(reward, spawnCell7, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Nurgle".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(reward.stackCount, reward.def.label));

                    success = true;
                }
                else if (rando < 31)
                {
                    // Drug.
                    IntVec3 spawnCell7;
                    CellRect cellRectAroundHerdstone7 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone7, herdstone.Map, out spawnCell7);

                    Thing drug = ThingMaker.MakeThing(ThingDef.Named("Ambrosia"));
                    drug.stackCount = 21;
                    GenSpawn.Spawn(drug, spawnCell7, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Nurgle".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(drug.stackCount, drug.def.label));

                    success = true;
                }
                else if (rando < 45)
                {
                    // Food.
                    IntVec3 spawnCell2;
                    CellRect cellRectAroundHerdstone2 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone2, herdstone.Map, out spawnCell2);

                    Thing food = ThingMaker.MakeThing(ThingDefOf.RawPotatoes);
                    food.stackCount = 49;
                    GenSpawn.Spawn(food, spawnCell2, herdstone.Map);

                    // Unqiue.
                    s.Append("\n\n" + "RH_TET_Beastmen_Nurgle".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate(food.stackCount, food.def.label));

                    success = true;
                }
                else if (rando < 61)
                {
                    // Artiact
                    IntVec3 spawnCell3;
                    CellRect cellRectAroundHerdstone3 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone3, herdstone.Map, out spawnCell3);

                    Thing artifact = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_NurgleCharm);
                    GenSpawn.Spawn(artifact, spawnCell3, herdstone.Map);

                    // Unique.
                    s.Append("\n\n" + "RH_TET_Beastmen_Nurgle".Translate() + " " + "RH_TET_Beastmen_GenericBoonTextSingle".Translate(artifact.Label));

                    success = true;
                }
                else if (rando < 76)
                {
                    // Wood
                    IntVec3 spawnCell5;
                    CellRect cellRectAroundHerdstone5 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone5, herdstone.Map, out spawnCell5);

                    Thing thingGift = ThingMaker.MakeThing(ThingDefOf.WoodLog);

                    thingGift.stackCount = 75;

                    GenSpawn.Spawn(thingGift, spawnCell5, herdstone.Map);

                    IntVec3 spawnCell55;
                    CellRect cellRectAroundHerdstone55 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone55, herdstone.Map, out spawnCell55);

                    Thing thingGift2 = ThingMaker.MakeThing(ThingDefOf.WoodLog);

                    thingGift2.stackCount = 72;

                    GenSpawn.Spawn(thingGift2, spawnCell55, herdstone.Map);

                    // Unique
                    s.Append("\n\n" + "RH_TET_Beastmen_Nurgle".Translate() + " " + "RH_TET_Beastmen_GenericBoonText".Translate((thingGift.stackCount + thingGift2.stackCount), thingGift2.def.label));

                    success = true;
                }
                else if (rando < 91)
                {
                    // Weapon/Armor.
                    IntVec3 spawnCell6;
                    CellRect cellRectAroundHerdstone6 = new CellRect(herdstone.InteractionCell.x - 4, herdstone.InteractionCell.z - 4, 10, 12);
                    TryFindSpawnCellForItem(cellRectAroundHerdstone6, herdstone.Map, out spawnCell6);

                    Thing apparelToMake = ThingMaker.MakeThing(BeastmenDefOf.RH_TET_Beastmen_Apparel_GorArmor, ThingDefOf.Steel);
                    apparelToMake.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                    GenSpawn.Spawn(apparelToMake, spawnCell6, herdstone.Map);

                    // Unique.
                    s.Append("\n\n" + "RH_TET_Beastmen_Nurgle".Translate() + " " + "RH_TET_Beastmen_BoonQualityStuffItemTextSingle".Translate(QualityCategory.Legendary.GetLabel(), apparelToMake.Stuff.label, apparelToMake.Label));

                    success = true;
                }
                else
                {
                    // Skill Increase
                    // Find random pawn that is capable of necessary skill.
                    int experiencePoints = 14000;
                    IEnumerable<Pawn> colonists = herdstone.Map.mapPawns.FreeColonists;
                    Pawn p = null;
                    if (colonists != null && colonists.Count() > 0)
                    {
                        p = FindRandomCapablePawn(colonists, RH_TET_BeastmenMod.random.Next(0, colonists.Count()), WorkTags.Caring);
                        if (p != null)
                        {
                            p.skills.GetSkill(SkillDefOf.Medicine).Learn(experiencePoints, true);
                        }
                        else
                        {
                            Log.Warning("Nurgle attempted to grant a pawn additional medical skill, but there was no available pawn.");
                            success = false;
                            return;
                        }
                    }

                    if (p != null)
                    {
                        // Unique
                        s.Append("\n\n" + "RH_TET_Beastmen_Nurgle".Translate() + " " + "RH_TET_Beastmen_GenericStatIncreaseText".Translate(p.Name, experiencePoints, SkillDefOf.Medicine.label));
                        
                        success = true;
                    }
                }
            }

            if (success)
            {
                // Wrap up info letter, and send.
                s.Append("\n\n" + "RH_TET_Beastmen_GenericBoonTextEnd".Translate());
                Find.LetterStack.ReceiveLetter(textLabel, s.ToString(), letterDef, new TargetInfo(herdstone.InteractionCell, herdstone.Map));

                // Make it dramatic!
                Find.CameraDriver.shaker.DoShake(2f);
                SoundInfo info = SoundInfo.InMap(new TargetInfo(herdstone.InteractionCell, herdstone.Map, false), MaintenanceType.None);
                SoundDefOf.Thunder_OnMap.PlayOneShot(info);
            }
        }

        public static void SpawnGift(Building building, ThingDef thingToSpawnDef)
        {
            Thing thingToSpawn = ThingMaker.MakeThing(thingToSpawnDef);
            GenSpawn.Spawn(thingToSpawn, building.Position, building.Map);

            Messages.Message(thingToSpawn.Label + " " + "RH_TET_Beastmen_RewardSpawned".Translate(), thingToSpawn, MessageTypeDefOf.PositiveEvent, false);
        }
    }
}