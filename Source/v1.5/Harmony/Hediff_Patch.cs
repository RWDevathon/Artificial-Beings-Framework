using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using System.Reflection.Emit;

namespace ArtificialBeings
{
    public class Hediff_Patch
    {
        // Vanilla has it set so that injuries cannot be tended to if the pawn cannot bleed, for whatever reason. 
        [HarmonyPatch(typeof(Hediff), nameof(Hediff.TendableNow))]
        public class TendableNow_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                int instsToSkip = 0;
                bool transpilerWillExplodeIfAnotherIsInstFound = false;

                for (int i = 0; i < instructions.Count; i++)
                {
                    if (i < instructions.Count - 2 && instructions[i + 2].opcode == OpCodes.Isinst)
                    {
                        if (transpilerWillExplodeIfAnotherIsInstFound)
                        {
                            Log.Warning("[ABF] A transpiler has added an extra Isinst call to Hediff.TendableNow. Please provide a list of patches (included in hugslogs) to Killathon in the mod-development channel on the RW server or on the MH discord so he has to realize he needs to make his transpiler better or he can rationalize why the mod that added it is very dumb and is to be scolded. Thank you. - Killathon");
                        }
                        else
                        {
                            instsToSkip = 9;
                            transpilerWillExplodeIfAnotherIsInstFound = true;
                            yield return new CodeInstruction(OpCodes.Brtrue_S, instructions[i + 3].operand);
                        }
                    }

                    if (instsToSkip > 0)
                    {
                        instsToSkip--;
                    }
                    else
                    {
                        yield return instructions[i];
                    }
                }
            }
        }

        [HarmonyPatch]
        public class PainOffset_Patch
        {
            [HarmonyTargetMethods]
            static IEnumerable<MethodBase> TargetProperties()
            {
                yield return AccessTools.DeclaredPropertyGetter(typeof(Hediff), "PainOffset");
                foreach (System.Type subclass in typeof(Hediff).AllSubclasses())
                {
                    if (AccessTools.DeclaredPropertyGetter(subclass, "PainOffset") is MethodInfo method)
                    {
                        yield return method;
                    }
                }
            }

            [HarmonyPrefix]
            public static bool Prefix(ref float __result, Pawn ___pawn)
            {
                if (ABF_Utils.IsArtificial(___pawn))
                {
                    __result = 0f;
                    return false;
                }
                return true;
            }
        }
    }
}
