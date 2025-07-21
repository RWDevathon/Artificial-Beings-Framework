using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using static ArtificialBeings.ThoughtWorker_Precept_IdeoDiversity_Patch;

namespace ArtificialBeings
{
    public class ThoughtWorker_Precept_IdeoDiversity_Uniform_Patch
    {
        // Artificial drones don't care about ideological diversity. Other pawns don't care about the drones' lack of ideology.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_IdeoDiversity_Uniform), "ShouldHaveThought")]
        public class TW_Precept_IdeoUniform_ShouldHaveThought
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.Humanlike));

                for (int i = 0; i < instructions.Count; i++)
                {
                    yield return instructions[i];
                    if (instructions[i].Calls(targetProperty))
                    {
                        // Duplicate the instructions to pull the target pawn (list[i] is a local variable, and i is also a local variable). We don't want the RaceProperties.
                        for (int subI = i - 4; subI < i - 1; subI++)
                        {
                            yield return instructions[subI];
                        }
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ThoughtWorker_Precept_IdeoDiversity_ShouldHaveThought), nameof(SapientHumanlike))); // Our function call
                    }
                }
            }

            private static bool SapientHumanlike(bool humanlike, Pawn pawn)
            {
                return humanlike && !ABF_Utils.IsConsideredNonHumanlike(pawn);
            }
        }
    }
}