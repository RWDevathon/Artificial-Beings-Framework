using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class CompAbilityEffect_BloodfeederBite_Patch
    {
        // Pawns which cannot bleed (because they have no blood or are immune to blood loss at the moment) are invalid for blood feeding.
        [HarmonyPatch(typeof(CompAbilityEffect_BloodfeederBite), "Valid")]
        public class Valid_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, LocalTargetInfo target, bool throwMessages)
            {
                if (!__result)
                {
                    return;
                }

                if (!target.Pawn.health.CanBleed)
                {
                    __result = false;
                    if (throwMessages)
                    {
                        Messages.Message("ABF_TargetHasNoBlood".Translate(target.Pawn.LabelShortCap), target.Pawn, MessageTypeDefOf.RejectInput, historical: false);
                    }
                }
                __result = __result && !ABF_Utils.IsArtificial(target.Pawn);
            }
        }
    }
}