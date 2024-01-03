using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    [StaticConstructorOnStartup]
    public class CompFightingPit : ThingComp
    {
        private static List<IntVec3> validCells = new List<IntVec3>();
        public float radius = 3f;

        public void decreaseRad()
        {
            this.radius = Mathf.Max(1.9f, this.radius - 1f);
        }

        public void increaseRad()
        {
            this.radius = Mathf.Min(9.9f, this.radius + 1f);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.radius, "radius", 9.9f, false);
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            Map currentMap = Find.CurrentMap;
            CellRect rect1 = CellRect.CenteredOn(this.parent.Position, 1);
            rect1 = rect1.ExpandedBy(Mathf.RoundToInt(this.radius));
            CellRect rect2 = rect1.ContractedBy(1);
            GenDraw.DrawFieldEdges(this.ValidCellsAround(this.parent.Position, currentMap, rect1), Color.gray);
            GenDraw.DrawFieldEdges(this.ValidCellsAround(this.parent.Position, currentMap, rect2));
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                Gizmo baseGizmo = gizmo;
                yield return baseGizmo;
                baseGizmo = (Gizmo)null;
            }
            Command_Action commandAction1 = new Command_Action();
            commandAction1.action = new Action(this.increaseRad);
            commandAction1.defaultLabel = "Increase radius";
            commandAction1.defaultDesc = "Increase pit fight area radius";
            commandAction1.hotKey = KeyBindingDefOf.Misc5;
            commandAction1.icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_IncreaseRadius", true);
            yield return (Gizmo)commandAction1;
            Command_Action commandAction2 = new Command_Action();
            commandAction2.action = new Action(this.decreaseRad);
            commandAction2.defaultLabel = "Decrease radius";
            commandAction2.defaultDesc = "Decrease pit fight area radius";
            commandAction2.hotKey = KeyBindingDefOf.Misc5;
            commandAction2.icon = ContentFinder<Texture2D>.Get("UI/RH_TET_Beastmen_DecreaseRadius", true);
            yield return (Gizmo)commandAction2;
        }

        public IEnumerable<IntVec3> ValidCells
        {
            get
            {
                CellRect cellRect = CellRect.CenteredOn(this.parent.Position, 1);
                cellRect = cellRect.ExpandedBy(Mathf.RoundToInt(this.radius));
                return (IEnumerable<IntVec3>)this.ValidCellsAround(this.parent.Position, this.parent.Map, cellRect.ContractedBy(1));
            }
        }

        public List<IntVec3> ValidCellsAround(IntVec3 pos, Map map, CellRect rect)
        {
            CompFightingPit.validCells.Clear();
            if (!pos.InBounds(map))
                return CompFightingPit.validCells;
            Region region = pos.GetRegion(map, RegionType.Set_Passable);
            if (region == null)
                return CompFightingPit.validCells;
            RegionTraverser.BreadthFirstTraverse(region, (RegionEntryPredicate)((from, r) => r.door == null), (RegionProcessor)(r =>
            {
                foreach (IntVec3 cell in r.Cells)
                {
                    if (this.InDistOfRect(cell, pos, rect))
                        CompFightingPit.validCells.Add(cell);
                }
                return false;
            }), 13, RegionType.Set_Passable);
            return CompFightingPit.validCells;
        }

        public bool InDistOfRect(IntVec3 pos, IntVec3 otherLoc, CellRect rect)
        {
            float x = (float)pos.x;
            float z = (float)pos.z;
            return (double)x <= (double)rect.maxX && (double)x >= (double)rect.minX && ((double)z <= (double)rect.maxZ && (double)z >= (double)rect.minZ);
        }
    }
}
