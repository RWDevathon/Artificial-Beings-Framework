using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class StatPart_DroneNullified : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn && ABF_Utils.IsArtificialDrone(pawn))
            {
                val *= 0;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.Thing is Pawn pawn && ABF_Utils.IsArtificialDrone(pawn))
            {
                return "ABF_DroneNullifiedStat".Translate();
            }
            return null;
        }
    }
}
