using System;
using Verse;

namespace ArtificialBeings
{
    public static class ListingExtensions
    {
        public static void CheckboxLabeled(this Listing_Standard instance, string label, ref bool checkOn, string tooltip = null, Action onChange = null)
        {
            var valueBefore = checkOn;
            instance.CheckboxLabeled(label, ref checkOn, tooltip);
            if (checkOn != valueBefore)
            {
                onChange?.Invoke();
            }
        }
    }
}
