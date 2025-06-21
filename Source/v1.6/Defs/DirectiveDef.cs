using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace ArtificialBeings
{
    public class DirectiveDef : Def
    {
        public Type directiveClass = typeof(Directive);

        [NoTranslate]
        public string iconPath;

        [Unsaved(false)]
        private Texture2D cachedIcon;

        [Unsaved(false)]
        private string cachedCustomDescription;

        // List of abilities that the drone will be able to use as long as this directive is active.
        public List<AbilityDef> abilities;

        public List<StatModifier> statOffsets;

        public List<StatModifier> statFactors;

        // For energy, which isn't a stat for some reason.
        public float hungerRateFactor = 1f;

        // If provided, adding/removing this directive will add/remove an instance of this hediff.
        public HediffDef associatedHediff;

        // Category for grouping together directives on the reprogramming interface. Is not used anywhere else.
        // Defaults to "Uncategorized" if left null.
        public string directiveCategory;

        // List of tags that this directive should be associated with for validation purposes.
        // Note that this does not do anything itself - it can be used by requirement workers for enforcing rules.
        public List<string> exclusionTags;

        // How much complexity this directive costs (or provides) to a drone.
        public int complexityCost = 1;

        // Directives are not ticked by default. Setting this to a value (Normal, Rare, or Long) will have instances of this directive def
        // added to a WorldComponent for ticking purposes.
        public TickerType tickerType;

        // Description that describes unique effects of the directive that belong in the effects group rather than the directive description.
        public string customEffectsDescription;

        // List of requirement workers that enforce rules for how this directive may be used.
        public List<DirectiveRequirementWorker> requirementWorkers = new List<DirectiveRequirementWorker>();

        // Optional weights to configure whether pawns should or should not be encouraged to "buy" this directive as part of a particular group kind.
        public Dictionary<PawnGroupKindDef, float> groupKindWeights = new Dictionary<PawnGroupKindDef, float>();

        // Optional weights to configure whether pawns should or should not be encouraged to "buy" this directive depending on their skills.
        // Setting this above 0 will mean pawns will not take it if the corresponding skill is absent, and vice versa.
        public Dictionary<SkillDef, float> skillChoiceWeights = new Dictionary<SkillDef, float>();

        // Optional int for offsetting the quality of all items manufactured by pawns with this directive (positive or negative).
        public int producedItemQualityOffset = 0;

        // Optional QualityCategory for specifying a mandatory quality of all items manufactured by pawns with this directive (overrides above field).
        // If there are two directives with conflicting forced item qualities, the lowest quality one will be chosen.
        public QualityCategory? forcedItemQuality = null;

        public Texture2D Icon
        {
            get
            {
                if (cachedIcon == null)
                {
                    if (iconPath.NullOrEmpty())
                    {
                        cachedIcon = ABF_Textures.complexityIcon;
                    }
                    else
                    {
                        cachedIcon = ContentFinder<Texture2D>.Get(iconPath) ?? BaseContent.BadTex;
                    }
                }
                return cachedIcon;
            }
        }

        public string CustomDescription
        {
            get
            {
                if (cachedCustomDescription == null)
                {
                    StringBuilder customDescription = new StringBuilder();
                    customDescription.AppendLine("ABF_DirectiveDefHeader".Translate().Colorize(ColoredText.TipSectionTitleColor));
                    if (customEffectsDescription != null)
                    {
                        customDescription.AppendLine(customEffectsDescription);
                    }
                    if (!abilities.NullOrEmpty())
                    {
                        string abilityLabels = abilities.Join(abilityDef => abilityDef.LabelCap);
                        customDescription.AppendLine("ABF_DirectiveDefAbilities".Translate(abilityLabels));
                    }
                    if (associatedHediff != null)
                    {
                        customDescription.AppendLine("ABF_DirectiveDefHediff".Translate(associatedHediff.label));
                    }

                    if (statOffsets != null)
                    {
                        customDescription.AppendLine();
                        customDescription.AppendLine("ABF_DirectiveDefStatOffsetHeader".Translate().Colorize(ColoredText.TipSectionTitleColor));
                        foreach (StatModifier statOffset in statOffsets)
                        {
                            customDescription.AppendLine("ABF_DirectiveDefStatIndent".Translate(statOffset.stat.LabelForFullStatListCap, statOffset.ValueToStringAsOffset));
                        }
                    }
                    if (statFactors != null)
                    {
                        customDescription.AppendLine();
                        customDescription.AppendLine("ABF_DirectiveDefStatFactorHeader".Translate().Colorize(ColoredText.TipSectionTitleColor));
                        foreach (StatModifier statFactor in statFactors)
                        {
                            customDescription.AppendLine("ABF_DirectiveDefStatIndent".Translate(statFactor.stat.LabelForFullStatListCap, statFactor.ToStringAsFactor));
                        }
                    }
                    if (forcedItemQuality != null)
                    {
                        customDescription.AppendLine();
                        customDescription.AppendLine("ABF_DirectiveDefForcedItemQualityHeader".Translate().Colorize(ColoredText.TipSectionTitleColor));
                        customDescription.AppendLine("ABF_DirectiveDefForcedItemQuality".Translate(((QualityCategory)forcedItemQuality).GetLabel()));
                    }
                    if (producedItemQualityOffset != 0 && forcedItemQuality == null)
                    {
                        customDescription.AppendLine();
                        customDescription.AppendLine("ABF_DirectiveDefProducedItemQualityOffsetHeader".Translate().Colorize(ColoredText.TipSectionTitleColor));
                        customDescription.AppendLine("ABF_DirectiveDefProducedItemQualityOffset".Translate(producedItemQualityOffset));
                    }
                    if (hungerRateFactor != 1f)
                    {
                        customDescription.AppendLine("ABF_DirectiveDefEnergyConsumptionFactor".Translate(hungerRateFactor.ToStringPercent()));
                    }
                    cachedCustomDescription = customDescription.ToString();
                }
                return cachedCustomDescription;
            }
        }

        // Directives can never be stacked with the same def, and must pass all requirement workers for compatibility.
        public AcceptanceReport CompatibleWith(DirectiveDef other)
        {
            if (this == other) return "ABF_SameDirective".Translate();
            if (requirementWorkers.NullOrEmpty())
            {
                return true;
            }
            foreach (DirectiveRequirementWorker worker in requirementWorkers)
            {
                AcceptanceReport requirementReport = worker.CompatibleWith(other);
                if (!requirementReport)
                {
                    return requirementReport.Reason;
                }
            }
            return true;
        }

        // Some directives are unique, and should be totally hidden if it is never valiid for a particular pawn.
        public AcceptanceReport EverValidFor(Pawn pawn)
        {
            foreach (DirectiveRequirementWorker worker in requirementWorkers)
            {
                AcceptanceReport requirementReport = worker.EverValidFor(pawn);
                if (!requirementReport)
                {
                    return requirementReport.Reason;
                }
            }
            return true;
        }

        // Directive workers control whether the particular directive is valid for a pawn (used when reprogrammed).
        public AcceptanceReport ValidFor(Pawn pawn)
        {
            foreach (DirectiveRequirementWorker worker in requirementWorkers)
            {
                AcceptanceReport requirementReport = worker.ValidFor(pawn);
                if (!requirementReport)
                {
                    return requirementReport.Reason;
                }
            }
            return true;
        }

        // Ensure all directive requiremnet workers attached to this def have a reference stored to this def.
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (!requirementWorkers.NullOrEmpty())
            {
                foreach (DirectiveRequirementWorker requirementWorker in requirementWorkers)
                {
                    requirementWorker.def = this;
                }
            }
            if (directiveCategory == null)
            {
                directiveCategory = "Uncategorized";
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (directiveClass == null)
            {
                yield return "directiveClass is null";
            }
            if (!typeof(Directive).IsAssignableFrom(directiveClass))
            {
                yield return "directiveClass is not Directive or subclass thereof";
            }
            if (directiveCategory == null)
            {
                yield return "directiveCategory is null";
            }
        }
    }
}
