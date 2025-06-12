using Verse;

namespace ArtificialBeings
{
    public class ABF_MapComponent : MapComponent
    {
        public int playerDronesSpawned = 0;

        public ABF_MapComponent(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            if (map.IsHashIntervalTick(GenTicks.TickLongInterval))
            {
                playerDronesSpawned = 0;
                for (int i = map.mapPawns.FreeColonistsAndPrisonersSpawnedCount - 1; i >= 0; i--)
                {
                    if (ABF_Utils.IsArtificialDrone(map.mapPawns.FreeColonistsAndPrisonersSpawned[i]))
                    {
                        playerDronesSpawned++;
                    }
                }
            }
        }
    }
}
