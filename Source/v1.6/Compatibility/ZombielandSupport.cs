using ArtificialBeings;
using Verse;

public class ZombielandSupport
{
    // Artificial pawns can not be zombies.
    public static bool CanBecomeZombie(Pawn pawn)
    {
        return !ABF_Utils.IsArtificial(pawn);
    }

    // Artificial pawns do not attract the attention of zombies.
    public static bool AttractsZombies(Pawn pawn)
    {
        return !ABF_Utils.IsArtificial(pawn);
    }
}