using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace ArtificialBeings
{
    public class WorkGiver_PatientGoToBedTreatment_Patch
    {
        // Artificial units need to check if there is an artificer available, not if there is a doctor available, when seeking treatment.
        [HarmonyPatch(typeof(WorkGiver_PatientGoToBedTreatment), "AnyAvailableDoctorFor")]
        public class AnyAvailableDoctorFor_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref bool __result)
            {
                // Only override the method for artificial pawns.
                if (!ABF_Utils.IsArtificial(pawn))
                    return;

                // Don't worry about checks for map-less pawns. Vanilla behavior can handle that case.
                Map mapHeld = pawn.MapHeld;
                if (mapHeld == null)
                {
                    return;
                }

                // Attempt to locate an available artificer in the faction.
                List<Pawn> list = mapHeld.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
                for (int i = 0; i < list.Count; i++)
                {
                    Pawn target = list[i];
                    if (target != pawn && (target.RaceProps.Humanlike || target.IsColonyMechPlayerControlled) && !target.Downed && target.Awake() && !target.InBed() && !target.InMentalState && !target.IsPrisoner && target.workSettings != null && target.workSettings.EverWork && target.workSettings.WorkIsActive(ABF_WorkTypeDefOf.ABF_Artificer) && target.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && target.CanReach(pawn, PathEndMode.Touch, Danger.Deadly))
                    {
                        __result = true;
                        return;
                    }
                }

                // If no artificer was found for a artificial unit, even if there is a doctor available, set the result to false.
                __result = false;
            }
        }
    }
}