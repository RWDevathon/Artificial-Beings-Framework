using Verse;

namespace ArtificialBeings
{
    // ModExtension containing information for RecipeDefs to inform Recipe_RestorePart how to behave.
    public class ABF_RestorePartExtension : DefModExtension
    {
        // How much HP to restore to damaged/destroyed parts, or how much severity of other conditions (multiplied by 10) to reduce.
        public float severityToRestore = 100f;

        // Optional flag to simply restore the selected part and all sub-parts, without caring about severity.
        public bool fullRestore = false;

        // Should leftover HP for restoration propagate back to the core part to potentially fix other problems?
        public bool propagateUpwards = false;
    }
}