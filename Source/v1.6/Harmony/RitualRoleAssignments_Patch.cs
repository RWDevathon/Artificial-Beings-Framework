using Verse;
using HarmonyLib;
using RimWorld;
using System;

namespace ArtificialBeings
{
    public class RitualRoleAssignments_Patch
    {
        // Drones can not engage in rituals.
        [HarmonyPatch(typeof(RitualRoleAssignments), "PawnNotAssignableReason")]
        [HarmonyPatch(new Type[] { typeof(Pawn), typeof(RitualRole), typeof(Precept_Ritual), typeof(RitualRoleAssignments), typeof(TargetInfo), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref })]

        public class PawnNotAssignableReason_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, RitualRole role, ref string __result)
            {
                // If this pawn is invalid for some other reason, allow that to take priority.
                if (__result != null)
                    return;

                // Drones are not allowed to hold any roles in rituals.
                if (ABF_Utils.IsArtificialDrone(p))
                {
                    __result = "ABF_InsufficientIntelligence".Translate();
                }
            }
        }
    }
}