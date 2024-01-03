using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class CompProperties_FireOverlayEvenMore : CompProperties
    {
        public float fireSize = 1f;
        public Vector3 offset;

        public CompProperties_FireOverlayEvenMore()
        {
            this.compClass = typeof(CompFireOverlayEvenMore);
        }

        public override void DrawGhost(
          IntVec3 center,
          Rot4 rot,
          ThingDef thingDef,
          Color ghostCol,
          AltitudeLayer drawAltitude,
          Thing thing = null)
        {
            GhostUtility.GhostGraphicFor(CompFireOverlayEvenMore.FireGraphic, thingDef, ghostCol).DrawFromDef(center.ToVector3ShiftedWithAltitude(drawAltitude), rot, thingDef, 0.0f);
        }
    }
}
