using Verse;

namespace ArtificialBeings
{
    public class Directive_Ticker : Directive
    {
        // Method for reacting to the Directive being added to a particular pawn.
        public override void PostAdd()
        {
            base.PostAdd();
            if (def.tickerType != TickerType.Never)
            {
                Find.World.GetComponent<ABF_WorldComponent>().RegisterDirective(this, def.tickerType);
            }
        }

        // Method for acting after the Directive is removed from a pawn (reprogrammed).
        public override void PostRemove()
        {
            base.PostRemove();
            if (def.tickerType != TickerType.Never)
            {
                Find.World.GetComponent<ABF_WorldComponent>().DeregisterDirective(this, def.tickerType);
            }
        }
    }
}
