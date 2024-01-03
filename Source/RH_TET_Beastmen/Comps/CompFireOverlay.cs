using RimWorld;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    [StaticConstructorOnStartup]
    public class CompFireOverlay : ThingComp
    {
        public static readonly Graphic FireGraphic = GraphicDatabase.Get<Graphic_FlickerCustom>("Things/Special/Fire", ShaderDatabase.TransparentPostLight, new Vector2(5, 5), Color.white);
        protected CompRefuelable refuelableComp;

        public CompProperties_FireOverlay Props
        {
            get
            {
                return (CompProperties_FireOverlay)this.props;
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (this.refuelableComp != null && !this.refuelableComp.HasFuel)
                return;
            Vector3 drawPos = this.parent.DrawPos;
            drawPos.y += 0.04285714f;
            CompFireOverlay.FireGraphic.Draw(drawPos, Rot4.North, (Thing)this.parent, 0.0f);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.refuelableComp = this.parent.GetComp<CompRefuelable>();
        }
    }
}
