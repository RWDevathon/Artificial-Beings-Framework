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
                if (ABF_Utils.cachedArtificialHediffReplacements.TryGetValue(pawn.def, out List<HediffReplacementRecord> replacementRecords))
                {
                    foreach (HediffReplacementRecord record in replacementRecords)
                    {
                        if (record.toReplace == def)
                        {
                            def = record.toBeReplaced;
                            break;
                        }
                    }
                }
                return true;

            }
        }
    }
}
