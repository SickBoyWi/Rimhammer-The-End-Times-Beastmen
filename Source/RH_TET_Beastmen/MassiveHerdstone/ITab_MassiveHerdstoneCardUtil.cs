using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Verse;
using TheEndTimes_Magic;

namespace TheEndTimes_Beastmen
{
    class ITab_MassiveHerdstoneCardUtil
    {
        public static void DrawGod(DarkGod god, Rect rect3, float offset = 0f)
        {
            string godLabel = "";
            string godDescrip = "";
            if (god != null)
            {
                godLabel = god.LabelCap;
                godDescrip = god.def.description;
            }

            Rect secondBox = rect3;
            secondBox.x += rect3.x + 10f + 30f + offset;
            secondBox.xMax += 125f;
            secondBox.height = ITab_HerdstoneSacrificeCardUtil.ButtonSize;
            Text.Font = GameFont.Medium;
            Widgets.Label(secondBox, godLabel);
            Text.Font = GameFont.Small;
            Rect secondBoxUnder = secondBox;
            secondBoxUnder.y += ITab_HerdstoneSacrificeCardUtil.ButtonSize;
            secondBoxUnder.width -= 15f;
            secondBoxUnder.height = ITab_HerdstoneSacrificeCardUtil.ButtonSize * 3;
            Widgets.Label(secondBoxUnder, godDescrip);
            ITab_MassiveHerdstoneCardUtil.DrawGodFavor(god, new Vector2(secondBoxUnder.x, secondBoxUnder.y + 120f));
            Rect secondBoxUnder2 = secondBoxUnder;
            secondBoxUnder2.y += ITab_HerdstoneSacrificeCardUtil.ButtonSize * 2 + (ITab_HerdstoneSacrificeCardUtil.SpacingOffset * 2);
            secondBoxUnder2.height = 250f;
        }
        
        public static void DrawGodsFavors(List<DarkGod> gods, Rect rect)
        {
            if (gods != null)
            {
                float yIncrement = 0;
                foreach (DarkGod g in gods)
                {
                    DrawGodFavor(g, new Vector2(rect.x, rect.y + yIncrement), true);
                    yIncrement += ITab_HerdstoneSacrificeCardUtil.ButtonSize;
                }
            }
        }
        
        public static void DrawGodFavor(DarkGod god, Vector2 topLeft, bool useGodLabel = false)
        {
            if (god == null) return;
            string standingLabel = "RH_TET_Beastmen_Favor".Translate() + ":";
            string favorText = "" + god.PlayerFavor;

            if (useGodLabel)
            {
                standingLabel = god.LabelCap + " " + standingLabel;
            }
            
            string standingLabel2 = "RH_TET_Beastmen_HumanlikeSacrifices".Translate();
            string sacrificesText = "" + god.humanlikesSacrificedCount;

            if (useGodLabel)
            {
                standingLabel2 = god.LabelCap + " " + standingLabel2;
            }

            float tierLabelWidth = Text.CalcSize(standingLabel).x;
            float tierLabelWidth2 = Text.CalcSize(standingLabel2).x;
            Rect rect = new Rect(topLeft.x, topLeft.y, 225f, 48f);
            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }

            GUI.BeginGroup(rect);
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect rect2 = new Rect(0f, 0f, tierLabelWidth + 5f, rect.height);
            Widgets.Label(rect2, standingLabel);
            Rect position = new Rect(rect2.xMax, 0f, 10f, 24f);
            Rect rect3 = new Rect(position.xMax, 0f, rect.width - position.xMax + 50f, rect.height);
            
            Widgets.Label(rect3, favorText);

            if (god.humanlikesSacrificedCount > 0)
            { 
                Rect rect22 = new Rect(0f, ITab_HerdstoneSacrificeCardUtil.SpacingOffset + 3f, tierLabelWidth2 + 5f, rect.height);
                Widgets.Label(rect22, standingLabel2);
                Rect position2 = new Rect(rect22.xMax, 0f, 10f, 24f);
                Rect rect33 = new Rect(position2.xMax, ITab_HerdstoneSacrificeCardUtil.SpacingOffset + 3f, rect.width - position2.xMax + 50f, rect.height);

                Widgets.Label(rect33, sacrificesText);
            }

            Rect rect4 = new Rect(position.xMax + 4f, 0f, 999f, rect.height);
            rect4.yMax += 18f;
            GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
            GenUI.ResetLabelAlign();
            GUI.color = Color.white;
            GUI.EndGroup();
        }

        ////////General Stuff

        public static string SacrificeLabel(Building_MassiveHerdstone herdstone)
        {
            if (herdstone.tempSacrifice == null) return "None";
            return herdstone.tempSacrifice.Name.ToStringShort;
        }

        public static string ExecutionerLabel(Building_MassiveHerdstone herdstone)
        {
            if (herdstone.tempExecutioner == null) return "None";
            return herdstone.tempExecutioner.Name.ToStringShort;
        }

