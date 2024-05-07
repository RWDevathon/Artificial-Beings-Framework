namespace ArtificialBeings
{
    public class Need_Patch
    {
        // TODO: This isn't be necessary!
        //// Artificials don't have a food meter, they have an energy meter. Since we're hijacking hunger, change the labelled name.
        //[HarmonyPatch(typeof(Need), "get_LabelCap")]
        //public class get_LabelCap_Patch
        //{
        //    [HarmonyPostfix]
        //    public static void Listener( ref string __result, Pawn ___pawn, Need __instance)
        //    {
        //        if (__instance.def.defName == "Food")
        //        {
        //            if (ABF_Utils.IsArtificial(___pawn))
        //            {
        //                __result = "ABF_EnergyNeed".Translate();
        //            }
        //        }
        //    }
        //}

        //// Ensure the tooltip for hunger displays a tooltip about energy.
        //[HarmonyPatch(typeof(Need_Food), "GetTipString")]
        //public class GetTipString_Patch
        //{
        //    [HarmonyPostfix]
        //    public static void Listener(ref string __result, Pawn ___pawn, Need __instance)
        //    {
        //        if (__instance.def.defName == "Food")
        //        {
        //            if (ABF_Utils.IsArtificial(___pawn))
        //            {
        //                __result = $"{"ABF_EnergyNeed".Translate()}: {__instance.CurLevelPercentage.ToStringPercent()} ({__instance.CurLevel:0.##}/{__instance.MaxLevel:0.##})\n{"ABF_EnergyNeedDesc".Translate()}";
        //                return;
        //            }
        //        }
        //    }
        //}
    }
}