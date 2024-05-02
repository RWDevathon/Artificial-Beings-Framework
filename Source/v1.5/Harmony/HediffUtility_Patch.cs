using Verse;
using HarmonyLib;

namespace ArtificialBeings
{
    public class HediffUtility_Patch
    {
        // Artificial units are effectively all implants (bionics for organics usually), so don't even bother calculating for them.
        [HarmonyPatch(typeof(HediffUtility), "CountAddedAndImplantedParts")]
        public class CountAddedParts_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(HediffSet hs, ref int __result)
            {
                if (ABF_Utils.IsArtificial(hs.pawn))
                {
                    __result = 50;
                    return false;
                }
                return true;
            }
        }
    }
}