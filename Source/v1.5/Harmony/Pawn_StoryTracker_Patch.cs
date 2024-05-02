using HarmonyLib;
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace ArtificialBeings
{
    public static class Pawn_StoryTracker_Patch
    {
        // Programmable drones have their disabled work tags identified through their programming comp.
        [HarmonyPatch(typeof(Pawn_StoryTracker), "get_DisabledWorkTagsBackstoryTraitsAndGenes")]
        public class get_DisabledWorkTagsBackstoryTraitsAndGenes_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn ___pawn, ref WorkTags __result)
            {
                if (ABF_Utils.IsProgrammableDrone(___pawn))
                {
                    if (PawnGenerator.IsBeingGenerated(___pawn))
                    {
                        return true;
                    }
                    CompArtificialPawn compReprogrammableDrone = ___pawn.GetComp<CompArtificialPawn>();
                    WorkTags enabledTags = WorkTags.None;
                    List<WorkTypeDef> enabledWorkTypes = compReprogrammableDrone.enabledWorkTypes;
                    for (int i = enabledWorkTypes.Count - 1; i >= 0; i--)
                    {
                        enabledTags |= enabledWorkTypes[i].workTags;
                    }

                    enabledTags |= SpecialTags(___pawn);

                    __result = ~enabledTags;
                    return false;
                }
                return true;
            }
        }

        // In some cases, work tags should be enabled despite programming. This method allows checking and patching those instances in.
        public static WorkTags SpecialTags(Pawn pawn)
        {
            WorkTags workTags = WorkTags.None;
            if (pawn.Faction.HostileTo(Faction.OfPlayer))
            {
                workTags |= WorkTags.Violent;
            }
            return workTags;
        }
    }
}