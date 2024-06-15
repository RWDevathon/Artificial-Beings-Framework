using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;
using UnityEngine;

namespace ArtificialBeings
{
    public static class ABF_Utils
    {
        /* === GENERAL UTILITIES === */

        private const float complexityStepPerLevel = 0.5f;

        public static bool IsArtificial(Pawn pawn)
        {
            return PawnStateFor(pawn) != ABF_ArtificialState.Unknown;
        }

        public static bool IsArtificialBlank(Pawn pawn)
        {
            return PawnStateFor(pawn) == ABF_ArtificialState.Blank;
        }

        public static bool IsArtificialSapient(Pawn pawn)
        {
            return PawnStateFor(pawn) == ABF_ArtificialState.Sapient;
        }

        public static bool IsArtificialDrone(Pawn pawn)
        {
            return PawnStateFor(pawn) == ABF_ArtificialState.Drone || PawnStateFor(pawn) == ABF_ArtificialState.Reprogrammable;
        }

        public static bool IsProgrammableDrone(Pawn pawn)
        {
            return PawnStateFor(pawn) == ABF_ArtificialState.Reprogrammable;
        }

        // Some pawns may be treated as non-humanlikes even if they are one, such as drones. Other mods may wish to hook into this method to add extra qualifications.
        public static bool IsConsideredNonHumanlike(Pawn pawn)
        {
            return IsArtificialDrone(pawn) || PawnStateFor(pawn) == ABF_ArtificialState.Blank;
        }

        // Retrieve the current cached state of the pawn. Artificial pawns keep their states up to date. Non-artificial pawns have no states.
        public static ABF_ArtificialState PawnStateFor(Pawn pawn)
        {
            if (cachedPawnStates.ContainsKey(pawn.thingIDNumber))
            {
                return cachedPawnStates[pawn.thingIDNumber];
            }
            else
            {
                return ABF_ArtificialState.Unknown;
            }
        }

        // Update the current cached state of the pawn. Providing an unknown state removes them from the dictionary.
        // This does NOT change the state of the pawn itself. This should only be called by the Comp itself.
        internal static void UpdateStateFor(Pawn pawn, ABF_ArtificialState state)
        {
            if (state == ABF_ArtificialState.Unknown)
            {
                cachedPawnStates.Remove(pawn.thingIDNumber);
            }
            else
            {
                cachedPawnStates[pawn.thingIDNumber] = state;
            }
        }

        // TODO: This should not exist long-term.
        internal static void LogStates()
        {
            Log.Warning("There are " + cachedPawnStates.Count + " artificial pawns cached.");
            foreach (var thingID in cachedPawnStates.Keys)
            {
                Log.Warning("Pawn " + thingID + " has state " + cachedPawnStates[thingID]);
            }
        }

        /* === HEALTH UTILITIES === */

        public static bool IsMedicineValid(ThingDef medicine, ThingDef race)
        {
            ABF_ArtificialPawnExtension pawnExtension = race.GetModExtension<ABF_ArtificialPawnExtension>();
            bool result = true;

            if (cachedRestrictedMedicines.Contains(medicine) || (pawnExtension?.onlyUseRaceRestrictedMedicine ?? false))
            {
                result = pawnExtension?.whiteMedicineList?.Contains(medicine) ?? false;
            }

            return result && !(pawnExtension?.blackMedicineList?.Contains(medicine) ?? false);
        }

        public static Texture2D GetVanillaMedicalIcon(int index)
        {
            index = Mathf.Clamp(index, 0, 4);

            if (cachedMedicalCareIcons == null)
            {
                cachedMedicalCareIcons = new List<Texture2D>()
                {
                    ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoCare"),
                    ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoMeds"),
                    ThingDefOf.MedicineHerbal.uiIcon,
                    ThingDefOf.MedicineIndustrial.uiIcon,
                    ThingDefOf.MedicineUltratech.uiIcon
                };
            }
            return cachedMedicalCareIcons[index];
        }

