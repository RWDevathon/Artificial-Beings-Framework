using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class Recipe_DroneIncreaseLevel : Recipe_Surgery
    {
        // Only available for programmable drones that have the hediff to reduce.
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (thing is Pawn pawn && ABF_Utils.IsProgrammableDrone(pawn))
            {
                Hediff hardwareComplexity = pawn.health.hediffSet.GetFirstHediffOfDef(recipe.addsHediff);
                if (hardwareComplexity == null || hardwareComplexity.Severity < hardwareComplexity.def.maxSeverity)
                {
                    return true;
                }
            }
            return false;
        }

        // Always "apply" to the brain as it is easy to target, and guaranteed to exist if the pawn is alive.
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            yield return pawn.health.hediffSet.GetBrain();
        }

        // Change the severity of the target hediff by one.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            Hediff hardwareComplexity = pawn.health.hediffSet.GetFirstHediffOfDef(recipe.addsHediff);
            if (hardwareComplexity != null)
            {
                hardwareComplexity.Severity += 1f;
            }
            else
            {
                hardwareComplexity = HediffMaker.MakeHediff(recipe.addsHediff, pawn);
                hardwareComplexity.Severity = 1f;
                pawn.health.AddHediff(hardwareComplexity);
            }
            pawn.GetComp<CompArtificialPawn>()?.RecalculateComplexity();
        }
    }
}