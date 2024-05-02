using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace ArtificialBeings
{
    public class HealthAIUtility_Patch
    {
        // This transpiler alters the behavior of finding medicine to account for races which have defined restrictions.
        [HarmonyPatch]
        public class FindBestMedicine_Patch
        {
            [HarmonyPatch]
            static MethodInfo TargetMethod()
            {
                return typeof(HealthAIUtility).GetNestedTypes(AccessTools.all).SelectMany(AccessTools.GetDeclaredMethods).First(target => target.ReturnType == typeof(bool) && target.GetParameters().First().ParameterType == typeof(Thing));
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetMethod = AccessTools.Method(typeof(ForbidUtility), nameof(ForbidUtility.IsForbidden), new Type[] { typeof(Thing), typeof(Pawn) });
                FieldInfo fieldInfo = typeof(HealthAIUtility).GetNestedTypes(AccessTools.all).First().GetFields(AccessTools.all).First(field => field.Name == "patient");

                // Yield the actual instructions, adding in our additional instructions where necessary.
                for (int i = 0; i < instructions.Count; i++)
                {
                    yield return instructions[i];
                    if (instructions[i].Calls(targetMethod))
                    {
                        yield return instructions[++i];
                        instructions[i].Branches(out Label? label);
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // Load Thing
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load "This" - AKA. the class this validator is in
                        yield return new CodeInstruction(OpCodes.Ldfld, fieldInfo); // Load the patient Pawn from the class
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FindBestMedicine_Patch), nameof(ValidMedicine))); // Our function call
                        yield return new CodeInstruction(OpCodes.Brtrue, label);
                    }
                }
            }

            private static bool ValidMedicine(Thing medicine, Pawn pawn)
            {
                return ABF_Utils.IsMedicineValid(medicine.def, pawn.def);
            }
        }
    }
}