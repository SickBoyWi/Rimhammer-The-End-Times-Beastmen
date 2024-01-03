using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheEndTimes_Magic;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class Bill_Sacrifice : IExposable
    {
        private Pawn sacrifice;
        private Pawn executioner;
        private List<Pawn> congregation;
        private DarkGod darkGod;

        public Pawn Sacrifice => sacrifice;
        public Pawn Executioner => executioner;
        public List<Pawn> Congregation { get => congregation; set => congregation = value; }
        public DarkGod God => darkGod;
        public HerdUtility.SacrificeType Type
        {
            get
            {
                return (Sacrifice?.RaceProps?.Animal ?? false) ? HerdUtility.SacrificeType.animal : HerdUtility.SacrificeType.humanlike;
            }
        }

        public Bill_Sacrifice()
        {

        }

        public Bill_Sacrifice(Pawn newSacrifice, Pawn newExecutioner, DarkGod newGod)
        {
            this.sacrifice = newSacrifice;
            this.executioner = newExecutioner;
            this.darkGod = newGod;
        }

        public void ExposeData()
        {
            Scribe_References.Look<Pawn>(ref this.sacrifice, "sacrifice");
            Scribe_References.Look<Pawn>(ref this.executioner, "executioner");
            Scribe_Collections.Look<Pawn>(ref this.congregation, "congregation", LookMode.Reference);
            Scribe_References.Look<DarkGod>(ref this.darkGod, "god");
        }
    }
}