using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Beastmen
{
    public class Building_HeadSpike : Building, IObservedThoughtGiver
    {
        public Thought_Memory GiveObservedThought(Pawn p)
        {
            Thought_MemoryObservation memoryObservation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(ThoughtDef.Named("ObservedLayingRottingCorpse"));
            memoryObservation.Target = (Thing)this;
            return (Thought_Memory)memoryObservation;
        }

        public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
        {
            return (HistoryEventDef)null;
        }
    }
}
