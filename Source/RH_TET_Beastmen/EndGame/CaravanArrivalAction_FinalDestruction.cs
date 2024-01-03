using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Beastmen
{
    public class CaravanArrivalAction_FinalDestruction : CaravanArrivalAction_EnterMap
    {
        public static List<IntVec3> PLAYER_ENTER_LOCS = new List<IntVec3>();

        private IntVec3 PLAYER_ENTER_LOC1 = new IntVec3(10, 0, 235);
        private IntVec3 PLAYER_ENTER_LOC2 = new IntVec3(9, 0, 8);
        private IntVec3 PLAYER_ENTER_LOC3 = new IntVec3(232, 0, 12);
        private IntVec3 PLAYER_ENTER_LOC4 = new IntVec3(236, 0, 242);

        public override IntVec3 MapSize
        {
            get
            {
                return new IntVec3(250, 1, 250);
            }
        }

        public CaravanArrivalAction_FinalDestruction()
        {
            PLAYER_ENTER_LOCS.Add(PLAYER_ENTER_LOC1);
            PLAYER_ENTER_LOCS.Add(PLAYER_ENTER_LOC2);
            PLAYER_ENTER_LOCS.Add(PLAYER_ENTER_LOC3);
            PLAYER_ENTER_LOCS.Add(PLAYER_ENTER_LOC4);
        }

        public CaravanArrivalAction_FinalDestruction(MapParent mapParent)
          : base(mapParent)
        {
            PLAYER_ENTER_LOCS.Add(PLAYER_ENTER_LOC1);
            PLAYER_ENTER_LOCS.Add(PLAYER_ENTER_LOC2);
            PLAYER_ENTER_LOCS.Add(PLAYER_ENTER_LOC3);
            PLAYER_ENTER_LOCS.Add(PLAYER_ENTER_LOC4);
        }

        public override FloatMenuAcceptanceReport CanVisit(
          Caravan caravan,
          MapParent mapParent)
        {
            if (mapParent == null || !mapParent.Spawned)
                return (FloatMenuAcceptanceReport)false;
            return (FloatMenuAcceptanceReport)true;
        }

        public override Map GetOrGenerateMap(
          int tile,
          IntVec3 mapSize,
          WorldObjectDef suggestedMapParentDef)
        {
            return Current.Game.FindMap(tile) ?? Verse.MapGenerator.GenerateMap(mapSize, this.MapParent, BeastmenDefOf.RH_TET_Beastmen_EmptyMap, (IEnumerable<GenStepWithParams>)null, (Action<Map>)null);
        }

        public override void DoEnter(Caravan caravan)
        {
            Pawn pawn = caravan.PawnsListForReading[0];
            int num = !this.MapParent.HasMap ? 1 : 0;
            Map orGenerateMap = this.GetOrGenerateMap(this.MapParent.Tile, this.MapSize, (WorldObjectDef)null);
            Map map = orGenerateMap;
            IntVec3 enterPos = PLAYER_ENTER_LOCS.RandomElement<IntVec3>();
            CaravanEnterMapUtility.Enter(caravan, map, (Func<Pawn, IntVec3>)(p => enterPos), CaravanDropInventoryMode.DoNotDrop, true);
            FinalDestructionComp component = this.MapParent.GetComponent<FinalDestructionComp>();

            if (!FinalDestructionSite.playerWon)
            {
                Log.Clear();
                Faction f = GetEnemyFaction();
                this.SetMapSpecificJobs(map.mapPawns.SpawnedPawnsInFaction(GetEnemyFaction()), f, map);
            }
        }

        private Faction GetEnemyFaction()
        {
            Faction f = Find.FactionManager.FirstFactionOfDef(BeastmenDefOf.RH_TET_EmpireOfMan);
            return f;
        }

        private void SetMapSpecificJobs(List<Pawn> pawnList, Faction f, Map map)
        {
            List<IntVec3> locs = new List<IntVec3>();
            List<IntVec3> locs2 = new List<IntVec3>();
            List<IntVec3> locs3 = new List<IntVec3>();
            List<IntVec3> locs4 = new List<IntVec3>();

            // LL
            locs.Add(new IntVec3(85, 0, 67));
            locs.Add(new IntVec3(76, 0, 57));
            locs.Add(new IntVec3(76, 0, 65));
            locs.Add(new IntVec3(82, 0, 57));
            locs.Add(new IntVec3(108, 0, 108));

            // UL
            locs2.Add(new IntVec3(85, 0, 166));
            locs2.Add(new IntVec3(77, 0, 172));
            locs2.Add(new IntVec3(76, 0, 165));
            locs2.Add(new IntVec3(84, 0, 174));
            locs2.Add(new IntVec3(139, 0, 72));

            // LR
            locs3.Add(new IntVec3(186, 0, 66));
            locs3.Add(new IntVec3(194, 0, 57));
            locs3.Add(new IntVec3(186, 0, 57));
            locs3.Add(new IntVec3(194, 0, 65));

            // UR
            locs3.Add(new IntVec3(193, 0, 173));
            locs3.Add(new IntVec3(194, 0, 166));
            locs3.Add(new IntVec3(186, 0, 174));
            locs3.Add(new IntVec3(185, 0, 164));
            locs3.Add(new IntVec3(91, 0, 150));

            List<Pawn> turretPawns = new List<Pawn>();
            List<Pawn> turretPawns2 = new List<Pawn>();
            List<Pawn> turretPawns3 = new List<Pawn>();
            List<Pawn> turretPawns4 = new List<Pawn>();

            foreach (Pawn p in pawnList)
            {
                if (locs.Contains(p.Position))
                {
                    turretPawns.Add(p);
                }
                else if (locs2.Contains(p.Position))
                {
                    turretPawns2.Add(p);
                }
                else if (locs3.Contains(p.Position))
                {
                    turretPawns3.Add(p);
                }
                else if (locs4.Contains(p.Position))
                {
                    turretPawns4.Add(p);
                }
            }

            foreach (Pawn pawn in turretPawns)
                pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord, new DamageInfo?());
            Lord lord = LordMaker.MakeNewLord(f, (LordJob)new LordJob_ManTurrets(), map, turretPawns);

            foreach (Pawn pawn in turretPawns2)
                pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord, new DamageInfo?());
            Lord lord2 = LordMaker.MakeNewLord(f, (LordJob)new LordJob_ManTurrets(), map, turretPawns2);

            foreach (Pawn pawn in turretPawns3)
                pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord, new DamageInfo?());
            Lord lord3 = LordMaker.MakeNewLord(f, (LordJob)new LordJob_ManTurrets(), map, turretPawns3);

            foreach (Pawn pawn in turretPawns4)
                pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord, new DamageInfo?());
            Lord lord4 = LordMaker.MakeNewLord(f, (LordJob)new LordJob_ManTurrets(), map, turretPawns4);

            foreach (Pawn pawn in turretPawns)
            {
                if (pawn.Spawned)
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            }
            foreach (Pawn pawn in turretPawns2)
            {
                if (pawn.Spawned)
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            }
            foreach (Pawn pawn in turretPawns3)
            {
                if (pawn.Spawned)
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            }
            foreach (Pawn pawn in turretPawns4)
            {
                if (pawn.Spawned)
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            }
        }
    }
}
