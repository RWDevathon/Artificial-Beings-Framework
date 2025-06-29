using Verse;

namespace ArtificialBeings
{
    // ModExtension marking a NeedDef as belonging only to artificial pawns, with appropriate details for handling what pawns are valid and how to operate the Need.
    public class ABF_ArtificialNeedExtension : DefModExtension
    {
        // Floats for the thresholds at which this need should be considered critical to affect when the pawn should automatically try to satisfy it.
        public float criticalThreshold = 0.15f;

        // Used to describe the fall rate per day in the tooltip when hovering over the need. IE. consuming 400 watts per day
        public string unitsLabel = "";

        // Optional hediff to apply when need hits 0, and how quickly it will rise while 0 and fall when not 0, as well as a statFactor that can modify it.
        public HediffDef hediffToApplyOnEmpty;
        public float hediffRisePerDay = 1;
        public float hediffFallPerDay = 4;
    }
}