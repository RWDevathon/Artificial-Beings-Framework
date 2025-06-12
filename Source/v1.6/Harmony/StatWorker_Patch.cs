using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using RimWorld;
using System;
using System.Text;

namespace ArtificialBeings
{
    //public static class StatWorker_Patch
    //{
    //    // Reprogrammable drones have directives which may have stat offsets or factors. Those should be accounted for in stat calculations.
    //    [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
    //    public class GetValueUnfinalized_Patch
    //    {
    //        [HarmonyTranspiler]
    //        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
    //        {
    //            List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
    //            MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Ideo));
    //            bool offsetInsertionComplete = false;
    //            bool factorInsertionComplete = false;

    //            for (int i = 0; i < instructions.Count; i++)
    //            {
    //                if (!offsetInsertionComplete)
    //                {
    //                    // The target is Ideo's getter being null-checked, and we insert before that check.
    //                    if (instructions[i + 1].Calls(targetProperty) && instructions[i + 2].opcode == OpCodes.Brfalse)
    //                    {
    //                        Label offsetLabel = generator.DefineLabel();
    //                        instructions[i].labels.Add(offsetLabel);
    //                        yield return new CodeInstruction(OpCodes.Ldloc_1); // Load Pawn
    //                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ABF_Utils), nameof(ABF_Utils.IsProgrammableDrone), new Type[] { typeof(Pawn) })); // Check if its a programmable drone
    //                        yield return new CodeInstruction(OpCodes.Brfalse, offsetLabel);

    //                        yield return new CodeInstruction(OpCodes.Ldloc_0); // Load the result num onto the stack for later
    //                        yield return new CodeInstruction(OpCodes.Ldloc_1); // Load Pawn
    //                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load StatWorker
    //                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat")); // Load StatDef using StatWorker
    //                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetValueUnfinalized_Patch), nameof(StatOffsetForReprogrammableDrones))); // Our function call
    //                        yield return new CodeInstruction(OpCodes.Add); // Add the result num from earlier with the result of the function call
    //                        yield return new CodeInstruction(OpCodes.Stloc_0); // Store result back into num

    //                        offsetInsertionComplete = true; // Don't insert this code more than once and don't walk face-first into an out-of-index error.
    //                    }
    //                    yield return instructions[i];
    //                }
    //                else if (offsetInsertionComplete && !factorInsertionComplete)
    //                {
    //                    // The target is Ideo's getter being null-checked, and we insert before that check.
    //                    if (instructions[i + 1].Calls(targetProperty) && instructions[i + 2].opcode == OpCodes.Brfalse)
    //                    {
    //                        Label factorLabel = generator.DefineLabel();
    //                        instructions[i].labels.Add(factorLabel);
    //                        yield return new CodeInstruction(OpCodes.Ldloc_1); // Load Pawn
    //                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ABF_Utils), nameof(ABF_Utils.IsProgrammableDrone), new Type[] { typeof(Pawn) })); // Check if its a programmable drone
    //                        yield return new CodeInstruction(OpCodes.Brfalse, factorLabel);

    //                        yield return new CodeInstruction(OpCodes.Ldloc_0); // Load the result num onto the stack for later
    //                        yield return new CodeInstruction(OpCodes.Ldloc_1); // Load Pawn
    //                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load StatWorker
    //                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat")); // Load StatDef using StatWorker
    //                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetValueUnfinalized_Patch), nameof(StatFactorsForReprogrammableDrones))); // Our function call
    //                        yield return new CodeInstruction(OpCodes.Mul); // Multiply the result num from earlier with the result of the function call
    //                        yield return new CodeInstruction(OpCodes.Stloc_0); // Store result back into num

    //                        factorInsertionComplete = true; // Don't insert this code more than once and don't walk face-first into an out-of-index error.
    //                    }
    //                    yield return instructions[i];
    //                }
    //                else
    //                {
    //                    yield return instructions[i];
    //                }
    //            }
    //        }

    //        // Insert our stat offsets for a reprogrammable drone's directives here.
    //        private static float StatOffsetForReprogrammableDrones(Pawn pawn, StatDef statDef)
    //        {
    //            float result = 0f;
    //            foreach (DirectiveDef directiveDef in pawn.GetComp<CompArtificialPawn>().ActiveDirectives)
    //            {
    //                result += directiveDef.statOffsets?.GetStatOffsetFromList(statDef) ?? 0;
    //            }
    //            return result;
    //        }

