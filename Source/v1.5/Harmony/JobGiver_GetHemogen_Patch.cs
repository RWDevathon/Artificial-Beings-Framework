using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class JobGiver_GetHemogen_Patch
    {
        // Artificial units are invalid targets for blood feeding.
        [HarmonyPatch(typeof(JobGiver_GetHemogen), "CanFeedOnPrisoner")]
        public class CanFeedOnPrisoner_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref AcceptanceReport __result, Pawn prisoner)
            {
                if (ABF_Utils.IsArtificial(prisoner))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}