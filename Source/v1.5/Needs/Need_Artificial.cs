using RimWorld;
using Verse;

namespace ArtificialBeings
{
    public class Need_Artificial : Need
    {
        private int ticksAtZero = 0;

        private float maxLevel = 1f;

        private ABF_ArtificialNeedExtension needExtension;

        public int TicksAtZero => ticksAtZero;

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

            if (CurLevel <= 0.001)
            {
                ticksAtZero += 150;
                if (NeedExtension.hediffToApplyOnEmpty != null)
                {
                    HealthUtility.AdjustSeverity(pawn, NeedExtension.hediffToApplyOnEmpty, 150 * (NeedExtension.hediffRisePerDay / 60000));
                }
            }
            else
            {
                ticksAtZero = 0;
                if (NeedExtension.hediffToApplyOnEmpty != null)
                {
                    HealthUtility.AdjustSeverity(pawn, NeedExtension.hediffToApplyOnEmpty, -150 * (NeedExtension.hediffFallPerDay / 60000));
                }
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
            Scribe_Values.Look(ref ticksAtZero, "ABF_ticksAtZero", 0);
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
