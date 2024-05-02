using Verse;

namespace ArtificialBeings
{
    public abstract class DirectiveRequirementWorker
    {
        public DirectiveDef def;

        public string requirementDescription;

        // Method for establishing whether a particular pawn may have this directive.
        public virtual AcceptanceReport ValidFor(Pawn pawn)
        {
            return EverValidFor(pawn);
        }

        // Method for establishing whether a particular pawn may ever have this directive. If it can not, it is hidden entirely.
        public virtual AcceptanceReport EverValidFor(Pawn pawn)
        {
            return true;
        }

        // Method for establishing whether the parent DirectiveDef is compatible with another.
        public virtual AcceptanceReport CompatibleWith(DirectiveDef other)
        {
            return true;
        }
    }
}
