using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    // This worker takes a list of required work types and ensures the pawn has all (or at least one if anyOfTypeAcceptable is true)
    // of the work types enabled in order for them to have this directive.
    public class DirectiveRequirementWorker_RequiredWorkType : DirectiveRequirementWorker
    {
        public List<WorkTypeDef> requiredWorkTypes = new List<WorkTypeDef>();

        public bool anyOfTypeAcceptable = false;

        public override AcceptanceReport ValidFor(Pawn pawn)
        {
            AcceptanceReport baseReport = base.ValidFor(pawn);
            if (!baseReport)
            {
                return baseReport.Reason;
            }

            if (requiredWorkTypes.NullOrEmpty())
            {
                return true;
            }

            // Scan through all required work types.
            for (int i = requiredWorkTypes.Count - 1; i >= 0; i--)
            {
                if (pawn.WorkTypeIsDisabled(requiredWorkTypes[i]))
                {
                    // If this is a work type that is disabled and all work types must be enabled, this is invalid.
                    if (!anyOfTypeAcceptable)
                    {
                        return "ABF_RequiredWorkTypeAbsent".Translate(pawn, requiredWorkTypes[i].labelShort);
                    }
                    // If it is disabled but only one of the types must be enabled, continue scanning.
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    // If the work type is enabled and only one of the required ones must be active, this is valid.
                    if (anyOfTypeAcceptable)
                    {
                        return true;
                    }
                }
            }
            // If we scanned all required work types and all were disabled (did not terminate early), this is invalid.
            if (anyOfTypeAcceptable)
            {
                return "ABF_RequiredWorkTypeSetAbsent".Translate(pawn, requiredWorkTypes.Join(workType => workType.defName));
            }
            // If we scanned all required work types and all were active, this is valid.
            return true;
        }
    }
}
