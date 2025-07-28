using HarmonyLib;
using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class ThoughtWorker_Precept_Scarification_Social_Patch
    {
        // Artificial beings don't care about being scarred or having a lack of them. Others don't care about them being scarred or having a lack of them.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_Scarification_Social), "ShouldHaveThought")]
        public class ThoughtWorker_Precept_Scarification_Social_ShouldHaveThought
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn p, Pawn otherPawn, ref ThoughtState __result)
            {
                if (ABF_Utils.IsArtificial(otherPawn))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}