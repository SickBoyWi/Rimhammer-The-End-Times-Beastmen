using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld.Planet;
using RimWorld;
using TheEndTimes_Magic;

namespace TheEndTimes_Beastmen
{
    public partial class Building_MassiveHerdstone : Building, IBillGiver, IThingHolder
    {
        #region IBillGiver

        public IEnumerable<IntVec3> CellsAround => GenRadial.RadialCellsAround(Position, 5, true);
        public BillStack BillStack { get; }
        public IEnumerable<IntVec3> IngredientStackCells => GenAdj.CellsOccupiedBy(this);

        #endregion IBillGiver

        #region Variables

        //Universal Variables
        public enum State { notinuse = 0, sacrificing, offering, converting };
        public enum Function { Level1 = 0, Level2 = 1, Level3 = 2, Level4 = 3 };
        public Function currentFunction = Function.Level1;
        private State currentState = State.notinuse;
        private bool destroyedFlag = false;
        public string HerdstoneName = "Massive Herdstone";

        //Offering Related Variables
        public enum OfferingState { off = 0, started, offering, finished };
        public OfferingState currentOfferingState = OfferingState.off;
        public Pawn tempOfferer;
        private Pawn offerer;
        public DarkGod currentOfferingGod;
        public DarkGod tempCurrentOfferingGod;
        public HerdUtility.SacrificeType tempOfferingType = HerdUtility.SacrificeType.none;
        public HerdUtility.OfferingSize tempOfferingSize = HerdUtility.OfferingSize.none;
        private List<ThingCount> tempDeterminedOfferings = new List<ThingCount>();
        public List<ThingCount> determinedOfferings = new List<ThingCount>();
        private RecipeDef billRecipe = null;
        public int newPawnEventBonus = 0; // renown
        public int sarsenBonus = 0;

        public Pawn shaman = null;
        public Pawn tempShaman = null;

        //Sacrifice Related Variables
        public enum SacrificeState { off = 0, started, gathering, sacrificing, finishing, finished };
        public SacrificeState currentSacrificeState = SacrificeState.off;

        private Bill_Sacrifice sacrificeData = null;
        public Bill_Sacrifice SacrificeData => sacrificeData;


        private Bill_Convert conversionData = null;
        public Bill_Convert ConversionData => conversionData;

        // Events
        private int nextRaidWeakTick = -1;
        private int nextRaidStrongTick = -1;
        private int nextFreePawnTick = -1;
        private int nextChasedRefugeesTick = -1;
        private int nextFreeAnimalTick = -1;
        private int WEAK_RAID_TICKS_MIN = 60000 * 2; // 1 day is 60000 ticks.
        private int STRONG_RAID_TICKS_MIN = 65000 * 6; 
        private int FREE_PAWN_TICKS_MIN = 63000 * 4; 
        private int REFUGEES_CHASED_TICKS_MIN = 61000 * 5;
        private int FREE_ANIMALS_TICKS_MIN = 64000 * 3;

        public Pawn tempSacrifice = null;
        public Pawn tempExecutioner = null;
        public DarkGod tempCurrentSacrificeGod;
        public int sacrificedAnimalsCount = 0;
        public int sacrificedHumansCount = 0;

        // End game
        public bool destructionQuestExposed = false;
        public bool finalDestructionQuestExposed = false;

        //Misc Event Variables
        private Function lastFunction = Function.Level1;

        //private StorageSettings storageSettings;
        protected ThingOwner innerContainer;

        #endregion Variables

        public Building_MassiveHerdstone()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
            BillStack = new BillStack(this);
        }

        #region Storage

