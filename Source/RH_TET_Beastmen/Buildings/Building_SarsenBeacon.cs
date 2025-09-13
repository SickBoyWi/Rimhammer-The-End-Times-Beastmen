using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld.Planet;
using RimWorld;
using TheEndTimes_Magic;
using Verse.Sound;

namespace TheEndTimes_Beastmen
{
    public partial class Building_SarsenBeacon : Building
    {
        public bool isOn = false;

        public override IEnumerable<Gizmo> GetGizmos()
        {
       
            foreach (var g in base.GetGizmos())
                yield return g;

            if (!isOn)
            {
                var command_Action = new Command_Action
                {
                    action = TurnOn,
                    defaultLabel = "RH_TET_Beastmen_TurnOn".Translate(),
                    defaultDesc = "RH_TET_Beastmen_TurnOnDesc".Translate(),
                    disabledReason = "RH_TET_Beastmen_CommandDisabled".Translate(),
                    hotKey = KeyBindingDefOf.Misc1,
                    icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_TurnOn")
                };
                command_Action.Disabled = false;
                yield return command_Action;
            }
            else
            {
                var command_Action = new Command_Action
                {
                    action = TurnOff,
                    defaultLabel = "RH_TET_Beastmen_TurnOff".Translate(),
                    defaultDesc = "RH_TET_Beastmen_TurnOffDesc".Translate(),
                    disabledReason = "RH_TET_Beastmen_CommandDisabled".Translate(),
                    hotKey = KeyBindingDefOf.Misc1,
                    icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_TurnOff")
                };
                command_Action.Disabled = false;
                yield return command_Action;
            }

            yield break;
        }

        private void TurnOn()
        {
            this.isOn = true;

            List<Thing> thingList = new List<Thing>();
            // TODO: REMEMBER THIS ISSUE: MAKES THE WHOLE GAME GO TO CRAP.
            thingList.AddRange(this.Map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone));
            thingList.AddRange(this.Map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone_UpgradeOne));
            thingList.AddRange(this.Map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone_UpgradeTwo));
            thingList.AddRange(this.Map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone_UpgradeThree));

            if (thingList != null && thingList.Count > 0)
            {
                foreach (Thing t in thingList)
                {
                    Building_MassiveHerdstone herdstone = t as Building_MassiveHerdstone;
                    if (herdstone != null)
                    {
                        herdstone.sarsenBonus = 30000;
                    }
                }
            }
        }

        private void TurnOff()
        {
            this.isOn = false;

            List<Thing> thingList = new List<Thing>();
            thingList.AddRange(this.Map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone));
            thingList.AddRange(this.Map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone_UpgradeOne));
            thingList.AddRange(this.Map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone_UpgradeTwo));
            thingList.AddRange(this.Map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_MassiveHerdstone_UpgradeThree));

            if (thingList != null && thingList.Count > 0)
            {
                foreach (Thing t in thingList)
                {
                    Building_MassiveHerdstone herdstone = t as Building_MassiveHerdstone;
                    if (herdstone != null)
                        herdstone.sarsenBonus = 0;
                }
            }
        }

        protected override void Tick()
        {
            base.Tick();
            if (isOn &&  (Find.TickManager.TicksGame % 600 == 0))
            {
                FleckMaker.Static(this.Position, this.Map, FleckDefOf.PsycastAreaEffect, 4f);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref isOn, "isOn", false, false);
        }
    }
}