using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace ArtificialBeings
{
    public class MedicalCareUtility_Patch
    {
        // The medical care selector uses hardcoded icons and categories that may not suit the pawn's race.
        [HarmonyPatch(typeof(MedicalCareUtility), "MedicalCareSelectButton")]
        public class MedicalCareUtility_MedicalCareSelectButton_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Rect rect, Pawn pawn)
            {
                List<ThingDef> legalMedicines = new List<ThingDef>();
                if (!ABF_Utils.cachedRaceMedicines.TryGetValue(pawn.def, out legalMedicines) && !ABF_Utils.cachedRaceMedicines.TryGetValue(ThingDefOf.Human, out legalMedicines))
                {
                    Log.ErrorOnce("[ABF] race " + pawn.def + " has no cached medicines and there are no defaults to use! A third-party mod has seriously altered how races are set up. " + pawn.LabelShort + " and the matching race will not be able to use any medicines!", 948123);
                    return true;
                }
                List<int> iconIndices = ABF_Utils.GetCategoryMarkerIndices(legalMedicines);

                Func<Pawn, MedicalCareCategory> getPayload = new Func<Pawn, MedicalCareCategory>(MedicalCareSelectButton_GetMedicalCare);
                Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>>> menuGenerator = new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>>>(MedicalCareSelectButton_GenerateMenu);
                Texture2D buttonIcon;
                int medCareInt = (int)pawn.playerSettings.medCare;

                // No Care/No Meds icons are the same for all races, and use vanilla icons if we did not find one for the race.
                if (iconIndices[medCareInt] == -1)
                {
                    buttonIcon = ABF_Utils.GetVanillaMedicalIcon(medCareInt);
                }
                // Grab the icon of whatever the legal medicine was
                else
                {
                    buttonIcon = legalMedicines[iconIndices[medCareInt]].uiIcon;
                }

                Widgets.Dropdown(rect, pawn, getPayload, menuGenerator, null, buttonIcon, null, null, null, true);
                return false;
            }

            private static MedicalCareCategory MedicalCareSelectButton_GetMedicalCare(Pawn pawn)
            {
                return pawn.playerSettings.medCare;
            }

            private static IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>> MedicalCareSelectButton_GenerateMenu(Pawn p)
            {
                List<ThingDef> legalMedicines = new List<ThingDef>();
                if (!ABF_Utils.cachedRaceMedicines.TryGetValue(p.def, out legalMedicines) && !ABF_Utils.cachedRaceMedicines.TryGetValue(ThingDefOf.Human, out legalMedicines))
                {
                    Log.ErrorOnce("[ABF] race " + p.def + " has no cached medicines and there are no defaults to use! A third-party mod has seriously altered how races are set up. " + p.LabelShort + " and the matching race will not be able to use any medicines!", 948123);
                    yield break;
                }
                List<int> targetIndices = ABF_Utils.GetCategoryMarkerIndices(legalMedicines);

                for (int i = 0; i < 5; i++)
                {
                    Texture2D icon;
                    int legalIndex = targetIndices[i];
                    MedicalCareCategory category = (MedicalCareCategory)i;
                    if (legalIndex > -1)
                    {
                        icon = legalMedicines[legalIndex].uiIcon;
                    }
                    else
                    {
                        icon = ABF_Utils.GetVanillaMedicalIcon(i);
                    }

                    if (i < 2 || legalIndex > -1)
                    {
                        yield return new Widgets.DropdownMenuElement<MedicalCareCategory>
                        {
                            option = new FloatMenuOption(category.GetLabel(), delegate
                            {
                                p.playerSettings.medCare = category;
                            }, icon, Color.white),
                            payload = category
                        };
                    }
                }
                yield return new Widgets.DropdownMenuElement<MedicalCareCategory>
                {
                    option = new FloatMenuOption("ChangeDefaults".Translate(), delegate
                    {
                        Find.WindowStack.Add(new Dialog_MedicalDefaults());
                    })
                };
            }
        }
    }
}