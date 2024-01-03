using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class IncidentWorker_CygorJoin : IncidentWorker
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
            PawnKindDef kind = BeastmenDefOf.RH_TET_Beastmen_CygorBeastmenPlayer;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out result, target, CellFinder.EdgeRoadChance_Animal, false, (Predicate<IntVec3>)null))
                return false;
            int num = 1;
            for (int index = 0; index < num; ++index)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, target, 12, (Predicate<IntVec3>)null);
                Pawn pawn = PawnGenerator.GeneratePawn(kind, (Faction)null);

                try
                {
                    pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
                }
                catch
                {
                    // Ignore if no ideos present.
                }

                GenSpawn.Spawn((Thing)pawn, loc, target, Rot4.Random, WipeMode.Vanish, false);
                pawn.SetFaction(Faction.OfPlayer, (Pawn)null);
            }
            Find.LetterStack.ReceiveLetter(kind.label.CapitalizeFirst() + " " + "RH_TET_Beastmen_Joins".Translate(), "RH_TET_Beastmen_CygorJoins".Translate(), LetterDefOf.PositiveEvent, (LookTargets)new TargetInfo(result, target, false), (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);

            return true;
        }
    }
}
