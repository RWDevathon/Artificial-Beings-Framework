using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class StatPart_AgeOffset_Patch
    {
        // Age as a StatPart is not used for artificial units. This resolves issues around "baby" artificial units with reduced work speed, and other related issues.
        [HarmonyPatch(typeof(StatPart_AgeOffset), "ActiveFor")]
        public class ActiveFor_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn, ref bool __result)
            {
                if (ABF_Utils.IsArtificial(pawn))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}