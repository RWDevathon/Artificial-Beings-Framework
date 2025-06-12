using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class FoodUtility_Patch
    {
        // Artificial units can not be food poisoned, so set their chance to receive food poisoning to zero.
        [HarmonyPatch(typeof(FoodUtility), "GetFoodPoisonChanceFactor")]
        public class GetFoodPoisonChanceFactor_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn ingester, ref float __result)
            {
                if (ABF_Utils.IsArtificial(ingester))
                {
                    __result = 0f;
                    return false;
                }
                return true;
            }
        }
    }
}