        // Very specific Util that takes a list of medicines and returns a list of the indices marking which ones are the "thresholds" between categories.
        public static List<int> GetCategoryMarkerIndices(List<ThingDef> legalMedicines)
        {
            List<int> result = new List<int>() { -1, -1, -1, -1, legalMedicines.Count - 1 };
            if (legalMedicines.Count == 0)
            {
                return result;
            }

            int index = 0;
            MedicalCareCategory category = MedicalCareCategory.HerbalOrWorse;
            while (index <= legalMedicines.Count - 2 && category < MedicalCareCategory.Best)
            {
                if (category.AllowsMedicine(legalMedicines[index]) && !category.AllowsMedicine(legalMedicines[index + 1]))
                {
                    result[(int)category] = index;
                    category++;
                }
                else if (!category.AllowsMedicine(legalMedicines[index]))
                {
                    category++;
                    continue;
                }
                index++;
            }
            return result;
        }

        // Handle various parts of resetting drones to default status.
        public static void ReconfigureDrone(Pawn pawn)
        {
            // Humanlike-intelligence drones
            if (pawn.RaceProps.intelligence == Intelligence.Humanlike)
            {
                ABF_ArtificialPawnExtension pawnExtension = pawn.def.GetModExtension<ABF_ArtificialPawnExtension>();
                if (pawnExtension?.dronesCanHaveTraits == false)
                {
                    foreach (Trait trait in pawn.story.traits.allTraits.ToList())
                    {
                        pawn.story.traits.RemoveTrait(trait);
                    }
                }

                // Drones don't have ideos.
                pawn.ideo = null;

                // Drones always take their last name (ID #) as their nickname.
                if (pawn.Name is NameTriple name)
                {
                    pawn.Name = new NameTriple(name.First, name.Last, name.Last);
                }

                // Drone skill levels start at their baseline level for true drones and at 0 for reprogrammable drones. The inherentSkills is then added/subtracted to that baseline.
                // Since both true drones and reprogrammable drones are incapable of learning, their passions and xp does not matter. It should be set to 0 for simplicity.
                int baselineSkill = IsProgrammableDrone(pawn) ? 0 : (pawnExtension?.droneSkillLevel ?? 4);

                foreach(SkillRecord skillRecord in pawn.skills.skills)
                {
                    skillRecord.passion = 0;
                    skillRecord.xpSinceLastLevel = 0;
                    skillRecord.Level = baselineSkill + pawnExtension?.inherentSkills?.TryGetValue(skillRecord.def, 0) ?? 0;
                }
            }
            // Animal-intelligence drones need to handle their training and a few other details.
            else if (pawn.RaceProps.intelligence == Intelligence.Animal)
            {

            }
        }

        // Caches and returns the PawnKindDefs that may be used as "starter" drones. These are generally the kinds that are spawned when produced by players.
        public static HashSet<PawnKindDef> GetStarterDroneKinds()
        {
            if (cachedFriendlyDroneKinds == null)
            {
                cachedFriendlyDroneKinds = new HashSet<PawnKindDef>();
                foreach (PawnKindDef pawnKindDef in DefDatabase<PawnKindDef>.AllDefsListForReading)
                {
                    if (pawnKindDef.defaultFactionType?.isPlayer == true && pawnKindDef.race.GetModExtension<ABF_ArtificialPawnExtension>() is ABF_ArtificialPawnExtension extension && extension.canBeDrone)
                    {
                        cachedFriendlyDroneKinds.Add(pawnKindDef);
                    }
                }
            }
            return cachedFriendlyDroneKinds;
        }

        // Caches and returns the HediffDefs contained in the HediffGiver of a given ThingDef's race.
        public static HashSet<HediffDef> GetTemperatureHediffDefs(ThingDef thingDef)
        {
            try
            {
                if (!cachedTemperatureHediffs.ContainsKey(thingDef))
                {
                    List<HediffGiverSetDef> hediffGiverSetDefs = thingDef.race.hediffGiverSets;
                    HashSet<HediffDef> targetHediffs = new HashSet<HediffDef>();

                    if (hediffGiverSetDefs != null)
                    {
                        foreach (HediffGiverSetDef hediffGiverSetDef in hediffGiverSetDefs)
                        {
                            foreach (HediffGiver hediffGiver in hediffGiverSetDef.hediffGivers)
                            {
                                if (typeof(HediffGiver_Heat).IsAssignableFrom(hediffGiver.GetType()) || typeof(HediffGiver_Hypothermia).IsAssignableFrom(hediffGiver.GetType()))
                                {
                                    targetHediffs.Add(hediffGiver.hediff);
                                }
                            }
                        }
                    }
                    cachedTemperatureHediffs[thingDef] = targetHediffs;
                }
                return cachedTemperatureHediffs[thingDef];
            }
            catch (Exception ex)
            {
                Log.Warning("[ABF] Encountered an error while trying to get temperature HediffDefs for a specific race. Returning empty set." + ex.Message + ex.StackTrace);
                return new HashSet<HediffDef>();
            }
        }

