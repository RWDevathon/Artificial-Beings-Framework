using HarmonyLib;
using System.Reflection;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace ArtificialBeings
{
    public class ArtificialBeings : Mod
    {
        public ArtificialBeings(ModContentPack content) : base(content)
        {
            new Harmony("ArtificialBeings").PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [StaticConstructorOnStartup]
    public static class ArtificialBeings_PostInit
    {
        static ArtificialBeings_PostInit()
        {
            // Acquire Defs for artificial butchering so that races are placed in the correct categories.
            RecipeDef artificialDisassembly = DefDatabase<RecipeDef>.GetNamed("ButcherCorpseMechanoid");
            RecipeDef artificialSmashing = DefDatabase<RecipeDef>.GetNamed("SmashCorpseMechanoid");
            RecipeDef butcherFlesh = DefDatabase<RecipeDef>.GetNamed("ButcherCorpseFlesh");

            // Some patches can't be run with the other harmony patches as Defs aren't loaded yet. So we patch them here.
            if (HealthCardUtility_Patch.DrawOverviewTab_Patch.Prepare())
            {
                new Harmony("ArtificialBeings").CreateClassProcessor(typeof(HealthCardUtility_Patch.DrawOverviewTab_Patch)).Patch();
            }

            List<ThingDef> allRaces = new List<ThingDef>();
            List<ThingDef> allMedicines = new List<ThingDef>();

            // Must dynamically modify some ThingDefs based on certain qualifications.
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                // Check race to see if the thingDef is for a Pawn.
                if (thingDef.race != null && !thingDef.IsCorpse)
                {
                    allRaces.Add(thingDef);
                    // Artificial beings have special considerations that need to be made.
                    if (thingDef.GetModExtension<ABF_ArtificialPawnExtension>() is ABF_ArtificialPawnExtension extension)
                    {
                        ThingDef corpseDef = thingDef.race?.corpseDef;
                        if (corpseDef != null)
                        {
                            // Eliminate rottable and spawnerFilth comps from artificial corpses.
                            corpseDef.comps.RemoveAll(compProperties => compProperties is CompProperties_Rottable || compProperties is CompProperties_SpawnerFilth);

                            // Put artifical disassembly in the machining table and crafting spot (smashing) and remove from the butcher table.
                            artificialDisassembly.fixedIngredientFilter.SetAllow(corpseDef, true);
                            artificialSmashing.fixedIngredientFilter.SetAllow(corpseDef, true);
                            butcherFlesh.fixedIngredientFilter.SetAllow(corpseDef, false);

                            // Make artificial corpses not edible.
                            IngestibleProperties ingestibleProps = corpseDef.ingestible;
                            if (ingestibleProps != null)
                            {
                                ingestibleProps.preferability = FoodPreferability.Undefined;
                            }
                        }

                        // Cache all bleeding hediffs associated with this race so that they are accounted for in health calculations.
                        List<HediffGiverSetDef> hediffGiverSetDefs = thingDef.race.hediffGiverSets;
                        List<KeyValuePair<HediffGiver_Leaking, float>> targetHediffPairs = new List<KeyValuePair<HediffGiver_Leaking, float>>();

                        // Cache if this race is vulnerable to EMP attacks.
                        if (extension.vulnerableToEMP)
                        {
                            ABF_Utils.cachedVulnerableToEMP.Add(thingDef);
                        }

                        if (hediffGiverSetDefs != null)
                        {
                            foreach (HediffGiverSetDef hediffGiverSetDef in hediffGiverSetDefs)
                            {
                                foreach (HediffGiver hediffGiver in hediffGiverSetDef.hediffGivers)
                                {
                                    if (hediffGiver is HediffGiver_Leaking artificialBleedingGiver)
                                    {
                                        float criticalThreshold = -1;
                                        foreach (HediffStage hediffStage in hediffGiver.hediff.stages)
                                        {
                                            if (hediffStage.lifeThreatening)
                                            {
                                                criticalThreshold = hediffStage.minSeverity;
                                                break;
                                            }
                                        }
                                        if (criticalThreshold < 0 && hediffGiver.hediff.lethalSeverity >= 0)
                                        {
                                            criticalThreshold = hediffGiver.hediff.lethalSeverity;
                                        }
                                        else if (criticalThreshold < 0)
                                        {
                                            criticalThreshold = hediffGiver.hediff.maxSeverity;
                                        }
                                        targetHediffPairs.Add(new KeyValuePair<HediffGiver_Leaking, float>(artificialBleedingGiver, criticalThreshold));
                                    }
                                }
                            }
                        }
                        if (targetHediffPairs.Count > 0)
                        {
                            ABF_Utils.cachedBleedingHediffGivers[thingDef] = targetHediffPairs;
                        }

                        // If the pawn replaces some normal hediffs with new ones, then cache them.
                        List<HediffReplacementRecord> hediffReplacements = extension.hediffReplacements;
                        if (hediffReplacements != null)
                        {
                            ABF_Utils.cachedArtificialHediffReplacements[thingDef] = hediffReplacements;
                        }

                        // Cache what medicines the unit may use and what medicines the race restricts.
                        foreach (ThingDef medicine in extension.medicineList)
                        {
                            ABF_Utils.cachedRestrictedMedicines.Add(medicine);
                            extension.whiteMedicineList.Add(medicine);
                        }
                    }
                }
                // Things that fulfill artificial needs should be marked appropriately.
                if (thingDef.HasModExtension<ABF_NeedFulfillerExtension>())
                {
                    ABF_NeedFulfillerExtension ingestibleExtension = thingDef.GetModExtension<ABF_NeedFulfillerExtension>();
                    if (ingestibleExtension != null && ingestibleExtension.needOffsetRelations != null)
                    {
                        foreach (NeedDef needDef in ingestibleExtension.needOffsetRelations.Keys)
                        {
                            if (ingestibleExtension.needOffsetRelations[needDef] < 0)
                            {
                                continue;
                            }

                            if (ABF_Utils.cachedArtificialNeedFulfillments.ContainsKey(needDef))
                            {
                                ABF_Utils.cachedArtificialNeedFulfillments[needDef].Add(thingDef);
                            }
                            else
                            {
                                ABF_Utils.cachedArtificialNeedFulfillments[needDef] = new List<ThingDef> { thingDef };
                            }
                        }
                    }
                }
                // Cache all medicines in order to later cache the dictionaries of all medicines races can use.
                if (thingDef.IsMedicine)
                {
                    allMedicines.Add(thingDef);
                }
            }

            // Cache and sort all directives for use in reprogramming.
            foreach (DirectiveDef def in DefDatabase<DirectiveDef>.AllDefsListForReading)
            {
                if (!ABF_Utils.cachedDirectiveCategories.Contains(def.directiveCategory))
                {
                    ABF_Utils.cachedDirectiveCategories.Add(def.directiveCategory);
                }

                ABF_Utils.cachedSortedDirectives.Add(def);
            }
            ABF_Utils.cachedSortedDirectives.SortBy(def => def.directiveCategory, def => def.exclusionTags?.FirstOrFallback() ?? def.label, def => def.complexityCost);

            // For all races and medicines, populate the dictionary of what medicines are legal for each race.
            foreach (ThingDef race in allRaces)
            {
                List<ThingDef> legalMedicines = new List<ThingDef>();
                for (int j = allMedicines.Count - 1; j >= 0; j--)
                {
                    if (ABF_Utils.IsMedicineValid(allMedicines[j], race))
                    {
                        legalMedicines.Add(allMedicines[j]);
                    }
                }
                legalMedicines.SortBy(thingDef => thingDef.statBases.GetStatValueFromList(StatDefOf.MedicalPotency, 0f));
                ABF_Utils.cachedRaceMedicines[race] = legalMedicines;
            }
        }
    }
}