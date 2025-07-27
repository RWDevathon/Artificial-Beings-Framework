using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ArtificialBeings
{
    public class CompArtificialPawn : ThingComp
    {
        /* ALL ARTIFICIAL PAWNS */
        private ABF_ArtificialState state = ABF_ArtificialState.Unknown;

        private Dictionary<ABF_ArtificialState, string> cachedStateTranslations = new Dictionary<ABF_ArtificialState, string>() 
        {
            { ABF_ArtificialState.Unknown, "ABF_UnknownPawnState".Translate() },
            { ABF_ArtificialState.Blank, "ABF_BlankPawnState".Translate() },
            { ABF_ArtificialState.Drone, "ABF_DronePawnState".Translate() },
            { ABF_ArtificialState.Reprogrammable, "ABF_ReprogrammablePawnState".Translate() },
            { ABF_ArtificialState.Sapient, "ABF_SapientPawnState".Translate() },
        };

        public ABF_ArtificialState State
        {
            get
            {
                return state;
            }
            set
            {
                // If trying to set the pawn to its existing state, do nothing.
                if (state == value)
                {
                    return;
                }

                // Check to see if the pawn is allowed to enter the state that is being requested. If not, log an error and do nothing.
                ABF_ArtificialPawnExtension pawnExt = Pawn.def.GetModExtension<ABF_ArtificialPawnExtension>();
                if (pawnExt == null)
                {
                    Log.Error($"[ABF] {Pawn.LabelShortCap}'s race of {Pawn.def.defName} is missing an artificial def mod extension! It should not have a CompArtificialPawn or try to change state.");
                    return;
                }

                if (!pawnExt.canBeSapient && value == ABF_ArtificialState.Sapient)
                {
                    Log.Warning($"[ABF] {Pawn.LabelShortCap} attempted to change state to Sapient but it is disabled by its race.");
                    return;
                }
                if (!pawnExt.canBeReprogrammable && value == ABF_ArtificialState.Reprogrammable)
                {
                    Log.Warning($"[ABF] {Pawn.LabelShortCap} attempted to change state to Reprogrammable but it is disabled by its race.");
                    return;
                }
                if (!pawnExt.canBeDrone && value == ABF_ArtificialState.Drone)
                {
                    Log.Warning($"[ABF] {Pawn.LabelShortCap} attempted to change state to Drone but it is disabled by its race.");
                    return;
                }

                // If changing away from the reprogrammable state, clean up the pawn data.
                if (state == ABF_ArtificialState.Reprogrammable)
                {
                    ABF_Utils.Deprogram(Pawn);
                    enabledWorkTypes = null;
                    complexitySources = null;
                    // Remove all directives that should no longer exist, calling PostRemove on them as it goes.
                    List<Directive> oldDirectives = directives;
                    for (int i = oldDirectives.Count - 1; i >= 0; i--)
                    {
                        Directive oldDirective = oldDirectives[i];
                        directives.RemoveAt(i);
                        directiveDefs.Remove(oldDirective.def);
                        oldDirective.PostRemove();
                    }
                    directives = null;
                    directiveDefs = null;
                }
                // If changing away from the sapient state, clean up relations data. Other data will be cleaned on state switch.
                else if (state == ABF_ArtificialState.Sapient)
                {
                    Pawn.relations.ClearAllRelations();
                }
                // If changing away from the blank state, give the pawn a name.
                else if (state == ABF_ArtificialState.Blank)
                {
                    Pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(Pawn, NameStyle.Full, null, false, null);
                }

                ABF_Utils.UpdateStateFor(Pawn, value);

                // Switching states usually requires setting up some pawn data.
                if (value == ABF_ArtificialState.Drone)
                {
                    ABF_Utils.ReconfigureDrone(Pawn);
                }
                else if (value == ABF_ArtificialState.Reprogrammable)
                {
                    ABF_Utils.ReconfigureDrone(Pawn);
                    complexitySources = new Dictionary<string, int>();
                    directives = new List<Directive>();
                    directiveDefs = new List<DirectiveDef>();
                    ABF_Utils.Deprogram(Pawn);
                    RecalculateComplexity();
                }
                else if (value == ABF_ArtificialState.Blank)
                {
                    // Only do these things for humanlikes blanks, not animals.
                    if (Pawn.RaceProps.intelligence == Intelligence.Humanlike)
                    {
                        // Blanks do not have traits.
                        foreach (Trait trait in Pawn.story.traits.allTraits.ToList())
                        {
                            Pawn.story.traits.RemoveTrait(trait);
                        }

                        // Blanks do not have ideos.
                        Pawn.ideo = null;

                        // Blanks should obviously have a blank name, if they use full names.
                        if (Pawn.Name is NameTriple)
                        {
                            Pawn.Name = new NameTriple("", "ABF_BlankPawnName".Translate(), "");
                        }

                        // Blanks have no skills nor passions.
                        foreach (SkillRecord skillRecord in Pawn.skills.skills)
                        {
                            skillRecord.passion = 0;
                            skillRecord.xpSinceLastLevel = 0;
                            skillRecord.Level = 0;
                        }
                    }
                }

                // Switching away from the blank state removes the disabled hediff, unless they are reprogrammable. Reprogrammable drones need to be programmed first.
                if (state == ABF_ArtificialState.Blank && value != ABF_ArtificialState.Reprogrammable)
                {
                    Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(ABF_HediffDefOf.ABF_Hediff_Artificial_Disabled);
                    if (hediff != null)
                    {
                        Pawn.health.RemoveHediff(hediff);
                    }
                }

                state = value;
                Pawn.needs?.AddOrRemoveNeedsAsAppropriate();
                Pawn.Notify_DisabledWorkTypesChanged();
            }
        }

        /* REPROGRAMMABLE DRONES */
        public List<WorkTypeDef> enabledWorkTypes;

        private List<DirectiveDef> directiveDefs;

        private int cachedComplexity = 0;

        private int cachedMaxComplexity = 0;

        private int cachedBaselineComplexity = -1;

        private List<Directive> directives;

        // Local reserved storage for saving/loading complexitySources in the ExposeData method.
        private List<string> sourceKey = new List<string>();
        private List<int> sourceValue = new List<int>();

        private Dictionary<string, int> complexitySources;

        public Pawn Pawn => (Pawn)parent;

        public IEnumerable<DirectiveDef> ActiveDirectives => directiveDefs.AsReadOnly();

        public int Complexity
        {
            get
            {
                return cachedComplexity;
            }
        }

        public int MaxComplexity
        {
            get
            {
                return cachedMaxComplexity;
            }
        }

        public int BaselineComplexity
        {
            get
            {
                if (cachedBaselineComplexity == -1)
                {
                    // Cache the baseline complexity stat from the pawn's statBases. Do this exactly once as it does not change without reloading the game.
                    StatDef baselineComplexityStat = ABF_StatDefOf.ABF_Stat_Artificial_ComplexityLimit;
                    cachedBaselineComplexity = (int)baselineComplexityStat.defaultBaseValue;
                    foreach (StatModifier statMod in Pawn.def.statBases)
                    {
                        if (statMod.stat == baselineComplexityStat)
                        {
                            cachedBaselineComplexity = (int)statMod.value;
                            break;
                        }
                    }
                }
                return cachedBaselineComplexity;
            }
        }

        /* BASE METHODS */

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (State == ABF_ArtificialState.Reprogrammable)
            {
                RecalculateComplexity();

                if (!respawningAfterLoad)
                {
                    foreach (Directive directive in directives)
                    {
                        directive.PostSpawn(Pawn.Map);
                    }
                }
            }
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            if (State == ABF_ArtificialState.Reprogrammable)
            {
                foreach (Directive directive in directives)
                {
                    directive.PostDespawn(map);
                }
            }
        }

        public override void Notify_MapRemoved()
        {
            base.Notify_MapRemoved();
            if (State == ABF_ArtificialState.Reprogrammable)
            {
                foreach (Directive directive in directives)
                {
                    directive.PostDespawn(null);
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            string result = "ABF_CurrentPawnState".Translate(cachedStateTranslations[state]);
            if (State == ABF_ArtificialState.Reprogrammable)
            {
                List<string> directiveStrings = new List<string>();
                foreach (Directive directive in directives)
                {
                    directiveStrings.AddRange(directive.GetCompInspectStrings());
                }
                if (directiveStrings.Count > 0)
                {
                    result = result + "\n" + string.Join("\n", directiveStrings);
                }
            }
            return result;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Reprogrammable drone directives may desire to have their own Gizmo's to show.
            if (State == ABF_ArtificialState.Reprogrammable)
            {
                foreach (Directive directive in directives)
                {
                    foreach (Gizmo gizmo in directive.GetGizmos())
                    {
                        yield return gizmo;
                    }
                }
            }
            // Dev mode commands. These may be removed in the future.
            if (DebugSettings.ShowDevGizmos && Pawn.RaceProps.intelligence == Intelligence.Humanlike)
            {
                if (State != ABF_ArtificialState.Blank)
                {
                    Command_Action makeBlank = new Command_Action
                    {
                        defaultLabel = "DEV: Force Blank",
                        action = delegate
                        {
                            State = ABF_ArtificialState.Blank;
                        }
                    };
                    yield return makeBlank;
                }
                if (State == ABF_ArtificialState.Blank)
                {
                    Command_Action makeSapient = new Command_Action
                    {
                        defaultLabel = "DEV: Force Sapience",
                        action = delegate
                        {
                            State = ABF_ArtificialState.Sapient;
                        }
                    };
                    yield return makeSapient;
                    Command_Action makeDrone = new Command_Action
                    {
                        defaultLabel = "DEV: Force Drone",
                        action = delegate
                        {
                            State = ABF_ArtificialState.Drone;
                        }
                    };
                    yield return makeDrone;
                    Command_Action makeReprogrammable = new Command_Action
                    {
                        defaultLabel = "DEV: Force Reprogrammable",
                        action = delegate
                        {
                            State = ABF_ArtificialState.Reprogrammable;
                        }
                    };
                    yield return makeReprogrammable;
                }
            }
            if (DebugSettings.ShowDevGizmos && State == ABF_ArtificialState.Reprogrammable)
            {
                Command_Action printComplexities = new Command_Action
                {
                    defaultLabel = "DEV: Log Complexity Sources",
                    action = delegate
                    {
                        Log.Message("[ABF DEBUG] Complexity Sources for " + Pawn);
                        foreach (string sourceKey in complexitySources.Keys)
                        {
                            Log.Message("[ABF DEBUG] " + sourceKey + ": " + complexitySources[sourceKey]);
                        }
                    }
                };
                yield return printComplexities;
                Command_Action forceReprogramming = new Command_Action
                {
                    defaultLabel = "DEV: Force Reprogramming",
                    action = delegate
                    {
                        Find.WindowStack.Add(new Dialog_ReprogramDrone(Pawn));
                        // If the unit had the no programming hediff, remove that hediff.
                        Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(ABF_HediffDefOf.ABF_Hediff_Artificial_Disabled);
                        if (hediff != null)
                        {
                            Pawn.health.RemoveHediff(hediff);
                        }
                        // Reprogrammable drones do not need to restart after programming is complete.
                        hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(ABF_HediffDefOf.ABF_Hediff_Artificial_Incapacitated);
                        if (hediff != null)
                        {
                            Pawn.health.RemoveHediff(hediff);
                        }
                    }
                };
                yield return forceReprogramming;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref state, "ABF_artificialState", ABF_ArtificialState.Unknown);
            Scribe_Collections.Look(ref enabledWorkTypes, "ABF_enabledWorkTypes", LookMode.Def);
            Scribe_Collections.Look(ref complexitySources, "ABF_complexitySources", LookMode.Value, LookMode.Value, ref sourceKey, ref sourceValue);
            Scribe_Collections.Look(ref directives, "ABF_activeDirectives", LookMode.Deep);
            // Needs to explicitly set state as soon as possible in loading phase so that work settings check disabled types AFTER state is set for reprogrammable drones.
            // Not doing this results in loss of configured work settings for reprogrammable drones.
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                ABF_Utils.UpdateStateFor(Pawn, State);
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ABF_Utils.UpdateStateFor(Pawn, State);

                if (State == ABF_ArtificialState.Reprogrammable)
                {
                    RecalculateComplexity();
                    Pawn.Notify_DisabledWorkTypesChanged();
                    // Directives should have their pawn reference set and any tickers restored.
                    directiveDefs = new List<DirectiveDef>();
                    foreach (Directive directive in directives)
                    {
                        directiveDefs.Add(directive.def);
                        directive.pawn = Pawn;
                        if (directive.def.tickerType != TickerType.Never)
                        {
                            Find.World.GetComponent<ABF_WorldComponent>().RegisterDirective(directive, directive.def.tickerType);
                        }
                    }
                }
            }
        }

        public override float GetStatFactor(StatDef stat)
        {
            if (State != ABF_ArtificialState.Reprogrammable)
            {
                return base.GetStatFactor(stat);
            }

            float result = 1f;
            foreach (DirectiveDef directiveDef in ActiveDirectives)
            {
                result *= directiveDef.statFactors?.GetStatFactorFromList(stat) ?? 1f;
            }
            return result;
        }

        public override float GetStatOffset(StatDef stat)
        {
            if (State != ABF_ArtificialState.Reprogrammable)
            {
                return base.GetStatOffset(stat);
            }

            float result = 0f;
            foreach (DirectiveDef directiveDef in ActiveDirectives)
            {
                result += directiveDef.statOffsets?.GetStatOffsetFromList(stat) ?? 0;
            }
            return result;
        }

        public override void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
        {
            if (State != ABF_ArtificialState.Reprogrammable)
            {
                base.GetStatsExplanation(stat, sb, whitespace);
                return;
            }

            float offsetAmount = 0f;
            float factorAmount = 1f;
            foreach (DirectiveDef directiveDef in ActiveDirectives)
            {
                offsetAmount += directiveDef.statOffsets?.GetStatOffsetFromList(stat) ?? 0;
                factorAmount *= directiveDef.statFactors?.GetStatFactorFromList(stat) ?? 1f;
            }
            if (offsetAmount != 0f)
            {
                sb.AppendLine("ABF_StatsReport_Directives".Translate(offsetAmount.ToStringPercentSigned()));
            }
            if (factorAmount != 1f)
            {
                sb.AppendLine("ABF_StatsReport_Directives".Translate(factorAmount.ToStringPercent()));
            }
        }

        /* ALL ARTIFICIAL PAWNS METHODS */
        // Initialize the state of the pawn based on their PawnKind DefModExtension or their ThingDef DefModExtension.
        public void InitializeState()
        {
            if (Pawn.kindDef.GetModExtension<ABF_ArtificialPawnKindExtension>() is ABF_ArtificialPawnKindExtension pawnKindExtension)
            {
                State = pawnKindExtension.pawnState;
            }
            else if (Pawn.def.GetModExtension<ABF_ArtificialPawnExtension>() is ABF_ArtificialPawnExtension pawnExtension)
            {
                if (pawnExtension.canBeSapient)
                {
                    State = ABF_ArtificialState.Sapient;
                }
                else if (pawnExtension.canBeReprogrammable)
                {
                    State = ABF_ArtificialState.Reprogrammable;
                }
                else
                {
                    State = ABF_ArtificialState.Drone;
                }
            }
            else
            {
                State = ABF_ArtificialState.Sapient;
            }
        }


        /* REPROGRAMMABLE DRONE METHODS */

        // Take a list of directive defs, and create new Directives for this pawn with them. Skip defs that already have an instance of that Directive active.
        // Remove all directives that have a def not in the given list.
        public void SetDirectives(List<DirectiveDef> newDirectives)
        {
            // Remove all directives that should no longer exist, calling PostRemove on them as it goes.
            List<Directive> oldDirectives = directives;
            for (int i = oldDirectives.Count - 1; i >= 0; i--)
            {
                Directive oldDirective = oldDirectives[i];
                if (!newDirectives.Contains(oldDirective.def))
                {
                    directives.RemoveAt(i);
                    directiveDefs.Remove(oldDirective.def);
                    oldDirective.PostRemove();
                }
            }

            // Add all directives that should now exist, skipping directives that are already stored.
            foreach (DirectiveDef newDirectiveDef in newDirectives)
            {
                if (directiveDefs.Contains(newDirectiveDef))
                {
                    continue;
                }

                Directive newDirective = DirectiveMaker.MakeDirective(newDirectiveDef, Pawn);
                directives.Add(newDirective);
                directiveDefs.Add(newDirectiveDef);
                newDirective.PostAdd();
            }
        }

        // Initialize inherent work tasks for the drone as per the race and work types that should always be permitted.
        public void InitializeEnabledWorkTypes()
        {
            enabledWorkTypes = new List<WorkTypeDef> { };
            // Initialize enabledWorkTypes with those that are inherent to this pawn's race. They have no cost and do not reduce the number of free work types.
            foreach (WorkTypeDef workTypeDef in Pawn.def.GetModExtension<ABF_ArtificialPawnExtension>().inherentWorkTypes)
            {
                enabledWorkTypes.Add(workTypeDef);
            }
            // Programmable drones may also inherently do any task which has no associated work tags and no relevant skills, like bed resting.
            foreach (WorkTypeDef workTypeDef in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (workTypeDef.relevantSkills.NullOrEmpty() && workTypeDef.workTags == WorkTags.None)
                {
                    enabledWorkTypes.Add(workTypeDef);
                }
            }
            Pawn.Notify_DisabledWorkTypesChanged();
        }

        // Take a source as the key value and an int as the value to update, and recalculate complexity.
        // If source does not exist as a key and value is not 0, then add it as a source.
        // If the source exists, update its existing value. If value is 0, remove the source and value.
        public void UpdateComplexity(string source, int value)
        {
            if (complexitySources.ContainsKey(source))
            {
                if (value == 0)
                {
                    complexitySources.Remove(source);
                }
                else
                {
                    complexitySources[source] = value;
                }
            }
            else if (value != 0)
            {
                complexitySources.Add(source, value);
            }
            RecalculateComplexity();
        }

        // Given a string source, return the int value associated with that key string in complexitySources.
        // If the source does not exist, return 0.
        public int GetComplexityFromSource(string source)
        {
            return complexitySources.GetWithFallback(source, 0);
        }

        // Recalculates and recaches the complexity value for this pawn based on their complexity sources.
        public void RecalculateComplexity()
        {
            int sum = 0;
            foreach (int complexity in complexitySources.Values)
            {
                sum += complexity;
            }
            cachedComplexity = Math.Max(0, sum);
            cachedMaxComplexity = (int)Pawn.GetStatValue(ABF_StatDefOf.ABF_Stat_Artificial_ComplexityLimit);
        }
    }
}