using HarmonyLib;
using Verse;

namespace ArtificialBeings
{
    class Hediff_PorcupineQuill_Patch
    {
        // Artificial pawns should not suffer porcupine quills. It mostly just painful, so does nothing much to them anyway.
        [HarmonyPatch(typeof(Hediff_PorcupineQuill), nameof(Hediff_PorcupineQuill.PostAdd))]
        public static class Hediff_PorcupineQuill_PostAdd_Patch
        {
            [HarmonyPostfix]
            public static void Listener(DamageInfo? dinfo, Pawn ___pawn, Hediff __instance)
            {
                if (ABF_Utils.IsArtificial(___pawn))
                {
                    ___pawn.health.RemoveHediff(__instance);
                }
            }
        }
    }
}
