using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    internal class ITab_FightingPitManagerUtility
    {
        public static float ButtonSize = 40f;
        public static float SpacingOffset = 15f;
        public static float ColumnSize = 245f;

        public static string FighterLabel(int index, Building_PitFightSpot pit)
        {
            if (pit.fighter1.p == null)
                return "Select";
            if (index == 0)
                return pit.fighter1.p.Name.ToStringShort;
            if (index == 1)
                return pit.fighter2.p.Name.ToStringShort;
            return "error";
        }

        public static void OpenActor1SelectMenu(Building_PitFightSpot pit)
        {
            List<Pawn> pawnList = new List<Pawn>();
            StringBuilder stringBuilder = new StringBuilder();
            if (pit.Map.mapPawns.FreeColonistsSpawned == null)
                Messages.Message("RH_TET_Beastmen_NoFighters".Translate(), MessageTypeDefOf.RejectInput, true);
            else if (pit.Map.mapPawns.FreeColonistsSpawnedCount <= 0)
            {
                Messages.Message("RH_TET_Beastmen_NoFighters".Translate(), MessageTypeDefOf.RejectInput, true);
            }
            else
            {
                foreach (Pawn pawn in pit.Map.mapPawns.FreeColonistsSpawned)
                    pawnList.Add(pawn);
                if (pawnList != null && pawnList.Count <= 0)
                {
                    Messages.Message("RH_TET_Beastmen_NoFighters".Translate(), MessageTypeDefOf.RejectInput, true);
                }
                else
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (Pawn pawn in pawnList)
                    {
                        Pawn localCol = pawn;
                        Action action = (Action)(() =>
                        {
                            if (localCol.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                            { 
                                pit.fighter1.p = localCol;
                                pit.fighter1.isInFight = false;
                                pit.fighter2.isInFight = false;
                            }
                            else
                                Messages.Message(localCol.Name.ToStringShort + " " + "RH_TET_Beastmen_FighterCantFight".Translate(), MessageTypeDefOf.RejectInput, true);
                        });
                        options.Add(new FloatMenuOption(localCol.LabelShort, action, MenuOptionPriority.Default, null, (Thing)null, 0.0f, (Func<Rect, bool>)null, (WorldObject)null));
                    }
                    Find.WindowStack.Add((Window)new FloatMenu(options));
                }
            }
        }

        public static void OpenActor2SelectMenu(Building_PitFightSpot pit)
        {
            List<Pawn> pawnList = new List<Pawn>();
            StringBuilder stringBuilder = new StringBuilder();
            if (pit.Map.mapPawns.FreeColonistsSpawned == null)
                Messages.Message("RH_TET_Beastmen_NoFighters".Translate(), MessageTypeDefOf.RejectInput, true);
            else if (pit.Map.mapPawns.FreeColonistsSpawnedCount <= 0)
            {
                Messages.Message("RH_TET_Beastmen_NoFighters".Translate(), MessageTypeDefOf.RejectInput, true);
            }
            else
            {
                foreach (Pawn pawn in pit.Map.mapPawns.FreeColonistsSpawned)
                    pawnList.Add(pawn);
                if (pawnList != null && pawnList.Count <= 0)
                {
                    Messages.Message("RH_TET_Beastmen_NoFighters".Translate(), MessageTypeDefOf.RejectInput, true);
                }
                else
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (Pawn pawn in pawnList)
                    {
                        Pawn localCol = pawn;
                        Action action = (Action)(() =>
                        {
                            if (localCol.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                            {
                                pit.fighter2.p = localCol;
                                pit.fighter2.isInFight = false;
                                pit.fighter1.isInFight = false;
                            }
                            else
                                Messages.Message(localCol.Name.ToStringShort + " " + "RH_TET_Beastmen_FighterCantFight".Translate(), MessageTypeDefOf.RejectInput, true);
                        });
                        options.Add(new FloatMenuOption(localCol.LabelShort, action, MenuOptionPriority.Default, null, (Thing)null, 0.0f, (Func<Rect, bool>)null, (WorldObject)null));
                    }
                    Find.WindowStack.Add((Window)new FloatMenu(options));
                }
            }
        }
    }
}
