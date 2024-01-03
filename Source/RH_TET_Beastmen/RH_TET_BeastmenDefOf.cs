using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Beastmen
{
    [DefOf]
    public static class BeastmenDefOf
    {
        // =============== Letters ===============
        public static LetterDef RH_TET_Beastmen_KhorneMessage;
        public static LetterDef RH_TET_Beastmen_NurgleMessage;
        public static LetterDef RH_TET_Beastmen_SlaaneshMessage;
        public static LetterDef RH_TET_Beastmen_TzeentchMessage;

        // =============== Weapons ===============
        public static ThingDef RH_TET_Beastmen_MeleeWeapon_GreatAxe;
        public static ThingDef RH_TET_Beastmen_RangedWeapon_UngorBow;
        public static ThingDef RH_TET_Beastmen_MeleeWeapon_Sword;

        // =============== Armor ===============
        public static ThingDef RH_TET_Beastmen_Apparel_GorArmor;
        public static ThingDef RH_TET_Beastmen_Apparel_WargorArmor;

        // =============== Pawnkinds ===============
        public static PawnKindDef RH_TET_Beastmen_Khornegor;
        public static PawnKindDef RH_TET_Beastmen_Pestigor;
        public static PawnKindDef RH_TET_Beastmen_Tzaangor;
        public static PawnKindDef RH_TET_Beastmen_Slaangor;
        public static PawnKindDef RH_TET_Beastmen_GorColony;
        public static PawnKindDef RH_TET_Beastmen_GorTribe;
        public static PawnKindDef RH_TET_Beastmen_UngorColony;
        public static PawnKindDef RH_TET_Beastmen_UngorTribe;
        public static PawnKindDef RH_TET_Beastmen_BrayScenario;
        public static PawnKindDef RH_TET_Beastmen_BullgorBeastmenPlayer;
        public static PawnKindDef RH_TET_Beastmen_GorBeastmenPlayer;
        public static PawnKindDef RH_TET_Beastmen_UngorBeastmenPlayer;
        public static PawnKindDef RH_TET_Beastmen_WildGor;
        public static PawnKindDef RH_TET_Beastmen_WildUngor;
        public static PawnKindDef RH_TET_Beastmen_UngorExile;
        public static PawnKindDef RH_TET_Beastmen_GorExile;
        public static PawnKindDef RH_TET_Beastmen_RaiderMelee;
        public static PawnKindDef RH_TET_Beastmen_Raider;
        public static PawnKindDef RH_TET_Beastmen_Ravager;
        public static PawnKindDef RH_TET_Beastmen_Bestigor;
        public static PawnKindDef RH_TET_Beastmen_Wargor;
        public static PawnKindDef RH_TET_Beastmen_Beastlord;
        public static PawnKindDef RH_TET_Beastmen_GreatBrayShaman;
        public static PawnKindDef RH_TET_Beastmen_BrayShaman;
        public static PawnKindDef RH_TET_Beastmen_Gorebull;
        public static PawnKindDef RH_TET_Beastmen_Minotaur;
        public static PawnKindDef RH_TET_Beastmen_BullgorColony;
        public static PawnKindDef RH_TET_Beastmen_BullgorTribe;
        public static PawnKindDef RH_TET_Beastmen_BullgorExile;
        public static PawnKindDef RH_TET_Beastmen_WildBullgor;
        public static PawnKindDef RH_TET_Beastmen_ClovenCygorFaction; 
        public static PawnKindDef RH_TET_Beastmen_WildCygor;
        public static PawnKindDef RH_TET_Beastmen_CygorBeastmenPlayer;
        public static PawnKindDef RH_TET_ChaosSpawnAnimal;
        public static PawnKindDef RH_TET_RazorgorAnimal;
        public static PawnKindDef RH_TET_ChaosSpawnAncientAnimal;
        public static PawnKindDef RH_TET_TuskgorAnimal;
        public static PawnKindDef RH_TET_ChaosWarhoundAnimal;
        public static PawnKindDef RH_TET_JabberslytheAnimal;
        public static PawnKindDef RH_TET_Beastmen_BeastmenPawnHarpy;

        // =============== Offering Related Defs ===============
        public static JobDef RH_TET_Beastmen_MakeOffering;
        public static JobDef RH_TET_Beastmen_WaitThroughOffering;
        public static JobDef RH_TET_Beastmen_CarryToMassiveHerdstone;

        // =============== Sacrifice Related Defs ===============
        public static JobDef RH_TET_Beastmen_HoldSacrifice;
        public static JobDef RH_TET_Beastmen_HoldConversion;
        public static JobDef RH_TET_Beastmen_AttendSacrifice;
        public static JobDef RH_TET_Beastmen_ReflectOnResult;
        public static JobDef RH_TET_Beastmen_AwaitDeath;

        // =============== Incidents ===============
        public static IncidentDef RH_TET_Beastmen_RefugeesChased;
        public static IncidentDef RH_TET_Beastmen_MonsterJoin;
        public static IncidentDef RH_TET_Beastmen_WandererJoin;
        public static IncidentDef RH_TET_Beastmen_FinalDestruction;

        // =============== THOUGHTS ===============
        public static ThoughtDef RH_TET_Beastmen_OtherPrisonerWasSacrificed;
        public static ThoughtDef RH_TET_Beastmen_ExecutedFamily;
        public static ThoughtDef RH_TET_Beastmen_ExecutedPet;
        public static ThoughtDef RH_TET_Beastmen_SacrificedFamily;
        public static ThoughtDef RH_TET_Beastmen_SacrificedPet;
        public static ThoughtDef RH_TET_Beastmen_SacrificedFriend;
        public static ThoughtDef RH_TET_Beastmen_SacrificedRival;
        public static ThoughtDef RH_TET_Beastmen_ConvertedFamily;
        public static ThoughtDef RH_TET_Beastmen_ConvertedFriend;
        public static ThoughtDef RH_TET_Beastmen_ConvertedRival; 
        public static ThoughtDef RH_TET_Beastmen_AttendedSuccessfulConversion;
        public static ThoughtDef RH_TET_Beastmen_AttendedFailedConversion;
        public static ThoughtDef RH_TET_Beastmen_FinalDestructionDefeat;
        public static ThoughtDef RH_TET_Beastmen_FightingFinalBattle;
        public static ThoughtDef RH_TET_Beastmen_DestroyedFinalOrder;
        public static ThoughtDef RH_TET_Beastmen_GorAteHumanlikeMeatAsIngredient;

        // =============== Needs =============== 
        public static NeedDef RH_TET_Beastmen_NeedViolence;

        // =============== Sacrifice =============== 
        public static ThoughtDef RH_TET_Beastmen_AttendedSuccessfulSacrifice;
        public static ThoughtDef RH_TET_Beastmen_AttendedFailedSacrifice;

        // =============== World Stuff =============== 
        public static SitePartDef RH_TET_Beastmen_Homestead;
        public static SitePartDef RH_TET_Beastmen_Caravan;

        // =============== SOUNDS =============== 
        public static SoundDef RH_TET_Beastmen_Braying;
        public static SoundDef RH_TET_Beastmen_Victory;

        // =============== BUILDINGS ===============
        public static ThingDef RH_TET_Beastmen_GorStone;
        public static ThingDef RH_TET_Beastmen_GorStoneM;
        public static ThingDef RH_TET_Beastmen_MassiveHerdstone;
        public static ThingDef RH_TET_Beastmen_MassiveHerdstone_UpgradeOne;
        public static ThingDef RH_TET_Beastmen_MassiveHerdstone_UpgradeTwo;
        public static ThingDef RH_TET_Beastmen_MassiveHerdstone_UpgradeThree;
        public static ThingDef RH_TET_Beastmen_Drum; 
        public static ThingDef RH_TET_Beastmen_SarsenLintel_Psychic;
        public static ThingDef RH_TET_Beastmen_Sarsen_Gifter;
        public static ThingDef RH_TET_Beastmen_SarsenDouble_Conversion;
        public static ThingDef RH_TET_Beastmen_Sarsen_Tutor;
        public static ThingDef RH_TET_Beastmen_Sarsen_Healer;
        public static ThingDef RH_TET_Beastmen_SarsenLintel_Pulse;
        public static ThingDef RH_TET_Beastmen_SarsenLintel_Gas;
        public static ThingDef RH_TET_Beastmen_SarsenLintel_Beacon;

        // =============== Pit Fight Related ===============
        public static DutyDef RH_TET_Beastmen_WatchPitFight;
        public static InteractionDef RH_TET_Beastmen_UrgeOn;
        public static JobDef RH_TET_Beastmen_JoinPitFightJob;
        public static JobDef RH_TET_Beastmen_WatchPitFightJob;
        public static JobDef RH_TET_Beastmen_UrgeOnJob;
        public static MentalStateDef RH_TET_Beastmen_PitFighter;
        public static ThingDef RH_TET_Beastmen_PitFightingSpot;
        public static ThoughtDef RH_TET_Beastmen_PitFightWinner;
        public static ThoughtDef RH_TET_Beastmen_PitFightLoser;
        public static ThoughtDef RH_TET_Beastmen_WatchedPitFight;

        // =============== Getting Soft ===============
        public static GettingSoftDef RH_TET_Beastmen_ExtremelyLow;
        public static GettingSoftDef RH_TET_Beastmen_VeryLow;
        public static GettingSoftDef RH_TET_Beastmen_Low;
        public static GettingSoftDef RH_TET_Beastmen_Moderate;
        public static GettingSoftDef RH_TET_Beastmen_High;
        public static GettingSoftDef RH_TET_Beastmen_SkyHigh;

        // =============== Biomes ===============
        public static ElementSpawnDef RH_TET_Beastmen_SpawnGreatOakDead;

        // =============== Motes ===============
        public static ThingDef RH_TET_Beastmen_MoteAxe;

        // =============== Cauldron/Food ===============
        public static ThingDef RH_TET_Beastmen_SlopMeal;
        public static ThingDef RH_TET_Beastmen_SlopBucket;
        public static ThingDef RH_TET_Beastmen_CauldronSlopDispenser;

        // =============== Recipes ===============
        public static RecipeDef RH_TET_Beastmen_SpikeHead;

        // =============== Faction ===============
        public static FactionDef RH_TET_Beastmen_GorFaction;
        public static FactionDef RH_TET_EmpireOfMan;

        // =============== Tale ===============
        public static TaleDef RH_TET_Beastmen_ConvertedPrisoner;
        public static TaleDef RH_TET_Beastmen_FinalDestructionVictory;

        // =============== Maps ===============
        public static MapGeneratorDef RH_TET_Beastmen_EmptyMap;
        public static MapGenDef RH_TET_Beastmen_FinalDestructionMap;
    }
}