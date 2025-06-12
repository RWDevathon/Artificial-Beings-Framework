using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ArtificialBeings
{
    // Simple mod extension for marking how much of a particular need this item fulfills, used for fulfilling artificial needs primarily.
    public class ABF_NeedFulfillerExtension : DefModExtension
    {
        // Dictionary linking NeedDefs to how much that need will be offset when this is consumed.
        public Dictionary<NeedDef, float> needOffsetRelations;

        public override IEnumerable<string> ConfigErrors()
        {
            if (needOffsetRelations == null)
            {
                yield return "[ABF] A ThingDef has the extension for fulfilling needs but did not specify any needs to fulfill.";
            }
        }
    }
}