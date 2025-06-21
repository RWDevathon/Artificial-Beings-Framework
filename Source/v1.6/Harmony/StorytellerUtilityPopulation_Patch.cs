using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class StorytellerUtilityPopulation_Patch
    {
        // Drones may be counted differently for population purposes.
        [HarmonyPatch(typeof(StorytellerUtilityPopulation), "AdjustedPopulationValue")]
        public class StorytellerUtilityPopulation_AdjustedPopulationValue_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref float __result, Pawn pawn)
            {
                if (__result == 0)
                {
                    return;
                }

                if (ABF_Utils.IsArtificialDrone(pawn))
                {
                    __result *= pawn.def.GetModExtension<ABF_ArtificialPawnExtension>()?.dronePopulationFactor ?? 1f;
                }
            }
        }
    }
}