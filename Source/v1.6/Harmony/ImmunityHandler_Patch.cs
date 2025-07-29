using HarmonyLib;
using Verse;

namespace ArtificialBeings
{
    public static class ImmunityHandler_Patch
    {
        // Artificial beings are immune to disease. They do not need to tick the immunity handler.
        [HarmonyPatch(typeof(ImmunityHandler), "ImmunityHandlerTickInterval")]
        public class ImmunityHandler_ImmunityHandlerTickInterval_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(int delta, Pawn ___pawn)
            {
                if (ABF_Utils.IsArtificial(___pawn))
                {
                    return false;
                }
                return true;
            }
        }

        // Artificial beings are immune to disease. They are instantly immune to anything that they can be immune to.
        [HarmonyPatch(typeof(ImmunityHandler), nameof(ImmunityHandler.GetImmunity))]
        public class ImmunityHandler_GetImmunity_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(HediffDef def, Pawn ___pawn, ref float __result, bool naturalImmunityOnly = false)
            {
                if (ABF_Utils.IsArtificial(___pawn))
                {
                    __result = 1f;
                    return false;
                }
                return true;
            }
        }
    }
}