        // Utility method that will return the number of ticks before a given pawn will reach a critical threshold.
        public static int TicksUntilCriticalFailure(Pawn pawn, KeyValuePair<HediffGiver_Leaking, float> bleedHediffPair)
        {
            float bleedRateTotal = pawn.health.hediffSet.BleedRateTotal;
            Hediff targetHediff = pawn.health.hediffSet.GetFirstHediffOfDef(bleedHediffPair.Key.hediff);
            float severityDifference = Math.Max(0, bleedHediffPair.Value - (targetHediff?.Severity ?? 0));
            return (int)(severityDifference / bleedRateTotal * GenDate.TicksPerDay * bleedHediffPair.Key.riseRatePerDay);
        }

        /* Reprogrammable Drone Utilities */

        // Given a pawn and skill, calculate and return the complexity cost of the pawn's current skill level.
        public static float SkillComplexityCostFor(Pawn pawn, SkillDef skill)
        {
            int offsetSkillLevel = pawn.skills.GetSkill(skill).Level - pawn.def.GetModExtension<ABF_ArtificialPawnExtension>().inherentSkills.GetWithFallback(skill, 0);
            if (offsetSkillLevel < 0)
            {
                Log.Warning("[ABF] A programmable drone, " + pawn.LabelShortCap + ", had a lower skill level than their inherent skills. Complexity calculations may be incorrect.");
            }
            return complexityStepPerLevel * offsetSkillLevel;
        }

        // Given a pawn and skill, calculate the maximum level the pawn may have in the skill.
        public static float SkillLimitFor(Pawn pawn, SkillDef skill)
        {
            return pawn.GetStatValue(ABF_StatDefOf.ABF_Stat_Artificial_SkillLimit) + pawn.def.GetModExtension<ABF_ArtificialPawnExtension>()?.inherentSkills?.GetWithFallback(skill, 0) ?? 0;
        }

        public static void Deprogram(Pawn pawn)
        {
            if (!IsProgrammableDrone(pawn))
            {
                Log.Warning("[ABF] A pawn " + pawn.LabelShortCap + ", who is not a programmable drone, had Deprogram called on it.");
                return;
            }

            CompArtificialPawn reprogramComp = pawn.GetComp<CompArtificialPawn>();
            ABF_ArtificialPawnExtension reprogramExtension = pawn.def.GetModExtension<ABF_ArtificialPawnExtension>();

            // WorkTypes
            reprogramComp.InitializeEnabledWorkTypes();
            reprogramComp.UpdateComplexity("Work Types", 0);

            // Skills
            foreach (SkillRecord skillRecord in pawn.skills?.skills)
            {
                if (reprogramExtension.inherentSkills?.ContainsKey(skillRecord.def) == true)
                {
                    skillRecord.Level = reprogramExtension.inherentSkills[skillRecord.def];
                }
                else
                {
                    skillRecord.Level = 0;
                }
                skillRecord.passion = 0;
                skillRecord.xpSinceLastLevel = 0;
                skillRecord.xpSinceMidnight = 0;
            }
            reprogramComp.UpdateComplexity("Skills", 0);

            // Directives
            reprogramComp.SetDirectives(pawn.def.GetModExtension<ABF_ArtificialPawnExtension>().inherentDirectives);
            reprogramComp.UpdateComplexity("Active Directives", 0);
        }

        // Randomize the given programmable drone's skills, work types, and directives within the constraints of their pawn kind def.
        // If pawn group maker parms is provided as context, randomization will take into account the unit's "role" in a group.
        public static void RandomizeProgrammableDrone(Pawn pawn, PawnGroupMakerParms context = null)
        {
            Deprogram(pawn);

            // Purchase required work types
            SetRequiredDroneWorkTypes(pawn, context);

            // Purchase skills to be in range min
            SetRequiredDroneSkills(pawn, context);

            // Purchase directives to be in range min
            SetRequiredDroneDirectives(pawn, context);

            // With spare complexity, randomly choose to use or not use on directives, work types, and skills.
            RandomizeDroneCharacteristics(pawn, context);
        }