        public static string GodLabel(Building_MassiveHerdstone herdstone, GodType godType)
        {
            switch (godType)
            {
                case GodType.OfferingGod:
                    if (herdstone.tempCurrentOfferingGod == null) return "None";
                    return herdstone.tempCurrentOfferingGod.LabelCap;
                case GodType.SacrificeGod:
                    if (herdstone.tempCurrentSacrificeGod == null) return "None";
                    return herdstone.tempCurrentSacrificeGod.LabelCap;
            }
            return "None";
        }

        public static string GodDescription(Building_MassiveHerdstone herdstone)
        {
            if (herdstone.tempCurrentSacrificeGod == null) return "None";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.Append(herdstone.tempCurrentSacrificeGod.def.description);
            return stringBuilder.ToString();
        }

        public static void OpenSacrificeSelectMenu(Building_MassiveHerdstone herdstone)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                herdstone.tempSacrifice = null;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            foreach (Pawn current in herdstone.Map.mapPawns.AllPawnsSpawned)
            {
                if (current.RaceProps.Animal && current.Faction == Faction.OfPlayer)
                {
                    Action action;
                    Pawn localCol = current;
                    action = delegate
                    {
                        herdstone.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedHerdstone = herdstone;
                        herdstone.Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType = HerdUtility.SacrificeType.animal;
                        herdstone.tempSacrifice = localCol;
                    };
                    list.Add(new FloatMenuOption(localCol.LabelShort, action, MenuOptionPriority.Default, null, null, 0f, null));
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        /// <summary>
        /// This will make things easier for processing multiple possibile "actors" at the altar.
        /// </summary>
        public enum ActorType
        {
            executioner = 0,
            shaman = 1,
            offerer = 2,
            attendee = 3,
            prisoner = 4,
            animalSacrifice = 5
        }

        public static void OpenActorSelectMenu(Building_MassiveHerdstone herdstone, ActorType actorType, bool allowBeastmen = true)
        {
            /// Error Handling
            if (herdstone == null)
            {
                TheEndTimes_Beastmen.Utility.ErrorReport("Herdstone Null Exception");
                return;
            }
            if (herdstone.Map == null)
            {
                TheEndTimes_Beastmen.Utility.ErrorReport("Map Null Exception");
            }
            if (herdstone.Map.mapPawns == null)
            {
                TheEndTimes_Beastmen.Utility.ErrorReport("mapPawns Null Exception");
                return;
            }
            if (herdstone.Map.mapPawns.FreeColonistsSpawned == null)
            {
                TheEndTimes_Beastmen.Utility.ErrorReport("FreeColonistsSpawned Null Exception");
                return;
            }
            
            /// Get Actor List
            List<Pawn> actorList = new List<Pawn>();
            StringBuilder s = new StringBuilder();
            switch (actorType)
            {
                case ActorType.executioner:
                case ActorType.offerer:
                    // Cycle through candidates
                    foreach (Pawn candidate in herdstone.Map.mapPawns.FreeColonistsSpawned)
                    {
                        if (!HerdUtility.IsBrayShamanAvailable(candidate)) continue;
                        
                        // Executioners must be able to use tool and move.
                        if (candidate.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
                            candidate.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                        {
                            // Add the actors.
                            actorList.Add(candidate);
                            TheEndTimes_Beastmen.Utility.DebugReport("Actor List :: Added " + candidate.Name);
                        }
                    }
                    TheEndTimes_Beastmen.Utility.DebugReport(s.ToString());
                    break;
                case ActorType.shaman:
                    // Cycle through candidates
                    foreach (Pawn candidate in herdstone.Map.mapPawns.FreeColonistsSpawned)
                    {
                        if (!HerdUtility.IsBrayShamanAvailable(candidate)) continue;

                        // Preachers must be able to move and talk.
                        if (candidate.health.capacities.CapableOf(PawnCapacityDefOf.Moving) &&
                            candidate.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
                        {
                            // Add the actors.
                            actorList.Add(candidate);
                            TheEndTimes_Beastmen.Utility.DebugReport("Actor List :: Added " + candidate.Name);
                        }
                    }
                    TheEndTimes_Beastmen.Utility.DebugReport(s.ToString());
                    break;
                case ActorType.prisoner:
                    ///
                    /// ERROR HANDLING
                    /// 

                    if (herdstone.Map.mapPawns.PrisonersOfColonySpawned == null)
                    {
                        Messages.Message("RH_TET_Beastmen_NoPrisonerAvailable".Translate(), MessageTypeDefOf.RejectInput);
                        return;
                    }
                    if (herdstone.Map.mapPawns.PrisonersOfColonySpawnedCount <= 0)
                    {
                        Messages.Message("RH_TET_Beastmen_NoPrisonerAvailable".Translate(), MessageTypeDefOf.RejectInput);
                        return;
                    }


                    /// Cycle through possible candidates in the map's prisoner list
                    foreach (Pawn candidate in herdstone.Map.mapPawns.PrisonersOfColonySpawned)
                    {
                        if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(candidate, true) || (!allowBeastmen && HerdUtility.IsBeastman(candidate))) continue;
                        actorList.Add(candidate);
                    }
                    break;
                case ActorType.animalSacrifice:
                    ///
                    /// ERROR HANDLING
                    /// 

                    if (herdstone.Map.mapPawns.AllPawnsSpawned == null)
                    {
                        Messages.Message("No " + actorType.ToString() + "s available.", MessageTypeDefOf.RejectInput);
                        return;
                    }
                    if (herdstone.Map.mapPawns.AllPawnsSpawnedCount <= 0)
                    {
                        Messages.Message("No " + actorType.ToString() + "s available.", MessageTypeDefOf.RejectInput);
                        return;
                    }

                    /// Cycle through possible candidates in the player's owned animals list.
                    foreach (Pawn candidate in herdstone.Map.mapPawns.AllPawnsSpawned)
                    {
                        if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(candidate, true)) continue;
                        if (candidate.Faction != Faction.OfPlayer) continue;
                        if (candidate.RaceProps == null) continue;
                        if (!candidate.RaceProps.Animal) continue;
                        actorList.Add(candidate);
                    }

                    break;
            }

            /// Let the player know there are no prisoners available.
            if (actorList != null)
            {
                if (actorList.Count <= 0)
                {
                    Messages.Message("No " + actorType.ToString() + "s available.", MessageTypeDefOf.RejectInput);
                    return;
                }
            }

            ///
            /// Create float menus
            ///

            //There must always be a none.
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                herdstone.tempExecutioner = null;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            foreach (Pawn actor in actorList)
            {
                Action action;
                Pawn localCol = actor;
                action = delegate
                {
                    switch (actorType)
                    {
                        case ActorType.executioner:
                            MapComponent_SacrificeTracker.Get(herdstone.Map).lastUsedHerdstone = herdstone;
                            herdstone.tempExecutioner = localCol;
                            break;
                        case ActorType.shaman:
                            herdstone.tempShaman = localCol;
                            break;
                        case ActorType.offerer:
                            MapComponent_SacrificeTracker.Get(herdstone.Map).lastUsedHerdstone = herdstone;
                            herdstone.tempOfferer = localCol;
                            break;
                        case ActorType.prisoner:
                            MapComponent_SacrificeTracker.Get(herdstone.Map).lastUsedHerdstone = herdstone;
                            MapComponent_SacrificeTracker.Get(herdstone.Map).lastSacrificeType = HerdUtility.SacrificeType.humanlike;
                            herdstone.tempSacrifice = localCol;
                            break;
                        case ActorType.animalSacrifice:
                            MapComponent_SacrificeTracker.Get(herdstone.Map).lastUsedHerdstone = herdstone;
                            MapComponent_SacrificeTracker.Get(herdstone.Map).lastSacrificeType = HerdUtility.SacrificeType.animal;
                            herdstone.tempSacrifice = localCol;
                            break;
                    }
                };
                list.Add(new FloatMenuOption(localCol.LabelShort, action, MenuOptionPriority.Default, null, null, 0f, null));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        public enum GodType
        {
            WorshipGod = 0,
            OfferingGod = 1,
            SacrificeGod = 2
        }

        public static bool GodInfoCardButton(float x, float y, DarkGod god)
        {
            bool result;
            if ((bool)AccessTools.Method(type: typeof(Widgets), name: "InfoCardButtonWorker", parameters: new Type[] { typeof(float), typeof(float) }).Invoke(obj: null, parameters: new object[] { x, y }))
            {
                Find.WindowStack.Add(new Dialog_DarkGodInfoBox(god));
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public static void OpenGodSelectMenu(Building_MassiveHerdstone herdstone, GodType godType)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                herdstone.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedHerdstone = herdstone;
                herdstone.tempCurrentSacrificeGod = null;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            foreach (DarkGod current in DarkGodTracker.Get.GodCache.Keys)
            {
                Action action;
                DarkGod localGod = current;
                action = delegate
                {

                    MapComponent_SacrificeTracker.Get(herdstone.Map).lastUsedHerdstone = herdstone;
                    switch (godType)
                    {
                        case GodType.OfferingGod:
                            herdstone.tempCurrentOfferingGod = localGod;
                            break;
                        case GodType.SacrificeGod:
                            herdstone.tempCurrentSacrificeGod = localGod;
                            break;
                    }
                };
                Func<Rect, bool> extraPartOnGUI = (Rect rect) => GodInfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, current);
                list.Add(new FloatMenuOption(localGod.LabelCap, action, MenuOptionPriority.Default, null, null, 29f, extraPartOnGUI));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

    }
}
