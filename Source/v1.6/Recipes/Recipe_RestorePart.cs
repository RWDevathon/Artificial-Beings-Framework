using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace ArtificialBeings
{
    public class Recipe_RestorePart : Recipe_Surgery
    {
        // This surgery may be done on any missing part, and on damaged or defective part if the restore extension doesn't forbid it. Get the list of them and return it.
        // No reason to apply this to a part that has its parent missing, damaged, or is an added part. Restoring the parent would restore this part. IE. Why restore a finger when you can restore the hand?
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if (recipe.GetModExtension<ABF_RestorePartExtension>()?.missingPartsOnly == true)
            {
                foreach (BodyPartRecord part in pawn.def.race.body.AllParts)
                {
                    if (pawn.health.hediffSet.PartIsMissing(part) && !pawn.health.hediffSet.PartIsMissing(part.parent) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                    {
                        yield return part;
                    }
                }
            }
            else
            {
                foreach (BodyPartRecord part in GetMissingOrDamagedParts(pawn))
                {
                    if (!pawn.health.hediffSet.PartIsMissing(part.parent) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                    {
                        yield return part;
                    }
                }
            }
        }

        // Restore the body part and all of its child parts. This surgery can not fail, and will never be treated as a violation of other faction pawns.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        { 
            if (billDoer != null)
            {
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }

            ABF_RestorePartExtension restorePartExtension = recipe.GetModExtension<ABF_RestorePartExtension>();

            // Restore 100 points of severity for this part and child parts to normal functionality.
            float HPForRepair = restorePartExtension?.severityToRestore ?? 100f;
            bool fullRestore = restorePartExtension?.fullRestore ?? false;
            RestoreParts(pawn, part, ref HPForRepair, fullRestore);

            // If not all hp was used in a non-core part, then apply remaining hp to the Core part and its children.
            if ((HPForRepair > 0 || fullRestore) && (restorePartExtension?.propagateUpwards ?? false) && part != pawn.def.race.body.corePart)
            {
                RestoreParts(pawn, pawn.def.race.body.corePart, ref HPForRepair, fullRestore);
            }
        }

        // Return an enumerable of all the missing or damaged body parts on this pawn.
        protected IEnumerable<BodyPartRecord> GetMissingOrDamagedParts(Pawn pawn)
        { 
            List<BodyPartRecord> allPartsList = pawn.def.race.body.AllParts;
            foreach (BodyPartRecord part in allPartsList)
            {
                if (pawn.health.hediffSet.PartIsMissing(part) || pawn.health.hediffSet.hediffs.Any(hediff => hediff.Part == part && (hediff.def.tendable || hediff.def.chronic)))
                {
                    yield return part;
                }
            }
        }

        // Recursively restore children parts of the originally restored part. IE. hands and fingers when an arm was restored.
        private void RestoreParts(Pawn pawn, BodyPartRecord part, ref float HPLeftToRestoreChildren, bool fullRestore)
        {
            if (part == null || (HPLeftToRestoreChildren <= 0 && !fullRestore))
                return;

            // Acquire a list of all hediffs on this specific part, and prepare a bool to check if this part has hediffs that can't be handled with the available points.
            List<Hediff> targetHediffs = pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part == part && !hediff.def.keepOnBodyPartRestoration && hediff.def.isBad).ToList();

            // Destroy hediffs that does not put the HPLeft below 0. If there is any hediff with a severity too high, then recursion stops at this node.
            foreach (Hediff hediff in targetHediffs)
            {
                // Full restore ignores severity and removes all negative hediffs.
                if (fullRestore)
                {
                    pawn.health.RemoveHediff(hediff);
                    continue;
                }

                // If the Hediff has injuryProps, it's an injury whose severity matches the amount of lost HP.
                // If it does not have injuryProps, it's a disease or other condition whose severity is likely between 0 - 1 and should be adjusted to not be insignificant compared to injuries.
                float severity = hediff.Severity * (hediff.def.injuryProps == null ? 10 : 1);

                if (HPLeftToRestoreChildren < severity)
                {
                    // Injury severity can be reduced directly.
                    if (hediff.def.injuryProps != null)
                    {
                        hediff.Severity -= HPLeftToRestoreChildren;
                        HPLeftToRestoreChildren = 0;
                    }
                    // If the part is missing entirely and there is at least half the HP necessary to restore the part or half of the original HP unused, let it get away with it.
                    else if (hediff.def == HediffDefOf.MissingBodyPart && (severity / 2 < HPLeftToRestoreChildren || HPLeftToRestoreChildren >= 50))
                    {
                        pawn.health.RemoveHediff(hediff);
                        HPLeftToRestoreChildren = 0;
                    }
                }
                else
                {
                    HPLeftToRestoreChildren -= severity;
                    pawn.health.RemoveHediff(hediff);
                }
            }

            foreach (BodyPartRecord childPart in part.GetDirectChildParts())
            {
                RestoreParts(pawn, childPart, ref HPLeftToRestoreChildren, fullRestore);
            }
        }
    }
}