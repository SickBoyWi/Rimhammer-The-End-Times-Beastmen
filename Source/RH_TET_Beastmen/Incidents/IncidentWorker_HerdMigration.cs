using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Beastmen
{
    public class IncidentWorker_HerdMigration : IncidentWorker
    {
        private static readonly IntRange AnimalsCount = new IntRange(3, 5);
        private const float MinTotalBodySize = 4f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            // Make sure this only happens to beastmen.
            if (RH_TET_BeastmenMod.anyOneCanBeastActive || Faction.OfPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                || Faction.OfPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
            {
                Map target = (Map)parms.target;
                PawnKindDef animalKind;
                if (this.TryFindAnimalKind(target.Tile, out animalKind))
                {
                    IntVec3 start;
                    IntVec3 end;
                    return this.TryFindStartAndEndCells(target, out start, out end);
                }
            }
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map target = (Map)parms.target;
            PawnKindDef animalKind;
            IntVec3 start;
            IntVec3 end;
            if (!this.TryFindAnimalKind(target.Tile, out animalKind) || !this.TryFindStartAndEndCells(target, out start, out end))
                return false;
            Rot4 rot = Rot4.FromAngleFlat((target.Center - start).AngleFlat);
            List<Pawn> animals = this.GenerateAnimals(animalKind, target.Tile);
            for (int index = 0; index < animals.Count; ++index)
                GenSpawn.Spawn((Thing)animals[index], CellFinder.RandomClosewalkCellNear(start, target, 10, (Predicate<IntVec3>)null), target, rot, WipeMode.Vanish, false);
            LordMaker.MakeNewLord((Faction)null, (LordJob)new LordJob_ExitMapNear(end, LocomotionUrgency.Walk, 12f, false, false), target, (IEnumerable<Pawn>)animals);
            string text = string.Format(this.def.letterText, (object)animalKind.GetLabelPlural(-1)).CapitalizeFirst();
            Find.LetterStack.ReceiveLetter(string.Format(this.def.letterLabel, (object)animalKind.GetLabelPlural(-1).CapitalizeFirst()), text, this.def.letterDef, (LookTargets)((Thing)animals[0]), (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);
            
            return true;
        }

        private bool TryFindAnimalKind(int tile, out PawnKindDef animalKind)
        {
            return DefDatabase<PawnKindDef>.AllDefs.Where<PawnKindDef>((Func<PawnKindDef, bool>)(k =>
            {
                if (k.RaceProps.CanDoHerdMigration)
                    return Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race);
                return false;
            })).TryRandomElementByWeight<PawnKindDef>((Func<PawnKindDef, float>)(x => Mathf.Lerp(0.2f, 1f, x.RaceProps.wildness)), out animalKind);
        }

        private bool TryFindStartAndEndCells(Map map, out IntVec3 start, out IntVec3 end)
        {
            if (!RCellFinder.TryFindRandomPawnEntryCell(out start, map, CellFinder.EdgeRoadChance_Animal, false, (Predicate<IntVec3>)null))
            {
                end = IntVec3.Invalid;
                return false;
            }
            end = IntVec3.Invalid;
            for (int index = 0; index < 8; ++index)
            {
                IntVec3 startLocal = start;
                IntVec3 result;
                if (CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)(x => map.reachability.CanReach(startLocal, (LocalTargetInfo)x, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly)), map, CellFinder.EdgeRoadChance_Ignore, out result))
                {
                    if (!end.IsValid || result.DistanceToSquared(start) > end.DistanceToSquared(start))
                        end = result;
                }
                else
                    break;
            }
            return end.IsValid;
        }

        private List<Pawn> GenerateAnimals(PawnKindDef animalKind, int tile)
        {
            int num1 = Mathf.Max(IncidentWorker_HerdMigration.AnimalsCount.RandomInRange, Mathf.CeilToInt(4f / animalKind.RaceProps.baseBodySize));
            List<Pawn> pawnList = new List<Pawn>();
            for (int index = 0; index < num1; ++index)
            {
                PawnKindDef pawnKindDef = animalKind;
                int num2 = tile;
                PawnKindDef kind = pawnKindDef;
                int tile1 = num2;
                PawnGenerationRequest local = new PawnGenerationRequest(kind, (Faction)null, PawnGenerationContext.NonPlayer, tile1, 
                    false, false, false, 
                    false, false, 0.0f, 
                    false, true, false, 
                    false, false, false,
                    false, false, false,
                    0.0f, 0.0f, null, 
                    0.0f, (Predicate<Pawn>)null, (Predicate<Pawn>)null, 
                    null, null, null, 
                    null, null, null, 
                    null, (string)null, null, 
                    null, false, false,
                    false, false, null);
                Pawn pawn = PawnGenerator.GeneratePawn(local);
                pawnList.Add(pawn);
            }
            return pawnList;
        }
    }
}
