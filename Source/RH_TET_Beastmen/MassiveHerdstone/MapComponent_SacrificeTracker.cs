using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld.Planet;
using RimWorld;

namespace TheEndTimes_Beastmen
{
    public partial class MapComponent_SacrificeTracker : MapComponent
    {

        public static int resurrectionTicks = 10000;

        /// Unspeakable Oath Variables
        public List<Pawn> toBeResurrected = new List<Pawn>();
        public int ticksUntilResurrection = -999;

        /// Defend the Brood Variables
        public List<Pawn> defendTheBroodPawns = new List<Pawn>();

        /// Sacrifice Tracker Variables
        public List<Pawn> unspeakableOathPawns = new List<Pawn>();


        public string lastSacrificeName = "";
        public bool wasDoubleTheFun = false;
        public bool ASMwasPet = false;
        public bool ASMwasBonded = false;
        public bool ASMwasExcMaster = false;
        public bool HSMwasFamily = false;
        public IntVec3 lastLocation = IntVec3.Invalid;
        public PawnRelationDef lastRelation = null;
        public IncidentDef lastDoubleSideEffect = null;
        public IncidentDef lastSideEffect = null;
        public Building_MassiveHerdstone lastUsedHerdstone = null; //Default
        public HerdUtility.SacrificeResult lastResult = HerdUtility.SacrificeResult.none; // Default
        public HerdUtility.SacrificeType lastSacrificeType = HerdUtility.SacrificeType.none;
        public HerdUtility.OfferingSize lastOfferingSize = HerdUtility.OfferingSize.none;

        #region Setup
        
        public static MapComponent_SacrificeTracker Get(Map map)
        {
            MapComponent_SacrificeTracker mapComponent_SacrificeTracker = map.components.OfType<MapComponent_SacrificeTracker>().FirstOrDefault<MapComponent_SacrificeTracker>();
            if (mapComponent_SacrificeTracker == null)
            {
                mapComponent_SacrificeTracker = new MapComponent_SacrificeTracker(map);
                map.components.Add(mapComponent_SacrificeTracker);
            }
            return mapComponent_SacrificeTracker;
        }

        public MapComponent_SacrificeTracker(Map map) : base(map)
        {
            this.map = map;
        }
        
        public void ClearSacrificeVariables()
        {
            lastUsedHerdstone.tempSacrifice = null;
            lastResult = HerdUtility.SacrificeResult.none; // Default
            lastSacrificeType = HerdUtility.SacrificeType.none;
            lastUsedHerdstone = null; //Default
            lastSideEffect = null;
            lastLocation = IntVec3.Invalid;
            wasDoubleTheFun = false;
            lastDoubleSideEffect = null;
            lastRelation = null;
            lastSacrificeName = "";
            ASMwasPet = false;
            ASMwasBonded = false;
            ASMwasExcMaster = false;
            HSMwasFamily = false;
        }

        #endregion Setup

        #region Reports
        public string GenerateFailureString()
        {
            StringBuilder s = new StringBuilder();
            int ran = Rand.Range(1, 40);
            string message = "RH_TET_Beastmen_SacrificeFailMessage".Translate() + ran.ToString();
            string messageObject = message.Translate(new object[]
            {
                    lastUsedHerdstone.SacrificeData.Executioner
            });
            s.Append(messageObject);
            return s.ToString();
        }
        public string GenerateConversionFailureString()
        {
            StringBuilder s = new StringBuilder();
            int ran = Rand.Range(1, 40);
            string message = "RH_TET_Beastmen_SacrificeFailMessage".Translate() + ran.ToString();
            string messageObject = message.Translate(new object[]
            {
                    lastUsedHerdstone.ConversionData.Executioner
            });
            s.Append(messageObject);
            return s.ToString();
        }

