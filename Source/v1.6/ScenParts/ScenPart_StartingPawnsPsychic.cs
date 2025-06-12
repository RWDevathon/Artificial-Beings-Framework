using RimWorld;
using UnityEngine;
using Verse;

namespace ArtificialBeings
{
    public class ScenPart_StartingPawnsPsychic : ScenPart_PawnModifier
    {
        public int psylinkLevel = 1;

        private string psylinkLevelBuffer;

        public override string Summary(Scenario scen)
        {
            return "ABF_ScenPart_StartingPawnsPsychic".Translate(psylinkLevel);
        }

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(this, RowHeight);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(scenPartRect);
            listing_Standard.ColumnWidth = scenPartRect.width;
            listing_Standard.TextFieldNumeric(ref psylinkLevel, ref psylinkLevelBuffer, 1f, 10f);
            listing_Standard.End();
        }

        public override void PostMapGenerate(Map map)
        {
            if (Find.GameInitData == null || !context.Includes(PawnGenerationContext.PlayerStarter))
            {
                return;
            }
            foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
            {
                if (startingAndOptionalPawn.RaceProps.Humanlike && !ABF_Utils.IsArtificialDrone(startingAndOptionalPawn))
                {
                    ModifyHideOffMapStartingPawnPostMapGenerate(startingAndOptionalPawn);
                    startingAndOptionalPawn.ChangePsylinkLevel(psylinkLevel, false);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref psylinkLevel, "ABF_psylinkLevel", 0);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ psylinkLevel.GetHashCode();
        }
    }
}
