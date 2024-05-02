using RimWorld;
using Verse;

namespace ArtificialBeings
{
    // Simple class containing calculated values for a given skillRecord for the cost at the current level and the min and max of the skill.
    // Best if used in a Dict<SkillRecord, DroneSkillContext> object.
    public class DroneSkillContext
    {
        public float skillComplexityCost = 0;

        public int skillFloor = 0;

        public int skillCeiling = 0;

        public DroneSkillContext(SkillRecord skillRecord)
        {
            skillComplexityCost = ABF_Utils.SkillComplexityCostFor(skillRecord.Pawn, skillRecord.def);
            skillFloor = skillRecord.Pawn.def.GetModExtension<ABF_ArtificialPawnExtension>().inherentSkills.GetWithFallback(skillRecord.def, SkillRecord.MinLevel);
            skillCeiling = (int)skillRecord.Pawn.GetStatValue(ABF_StatDefOf.ABF_SkillLimit) + skillFloor;
        }

    }
}
