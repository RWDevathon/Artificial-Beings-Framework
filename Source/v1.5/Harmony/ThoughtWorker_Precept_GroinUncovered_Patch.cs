using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class ThoughtWorker_Precept_GroinUncovered_Patch
    {
        // Artificial units don't care about uncovered groins.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_GroinUncovered), "HasUncoveredGroin")]
        public class TW_Precept_GroinUncovered_HasUncoveredGroin
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref bool __result)
            {
                if (!__result)
                    return;

                if (ABF_Utils.IsArtificialSapient(p))
                {
                    __result = false;
                }
            }
        }
    }
}