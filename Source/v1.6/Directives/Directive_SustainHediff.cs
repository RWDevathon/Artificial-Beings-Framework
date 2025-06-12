using Verse;

namespace ArtificialBeings
{
    public class Directive_SustainHediff : Directive
    {
        public Hediff hediff;

        // Method for reacting to the Directive being added to a particular pawn.
        public override void PostAdd()
        {
            hediff = HediffMaker.MakeHediff(def.associatedHediff, pawn);
            pawn.health.AddHediff(hediff);
        }

        // Method for acting after the Directive is removed from a pawn (reprogrammed).
        public override void PostRemove()
        {
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
                hediff = null;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref hediff, "ABF_directiveHediff", saveDestroyedThings: true);
        }
    }
}
