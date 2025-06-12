using Verse;
using HarmonyLib;
using RimWorld;

namespace ArtificialBeings
{
    public class Pawn_NeedsTracker_Patch
    {
        // Ensure artificial units have applicable needs as determined by their extension.
        [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
        public class ShouldHaveNeed_Patch
        {
            [HarmonyPostfix]
            public static void Listener(NeedDef nd, ref bool __result, Pawn ___pawn)
            {
                // If the result is already false or the pawn is null for some reason, do nothing.
                if (!__result || ___pawn == null)
                    return;

                // Get the pawn's artificial extension and the need's artificial extension (if they exist).
                // Artificial pawns have access to a blacklist, while need extensions indicate only artificial pawns may have it.
                ABF_ArtificialPawnExtension pawnExtension = ___pawn.def.GetModExtension<ABF_ArtificialPawnExtension>();
                bool isArtificialNeed = typeof(Need_Artificial).IsAssignableFrom(nd.needClass);

                // If there is no pawn extension, then this isn't an artificial pawn.
                if (pawnExtension == null)
                {
                    // If there is a need extension, then it is only for artificial pawns. This pawn should not have it.
                    if (isArtificialNeed)
                    {
                        __result = false;
                        return;
                    }

                    // Without an extension, there is nothing else to check. The pawn may have this need.
                    return;
                }

                // Race-wide blacklisted needs.
                if (pawnExtension.blacklistedNeeds?.Contains(nd) ?? false)
                {
                    __result = false;
                    return;
                }

                // Sapient blacklisted needs.
                if ((ABF_Utils.IsArtificialSapient(___pawn) || ABF_Utils.IsArtificialBlank(___pawn)) && (pawnExtension.blacklistedSapientNeeds?.Contains(nd) ?? false))
                {
                    __result = false;
                    return;
                }

                // Drone blacklisted needs.
                if ((ABF_Utils.IsArtificialDrone(___pawn) || ABF_Utils.IsArtificialBlank(___pawn)) && (pawnExtension.blacklistedDroneNeeds?.Contains(nd) ?? false))
                {
                    __result = false;
                    return;
                }

                // Artificial needs are whitelist only.
                if (isArtificialNeed && (pawnExtension.artificialNeeds.NullOrEmpty() || !pawnExtension.artificialNeeds.ContainsKey(nd)) && !___pawn.health.hediffSet.TryGetNeedEnablingHediff(nd, out _))
                {
                    __result = false;
                }
            }
        }
    }
}