using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class SurgeryOutcome_FailureRefund : SurgeryOutcome_Failure
    {
        public override bool Apply(float quality, RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part)
        {
            if (CanApply(recipe))
            {
                ApplyDamage(patient, part);
                PostDamagedApplied(patient);
                if (!patient.Dead)
                {
                    TryGainBotchedSurgeryThought(patient, surgeon);
                }
                SendLetter(surgeon, patient, recipe);
                return true;
            }
            return false;
        }

        public virtual bool RefundIngredients(float quality, RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part)
        {
            foreach (IngredientCount ingredientCount in recipe.ingredients)
            {
                if (ingredientCount.IsFixedIngredient)
                {
                    Thing thing = ThingMaker.MakeThing(ingredientCount.FixedIngredient);
                    thing.stackCount = (int)ingredientCount.GetBaseCount();
                    GenSpawn.Spawn(thing, surgeon.Position, surgeon.Map);
                }
            }
            return true;
        }
    }
}