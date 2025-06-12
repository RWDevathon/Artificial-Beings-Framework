using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    // This worker allows for making a directive exclusive to a particular race or set of races.
    public class DirectiveRequirementWorker_ExclusiveToRace : DirectiveRequirementWorker
    {
        private List<ThingDef> raceDefs = new List<ThingDef>();

        public override AcceptanceReport EverValidFor(Pawn pawn)
        {
            if (raceDefs.Contains(pawn.def))
            {
                return true;
            }
            return "ABF_ExclusiveToRace".Translate(def.label, pawn.LabelCap);
        }
    }
}