using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class CompProperties_AbilityGiveHediffsUserTarget : CompProperties_AbilityEffectWithDuration
    {
        public HediffDef hediffDefSelf;

        public HediffDef hediffDefTarget;

        public float severitySelf = -1f;

        public float severityTarget = -1f;
    }

}
