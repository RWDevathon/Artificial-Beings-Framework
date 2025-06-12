﻿using Verse;
using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace ArtificialBeings
{
    public class ThoughtWorker_Precept_IdeoDiversity_Patch
    {
        // Drones don't care about ideological diversity. Other pawns don't care about the drones' lack of ideology.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_IdeoDiversity), "ShouldHaveThought")]
        public class TW_Precept_IdeoDiversity_ShouldHaveThought
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn p, ref ThoughtState __result, ThoughtWorker_Precept_IdeoDiversity __instance)
            {
                if (p.Faction == null || !p.IsColonist || ABF_Utils.IsArtificialDrone(p))
                {
                    __result = false;
                    return false;
                }
                int num = 0;
                int num2 = 0;
                foreach (Pawn pawn in p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction))
                {
                    if (!pawn.IsQuestLodger() && pawn.RaceProps.Humanlike && !pawn.IsSlave && !pawn.IsPrisoner && !ABF_Utils.IsArtificialDrone(pawn))
                    {
                        num2++;
                        if (pawn != p && pawn.Ideo != p.Ideo)
                            num++;
                    }
                }
                if (num == 0)
                {
                    __result = ThoughtState.Inactive;
                    return false;
                }
                __result = ThoughtState.ActiveAtStage(Mathf.RoundToInt((float)num / (float)(num2 - 1) * (float)(__instance.def.stages.Count - 1)));
                return false;
            }
        }
    }
}