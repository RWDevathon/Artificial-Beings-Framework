using Verse;
using RimWorld;

namespace ArtificialBeings
{
    public class Recipe_RemoveAndRestoreBodyPart : Recipe_RemoveBodyPart
    {
        // Unlike normal operations, we will not leave the part missing and will assume it was replaced with standard issue parts.
        public override void DamagePart(Pawn pawn, BodyPartRecord part)
        {
            pawn.health.RestorePart(part);
        }
    }
}