using RimWorld;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class ThoughtWorker_GettingSoft : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            GettingSoftDef expectationDef = GettingSoftUtility.CurrentExpectationFor(p);
            if (expectationDef == null)
                return ThoughtState.Inactive;
            return ThoughtState.ActiveAtStage(expectationDef.thoughtStage);
        }
    }
}