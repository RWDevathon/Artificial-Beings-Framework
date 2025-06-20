using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace ArtificialBeings
{
    public class DamageWorker_Nerve_Patch
    {
        // Nerve damage shouldn't affect artificial units, which have no nerves.
        [HarmonyPatch(typeof(DamageWorker_Nerve), "ApplySpecialEffectsToPart")]
        public class DamageWorker_Nerve_ApplySpecialEffectsToPart_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.IsMechanoid));

                for (int i = 0; i < instructions.Count; i++)
                {
                    if (i < instructions.Count - 1 && instructions[i + 1].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Dup); // Load a copy of the Pawn onto the Stack
                    }

                    yield return instructions[i];

                    if (instructions[i].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DamageWorker_Nerve_ApplySpecialEffectsToPart_Patch), nameof(IsArtificial))); // Our function call
                    }
                }
            }

            // Returns true if the pawn is artificial.
            private static bool IsArtificial(Pawn pawn, bool mechanoid)
            {
                return mechanoid || ABF_Utils.IsArtificial(pawn);
            }
        }
    }
}