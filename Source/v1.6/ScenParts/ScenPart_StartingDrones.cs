using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArtificialBeings
{
    public class ScenPart_StartingDrones : ScenPart
    {
        public List<PawnKindCount> droneCounts = new List<PawnKindCount>();

        private float ElementHeight => RowHeight * 2f + GenUI.GapTiny;

        public override string Summary(Scenario scen)
        {
            return "ABF_ScenPart_StartWithDrones".Translate(droneCounts.Select((PawnKindCount x) => x.Summary).ToCommaList(useAnd: true));
        }

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(this, ElementHeight * droneCounts.Count + RowHeight);
            scenPartRect.height = RowHeight;
            for (int i = droneCounts.Count - 1; i >= 0; i--)
            {
                PawnKindCount droneCount = droneCounts[i];
                Rect rect = scenPartRect;
                rect.xMax -= RowHeight;
                if (Widgets.ButtonText(rect, droneCount.kindDef.LabelCap))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (PawnKindDef def in ABF_Utils.GetStarterDroneKinds())
                    {
                        list.Add(new FloatMenuOption(def.LabelCap, delegate
                        {
                            droneCount.kindDef = def;
                        }));
                    }
                    if (list.Any())
                    {
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }
                Rect butRect = scenPartRect;
                butRect.xMin = rect.xMax;
                if (Widgets.ButtonImage(butRect, TexButton.Delete))
                {
                    droneCounts.RemoveAt(i);
                    return;
                }
                scenPartRect.y += RowHeight;
                Widgets.TextFieldNumeric(scenPartRect, ref droneCount.count, ref droneCount.countBuffer, 1f, 10f);
                scenPartRect.y += RowHeight + GenUI.GapTiny;
            }
            if (Widgets.ButtonText(scenPartRect, "Add"))
            {
                droneCounts.Add(new PawnKindCount
                {
                    kindDef = ABF_Utils.GetStarterDroneKinds().RandomElement(),
                    count = 1
                });
            }
        }

        public IEnumerable<Pawn> GetDrones()
        {
            foreach (PawnKindCount droneCount in droneCounts)
            {
                for (int i = droneCount.count; i > 0; i--)
                {
                    yield return PawnGenerator.GeneratePawn(new PawnGenerationRequest(droneCount.kindDef, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, forceGenerateNewPawn: true, canGeneratePawnRelations: false));
                }
            }
        }

        public override void PreMapGenerate()
        {
            base.PreMapGenerate();
            List<Pawn> pawns = GetDrones().ToList();
            GameInitData gameInitData = Find.GameInitData;
            gameInitData.startingAndOptionalPawns.AddRange(pawns);
            gameInitData.startingPawnCount += pawns.Count;

            // Drones do not start with possessions, but they need to be in the dictionary.
            foreach (Pawn pawn in pawns)
            {
                gameInitData.startingPossessions[pawn] = new List<ThingDefCount>();
            }
        }

        public override void PostGameStart()
        {
            base.PostGameStart();
            foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonists)
            {
                if (ABF_Utils.IsArtificialDrone(pawn))
                {
                    if (ABF_Utils.IsProgrammableDrone(pawn))
                    {
                        ChoiceLetter_ReprogramDrone choiceLetter = (ChoiceLetter_ReprogramDrone)LetterMaker.MakeLetter(ABF_LetterDefOf.ABF_Letter_Artificial_ReprogramDrone);
                        choiceLetter.subject = pawn;
                        choiceLetter.Label = "ABF_ReprogramDrone".Translate(pawn.LabelShortCap);
                        choiceLetter.Text = "ABF_ReprogramDroneDesc".Translate(pawn.LabelShortCap);
                        choiceLetter.lookTargets = pawn;
                        Find.LetterStack.ReceiveLetter(choiceLetter);
                    }
                }
            }
        }

        public override bool HasNullDefs()
        {
            if (base.HasNullDefs())
            {
                return true;
            }
            foreach (PawnKindCount droneCount in droneCounts)
            {
                if (droneCount.kindDef == null)
                {
                    return true;
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref droneCounts, "droneCounts", LookMode.Deep);
        }

        public override int GetHashCode()
        {
            int num = base.GetHashCode();
            foreach (PawnKindCount droneCount in droneCounts)
            {
                num ^= droneCount.GetHashCode();
            }
            return num;
        }
    }

}
