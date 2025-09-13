using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Beastmen
{
    public class FinalDestructionSite : MapParent
    {
        public static FinalDestructionSite ActiveSite;
        public CaravanArrivalAction_FinalDestruction caravanAction;
        public FinalDestructionComp finalDestructionComp;
        public static bool playerWon = false;
        public bool aiAssault = false;
        public bool aiFlee = false;
        private bool giveMoodBoost = true;
        public int startingEnemyCount = -1;
        public float enemyAttackThreshold = .50F;
        public float enemyFleeThreshold = .15F;

        public override void SpawnSetup()
        {
            if (this.Faction == null)
            {
                this.SetFaction(this.GetEnemyFaction());
            }

            base.SpawnSetup();
            this.caravanAction = new CaravanArrivalAction_FinalDestruction((MapParent)this);
            this.finalDestructionComp = this.GetComponent<FinalDestructionComp>();

            Faction f = this.Faction;

            if (!f.HostileTo(Faction.OfPlayer))
            {
                int currentGoodwill = f.GoodwillWith(Faction.OfPlayer);

                if (currentGoodwill > 0)
                {
                    currentGoodwill *= -1;
                    currentGoodwill += -100;
                }
                else if (currentGoodwill < 0)
                {
                    currentGoodwill *= -1;
                    currentGoodwill = -100 + currentGoodwill;
                }
                else
                    currentGoodwill = -100;

                f.TryAffectGoodwillWith(Faction.OfPlayer, currentGoodwill, true, true, null);
            }
        }

        private Faction GetEnemyFaction()
        {
            Faction f = Find.FactionManager.FirstFactionOfDef(BeastmenDefOf.RH_TET_EmpireOfMan);
            return f;
        }

        protected override void Tick()
        {
            base.Tick();

            if (!this.HasMap || playerWon)
                return;

            this.CheckPlayerWon();
            this.CheckColonistsNow();
        }

        private void CheckPlayerWon()
        {
            if (playerWon || this.Faction == Faction.OfPlayer)
                return;

            Map map = this.Map;

            if (giveMoodBoost && map != null)
            {
                HerdUtility.GiveMoodToPawns(BeastmenDefOf.RH_TET_Beastmen_FightingFinalBattle, map.mapPawns.FreeColonistsSpawned);
                giveMoodBoost = false;
            }

            if (map == null || !this.IsDefeated(map, this.Faction))
                return;

            // Do end game / victory for beastmen stuff.
            playerWon = true;

            // Do positive mood thing for a pawns involved.
            HerdUtility.RemoveMoodFromPawns(BeastmenDefOf.RH_TET_Beastmen_FightingFinalBattle, map.mapPawns.FreeColonistsSpawned);
            HerdUtility.GiveMoodToPawns(BeastmenDefOf.RH_TET_Beastmen_DestroyedFinalOrder, PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists);

            StringBuilder text = new StringBuilder();
            text.Append("RH_TET_Beastmen_DestroyedOrderDesc".Translate((NamedArgument)this.Label, (NamedArgument)TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(60000)));
            foreach (Faction allFaction in Find.FactionManager.AllFactions)
            {
                if (!allFaction.def.hidden && !allFaction.IsPlayer && (allFaction != this.Faction && allFaction.HostileTo(this.Faction)))
                {
                    FactionRelationKind playerRelationKind = allFaction.PlayerRelationKind;
                    if (allFaction.TryAffectGoodwillWith(Faction.OfPlayer, 20, false, false, null, new GlobalTargetInfo?()))
                    {
                        text.AppendLine();
                        text.AppendLine();
                        text.Append("RelationsWith".Translate((NamedArgument)allFaction.Name) + ": " + 20.ToStringWithSign());
                        allFaction.TryAppendRelationKindChangedInfo(text, playerRelationKind, allFaction.PlayerRelationKind, (string)null);
                    }
                }
            }

            Find.LetterStack.ReceiveLetter("RH_TET_Beastmen_DestroyedOrderLabel".Translate(),
                    text.ToString(),
                    LetterDefOf.PositiveEvent,
                    (LookTargets)new GlobalTargetInfo(this.Tile),
                    this.Faction,
                    null,
                    null,
                    null);

            this.SetFaction(Faction.OfPlayer);

            TaleRecorder.RecordTale(BeastmenDefOf.RH_TET_Beastmen_FinalDestructionVictory, (object)map.mapPawns.FreeColonists.RandomElement<Pawn>());

            // Show credits!
            RH_TET_Beastmen_GameVictoryUtility.FinalDestructionVictory();
        }

        private bool IsDefeated(Map map, Faction faction)
        {
            IEnumerable<Pawn> pawnList = map.mapPawns.FreeHumanlikesSpawnedOfFaction(faction);

            if (this.startingEnemyCount == -1)
                startingEnemyCount = pawnList.Count();

            int threatCount = 0;
            foreach (Pawn pawn in pawnList)
            {
                if (pawn.Map.fogGrid.IsFogged(pawn.Position))
                    threatCount++;
                else if (pawn.RaceProps.Humanlike && GenHostility.IsActiveThreatToPlayer((IAttackTarget)pawn))
                    threatCount++;
            }

            if (!aiFlee && (threatCount <= (int)(this.startingEnemyCount * enemyFleeThreshold)))
            {
                aiFlee = true;
                // Victory! Make remaining enemies flee.
                // TODO MAKES ANIMALS FLEE TOO. FIX. MAYBE JUST DON'T SPAWN ANY ANIMALS?
                List<Lord> lords = new List<Lord>();
                List<LordToil> lordToils = new List<LordToil>();
                foreach (Lord lord in this.Map.lordManager.lords)
                {
                    if (lord.faction != null && lord.faction.HostileTo(Faction.OfPlayer) && lord.faction.def.autoFlee)
                    {
                        LordToil newLordToil = lord.Graph.lordToils.FirstOrDefault<LordToil>((Func<LordToil, bool>)(st => st is LordToil_PanicFlee));
                        if (newLordToil != null)
                        {
                            lordToils.Add(newLordToil);
                            lords.Add(lord);
                        }
                    }
                }

                int i = 0;
                foreach (Lord lord in lords)
                {
                    LordToil newLordToil = lord.Graph.lordToils.FirstOrDefault<LordToil>((Func<LordToil, bool>)(st => st is LordToil_PanicFlee));
                    lord.GotoToil(lordToils.ElementAt(i));
                    i++;
                }

                return true;
            }
            else if (!aiFlee && !aiAssault && (threatCount <= (int)(this.startingEnemyCount * enemyAttackThreshold)))
            {
                aiAssault = true;
                // Now they're angry!
                foreach (Lord lord in this.Map.lordManager.lords)
                {
                    if (lord.faction != null && lord.faction.HostileTo(Faction.OfPlayer))
                    {
                        LordToil_AssaultColony toilAssaultColony = new LordToil_AssaultColony(true);
                        LordToil lordToilDefendBase1 = lord.Graph.lordToils.FirstOrDefault<LordToil>((Func<LordToil, bool>)(st => st is LordToil_DefendBase));

                        toilAssaultColony.useAvoidGrid = true;
                        toilAssaultColony.lord = lord;
                        lord.Graph.AddToil((LordToil)toilAssaultColony);

                        Transition transition3 = new Transition((LordToil)lordToilDefendBase1, (LordToil)toilAssaultColony, false, true);
                        transition3.AddTrigger((Trigger)new Trigger_FractionPawnsLost(0.2f));
                        transition3.AddTrigger((Trigger)new Trigger_PawnHarmed(0.4f, false, (Faction)null));
                        transition3.AddTrigger((Trigger)new Trigger_ChanceOnTickInterval(2500, 0.03f));
                        transition3.AddTrigger((Trigger)new Trigger_TicksPassed(251999));
                        transition3.AddTrigger((Trigger)new Trigger_UrgentlyHungry());
                        transition3.AddTrigger((Trigger)new Trigger_ChanceOnPlayerHarmNPCBuilding(0.4f));
                        transition3.AddPostAction((TransitionAction)new TransitionAction_WakeAll());
                        
                        TaggedString taggedString = "MessageDefendersAttacking".Translate((NamedArgument)lord.faction.def.pawnsPlural, (NamedArgument)lord.faction.Name, (NamedArgument)Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
                        Messages.Message(taggedString, (MessageTypeDef)MessageTypeDefOf.ThreatBig, true);
                        
                        lord.Graph.AddTransition(transition3, false);

                        LordToil newLordToil = lord.Graph.lordToils.FirstOrDefault<LordToil>((Func<LordToil, bool>)(st => st is LordToil_AssaultColony));
                        if (newLordToil != null)
                            lord.GotoToil(newLordToil);
                    }
                }
            }

            return false;
        }

        public void CheckColonistsNow()
        {
            if (playerWon)
                return;

            List<Pawn> list = this.Map.mapPawns.FreeColonists.ToList<Pawn>();

            int downedPawns = 0;
            list.ForEach((Action<Pawn>)(p =>
            {
                if (!p.Downed && !p.Dead && p.Spawned)
                    return;
                ++downedPawns;
            }));

            if (downedPawns != list.Count)
                return;

            Find.LetterStack.ReceiveLetter((Letter)LetterMaker.MakeLetter("RH_TET_Beastmen_FailedFinalDestructionLabel".Translate(), "RH_TET_Beastmen_FailedFinalDestructionDesc".Translate(), LetterDefOf.NegativeEvent), (string)null);
            Current.Game.DeinitAndRemoveMap(this.Map, false);

            HerdUtility.GiveMoodToPawns(BeastmenDefOf.RH_TET_Beastmen_FinalDestructionDefeat, PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists);
        }

        public override void PostMapGenerate()
        {
            base.PostMapGenerate();

            List<Pawn> pawnList;

            MapGenHandler.GenerateMap(BeastmenDefOf.RH_TET_Beastmen_FinalDestructionMap, this.Map, out pawnList, false, true, true, true, false, true, true, false, Find.FactionManager.FirstFactionOfDef(BeastmenDefOf.RH_TET_EmpireOfMan));

            this.ShowHelp();
        }

        private void ShowHelp()
        {
            DiaNode nodeRoot = new DiaNode("RH_TET_Beastmen_FinalDestructionHelp".Translate());

            nodeRoot.options.Add(new DiaOption("OK".Translate())
            {
                resolveTree = true
            });
            nodeRoot.options.Add(new DiaOption("JumpToLocation".Translate())
            {
                action = (Action)(() => CameraJumper.TryJumpAndSelect((GlobalTargetInfo)this.Map.PlayerPawnsForStoryteller.First<Pawn>())),
                resolveTree = true
            });
            Find.WindowStack.Add((Window)new Dialog_NodeTree(nodeRoot, false, false, (string)null));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref FinalDestructionSite.playerWon, "playerWon", false, false);
            Scribe_References.Look<FinalDestructionSite>(ref FinalDestructionSite.ActiveSite, "ActiveSite", false);
        }

        private bool AnyHostileOnMap(Map map, Faction enemyFaction)
        {
            List<Pawn> list = map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(p =>
            {
                if (p.Faction == enemyFaction && !p.Dead)
                    return p.RaceProps.Humanlike;
                return false;
            })).ToList<Pawn>();
            return list != null && list.Count != 0;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(
          Caravan caravan)
        {
            foreach (FloatMenuOption floatMenuOption in CaravanArrivalActionUtility.GetFloatMenuOptions<CaravanArrivalAction_FinalDestruction>((Func<FloatMenuAcceptanceReport>)(() => this.caravanAction.CanVisit(caravan, this)), (Func<CaravanArrivalAction_FinalDestruction>)(() => this.caravanAction), "EnterMap".Translate((NamedArgument)this.Label), caravan, this.Tile, (WorldObject)this))
                yield return floatMenuOption;
        }
    }
}
