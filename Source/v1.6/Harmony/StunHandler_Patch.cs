﻿using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace ArtificialBeings
{
    public class StunHandler_Patch
    {
        // Artificial units feel no pain.
        [HarmonyPatch(typeof(StunHandler), "CanBeStunnedByDamage")]
        public class StunHandler_CanBeStunnedByDamage_Patch
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
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StunHandler_CanBeStunnedByDamage_Patch), nameof(OrganicOrInvulnerableToEmp))); // Our function call
                    }
                }
            }

            private static bool OrganicOrInvulnerableToEmp(Pawn pawn, bool organic)
            {
                return organic && !ABF_Utils.cachedVulnerableToEMP.Contains(pawn.def);
            }
        }

        // Artificial units that are vulnerable to EMP should be able to adapt to it to avoid being permanently stun-locked.
        [HarmonyPatch(typeof(StunHandler), "CanAdaptToDamage")]
        public class StunHandler_CanAdaptToDamage_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(DamageDef def, ref bool __result, Thing ___parent)
            {
                if (def == DamageDefOf.EMP && ___parent is Pawn pawn && ABF_Utils.IsArtificial(pawn))
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}