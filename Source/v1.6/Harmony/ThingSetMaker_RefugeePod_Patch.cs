using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace ArtificialBeings
{
    public class ThingSetMaker_RefugeePod_Patch
    {
        // Artificial "refugees" in pods reboot as they feel no pain and thus may not be immobilized otherwise.
        [HarmonyPatch(typeof(ThingSetMaker_RefugeePod), "Generate")]
        public class Generate_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ThingSetMakerParams parms, ref List<Thing> outThings)
            {
                for (int i = outThings.Count - 1; i >= 0; i--)
                {
                    Thing thing = outThings[i];
                    if (thing is Pawn pawn)
                    {
                        if (ABF_Utils.IsArtificial(pawn))
                        {
                            pawn.health.AddHediff(ABF_HediffDefOf.ABF_Hediff_Artificial_Incapacitated);
                        }
                    }
                }
            }
        }
    }
}