    //        // Insert our stat factors for a reprogrammable drone's directives here.
    //        private static float StatFactorsForReprogrammableDrones(Pawn pawn, StatDef statDef)
    //        {
    //            float result = 1f;
    //            foreach (DirectiveDef directiveDef in pawn.GetComp<CompArtificialPawn>().ActiveDirectives)
    //            {
    //                result *= directiveDef.statFactors?.GetStatFactorFromList(statDef) ?? 1f;
    //            }
    //            return result;
    //        }
    //    }


    //    // Reprogrammable drones have directives which may have stat offsets or factors. Those should be accounted for in stat explanations.
    //    [HarmonyPatch(typeof(StatWorker), "GetOffsetsAndFactorsExplanation")]
    //    public class GetOffsetsAndFactorsExplanation_Patch
    //    {
    //        [HarmonyTranspiler]
    //        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
    //        {
    //            List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
    //            MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Ideo));
    //            bool insertionComplete = false;

    //            for (int i = 0; i < instructions.Count - 2; i++)
    //            {
    //                if (!insertionComplete)
    //                {
    //                    // The target is Ideo's getter being null-checked, and we insert before that check.
    //                    if (instructions[i + 1].Calls(targetProperty) && instructions[i + 2].opcode == OpCodes.Brfalse)
    //                    {
    //                        Label offsetLabel = generator.DefineLabel();
    //                        instructions[i].labels.Add(offsetLabel);
    //                        yield return new CodeInstruction(OpCodes.Ldloc_2); // Load Pawn
    //                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ABF_Utils), nameof(ABF_Utils.IsProgrammableDrone), new Type[] { typeof(Pawn) })); // Check if its a programmable drone
    //                        yield return new CodeInstruction(OpCodes.Brfalse, offsetLabel);

    //                        yield return new CodeInstruction(OpCodes.Ldloc_0); // Load the result stringBuilder
    //                        yield return new CodeInstruction(OpCodes.Ldloc_2); // Load Pawn
    //                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load StatWorker
    //                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load StatWorker (for accessing the stat def)
    //                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat")); // Load StatDef using StatWorker
    //                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetOffsetsAndFactorsExplanation_Patch), nameof(GetStatExplanationsForDrones))); // Our function call
    //                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(StringBuilder), "AppendLine", new Type[] { typeof(string) })); // Append our lines to the string builder
    //                        yield return new CodeInstruction(OpCodes.Pop); // AppendLine handles adding it to the result for us, we need to clean the stack up.

    //                        insertionComplete = true; // Don't insert this code more than once.
    //                    }
    //                    yield return instructions[i];
    //                }
    //                else
    //                {
    //                    yield return instructions[i];
    //                }
    //            }
    //        }

    //        // Insert our stat offsets for a reprogrammable drone's directives here.
    //        private static string GetStatExplanationsForDrones(Pawn pawn, StatWorker statWorker, StatDef statDef)
    //        {
    //            StringBuilder result = new StringBuilder();
    //            float offsetAmount = 0f;
    //            float factorAmount = 1f;
    //            foreach (DirectiveDef directiveDef in pawn.GetComp<CompArtificialPawn>().ActiveDirectives)
    //            {
    //                offsetAmount += directiveDef.statOffsets?.GetStatOffsetFromList(statDef) ?? 0;
    //                factorAmount *= directiveDef.statFactors?.GetStatFactorFromList(statDef) ?? 1f;
    //            }
    //            if (offsetAmount != 0f)
    //            {
    //                result.AppendLine("ABF_StatsReport_Directives".Translate() + ": " + statWorker.ValueToString(offsetAmount, false, ToStringNumberSense.Offset));
    //            }
    //            if (factorAmount != 1f)
    //            {
    //                result.AppendLine("ABF_StatsReport_Directives".Translate() + ": " + statWorker.ValueToString(factorAmount, false, ToStringNumberSense.Factor));
    //            }
    //            return result.ToString();
    //        }
    //    }
    //}
}