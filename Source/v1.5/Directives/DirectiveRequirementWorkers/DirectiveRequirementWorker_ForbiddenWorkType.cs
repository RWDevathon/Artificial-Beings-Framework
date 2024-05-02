using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    // This worker takes a list of forbidden work types and ensures the pawn has none
    // of the work types enabled in order for them to have this directive.
    public class DirectiveRequirementWorker_ForbiddenWorkType : DirectiveRequirementWorker
    {
        public List<WorkTypeDef> forbiddenWorkTypes = new List<WorkTypeDef>();

        public override AcceptanceReport ValidFor(Pawn pawn)
        {
            AcceptanceReport baseReport = base.ValidFor(pawn);
            if (!baseReport)
            {
                return baseReport.Reason;
            }

            if (forbiddenWorkTypes.NullOrEmpty())
            {
                return true;
            }

            // Scan through all forbidden work types.
            for (int i = forbiddenWorkTypes.Count - 1; i >= 0; i--)
            {
                if (!pawn.WorkTypeIsDisabled(forbiddenWorkTypes[i]))
                {
                    return "ABF_ForbiddenWorkTypePresent".Translate(forbiddenWorkTypes[i].labelShort);
                }
            }
            // If we scanned all work types and no forbidden work types were active, this is valid.
            return true;
        }
    }
}
