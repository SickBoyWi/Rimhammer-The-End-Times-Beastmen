using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Beastmen
{
    public class PlaceWorker_FightingPitRadius : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            CellRect cellRect1 = CellRect.CenteredOn(center, 1);
            cellRect1 = cellRect1.ExpandedBy(Mathf.RoundToInt(3f));
            CellRect cellRect2 = cellRect1.ExpandedBy(1);
            GenDraw.DrawFieldEdges(cellRect1.Cells.ToList<IntVec3>(), Color.white);
            GenDraw.DrawFieldEdges(cellRect2.Cells.ToList<IntVec3>(), Color.gray);
        }
    }
}
