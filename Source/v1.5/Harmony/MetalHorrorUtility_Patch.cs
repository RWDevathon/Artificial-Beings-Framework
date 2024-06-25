using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class MetalHorrorUtility_Patch
    {
        // Artificial beings are immune to metal horror infections.
        [HarmonyPatch(typeof(MetalhorrorUtility), "CanBeInfected")]
        public class MetalhorrorUtility_CanBeInfected_Patch
        {
            [HarmonyPostfix]
            public static bool Listener(bool __result, Pawn pawn)
            {
                return __result && !ABF_Utils.IsArtificial(pawn);
            }
        }
    }
}