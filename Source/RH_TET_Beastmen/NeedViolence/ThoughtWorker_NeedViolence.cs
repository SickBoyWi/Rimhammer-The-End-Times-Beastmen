using RimWorld;
using System;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class ThoughtWorker_NeedViolence : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            Need_Violence need = (Need_Violence)p.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == BeastmenDefOf.RH_TET_Beastmen_NeedViolence));

            if (need == null || p.HostFaction != null)
                return ThoughtState.Inactive;
            
            switch (need.CurCategory)
            {
                case ViolenceCategory.Obsessed:
                    return ThoughtState.ActiveAtStage(0);
                case ViolenceCategory.Requires:
                    return ThoughtState.ActiveAtStage(1);
                case ViolenceCategory.Craves:
                    return ThoughtState.ActiveAtStage(2);
                case ViolenceCategory.Desires:
                    return ThoughtState.ActiveAtStage(3);
                case ViolenceCategory.Wants:
                    return ThoughtState.ActiveAtStage(4);
                case ViolenceCategory.Free:
                    return ThoughtState.Inactive;
                default:
                    throw new InvalidOperationException("Unknown ViolenceCategory");
            }
        }
    }
}
