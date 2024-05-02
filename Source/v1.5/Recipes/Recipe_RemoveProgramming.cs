using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ArtificialBeings
{
    public class Recipe_RemoveProgramming : Recipe_Surgery
    {
        // This operation is only available on programmable drones that have programming.
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return thing is Pawn pawn
                && ABF_Utils.IsProgrammableDrone(pawn)
                && pawn.health.hediffSet.GetFirstHediffOfDef(ABF_HediffDefOf.ABF_Disabled) == null;
        }

        // This operation resets the drone back to being "blank" and having no programming.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            pawn.health.AddHediff(ABF_HediffDefOf.ABF_Disabled);
            ABF_Utils.Deprogram(pawn);
        }
    }
}