        // Randomize programmable drone's work types. If requiredOnly is true, it will only apply required work types from the pawn kind def.
        // If it is false, it will select random work types that it does not already have.
        public static void SetRequiredDroneWorkTypes(Pawn pawn, PawnGroupMakerParms context = null)
        {
            ABF_ArtificialPawnExtension pawnExtension = pawn.def.GetModExtension<ABF_ArtificialPawnExtension>();
            CompArtificialPawn pawnComp = pawn.GetComp<CompArtificialPawn>();
            PawnKindDef pawnKindDef = pawn.kindDef;

            int requiredWorkTypeComplexity = 0;
            List<WorkTypeDef> legalWorkTypes = new List<WorkTypeDef>();
            List<WorkTypeDef> combatWorkTypes = new List<WorkTypeDef>();
            foreach (WorkTypeDef workTypeDef in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if ((workTypeDef.workTags & WorkTags.Violent) != WorkTags.None)
                {
                    combatWorkTypes.Add(workTypeDef);
                }

                if ((workTypeDef.workTags != WorkTags.None || !workTypeDef.relevantSkills.NullOrEmpty())
                    && !pawnExtension.forbiddenWorkTypes.NotNullAndContains(workTypeDef)
                    && (workTypeDef.GetModExtension<ABF_WorkTypeExtension>()?.ValidFor(pawn).Accepted ?? true)
                    && !pawnComp.enabledWorkTypes.Contains(workTypeDef))
                {
                    legalWorkTypes.Add(workTypeDef);
                }
            }

            if (pawnKindDef.requiredWorkTags != WorkTags.None)
            {
                foreach (WorkTypeDef workTypeDef in legalWorkTypes)
                {
                    if ((workTypeDef.workTags & pawnKindDef.requiredWorkTags) != WorkTags.None)
                    {
                        pawnComp.enabledWorkTypes.Add(workTypeDef);

                        requiredWorkTypeComplexity += ComplexityCostFor(workTypeDef, pawn, true);
                    }
                }
            }

            // Ensure the pawn has all combat work types enabled if it must be a fighter. They get these work types for free.
            if (context != null && context.raidStrategy != null)
            {
                foreach (WorkTypeDef combatWorkTypeDef in combatWorkTypes)
                {
                    if ((combatWorkTypeDef.workTags & WorkTags.Violent) != WorkTags.None)
                    {
                        pawnComp.enabledWorkTypes.Add(combatWorkTypeDef);
                    }
                }
            }

            if (requiredWorkTypeComplexity != 0)
            {
                pawn.Notify_DisabledWorkTypesChanged();
                pawnComp.UpdateComplexity("Work Types", requiredWorkTypeComplexity + pawnComp.GetComplexityFromSource("Work Types"));
            }
        }

        // Randomize programmable drone's skills based on the group kind context and the pawn kind.
        public static void SetRequiredDroneSkills(Pawn pawn, PawnGroupMakerParms context = null)
        {
            CompArtificialPawn pawnComp = pawn.GetComp<CompArtificialPawn>();
            PawnKindDef pawnKindDef = pawn.kindDef;
            List<SkillRange> skillRanges = pawnKindDef.skills;

            if (!skillRanges.NullOrEmpty())
            {
                float requiredSkillComplexity = pawnComp.GetComplexityFromSource("Skills");
                foreach (SkillRange skillRange in skillRanges)
                {
                    SkillDef skillDef = skillRange.Skill;
                    SkillRecord skillRecord = pawn.skills.GetSkill(skillDef);
                    int skillFloor = skillRange.Range.min;
                    int skillLevel = skillRecord.Level;
                    if (skillLevel >= skillFloor)
                    {
                        continue;
                    }

                    float skillComplexityCost = SkillComplexityCostFor(pawn, skillDef);
                    float skillComplexityUsage = 0;
                    while (skillLevel < skillFloor)
                    {
                        skillComplexityUsage += skillComplexityCost;
                        skillComplexityCost += complexityStepPerLevel;
                        skillLevel++;
                    }
                    requiredSkillComplexity += skillComplexityUsage;
                    skillRecord.Level = skillFloor;
                }
                pawnComp.UpdateComplexity("Skills", Mathf.CeilToInt(requiredSkillComplexity));
            }
        }

