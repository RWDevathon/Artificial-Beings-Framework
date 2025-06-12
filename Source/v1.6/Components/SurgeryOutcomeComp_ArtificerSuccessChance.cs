﻿using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ArtificialBeings
{
    // Override AffectQuality to use ABF_Stat_Artificial_SurgerySuccessChance instead of SurgerySuccessChance
    public class SurgeryOutcomeComp_ArtificerSuccessChance : SurgeryOutcomeComp_SurgeonSuccessChance
    {
        public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
        {
            quality *= surgeon.GetStatValue(ABF_StatDefOf.ABF_Stat_Artificial_SurgerySuccessChance);
        }
    }
}