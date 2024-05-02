using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    // Mod extension for artificial being pawn kinds to specify what state they should start with when they are generated.
    public class ABF_ArtificialPawnKindExtension : DefModExtension
    {
        // State that the pawn should have when generated.
        public ABF_ArtificialState pawnState = ABF_ArtificialState.Sapient;

        /* Features for Reprogrammable Drones */

        // Work Types that the drone must generate with, even if this exceeds complexity limits and the discretionary complexity.
        public List<WorkTypeDef> requiredWorkTypes = new List<WorkTypeDef>();

        // Directives that the drone must generate with, even if this exceeds complexity limits and the discretionary complexity.
        public List<DirectiveDef> requiredDirectives = new List<DirectiveDef>();

        // How many non-mandatory, non-inherent directives the drone may generate with if it does not exceed discretionary complexity.
        public IntRange discretionaryDirectives = new IntRange(0, 0);

        // How many non-mandatory, non-inherent work types the drone may generate with if it does not exceed discretionary complexity.
        public IntRange discretionaryWorkTypes = new IntRange(1, 3);

        // How much complexity the drone can use for random generation on top of their required complexity - does not take limits into account.
        public IntRange discretionaryComplexity = new IntRange(0, 10);
    }
}