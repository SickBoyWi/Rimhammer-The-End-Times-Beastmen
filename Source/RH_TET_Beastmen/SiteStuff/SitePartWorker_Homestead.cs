using RimWorld.Planet;
using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class SitePartWorker_Homestead : SitePartWorker
    {
        public override string GetArrivedLetterPart(
          Map map,
          out LetterDef preferredLetterDef,
          out LookTargets lookTargets)
        {
            string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
            lookTargets = (LookTargets)((Thing)map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(x =>
            {
                if (x.RaceProps.Humanlike)
                    return x.HostileTo(Faction.OfPlayer);
                return false;
            })).FirstOrDefault<Pawn>());
            return arrivedLetterPart;
        }

        public override string GetPostProcessedThreatLabel(Site site, SitePart siteCoreOrPart)
        {
            return base.GetPostProcessedThreatLabel(site, siteCoreOrPart);
        }

        public override SitePartParams GenerateDefaultParams(
              float myThreatPoints,
              int tile,
              Faction faction)
        {
            SitePartParams defaultParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
            defaultParams.threatPoints = Mathf.Max(defaultParams.threatPoints, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement));
            return defaultParams;
        }
        
        protected int GetEnemiesCount(Site site, SitePartParams parms)
        {
            return PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms()
            {
                tile = site.Tile,
                faction = site.Faction,
                groupKind = PawnGroupKindDefOf.Settlement,
                points = parms.threatPoints,
                inhabitants = true,
                seed = new int?(OutpostSitePartUtility.GetPawnGroupMakerSeed(parms))
            }).Count<PawnKindDef>();
        }
    }
}
