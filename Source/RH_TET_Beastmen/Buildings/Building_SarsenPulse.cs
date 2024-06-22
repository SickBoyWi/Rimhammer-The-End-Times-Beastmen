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
    public partial class Building_SarsenPulse : Building
    {
        private int ticksUntilNextUse = -1;
        public static int RESETTICKS = 60000 * 8;// 8 days.

        public override IEnumerable<Gizmo> GetGizmos()
        {
       
            foreach (var g in base.GetGizmos())
                yield return g;

            if (ticksUntilNextUse <= 0)
            {
                var command_Action = new Command_Action
                {
                    action = PerformPsychicPulse,
                    defaultLabel = "RH_TET_Beastmen_CommandPulse".Translate(),
                    defaultDesc = "RH_TET_Beastmen_CommandPulseDesc".Translate(),
                    disabledReason = "RH_TET_Beastmen_CommandDisabled".Translate(),
                    hotKey = KeyBindingDefOf.Misc1,
                    icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_PerformPulse")
                };
                command_Action.Disabled = false;
                if (ticksUntilNextUse > 0) command_Action.icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_PerformPulseDisabled");
                yield return command_Action;
            }

            if (Prefs.DevMode && ticksUntilNextUse > 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Reset Pulse",
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
            IEnumerable<Pawn> pawns = this.Map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(p => p.RaceProps.Animal && p.Faction != Faction.OfPlayer));
            bool flag = false;
            foreach (Pawn pawn in pawns)
            {
                if (pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, (string)null, true, true, false, (Pawn)null, false))
                    flag = true;
            }
            if (!flag)
                return;
            Messages.Message("RH_TET_Beastmen_PulseOccurred".Translate(), (LookTargets)(Thing)this, MessageTypeDefOf.ThreatSmall, true);
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(this.Map);
            if (this.Map != Find.CurrentMap)
                return;
            Find.CameraDriver.shaker.DoShake(4f);

            FleckMaker.Static(this.Position, this.Map, FleckDefOf.PsycastAreaEffect, 5f);

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