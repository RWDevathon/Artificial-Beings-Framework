using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ArtificialBeings
{
    public class PawnGroupMakerUtility_Patch
    {
        // Reprogrammable drones that spawn as part of random factions should have random programming.
        // The randomness should take into account the pawn kind def and the type of group that is being made.
        [HarmonyPatch(typeof(PawnGroupMakerUtility), "GeneratePawns")]
        public class GeneratePawns_Patch
        {
            [HarmonyPostfix]
            public static void Listener(PawnGroupMakerParms parms, bool warnOnZeroResults, ref IEnumerable<Pawn> __result)
            {
                List<Pawn> modifiedResults = __result.ToList();
                foreach (Pawn member in modifiedResults)
                {
                    if (ABF_Utils.IsProgrammableDrone(member))
                    {
                        ABF_Utils.RandomizeProgrammableDrone(member, parms);
                    }
                }
                __result = modifiedResults;
            }
        }
    }
}
