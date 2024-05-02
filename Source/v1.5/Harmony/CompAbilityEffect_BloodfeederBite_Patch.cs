using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class CompAbilityEffect_BloodfeederBite_Patch
    {
        // Artificial units are invalid targets for blood feeding.
        [HarmonyPatch(typeof(CompAbilityEffect_BloodfeederBite), "Valid")]
        public class Valid_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, LocalTargetInfo target)
            {
                __result = __result && !ABF_Utils.IsArtificial(target.Pawn);
            }
        }
    }
}