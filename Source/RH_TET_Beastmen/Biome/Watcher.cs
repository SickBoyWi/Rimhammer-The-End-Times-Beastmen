using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TheEndTimes_Beastmen
{
    public class Watcher : MapComponent
    {
        private bool regenCellLists = true;

        public Watcher(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
        }
        
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref this.regenCellLists, "regenCellLists", true, true);
        }
        
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            this.rebuildCellLists();
        }
        
        #region runs on setup

        public void rebuildCellLists()
        {
            Rot4 rot = Find.World.CoastDirectionAt(map.Tile);

            if (this.regenCellLists)
            {
                IEnumerable<IntVec3> tmpTerrain = map.AllCells.InRandomOrder();
                foreach (IntVec3 c in tmpTerrain)
                {
                    this.SpawnSpecialElements(c);
                }
                
                this.regenCellLists = false;
            }
        }

        public void SpawnSpecialElements(IntVec3 c)
        {
            TerrainDef terrain = c.GetTerrain(map);

            foreach (ElementSpawnDef element in DefDatabase<ElementSpawnDef>.AllDefs)
            {
                bool canSpawn = true;
                foreach (string biome in element.forbiddenBiomes)
                {
                    if (map.Biome.defName == biome)
                    {
                        canSpawn = false;
                        break;
                    }
                }
                
                foreach (string biome in element.allowedBiomes)
                {
                    if (map.Biome.defName != biome)
                    {
                        canSpawn = false;
                        break;
                    }
                }

                if (!canSpawn)
                {
                    continue;
                }

                foreach (string allowed in element.terrainValidationAllowed)
                {
                    if (terrain.defName == allowed)
                    {
                        canSpawn = true;
                        break;
                    }
                }

                foreach (string notAllowed in element.terrainValidationAllowed)
                {
                    if (terrain.HasTag(notAllowed))
                    {
                        canSpawn = false;
                        break;
                    }
                }
                
                if (canSpawn && Rand.Value < .0005f)
                {
                    Thing thing = (Thing)ThingMaker.MakeThing(element.thingDef, null);
                    GenSpawn.Spawn(thing, c, map);
                }
            }
        }

        #endregion
    }
}
