using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class CompProperties_FireOverlayMulti : CompProperties
    {
        public float fireSize = 1f;
        public Vector3 offset;

        public CompProperties_FireOverlayMulti()
        {
            this.compClass = typeof(CompFireOverlayMulti);
        }

        public override void DrawGhost(
          IntVec3 center,
          Rot4 rot,
          ThingDef thingDef,
          Color ghostCol,
          AltitudeLayer drawAltitude,
          Thing thing = null)
        {
            GhostUtility.GhostGraphicFor(CompFireOverlayMulti.FireGraphic, thingDef, ghostCol).DrawFromDef(center.ToVector3ShiftedWithAltitude(drawAltitude), rot, thingDef, 0.0f);
        }
    }
}
