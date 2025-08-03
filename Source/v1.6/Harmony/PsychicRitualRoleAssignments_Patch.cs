using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace ArtificialBeings
{
    public class PsychicRitualRoleAssignments_Patch
    {
        // Drones can not engage in rituals unless specifically permitted via def extension.
        [HarmonyPatch(typeof(PsychicRitualRoleAssignments), "PawnNotAssignableReason")]

        public class PsychicRitualRoleAssignments_PawnNotAssignableReason_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, PsychicRitualRoleDef role, ref string __result)
            {
                // If this pawn is invalid for some other reason, allow that to take priority.
                if (__result != null)
                    return;

                // Drones are not normally allowed to hold any roles in rituals.
                if (ABF_Utils.IsArtificialDrone(pawn) && pawn.def.GetModExtension<ABF_ArtificialPawnExtension>()?.allowedDronePsychicRitualParticipation?.Contains(role) != true)
                {
                    __result = "ABF_InsufficientIntelligence".Translate();
                }  
            }
        }
    }
}