using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class FinalDestructionComp : WorldObjectComp
    {
        public FinalDestructionSite Parent;

        public FinalDestructionComp()
        {
            this.Parent = (FinalDestructionSite)this.parent;
        }

        public override void CompTick()
        {
            base.CompTick();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            return stringBuilder.ToString();
        }
    }
}