        // Randomize programmable drone's directives based on the group kind context and the pawn kind.
        public static void SetRequiredDroneDirectives(Pawn pawn, PawnGroupMakerParms context = null)
        {
            CompArtificialPawn pawnComp = pawn.GetComp<CompArtificialPawn>();
            PawnKindDef pawnKindDef = pawn.kindDef;
            ABF_ArtificialPawnKindExtension pawnKindExtension = pawnKindDef.GetModExtension<ABF_ArtificialPawnKindExtension>();
            ABF_ArtificialPawnExtension pawnExtension = pawn.def.GetModExtension<ABF_ArtificialPawnExtension>();

            List<DirectiveDef> activeDirectiveDefs = new List<DirectiveDef>();
            activeDirectiveDefs.AddRange(pawnExtension.inherentDirectives);
            activeDirectiveDefs.AddRange(pawnComp.ActiveDirectives);
            pawnComp.SetDirectives(activeDirectiveDefs);

            if (pawnKindExtension?.requiredDirectives.NullOrEmpty() == false)
            {
                int requiredDirectiveComplexity = 0;
                foreach (DirectiveDef directiveDef in pawnKindExtension.requiredDirectives)
                {
                    if (activeDirectiveDefs.Contains(directiveDef))
                    {
                        continue;
                    }

                    requiredDirectiveComplexity += directiveDef.complexityCost;
                    activeDirectiveDefs.Add(directiveDef);
                }

                pawnComp.UpdateComplexity("Active Directives", requiredDirectiveComplexity + pawnComp.GetComplexityFromSource("Active Directives"));
                pawnComp.SetDirectives(activeDirectiveDefs);
            }
        }


