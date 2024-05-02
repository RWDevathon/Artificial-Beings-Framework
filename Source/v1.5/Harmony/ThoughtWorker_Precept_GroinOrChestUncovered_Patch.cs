using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class ThoughtWorker_Precept_GroinOrChestUncovered_Patch
    {
        // Artificial units don't have concerns about groins or chests.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_GroinOrChestUncovered), "HasUncoveredGroinOrChest")]
        public class TW_Precept_GroinUncovered_HasUncoveredGroinOrChest
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