using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class JobGiver_GetHemogen_Patch
    {
        // Pawns which cannot bleed (because they have no blood or are immune to blood loss at the moment) are invalid for blood feeding.
        [HarmonyPatch(typeof(JobGiver_GetHemogen), "CanFeedOnPrisoner")]
        public class CanFeedOnPrisoner_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref AcceptanceReport __result, Pawn prisoner)
            {
                if (!prisoner.health.CanBleed)
                {
                    __result = "ABF_TargetHasNoBlood".Translate(prisoner.LabelShortCap);
                    return false;
                }
                return true;
            }
        }
    }
}