using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ArtificialBeings
{
    // Pawns in caravans should attempt to refill artificial needs.
    [HarmonyPatch(typeof(Caravan_NeedsTracker), "TrySatisfyPawnNeeds")]
    public static class Caravan_NeedsTracker_TrySatisfyPawnNeeds_Patch
    {
        [HarmonyPostfix]
        public static void Listener(Caravan ___caravan, Pawn pawn)
        {
            if (ABF_Utils.IsArtificial(pawn) && pawn.needs is Pawn_NeedsTracker needTracker)
            {
                foreach (Need need in needTracker.AllNeeds)
                {
                    if (need.def.GetModExtension<ABF_ArtificialNeedExtension>() is ABF_ArtificialNeedExtension needExtension && need.CurLevelPercentage <= needExtension.criticalThreshold)
                    {
                        List<ThingDef> fulfillingThingDefs = ABF_Utils.cachedArtificialNeedFulfillments[need.def];
                        if (fulfillingThingDefs != null && fulfillingThingDefs.Count > 0)
                        {
                            foreach (Thing thing in CaravanInventoryUtility.AllInventoryItems(___caravan))
                            {
                                if (fulfillingThingDefs.Contains(thing.def))
                                {
                                    ABF_NeedFulfillerExtension needFulfiller = thing.def.GetModExtension<ABF_NeedFulfillerExtension>();
                                    float fulfillmentAmount = needFulfiller.needOffsetRelations[need.def];
                                    int consumptionCount = Mathf.Min(thing.stackCount, Mathf.Max(1, Mathf.FloorToInt((need.MaxLevel - need.CurLevel) / fulfillmentAmount)));
                                    need.CurLevel += fulfillmentAmount * consumptionCount;
                                    if (thing.stackCount == consumptionCount)
                                    {
                                        thing.Destroy();
                                    }
                                    else
                                    {
                                        thing.stackCount -= consumptionCount;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
