using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    // This worker takes a list of forbidden skills and ensures the pawn has none of the skills enabled in order for them to have this directive.
    public class DirectiveRequirementWorker_ForbiddenSkill : DirectiveRequirementWorker
    {
        public List<SkillDef> forbiddenSkills = new List<SkillDef>();

        public override AcceptanceReport ValidFor(Pawn pawn)
        {
            AcceptanceReport baseReport = base.ValidFor(pawn);
            if (!baseReport)
            {
                return baseReport.Reason;
            }

            if (forbiddenSkills.NullOrEmpty())
            {
                return true;
            }

            // Scan through all forbidden skills.
            for (int i = forbiddenSkills.Count - 1; i >= 0; i--)
            {
                if (!pawn.skills.GetSkill(forbiddenSkills[i]).TotallyDisabled)
                {
                    return "ABF_ForbiddenSkillPresent".Translate(forbiddenSkills[i].label);
                }
            }
            // If we scanned all skills and no forbidden skill were active, this is valid.
            return true;
        }
    }
}
