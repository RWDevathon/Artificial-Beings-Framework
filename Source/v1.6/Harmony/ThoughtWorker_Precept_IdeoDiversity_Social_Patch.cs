using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class ThoughtWorker_Precept_IdeoDiversity_Social_Patch
    {
        // Drones don't care about ideological diversity. Other pawns don't care about the drones' lack of ideology.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_IdeoDiversity_Social), "ShouldHaveThought")]
        public class ThoughtWorker_Precept_IdeoDiversity_Social_ShouldHaveThought_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn p, Pawn otherPawn, ref ThoughtState __result)
            {
                if (!ABF_Utils.IsConsideredNonHumanlike(otherPawn))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}