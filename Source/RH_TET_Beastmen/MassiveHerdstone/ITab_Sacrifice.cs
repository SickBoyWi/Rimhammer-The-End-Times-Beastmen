using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld.Planet;
using RimWorld;

namespace TheEndTimes_Beastmen
{
    public class ITab_Sacrifice : ITab
    {
        protected Building_MassiveHerdstone SelHerdstone
        {
            get
            {
                return (Building_MassiveHerdstone)base.SelThing;
            }
        }

        public ITab_Sacrifice()
        {
            this.size = ITab_HerdstoneSacrificeCardUtil.SacrificeCardSize;
            this.labelKey = "RH_TET_Beastmen_TabOffering"; 
        }

        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(5f);
            ITab_HerdstoneSacrificeCardUtil.DrawSacrificeCard(rect, SelHerdstone);
        }
    }

}

