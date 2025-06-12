using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class SurgeryOutcomeComp_LocationAndRoomQuality : SurgeryOutcomeComp
    {
        public StatDef statDef;

        public override bool Affects(RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part)
        {
            if (!recipe.surgeryIgnoreEnvironment)
            {
                return patient.InBed();
            }
            return false;
        }

        public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
        {
            quality *= patient.CurrentBed().GetStatValue(statDef);
        }
    }
}