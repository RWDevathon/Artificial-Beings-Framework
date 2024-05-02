using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    // This worker takes a list of required skills and ensures the pawn has all (or at least one if anyOfTypeAcceptable is true)
    // of the skills enabled in order for them to have this directive.
    public class DirectiveRequirementWorker_RequiredSkill : DirectiveRequirementWorker
    {
        public List<SkillDef> requiredSkills = new List<SkillDef>();

        public bool anyOfTypeAcceptable = false;

        public override AcceptanceReport ValidFor(Pawn pawn)
        {
            AcceptanceReport baseReport = base.ValidFor(pawn);
            if (!baseReport)
            {
                return baseReport.Reason;
            }

            if (requiredSkills.NullOrEmpty())
            {
                return true;
            }

            // Scan through all required skills.
            for (int i = requiredSkills.Count - 1; i >= 0; i--)
            {
                if (pawn.skills.GetSkill(requiredSkills[i]).TotallyDisabled)
                {
                    // If this is a skill that is disabled and all skills must be enabled, this is invalid.
                    if (!anyOfTypeAcceptable)
                    {
                        return "ABF_RequiredSkillAbsent".Translate(requiredSkills[i].label);
                    }
                    // If it is disabled but only one of the types must be enabled, continue scanning.
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    // If the skill is enabled and only one of the required ones must be active, this is valid.
                    if (anyOfTypeAcceptable)
                    {
                        return true;
                    }
                }
            }
            // If we scanned all required skill and all were disabled (did not terminate early), this is invalid.
            if (anyOfTypeAcceptable)
            {
                return "ABF_RequiredSkillSetAbsent".Translate(requiredSkills.Join(skill => skill.defName));
            }
            // If we scanned all required skills and all were active, this is valid.
            return true;
        }
    }
}
