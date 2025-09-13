using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public class JobDriver_UrgeOnPitFighter : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return this.InteractToil();
            yield return new Toil()
            {
                tickAction = (Action)(() =>
                {
                    // Make sure only player pawns can do this.
                    if (this.pawn.Faction != null && !this.pawn.IsPrisoner && (this.pawn.Faction.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction")
                    || this.pawn.Faction.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction")))
                    {
                        if (((Building_PitFightSpot)this.pawn.mindState.duty.focus.Thing).currentState == Building_PitFightSpot.State.fighting)
                            JoyUtility.JoyTickCheckEnd(this.pawn, 1, JoyTickFullJoyAction.None, 1f, (Building)null);
                        this.pawn.GainComfortFromCellIfPossible(1);
                        if (!this.pawn.IsHashIntervalTick(100))
                            return;
                        this.pawn.jobs.CheckForJobOverride();
                    }
                }),
                defaultCompleteMode = ToilCompleteMode.Never,
                handlingFacing = true
            };
        }

        private Toil InteractToil()
        {
            return Toils_General.Do((Action)(() =>
            {
                if (((Building_PitFightSpot)this.pawn.mindState.duty.focus.Thing).currentState == Building_PitFightSpot.State.fighting)
                {
                    RenderTexture renderTexture = PortraitsCache.Get(
                        RH_TET_BeastmenMod.random.Next(0, 2) != 0 ? ((Building_PitFightSpot)this.pawn.mindState.duty.focus.Thing).fighter2.p : ((Building_PitFightSpot)this.pawn.mindState.duty.focus.Thing).fighter1.p, 
                        ColonistBarColonistDrawer.PawnTextureSize, 
                        Rot4.South);
                    Texture2D aTop = new Texture2D(75, 75);
                    for (int x = 0; x < 75; ++x)
                    {
                        for (int y = 0; y < 75; ++y)
                            aTop.SetPixel(x, y, Color.clear);
                    }
                    RenderTexture.active = renderTexture;
                    aTop.ReadPixels(new Rect(-10f, 0.0f, 50f, (float)renderTexture.height), 0, 0);
                    aTop.Apply();
                    Texture2D symbol1 = BeastmenDefOf.RH_TET_Beastmen_UrgeOn.GetSymbol();
                    RenderTexture temporary = RenderTexture.GetTemporary(symbol1.width, symbol1.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                    Graphics.Blit((Texture)symbol1, temporary);
                    RenderTexture active = RenderTexture.active;
                    RenderTexture.active = temporary;
                    Texture2D aBottom = new Texture2D(symbol1.width, symbol1.height);
                    aBottom.ReadPixels(new Rect(0.0f, 0.0f, (float)temporary.width, (float)temporary.height), 0, 0);
                    aBottom.Apply();
                    RenderTexture.active = active;
                    RenderTexture.ReleaseTemporary(temporary);
                    Texture2D symbol2 = JobDriver_UrgeOnPitFighter.MergeTextures(aBottom, aTop);
                    MoteMaker.MakeInteractionBubble(this.pawn, (Pawn)null, ThingDefOf.Mote_Speech, symbol2);
                }
            }));
        }

        public static Texture2D MergeTextures(Texture2D aBottom, Texture2D aTop)
        {
            if (aBottom.width != aTop.width || aBottom.height != aTop.height)
                throw new InvalidOperationException("AlphaBlend only works with two equal sized images");
            Color[] pixels1 = aBottom.GetPixels();
            Color[] pixels2 = aTop.GetPixels();
            int length = pixels1.Length;
            Color[] colors = new Color[length];
            for (int index = 0; index < length; ++index)
            {
                Color color1 = pixels1[index];
                Color color2 = pixels2[index];
                float a = color2.a;
                float num1 = 1f - color2.a;
                float num2 = a + num1 * color1.a;
                Color color3 = (color2 * a + color1 * color1.a * num1) / num2;
                color3.a = num2;
                colors[index] = color3;
            }
            Texture2D texture2D = new Texture2D(aTop.width, aTop.height);
            texture2D.SetPixels(colors);
            texture2D.Apply();
            return texture2D;
        }
    }
}