        public static Building_MassiveHerdstone FindMassiveHerdstoneFor(Corpse victim, Pawn traveler, bool requireFuel = true, bool ignoreOtherReservations = false)
        {
            IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
                                               where typeof(Building_MassiveHerdstone).IsAssignableFrom(def.thingClass)
                                               select def;
            foreach (ThingDef current in enumerable)
            {
                Building_MassiveHerdstone building_MassiveHerdstone = (Building_MassiveHerdstone)GenClosest.ClosestThingReachable(
                        victim.Position, 
                        victim.Map, 
                        ThingRequest.ForDef(current), 
                        PathEndMode.InteractionCell, 
                        TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false), 
                        9999f, 
                        delegate (Thing x)
                            {
                                bool canReserve;

                                Pawn traveler2 = traveler;
                                LocalTargetInfo target = x;
                                bool ignoreOtherReservations2 = ignoreOtherReservations;
                                canReserve = traveler2.CanReserve(target, 1, -1, null, ignoreOtherReservations2);
                                bool hasFuel = true;
                                if (requireFuel) hasFuel = ((Building_MassiveHerdstone)x).CurrentlyUsableForBills();
                                return hasFuel && canReserve;

                            }, null, 0, -1, false, RegionType.Set_Passable, false);
                if (building_MassiveHerdstone != null)
                {
                    return building_MassiveHerdstone;
                }
            }
            return null;
        }

        public bool Accepts(Thing thing)
        {
            if (!this.innerContainer.CanAcceptAnyOf(thing, true))
            {
                return false;
            }
            return true;
        }

        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }

            if (!RewardUtility.PleasesPapaNurgle(((Corpse)thing).GetRotStage()))
            {
                DarkGod g = DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Khorne);
                g.ConsumeBodyOffering(thing);
                RewardUtility.ProcessBurnedPawn(g, this);
            }
            else
            {
                DarkGod g = DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Nurgle);
                g.ConsumeBodyOffering(thing);
                RewardUtility.ProcessBurnedDessicatedRottenPawn(g, this);
            }
            
            return true;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        #endregion Storage

        #region Bools
        private bool IsCongregating() => IsOffering() || IsSacrificing();
        private bool IsOffering() =>
            currentState == State.offering || (currentOfferingState != OfferingState.finished && currentOfferingState != OfferingState.off);
        private bool IsSacrificing() =>
            currentState == State.sacrificing || (currentSacrificeState != SacrificeState.finished && currentSacrificeState != SacrificeState.off);
        private bool IsConverting() =>
            currentState == State.converting || (currentSacrificeState != SacrificeState.finished && currentSacrificeState != SacrificeState.off);

        private static bool RejectMessage(string s, Pawn pawn = null)
        {
            Messages.Message(s, TargetInfo.Invalid, MessageTypeDefOf.RejectInput);
            if (pawn != null) pawn = null;
            return false;
        }
        #endregion Bools

        #region State
        public void ChangeState(State type)
        {
            if (type == State.notinuse)
            {
                currentState = type;
                currentSacrificeState = SacrificeState.off;
                currentOfferingState = OfferingState.off;
                availableAttendees = null;
            }
            else Log.Error("Changed default state of Herdstone this should never happen.");
            ReportState();
        }

        public void ChangeState(State type, SacrificeState sacrificeState)
        {
            currentState = type;
            currentSacrificeState = sacrificeState;
            ReportState();
        }

        public void ChangeState(State type, OfferingState offeringState)
        {
            currentState = type;
            currentOfferingState = offeringState;
            ReportState();
        }

        private void ReportState()
        {
            var s = new StringBuilder();
            s.Append("===================");
            s.AppendLine("Herdstone States Changed");
            s.AppendLine("===================");
            s.AppendLine("State: " + currentState);
            s.AppendLine("Offering: " + currentOfferingState);
            s.AppendLine("Sacrifice: " + currentSacrificeState);
            s.AppendLine("===================");
            TheEndTimes_Beastmen.Utility.DebugReport(s.ToString());
        }
        #endregion State

        #region Bed-Like
        public static int LyingSlotsCount { get; } = 1;

        public bool AnyUnoccupiedLyingSlot
        {
            get
            {
                for (var i = 0; i < LyingSlotsCount; i++)
                    if (GetCurOccupant() == null)
                        return true;
                return false;
            }
        }

        private Pawn GetCurOccupant()
        {
            var sleepingSlotPos = GetLyingSlotPos();
            var list = Map.thingGrid.ThingsListAt(sleepingSlotPos);
            if (list.NullOrEmpty()) return null;
            foreach (var t in list)
            {
                if (!(t is Pawn pawn)) continue;
                if (pawn.CurJob == null) continue;
                if (pawn.jobs.posture != PawnPosture.Standing)
                {
                    return pawn;
                }
            }
            return null;
        }

        public IntVec3 GetLyingSlotPos()
        {
            var index = 1;
            var cellRect = this.OccupiedRect();
            if (Rotation == Rot4.North)
            {
                return new IntVec3(cellRect.minX + index, Position.y, cellRect.minZ);
            }
            if (Rotation == Rot4.East)
            {
                return new IntVec3(cellRect.minX, Position.y, cellRect.maxZ - index);
            }
            return Rotation == Rot4.South ? new IntVec3(cellRect.minX + index, Position.y, cellRect.maxZ) : new IntVec3(cellRect.maxX, Position.y, cellRect.maxZ - index);
        }

        #endregion Bed-Like

        #region Spawn
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (HerdstoneName == null) HerdstoneName = "Massive Herdstone";

            DarkGodTracker.Get.orGenerate();
            switch (def.defName)
            {
                case "RH_TET_Beastmen_MassiveHerdstone":
                    if (Map.IsPlayerHome && !HerdUtility.destructionQuestExposedYet(HerdUtility.getAllHerdstones()))
                    {
                        // Send message so they know about the quest, and the god rewards.
                        LetterDef letterDef = BeastmenDefOf.RH_TET_Beastmen_KhorneMessage;
                        Find.LetterStack.ReceiveLetter("RH_TET_Beastmen_Destruction".Translate(), "RH_TET_Beastmen_DestructionDesc".Translate(), letterDef, new TargetInfo(this.InteractionCell, this.Map));

                        RH_TET_MagicDefOf.RH_TET_Magic_SoundBrayScream.PlayOneShotOnCamera(null);

                        this.destructionQuestExposed = true;
                    }
                    break;
                case "RH_TET_Beastmen_MassiveHerdstone_UpgradeOne":
                    currentFunction = Function.Level2;
                    break;
                case "RH_TET_Beastmen_MassiveHerdstone_UpgradeTwo":
                    currentFunction = Function.Level3;
                    break;
                case "RH_TET_Beastmen_MassiveHerdstone_UpgradeThree":
                    currentFunction = Function.Level4;
                    break;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<string>(ref HerdstoneName, "RoomName");
            Scribe_Deep.Look<Bill_Sacrifice>(ref sacrificeData, "sacrificeData", new object[0]);
            Scribe_Deep.Look<Bill_Convert>(ref conversionData, "conversionData", new object[0]);
            Scribe_References.Look<DarkGod>(ref tempCurrentSacrificeGod, "tempCurrentSacrificeGod");
            Scribe_References.Look<Pawn>(ref tempSacrifice, "tempSacrifice");
            Scribe_References.Look<Pawn>(ref tempExecutioner, "tempExecutioner");
            Scribe_Values.Look<State>(ref currentState, "currentState");
            Scribe_Values.Look<SacrificeState>(ref currentSacrificeState, "currentSacrificeState");
            Scribe_Values.Look<Function>(ref lastFunction, "lastFunction");
            Scribe_Values.Look<Function>(ref currentFunction, "currentFunction");
            Scribe_Values.Look<int>(ref sacrificedAnimalsCount, "sacrificedAnimalsCount");
            Scribe_Values.Look<int>(ref sacrificedHumansCount, "sacrificedHumansCount");
            Scribe_Values.Look<int>(ref this.newPawnEventBonus, "newPawnEventBonus");
            Scribe_Values.Look<int>(ref this.sarsenBonus, "sarsenBonus");
            Scribe_Values.Look<int>(ref this.nextRaidWeakTick, "nextRaidWeakTick");
            Scribe_Values.Look<int>(ref this.nextRaidStrongTick, "nextRaidStrongTick");
            Scribe_Values.Look<int>(ref this.nextFreePawnTick, "nextFreePawnTick");
            Scribe_Values.Look<int>(ref this.nextFreeAnimalTick, "nextFreeAnimalTick");
            Scribe_Values.Look<int>(ref this.nextChasedRefugeesTick, "nextChasedRefugeesTick");
            Scribe_Values.Look<bool>(ref this.destructionQuestExposed, "destructionQuestExposed", false);
            Scribe_Values.Look<bool>(ref this.finalDestructionQuestExposed, "finalDestructionQuestExposed", false);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // block further ticker work
            destroyedFlag = true;
            ITab_HerdstoneSacrificeCardUtil.Tab = ITab_HerdstoneSacrificeCardUtil.SacrificeCardTab.Offering;
            base.Destroy(mode);
        }
        #endregion Spawn

        #region Ticker

        protected override void Tick()
        {
            if (destroyedFlag)
                return;

            if (!Spawned) return;

            // Don't forget the base work
            base.Tick();

            if (!this.Map.IsPlayerHome)
                return;

            if (Faction.OfPlayerSilentFail == null || (!(Faction.OfPlayerSilentFail.def.defName.Equals("RH_TET_Beastmen_BrayPlayerFaction") || Faction.OfPlayerSilentFail.def.defName.Equals("RH_TET_Beastmen_BeastmenPlayerFaction")) && !RH_TET_BeastmenMod.anyOneCanBeastActive))
                return;

            MassiveHerdstoneTick();
            NewEventsTick();
        }

        private void NewEventsTick()
        {
            if (this.destroyedFlag)
                return;

            if (nextRaidWeakTick <= 0)
            {
                nextRaidWeakTick = Find.TickManager.TicksGame + (int)(WEAK_RAID_TICKS_MIN * Settings.GetHerdstoneRaidFrequency());
                nextRaidStrongTick = Find.TickManager.TicksGame + (int)(STRONG_RAID_TICKS_MIN * Settings.GetHerdstoneRaidFrequency());
                nextFreePawnTick = Find.TickManager.TicksGame;
                nextChasedRefugeesTick = Find.TickManager.TicksGame + (int)(REFUGEES_CHASED_TICKS_MIN * Settings.GetHerdstoneRaidFrequency());
                nextFreeAnimalTick = Find.TickManager.TicksGame + (int)(FREE_ANIMALS_TICKS_MIN * Settings.GetHerdstoneRaidFrequency());
            }

            if (!Settings.UseHerdstoneEvents)
                return;

            if (nextRaidWeakTick < (Find.TickManager.TicksGame + (newPawnEventBonus * 150) + (sarsenBonus/2)))
            {
                nextRaidWeakTick = Find.TickManager.TicksGame + (int)(WEAK_RAID_TICKS_MIN * Settings.GetHerdstoneRaidFrequency());

                EventMakerUtility.SpawnRaidEvent(this, new FloatRange(.7f, .8f));
            }
            else if (nextRaidStrongTick < (Find.TickManager.TicksGame + (newPawnEventBonus * 50) + (sarsenBonus/3)))
            {
                nextRaidStrongTick = Find.TickManager.TicksGame + (int)(STRONG_RAID_TICKS_MIN * Settings.GetHerdstoneRaidFrequency());

                EventMakerUtility.SpawnRaidEvent(this, new FloatRange(.9f, 1.1f));
            }
            else if (nextFreePawnTick < (Find.TickManager.TicksGame + (newPawnEventBonus * 200) + sarsenBonus))
            {
                nextFreePawnTick = Find.TickManager.TicksGame + (int)(FREE_PAWN_TICKS_MIN * Settings.GetHerdstoneRaidFrequency());

                EventMakerUtility.SpawnWandererJoin(this);
            }
            else if (nextFreeAnimalTick < (Find.TickManager.TicksGame + (newPawnEventBonus * 200) + (sarsenBonus / 2)))
            {
                nextFreeAnimalTick = Find.TickManager.TicksGame + (int)(FREE_ANIMALS_TICKS_MIN * Settings.GetHerdstoneRaidFrequency());

                EventMakerUtility.SpawnAnimalsJoin(this);
            }
            else if (nextChasedRefugeesTick < (Find.TickManager.TicksGame + (newPawnEventBonus * 50) + sarsenBonus))
            {
                nextChasedRefugeesTick = Find.TickManager.TicksGame + (int)(REFUGEES_CHASED_TICKS_MIN * Settings.GetHerdstoneRaidFrequency());

                EventMakerUtility.SpawnChasedRefugeeEvent(this);
            }
        }

        private void MassiveHerdstoneTick()
        {
            ConversionCheckTick();
            ConversionTick();

            // Auto upgrade if appropriate.
            if (currentFunction == Function.Level1)
            {
                if (DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Khorne).PlayerFavor >= 8F
                    && DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Nurgle).PlayerFavor >= 7F)
                {
                    TryUpgrade();
                }
            }
            else if (currentFunction == Function.Level2)
            {
                OfferingCheckTick();

                if (DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Slaanesh).PlayerFavor >= 36F
                    && DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Tzeentch).PlayerFavor >= 81F)
                {
                    TryUpgrade();
                }
            }
            else if (currentFunction == Function.Level3)
            {
                OfferingCheckTick();
                SacrificeCheckTick();

                if (this.sacrificedAnimalsCount >= 30)
                {
                    TryUpgrade();
                }
            }
            else if (currentFunction == Function.Level4)
            {
                OfferingCheckTick();
                SacrificeCheckTick();

                SacrificeTick();
            }
        }

        public void ConversionCheckTick()
        {
            if (!Spawned) return;
            if (ConversionData?.Executioner == null) return;
            if (currentState != State.converting) return;
            switch (currentSacrificeState)
            {
                case SacrificeState.started:
                case SacrificeState.gathering:
                case SacrificeState.sacrificing:
                    if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(ConversionData.Executioner))
                    {
                        String shaman = "RH_TET_Beastmen_Executioner".Translate();
                        if (currentState == State.converting) shaman = "RH_TET_Beastmen_Shaman".Translate();

                        HerdUtility.AbortCongregation(this, shaman + "RH_TET_Beastmen_IsUnavailable".Translate());
                        return;
                    }
                    else if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(ConversionData.Victim, true))
                    {
                        String victim = "RH_TET_Beastmen_Sacrifice".Translate();
                        if (currentState == State.converting) victim = "RH_TET_Beastmen_ConversionVictim".Translate();

                        HerdUtility.AbortCongregation(this, victim + "RH_TET_Beastmen_IsUnavailable".Translate());
                        return;
                    }
                    else
                    {
                        if (ConversionData.Executioner?.CurJob != null &&
                            ConversionData.Executioner.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_HoldConversion)
                        {
                            String shaman = "RH_TET_Beastmen_Executioner".Translate();
                            if (currentState == State.converting) shaman = "RH_TET_Beastmen_Shaman".Translate();

                            HerdUtility.AbortCongregation(this,
                                shaman + "RH_TET_Beastmen_IsUnavailable".Translate());
                            return;
                        }
                    }
                    //Log.Error("1");
                    if (availableAttendees == null)
                    {
                        GetSacrificeGroup();
                    }
                    return;

                case SacrificeState.finishing:
                    if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(ConversionData.Executioner))
                    {
                        String shaman = "RH_TET_Beastmen_Executioner".Translate();
                        if (currentState == State.converting) shaman = "RH_TET_Beastmen_Shaman".Translate();

                        HerdUtility.AbortCongregation(this, shaman + "RH_TET_Beastmen_IsUnavailable".Translate());
                        return;
                    }
                    if (ConversionData.Executioner?.CurJob != null &&
                        ConversionData.Executioner.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_ReflectOnResult) return;
                    GetSacrificeGroup();
                    return;

                case SacrificeState.finished:
                case SacrificeState.off:
                    ChangeState(State.notinuse);
                    return;
            }
        }

        public void SacrificeCheckTick()
        {
            if (!Spawned) return;
            if (SacrificeData?.Executioner == null) return;
            if (currentState != State.sacrificing) return;
            switch (currentSacrificeState)
            {
                case SacrificeState.started:
                case SacrificeState.gathering:
                case SacrificeState.sacrificing:
                    if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(SacrificeData.Executioner))
                    {
                        String shaman = "RH_TET_Beastmen_Executioner".Translate();
                        if (currentState == State.converting) shaman = "RH_TET_Beastmen_Shaman".Translate();

                        HerdUtility.AbortCongregation(this, shaman + "RH_TET_Beastmen_IsUnavailable".Translate());
                        return;
                    }
                    else if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(SacrificeData.Sacrifice, true))
                    {
                        String victim = "RH_TET_Beastmen_Sacrifice".Translate();
                        if (currentState == State.converting) victim = "RH_TET_Beastmen_ConversionVictim".Translate();

                        HerdUtility.AbortCongregation(this, victim + "RH_TET_Beastmen_IsUnavailable".Translate());
                        return;
                    }
                    else
                    {
                        if (SacrificeData.Executioner?.CurJob != null &&
                            SacrificeData.Executioner.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_HoldSacrifice)
                        {
                            String shaman = "RH_TET_Beastmen_Executioner".Translate();
                            if (currentState == State.converting) shaman = "RH_TET_Beastmen_Shaman".Translate();

                            HerdUtility.AbortCongregation(this,
                                shaman + "RH_TET_Beastmen_IsUnavailable".Translate());
                            return;
                        }
                    }
                    if (availableAttendees == null)
                    {
                        GetSacrificeGroup();
                    }
                    return;

                case SacrificeState.finishing:
                    if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(SacrificeData.Executioner))
                    {
                        String shaman = "RH_TET_Beastmen_Executioner".Translate();
                        if (currentState == State.converting) shaman = "RH_TET_Beastmen_Shaman".Translate();

                        HerdUtility.AbortCongregation(this, shaman + "RH_TET_Beastmen_IsUnavailable".Translate());
                        return;
                    }
                    if (SacrificeData.Executioner?.CurJob != null &&
                        SacrificeData.Executioner.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_ReflectOnResult) return;
                    GetSacrificeGroup();
                    return;

                case SacrificeState.finished:
                case SacrificeState.off:
                    ChangeState(State.notinuse);
                    return;
            }
        }
        public void OfferingCheckTick()
        {
            if (currentState != State.offering) return;
            switch (currentOfferingState)
            {
                case OfferingState.started:
                    if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(offerer))
                    {
                        HerdUtility.AbortCongregation(this, "RH_TET_Beastmen_Offerer".Translate() + "RH_TET_Beastmen_IsUnavailable".Translate());
                        return;
                    }
                    else if (offerer.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_MakeOffering)
                    {
                        TheEndTimes_Beastmen.Utility.DebugReport(offerer.CurJob.def.defName);
                        HerdUtility.AbortCongregation(this, "Offerer is not performing the task at hand.");
                        return;
                    }
                    return;
                case OfferingState.offering:
                    if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(offerer))
                    {
                        HerdUtility.AbortCongregation(this, "RH_TET_Beastmen_Offerer".Translate() + "RH_TET_Beastmen_IsUnavailable".Translate());
                        return;
                    }
                    else if (offerer.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_MakeOffering)
                    {
                        TheEndTimes_Beastmen.Utility.DebugReport(offerer.CurJob.def.defName);
                        HerdUtility.AbortCongregation(this, "Offerer is not performing the task at hand.");
                        return;
                    }
                    return;
                case OfferingState.finished:
                case OfferingState.off:
                    ChangeState(State.notinuse);
                    return;
            }
        }

        public void SacrificeTick()
        {
            if (currentState != State.sacrificing) return;
            switch (currentSacrificeState)
            {
                case SacrificeState.started:
                case SacrificeState.gathering:
                case SacrificeState.sacrificing:
                    if (TheEndTimes_Beastmen.Utility.IsActorAvailable(SacrificeData.Executioner))
                    {
                        if (TheEndTimes_Beastmen.Utility.IsActorAvailable(SacrificeData.Sacrifice, true))
                        {
                            if (SacrificeData.Executioner.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_HoldSacrifice)
                            {
                                HerdUtility.AbortCongregation(this, "RH_TET_Beastmen_Executioner".Translate() + "RH_TET_Beastmen_IsUnavailable".Translate());
                            }
                            return;
                        }
                        HerdUtility.AbortCongregation(this, "RH_TET_Beastmen_Sacrifice".Translate() + "RH_TET_Beastmen_IsUnavailable".Translate());
                        return;
                    }
                    HerdUtility.AbortCongregation(this, "RH_TET_Beastmen_Executioner".Translate() + "RH_TET_Beastmen_IsUnavailable".Translate());
                    return;

                case SacrificeState.finishing:
                    if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(SacrificeData.Executioner))
                    {
                        HerdUtility.AbortCongregation(this, "RH_TET_Beastmen_Executioner".Translate() + "RH_TET_Beastmen_IsUnavailable".Translate());
                    }
                    if (MapComponent_SacrificeTracker.Get(Map).lastSacrificeType == HerdUtility.SacrificeType.animal)
                    {
                        ChangeState(State.sacrificing, SacrificeState.finished);
                    }
                    return;

                case SacrificeState.finished:
                case SacrificeState.off:
                    ChangeState(State.notinuse);
                    return;
            }
        }

        public void ConversionTick()
        {
            if (currentState != State.converting) return;
            switch (currentSacrificeState)
            {
                case SacrificeState.started:
                case SacrificeState.gathering:
                case SacrificeState.sacrificing:
                    if (TheEndTimes_Beastmen.Utility.IsActorAvailable(ConversionData.Executioner))
                    {
                        if (TheEndTimes_Beastmen.Utility.IsActorAvailable(ConversionData.Victim, true))
                        {
                            if (ConversionData.Executioner.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_HoldConversion)
                            {
                                String shaman2 = "RH_TET_Beastmen_Executioner".Translate();
                                if (currentState == State.converting) shaman2 = "RH_TET_Beastmen_Shaman".Translate();

                                HerdUtility.AbortCongregation(this, shaman2 + "RH_TET_Beastmen_IsUnavailable".Translate());
                            }
                            return;
                        }

                        String victim = "RH_TET_Beastmen_Sacrifice".Translate();
                        if (currentState == State.converting) victim = "RH_TET_Beastmen_ConversionVictim".Translate();

                        HerdUtility.AbortCongregation(this, victim + "RH_TET_Beastmen_IsUnavailable".Translate());
                        return;
                    }
                    String shaman = "RH_TET_Beastmen_Executioner".Translate();
                    if (currentState == State.converting) shaman = "RH_TET_Beastmen_Shaman".Translate();

                    HerdUtility.AbortCongregation(this, shaman + "RH_TET_Beastmen_IsUnavailable".Translate());
                    return;

                case SacrificeState.finishing:
                    if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(ConversionData.Executioner))
                    {
                        String shaman3 = "RH_TET_Beastmen_Executioner".Translate();
                        if (currentState == State.converting) shaman3 = "RH_TET_Beastmen_Shaman".Translate();

                        HerdUtility.AbortCongregation(this, shaman3 + "RH_TET_Beastmen_IsUnavailable".Translate());
                    }
                    if (MapComponent_SacrificeTracker.Get(Map).lastSacrificeType == HerdUtility.SacrificeType.animal)
                    {
                        ChangeState(State.sacrificing, SacrificeState.finished);
                    }
                    return;

                case SacrificeState.finished:
                case SacrificeState.off:
                    ChangeState(State.notinuse);
                    return;
            }
        }

        #endregion Ticker

        #region Inspect
        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();

            // Add the inspections string from the base
            stringBuilder.Append(base.GetInspectString());

            // return the complete string
            return stringBuilder.ToString();
        }
        #endregion Inspect

        #region Gizmos
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
                yield return g;

            // End Game
            if (this.HasConversionSarsen())
            {
                if (!IsConverting())
                {
                    var command_Action = new Command_Action
                    {
                        action = TryConversion,
                        defaultLabel = "RH_TET_Beastmen_CommandConversion".Translate(),
                        defaultDesc = "RH_TET_Beastmen_CommandConversionDesc".Translate(),
                        disabledReason = "RH_TET_Beastmen_CommandDisabled".Translate(),
                        hotKey = KeyBindingDefOf.Misc1,
                        icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_Conversion")
                    };
                    command_Action.Disabled = false;
                    if (currentFunction < Function.Level2) command_Action.icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_ConversionDisabled");
                    yield return command_Action;
                }
                else
                {
                    var command_Cancel = new Command_Action
                    {
                        action = CancelConversion,
                        defaultLabel = "CommandCancelConstructionLabel".Translate(),
                        defaultDesc = "RH_TET_Beastmen_CommandCancelConversion".Translate(),
                        hotKey = KeyBindingDefOf.Designator_Cancel,
                        icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel")
                    };
                    command_Cancel.Disabled = false;
                    yield return command_Cancel;
                }
            }

            if (currentFunction >= Function.Level2)
            {
                if (!IsOffering())
                {
                    var command_Action = new Command_Action
                    {
                        action = TryOffering,
                        defaultLabel = "RH_TET_Beastmen_CommandOffering".Translate(),
                        defaultDesc = "RH_TET_Beastmen_CommandOfferingDesc".Translate(),
                        hotKey = KeyBindingDefOf.Misc3,
                        icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_MakeOffering")
                    };
                    command_Action.Disabled = false;
                    yield return command_Action;
                }
                else
                {
                    var command_Cancel = new Command_Action
                    {
                        action = CancelOffering,
                        defaultLabel = "RH_TET_Beastmen_Cancel".Translate(),
                        defaultDesc = "RH_TET_Beastmen_CommandCancelOffering".Translate(),
                        hotKey = KeyBindingDefOf.Designator_Cancel,
                        icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel")
                    };
                    yield return command_Cancel;
                }
                
                if (currentFunction >= Function.Level3)
                {
                    if (!IsSacrificing())
                    {
                        var command_Action = new Command_Action
                        {
                            action = TrySacrifice,
                            defaultLabel = "RH_TET_Beastmen_CommandSacrifice".Translate(),
                            defaultDesc = "RH_TET_Beastmen_CommandSacrificeDesc".Translate(),
                            disabledReason = "RH_TET_Beastmen_CommandDisabled".Translate(),
                            hotKey = KeyBindingDefOf.Misc1,
                            icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_Sacrifice")
                        };
                        command_Action.Disabled = (currentFunction < Function.Level2);
                        if (currentFunction < Function.Level2) command_Action.icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_SacrificeDisabled");
                        yield return command_Action;
                    }
                    else
                    {
                        var command_Cancel = new Command_Action
                        {
                            action = CancelSacrifice,
                            defaultLabel = "CommandCancelConstructionLabel".Translate(),
                            defaultDesc = "RH_TET_Beastmen_CommandCancelSacrifice".Translate(),
                            hotKey = KeyBindingDefOf.Designator_Cancel,
                            icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel")
                        };
                        command_Cancel.Disabled = (currentFunction < Function.Level2);
                        yield return command_Cancel;
                    }
                }
            }

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Upgrade Herdstone",
                    action = delegate
                    {
                        TryUpgrade();
                    }
                };
                if (finalDestructionQuestExposed)
                { 
                    yield return new Command_Action
                    {
                        defaultLabel = "Debug: Reset Final Destruction Quest Spawn",
                        action = delegate
                        {
                            this.finalDestructionQuestExposed = false;
                        }
                    };
                }
            }
            
            yield break;
        }

        public bool HasConversionSarsen()
        {
            List<Thing> thingList = this.Map.listerThings.ThingsOfDef(BeastmenDefOf.RH_TET_Beastmen_SarsenDouble_Conversion);
            if (thingList != null && thingList.Count > 0)
                return true;
            else return false;
        }
        #endregion Gizmos

        #region LevelChangers

        private void TryUpgrade()
        {
            if (IsCongregating())
            {
                return;
            }
            Upgrade();
        }

        public void Upgrade()
        {
            var newDefName = "";
            Function newFunction = Function.Level1;
            switch (currentFunction)
            {
                case Function.Level1:
                    newDefName = "RH_TET_Beastmen_MassiveHerdstone_UpgradeOne";
                    newFunction = Function.Level2;
                    break;
                case Function.Level2:
                    newDefName = "RH_TET_Beastmen_MassiveHerdstone_UpgradeTwo";
                    newFunction = Function.Level3;
                    break;
                case Function.Level3:
                    newDefName = "RH_TET_Beastmen_MassiveHerdstone_UpgradeThree";
                    newFunction = Function.Level4;
                    break;
                case Function.Level4:
                    Log.Error("Tried to upgrade fully upgraded massive herdstone. This should never happen.");
                    break;
            }
            if (newDefName == "") return;
            ReplaceHerdstoneWith(newDefName, newFunction);
        }

        private Building_MassiveHerdstone ReplaceHerdstoneWith(string newDefName, Function newCurrentFunction)
        {
            Building_MassiveHerdstone result = null;
            if (newDefName == "")
            {
                TheEndTimes_Beastmen.Utility.ErrorReport("ReplaceHerdstoneWith :: Null newDef exception.");
                return result;
            }

            //Copy the important values.
            var currentLocation = Position;
            var currentRotation = Rotation;
            var currentStuff = Stuff;
            var compQuality = this.TryGetComp<CompQuality>();
            var currentLastFunction = lastFunction;
            var currentMap = this.Map;
            var qualityCat = QualityCategory.Normal;
            if (compQuality != null)
            {
                qualityCat = compQuality.Quality;
            }

            //Worship values
            var s1 = HerdstoneName;
            var p1 = tempShaman;
            var sarsenVal = this.sarsenBonus;
            var animalsVal = this.sacrificedAnimalsCount;
            var humansVal = this.sacrificedHumansCount;

            Destroy();

            //Spawn the new altar over the other
            var thing = (Building_MassiveHerdstone)ThingMaker.MakeThing(ThingDef.Named(newDefName), currentStuff);
            result = thing;
            thing.SetFaction(Faction.OfPlayer);
            thing.Rotation = currentRotation;
            GenPlace.TryPlaceThing(thing, currentLocation, currentMap, ThingPlaceMode.Direct);
            thing.Rotation = currentRotation;
            thing.TryGetComp<CompQuality>().SetQuality(qualityCat, ArtGenerationContext.Colony);
            thing.lastFunction = currentLastFunction;
            thing.currentFunction = newCurrentFunction;

            if (newDefName.Equals("RH_TET_Beastmen_MassiveHerdstone_UpgradeOne"))
            {
                IntVec3 intVec3 = this.Position;
                string text = "RH_TET_Beastmen_UpgradeSuccessful".Translate();

                Find.LetterStack.ReceiveLetter("RH_TET_Beastmen_HerdstoneUpgradeLabel".Translate(), text, LetterDefOf.PositiveEvent, (LookTargets)new GlobalTargetInfo(intVec3, this.Map, false), null, null);
            }
            else if (newDefName.Equals("RH_TET_Beastmen_MassiveHerdstone_UpgradeTwo"))
            {
                IntVec3 intVec3 = this.Position;
                string text = "RH_TET_Beastmen_UpgradeSuccessful2".Translate();

                Find.LetterStack.ReceiveLetter("RH_TET_Beastmen_HerdstoneUpgradeLabel".Translate(), text, LetterDefOf.PositiveEvent, (LookTargets)new GlobalTargetInfo(intVec3, this.Map, false), null, null);
            }
            else if (newDefName.Equals("RH_TET_Beastmen_MassiveHerdstone_UpgradeThree"))
            {
                IntVec3 intVec3 = this.Position;
                string text = "RH_TET_Beastmen_UpgradeSuccessful3".Translate();

                Find.LetterStack.ReceiveLetter("RH_TET_Beastmen_HerdstoneUpgradeLabel".Translate(), text, LetterDefOf.PositiveEvent, (LookTargets)new GlobalTargetInfo(intVec3, this.Map, false), null, null);
            }

            //Pass worship and other values
            thing.HerdstoneName = s1;
            thing.tempShaman = p1;
            thing.sarsenBonus = sarsenVal;
            thing.sacrificedAnimalsCount = animalsVal;
            thing.sacrificedHumansCount = humansVal;

            return result;
        }

        #endregion LevelChangers

        #region Offering
        private void CancelOffering()
        {
            Pawn pawn = null;
            // JEH 1.4
            //var listeners = Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            // JEH 1.5
            var listeners = Map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(x => x.RaceProps.intelligence == Intelligence.Humanlike)).ToList<Pawn>();
            var flag = new bool[listeners.Count];
            for (var i = 0; i < listeners.Count; i++)
            {
                pawn = listeners[i];
                if (pawn.Faction == Faction.OfPlayer)
                {
                    //Hold Offering
                    if (pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_MakeOffering)
                    {
                        pawn.jobs.StopAll();
                    }
                }
            }
            ChangeState(State.notinuse);

            Messages.Message("RH_TET_Beastmen_CommandCancellingOffering".Translate(), MessageTypeDefOf.NegativeEvent);
        }

        private void TryOffering()
        {
            if (IsCongregating())
            {
                Messages.Message("RH_TET_Beastmen_HerdGathering".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            if (!CanGatherOfferingNow()) return;

            if (!TryDetermineOffering(tempOfferingType, tempOfferingSize, tempOfferer, this, out tempDeterminedOfferings, out billRecipe))
            {
                TheEndTimes_Beastmen.Utility.DebugReport("Failed to determine offering");
                return;
            }

            switch (currentOfferingState)
            {
                case OfferingState.finished:
                case OfferingState.off:
                    if (IsSacrificing())
                    {
                        CancelSacrifice();
                    }
                    StartOffering();
                    return;

                case OfferingState.started:
                case OfferingState.offering:
                    Messages.Message("RH_TET_Beastmen_OfferingHappening".Translate(), TargetInfo.Invalid, MessageTypeDefOf.RejectInput);
                    return;
            }
        }

        private bool CanGatherOfferingNow()
        {
            if (tempOfferingType == HerdUtility.SacrificeType.none) return RejectMessage("RH_TET_Beastmen_NoOfferingSelected".Translate());
            if (tempOfferingSize == HerdUtility.OfferingSize.none) return RejectMessage("RH_TET_Beastmen_NoOfferingAmtSelected".Translate());
            if (tempOfferer == null) return RejectMessage("RH_TET_Beastmen_NoOffererSelected".Translate());
            if (tempOfferer.Drafted) return RejectMessage("RH_TET_Beastmen_OffererDrafted".Translate());
            if (tempOfferer.Dead || tempOfferer.Downed) return RejectMessage("RH_TET_Beastmen_AbleOfferer".Translate(), tempOfferer);
            if (!tempOfferer.CanReserve(this)) return RejectMessage("RH_TET_Beastmen_HerdstoneReserved".Translate());
            return !Position.GetThingList(Map).OfType<Corpse>().Any() || RejectMessage("RH_TET_Beastmen_HerdstoneClear".Translate());
        }

        public void StartOffering()
        {
            determinedOfferings = tempDeterminedOfferings;
            offerer = tempOfferer;
            currentOfferingGod = tempCurrentOfferingGod;

            if (Destroyed || !Spawned)
            {
                HerdUtility.AbortCongregation(null, "RH_TET_Beastmen_Herdstone".Translate() + "RH_TET_Beastmen_IsUnavailable".Translate());
                return;
            }
            if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(offerer))
            {
                HerdUtility.AbortCongregation(this, "RH_TET_Beastmen_Offerer".Translate() + " " + offerer.LabelShort + "RH_TET_Beastmen_IsUnavailable".Translate());
                offerer = null;
                return;
            }

            Messages.Message("RH_TET_Beastmen_OfferingGathered".Translate(), TargetInfo.Invalid, MessageTypeDefOf.NeutralEvent);
            ChangeState(State.offering, OfferingState.started);
            TheEndTimes_Beastmen.Utility.DebugReport("Make offering called.");

            var job2 = new Job(BeastmenDefOf.RH_TET_Beastmen_MakeOffering)
            {
                playerForced = true,
                targetA = this,
                targetQueueB = new List<LocalTargetInfo>(determinedOfferings.Count),
                targetC = PositionHeld,
                countQueue = new List<int>(determinedOfferings.Count)
            };
            for (var i = 0; i < determinedOfferings.Count; i++)
            {
                job2.targetQueueB.Add(determinedOfferings[i].Thing);
                job2.countQueue.Add(determinedOfferings[i].Count);
            }
            job2.haulMode = HaulMode.ToCellNonStorage;
            job2.locomotionUrgency = LocomotionUrgency.Sprint;
            job2.bill = new Bill_Production(billRecipe);
            offerer.jobs.TryTakeOrderedJob(job2);
        }

        #endregion Offering

        #region Sacrifice


        private void CancelSacrifice()
        {
            Pawn pawn = null;
            // JEH 1.4
            //var listeners = Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            // JEH 1.5
            var listeners = Map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(x => x.RaceProps.intelligence == Intelligence.Humanlike)).ToList<Pawn>();
            if (!listeners.NullOrEmpty())
            {
                foreach (var t in listeners)
                {
                    pawn = t;
                    if (pawn.Faction != Faction.OfPlayer) continue;
                    if (pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_HoldSacrifice ||
                        pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_HoldConversion ||
                        pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_AttendSacrifice ||
                        pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_ReflectOnResult ||
                        pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_MakeOffering)
                    {
                        pawn.jobs.StopAll();
                    }
                }
            }
            ChangeState(State.notinuse);
            Messages.Message("RH_TET_Beastmen_SacrificeCancelled".Translate(), MessageTypeDefOf.NegativeEvent);
        }
        private void TrySacrifice()
        {
            if (IsSacrificing())
            {
                Messages.Message("RH_TET_Beastmen_SacrificeGatheringAlready".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            if (!CanGatherSacrificeNow()) return;

            switch (currentSacrificeState)
            {
                case SacrificeState.finished:
                case SacrificeState.off:
                    StartSacrifice();
                    return;

                case SacrificeState.started:
                case SacrificeState.gathering:
                case SacrificeState.sacrificing:
                case SacrificeState.finishing:
                    Messages.Message("RH_TET_Beastmen_SacrificeGatheringAlready".Translate(), TargetInfo.Invalid, MessageTypeDefOf.RejectInput);
                    return;
            }
        }
        private bool CanGatherSacrificeNow()
        {
            if (tempSacrifice == null) return RejectMessage("RH_TET_Beastmen_NoVictim".Translate());
            if (tempExecutioner == null) return RejectMessage("RH_TET_Beastmen_NoExecutionerSelected".Translate());
            if (tempExecutioner.Drafted) return RejectMessage("RH_TET_Beastmen_ExecutionerDrafted".Translate());
            if (tempCurrentSacrificeGod == null) return RejectMessage("RH_TET_Beastmen_NoDarkGod".Translate());
            if (tempSacrifice.Dead) return RejectMessage("RH_TET_Beastmen_VictimDead".Translate(), tempSacrifice);
            if (tempExecutioner.Dead || tempExecutioner.Downed) return RejectMessage("RH_TET_Beastmen_AbleExec".Translate());
            if (!tempExecutioner.CanReserve(tempSacrifice)) return RejectMessage("RH_TET_Beastmen_CantReserveSacrifice".Translate());
            if (!tempExecutioner.CanReserve(this)) return RejectMessage("RH_TET_Beastmen_HerdstoneIsReserved".Translate());
            if (Position.GetThingList(Map).OfType<Corpse>().Any())
            {
                return RejectMessage("RH_TET_Beastmen_HerdstoneClear".Translate());
            }

            return true;
        }
        private bool CanGatherConversionNow()
        {
            if (tempSacrifice == null) return RejectMessage("RH_TET_Beastmen_NoVictim".Translate());
            if (tempExecutioner == null) return RejectMessage("RH_TET_Beastmen_NoExecutionerSelected".Translate());
            if (tempExecutioner.Drafted) return RejectMessage("RH_TET_Beastmen_ExecutionerDrafted".Translate());
            if (tempSacrifice.Dead) return RejectMessage("RH_TET_Beastmen_VictimDead".Translate(), tempSacrifice);
            if (tempExecutioner.Dead || tempExecutioner.Downed) return RejectMessage("RH_TET_Beastmen_AbleExec".Translate());
            if (!tempExecutioner.CanReserve(tempSacrifice)) return RejectMessage("RH_TET_Beastmen_CantReserveSacrifice".Translate());
            if (!tempExecutioner.CanReserve(this)) return RejectMessage("RH_TET_Beastmen_HerdstoneIsReserved".Translate());
            if (Position.GetThingList(Map).OfType<Corpse>().Any())
            {
                return RejectMessage("RH_TET_Beastmen_HerdstoneClear".Translate());
            }

            return true;
        }
        private void StartSacrifice()
        {
            //Check for missing actors
            if (!PreStartSacrificeReady()) return;

            //Reset results
            MapComponent_SacrificeTracker.Get(Map).lastResult = HerdUtility.SacrificeResult.none;

            //Send a message about the gathering
            if (Map?.info?.parent is Settlement factionBase)
            {
                Messages.Message("RH_TET_Beastmen_SacrificeGathering".Translate(new object[] {
                    "Herd"
                }), TargetInfo.Invalid, MessageTypeDefOf.NeutralEvent);
            }

            //Change the state
            ChangeState(State.sacrificing, SacrificeState.started);

            //Create a new "bill" with all the variables from the sacrifice form
            sacrificeData = new Bill_Sacrifice(tempSacrifice, tempExecutioner, tempCurrentSacrificeGod);
            TheEndTimes_Beastmen.Utility.DebugReport("Force Sacrifice called");

            //Give the sacrifice job
            var job = new Job(BeastmenDefOf.RH_TET_Beastmen_HoldSacrifice, tempSacrifice, this) { count = 1 };
            tempExecutioner.jobs.TryTakeOrderedJob(job);

            //Set the congregation
            sacrificeData.Congregation = GetSacrificeGroup();
            TheEndTimes_Beastmen.Utility.DebugReport("Sacrifice state set to gathering");
        }

        private bool PreStartSacrificeReady()
        {
            if (Destroyed || !Spawned)
            {
                HerdUtility.AbortCongregation(null, "The herdstone is unavailable.");
                return false;
            }
            if (!CurrentlyUsableForBills())
            {
                Messages.Message("RH_TET_Beastmen_NoMassiveHerdstone".Translate() + " " + "RH_TET_Beastmen_HerdstoneRequiresFuel".Translate(), TargetInfo.Invalid, MessageTypeDefOf.RejectInput);
                return false;
            }
            if (!TheEndTimes_Beastmen.Utility.IsActorAvailable(tempExecutioner))
            {
                HerdUtility.AbortCongregation(this,
                    "The executioner, " + tempExecutioner.LabelShort + " is unavaialable.");
                tempExecutioner = null;
                return false;
            }
            if (TheEndTimes_Beastmen.Utility.IsActorAvailable(tempSacrifice, true)) return true;
            HerdUtility.AbortCongregation(this, "The victim, " + tempSacrifice.LabelShort + " is unavaialable.");
            tempSacrifice = null;
            return false;
        }

        private HashSet<Pawn> availableAttendees;// JEH ADDED

        public HashSet<Pawn> AvailableWorshippers
        {
            get
            {
                if (availableAttendees == null || availableAttendees.Count == 0)
                {
                    availableAttendees =
                        new HashSet<Pawn>(
                            
                            //Map.mapPawns.AllPawnsSpawned.FindAll(y => y is Pawn x &&
                            //   x.RaceProps.Humanlike &&
                            //   !x.IsPrisoner &&
                            //   x.Faction == Faction &&
                            //   x.RaceProps.intelligence == Intelligence.Humanlike &&
                            //   !x.Downed && !x.Dead &&
                            //   !x.InMentalState && !x.InAggroMentalState &&
                            //   x.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_AttendSacrifice &&
                            //   x.CurJob.def != JobDefOf.Capture &&
                            //   x.CurJob.def != JobDefOf.ExtinguishSelf && 
                            //   x.CurJob.def != JobDefOf.Rescue && 
                            //   x.CurJob.def != JobDefOf.TendPatient && 
                            //   x.CurJob.def != JobDefOf.BeatFire && 
                            //   x.CurJob.def != JobDefOf.Lovin && 
                            //   x.CurJob.def != JobDefOf.LayDown && 
                            //   x.CurJob.def != JobDefOf.FleeAndCower

                            Map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(y => y is Pawn x &&
                               x.RaceProps.Humanlike &&
                               !x.IsPrisoner &&
                               x.Faction == Faction &&
                               x.RaceProps.intelligence == Intelligence.Humanlike &&
                               !x.Downed && !x.Dead &&
                               !x.InMentalState && !x.InAggroMentalState &&
                               x.CurJob.def != BeastmenDefOf.RH_TET_Beastmen_AttendSacrifice &&
                               x.CurJob.def != JobDefOf.Capture &&
                               x.CurJob.def != JobDefOf.ExtinguishSelf &&
                               x.CurJob.def != JobDefOf.Rescue &&
                               x.CurJob.def != JobDefOf.TendPatient &&
                               x.CurJob.def != JobDefOf.BeatFire &&
                               x.CurJob.def != JobDefOf.Lovin &&
                               x.CurJob.def != JobDefOf.LayDown &&
                               x.CurJob.def != JobDefOf.FleeAndCower)
                            )).ToHashSet<Pawn>();
                }
                return availableAttendees;
            }
        }

        public static List<Pawn> GetSacrificeGroup(Building_MassiveHerdstone herdstone) => herdstone.GetSacrificeGroup();

        public List<Pawn> GetSacrificeGroup()
        {
            var room = this.GetRoom();

            var pawns = new List<Pawn>();
            if (room.Role == RoomRoleDefOf.PrisonBarracks || room.Role == RoomRoleDefOf.PrisonCell) return pawns;

            if (AvailableWorshippers == null || AvailableWorshippers.Count <= 0) return pawns;

            foreach (var p in AvailableWorshippers)
            {
                HerdUtility.GiveAttendSacrificeJob(this, p);
                pawns.Add(p);
            }
            return pawns;
        }

        #endregion Sacrifice

        #region Conversion
        private void CancelConversion()
        {
            Pawn pawn = null;
            // JEH 1.4
            //var listeners = Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            // JEH 1.5
            var listeners = Map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(x => x.RaceProps.intelligence == Intelligence.Humanlike)).ToList<Pawn>();
            if (!listeners.NullOrEmpty())
            {
                foreach (var t in listeners)
                {
                    pawn = t;
                    if (pawn.Faction != Faction.OfPlayer) continue;
                    if (pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_HoldConversion ||
                        pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_AttendSacrifice ||
                        pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_ReflectOnResult ||
                        pawn.CurJob.def == BeastmenDefOf.RH_TET_Beastmen_MakeOffering)
                    {
                        pawn.jobs.StopAll();
                    }
                }
            }
            ChangeState(State.notinuse);
            Messages.Message("RH_TET_Beastmen_ConversionCancelled".Translate(), MessageTypeDefOf.NegativeEvent);
        }
        private void TryConversion()
        {
            if (IsSacrificing())
            {
                Messages.Message("RH_TET_Beastmen_SacrificeGatheringAlready".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            if (!CanGatherConversionNow()) return;

            switch (currentSacrificeState)
            {
                case SacrificeState.finished:
                case SacrificeState.off:
                    StartConversion();
                    return;

                case SacrificeState.started:
                case SacrificeState.gathering:
                case SacrificeState.sacrificing:
                case SacrificeState.finishing:
                    Messages.Message("RH_TET_Beastmen_SacrificeGatheringAlready".Translate(), TargetInfo.Invalid, MessageTypeDefOf.RejectInput);
                    return;
            }
        }
        private void StartConversion()
        {
            //Check for missing actors
            if (!PreStartSacrificeReady()) return;

            //Reset results
            MapComponent_SacrificeTracker.Get(Map).lastResult = HerdUtility.SacrificeResult.none;

            //Send a message about the gathering
            if (Map?.info?.parent is Settlement factionBase)
            {
                Messages.Message("RH_TET_Beastmen_WorshipGathering".Translate(new object[] {
                    "Herd"
                }), TargetInfo.Invalid, MessageTypeDefOf.NeutralEvent);
            }

            //Change the state
            ChangeState(State.converting, SacrificeState.started);

            //Create a new "bill" with all the variables from the sacrifice form
            conversionData = new Bill_Convert(tempSacrifice, tempExecutioner);
            TheEndTimes_Beastmen.Utility.DebugReport("Force Conversion called");

            //Give the sacrifice job
            var job = new Job(BeastmenDefOf.RH_TET_Beastmen_HoldConversion, tempSacrifice, this) { count = 1 };
            tempExecutioner.jobs.TryTakeOrderedJob(job);

            //Set the congregation
            conversionData.Congregation = GetSacrificeGroup();
            TheEndTimes_Beastmen.Utility.DebugReport("Conversion state set to gathering");
        }

        #endregion Conversion

        #region Misc

        // RimWorld.WorkGiver_DoBill
        // Vanilla Code
        private bool TryFindBestOfferingIngredients(RecipeDef recipe, Pawn pawn, Building_MassiveHerdstone billGiver, List<ThingCount> chosen)
        {
            chosen.Clear();
            var relevantThings = new List<Thing>();
            var ingredientsOrdered = new List<IngredientCount>();
            var newRelevantThings = new List<Thing>();
            
            if (recipe.ingredients.Count == 0)
            {
                return true;
            }
            
            var billGiverRootCell = billGiver.InteractionCell;
            var validRegionAt = Map.regionGrid.GetValidRegionAt(billGiverRootCell);
            if (validRegionAt == null)
            {
                TheEndTimes_Beastmen.Utility.DebugReport("Invalid Region");

                return false;
            }

            ingredientsOrdered.Clear();
            if (recipe.productHasIngredientStuff)
            {
                TheEndTimes_Beastmen.Utility.DebugReport(recipe.ingredients[0].ToString());
                ingredientsOrdered.Add(recipe.ingredients[0]);
            }
            
            for (var i = 0; i < recipe.ingredients.Count; i++)
            {
                if (!recipe.productHasIngredientStuff || i != 0)
                {
                    var ingredientCount = recipe.ingredients[i];
                    if (ingredientCount.filter.AllowedDefCount == 1)
                    {
                        TheEndTimes_Beastmen.Utility.DebugReport(ingredientCount.ToString());
                        ingredientsOrdered.Add(ingredientCount);
                    }
                }
            }

            for (var j = 0; j < recipe.ingredients.Count; j++)
            {
                var item = recipe.ingredients[j];
                if (!ingredientsOrdered.Contains(item))
                {
                    TheEndTimes_Beastmen.Utility.DebugReport(item.ToString());
                    ingredientsOrdered.Add(item);
                }
            }
            relevantThings.Clear();

            var foundAll = false;
            bool BaseValidator(Thing t)
            {
                return t.Spawned && !t.IsForbidden(pawn) &&
                       (t.Position - billGiver.Position).LengthHorizontalSquared < 999 * 999 &&
                       recipe.fixedIngredientFilter.Allows(t) && recipe.defaultIngredientFilter.Allows(t) &&
                       recipe.ingredients.Any((IngredientCount ingNeed) => ingNeed.filter.Allows(t)) &&
                       pawn.CanReserve(t);
            }

            bool RegionProcessor(Region r)
            {
                newRelevantThings.Clear();
                var list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                for (var i = 0; i < list.Count; i++)
                {
                    var thing = list[i];
                    if (!BaseValidator(thing)) continue;
                    TheEndTimes_Beastmen.Utility.DebugReport(thing.ToString());
                    newRelevantThings.Add(thing);
                }
                if (newRelevantThings.Count <= 0) return false;
                Comparison<Thing> comparison = delegate (Thing t1, Thing t2)
                {
                    float lengthHorizontalSquared = (t1.Position - pawn.Position).LengthHorizontalSquared;
                    float lengthHorizontalSquared2 = (t2.Position - pawn.Position).LengthHorizontalSquared;
                    return lengthHorizontalSquared.CompareTo(lengthHorizontalSquared2);
                };
                newRelevantThings.Sort(comparison);
                relevantThings.AddRange(newRelevantThings);
                newRelevantThings.Clear();
                var flag = true;
                foreach (var ingredientCount in recipe.ingredients)
                {
                    var num = ingredientCount.GetBaseCount();
                    foreach (var thing in relevantThings)
                    {
                        if (!ingredientCount.filter.Allows(thing)) continue;
                        TheEndTimes_Beastmen.Utility.DebugReport(thing.ToString());
                        var num2 = recipe.IngredientValueGetter.ValuePerUnitOf(thing.def);
                        var num3 = Mathf.Min(Mathf.CeilToInt(num / num2), thing.stackCount);
                        AddToList(chosen, thing, num3);
                        num -= (float)num3 * num2;
                        if (num <= 0.0001f)
                            break;
                    }
                    if (num > 0.0001f)
                        flag = false;
                }
                if (!flag) return false;
                foundAll = true;
                return true;
            }

            var traverseParams = TraverseParms.For(pawn);
            RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParams, false);
            RegionTraverser.BreadthFirstTraverse(validRegionAt, entryCondition, RegionProcessor, 99999);
            return foundAll;
        }

        public static void AddToList(List<ThingCount> list, Thing thing, int countToAdd)
        {
            for (int index = 0; index < list.Count; ++index)
            {
                if (list[index].Thing == thing)
                {
                    int totalCount = list[index].Count + countToAdd;
                    if (totalCount > list[index].Count)
                    {
                        totalCount = countToAdd;
                    }
                    list[index] = list[index].WithCount(totalCount);
                    return;
                }
            }
            list.Add(new ThingCount(thing, countToAdd));
        }

        public bool TryDetermineOffering(HerdUtility.SacrificeType type, HerdUtility.OfferingSize size, Pawn pawn,
            Building_MassiveHerdstone herdstone, out List<ThingCount> result, out RecipeDef resultRecipe)

        {
            result = null;
            resultRecipe = null;
            var list = new List<ThingCount>();
            RecipeDef recipe = null;

            var recipeDefName = "RH_TET_Beastmen_OfferingOf";
            switch (type)
            {
                case HerdUtility.SacrificeType.plants:
                    recipeDefName = recipeDefName + "Plants";
                    MapComponent_SacrificeTracker.Get(Map).lastSacrificeType = HerdUtility.SacrificeType.plants;
                    herdstone.tempCurrentOfferingGod = DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Tzeentch);
                    herdstone.currentOfferingGod = DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Tzeentch);
                    break;
                case HerdUtility.SacrificeType.meat:
                    recipeDefName = recipeDefName + "Meat";
                    MapComponent_SacrificeTracker.Get(Map).lastSacrificeType = HerdUtility.SacrificeType.meat;
                    herdstone.tempCurrentOfferingGod = DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Slaanesh);
                    herdstone.currentOfferingGod = DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Slaanesh);
                    break;
                case HerdUtility.SacrificeType.meals:
                    // For now, I shut off meals. Do a scan on the meals enum above and find commented out code to turn on.
                    recipeDefName = recipeDefName + "Meals";
                    MapComponent_SacrificeTracker.Get(Map).lastSacrificeType = HerdUtility.SacrificeType.meals;
                    // Select a god randomly when meals are given.
                    if (RH_TET_BeastmenMod.random.Next(0, 2) == 0)
                    { 
                        herdstone.tempCurrentOfferingGod = DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Tzeentch);
                    }
                    else
                    { 
                        herdstone.tempCurrentOfferingGod = DarkGodTracker.FindDarkGod(RH_TET_MagicDefOf.RH_TET_Slaanesh);
                    }
                    break;
            }
            switch (size)
            {
                case HerdUtility.OfferingSize.weak:
                    recipeDefName = recipeDefName + "_Meagre";
                    break;
                case HerdUtility.OfferingSize.ok:
                    recipeDefName = recipeDefName + "_Decent";
                    break;
                case HerdUtility.OfferingSize.nice:
                    recipeDefName = recipeDefName + "_Sizable";
                    break;
                case HerdUtility.OfferingSize.solid:
                    recipeDefName = recipeDefName + "_Worthy";
                    break;
                case HerdUtility.OfferingSize.great:
                    recipeDefName = recipeDefName + "_Impressive";
                    break;
            }

            recipe = DefDatabase<RecipeDef>.GetNamed(recipeDefName);
            
            resultRecipe = recipe;
            if (!TryFindBestOfferingIngredients(recipe, pawn, herdstone, list))
            {
                Messages.Message("RH_TET_Beastmen_FailedOfferingIngredients".Translate(), MessageTypeDefOf.RejectInput);
                result = null;
                return false;
            }
            result = list;
            return true;
        }

        #endregion Misc

        public bool CurrentlyUsableForBills()
        {
            CompRefuelable compRefuelable = this.TryGetComp<CompRefuelable>();
            return this.UsableForBillsAfterFueling() && (compRefuelable != null && compRefuelable.HasFuel);
        }

        public bool UsableForBillsAfterFueling()
        {
            return true;
        }
        public virtual void Notify_BillDeleted(Bill bill)
        {
        }
    }
}