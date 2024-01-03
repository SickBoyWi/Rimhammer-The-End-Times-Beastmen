﻿using RimWorld;
using System;
using Verse;
using System.Collections.Generic;

namespace TheEndTimes_Beastmen
{
    public class ElementSpawnDef : Def
    {
        public ThingDef thingDef;
        public bool allowOnWater;
        public List<string> terrainValidationAllowed;
        public List<string> terrainValidationDisallowed;
        public List<string> forbiddenBiomes;
        public List<string> allowedBiomes;
    }
}
