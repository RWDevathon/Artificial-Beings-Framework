using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class Recipe_ReprogramDrone : Recipe_Surgery
    {
        // Always available for foreign drones and all reprogrammable drones.
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            Pawn pawn = thing as Pawn;
            return ABF_Utils.IsProgrammableDrone(pawn) || (ABF_Utils.IsArtificialDrone(pawn) && pawn.Faction != Faction.OfPlayer);
        }

        // Reprogram the drone.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (recipe.addsHediff != null)
            {
                pawn.health.AddHediff(recipe.addsHediff, part);
            }
            // Non-player pawns have foreign programming that may result in failure.
            if (pawn.Faction != Faction.OfPlayer)
            {
                if (!CheckSurgeryFail(billDoer, pawn, ingredients, part, null))
                {
                    TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                    {
                        billDoer,
                        pawn
                    });
                    pawn.SetFaction(Faction.OfPlayer, null);
                    Find.LetterStack.ReceiveLetter("ABF_ReprogramSuccess".Translate(), "ABF_ReprogramSuccessDesc".Translate(pawn.Name.ToStringShort), LetterDefOf.PositiveEvent, pawn, null);
                }
                else
                {
                    // If you failed to convert the drone to your faction, you do not get to reprogram it.
                    return;
                }
            }

            if (ABF_Utils.IsProgrammableDrone(pawn))
            {
                Find.WindowStack.Add(new Dialog_ReprogramDrone(pawn));
                // If the unit had the no programming hediff, remove that hediff.
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ABF_HediffDefOf.ABF_Hediff_Artificial_Disabled);
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
                // Reprogrammable drones do not need to restart after programming is complete.
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ABF_HediffDefOf.ABF_Hediff_Artificial_Incapacitated);
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }
    }
}