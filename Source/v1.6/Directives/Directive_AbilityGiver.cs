using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class Directive_AbilityGiver : Directive
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (def.abilities.NullOrEmpty())
            {
                return;
            }

            foreach (AbilityDef abilityDef in def.abilities)
            {
                pawn.abilities.GainAbility(abilityDef);
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (def.abilities.NullOrEmpty())
            {
                return;
            }

            foreach (AbilityDef abilityDef in def.abilities)
            {
                pawn.abilities.RemoveAbility(abilityDef);
            }
        }
    }
}
