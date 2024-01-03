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

namespace TheEndTimes_Beastmen
{
    public partial class Building_SarsenTutor : Building
    {
        private bool hasGift = true;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
                yield return g;

            if (hasGift)
            {
                var command_Action = new Command_Action
                {
                    action = SpawnGift,
                    defaultLabel = "RH_TET_Beastmen_CommandGift".Translate(),
                    defaultDesc = "RH_TET_Beastmen_CommandGiftDesc".Translate(),
                    disabled = false,
                    disabledReason = "RH_TET_Beastmen_CommandDisabled".Translate(),
                    hotKey = KeyBindingDefOf.Misc1,
                    icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_SpawnGift")
                };
                if (!hasGift) command_Action.icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_SpawnGiftDisabled");
                yield return command_Action;
            }

            if (Prefs.DevMode && !hasGift)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Reset Gift Availability",
                    action = delegate
                    {
                        hasGift = true;
                    }
                };
            }

            yield break;
        }

        private void SpawnGift()
        {
            RewardUtility.SpawnGift(this, RH_TET_MagicDefOf.RH_TET_StoneOfWisdomChaosUndiv);
            hasGift = false;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref hasGift, "hasGift", true, false);
        }
    }
}