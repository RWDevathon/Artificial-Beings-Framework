using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class SurgeryOutcomeComp_OperatorSuccessChance : SurgeryOutcomeComp
    {
        public StatDef statDef;

        public override bool Affects(RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part)
        {
            return ABF_Utils.IsArtificial(patient);
        }

        public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
        {
            quality *= surgeon.GetStatValue(statDef);
        }
    }
}