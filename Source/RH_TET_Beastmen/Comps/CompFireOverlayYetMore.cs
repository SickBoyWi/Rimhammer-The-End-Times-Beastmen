﻿using RimWorld;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    [StaticConstructorOnStartup]
    public class CompFireOverlayYetMore : ThingComp
    {
        public static readonly Graphic FireGraphic = GraphicDatabase.Get<Graphic_FlickerCustom4>("Things/Special/Fire", ShaderDatabase.TransparentPostLight, new Vector2(5, 5), Color.white);
        protected CompRefuelable refuelableComp;

        public CompProperties_FireOverlayYetMore Props
        {
            get
            {
                return (CompProperties_FireOverlayYetMore)this.props;
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (this.refuelableComp != null && !this.refuelableComp.HasFuel)
                return;
            Vector3 drawPos = this.parent.DrawPos;
            drawPos.y += 0.04285714f;
            CompFireOverlayYetMore.FireGraphic.Draw(drawPos, Rot4.North, (Thing)this.parent, 0.0f);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.refuelableComp = this.parent.GetComp<CompRefuelable>();
        }
    }
}
