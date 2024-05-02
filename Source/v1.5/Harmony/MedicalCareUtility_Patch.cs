using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;
using UnityEngine;
using Verse.Sound;

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
                List<ThingDef> legalMedicines = ABF_Utils.cachedRaceMedicines[pawn.def];
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
                List<ThingDef> legalMedicines = ABF_Utils.cachedRaceMedicines[p.def];
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

        // The medical care setter uses hardcoded icons and categories that may not suit the pawn's race.
        //[HarmonyPatch(typeof(MedicalCareUtility), "MedicalCareSetter")]
        //public class MedicalCareUtility_MedicalCareSetter_Patch
        //{
        //    public static bool medicalCarePainting;

        //    [HarmonyPrefix]
        //    public static bool Listener(Rect rect, ref MedicalCareCategory medCare)
        //    {
        //        Rect textureRect = new Rect(rect.x, rect.y, rect.width / 5f, rect.height);
        //        List<ThingDef> legalMedicines;
        //        List<int> targetIndices;
        //        if (Find.Selector.SingleSelectedThing is Pawn pawn)
        //        {
        //            legalMedicines = ABF_Utils.cachedRaceMedicines[pawn.def];
        //        }
        //        else
        //        {
        //            legalMedicines = new List<ThingDef>();
        //        }
        //        targetIndices = ABF_Utils.GetCategoryMarkerIndices(legalMedicines);
        //        for (int i = 0; i < 5; i++)
        //        {
        //            Texture2D icon;
        //            int legalIndex = targetIndices[i];

        //            if (legalIndex > -1)
        //            {
        //                icon = legalMedicines[legalIndex].uiIcon;
        //            }
        //            else
        //            {
        //                icon = ABF_Utils.GetVanillaMedicalIcon(i);
        //            }

        //            MedicalCareCategory medicalCareCategory = (MedicalCareCategory)i;
        //            Widgets.DrawHighlightIfMouseover(textureRect);
        //            MouseoverSounds.DoRegion(textureRect);
        //            GUI.DrawTexture(textureRect, icon);
        //            Widgets.DraggableResult draggableResult = Widgets.ButtonInvisibleDraggable(textureRect);
        //            if (draggableResult == Widgets.DraggableResult.Dragged)
        //            {
        //                medicalCarePainting = true;
        //            }
        //            if ((medicalCarePainting && Mouse.IsOver(textureRect) && medCare != medicalCareCategory)
        //                || draggableResult == Widgets.DraggableResult.Pressed
        //                || draggableResult == Widgets.DraggableResult.DraggedThenPressed)
        //            {
        //                medCare = medicalCareCategory;
        //                SoundDefOf.Tick_High.PlayOneShotOnCamera();
        //            }
        //            if (medCare == medicalCareCategory)
        //            {
        //                Widgets.DrawBox(textureRect, 3);
        //            }
        //            if (Mouse.IsOver(textureRect))
        //            {
        //                TooltipHandler.TipRegion(textureRect, () => medicalCareCategory.GetLabel().CapitalizeFirst(), 435621 + i * 15);
        //            }
        //            textureRect.x += textureRect.width;
        //        }
        //        if (!Input.GetMouseButton(0))
        //        {
        //            medicalCarePainting = false;
        //        }
        //        return false;
        //    }

        //    private static MedicalCareCategory MedicalCareSelectButton_GetMedicalCare(Pawn pawn)
        //    {
        //        return pawn.playerSettings.medCare;
        //    }

        //    private static IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>> MedicalCareSelectButton_GenerateMenu(Pawn p)
        //    {
        //        List<ThingDef> legalMedicines = ABF_Utils.cachedRaceMedicines[p.def];
        //        List<int> targetIndices = ABF_Utils.GetCategoryMarkerIndices(legalMedicines);

        //        for (int i = 0; i < 5; i++)
        //        {
        //            Texture2D icon;
        //            int legalIndex = targetIndices[i];
        //            MedicalCareCategory category = (MedicalCareCategory)i;
        //            if (legalIndex > -1)
        //            {
        //                icon = legalMedicines[legalIndex].uiIcon;
        //            }
        //            else
        //            {
        //                icon = ABF_Utils.GetVanillaMedicalIcon(i);
        //            }

        //            if (i < 2 || legalIndex > -1)
        //            {
        //                yield return new Widgets.DropdownMenuElement<MedicalCareCategory>
        //                {
        //                    option = new FloatMenuOption(category.GetLabel(), delegate
        //                    {
        //                        p.playerSettings.medCare = category;
        //                    }, icon, Color.white),
        //                    payload = category
        //                };
        //            }
        //        }
        //    }
        //}

        // MEDICALCARESETTER
    }
}