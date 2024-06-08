using Verse;
using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;

namespace ArtificialBeings
{
    public class TendUtility_Patch
    {
        // Calculate artificial tend quality (hijacking medical tend quality)
        [HarmonyPatch(typeof(TendUtility), "CalculateBaseTendQuality")]
        [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Pawn), typeof(float), typeof(float) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        public class CalculateBaseTendQuality_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn doctor, Pawn patient, float medicinePotency, float medicineQualityMax, ref float __result)
            {
                // Nothing to hijack if the patient isn't artificial.
                if (!ABF_Utils.IsArtificial(patient))
                    return true;

                // Essentially vanilla TendQuality but using artificial tend quality rather than medical.
                float tendQuality;
                if (doctor != null)
                {
                    tendQuality = doctor.GetStatValue(ABF_StatDefOf.ABF_Stat_Artificial_TendQuality, true);
                }
                else
                {
                    tendQuality = 0.75f;
                }
                tendQuality *= medicinePotency;
                Building_Bed building_Bed = patient?.CurrentBed();
                if (building_Bed != null)
                {
                    tendQuality += building_Bed.GetStatValue(ABF_StatDefOf.ABF_Stat_Artificial_TendQualityOffset, true);
                }
                if (doctor == patient && doctor != null)
                {
                    tendQuality *= 0.7f;
                }
                __result = Mathf.Clamp(tendQuality, 0f, medicineQualityMax);
                return false;
            }
        }
    }
}