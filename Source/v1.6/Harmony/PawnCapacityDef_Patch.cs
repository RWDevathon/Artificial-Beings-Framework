using Verse;
using HarmonyLib;
using System;

namespace ArtificialBeings
{
    public class PawnCapacityDef_Patch
    {
        // Artificial Beings should use mechanoid labels for capacities rather than organic labels, for flavor.
        [HarmonyPatch(typeof(PawnCapacityDef), "GetLabelFor")]
        [HarmonyPatch(new Type[] { typeof(Pawn) })]
        public class PawnCapacityDef_GetLabelFor_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn, ref string __result, string ___labelMechanoids)
            {
                if (ABF_Utils.IsArtificial(pawn) && ___labelMechanoids != "")
                {
                    __result = ___labelMechanoids;
                    return false;
                }
                return true;
            }
        }
    }
}