using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class InteractionUtility_Patch
    {
        // Artificial drones don't start social interactions unless they have social capabilities.
        [HarmonyPatch(typeof(SocialInteractionUtility), "CanInitiateInteraction")]
        public class SocialInteractionUtility_CanInitiateInteraction_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn, ref bool __result, InteractionDef interactionDef = null)
            {
                if (ABF_Utils.IsArtificialDrone(pawn))
                {
                    if (pawn.RaceProps.intelligence == Intelligence.Animal || (pawn.skills is Pawn_SkillTracker skills && (!skills.GetSkill(SkillDefOf.Social).TotallyDisabled || !skills.GetSkill(SkillDefOf.Animals).TotallyDisabled)))
                    {
                        return true;
                    }
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        // Drones may never initiate random social interactions - these are different than ordered/work-related interactions.
        [HarmonyPatch(typeof(SocialInteractionUtility), "CanInitiateRandomInteraction")]
        public class SocialInteractionUtility_CanInitiateRandomInteraction_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn p, ref bool __result)
            {
                if (ABF_Utils.IsArtificialDrone(p) && p.RaceProps.intelligence > Intelligence.Animal)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        // Drones don't receive social interactions.
        [HarmonyPatch(typeof(SocialInteractionUtility), "CanReceiveInteraction")]
        public class SocialInteractionUtility_CanReceiveInteraction_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn, ref bool __result, InteractionDef interactionDef = null)
            {
                if (ABF_Utils.IsArtificialDrone(pawn) && pawn.RaceProps.intelligence > Intelligence.Animal)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}