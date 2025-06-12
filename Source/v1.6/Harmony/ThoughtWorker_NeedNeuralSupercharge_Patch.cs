using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class ThoughtWorker_NeedNeuralSupercharge_Patch
    {
        // Artificial units do not care about neural supercharges.
        [HarmonyPatch(typeof(ThoughtWorker_NeedNeuralSupercharge), "ShouldHaveThought")]
        public class CurrentStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if (ABF_Utils.IsArtificialSapient(p))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}