using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheEndTimes_Magic;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class Bill_Convert : IExposable
    {
        private Pawn victim;
        private Pawn executioner;
        private List<Pawn> congregation;

        public Pawn Victim => victim;
        public Pawn Executioner => executioner;
        public List<Pawn> Congregation { get => congregation; set => congregation = value; }

        public Bill_Convert()
        {

        }

        public Bill_Convert(Pawn newSacrifice, Pawn newExecutioner)
        {
            this.victim = newSacrifice;
            this.executioner = newExecutioner;
        }

        public void ExposeData()
        {
            Scribe_References.Look<Pawn>(ref this.victim, "victim");
            Scribe_References.Look<Pawn>(ref this.executioner, "executioner");
            Scribe_Collections.Look<Pawn>(ref this.congregation, "congregation", LookMode.Reference);
        }
    }
}