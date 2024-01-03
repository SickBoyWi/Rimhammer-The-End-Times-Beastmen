using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class DefeatAllEnemiesQuestNoRequestorComp : WorldObjectComp
    {
        private bool active;

        public DefeatAllEnemiesQuestNoRequestorComp()
        {
        }

        public bool Active
        {
            get
            {
                return this.active;
            }
        }

        public void StartQuest()
        {
            this.StopQuest();
            this.active = true;
        }

        public void StopQuest()
        {
            this.active = false;
            RH_TET_BeastmenMod.questInEffect = false;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!this.active)
                return;
            MapParent parent = this.parent as MapParent;
            if (parent == null)
                return;
            this.CheckAllEnemiesDefeated(parent);
        }

        private void CheckAllEnemiesDefeated(MapParent mapParent)
        {
            if (!mapParent.HasMap || GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map))
                return;
            this.GiveRewardsAndSendLetter();
            this.StopQuest();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.active, "active", false, false);
        }

        private void GiveRewardsAndSendLetter()
        {
            Map map = Find.AnyPlayerHomeMap ?? ((MapParent)this.parent).Map;

            IntVec3 intVec3 = DropCellFinder.TradeDropSpot(map);
            string text = "RH_TET_Beastmen_HomesteadDestroyed".Translate();

            Find.LetterStack.ReceiveLetter("RH_TET_Beastmen_LabelHomesteadDestroyed".Translate(), text, LetterDefOf.PositiveEvent, (LookTargets)new GlobalTargetInfo(intVec3, map, false), null, null);
        }

        public override void PostPostRemove()
        {
            base.PostPostRemove();
        }

        public override string CompInspectStringExtra()
        {
            if (this.active)
                return "RH_TET_Beastmen_HomesteadInspectString".Translate().CapitalizeFirst();
            return (string)null;
        }
    }
}
