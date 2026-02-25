using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWAPI.NET;

namespace Shared.Interfaces
{
    public interface IGameData
    {
        Game Game { get; }

        IPlayerData Enemy();
        TilePosition GetBuildLocation(UnitType targetType, TilePosition desiredPosition, int maxRange);
        List<IUnitData> GetMinerals();
        List<IUnitData> GetNeutralUnits();
        IEnumerable<TilePosition> GetStartLocations();
        bool IsExplored(TilePosition tilePosition);
        IPlayerData Self();
        void SendText(string text);
    }
}
