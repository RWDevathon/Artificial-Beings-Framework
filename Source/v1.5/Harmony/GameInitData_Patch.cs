using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using AlienRace;

namespace ArtificialBeings
{
    public class GameInitData_Patch
    {
        // The ScenPart_StartingDrones inserts its drones just prior to map generation, as there is no configuration to be done for them.
        [HarmonyPatch(typeof(GameInitData), "PrepForMapGen")]
        public class GameInitData_PrepForMapGen_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref GameInitData __instance)
            {
                foreach (ScenPart part in Find.Scenario.AllParts)
                {
                    if (__instance.startingPawnCount < 0)
                    {
                        __instance.startingPawnCount = 0;
                    }

                    if (part is ScenPart_StartingDrones scenPart_startingDrones)
                    {
                        List<Pawn> pawns = scenPart_startingDrones.GetDrones().ToList();
                        __instance.startingAndOptionalPawns.InsertRange(__instance.startingPawnCount, pawns);
                        __instance.startingPawnCount += pawns.Count;

                        foreach (Pawn pawn in pawns)
                        {
                            CachedData.generateStartingPossessions(pawn);
                        }
                    }
                }
                return true;
            }
        }
    }
}