using RimWorld;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class Graphic_FlickerCustom4 : Graphic_Collection
    {
        private const int BaseTicksPerFrameChange = 15;
        private const int ExtraTicksPerFrameChange = 10;
        private const float MaxOffset = 0.05f;

        public override Material MatSingle
        {
            get
            {
                return this.subGraphics[Rand.Range(0, this.subGraphics.Length)].MatSingle;
            }
        }

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            if (thingDef == null)
                Log.ErrorOnce("Fire DrawWorker with null thingDef: " + (object)loc, 3427324);
            else if (this.subGraphics == null)
            {
                Log.ErrorOnce("Graphic_FlickerCustom2 has no subgraphics " + (object)thingDef, 358773632);
            }
            else
            {
                int ticksGame = Find.TickManager.TicksGame;
                if (thing != null) { 
                    ticksGame += Mathf.Abs(thing.thingIDNumber ^ 8453458);
                }
                float num2 = 1f;
                CompProperties_FireOverlayYetMore propertiesFireOverlay = (CompProperties_FireOverlayYetMore)null;
                Fire fire = thing as Fire;
                if (fire != null)
                    num2 = fire.fireSize;
                else if (thingDef != null)
                {
                    propertiesFireOverlay = thingDef.GetCompProperties<CompProperties_FireOverlayYetMore>();
                    if (propertiesFireOverlay != null)
                    { 
                        num2 = propertiesFireOverlay.fireSize;
                    }
                }
                int num1 = ticksGame / 21;
                int index = Mathf.Abs(num1 ^ (thing != null ? thing.thingIDNumber : 0) * 391) % this.subGraphics.Length;
                if (index < 0 || index >= this.subGraphics.Length)
                {
                    Log.ErrorOnce("Fire drawing out of range: " + (object)index, 7453435);
                    index = 0;
                }
                Graphic subGraphic = this.subGraphics[index];
                float num3 = Mathf.Min(num2 / 1.2f, 1.2f);
                Vector3 vector3 = GenRadial.RadialPattern[num1 % GenRadial.RadialPattern.Length].ToVector3() / GenRadial.MaxRadialPatternRadius * 0.05f;
                Vector3 pos = loc + vector3 * num2;
                if (propertiesFireOverlay != null)
                    pos += propertiesFireOverlay.offset;
                Vector3 s = new Vector3(num3, 1f, num3);
                Matrix4x4 matrix = new Matrix4x4();
                matrix.SetTRS(pos, Quaternion.identity, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, subGraphic.MatSingle, 0);
            }
        }

        public override string ToString()
        {
            return "Flicker(subGraphic[0]=" + this.subGraphics[0].ToString() + ", count=" + (object)this.subGraphics.Length + ")";
        }
    }
}
