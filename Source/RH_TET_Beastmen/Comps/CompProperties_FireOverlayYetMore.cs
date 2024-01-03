using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class CompProperties_FireOverlayYetMore : CompProperties
    {
        public float fireSize = 1f;
        public Vector3 offset;

        public CompProperties_FireOverlayYetMore()
        {
            this.compClass = typeof(CompFireOverlayYetMore);
        }

        public override void DrawGhost(
          IntVec3 center,
          Rot4 rot,
          ThingDef thingDef,
          Color ghostCol,
          AltitudeLayer drawAltitude,
          Thing thing = null)
        {
            GhostUtility.GhostGraphicFor(CompFireOverlayYetMore.FireGraphic, thingDef, ghostCol).DrawFromDef(center.ToVector3ShiftedWithAltitude(drawAltitude), rot, thingDef, 0.0f);
        }
    }
}
