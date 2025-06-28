using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ArtificialBeings
{
    public class CompHasGatherableBodyResource_Artificial : CompHasGatherableBodyResource
    {
        public CompProperties_ResourceProducerArtificial Props => (CompProperties_ResourceProducerArtificial)props;

        // This shouldn't actually be used, as the normal methods using it are overriden, but it's necessary due to the abstract base class.
        protected override int GatherResourcesIntervalDays => Mathf.RoundToInt(Props.resourceIntervalDays);

        protected override int ResourceAmount => Props.resourceCount;

        protected override ThingDef ResourceDef => Props.resourceDef;

        protected override string SaveKey => "ABF_resourceFullness";

        // Cheeky convenience method just for enabling god mode to fill up this need instantly.
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action makeFull = new Command_Action
                {
                    defaultLabel = "DEV: Make Full",
                    action = delegate
                    {
                        fullness = 1f;
                    }
                };
                yield return makeFull;
            }
            yield break;
        }

        // Base class runs on CompTicks, which we don't want. We want to use 1.6's fancy tick intervals.
        public override void CompTick()
        {
        }

        public override void CompTickInterval(int delta)
        {
            if (Active && fullness < 1f)
            {
                // We want to run our pause check separately from Active, so that pawns can still harvest products if it is full even if paused.
                foreach (NeedDef needDef in Props.pauseOnAnyEmpty)
                {
                    if (parent is Pawn pawn && pawn.needs.AllNeeds.Any(need => need.def == needDef && need.CurInstantLevelPercentage <= 0))
                    {
                        return;
                    }
                }
                fullness = Mathf.Clamp01(fullness + delta / (Props.resourceIntervalDays * GenDate.TicksPerDay));
            }
        }

        public override string CompInspectStringExtra()
        {
            string output = base.CompInspectStringExtra();
            if (output != null)
            {
                return output;
            }
            foreach (NeedDef needDef in Props.pauseOnAnyEmpty)
            {
                if (parent is Pawn pawn && pawn.needs.AllNeeds.Any(need => need.def == needDef && need.CurInstantLevelPercentage <= 0))
                {
                    return "ABF_PausedNeedEmpty".Translate();
                }
            }
            return "ABF_ResourceReadout".Translate(ResourceDef.LabelCap, Fullness.ToStringPercent());
        }
    }
}