        // Randomize programmable drone's directives, work types, and skills. This is dependent upon the pawn kind primarily, and assumes required characteristics are already set.
        public static void RandomizeDroneCharacteristics(Pawn pawn, PawnGroupMakerParms context = null)
        {
            CompArtificialPawn pawnComp = pawn.GetComp<CompArtificialPawn>();
            PawnKindDef pawnKindDef = pawn.kindDef;
            ABF_ArtificialPawnKindExtension pawnKindExtension = pawnKindDef.GetModExtension<ABF_ArtificialPawnKindExtension>();
            ABF_ArtificialPawnExtension pawnExtension = pawn.def.GetModExtension<ABF_ArtificialPawnExtension>();
            List<SkillRange> skillRanges = pawnKindDef.skills;

            // Get the complexity spare for discretionary spending on purchasing random effects. If the pawn kind does not exist, give 0-10 complexity.
            float discretionaryComplexity;
            if (pawnKindExtension == null)
            {
                discretionaryComplexity = Rand.RangeInclusive(0, 10);
            }
            else
            {
                discretionaryComplexity = pawnKindExtension.discretionaryComplexity.RandomInRange;
            }

            if (discretionaryComplexity < complexityStepPerLevel)
            {
                return;
            }

            // Randomize directives
            List<DirectiveDef> activeDirectiveDefs = new List<DirectiveDef>();
            activeDirectiveDefs.AddRange(pawnComp.ActiveDirectives);
            int discretionaryDirectives = Mathf.Min(
                pawnKindExtension?.discretionaryDirectives.RandomInRange ?? 0,
                pawnExtension.maxDirectives - (activeDirectiveDefs.Count - pawnExtension.inherentDirectives?.Count ?? 0));

            if (discretionaryDirectives > 0)
            {
                List<DirectiveDef> desiredDirectiveDefs = new List<DirectiveDef>();
                desiredDirectiveDefs.AddRange(pawnComp.ActiveDirectives);

                // Acquire a list of legal directives with their weighted chance to be selected in this instance as a pair.
                List<KeyValuePair<DirectiveDef, float>> legalDirectiveDefsWeighted = new List<KeyValuePair<DirectiveDef, float>>();
                foreach (DirectiveDef directiveDef in cachedSortedDirectives)
                {
                    // If the pawn already has this directive, skip it.
                    if (activeDirectiveDefs.Contains(directiveDef))
                    {
                        continue;
                    }

                    // Only valid directives with complexity lower than the spending limit are acceptable.
                    if (directiveDef.ValidFor(pawn) && directiveDef.complexityCost < discretionaryComplexity)
                    {
                        float selectionWeight = 1f;
                        // If this pawn is part of a group, consider the group kind weights of the directive.
                        if (!directiveDef.groupKindWeights.NullOrEmpty() && context != null)
                        {
                            float groupKindWeight = directiveDef.groupKindWeights.GetWithFallback(context.groupKind, 1f);
                            // If the group kind weight indicates this directive is undesirable, skip to the next directive.
                            if (groupKindWeight <= 0)
                            {
                                continue;
                            }
                            else
                            {
                                selectionWeight *= groupKindWeight;
                            }
                        }

                        // If this directive has skill weights, consider those weights in comparison to the pawn's skills.
                        if (!directiveDef.skillChoiceWeights.NullOrEmpty())
                        {
                            float skillWeight = 1f;
                            foreach (KeyValuePair<SkillDef, float> skillWeightPair in directiveDef.skillChoiceWeights)
                            {
                                SkillRecord pawnSkill = pawn.skills.GetSkill(skillWeightPair.Key);
                                // Positive weights mean the skill must be enabled, non-positive weights mean the skill must be disabled.
                                if ((pawnSkill.TotallyDisabled && skillWeightPair.Value > 0f) || (!pawnSkill.TotallyDisabled && skillWeightPair.Value <= 0))
                                {
                                    continue;
                                }
                                else
                                {
                                    skillWeight *= skillWeightPair.Value;
                                }
                            }
                            selectionWeight *= skillWeight;
                        }

                        // Pair the calculated selection weight and the legal def for use in randomization.
                        legalDirectiveDefsWeighted.Add(new KeyValuePair<DirectiveDef, float>(directiveDef, selectionWeight));
                    }
                }

                if (legalDirectiveDefsWeighted.Count > 0)
                {
                    int directiveComplexity = 0;
                    // Keep picking random directives until there's no valid ones, complexity left, or directive slots left.
                    while (legalDirectiveDefsWeighted.Count > 0 && discretionaryComplexity > 0 && discretionaryDirectives > 0)
                    {
                        legalDirectiveDefsWeighted.TryRandomElementByWeight((valuePair => valuePair.Value), out KeyValuePair<DirectiveDef, float> result);
                        // Previously added directives may change the discretionaryComplexity available without recalculating validity for the list.
                        if (result.Key.complexityCost > discretionaryComplexity)
                        {
                            legalDirectiveDefsWeighted.Remove(result);
                        }
                        else
                        {
                            activeDirectiveDefs.Add(result.Key);
                            legalDirectiveDefsWeighted.Remove(result);
                            directiveComplexity += result.Key.complexityCost;
                            discretionaryComplexity -= result.Key.complexityCost;
                            discretionaryDirectives--;
                        }
                    }
                    pawnComp.UpdateComplexity("Active Directives", directiveComplexity + pawnComp.GetComplexityFromSource("Active Directives"));
                    pawnComp.SetDirectives(activeDirectiveDefs);
                }
            }

            // If all discretionary complexity was used by directives, stop here.
            if (discretionaryComplexity < complexityStepPerLevel)
            {
                return;
            }

            // Randomize Work Types
            int discretionaryWorkTypes = pawnKindExtension?.discretionaryWorkTypes.RandomInRange ?? 0;
            if (discretionaryWorkTypes > 0)
            {
                List<WorkTypeDef> legalWorkTypes = new List<WorkTypeDef>();
                foreach (WorkTypeDef workTypeDef in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    ABF_WorkTypeExtension workExtension = workTypeDef.GetModExtension<ABF_WorkTypeExtension>();
                    if ((workTypeDef.workTags != WorkTags.None || !workTypeDef.relevantSkills.NullOrEmpty())
                        && !pawnExtension.forbiddenWorkTypes.NotNullAndContains(workTypeDef)
                        && (workExtension?.ValidFor(pawn).Accepted ?? true) && !pawnComp.enabledWorkTypes.Contains(workTypeDef)
                        && discretionaryComplexity >= ComplexityCostFor(workTypeDef, pawn, true))
                    {
                        legalWorkTypes.Add(workTypeDef);
                    }
                }

                int discretionaryWorkTypeComplexity = 0;
                while (legalWorkTypes.Count > 0 && discretionaryComplexity > 0 && discretionaryWorkTypes > 0)
                {
                    legalWorkTypes.TryRandomElement(out WorkTypeDef result);
                    int workTypeCost = ComplexityCostFor(result, pawn, true);
                    if (workTypeCost > discretionaryComplexity)
                    {
                        legalWorkTypes.Remove(result);
                    }
                    else
                    {
                        pawnComp.enabledWorkTypes.Add(result);
                        legalWorkTypes.Remove(result);
                        discretionaryWorkTypeComplexity += workTypeCost;
                        discretionaryComplexity -= workTypeCost;
                        discretionaryWorkTypes--;
                    }
                }

                pawn.Notify_DisabledWorkTypesChanged();
                pawnComp.UpdateComplexity("Work Types", discretionaryWorkTypeComplexity + pawnComp.GetComplexityFromSource("Work Types"));
            }

            // If all discretionary complexity was used by work types, stop here.
            if (discretionaryComplexity < complexityStepPerLevel)
            {
                return;
            }

            // Randomize Skills

            // Assemble a dictionary matching skill records to pairs of the cost to add and the maximum level possible for the skill.
            Dictionary<SkillRecord, DroneSkillContext> skillsToRandomize = new Dictionary<SkillRecord, DroneSkillContext>();
            foreach (SkillRecord skillRecord in pawn.skills.skills)
            {
                if (skillRecord.TotallyDisabled)
                {
                    continue;
                }
                skillsToRandomize[skillRecord] = new DroneSkillContext(skillRecord);
            }

            if (skillsToRandomize.Count > 0)
            {
                float requiredSkillComplexity = 0;
                while (skillsToRandomize.Count > 0 && discretionaryComplexity > 0)
                {
                    skillsToRandomize.TryRandomElement(out KeyValuePair<SkillRecord, DroneSkillContext> randomSkill);
                    if (randomSkill.Key.Level >= randomSkill.Value.skillCeiling)
                    {
                        skillsToRandomize.Remove(randomSkill.Key);
                    }
                    else
                    {
                        randomSkill.Key.Level++;
                        requiredSkillComplexity += randomSkill.Value.skillComplexityCost;
                        discretionaryComplexity -= randomSkill.Value.skillComplexityCost;
                        randomSkill.Value.skillComplexityCost += complexityStepPerLevel;
                    }
                }
                pawnComp.UpdateComplexity("Skills", Mathf.Max(0, Mathf.CeilToInt(requiredSkillComplexity + pawnComp.GetComplexityFromSource("Skills"))));
            }
        }

