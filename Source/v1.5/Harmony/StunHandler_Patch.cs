using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class StunHandler_Patch
    {
        // Artificial units are vulnerable to EMP.
        [HarmonyPatch(typeof(StunHandler), "CanBeStunnedByDamage")]
        public class StunHandler_CanBeStunnedByDamage_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, Thing ___parent)
            {
                // No need to do any checks if it is already true.
                if (__result)
                    return;

                if (___parent is Pawn pawn && pawn.def.GetModExtension<ABF_ArtificialPawnExtension>()?.vulnerableToEMP == true)
                {
                    __result = true;
                }
            }
        }
    }
}