
using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class JobDriver_GatherArtificialAnimalBodyResources : JobDriver_GatherAnimalBodyResources
    {
        protected override float WorkTotal => 800f;

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompHasGatherableBodyResource_Artificial>();
        }
    }
}
