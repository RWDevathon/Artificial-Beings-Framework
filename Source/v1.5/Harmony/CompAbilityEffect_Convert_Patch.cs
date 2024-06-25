using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class CompAbilityEffect_Convert_Patch
    {
        // Pawns that are considered non-humanlike intelligence are not valid targets for attempted Ideological conversions.
        [HarmonyPatch(typeof(CompAbilityEffect_Convert), "Valid")]
        public class Valid_Patch
        {
            [HarmonyPostfix]
            public static bool Listener(bool __result, LocalTargetInfo target)
            {
                return __result && !ABF_Utils.IsArtificialDrone(target.Pawn);
            }
        }
    }
}