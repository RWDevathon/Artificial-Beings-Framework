﻿using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ArtificialBeings
{
    // Alternate WorkGiver for TendSelfArtificial to use the appropriate artificial stats.
    public class WorkGiver_TendSelfArtificial : WorkGiver_TendArtificial
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Undefined);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            yield return pawn;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            bool num = pawn == t && pawn.playerSettings != null && base.HasJobOnThing(pawn, t, forced);
            if (num && !pawn.playerSettings.selfTend)
            {
                JobFailReason.Is("SelfTendDisabled".Translate());
            }

            if (num)
            {
                return pawn.playerSettings.selfTend;
            }

            return false;
        }
    }
}
