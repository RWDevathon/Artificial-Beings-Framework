using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using System.Linq;

namespace ArtificialBeings
{
    public class ThoughtWorker_Precept_ColonyXenotypeMakeup_Patch
    {
        // Artificial units are unaffected by preferred xenotype social effects as they can not have genetics or xenotypes.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_ColonyXenotypeMakeup), "ShouldHaveThought")]
        public class ThoughtWorker_Precept_ColonyXenotypeMakeup_ShouldHaveThought_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                int pawnInstructionIndex = 0;
                bool insertAfter = false;
                Label? branchLabel = null;
                var fieldInfo = typeof(Pawn).GetFields(AccessTools.all).First(field => field.FieldType == typeof(Pawn_GeneTracker));
                if (fieldInfo == null)
                {
                    Log.Error("Feck");
                    foreach (var instruction in instructions)
                    {
                        yield return instruction;
                    }
                }

                // Locate the gene tracker call and insert our artificial beings check after it.
                for (int i = 0; i < instructions.Count; i++)
                {
                    if (instructions[i].LoadsField(fieldInfo))
                    {
                        pawnInstructionIndex = i - 1;
                        insertAfter = true;
                    }
                    yield return instructions[i];
                    if (insertAfter && instructions[i].Branches(out branchLabel))
                    {
                        yield return instructions[pawnInstructionIndex];
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ABF_Utils), nameof(ABF_Utils.IsArtificial), new Type[] { typeof(Pawn) })); // Our function call
                        yield return new CodeInstruction(OpCodes.Brtrue_S, branchLabel); // Branch to the code beyond the conditional if true, we don't count artificial pawns.
                        insertAfter = false;
                    }
                }
            }
        }
    }
}