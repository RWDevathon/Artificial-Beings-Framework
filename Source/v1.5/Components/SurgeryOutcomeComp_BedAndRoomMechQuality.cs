using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ArtificialBeings
{
    // Override AffectQuality to use ABF_ArtificialSurgerySuccessChanceFactor instead of SurgerySuccessChanceFactor
    public class SurgeryOutcomeComp_BedAndRoomMechQuality : SurgeryOutcomeComp_BedAndRoomQuality
    {
        public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
        {
            quality *= patient.CurrentBed().GetStatValue(ABF_StatDefOf.ABF_ArtificialSurgerySuccessChanceFactor);
        }
    }
}