        public void GenerateSacrificeMessage(Building_MassiveHerdstone herdstone)
        {
            StringBuilder s = new StringBuilder();
            string labelToTranslate = "Error";
            string textLabel = "Error";
            LetterDef letterDef = LetterDefOf.ThreatSmall;

            //The sacrifice is human
            if (lastUsedHerdstone.SacrificeData.Type == HerdUtility.SacrificeType.humanlike)
            {

                s.Append("RH_TET_Beastmen_SacrificeIntro".Translate(lastUsedHerdstone.SacrificeData.God.Label));
                
                //Was the executioner a family member?
                if (HSMwasFamily)
                {
                    if (lastUsedHerdstone.SacrificeData.Executioner == null) Log.Error("Executioner null");
                    if (lastRelation == null) Log.Error("Null relation");
                    if (lastSacrificeName == null) Log.Error("Null name");
                    string familyString = "RH_TET_Beastmen_HumanlikeSacrificeWasFamily".Translate((new object[]
                    {
                            lastUsedHerdstone.SacrificeData.Executioner.LabelShort,
                            lastUsedHerdstone.SacrificeData.Executioner.gender.GetPossessive(),
                            lastRelation.label,
                            lastSacrificeName
                    }));
                    s.Append(familyString + ". ");
                }

                if (lastResult != HerdUtility.SacrificeResult.success)
                    s.Append(GenerateFailureString());

                if ((int)lastResult <= 3 && (int)lastResult > 1)
                {
                    s.Append(" " + lastSideEffect.letterText);
                    if (wasDoubleTheFun)
                    {
                        s.Append(" " + lastDoubleSideEffect.letterText);
                    }
                }
                if (lastResult == HerdUtility.SacrificeResult.mixedsuccess)
                {
                    List<string> buts = new List<string> {
                    "RH_TET_Beastmen_butsOne".Translate(),
                    "RH_TET_Beastmen_butsTwo".Translate(),
                    "RH_TET_Beastmen_butsThree".Translate(),
                    "RH_TET_Beastmen_butsFour".Translate()
                };
                    s.Append(". " + buts.RandomElement<string>() + ", ");
                }

                s.Append(" " + "RH_TET_Beastmen_ritualWas".Translate());

                switch (lastResult)
                {
                    case HerdUtility.SacrificeResult.success:
                        s.Append("RH_TET_Beastmen_ritualSuccess".Translate());
                        letterDef = BeastmenDefOf.RH_TET_Beastmen_KhorneMessage;
                        break;
                    case HerdUtility.SacrificeResult.mixedsuccess:
                        letterDef = BeastmenDefOf.RH_TET_Beastmen_KhorneMessage;
                        s.Append("RH_TET_Beastmen_ritualMixedSuccess".Translate());
                        break;
                    case HerdUtility.SacrificeResult.failure:
                        s.Append("RH_TET_Beastmen_ritualFailure".Translate());
                        break;
                    case HerdUtility.SacrificeResult.criticalfailure:
                        s.Append("RH_TET_Beastmen_ritualCompleteFailure".Translate());
                        break;
                    case HerdUtility.SacrificeResult.none:
                        s.Append("this should never happen");
                        break;
                }
                labelToTranslate = "RH_TET_Beastmen_SacrificeLabel" + lastResult.ToString();
            }
            else if (lastUsedHerdstone.SacrificeData.Type == HerdUtility.SacrificeType.animal)
            {
                s.Append("RH_TET_Beastmen_SacrificeIntroAnimal".Translate(lastUsedHerdstone.SacrificeData.God.Label));
                StringBuilder sb = new StringBuilder(herdstone.newPawnEventBonus + herdstone.sarsenBonus);
                if (herdstone.sarsenBonus > 0)
                    sb.Append(" (" + "RH_TET_Beastmen_SarsenInfo".Translate() + ")");
                s.Append("RH_TET_Beastmen_AnimalSacrificePleased".Translate(sb.ToString()));
                
                goto LetterStack;
            }
            else
            {
                s.Append(" " + "RH_TET_Beastmen_OfferingReason".Translate() + ".");
                s.Append(" " + "RH_TET_Beastmen_ReadilyReceived".Translate() + ".");
                textLabel = "RH_TET_Beastmen_OfferingLabel".Translate();

                goto LetterStack;
            }

        LetterStack:
            Messages.Message(s.ToString(), MessageTypeDefOf.RejectInput);
        }

