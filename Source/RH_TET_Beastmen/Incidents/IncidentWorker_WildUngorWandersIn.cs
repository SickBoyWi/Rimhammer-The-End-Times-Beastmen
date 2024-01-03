using RimWorld;
using System;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class IncidentWorker_WildUngorWandersIn : IncidentWorker
    {
        private Random r = new Random(Guid.NewGuid().GetHashCode());

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (RH_TET_BeastmenMod.anyOneCanBeastActive || Faction.OfPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
            || Faction.OfPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
            {
                if (!base.CanFireNowSub(parms))
                {
                    return false;
                }
                Faction faction;
                if (!this.TryFindFormerFaction(out faction))
                {
                    return false;
                }
                Map map = (Map)parms.target;
                IntVec3 intVec;
                return !map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && map.mapTemperature.SeasonAcceptableFor(ThingDefOf.Human) && this.TryFindEntryCell(map, out intVec);
            }
            else
                return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 loc;
            if (!this.TryFindEntryCell(map, out loc))
            {
                return false;
            }
            Faction faction;
            if (!this.TryFindFormerFaction(out faction))
            {
                return false;
            }

            Faction ofPlayer = Faction.OfPlayer;
            PawnKindDef wildToUse = PawnKindDef.Named("RH_TET_Beastmen_WildUngor");

            Pawn pawn = PawnGenerator.GeneratePawn(wildToUse, faction);
            try
            {
                pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            }
            catch
            {
                // Ignore if no ideos present.
            }
            string labelShortSave = pawn.LabelShort;

            pawn.SetFaction(null, null);
            pawn.ChangeKind(PawnKindDefOf.WildMan);

            GenSpawn.Spawn(pawn, loc, map, WipeMode.Vanish);
            TaggedString label = this.def.letterLabel.Formatted(labelShortSave, pawn.Named("PAWN"));
            TaggedString text = this.def.letterText.Formatted(labelShortSave, pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN").CapitalizeFirst();
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
            Find.LetterStack.ReceiveLetter(label, text, this.def.letterDef, pawn, null, null);
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Ignore, out cell);
        }

        private bool TryFindFormerFaction(out Faction formerFaction)
        {
            Faction ofPlayer = Faction.OfPlayer;

            if (ofPlayer.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                || ofPlayer.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction"))
            {
                foreach (Faction current in Find.FactionManager.AllFactionsInViewOrder)
                {
                    if (current.def.defName.Equals("RH_TET_Beastmen_GorFaction"))
                    {
                        formerFaction = current;
                        return true;
                    }
                }
            }

            formerFaction = null;
            return false;
        }
    }
}
