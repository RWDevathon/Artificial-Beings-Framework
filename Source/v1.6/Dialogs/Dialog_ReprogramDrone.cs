using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArtificialBeings
{
    public class Dialog_ReprogramDrone : Window
    {
        private Pawn pawn;

        private CompArtificialPawn programComp;

        private ABF_ArtificialPawnExtension programExtension;

        private List<DirectiveDef> proposedDirectives;

        private List<WorkTypeDef> localWorkTypes;

        private List<WorkTypeDef> proposedEnabledWorkTypes = new List<WorkTypeDef>();

        private Dictionary<SkillRecord, DroneSkillContext> skillContexts;

        private List<SkillDef> skillDefs;

        private int proposedWorkTypeComplexity;

        private float proposedSkillComplexity;

        private const float ContentBaseWidth = 200f;

        private const float WidthWithTabs = 1400f;

        private const float ContentHeight = 500f;

        private const float ButtonHeight = 30f;

        private float summaryCachedWidth = -1;

        private Dictionary<string, string> summaryCachedText = new Dictionary<string, string>();

        private static readonly CachedTexture backgroundTexture = new CachedTexture("UI/Icons/Dialogs/DrawPocket");

        private const float directiveBlockHeight = 80f;

        private int ProposedComplexity => programComp.Complexity;

        protected override float Margin => 12f;

        private float Height => 700f;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(WidthWithTabs, Height);
            }
        }

        public Dialog_ReprogramDrone(Pawn pawn)
        {
            closeOnCancel = false;
            forcePause = true;
            absorbInputAroundWindow = true;
            this.pawn = pawn;
            programComp = pawn.GetComp<CompArtificialPawn>();
            proposedDirectives = new List<DirectiveDef>();
            proposedDirectives.AddRange(programComp.ActiveDirectives);
            programExtension = pawn.def.GetModExtension<ABF_ArtificialPawnExtension>();
            CacheLocalAndAssignGlobalWorkTypes();
            proposedWorkTypeComplexity = programComp.GetComplexityFromSource("Work Types");
            skillContexts = new Dictionary<SkillRecord, DroneSkillContext>();
            foreach (SkillRecord skillRecord in pawn.skills.skills)
            {
                skillContexts[skillRecord] = new DroneSkillContext(skillRecord);
            }
            proposedSkillComplexity = programComp.GetComplexityFromSource("Skills");
            skillDefs = DefDatabase<SkillDef>.AllDefsListForReading;
        }

        Vector2 workTypeScrollPosition = Vector2.zero;
        float workTypeCachedScrollHeight = ContentHeight;

        Vector2 skillScrollPosition = Vector2.zero;
        float skillCachedScrollHeight = ContentHeight;

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            float curX = 0f;
            float curY = 0f;
            TaggedString title = "ABF_ReprogramDrone".Translate(pawn.LabelShortCap);
            float titleWidth = Text.CalcSize(title).x;
            Widgets.Label(curX, ref curY, titleWidth, title);
            Text.Font = GameFont.Small;
            Widgets.InfoCardButton(inRect.width - (3 * Margin), 0f, pawn);
            curY += Margin;

            Rect workTypeRect = new Rect(curX, curY, ContentBaseWidth, ContentHeight);
            proposedEnabledWorkTypes = programComp.enabledWorkTypes;
            DrawWorkTypes(workTypeRect);
            curX += ContentBaseWidth + Margin;

            Rect skillsRect = new Rect(curX, curY, ContentBaseWidth, ContentHeight);
            DrawSkills(skillsRect);
            curX += ContentBaseWidth + Margin;

            Rect directiveRect = new Rect(curX, curY, ContentBaseWidth * 2, ContentHeight);
            DrawDirectives(directiveRect);
            curX += ContentBaseWidth * 2 + Margin;
            Rect cardSection = new Rect(curX, curY, ContentBaseWidth * 3, ContentHeight);
            cardSection.xMin += 2f * Margin;
            cardSection.yMax -= Margin + CloseButSize.y;
            cardSection.yMin += 32f;
            CharacterCardUtility.DrawCharacterCard(cardSection, pawn, null, default, showName: true);
            Rect summaryRect = new Rect(Margin, inRect.height - Text.LineHeight * 4.5f - Margin, inRect.width - CloseButSize.x - (5 * Margin), Text.LineHeight * 4.5f);
            DrawSummary(summaryRect);
            Rect closeButton = new Rect(inRect.width - CloseButSize.x - (3 * Margin), inRect.height - CloseButSize.y - Margin, CloseButSize.x, CloseButSize.y);
            if (Widgets.ButtonText(closeButton, "OK".Translate()) && CanAccept())
            {
                Accept();
            }
        }

        private void DrawWorkTypes(Rect rect)
        {
            Rect contentRect = new Rect(rect);
            bool needToScroll = workTypeCachedScrollHeight > rect.height;
            if (needToScroll)
            {
                contentRect.width -= 20f;
                contentRect.height = workTypeCachedScrollHeight;
                Widgets.BeginScrollView(rect, ref workTypeScrollPosition, contentRect);
            }
            Listing_Standard listingStandard = new Listing_Standard
            {
                maxOneColumn = true
            };
            listingStandard.Begin(contentRect);

            listingStandard.Label("ABF_SelectedWorkTypes".Translate());
            foreach (WorkTypeDef workTypeDef in localWorkTypes)
            {
                // If the work type is illegal, display it with a tooltip as to why it is illegal, but do not allow players to interact with it.
                AcceptanceReport workTypeLegal = WorkTypeAcceptanceReport(workTypeDef);
                if (!workTypeLegal)
                {
                    if (programExtension.inherentWorkTypes.NotNullAndContains(workTypeDef))
                    {
                        // If it is illegal because it is inherent to the pawn, display the checkbox as enabled and don't let players change it.
                        bool fakeActive = true;
                        GUI.color = Color.green;
                        listingStandard.CheckboxLabeled(workTypeDef.labelShort.CapitalizeFirst(), ref fakeActive, tooltip: workTypeLegal.Reason, onChange: delegate
                        {
                            fakeActive = true;
                        });
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = Color.gray;
                        listingStandard.Label(workTypeDef.labelShort.CapitalizeFirst().Colorize(ColorLibrary.Grey), tooltip: workTypeLegal.Reason);
                        GUI.color = Color.white;
                    }
                    continue;
                }

                bool active = proposedEnabledWorkTypes.Contains(workTypeDef);

                listingStandard.CheckboxLabeled(workTypeDef.labelShort.CapitalizeFirst(), ref active, WorkTypeComplexityTooltip(workTypeDef, !active), onChange: delegate
                {
                    if (!active)
                    {
                        programComp.enabledWorkTypes.Remove(workTypeDef);
                        proposedWorkTypeComplexity -= ABF_Utils.ComplexityCostFor(workTypeDef, pawn, false);
                        // If the disabled work type def would result in a skill becoming totally disabled, remove all assigned skill points for it to avoid a complexity "leak".
                        if (workTypeDef.relevantSkills != null)
                        {
                            foreach (SkillDef oldEnabledSkillDef in workTypeDef.relevantSkills)
                            {
                                if (oldEnabledSkillDef.neverDisabledBasedOnWorkTypes || programComp.enabledWorkTypes.Any(workType => workType.relevantSkills.NotNullAndContains(oldEnabledSkillDef)))
                                {
                                    continue;
                                }

                                SkillRecord skillRecord = pawn.skills.GetSkill(oldEnabledSkillDef);
                                DroneSkillContext skillContext = skillContexts[skillRecord];
                                while (skillRecord.Level > skillContext.skillFloor)
                                {
                                    proposedSkillComplexity -= skillContext.skillComplexityCost;
                                    skillContext.skillComplexityCost -= 0.5f;
                                    skillRecord.Level--;
                                }
                            }
                        }
                        CheckLegalDirectives();
                    }
                    else
                    {
                        programComp.enabledWorkTypes.Add(workTypeDef);
                        proposedWorkTypeComplexity += ABF_Utils.ComplexityCostFor(workTypeDef, pawn, true);
                        CheckLegalDirectives();
                    }
                    pawn.Notify_DisabledWorkTypesChanged();
                    programComp.UpdateComplexity("Work Types", proposedWorkTypeComplexity);
                    programComp.UpdateComplexity("Skills", Mathf.Max(0, Mathf.CeilToInt(proposedSkillComplexity)));
                });
            }
            if (needToScroll)
            {
                Widgets.EndScrollView();
            }
            workTypeCachedScrollHeight = listingStandard.CurHeight;
            listingStandard.End();
        }

        private void DrawSkills(Rect rect)
        {
            Rect contentRect = new Rect(rect);
            bool needToScroll = skillCachedScrollHeight > rect.height;
            if (needToScroll)
            {
                contentRect.width -= 20f;
                contentRect.height = skillCachedScrollHeight;
                Widgets.BeginScrollView(rect, ref skillScrollPosition, contentRect);
            }
            Listing_Standard listingStandard = new Listing_Standard
            {
                maxOneColumn = true
            };
            listingStandard.Begin(contentRect);

            listingStandard.Label("ABF_SelectSkills".Translate());
            foreach (SkillDef skillDef in skillDefs)
            {
                SkillRecord skill = pawn.skills.GetSkill(skillDef);
                // Skip skills that are disabled by work tags that the pawn does not have.
                if (skill.TotallyDisabled)
                {
                    continue;
                }
                DroneSkillContext context = skillContexts[skill];
                listingStandard.Label(skillDef.LabelCap + ": " + skill.Level.ToString(), tooltip: skillDef.description);
                Rect skillDefRect = listingStandard.GetRect(ButtonHeight);

                RectDivider skillDefRowButtons = new RectDivider(skillDefRect, 8102830, new Vector2(0, 0));
                float skillDefColumnWidth = (skillDefRowButtons.Rect.width - Margin) / 2;
                RectDivider removeSkillLevelButton = skillDefRowButtons.NewCol(skillDefColumnWidth, HorizontalJustification.Left);
                TooltipHandler.TipRegion(removeSkillLevelButton, SkillComplexityTooltip(skill, context, false));
                if (Widgets.ButtonText(removeSkillLevelButton, "ABF_RemoveSkillLevel".Translate()))
                {
                    if (skill.Level <= SkillRecord.MinLevel)
                    {
                        Messages.Message("ABF_CantExceedSkillMin".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                    }
                    else if (skill.Level <= context.skillFloor)
                    {
                        Messages.Message("ABF_HasInherentSkills".Translate(pawn.LabelShortCap, skill.def.label, context.skillFloor), MessageTypeDefOf.RejectInput, false);
                    }
                    else
                    {
                        proposedSkillComplexity -= context.skillComplexityCost;
                        context.skillComplexityCost -= 0.5f;
                        skill.Level--;
                        programComp.UpdateComplexity("Skills", Mathf.Max(0, Mathf.CeilToInt(proposedSkillComplexity)));
                        CheckLegalDirectives();
                    }
                }

                RectDivider addSkillLevelButton = skillDefRowButtons.NewCol(skillDefColumnWidth, HorizontalJustification.Right);
                TooltipHandler.TipRegion(addSkillLevelButton, SkillComplexityTooltip(skill, context, true));
                if (Widgets.ButtonText(addSkillLevelButton, "ABF_AddSkillLevel".Translate()))
                {
                    if (skill.Level >= context.skillCeiling)
                    {
                        Messages.Message("ABF_CantExceedSkillMax".Translate(pawn.LabelShortCap, context.skillCeiling), MessageTypeDefOf.RejectInput, false);
                    }
                    else
                    {
                        proposedSkillComplexity += context.skillComplexityCost + 0.5f;
                        context.skillComplexityCost += 0.5f;
                        skill.Level++;
                        programComp.UpdateComplexity("Skills", Mathf.Max(0, Mathf.CeilToInt(proposedSkillComplexity)));
                        CheckLegalDirectives();
                    }
                }
                listingStandard.Gap(Margin);
            }
            if (needToScroll)
            {
                Widgets.EndScrollView();
            }
            skillCachedScrollHeight = listingStandard.CurHeight;
            listingStandard.End();
        }

        private void DrawDirectives(Rect rect)
        {
            float xIndex = rect.x;
            float yIndex = rect.y;
            Rect headerSection = new Rect(xIndex, yIndex, rect.width, Text.LineHeight);
            Widgets.Label(headerSection, "ABF_SelectedDirectives".Translate());
            yIndex += Text.LineHeight + Margin;
            if (Widgets.ButtonText(new Rect(xIndex, yIndex, rect.width, ButtonHeight), "ABF_SetDroneDirectives".Translate()))
            {
                Find.WindowStack.Add(new Dialog_SetDroneDirectives(pawn, ref proposedDirectives));
            }
            yIndex += ButtonHeight + Margin;
            float directiveBlockWidth = (rect.width - (4 * Margin)) / 3;
            float xMaxIndex = rect.x + rect.width;
            float directiveBlockWithMarginWidth = directiveBlockWidth + Margin;
            float directiveIconSize = Mathf.Min(directiveBlockWidth - Margin / 3, directiveBlockHeight - (Margin / 2));
            Rect contentBGSection = new Rect(xIndex, yIndex, rect.width, rect.height - yIndex);
            Widgets.DrawRectFast(contentBGSection, Widgets.MenuSectionBGFillColor);
            xIndex += Margin;
            yIndex += Margin;

            if (!proposedDirectives.Any())
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = ColoredText.SubtleGrayColor;
                Widgets.Label(contentBGSection, "(" + "NoneLower".Translate() + ")");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else
            {
                for (int i = 0; i < proposedDirectives.Count; i++)
                {
                    if (xIndex + directiveBlockWithMarginWidth > xMaxIndex)
                    {
                        xIndex = rect.x + Margin;
                        yIndex += directiveBlockHeight + Margin;
                    }
                    DirectiveDef directiveDef = proposedDirectives[i];
                    Rect blockSection = new Rect(xIndex, yIndex, directiveBlockWidth, directiveBlockHeight);
                    Widgets.DrawOptionBackground(blockSection, false);

                    // Stats section
                    xIndex += Margin / 3;
                    float textLineHeight = Text.LineHeightOf(GameFont.Small);
                    Text.Anchor = TextAnchor.MiddleRight;
                    string directiveComplexity = directiveDef.complexityCost.ToStringWithSign();
                    float statTextWidth = Text.CalcSize(directiveComplexity).x;
                    Widgets.LabelFit(new Rect(xIndex, yIndex + (Margin / 3), statTextWidth, textLineHeight), directiveComplexity);
                    Text.Anchor = TextAnchor.UpperLeft;
                    Rect statSection = new Rect(xIndex, yIndex + (Margin / 3), statTextWidth, textLineHeight);
                    if (Mouse.IsOver(statSection))
                    {
                        Widgets.DrawHighlight(statSection);
                        TooltipHandler.TipRegion(statSection, "ABF_Complexity".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "ABF_ComplexityDirectiveDesc".Translate());
                    }
                    xIndex += statTextWidth + (Margin / 3);

                    // Icon section
                    Rect directiveSection = new Rect(xIndex, yIndex + (Margin / 4), directiveBlockWidth - statTextWidth - Margin, directiveBlockHeight);
                    GUI.BeginGroup(directiveSection);
                    Rect iconSection = new Rect(directiveSection.width - directiveIconSize, 0f, directiveIconSize, directiveIconSize);
                    GUI.DrawTexture(iconSection, backgroundTexture.Texture);
                    Widgets.DrawTextureFitted(iconSection, directiveDef.Icon, 0.9f);
                    GUI.EndGroup();

                    // Label section
                    Text.Font = GameFont.Tiny;
                    float directiveLabelHeight = Text.CalcHeight(directiveDef.LabelCap, blockSection.width);
                    Rect directiveLabelBackground = new Rect(blockSection.x, blockSection.y + directiveBlockHeight - directiveLabelHeight, blockSection.width, directiveLabelHeight);
                    GUI.DrawTexture(new Rect(blockSection.x, blockSection.y + directiveBlockHeight - directiveLabelHeight, directiveLabelBackground.width, directiveLabelHeight), TexUI.GrayTextBG);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(directiveLabelBackground, directiveDef.LabelCap);
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;

                    if (Mouse.IsOver(directiveSection))
                    {
                        TooltipHandler.TipRegion(directiveSection, delegate
                        {
                            return directiveDef.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + directiveDef.description + "\n\n" + directiveDef.CustomDescription;
                        }, 209283172);
                    }
                    xIndex = blockSection.xMax + Margin;
                }
            }
        }

        private void DrawSummary(Rect summaryWrapper)
        {
            float summaryHeaderWidth = Mathf.Max(Text.CalcSize("ABF_BaselineComplexity".Translate()).x, Text.CalcSize("ABF_ComplexityEffects".Translate()).x);
            float summaryFullHeaderWidth = summaryHeaderWidth + ButtonHeight;
            float summaryRowHeight = summaryWrapper.height / 2;
            int proposedComplexity = ProposedComplexity;
            GUI.BeginGroup(summaryWrapper);
            Rect complexityRowIcon = new Rect(0f, (summaryRowHeight - GenUI.SmallIconSize) / 2f, GenUI.SmallIconSize, GenUI.SmallIconSize);
            Rect complexityRowHeader = new Rect(complexityRowIcon.xMax + Margin, 0, summaryHeaderWidth, summaryRowHeight);
            Rect complexityRow = new Rect(0f, complexityRowHeader.y, summaryWrapper.width, complexityRowHeader.height);
            Widgets.DrawHighlightIfMouseover(complexityRow);
            complexityRow.xMax = complexityRowHeader.xMax + 94f;
            TooltipHandler.TipRegion(complexityRow, "ABF_BaselineComplexityDesc".Translate());
            GUI.DrawTexture(complexityRowIcon, ABF_Textures.complexityIcon);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(complexityRowHeader, "ABF_BaselineComplexity".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Rect effectRowIcon = new Rect(0f, summaryRowHeight + (summaryRowHeight - GenUI.SmallIconSize) / 2f, GenUI.SmallIconSize, GenUI.SmallIconSize);
            Rect effectRowHeader = new Rect(effectRowIcon.xMax + Margin, summaryRowHeight, summaryHeaderWidth, summaryRowHeight);
            Rect summaryRow = new Rect(0f, effectRowHeader.y, summaryWrapper.width, effectRowHeader.height);
            Widgets.DrawHighlightIfMouseover(summaryRow);
            summaryRow.xMax = effectRowHeader.xMax + 94f;
            TooltipHandler.TipRegion(summaryRow, "ABF_ComplexityEffectsDesc".Translate());
            GUI.DrawTexture(effectRowIcon, ABF_Textures.complexityEffectIcon);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(effectRowHeader, "ABF_ComplexityEffects".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            string complexityText = programComp.BaselineComplexity.ToString();
            string complexityRelationText = proposedComplexity.ToString() + " / " + programComp.MaxComplexity.ToString();
            if (proposedComplexity > programComp.MaxComplexity)
            {
                complexityRelationText = complexityRelationText.Colorize(ColorLibrary.RedReadable);
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(summaryFullHeaderWidth, 0f, 90f, summaryRowHeight), complexityText);
            Widgets.Label(new Rect(summaryFullHeaderWidth, summaryRowHeight, 90f, summaryRowHeight), complexityRelationText);
            Text.Anchor = TextAnchor.MiddleLeft;
            float width = summaryWrapper.width - summaryHeaderWidth - 90f - GenUI.SmallIconSize - Margin;
            Rect summaryRowText = new Rect(summaryFullHeaderWidth + 90f + Margin, summaryRowHeight, width, summaryRowHeight);
            if (summaryRowText.width != summaryCachedWidth)
            {
                summaryCachedWidth = summaryRowText.width;
                summaryCachedText.Clear();
            }
            if (proposedComplexity > programComp.MaxComplexity)
            {
                Widgets.Label(summaryRowText, "ABF_ComplexityEffectIllegal".TranslateSimple().Truncate(summaryRowText.width, summaryCachedText));
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        private string WorkTypeComplexityTooltip(WorkTypeDef workTypeDef, bool adding)
        {
            // Inherent work types can not be removed, inform the player of this.
            if (programExtension.inherentWorkTypes.NotNullAndContains(workTypeDef))
            {
                return "ABF_CantRemoveInherentWorkTypes".Translate(workTypeDef.labelShort.CapitalizeFirst(), pawn.LabelShortCap);
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (adding)
            {
                stringBuilder.AppendLine("ABF_WorkTypeComplexityHeaderTooltip".Translate("ABF_Adding".Translate().CapitalizeFirst(), "ABF_Cost".Translate(), ABF_Utils.ComplexityCostFor(workTypeDef, pawn, adding).ToString()));
            }
            else
            {
                stringBuilder.AppendLine("ABF_WorkTypeComplexityHeaderTooltip".Translate("ABF_Removing".Translate().CapitalizeFirst(), "ABF_Refund".Translate(), ABF_Utils.ComplexityCostFor(workTypeDef, pawn, adding).ToString()));
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("ABF_WorkTypeComplexityBaseTooltip".Translate(workTypeDef.labelShort.CapitalizeFirst(), workTypeDef.GetModExtension<ABF_WorkTypeExtension>()?.baseComplexity.ToStringWithSign() ?? "+1"));

            int skillTypeComplexity = 0;
            if (adding)
            {
                // The additional cost is only applied to skills that are currently disabled.
                // It does not apply if another work type enabled it already.
                foreach (SkillDef skillDef in workTypeDef.relevantSkills)
                {
                    if (pawn.skills.GetSkill(skillDef).TotallyDisabled)
                    {
                        skillTypeComplexity += 1;
                    }
                }
            }
            else
            {
                // In order to properly calculate the complexity cost reduction, the worker must take into account other enabled work types.
                // It will only refund the additional complexity for skills that have 0 other work types enabling them.
                List<WorkTypeDef> otherEnabledWorkTypes = new List<WorkTypeDef>();
                foreach (WorkTypeDef enabledWorkTypeDef in programComp.enabledWorkTypes)
                {
                    if (enabledWorkTypeDef != workTypeDef)
                    {
                        otherEnabledWorkTypes.Add(enabledWorkTypeDef);
                    }
                }

                // Refund all skills that would be disabled if the parent work type were removed. Account for skills that are never disabled and other work types.
                foreach (SkillDef skillDef in workTypeDef.relevantSkills)
                {
                    if (!otherEnabledWorkTypes.Any(workType => workType.relevantSkills.NotNullAndContains(skillDef)))
                    {
                        skillTypeComplexity += 1;
                    }
                }
            }

            if (skillTypeComplexity != 0)
            {
                stringBuilder.AppendLine("ABF_WorkTypeComplexitySkillsTooltip".Translate(workTypeDef.labelShort.CapitalizeFirst(), skillTypeComplexity));
                if (!adding)
                {
                    stringBuilder.AppendLine("ABF_WorkTypeComplexitySkillsUnusedTooltip".Translate());
                }
            }

            return stringBuilder.ToString();
        }

        private AcceptanceReport WorkTypeAcceptanceReport(WorkTypeDef workTypeDef)
        {
            StringBuilder reason = new StringBuilder("ABF_CantInteractWithWorkType".Translate(workTypeDef.labelShort.CapitalizeFirst()));

            // Inherent work types can not be removed, inform the player of this.
            if (programExtension.inherentWorkTypes.NotNullAndContains(workTypeDef))
            {
                reason.Append("ABF_IsInherentWorkType".Translate(pawn.LabelShortCap));
                return reason.ToString();
            }

            // Forbidden work types can not be interacted with but should be visible so that players know this race can never add it to this race.
            if (programExtension.forbiddenWorkTypes.NotNullAndContains(workTypeDef))
            {
                reason.Append("ABF_IsForbiddenWorkType".Translate(pawn.def.LabelCap, pawn.LabelShortCap));
                return reason.ToString();
            }

            // If the worker extension itself reports this is an invalid work type for the pawn, use that.
            AcceptanceReport workerAcceptanceReport = workTypeDef.GetModExtension<ABF_WorkTypeExtension>()?.ValidFor(pawn) ?? AcceptanceReport.WasAccepted;
            if (!workerAcceptanceReport)
            {
                reason.Append(workerAcceptanceReport.Reason);
                return reason.ToString();
            }

            return AcceptanceReport.WasAccepted;
        }

        private string SkillComplexityTooltip(SkillRecord skill, DroneSkillContext context, bool adding)
        {
            if (adding && skill.Level >= context.skillCeiling)
            {
                return "ABF_CantExceedSkillMax".Translate(pawn.LabelShortCap, context.skillCeiling);
            }
            else if (!adding && skill.Level <= SkillRecord.MinLevel)
            {
                return "ABF_CantExceedSkillMin".Translate(pawn.LabelShortCap);
            }
            else if (!adding && skill.Level <= context.skillFloor)
            {
                return "ABF_HasInherentSkills".Translate(pawn.LabelShortCap, skill.def.label, context.skillFloor);
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (adding)
            {
                stringBuilder.AppendLine("ABF_SkillComplexityTooltip".Translate("ABF_Adding".Translate().CapitalizeFirst(), "ABF_Cost".Translate(), (context.skillComplexityCost + 0.5f).ToString()));
            }
            else
            {
                stringBuilder.AppendLine("ABF_SkillComplexityTooltip".Translate("ABF_Removing".Translate().CapitalizeFirst(), "ABF_Refund".Translate(), context.skillComplexityCost.ToString()));
            }

            return stringBuilder.ToString();
        }

        // Ensure that any directive that is now invalid is removed and notify the player.
        private void CheckLegalDirectives()
        {
            int directiveComplexity = 0;
            bool removedDirective = false;
            for (int i = proposedDirectives.Count - 1; i >= 0; i--)
            {
                if (!proposedDirectives[i].ValidFor(pawn) && !programExtension.inherentDirectives.Contains(proposedDirectives[i]))
                {
                    removedDirective = true;
                    proposedDirectives.RemoveAt(i);
                }
                else
                {
                    directiveComplexity += proposedDirectives[i].complexityCost;
                }
            }

            if (removedDirective)
            {
                programComp.UpdateComplexity("Active Directives", directiveComplexity);
                programComp.SetDirectives(proposedDirectives);
                Messages.Message("ABF_InvalidDirectivesRemoved".Translate(), MessageTypeDefOf.CautionInput, false);
            }
        }

        private void CacheLocalAndAssignGlobalWorkTypes()
        {
            localWorkTypes = new List<WorkTypeDef>();
            foreach (WorkTypeDef workTypeDef in DefDatabase<WorkTypeDef>.AllDefs.OrderByDescending(workTypeDef => workTypeDef.naturalPriority))
            {
                if (workTypeDef.workTags == WorkTags.None && workTypeDef.relevantSkills.NullOrEmpty())
                {
                    // Should always exist for the pawn, but only add it to the list if it's not already there.
                    if (!programComp.enabledWorkTypes.Contains(workTypeDef))
                    {
                        programComp.enabledWorkTypes.Add(workTypeDef);
                    }
                }
                else
                {
                    localWorkTypes.Add(workTypeDef);
                }
            }
            pawn.Notify_DisabledWorkTypesChanged();
        }

        private bool CanAccept()
        {
            if (programComp.MaxComplexity < ProposedComplexity)
            {
                Messages.Message("ABF_ComplexityLimitReached".Translate(), null, MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            return true;
        }

        protected void Accept()
        {
            if (programComp.MaxComplexity < ProposedComplexity)
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ABF_ComplexityLimitExceeded".Translate(programComp.MaxComplexity, ProposedComplexity) + "\n\n" + "WantToContinue".Translate(), PostAccept));
            }
            else
            {
                PostAccept();
            }
        }

        private void PostAccept()
        {
            programComp.UpdateComplexity("Work Types", proposedWorkTypeComplexity);
            programComp.UpdateComplexity("Skills", Mathf.Max(0, Mathf.CeilToInt(proposedSkillComplexity)));
            Close();
        }
    }
}