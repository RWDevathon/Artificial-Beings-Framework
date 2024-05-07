using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    class HediffMaker_Patch
    {
        // Artificial pawns may have specific replaced versions comparative to organic pawns. This prefix simply alters the HediffDef being made as it passes through.
        [HarmonyPatch(typeof(HediffMaker), "MakeHediff")]
        public static class HediffMaker_MakeHediff_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref HediffDef def, Pawn pawn)
            {
                if (ABF_Utils.cachedArtificialHediffReplacements.TryGetValue(pawn.def, out Dictionary<HediffDef, HediffDef> replacementDictionary) && replacementDictionary.TryGetValue(def, out HediffDef replacementDef))
                {
                    def = replacementDef;
                }
                return true;

            }
        }
    }
}
