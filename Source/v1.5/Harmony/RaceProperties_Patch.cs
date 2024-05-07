using Verse;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ArtificialBeings
{
    public class RaceProperties_Patch
    {
        // Reprogrammable drone directives may affect food need (energy) fall rate, which is handled separately from stats.
        // While the stat calculation is handled by the Need_Food_Patch, its explanation of the calculation is separate and is handled here.
        [HarmonyPatch(typeof(RaceProperties), "NutritionEatenPerDayExplanation")]
        public class NutritionEatenPerDayExplanation_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                FieldInfo targetField = AccessTools.Field(typeof(Pawn), "story");
                bool insertionComplete = false;

                for (int i = 0; i < instructions.Count; i++)
                {
                    // The target is the null-check for the pawn's story tracker, and we insert before that.
                    if (!insertionComplete && instructions[i].LoadsField(targetField))
                    {
                        Label factorLabel = generator.DefineLabel();
                        instructions[i].labels.Add(factorLabel);
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ABF_Utils), nameof(ABF_Utils.IsProgrammableDrone), new Type[] { typeof(Pawn) })); // Check if its a programmable drone
                        yield return new CodeInstruction(OpCodes.Brfalse, factorLabel);

                        yield return new CodeInstruction(OpCodes.Ldloc_0); // Load result string builder
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NutritionEatenPerDayExplanation_Patch), nameof(GetExplanationForDrones))); // Our function call
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(StringBuilder), "AppendLine", new Type[] { typeof(string) })); // Append our lines to the string builder
                        yield return new CodeInstruction(OpCodes.Pop); // AppendLine handles adding it to the result for us, we need to clean the stack up.
                        insertionComplete = true;
                    }
                    yield return instructions[i];
                }
            }

            // Insert our explanation for a reprogrammable drone's directives here.
            private static string GetExplanationForDrones(Pawn pawn)
            {
                StringBuilder result = new StringBuilder();
                float factorAmount = 1f;
                foreach (DirectiveDef directiveDef in pawn.GetComp<CompArtificialPawn>().ActiveDirectives)
                {
                    factorAmount *= directiveDef.hungerRateFactor;
                }
                factorAmount = Mathf.Max(factorAmount, 0);
                if (factorAmount != 1f)
                {
                    result.AppendLine();
                    result.AppendLine("ABF_StatsReport_Directives".Translate() + ": " + factorAmount.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Factor));
                }
                return result.ToString();
            }
        }
    }
}