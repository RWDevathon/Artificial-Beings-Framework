using Verse;

namespace ArtificialBeings
{
    public abstract class Directive : IExposable
    {
        public DirectiveDef def;

        [Unsaved(false)]
        public Pawn pawn;

        // Method for reacting to being made, prior to being added to a pawn.
        public virtual void PostMake()
        {
        }

        // Method for reacting to the Directive being added to a particular pawn.
        public virtual void PostAdd()
        {
        }

        // Method for reacting to the Directive being removed from a pawn (reprogrammed).
        public virtual void PostRemove()
        {
        }

        // Called every tick for a pawn, and is called too often to consider expensive computations.
        // Should only be used if real-time updates are absolutely necessary.
        public virtual void Tick()
        {
        }

        // Called every 250 ticks for a pawn. Best used for things that are computationally expensive
        // but need to be reasonably responsive to changes in the game state.
        public virtual void TickRare()
        {
        }

        // Called every 2000 ticks for a pawn. Best used for things that do not need to update in real-time
        // and do not need to be very responsive to game state changes.
        public virtual void TickLong()
        {
        }

        // Method for reacting to the host pawn spawning onto a map.
        public virtual void PostSpawn(Map map)
        {
        }

        // Method for reacting to the host pawn despawning off a map. Must be able to handle null cases.
        public virtual void PostDespawn(Map map)
        {
        }

        // This method saves fields for save files so that the information isn't lost when saving/loading.
        // The pawn field gets filled in by the CompArtificialPawn on loading.
        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "ABF_directiveDef");
        }
    }
}
