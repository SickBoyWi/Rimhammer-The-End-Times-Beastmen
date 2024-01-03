using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

/**
 * From core code for need outdoors.
 * */
namespace TheEndTimes_Beastmen
{
    public class Need_Violence : Need
    {
        private const float DeltaFactor_InBed = 0.2f;
        private float lastEffectiveDelta;
        public bool didViolence = false;

        public Need_Violence(Pawn pawn)
          : base(pawn)
        {
            this.threshPercents = new List<float>();
            this.threshPercents.Add(0.8f);
            this.threshPercents.Add(0.6f);
            this.threshPercents.Add(0.4f);
            this.threshPercents.Add(0.2f);
            this.threshPercents.Add(0.05f);
        }

        public override int GUIChangeArrow
        {
            get
            {
                if (this.IsFrozen)
                    return 0;
                return Math.Sign(this.lastEffectiveDelta);
            }
        }

        public ViolenceCategory CurCategory
        {
            get
            {
                if ((double)this.CurLevel > 0.800000011920929)
                    return ViolenceCategory.Free;
                if ((double)this.CurLevel > 0.600000023841858)
                    return ViolenceCategory.Wants;
                if ((double)this.CurLevel > 0.400000005960464)
                    return ViolenceCategory.Desires;
                if ((double)this.CurLevel > 0.200000002980232)
                    return ViolenceCategory.Craves;
                return (double)this.CurLevel > 0.0500000007450581 ? ViolenceCategory.Requires : ViolenceCategory.Obsessed;
            }
        }

        public override bool ShowOnNeedList
        {
            get
            {
                return !this.Disabled;
            }
        }

        public bool Disabled
        {
            get
            {
                if (Settings.MakePawnsRequireViolence && (HerdUtility.IsBeastman(this.pawn)))// || RH_TET_BeastmenMod.anyOneCanBeastActive))
                {
                    if (pawn.WorkTagIsDisabled(WorkTags.Violent))
                    {
                        // If non-violent beastman (should never happen), then disable.
                        return true;
                    }
                    else
                        return false;
                }
                // Always disabled for non-beastmen.
                return true;
                
            }
        }

        public override void SetInitialLevel()
        {
            this.CurLevel = 1f;
        }

        public override void NeedInterval()
        {
            if (this.Disabled)
            {
                this.CurLevel = 1f;
            }
            else
            {
                if (this.IsFrozen)
                    return;

                float b = 0.2f;
                bool flag = !this.pawn.Spawned;
                float num1;
                
                if (!flag)
                {
                    num1 = -0.45f;
                    b = 0.0f;
                }
                else
                    num1 = -0.4f;

                // Reduce the violence need if a pawn is in bed.
                if (this.pawn.InBed() && (double)num1 < 0.0)
                    num1 *= 0.2f;

                float num2 = num1 * (1f / 300f); // This sets how fast the need gets worse. 400f was there for underground from core code.
                                                 // Will always be negative here.

                // If they did violence recently, set back to 1 by setting num2 to 1.0.
                // This only gets set to true in the harmony class: Pawn_PostApplyDamage_PostFix.
                if (this.didViolence)
                {
                    num2 = 1;
                }

                float curLevel = this.CurLevel;

                if ((double)num2 < 0.0)
                    this.CurLevel = Mathf.Min(this.CurLevel, Mathf.Max(this.CurLevel + num2, b));
                else
                    this.CurLevel = Mathf.Min(this.CurLevel + num2, 1f);

                this.lastEffectiveDelta = this.CurLevel - curLevel;
            }
        }
    }
}
