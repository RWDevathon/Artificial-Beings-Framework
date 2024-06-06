using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace ArtificialBeings
{
    public class Dialog_SetDroneDirectives : Window
    {
        private Pawn pawn;

        private CompArtificialPawn programComp;

        private ABF_ArtificialPawnExtension programmableDroneExtension;

        private int directiveComplexity;

        private int maxDirectives;

        private int inherentDirectiveCount = 0;

        private List<DirectiveDef> selectedDirectives = new List<DirectiveDef>();

        private List<DirectiveDef> inherentDirectives = new List<DirectiveDef>();

        private bool? selectedCollapsed = false;

        private Dictionary<string, bool> collapsedCategories = new Dictionary<string, bool>();

        private bool hoveringOverDirective;

        private DirectiveDef hoveredDirective;

        private float selectedHeight;

        private float unselectedHeight;

        private float scrollHeight;

        private Vector2 scrollPosition;

        private static readonly CachedTexture backgroundTexture = new CachedTexture("UI/Icons/Settings/DrawPocket");

        private static readonly Vector2 ButSize = new Vector2(150f, 38f);

        private const float directiveBlockWidth = 120f;

        private const float directiveBlockHeight = 80f;

        private const float directiveIconSize = 68f;

        public override Vector2 InitialSize => new Vector2(Mathf.Min(UI.screenWidth, 1036), UI.screenHeight - 4);

        protected override float Margin => 12f;

        protected List<DirectiveDef> SelectedDirectives => selectedDirectives;

        protected string Header => "ABF_SetDroneDirectives".Translate();

        protected string AcceptButtonLabel => "Confirm".Translate().CapitalizeFirst();

        public Dialog_SetDroneDirectives(Pawn pawn, ref List<DirectiveDef> proposedDirectives)
        {
            forcePause = true;
            closeOnAccept = false;
            absorbInputAroundWindow = true;
            foreach (string category in ABF_Utils.cachedDirectiveCategories)
            {
                collapsedCategories.Add(category, false);
            }
            this.pawn = pawn;
            programComp = pawn.GetComp<CompArtificialPawn>();
            int activeDirectiveComplexity = programComp.GetComplexityFromSource("Active Directives");
            directiveComplexity = activeDirectiveComplexity;
            programmableDroneExtension = pawn.def.GetModExtension<ABF_ArtificialPawnExtension>();
            maxDirectives = programmableDroneExtension?.maxDirectives ?? 3;
            inherentDirectiveCount = programmableDroneExtension?.inherentDirectives?.Count ?? 0;
            foreach (DirectiveDef directiveDef in pawn.def.GetModExtension<ABF_ArtificialPawnExtension>()?.inherentDirectives)
            {
                inherentDirectives.Add(directiveDef);
            }
            selectedDirectives = proposedDirectives;
        }

        public override void DoWindowContents(Rect rect)
        {
            Rect fullWindow = rect;
            fullWindow.yMax -= ButSize.y + Margin;
            Rect header = new Rect(fullWindow.x, fullWindow.y, fullWindow.width, 35f);
            Text.Font = GameFont.Medium;
            Widgets.Label(header, Header);
            Text.Font = GameFont.Small;
            fullWindow.yMin += Text.CalcSize(Header).y + Margin;
            Rect directiveWindow = new Rect(fullWindow.x + (Margin / 2), fullWindow.y, fullWindow.width - Margin, fullWindow.height - Margin);
            DrawDirectives(directiveWindow);
            if (Widgets.ButtonText(new Rect(rect.xMax - ButSize.x, rect.yMax - ButSize.y, ButSize.x, ButSize.y), AcceptButtonLabel) && CanAccept())
            {
                Accept();
            }

            string directiveSlotText = "ABF_DirectiveSlots".Translate() + ": " + (selectedDirectives.Count - inherentDirectiveCount) + " / " + programmableDroneExtension.maxDirectives;
            Vector2 textSize = Text.CalcSize(directiveSlotText);
            Rect summaryRect = new Rect(rect.xMax - ButSize.x - Margin - textSize.x, rect.yMax - ButSize.y, textSize.x, ButSize.y);
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(summaryRect, directiveSlotText);
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(summaryRect, "ABF_DirectiveSlotsDesc".Translate());
        }

        // UI Section containing all possible directives
        private void DrawDirectives(Rect rect)
        {
            hoveringOverDirective = false;
            GUI.BeginGroup(rect);
            float xIndex = 0f;
            float yIndex = 0f;
            DrawSection(new Rect(xIndex, yIndex, rect.width, selectedHeight), selectedDirectives, "ABF_SelectedDirectives".Translate(), ref yIndex, ref selectedHeight, adding: false, rect, ref selectedCollapsed);
            if (!selectedCollapsed.Value)
            {
                yIndex += 10f;
            }
            float selectedDirectiveHeight = yIndex;
            Widgets.Label(xIndex, ref yIndex, rect.width, "ABF_Directives".Translate());
            yIndex += 10f;
            float height = yIndex - selectedDirectiveHeight - Margin;
            if (Widgets.ButtonText(new Rect(rect.width - 150f, selectedDirectiveHeight, 150f, height), "CollapseAllCategories".Translate()))
            {
                SoundDefOf.TabClose.PlayOneShotOnCamera();
                foreach (string allDef in ABF_Utils.cachedDirectiveCategories)
                {
                    collapsedCategories[allDef] = true;
                }
            }
            if (Widgets.ButtonText(new Rect(rect.width - 300f - Margin, selectedDirectiveHeight, 150f, height), "ExpandAllCategories".Translate()))
            {
                SoundDefOf.TabOpen.PlayOneShotOnCamera();
                foreach (string allDef in ABF_Utils.cachedDirectiveCategories)
                {
                    collapsedCategories[allDef] = false;
                }
            }
            float selectorIndex = yIndex;
            Rect directiveSelectorSection = new Rect(xIndex, yIndex, rect.width - 16f, scrollHeight);
            Widgets.BeginScrollView(new Rect(xIndex, yIndex, rect.width, rect.height - yIndex), ref scrollPosition, directiveSelectorSection);
            bool? collapsed = null;
            DrawSection(new Rect(xIndex, yIndex, rect.width, unselectedHeight), ABF_Utils.cachedSortedDirectives, null, ref yIndex, ref unselectedHeight, adding: true, rect, ref collapsed);
            if (Event.current.type == EventType.Layout)
            {
                scrollHeight = yIndex - selectorIndex;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            if (!hoveringOverDirective)
            {
                hoveredDirective = null;
            }
        }

        private void DrawSection(Rect rect, List<DirectiveDef> directives, string label, ref float yIndex, ref float sectionHeight, bool adding, Rect containingRect, ref bool? collapsed)
        {
            float xIndex = rect.x;
            if (!label.NullOrEmpty())
            {
                Rect headerSection = new Rect(0f, yIndex, rect.width, Text.LineHeight);
                headerSection.xMax -= adding ? 16f : (Text.CalcSize("ClickToAddOrRemove".Translate()).x + Margin);
                if (collapsed.HasValue)
                {
                    Rect collapsibleMenuSection = new Rect(headerSection.x, headerSection.y + (headerSection.height - 16f) / 2f, 16f, 16f);
                    GUI.DrawTexture(collapsibleMenuSection, collapsed.Value ? TexButton.Reveal : TexButton.Collapse);
                    if (Widgets.ButtonInvisible(headerSection))
                    {
                        collapsed = !collapsed;
                        if (collapsed.Value)
                        {
                            SoundDefOf.TabClose.PlayOneShotOnCamera();
                        }
                        else
                        {
                            SoundDefOf.TabOpen.PlayOneShotOnCamera();
                        }
                    }
                    if (Mouse.IsOver(headerSection))
                    {
                        Widgets.DrawHighlight(headerSection);
                    }
                    headerSection.xMin += collapsibleMenuSection.width;
                }
                Widgets.Label(headerSection, label);
                if (!adding)
                {
                    Text.Anchor = TextAnchor.UpperRight;
                    GUI.color = ColoredText.SubtleGrayColor;
                    Widgets.Label(new Rect(headerSection.xMax - (2 * Margin), yIndex, rect.width - headerSection.width, Text.LineHeight), "ClickToAddOrRemove".Translate());
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                yIndex += Text.LineHeight;
            }
            if (collapsed == true)
            {
                if (Event.current.type == EventType.Layout)
                {
                    sectionHeight = 0f;
                }
                return;
            }
            float headerSectionHeight = yIndex;
            bool reachedCategoryEnd = false;
            Rect contentBGSection = new Rect(xIndex, yIndex, rect.width, rect.height);
            if (!adding)
            {
                Widgets.DrawRectFast(contentBGSection, Widgets.MenuSectionBGFillColor);
                yIndex += Margin;
            }
            if (!directives.Any())
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = ColoredText.SubtleGrayColor;
                Widgets.Label(contentBGSection, "(" + "NoneLower".Translate() + ")");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else
            {
                string directiveCategory = null;
                for (int i = 0; i < directives.Count; i++)
                {
                    DirectiveDef directiveDef = directives[i];
                    if (!directiveDef.EverValidFor(pawn))
                    {
                        continue;
                    }
                    bool reachedWidthLimit = false;
                    if (xIndex + directiveBlockWidth > rect.width)
                    {
                        xIndex = rect.x;
                        yIndex += directiveBlockHeight + Margin;
                        reachedWidthLimit = true;
                    }
                    bool categoryCollapsed = collapsedCategories[directiveDef.directiveCategory];
                    if (adding && directiveCategory != directiveDef.directiveCategory)
                    {
                        if (!reachedWidthLimit && reachedCategoryEnd)
                        {
                            xIndex = rect.x;
                            yIndex += directiveBlockHeight + Margin;
                        }
                        directiveCategory = directiveDef.directiveCategory;
                        Rect categoryHeaderSection = new Rect(xIndex, yIndex, rect.width - 8f, Text.LineHeight);
                        Rect categoryCollapseIconSection = new Rect(categoryHeaderSection.x, categoryHeaderSection.y + (categoryHeaderSection.height - 16f) / 2f, 16f, 16f);
                        GUI.DrawTexture(categoryCollapseIconSection, categoryCollapsed ? TexButton.Reveal : TexButton.Collapse);
                        if (Widgets.ButtonInvisible(categoryHeaderSection))
                        {
                            collapsedCategories[directiveDef.directiveCategory] = !collapsedCategories[directiveDef.directiveCategory];
                            if (collapsedCategories[directiveDef.directiveCategory])
                            {
                                SoundDefOf.TabClose.PlayOneShotOnCamera();
                            }
                            else
                            {
                                SoundDefOf.TabOpen.PlayOneShotOnCamera();
                            }
                        }
                        if (i % 2 == 1)
                        {
                            Widgets.DrawLightHighlight(categoryHeaderSection);
                        }
                        if (Mouse.IsOver(categoryHeaderSection))
                        {
                            Widgets.DrawHighlight(categoryHeaderSection);
                        }
                        categoryHeaderSection.xMin += categoryCollapseIconSection.width;
                        Widgets.Label(categoryHeaderSection, directiveCategory);
                        yIndex += categoryHeaderSection.height;
                        if (!categoryCollapsed)
                        {
                            GUI.color = Color.grey;
                            Widgets.DrawLineHorizontal(xIndex, yIndex, rect.width - Margin);
                            GUI.color = Color.white;
                            yIndex += Margin;
                        }
                    }
                    if (adding && categoryCollapsed)
                    {
                        reachedCategoryEnd = false;
                        if (Event.current.type == EventType.Layout)
                        {
                            sectionHeight = yIndex - headerSectionHeight;
                        }
                        continue;
                    }
                    xIndex += Margin;
                    reachedCategoryEnd = true;
                    if (DrawDirective(directiveDef, !adding, ref xIndex, yIndex, containingRect) && directiveDef.ValidFor(pawn) && CompatibleWithSelections(directiveDef) && !inherentDirectives.Contains(directiveDef))
                    {
                        if (selectedDirectives.Contains(directiveDef))
                        {
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                            DirectiveRemoved(directiveDef);
                        }
                        else
                        {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                            DirectiveAdded(directiveDef);
                        }
                        break;
                    }
                }
            }
            if (!adding || reachedCategoryEnd)
            {
                yIndex += directiveBlockHeight + Margin;
            }
            if (Event.current.type == EventType.Layout)
            {
                sectionHeight = yIndex - headerSectionHeight;
            }
        }

        private bool DrawDirective(DirectiveDef directiveDef, bool listAllSection, ref float xIndex, float yIndex, Rect containingRect)
        {
            bool result = false;
            Rect blockSection = new Rect(xIndex, yIndex, directiveBlockWidth, directiveBlockHeight);
            if (!containingRect.Overlaps(blockSection))
            {
                xIndex = blockSection.xMax + Margin;
                return false;
            }
            bool selected = !listAllSection && selectedDirectives.Contains(directiveDef);
            Widgets.DrawOptionBackground(blockSection, selected);

            // Stat section
            xIndex += Margin / 3;
            float textLineHeight = Text.LineHeightOf(GameFont.Small);
            Text.Anchor = TextAnchor.MiddleRight;
            string directiveComplexity = directiveDef.complexityCost.ToStringWithSign();
            float statTextWidth = Text.CalcSize(directiveComplexity).x;
            Rect statSection = new Rect(xIndex, yIndex + (Margin / 3), statTextWidth, textLineHeight);
            Widgets.LabelFit(statSection, directiveComplexity);
            Text.Anchor = TextAnchor.UpperLeft;
            if (Mouse.IsOver(statSection))
            {
                Widgets.DrawHighlight(statSection);
                TooltipHandler.TipRegion(statSection, "ABF_Complexity".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "ABF_ComplexityDirectiveDesc".Translate());
            }
            xIndex += statTextWidth + (Margin / 3);
            yIndex += Margin / 2;

            // Icon section
            Rect iconWrapperSection = new Rect(xIndex, yIndex, directiveBlockWidth - statTextWidth - Margin, directiveBlockHeight);
            GUI.BeginGroup(iconWrapperSection);
            Rect iconSection = new Rect(iconWrapperSection.width - directiveIconSize, 0f, directiveIconSize, directiveIconSize);
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

            if (Mouse.IsOver(iconWrapperSection))
            {
                TooltipHandler.TipRegion(iconWrapperSection, delegate
                {
                    string text = directiveDef.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + directiveDef.description + "\n\n" + directiveDef.CustomDescription;
                    if (DirectiveTip(directiveDef) != null)
                    {
                        string tooltip = DirectiveTip(directiveDef);
                        if (!tooltip.NullOrEmpty() && tooltip != "ClickToRemove" && tooltip != "ClickToAdd")
                        {
                            text = text + "\n\n" + tooltip.Colorize(ColorLibrary.RedReadable);
                        }
                    }
                    return text;
                }, 201231048);
            }
            xIndex += 90f;
            if (Mouse.IsOver(blockSection))
            {
                hoveredDirective = directiveDef;
                hoveringOverDirective = true;
            }
            else if (hoveredDirective != null && !directiveDef.CompatibleWith(hoveredDirective))
            {
                Widgets.DrawLightHighlight(blockSection);
            }
            if (Widgets.ButtonInvisible(blockSection))
            {
                result = true;
            }
            xIndex = Mathf.Max(xIndex, blockSection.xMax + Margin);
            return result;
        }

        private string DirectiveTip(DirectiveDef directiveDef)
        {
            if (inherentDirectives.NotNullAndContains(directiveDef))
            {
                return "ABF_CantRemoveInherentDirectives".Translate(directiveDef.LabelCap, pawn.LabelShortCap);
            }
            if (!selectedDirectives.NotNullAndContains(directiveDef))
            {
                AcceptanceReport isValid = directiveDef.ValidFor(pawn);
                if (!isValid)
                {
                    return "ABF_InvalidFor".Translate(directiveDef.LabelCap, pawn.LabelShortCap, isValid.Reason);
                }
            }
            AcceptanceReport compatibilityReport = CompatibleWithSelections(directiveDef);
            if (!compatibilityReport)
            {
                return compatibilityReport.Reason;
            }
            return (selectedDirectives.Contains(directiveDef) ? "ClickToRemove" : "ClickToAdd").Translate().Colorize(ColoredText.SubtleGrayColor);
        }

        private void DirectiveAdded(DirectiveDef directiveDef)
        {
            selectedDirectives.Add(directiveDef);
            directiveComplexity += directiveDef.complexityCost;
        }

        private void DirectiveRemoved(DirectiveDef directiveDef)
        {
            selectedDirectives.Remove(directiveDef);
            directiveComplexity -= directiveDef.complexityCost;
        }

        private bool CanAccept()
        {
            foreach (DirectiveDef selectedDirective in selectedDirectives)
            {
                if (programmableDroneExtension?.inherentDirectives?.Contains(selectedDirective) != true && !selectedDirective.requirementWorkers.NullOrEmpty())
                {
                    foreach (DirectiveRequirementWorker requirementWorker in selectedDirective.requirementWorkers)
                    {
                        AcceptanceReport valid = requirementWorker.ValidFor(pawn);
                        if (!valid)
                        {
                            Messages.Message("ABF_InvalidFor".Translate(selectedDirective.label, pawn.LabelShortCap, valid.Reason), null, MessageTypeDefOf.RejectInput, historical: false);
                            return false;
                        }
                    }
                }
            }
            if (!selectedDirectives.NullOrEmpty() && selectedDirectives.Count - inherentDirectiveCount > maxDirectives)
            {
                Messages.Message("ABF_MaxDirectivesExceeded".Translate(selectedDirectives.Count - inherentDirectiveCount, pawn.LabelShortCap, maxDirectives), null, MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            return true;
        }

        // Check whether a proposed directive is compatible with other selected directives. RequirementWorkers do not have access to the selected list.
        private AcceptanceReport CompatibleWithSelections(DirectiveDef directiveDef)
        {
            foreach (DirectiveDef selectedDirective in selectedDirectives)
            {
                if (!selectedDirective.requirementWorkers.NullOrEmpty())
                {
                    foreach (DirectiveRequirementWorker requirementWorker in selectedDirective.requirementWorkers)
                    {
                        AcceptanceReport compatibility = requirementWorker.CompatibleWith(directiveDef);
                        if (!compatibility)
                        {
                            return compatibility.Reason;
                        }
                    }
                }
                if (!directiveDef.requirementWorkers.NullOrEmpty())
                {
                    foreach (DirectiveRequirementWorker requirementWorker in directiveDef.requirementWorkers)
                    {
                        AcceptanceReport compatibility = requirementWorker.CompatibleWith(selectedDirective);
                        if (!compatibility)
                        {
                            return compatibility.Reason;
                        }
                    }
                }
            }
            return true;
        }

        private void Accept()
        {
            programComp.UpdateComplexity("Active Directives", directiveComplexity);
            programComp.SetDirectives(selectedDirectives);
            Close();
        }
    }

}