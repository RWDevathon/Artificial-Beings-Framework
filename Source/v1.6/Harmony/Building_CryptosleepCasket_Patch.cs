using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace ArtificialBeings
{
    public class Building_CryptosleepCasket_Patch
    {
        // This transpiler prevents artificial pawns from getting cryptosleep sickness when exiting the caskets.
        [HarmonyPatch(typeof(Building_CryptosleepCasket), "EjectContents")]
        public class Building_CryptosleepCasket_EjectContents_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.IsFlesh));

                for (int i = 0; i < instructions.Count; i++)
                {
                    if (i < instructions.Count - 1 && instructions[i + 1].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Dup); // Load a copy of the Pawn onto the Stack
                    }

                    yield return instructions[i];

                    if (instructions[i].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Building_CryptosleepCasket_EjectContents_Patch), nameof(IsOrganic))); // Our function call
                    }
                }
            }

            // Returns true if the pawn is organic.
            private static bool IsOrganic(Pawn pawn, bool notMechanoid)
            {
                return notMechanoid && !ABF_Utils.IsArtificial(pawn);
            }
        }
    }
}