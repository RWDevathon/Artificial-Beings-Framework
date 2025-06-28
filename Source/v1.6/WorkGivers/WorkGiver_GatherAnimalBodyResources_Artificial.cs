using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class WorkGiver_GatherAnimalBodyResources_Artificial : WorkGiver_GatherAnimalBodyResources
    {
        protected override JobDef JobDef => ABF_JobDefOf.ABF_Job_Artificial_GatherArtificialAnimalBodyResources;

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompHasGatherableBodyResource_Artificial>();
        }
    }
}