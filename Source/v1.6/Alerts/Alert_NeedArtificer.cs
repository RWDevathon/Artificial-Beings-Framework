using System.Collections.Generic;
using System.Text;
using Verse;
using RimWorld;

namespace ArtificialBeings
{
    public class Alert_NeedArtificer : Alert
    {
        private List<Pawn> patientsResult = new List<Pawn>();

        private List<Pawn> Patients
        {
            get
            {
                patientsResult.Clear();
                foreach (Map map in Find.Maps)
                {
                    if (!map.IsPlayerHome)
                        continue;

                    bool hasArtificer = false;
                    foreach (Pawn colonist in map.mapPawns.FreeColonists)
                    {
                        if ((colonist.Spawned || colonist.BrieflyDespawned()) && !colonist.Downed && colonist.workSettings != null && colonist.workSettings.WorkIsActive(ABF_WorkTypeDefOf.ABF_WorkType_Artificial_Artificer))
                        {
                            hasArtificer = true;
                            break;
                        }
                    }

                    if (hasArtificer)
                        continue;

                    List<Pawn> colonists = map.mapPawns.FreeColonists;
                    for (int i = colonists.Count - 1; i >= 0; i--)
                    {
                        Pawn colonist = colonists[i];
                        if ((colonist.Spawned || colonist.BrieflyDespawned()) && ABF_Utils.IsArtificial(colonist) && HealthAIUtility.ShouldBeTendedNowByPlayer(colonist))
                        {
                            patientsResult.Add(colonist);
                        }
                    }
                }

                return patientsResult;
            }
        }

        public Alert_NeedArtificer()
        {
            defaultLabel = "ABF_NeedArtificer".Translate();
            defaultPriority = AlertPriority.High;
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn patient in Patients)
            {
                stringBuilder.AppendLine("  - " + patient.NameShortColored.Resolve());
            }

            return "ABF_NeedArtificerDesc".Translate(stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            if (Find.AnyPlayerHomeMap == null)
            {
                return false;
            }

            return AlertReport.CulpritsAre(Patients);
        }
    }
}