        public void GenerateConversionMessage(Building_MassiveHerdstone herdstone)
        {
            StringBuilder s = new StringBuilder();
            string labelToTranslate = "Error";
            string textLabel = "Error";
            LetterDef letterDef = LetterDefOf.ThreatSmall;

                s.Append("RH_TET_Beastmen_ConversionIntro".Translate());

                //Was the executioner a family member?
                if (HSMwasFamily)
                {
                    if (lastUsedHerdstone.ConversionData.Executioner == null) Log.Error("Shaman null");
                    if (lastRelation == null) Log.Error("Null relation");
                    if (lastSacrificeName == null) Log.Error("Null name");
                    string familyString = "RH_TET_Beastmen_HumanlikeConversionWasFamily".Translate((new object[]
                    {
                            lastUsedHerdstone.ConversionData.Executioner.LabelShort,
                            lastUsedHerdstone.ConversionData.Executioner.gender.GetPossessive(),
                            lastRelation.label,
                            lastSacrificeName
                    }));
                    s.Append(familyString + ". ");
                }

                if (lastResult != HerdUtility.SacrificeResult.success)
                    s.Append(GenerateConversionFailureString());

                if ((int)lastResult <= 3 && (int)lastResult > 1)
                {
                    s.Append(" " + lastSideEffect.letterText);
                    if (wasDoubleTheFun)
                    {
                        s.Append(" " + lastDoubleSideEffect.letterText);
                    }
                }
                if (lastResult == HerdUtility.SacrificeResult.mixedsuccess)
                {
                    List<string> buts = new List<string> {
                    "RH_TET_Beastmen_butsOne".Translate(),
                    "RH_TET_Beastmen_butsTwo".Translate(),
                    "RH_TET_Beastmen_butsThree".Translate(),
                    "RH_TET_Beastmen_butsFour".Translate()
                };
                    s.Append(". " + buts.RandomElement<string>() + ", ");
                }

                s.Append(" " + "RH_TET_Beastmen_ritualWas".Translate());

                switch (lastResult)
                {
                    case HerdUtility.SacrificeResult.success:
                        s.Append("RH_TET_Beastmen_ritualSuccess".Translate());
                        letterDef = BeastmenDefOf.RH_TET_Beastmen_KhorneMessage;
                        break;
                    case HerdUtility.SacrificeResult.mixedsuccess:
                        letterDef = BeastmenDefOf.RH_TET_Beastmen_KhorneMessage;
                        s.Append("RH_TET_Beastmen_ritualMixedSuccess".Translate());
                        break;
                    case HerdUtility.SacrificeResult.failure:
                        s.Append("RH_TET_Beastmen_ritualFailure".Translate());
                        break;
                    case HerdUtility.SacrificeResult.criticalfailure:
                        s.Append("RH_TET_Beastmen_ritualCompleteFailure".Translate());
                        break;
                    case HerdUtility.SacrificeResult.none:
                        s.Append("this should never happen");
                        break;
                }
                labelToTranslate = "RH_TET_Beastmen_SacrificeLabel" + lastResult.ToString();


        LetterStack:
            Messages.Message(s.ToString(), MessageTypeDefOf.RejectInput);
        }
        #endregion Reports

        public override void ExposeData()
        {
            //Unspeakable Oath Spell
            Scribe_Values.Look<int>(ref this.ticksUntilResurrection, "ticksUntilResurrection", -999);
            Scribe_Collections.Look<Pawn>(ref this.unspeakableOathPawns, "unspeakableOathPawns", LookMode.Reference);

            //Defend the Brood Spell
            Scribe_Collections.Look<Pawn>(ref this.defendTheBroodPawns, "defendTheBroodPawns", LookMode.Reference, new object[0]);

            //Sacrifice Variables
            Scribe_Values.Look<string>(ref this.lastSacrificeName, "lastSacrificeName", "None", false);
            Scribe_Values.Look<bool>(ref this.wasDoubleTheFun, "wasDoubleTheFun", false, false);
            Scribe_Values.Look<bool>(ref this.ASMwasPet, "ASMwasPet", false, false);
            Scribe_Values.Look<bool>(ref this.ASMwasBonded, "ASMwasBonded", false, false);
            Scribe_Values.Look<bool>(ref this.ASMwasExcMaster, "ASMwasExcMaster", false, false);
            Scribe_Values.Look<bool>(ref this.HSMwasFamily, "HSMwasFamily", false, false);

            Scribe_Values.Look<IntVec3>(ref this.lastLocation, "lastLocation", IntVec3.Invalid, false);
            Scribe_Defs.Look<PawnRelationDef>(ref this.lastRelation, "lastRelation");
            Scribe_Defs.Look<IncidentDef>(ref this.lastDoubleSideEffect, "lastDoubleSideEffect");
            Scribe_Defs.Look<IncidentDef>(ref this.lastSideEffect, "lastSideEffect");
            Scribe_References.Look<Building_MassiveHerdstone>(ref this.lastUsedHerdstone, "lastUsedHerdstone", false);
            Scribe_Values.Look<HerdUtility.SacrificeResult>(ref this.lastResult, "lastResult", HerdUtility.SacrificeResult.none, false);
            Scribe_Values.Look<HerdUtility.SacrificeType>(ref this.lastSacrificeType, "lastSacrificeType", HerdUtility.SacrificeType.none, false);
            Scribe_Values.Look<HerdUtility.OfferingSize>(ref this.lastOfferingSize, "lastOfferingSize", HerdUtility.OfferingSize.none, false);
            base.ExposeData();

        }
    }
}