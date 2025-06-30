using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace ArtificialBeings
{
    public class AnimalProductionUtility_Patch
    {
        // Animals using the artificial gatherable comp should have their products displayed just like organic products do.
        [HarmonyPatch(typeof(AnimalProductionUtility), "AnimalProductionStats")]
        public class AnimalProductionUtility_AnimalProductionStats_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref IEnumerable<StatDrawEntry> __result, ThingDef d)
            {
                List<StatDrawEntry> statDrawEntries = new List<StatDrawEntry>();
                // Make sure the original results are preserved.
                foreach (StatDrawEntry entry in __result)
                {
                    statDrawEntries.Add(entry);
                }

                // There needs to be a display priority to ensure things appear in order, and we want to try to keep a consistent order.
                int displayPriority;
                if (statDrawEntries.Count > 0)
                {
                    displayPriority = statDrawEntries[statDrawEntries.Count - 1].DisplayPriorityWithinCategory - 10;
                }
                else
                {
                    displayPriority = 10000;
                }

                foreach (CompProperties compProperty in d.comps)
                {
                    if (compProperty is CompProperties_ResourceProducerArtificial validProperty)
                    {
                        float resourcePerYear = GenDate.DaysPerYear / validProperty.resourceIntervalDays * validProperty.resourceCount;
                        float resourceValue = validProperty.resourceDef.BaseMarketValue;
                        float resourceValuePerYear = resourceValue * resourcePerYear;

                        statDrawEntries.Add(new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "ABF_Stat_Animal_ResourceType".Translate(), validProperty.resourceDef.LabelCap, "ABF_Stat_Animal_ResourceTypeDesc".Translate(), displayPriority, null, Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(validProperty.resourceDef))));
                        statDrawEntries.Add(new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "ABF_Stat_Animal_ResourceAmount".Translate(), validProperty.resourceCount.ToString(), "ABF_Stat_Animal_ResourceAmountDesc".Translate(), displayPriority - 10));
                        statDrawEntries.Add(new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "ABF_Stat_Animal_ResourceInterval".Translate(), "PeriodDays".Translate(validProperty.resourceIntervalDays.ToString("F2")), "ABF_Stat_Animal_ResourceIntervalDesc".Translate(), displayPriority - 20));
                        statDrawEntries.Add(new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "ABF_Stat_Animal_ResourcePerYear".Translate(), resourcePerYear.ToString("F0"), "ABF_Stat_Animal_ResourcePerYearDesc".Translate(), displayPriority - 30));
                        statDrawEntries.Add(new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "ABF_Stat_Animal_ResourceValue".Translate(), resourceValue.ToStringMoney(), "ABF_Stat_Animal_ResourceValueDesc".Translate(), displayPriority - 40));
                        statDrawEntries.Add(new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "ABF_Stat_Animal_ResourceValuePerYear".Translate(), resourceValuePerYear.ToStringMoney(), "ABF_Stat_Animal_ResourceAmountDesc".Translate(), displayPriority - 50));

                        displayPriority -= 60;
                    }
                }
                __result = statDrawEntries;
            }
        }
    }
}