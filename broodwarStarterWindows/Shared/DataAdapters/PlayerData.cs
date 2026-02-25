using BWAPI.NET;
using Shared.Interfaces;
using Shared.Wrappers;

namespace Shared.DataAdapters
{
    public class PlayerData : IPlayerData
    {
        private readonly Player _player;

        public PlayerData(Player player) => _player = player;

        public List<IUnitData> GetUnits() => _player?.GetUnits().Select(u => (IUnitData)new MyUnit(u)).ToList() ?? new();
        public int Gas() => _player?.Gas() ?? 0;
        public int SupplyUsed() => _player?.SupplyUsed() ?? 0;
        public int SupplyTotal() => _player?.SupplyTotal() ?? 0;
        public int Minerals() => _player?.Minerals() ?? 0;
        public int CompletedUnitCount(UnitType unitType) => _player?.CompletedUnitCount(unitType) ?? 0;
        public TilePosition GetStartLocation() => _player?.GetStartLocation() ?? TilePosition.None;
        public bool HasResearched(TechType type) => _player?.HasResearched(type) ?? false;
    }
}