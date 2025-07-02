using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class Pawn_TrainingTracker_Patch
    {
        // Artificial animals never lose their training (if/when they have any), so just don't run the method that makes it happen.
        [HarmonyPatch(typeof(Pawn_TrainingTracker), "TrainingTrackerTickRare")]
        public class Pawn_TrainingTracker_TrainingTrackerTickRare_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn ___pawn)
            {
                return !ABF_Utils.IsArtificial(___pawn);
            }
        }
    }
}