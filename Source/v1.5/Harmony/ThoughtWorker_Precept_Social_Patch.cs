using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class ThoughtWorker_Precept_Social_Patch
    {
        // Artificial drones don't have precepts. Other pawns don't judge them for this.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_Social), "CurrentSocialStateInternal")]
        public class CurrentSocialStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, Pawn otherPawn, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if (ABF_Utils.IsArtificialDrone(p) || ABF_Utils.IsArtificialDrone(otherPawn))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}