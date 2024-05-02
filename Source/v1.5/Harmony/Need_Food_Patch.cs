using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using RimWorld;
using System;
using UnityEngine;

namespace ArtificialBeings
{
    public static class Need_Food_Patch
    {
        // Reprogrammable drone directives may affect food need (energy) fall rate, which is handled separately from stats.
        [HarmonyPatch(typeof(Need_Food), "FoodFallPerTickAssumingCategory")]
        public class FoodFallPerTickAssumingCategory_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(ModsConfig), nameof(ModsConfig.BiotechActive));
                bool insertionComplete = false;

                for (int i = 0; i < instructions.Count; i++)
                {
                    // The target is BiotechActive's getter being null-checked, and we insert before that check.
                    if (!insertionComplete && instructions[i].Calls(targetProperty))
                    {
                        Label factorLabel = generator.DefineLabel();
                        instructions[i].labels.Add(factorLabel);
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load Need class "this"
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Need), "pawn")); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ABF_Utils), nameof(ABF_Utils.IsProgrammableDrone), new Type[] { typeof(Pawn) })); // Check if its a programmable drone
                        yield return new CodeInstruction(OpCodes.Brfalse, factorLabel);

                        yield return new CodeInstruction(OpCodes.Ldloc_1); // Load result
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load Need class "this"
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Need), "pawn")); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FoodFallPerTickAssumingCategory_Patch), nameof(HungerFactorFromDirectives))); // Our function call
                        yield return new CodeInstruction(OpCodes.Mul); // Multiply the result num from earlier with the result of the function call
                        yield return new CodeInstruction(OpCodes.Stloc_1); // Store result
                        insertionComplete = true;
                    }
                    yield return instructions[i];
                }
            }

            // Insert our hunger rate factor from a reprogrammable drone's directives here.
            private static float HungerFactorFromDirectives(Pawn pawn)
            {
                float result = 1f;
                foreach (DirectiveDef directiveDef in pawn.GetComp<CompArtificialPawn>().ActiveDirectives)
                {
                    result *= directiveDef.hungerRateFactor;
                }
                return Mathf.Max(result, 0);
            }
        }
    }
}