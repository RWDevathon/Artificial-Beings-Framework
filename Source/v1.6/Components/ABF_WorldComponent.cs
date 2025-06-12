using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    public class ABF_WorldComponent : WorldComponent
    {
        // List of directives that need to be ticked every tick.
        List<Directive> tickDirectives = new List<Directive>();

        // Hashed List of directives that need to be ticked every rare tick (250 ticks). One index per tick.
        List<List<Directive>> rareTickDirectives = new List<List<Directive>>();

        // Hashed List of directives that need to be ticked every long tick (2000 ticks). One index per tick.
        List<List<Directive>> longTickDirectives = new List<List<Directive>>();

        // Dictionary of directives that are requesting to be registered with the matching list of tickers.
        private Dictionary<Directive, TickerType> directivesToRegister = new Dictionary<Directive, TickerType>();

        // Dictionary of directives that are requesting to be deregistered with the matching list of tickers.
        private Dictionary<Directive, TickerType> directivesToDeregister = new Dictionary<Directive, TickerType>();

        public ABF_WorldComponent(World world) : base(world)
        {
            ABF_Utils.cachedPawnStates.Clear();
            for (int i = GenTicks.TickRareInterval; i > 0; i--)
            {
                rareTickDirectives.Add(new List<Directive>());
            }
            for (int i = GenTicks.TickLongInterval; i > 0; i--)
            {
                longTickDirectives.Add(new List<Directive>());
            }
        }

        public override void WorldComponentTick()
        {
            foreach (Directive directive in directivesToRegister.Keys)
            {
                BucketOf(directivesToRegister[directive], directive).Add(directive);
            }
            directivesToRegister.Clear();

            foreach (Directive directive in directivesToDeregister.Keys)
            {
                BucketOf(directivesToDeregister[directive], directive).Remove(directive);
            }
            directivesToDeregister.Clear();

            List<Directive> directives = tickDirectives;
            for (int i = directives.Count - 1; i >= 0; i--)
            {
                directives[i].Tick();
            }

            directives = rareTickDirectives[Find.TickManager.TicksGame % GenTicks.TickRareInterval];
            for (int i = directives.Count - 1; i >= 0; i--)
            {
                directives[i].TickRare();
            }

            directives = longTickDirectives[Find.TickManager.TicksGame % GenTicks.TickLongInterval];
            for (int i = directives.Count - 1; i >= 0; i--)
            {
                directives[i].TickLong();
            }
        }

        public void RegisterDirective(Directive directive, TickerType tickType)
        {
            directivesToRegister.Add(directive, tickType);
        }

        public void DeregisterDirective(Directive directive, TickerType tickType)
        {
            directivesToDeregister.Add(directive, tickType);
        }

        private List<Directive> BucketOf(TickerType type, Directive directive)
        {
            int hashCode = directive.pawn.GetHashCode();
            if (hashCode < 0)
            {
                hashCode *= -1;
            }

            switch (type)
            {
                case TickerType.Normal:
                    return tickDirectives;
                case TickerType.Rare:
                    return rareTickDirectives[hashCode % GenTicks.TickRareInterval];
                case TickerType.Long:
                    return longTickDirectives[hashCode % GenTicks.TickLongInterval];
                default:
                    return null;
            }
        }
    }
}
