using HarmonyLib;
using Verse;
using RimWorld;

namespace ArtificialBeings
{
    public static class TraderCaravanUtility_Patch
    {
        // Trade caravans only mark humanlike pawns with the Slave pawnkind as being for sale. This patch also marks pawns with the appropriate pawn kind extension as chattel.
        // Warning: This is an experimental patch, and should probably in future be made into a transpiler. HAR has one such transpiler.
        [HarmonyPatch(typeof(TraderCaravanUtility), "GetTraderCaravanRole")]
        public class TraderCaravanUtility_GetTraderCaravanRole_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref TraderCaravanRole __result)
            {
                if (__result == TraderCaravanRole.Guard && p.kindDef.GetModExtension<ABF_ArtificialPawnKindExtension>() is ABF_ArtificialPawnKindExtension ext)
                {
                    __result = ext.caravanRole;
                }
            }
        }
    }
}