using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    public class CompProperties_ResourceProducerArtificial : CompProperties
    {
        public CompProperties_ResourceProducerArtificial()
        {
            compClass = typeof(CompHasGatherableBodyResource_Artificial);
        }

        public float resourceIntervalDays;

        public int resourceCount;

        public ThingDef resourceDef;

        public List<NeedDef> pauseOnAnyEmpty = new List<NeedDef>();
    }
}
