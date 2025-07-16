using HarmonyLib;
using RimWorld;
using Verse;

namespace ArtificialBeings
{
    class CompPorcupine_Patch
    {
        // Artificial pawns should not suffer porcupine quills. It mostly just painful, so does nothing much to them anyway.
        [HarmonyPatch(typeof(CompPorcupine), nameof(CompPorcupine.PostPostApplyDamage))]
        public static class CompPorcupine_PostPostApplyDamage_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(DamageInfo dinfo, float totalDamageDealt)
            {
                if (dinfo.Instigator is Pawn pawn && ABF_Utils.IsArtificial(pawn))
                {
                    return false;
                }
                return true;

            }
        }
    }
}
