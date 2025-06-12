using Verse;
using HarmonyLib;

namespace ArtificialBeings
{
    public class GasUtility_Patch
    {
        // Artificial units are not affected by gas exposure.
        [HarmonyPatch(typeof(GasUtility), "IsAffectedByExposure")]
        public class IsAffectedByExposure_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Pawn pawn)
            {
                __result = !ABF_Utils.IsArtificial(pawn);
                return __result;
            }
        }
    }
}