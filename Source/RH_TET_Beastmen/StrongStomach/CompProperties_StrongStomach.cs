using Verse;

namespace TheEndTimes_Beastmen
{
    public class CompProperties_StrongStomach : CompProperties
    {
        public int rateInTicks = 800;

        public CompProperties_StrongStomach()
        {
            this.compClass = typeof(CompStrongStomach);
        }
    }
}
