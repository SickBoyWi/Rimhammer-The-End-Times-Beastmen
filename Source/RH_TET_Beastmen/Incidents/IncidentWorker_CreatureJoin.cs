using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class IncidentWorker_CreatureJoin : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Faction ofPlayer = Faction.OfPlayer;

            // Make sure this only happens to beastmen.
            if (RH_TET_BeastmenMod.anyOneCanBeastActive || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
            {
                if (!base.CanFireNowSub(parms))
                    return false;
                Map target = (Map)parms.target;
                IntVec3 result;
                if (RCellFinder.TryFindRandomPawnEntryCell(out result, target, CellFinder.EdgeRoadChance_Animal, false, (Predicate<IntVec3>)null))
                {
                    return true;
                }
                return false;
            }
            else
                return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map target = (Map)parms.target;
            IntVec3 result;

            PawnKindDef kind;
            int rando = RH_TET_BeastmenMod.random.Next(0, 100);
            int num = 0;

            if (rando < 20)
            {
                kind = BeastmenDefOf.RH_TET_Beastmen_BeastmenPawnHarpy;
                num = RH_TET_BeastmenMod.random.Next(2, 6);
            }
            else if (rando < 40)
            { 
                kind = BeastmenDefOf.RH_TET_ChaosWarhoundAnimal;
                num = RH_TET_BeastmenMod.random.Next(2, 6);
            }
            else if (rando < 55)
            { 
                kind = BeastmenDefOf.RH_TET_ChaosSpawnAnimal;
                num = RH_TET_BeastmenMod.random.Next(1, 3);
            }
            else if (rando < 75)
            { 
                kind = BeastmenDefOf.RH_TET_TuskgorAnimal;
                num = RH_TET_BeastmenMod.random.Next(2, 5);
            }
            else if (rando < 82)
            { 
                kind = BeastmenDefOf.RH_TET_RazorgorAnimal;
                num = RH_TET_BeastmenMod.random.Next(1, 3);
            }
            else if (rando < 91)
            { 
                kind = BeastmenDefOf.RH_TET_ChaosSpawnAncientAnimal;
                num = 1;
            }
            else
            { 
                kind = BeastmenDefOf.RH_TET_JabberslytheAnimal;
                num = 1;
            }

            if (!RCellFinder.TryFindRandomPawnEntryCell(out result, target, CellFinder.EdgeRoadChance_Animal, false, (Predicate<IntVec3>)null))
                return false;

            IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, target, 12, (Predicate<IntVec3>)null);
            for (int index = 0; index < num; ++index)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(kind, (Faction)null);
                //pawn.setSk
                GenSpawn.Spawn((Thing)pawn, loc, target, Rot4.Random, WipeMode.Vanish, false);
                pawn.SetFaction(Faction.OfPlayer, (Pawn)null);

                pawn.RaceProps.intelligence = Intelligence.Animal;
            }

            if (num == 1)
                Find.LetterStack.ReceiveLetter(kind.label.CapitalizeFirst() + " " + "RH_TET_Beastmen_Joins".Translate(), "RH_TET_Beastmen_CreaturesJoin".Translate(), LetterDefOf.PositiveEvent, (LookTargets)new TargetInfo(result, target, false), (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);
            else
                Find.LetterStack.ReceiveLetter(kind.label.CapitalizeFirst() + "s " + "RH_TET_Beastmen_Join".Translate(), "RH_TET_Beastmen_CreatureJoins".Translate(), LetterDefOf.PositiveEvent, (LookTargets)new TargetInfo(result, target, false), (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);

            return true;
        }
    }
}
