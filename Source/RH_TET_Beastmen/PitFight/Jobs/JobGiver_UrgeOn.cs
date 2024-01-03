using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    public class JobGiver_UrgeOn : JobGiver_SpectateDutySpectateRect
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            return new Job(BeastmenDefOf.RH_TET_Beastmen_UrgeOnJob);
        }
    }
}
