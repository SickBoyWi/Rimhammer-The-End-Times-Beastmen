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
    public partial class Building_SarsenGas : Building
    {
        private int ticksUntilNextUse = -1;
        public static int RESETTICKS = 60000 * 5;// 5 days.

        public static float initialConcentration = 15000;

        public override IEnumerable<Gizmo> GetGizmos()
        {
       
            foreach (var g in base.GetGizmos())
                yield return g;

            if (ticksUntilNextUse <= 0)
            {
                var command_Action = new Command_Action
                {
                    action = PerformPsychicPulse,
                    defaultLabel = "RH_TET_Beastmen_CommandGas".Translate(),
                    defaultDesc = "RH_TET_Beastmen_CommandGasDesc".Translate(),
                    disabled = false,
                    disabledReason = "RH_TET_Beastmen_CommandDisabled".Translate(),
                    hotKey = KeyBindingDefOf.Misc1,
                    icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_ReleaseGas")
                };
                if (ticksUntilNextUse > 0) command_Action.icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_ReleaseGasDisabled");
                yield return command_Action;
            }

            if (Prefs.DevMode && ticksUntilNextUse > 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Reset Gas",
                    action = delegate
                    {
                        ticksUntilNextUse = -1;
                    }
                };
            }

            yield break;
        }

        private void PerformPsychicPulse()
        {
            IntVec3 position = this.Position;
            Map map2 = this.Map;

            Thing thing = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_CombatGas, (ThingDef)null);
            GenPlace.TryPlaceThing(thing, position, map2, ThingPlaceMode.Direct, (Action<Verse.Thing, int>)null, (Predicate<IntVec3>)null, new Rot4());
            (thing as GasCloud).ReceiveConcentration(initialConcentration);

            FleckMaker.ThrowSmoke(position.ToVector3(), map2, 2f);

            SoundInfo info = SoundInfo.InMap(new TargetInfo(position, map2, false), MaintenanceType.None);
            info.pitchFactor = 1.3f;
            info.volumeFactor = 1.6f;
            SoundDefOf.Hive_Spawn.PlayOneShot(info);

            Thing thing2 = ThingMaker.MakeThing(RH_TET_MagicDefOf.RH_TET_CombatGas, (ThingDef)null);
            GenPlace.TryPlaceThing(thing2, position, map2, ThingPlaceMode.Direct, (Action<Verse.Thing, int>)null, (Predicate<IntVec3>)null, new Rot4());
            (thing2 as GasCloud).ReceiveConcentration(initialConcentration);

            FleckMaker.Static(this.Position, this.Map, FleckDefOf.PsycastAreaEffect, 3f);

            ticksUntilNextUse = RESETTICKS;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.AppendLine("RH_TET_Beastmen_EffectAvailableStart".Translate());

            if (ticksUntilNextUse > 0)
            {
                stringBuilder.Append(ticksUntilNextUse.ToStringTicksToPeriod(true, false, false, true));
            }
            else
            {
                stringBuilder.Append("RH_TET_Beastmen_Now".Translate());
            }

            return stringBuilder.ToString();
        }

        public override void Tick()
        {
            base.Tick();

            if (ticksUntilNextUse > 0)
            {
                ticksUntilNextUse--;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref ticksUntilNextUse, "ticksUntilNextUse", -1, false);
        }
    }
}