        // Some SkillDefs are disabled by WorkTags rather than by WorkTypes alone. We need to keep track of them here.
        private static List<SkillDef> cachedSkillsDisabledByWorkTags = new List<SkillDef>();

        // For the given work type def, pawn, and whether it is adding or removing, determine the complexity that would be spent or refunded.
        public static int ComplexityCostFor(WorkTypeDef workTypeDef, Pawn pawn, bool adding)
        {
            if (cachedSkillsDisabledByWorkTags == null)
            {
                foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs)
                {
                    if (skillDef.disablingWorkTags != WorkTags.None)
                    {
                        cachedSkillsDisabledByWorkTags.Add(skillDef);
                    }
                }
            }

            int result = workTypeDef.GetModExtension<ABF_WorkTypeExtension>()?.baseComplexity ?? 1;
            if (adding)
            {
                List<SkillDef> addedSkills = new List<SkillDef>();
                // The additional cost is only applied to skills that are currently disabled.
                // It does not apply if another work type enabled it already.
                foreach (SkillDef skillDef in workTypeDef.relevantSkills)
                {
                    if (pawn.skills.GetSkill(skillDef).TotallyDisabled)
                    {
                        result += 1;
                        addedSkills.Add(skillDef);
                    }
                }

                // Some skills are disabled by WorkTags rather than by WorkTypes. Account for them.
                // Only count skills that are disabled by a WorkTag this type has, but that no other work type directly has.
                WorkTags workTags = workTypeDef.workTags;
                foreach (SkillDef skillDef in cachedSkillsDisabledByWorkTags)
                {
                    if (pawn.skills.GetSkill(skillDef).TotallyDisabled && (skillDef.disablingWorkTags & workTags) != WorkTags.None && !addedSkills.Contains(skillDef)
                        && !DefDatabase<WorkTypeDef>.AllDefsListForReading.Any(workType => workType.relevantSkills.NotNullAndContains(skillDef) && !skillDef.neverDisabledBasedOnWorkTypes))
                    {
                        result += 1;
                    }
                }
            }
            else
            {
                List<SkillDef> addedSkills = new List<SkillDef>();
                // In order to properly calculate the complexity cost reduction, the worker must take into account other enabled work types.
                // It will only refund the additional complexity for skills that have 0 other work types enabling them.
                List<WorkTypeDef> otherEnabledWorkTypes = new List<WorkTypeDef>();
                foreach (WorkTypeDef enabledWorkTypeDef in pawn.GetComp<CompArtificialPawn>().enabledWorkTypes)
                {
                    if (enabledWorkTypeDef != workTypeDef)
                    {
                        otherEnabledWorkTypes.Add(enabledWorkTypeDef);
                    }
                }

                // Refund all skills that would be disabled if the parent work type were removed. Account for other work types.
                foreach (SkillDef skillDef in workTypeDef.relevantSkills)
                {
                    if (!otherEnabledWorkTypes.Any(workType => workType.relevantSkills.NotNullAndContains(skillDef)) && !skillDef.neverDisabledBasedOnWorkTypes)
                    {
                        result += 1;
                        addedSkills.Add(skillDef);
                    }
                }

                // Some skills are disabled by WorkTags rather than by WorkTypes. Account for them.
                // Only count skills that are disabled by a WorkTag this type has, but that no other work type directly has.
                foreach (SkillDef skillDef in cachedSkillsDisabledByWorkTags)
                {
                    WorkTags workTags = skillDef.disablingWorkTags;
                    if (!otherEnabledWorkTypes.Any(workType => (workType.workTags & workTags) != WorkTags.None) && !addedSkills.Contains(skillDef) && !pawn.skills.GetSkill(skillDef).TotallyDisabled
                        && !otherEnabledWorkTypes.Any(workType => workType.relevantSkills.NotNullAndContains(skillDef)))
                    {
                        result += 1;
                    }
                }
            }
            return result;
        }



