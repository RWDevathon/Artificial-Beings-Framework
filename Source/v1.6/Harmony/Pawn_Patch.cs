using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace ArtificialBeings
{
    public static class Pawn_Patch
    {
        // Programmable drones have their disabled work types identified through their programming comp, and by no other mechanic or feature.
        [HarmonyPatch(typeof(Pawn), "GetDisabledWorkTypes")]
        public class GetDisabledWorkTypes_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn __instance, ref List<WorkTypeDef> ___cachedDisabledWorkTypes, ref List<WorkTypeDef> __result, bool permanentOnly = false)
            {
                if (ABF_Utils.IsProgrammableDrone(__instance))
                {
                    if (___cachedDisabledWorkTypes == null)
                    {
                        // Do not cache work types while the pawn is being generated - the artificial comp is not initialized yet.
                        if (PawnGenerator.IsBeingGenerated(__instance))
                        {
                            return true;
                        }
                        List<WorkTypeDef> allWorkTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
                        List<WorkTypeDef> enabledWorkTypes = __instance.GetComp<CompArtificialPawn>().enabledWorkTypes;
                        if (enabledWorkTypes == null || enabledWorkTypes.Count == 0)
                        {
                            ___cachedDisabledWorkTypes = allWorkTypes.ToList();
                        }
                        else
                        {
                            ___cachedDisabledWorkTypes = new List<WorkTypeDef>();
                            for (int i = allWorkTypes.Count - 1; i >= 0; i--)
                            {
                                if (!enabledWorkTypes.Contains(allWorkTypes[i]))
                                {
                                    ___cachedDisabledWorkTypes.Add(allWorkTypes[i]);
                                }
                            }
                        }
                    }
                    __result = ___cachedDisabledWorkTypes;
                    return false;
                }
                return true;
            }
        }

        // Programmable drones have their disabled work tags identified through their programming comp, and by no other mechanic or feature.
        [HarmonyPatch(typeof(Pawn), "get_CombinedDisabledWorkTags")]
        public class get_CombinedDisabledWorkTags_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn __instance, ref List<WorkTypeDef> ___cachedDisabledWorkTypes, ref WorkTags __result)
            {
                if (ABF_Utils.IsProgrammableDrone(__instance))
                {
                    if (PawnGenerator.IsBeingGenerated(__instance))
                    {
                        return true;
                    }
                    WorkTags enabledTags = WorkTags.None;
                    List<WorkTypeDef> enabledWorkTypes = __instance.GetComp<CompArtificialPawn>().enabledWorkTypes;
                    if (enabledWorkTypes == null)
                    {
                        return true;
                    }
                    for (int i = enabledWorkTypes.Count - 1; i >= 0; i--)
                    {
                        enabledTags |= enabledWorkTypes[i].workTags;
                    }

                    __result = ~enabledTags;
                    return false;
                }
                return true;
            }
        }
    }
}