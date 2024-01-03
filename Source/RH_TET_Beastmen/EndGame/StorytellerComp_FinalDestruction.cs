using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class StorytellerComp_FinalDestruction : StorytellerComp
    {
        private const int StartOnDay = 9999999;

        private int IntervalsPassed
        {
            get
            {
                return Find.TickManager.TicksGame / 1000;
            }
        }

        [DebuggerHidden]
        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            return null;
        }
    }
}