        // Vanilla keeps a private cache of icons for medical care categories that we want to be able to use. Easier to cache it ourselves as needed.
        private static List<Texture2D> cachedMedicalCareIcons;

        // Cached list of player drone pawnkinds that may be used for scenario starts, cached only when needed.
        private static HashSet<PawnKindDef> cachedFriendlyDroneKinds;

        // Cached Hediffs for a particular pawn's race that count as temperature hediffs to avoid recalculation, cached when needed.
        private static Dictionary<ThingDef, HashSet<HediffDef>> cachedTemperatureHediffs = new Dictionary<ThingDef, HashSet<HediffDef>>();

        // Cached Dictionary of pawns' thingIDNumbers matched to their stored artificial state, cached when needed.
        internal static Dictionary<int, ABF_ArtificialState> cachedPawnStates = new Dictionary<int, ABF_ArtificialState>();

        // Cached dictionary matching artificial races to a dictionary of organic hediffs to be replaced by artificial equivalents.
        public static Dictionary<ThingDef, Dictionary<HediffDef, HediffDef>> cachedArtificialHediffReplacements = new Dictionary<ThingDef, Dictionary<HediffDef, HediffDef>>();

        // Cached HashSet of medicines that are restricted to particular races. Cached at startup.
        public static HashSet<ThingDef> cachedRestrictedMedicines = new HashSet<ThingDef>();

        // Cached dictionary matching races to a list of ThingDefs that are medicines that they may use. Cached at startup. Ordered from least to most medically potent.
        public static Dictionary<ThingDef, List<ThingDef>> cachedRaceMedicines = new Dictionary<ThingDef, List<ThingDef>>();

        // Cached dictionary matching artificial NeedDefs to the ingestible items which can satisfy that need, cached at startup.
        public static Dictionary<NeedDef, List<ThingDef>> cachedArtificialNeedFulfillments = new Dictionary<NeedDef, List<ThingDef>>();

        // Cached list of races that are vulnerable to EMP attacks. Cached at startup.
        public static List<ThingDef> cachedVulnerableToEMP = new List<ThingDef>();

        // Cached Hediffs and the severity it is considered critical for a particular pawn that are handled by HediffGiver_Bleeding so they may be appropriately checked, cached at startup.
        public static Dictionary<ThingDef, List<KeyValuePair<HediffGiver_Leaking, float>>> cachedBleedingHediffGivers = new Dictionary<ThingDef, List<KeyValuePair<HediffGiver_Leaking, float>>>();

        // Cached list of DirectiveDefs that are sorted for use in dialogs to reprogram proper drones. Cached at startup.
        public static List<DirectiveDef> cachedSortedDirectives = new List<DirectiveDef>();

        // Cached list of strings for the categories for directives for use in dialogs. Cached at startup.
        public static List<string> cachedDirectiveCategories = new List<string>();

    }
}
