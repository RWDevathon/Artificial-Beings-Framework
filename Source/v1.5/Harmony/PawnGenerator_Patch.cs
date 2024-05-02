using Verse;
using HarmonyLib;
using System;
using System.Collections.Generic;
using RimWorld;

namespace ArtificialBeings
{
    public class PawnGenerator_Patch
    {
        // The CompArtificialPawn needs to be properly initialized, and cannot use PostPostMake as the PawnKindDef isn't set yet there.
        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn")]
        [HarmonyPatch(new Type[] { typeof(PawnGenerationRequest) }, new ArgumentType[] { ArgumentType.Normal })]
        public class GeneratePawn_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref Pawn __result)
            {
                if (__result.GetComp<CompArtificialPawn>() is CompArtificialPawn comp)
                {
                    comp.InitializeState();
                }
            }
        }

        // Patch trait generation for artificial drones so they have none under the correct circumstances.
        [HarmonyPatch(typeof(PawnGenerator), "GenerateTraitsFor")]
        public class GenerateTraitsFor_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref List<Trait> __result, Pawn pawn, int traitCount, PawnGenerationRequest? req, bool growthMomentTrait)
            {
                if (ABF_Utils.IsArtificialDrone(pawn) && pawn.def.GetModExtension<ABF_ArtificialPawnExtension>()?.dronesCanHaveTraits != true)
                {
                    __result = new List<Trait>();
                    return false;
                }
                return true;
            }
        }
    }
}