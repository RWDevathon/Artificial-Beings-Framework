using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace ArtificialBeings
{
    public static class QualityUtility_Patch
    {
        // Drones may have restrictions placed on the quality of goods produced. Reprogrammable drones may have offsets or forced overrides to these qualities.
        [HarmonyPatch(typeof(QualityUtility), "GenerateQualityCreatedByPawn")]
        [HarmonyPatch(new Type[] { typeof(Pawn), typeof(SkillDef), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        public class GenerateQualityCreatedByPawn_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, SkillDef relevantSkill, bool consumeInspiration, ref QualityCategory __result)
            {
                if (ABF_Utils.IsArtificialDrone(pawn))
                {
                    ABF_ArtificialPawnExtension artificialPawnExtension = pawn.def.GetModExtension<ABF_ArtificialPawnExtension>();
                    if (artificialPawnExtension == null)
                    {
                        return;
                    }

                    // Clamp the result to the drone quality range.
                    QualityRange droneQualityRange = artificialPawnExtension.droneQualityRange;
                    if (__result < droneQualityRange.min)
                    {
                        __result = droneQualityRange.min;
                    }
                    else if (__result > droneQualityRange.max)
                    {
                        __result = droneQualityRange.max;
                    }

                    // Handle reprogrammable drone offsets and forced qualities now, if applicable.
                    if (ABF_Utils.IsProgrammableDrone(pawn))
                    {
                        CompArtificialPawn compArtificialPawn = pawn.GetComp<CompArtificialPawn>();
                        QualityCategory? forcedQuality = null;
                        int qualityOffsets = 0;
                        // Check all active directives for a forced item quality and the quality offsets. If the former exists, it overrides the latter.
                        foreach (DirectiveDef directiveDef in compArtificialPawn.ActiveDirectives)
                        {
                            if (directiveDef.forcedItemQuality != null && (forcedQuality == null || directiveDef.forcedItemQuality < forcedQuality))
                            {
                                forcedQuality = directiveDef.forcedItemQuality;
                            }
                            qualityOffsets += directiveDef.producedItemQualityOffset;
                        }
                        if (forcedQuality != null)
                        {
                            __result = forcedQuality.Value;
                            return;
                        }

                        // Applying the offsets needs to remain within the bounds of the QualityCategory enum.
                        __result = (QualityCategory)Mathf.Clamp((int)__result + qualityOffsets, (int)QualityRange.All.min, (int)QualityRange.All.max);
                    }
                }
            }
        }
    }
}