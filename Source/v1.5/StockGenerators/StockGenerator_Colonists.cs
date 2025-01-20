using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    public class StockGenerator_Colonists : StockGenerator
    {
        public bool respectPopulationIntent;

        public List<PawnKindDef> pawnKindDefs;

        public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
        {
            if (respectPopulationIntent && Rand.Value > StorytellerUtilityPopulation.PopulationIntent)
            {
                yield break;
            }

            for (int i = countRange.RandomInRange; i-- >= 0;)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(pawnKindDefs.RandomElement(), null, PawnGenerationContext.NonPlayer, forTile, forceGenerateNewPawn: true, forceBaselinerChance: 1, forceRecruitable: true);
                Pawn result = PawnGenerator.GeneratePawn(request);
                result.guest.joinStatus = JoinStatus.JoinAsColonist;
                yield return result;
            }
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike && thingDef.tradeability != Tradeability.None;
        }
    }
}
