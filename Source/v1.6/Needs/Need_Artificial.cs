using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class Need_Artificial : Need
    {
        private float maxLevel = 1f;

        private ABF_ArtificialNeedExtension needExtension;

        public float AmountDesired => MaxLevel - CurLevel;

        protected ABF_ArtificialNeedExtension NeedExtension
        {
            get
            {
                if (needExtension == null)
                {
                    needExtension = def.GetModExtension<ABF_ArtificialNeedExtension>();
                }
                return needExtension;
            }
        }

        public float PercentageFallRatePerTick
        {
            get
            {
                return def.fallPerDay / 60000;
            }
        }

        public Need_Artificial(Pawn pawn) : base(pawn)
        {
        }

        public override void SetInitialLevel()
        {
            maxLevel = pawn.def.GetModExtension<ABF_ArtificialPawnExtension>()?.artificialNeeds.TryGetValue(def, 1f) ?? 1f;
            CurLevelPercentage = 1.0f;
        }

        public override float MaxLevel
        {
            get
            {
                return maxLevel;
            }
        }

        public override void NeedInterval()
        {
            if (IsFrozen)
            {
                return;
            }

            CurLevelPercentage -= 150 * PercentageFallRatePerTick;

            Hediff needHediff = pawn.health.hediffSet.GetFirstHediffOfDef(NeedExtension.hediffToApplyOnEmpty);
            if (CurLevel <= 0.001)
            {
                if (needHediff == null)
                {
                    needHediff = HediffMaker.MakeHediff(NeedExtension.hediffToApplyOnEmpty, pawn);
                    needHediff.Severity = 0f;
                    pawn.health.AddHediff(needHediff);
                }
                needHediff.Severity += 150 * (NeedExtension.hediffRisePerDay / 60000);
            }
            else if (needHediff != null)
            {
                needHediff.Severity -= 150 * (NeedExtension.hediffFallPerDay / 60000);
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                if (IsFrozen)
                {
                    return 0;
                }
                return -1;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                maxLevel = pawn.def.GetModExtension<ABF_ArtificialPawnExtension>()?.artificialNeeds.TryGetValue(def, 1f) ?? 1f;
            }
        }

        public override string GetTipString()
        {
            return (LabelCap + ": " + CurLevelPercentage.ToStringPercent()).Colorize(ColoredText.TipSectionTitleColor) + " (" + CurLevel.ToString("0.##") + " / " + MaxLevel.ToString("0.##") + ")\n" + def.description;
        }
    }
}
