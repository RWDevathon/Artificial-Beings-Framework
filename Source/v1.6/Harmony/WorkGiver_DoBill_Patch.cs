﻿using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;

namespace ArtificialBeings
{
    public class WorkGiver_DoBill_Patch
    {
        // Listen for doctors/artificers doing a work bill, and make sure they select an appropriate medicine for their task.
        [HarmonyPatch(typeof(WorkGiver_DoBill), "AddEveryMedicineToRelevantThings")]
        public class AddEveryMedicineToRelevantThings_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, Thing billGiver, ref List<Thing> relevantThings, Predicate<Thing> baseValidator, Map map)
            {
                if (billGiver is Pawn patient)
                {
                    relevantThings.RemoveAll(thing => !(ABF_Utils.cachedRaceMedicines.TryGetValue(patient.def, null)?.Contains(thing.def) ?? ABF_Utils.cachedRaceMedicines.TryGetValue(ThingDefOf.Human, null)?.Contains(thing.def) ?? true));
                }
            }
        }

        // Forbid doctors from working on artificial pawns and artificers from working on organics.
        [HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
        public class JobOnThing_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, Thing thing, bool forced, ref Job __result, WorkGiver_DoBill __instance)
            {
                if (__result == null)
                {
                    return;
                }
                else if (__instance.def.workType == WorkTypeDefOf.Doctor && thing is Pawn patient && ABF_Utils.IsArtificial(patient))
                {
                    __result = null;
                }
                else if (__instance.def.workType == ABF_WorkTypeDefOf.ABF_WorkType_Artificial_Artificer && thing is Pawn unit && !ABF_Utils.IsArtificial(unit))
                {
                    __result = null;
                }
            }
        }
    }
}