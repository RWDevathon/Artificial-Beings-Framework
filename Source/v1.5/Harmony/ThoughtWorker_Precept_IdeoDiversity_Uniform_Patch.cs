using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace ArtificialBeings
{
    public class ThoughtWorker_Precept_IdeoDiversity_Uniform_Patch
    {
        // Artificial drones don't care about ideological diversity. Other pawns don't care about the drones' lack of ideology.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_IdeoDiversity_Uniform), "ShouldHaveThought")]
        public class TW_Precept_IdeoUniform_ShouldHaveThought
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn p, ref ThoughtState __result)
            {
                if (p.Faction == null || !p.IsColonist || ABF_Utils.IsArtificialDrone(p))
                {
                    __result = false;
                    return false;
                }
                List<Pawn> list = p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction);
                int num = 0;
                foreach (Pawn pawn in p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction))
                {
                    if (!pawn.IsQuestLodger() && pawn.RaceProps.Humanlike && !pawn.IsSlave && !pawn.IsPrisoner && !ABF_Utils.IsArtificialDrone(pawn))
                    {
                        if (pawn.Ideo != p.Ideo)
                        {
                            __result = false;
                            return false;
                        }
                        num++;
                    }
                }

                __result = num > 0;
                return false;
            }
        }
    }
}