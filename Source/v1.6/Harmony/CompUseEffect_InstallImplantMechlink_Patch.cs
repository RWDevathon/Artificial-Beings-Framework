using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class CompUseEffect_InstallImplantMechlink_Patch
    {
        // Non-sapient artificial units may not have mechlinks installed - it can possibly generate errors!
        [HarmonyPatch(typeof(CompUseEffect_InstallImplantMechlink), "CanBeUsedBy")]
        public class CompUseEffect_InstallImplantMechlink__CanBeUsedBy_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref AcceptanceReport __result, Pawn p)
            {
                if (!__result)
                {
                    return;
                }

                if (ABF_Utils.IsConsideredNonHumanlike(p))
                {
                    __result = "ABF_PawnTypeAutonomousTooltip".Translate();
                    return;
                }
            }
        }
    }
}