using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ArtificialBeings
{
    public class ITab_Programming : ITab
    {
        private const float margin = 12f;

        private Vector2 scrollPosition;

        private const float directiveBlockSize = 80f;

        private float summaryHeight = Text.LineHeight * 1.5f;

        public ITab_Programming()
        {
            size = new Vector2(Mathf.Min((3 * directiveBlockSize) + (6 * margin), UI.screenWidth), 350f);
            labelKey = "ABF_TabProgramming";
        }

        public override bool IsVisible => Find.Selector.SingleSelectedThing is Pawn pawn && ABF_Utils.IsProgrammableDrone(pawn);

        protected override void FillTab()
        {
            Rect tabWindow = new Rect(margin, margin, size.x - (2 * margin), size.y - margin);
            float xIndex = tabWindow.x;
            float yIndex = tabWindow.y;
            Pawn pawn = SelThing as Pawn;

            // Header
            Rect headerSect = new Rect(xIndex, yIndex, tabWindow.width, Text.LineHeight);
            Widgets.Label(headerSect, "ABF_SelectedDirectives".Translate());
            yIndex += headerSect.height + margin;

            // Directives
            Rect directivesSect = new Rect(xIndex, yIndex, tabWindow.width, tabWindow.height - headerSect.height - (3 * margin) - summaryHeight);
            Widgets.BeginScrollView(new Rect(xIndex, yIndex, directivesSect.width, directivesSect.height), ref scrollPosition, directivesSect);
            float xMaxIndex = xIndex + tabWindow.width;
            Rect directiveBGSect = new Rect(xIndex, yIndex, directivesSect.width, directivesSect.height);
            Widgets.DrawRectFast(directiveBGSect, Widgets.MenuSectionBGFillColor);
            xIndex += margin;
            yIndex += margin;

            CompArtificialPawn programComp = pawn.GetComp<CompArtificialPawn>();

            IEnumerable<DirectiveDef> directives = programComp.ActiveDirectives;
            if (directives.EnumerableNullOrEmpty())
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = ColoredText.SubtleGrayColor;
                Widgets.Label(directiveBGSect, "(" + "NoneLower".Translate() + ")");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else
            {
                foreach (DirectiveDef directive in directives)
                {
                    if (xIndex + directiveBlockSize + margin > xMaxIndex)
                    {
                        xIndex = directivesSect.x + margin;
                        yIndex += directiveBlockSize + margin;
                    }
                    Rect directiveSect = new Rect(xIndex, yIndex, directiveBlockSize, directiveBlockSize);
                    Widgets.DrawOptionBackground(directiveSect, false);
                    Widgets.DrawTextureFitted(directiveSect, directive.Icon, 0.75f);
                    GUI.BeginGroup(directiveSect);
                    float directiveLabelHeight = Text.CalcHeight(directive.LabelCap, directiveSect.width);
                    Rect directiveLabelSect = new Rect(0, directiveBlockSize - directiveLabelHeight, directiveSect.width, directiveLabelHeight);
                    GUI.DrawTexture(new Rect(0, directiveLabelSect.y, directiveLabelSect.width, directiveLabelHeight), TexUI.GrayTextBG);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(directiveLabelSect, directive.LabelCap);
                    Text.Anchor = TextAnchor.UpperLeft;
                    GUI.EndGroup();
                    if (Mouse.IsOver(directiveSect))
                    {
                        TooltipHandler.TipRegion(directiveSect, delegate
                        {
                            return directive.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + directive.description + "\n\n" + directive.CustomDescription;
                        }, 202248831);
                    }
                    xIndex = directiveSect.xMax + margin;
                }
            }
            Widgets.EndScrollView();

            // Complexity Summary
            Rect summarySect = new Rect(margin, tabWindow.height - summaryHeight, tabWindow.width, summaryHeight);
            int complexity = programComp.Complexity;
            GUI.BeginGroup(summarySect);
            Rect complexityIconSect = new Rect(summarySect.x, (summaryHeight - GenUI.SmallIconSize) / 2f, GenUI.SmallIconSize, GenUI.SmallIconSize);
            Rect complexityLabelSect = new Rect(complexityIconSect.xMax + margin, 0f, Text.CalcSize("ABF_ComplexityEffects".Translate()).x, summaryHeight);
            Rect complexityRow = new Rect(0f, complexityLabelSect.y, summarySect.width, summaryHeight);
            Widgets.DrawHighlightIfMouseover(complexityRow);
            TooltipHandler.TipRegion(complexityRow, "ABF_ComplexityDesc".Translate());
            GUI.DrawTexture(complexityIconSect, ABF_Textures.complexityEffectIcon);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(complexityLabelSect, "ABF_Complexity".Translate());
            Text.Anchor = TextAnchor.MiddleCenter;
            string complexityRelationText = complexity.ToString() + " / " + programComp.MaxComplexity.ToString();
            Widgets.Label(new Rect(complexityLabelSect.xMax + margin, 0f, Text.CalcSize(complexityRelationText).x + margin, summaryHeight), complexityRelationText);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }
    }
}