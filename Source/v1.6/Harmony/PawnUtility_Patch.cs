﻿using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class PawnUtility_Patch
    {
        // Artificial humanlike drones cannot be casually interacted with. This primarily prevents animals nuzzling them for no good reason. Make an exception for traders, which need to be interactible.
        [HarmonyPatch(typeof(PawnUtility), "CanCasuallyInteractNow")]
        public class PawnUtility_CanCasuallyInteractNow_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(ref Pawn p, bool twoWayInteraction, bool canInteractWhileSleeping, bool canInteractWhileRoaming, bool canInteractWhileDrafted, ref bool __result)
            {
                if (ABF_Utils.IsArtificialDrone(p) && p.RaceProps.Humanlike && p.trader == null)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}