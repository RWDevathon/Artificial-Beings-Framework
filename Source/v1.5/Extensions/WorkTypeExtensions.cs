using RimWorld;
using Verse;

namespace ArtificialBeings
{
    // Mod extension to work type defs for integration with reprogrammable drones.
    public class ABF_WorkTypeExtension : DefModExtension
    {
        public WorkTypeDef def;

        public int baseComplexity = 1;

        public int minimumRequiredComplexity = 0;

        // Method for establishing whether a particular pawn may have this work type enabled.
        public AcceptanceReport ValidFor(Pawn pawn)
        {
            CompArtificialPawn programComp = pawn.GetComp<CompArtificialPawn>();
            if (programComp.MaxComplexity < minimumRequiredComplexity)
            {
                return "ABF_ComplexityRequirementInsufficient".Translate(def.labelShort.CapitalizeFirst(), minimumRequiredComplexity, pawn.LabelShortCap, programComp.MaxComplexity);
            }
            return true;
        }
    }
}