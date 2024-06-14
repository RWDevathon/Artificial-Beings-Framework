using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    // Mod extension for races to control some features. Many attributes only apply to humanlikes, but the extension can also be applied effectively to animals.
    public class ABF_ArtificialPawnExtension : DefModExtension
    {
        // Bools for what states this race may have. Disabling all three is a configuration error.
        public bool canBeSapient = true;
        public bool canBeDrone = true;
        public bool canBeReprogrammable = true;

        // Bool for whether members of this race are vulnerable to EMP attacks and whether hostiles will attempt to use EMP weapons against them.
        public bool vulnerableToEMP = true;

        // List of needs that all units of this ThingDef do not have. Note that food and rest are unnecessary here, as HAR has a needsRest tag to enable/disable rest and setting foodType to None disables needing food/energy.
        public List<NeedDef> blacklistedNeeds;

        // List of needs that specifically artificial drones do not have. Same note as above.
        public List<NeedDef> blacklistedDroneNeeds;

        // List of needs that specifically artificial sapients do not have. Same note as above.
        public List<NeedDef> blacklistedSapientNeeds;

        // Dictionary of artificial-specific needs of this race. The Keys are NeedDefs they have, with the Values being an int representation of their maximum capacity of the need.
        // Since only NeedDefs that are added to this list are added to the pawn, this is a whitelist. Note that the NeedDef should have its own extension to fully work.
        public Dictionary<NeedDef, float> artificialNeeds = new Dictionary<NeedDef, float>();

        // Dictionary of artificial-specific hediff replacements for this race. The Keys are HediffDefs that should be replaced, with the Value being the Hediff to replace with.
        public Dictionary<HediffDef, HediffDef> hediffReplacements = new Dictionary<HediffDef, HediffDef>();

        // Controls for what medicines the individuals of this race will be able to use for tending injuries.
        public List<ThingDef> medicineList = new List<ThingDef>();
        public List<ThingDef> whiteMedicineList = new List<ThingDef>();
        public List<ThingDef> blackMedicineList = new List<ThingDef>();
        public bool onlyUseRaceRestrictedMedicine = false;

        /* Drone specific */

        // Int for the stat levels of this race when set as a non-reprogrammable drone. This does nothing if the race is not considered drones.
        public int droneSkillLevel = 4;

        // Bool for whether drones can have traits.
        public bool dronesCanHaveTraits = false;

        /* Reprogrammable specific */

        // List of DirectiveDefs that all members of this race will have at all times.
        // Inherent directives cost no complexity and do not contribute to the directive limit.
        public List<DirectiveDef> inherentDirectives = new List<DirectiveDef>();

        // List of WorkTypes that all members of this race will have at all times.
        // Inherent work types cost no complexity and enable corresponding skill groups for free.
        public List<WorkTypeDef> inherentWorkTypes = new List<WorkTypeDef>();

        // List of WorkTypes that all members of this race may never have.
        // Forbidden work types will not appear in the reprogramming interface.
        // If a corresponding skill group can have none of its work types enabled, it is effectively forbidden.
        public List<WorkTypeDef> forbiddenWorkTypes = new List<WorkTypeDef>();

        // Dictionary matching SkillDefs to the level that all members of this race will have as a minimum.
        // Inherent skills cost no complexity, contribute to a higher skill ceiling, and can not be removed.
        public Dictionary<SkillDef, int> inherentSkills = new Dictionary<SkillDef, int>();

        // Maximum number of directives units of this race may have.
        public int maxDirectives = 3;

        public override IEnumerable<string> ConfigErrors()
        {
            if (!canBeSapient && !canBeDrone && !canBeReprogrammable)
            {
                yield return "[ABF] A race has the ABF_ArtificialPawnExtension with no legal pawn states! This extension should be removed from the race if it isn't meant to be artificial.";
            }

            if (blacklistedNeeds.NotNullAndContains(NeedDefOf.Food) || blacklistedDroneNeeds.NotNullAndContains(NeedDefOf.Food) || blacklistedSapientNeeds.NotNullAndContains(NeedDefOf.Food))
            {
                yield return "[ABF] A race was set to disable the Food need via the artificial pawn extension rather than by setting the race's FoodType to None. Vanilla code will error because of this.";
            }

            if (canBeReprogrammable && !inherentWorkTypes.NullOrEmpty() && !forbiddenWorkTypes.NullOrEmpty())
            {
                if (inherentWorkTypes.Any(workType => forbiddenWorkTypes.Contains(workType)))
                {
                    yield return "[ABF] A race was set to have reprogrammable drones configured with a workType that was both inherent and forbidden.";
                }
            }
        }
    }
}