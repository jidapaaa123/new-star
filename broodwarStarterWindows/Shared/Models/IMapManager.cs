using BWAPI.NET;
using BWEM.NET;
using Shared.Interfaces;

public interface IMapManager
{
    IGameData GameData { get; }
    bool IsInitialized { get; set; }
    ChokePoint? GetMainChokepoint();
    Area? GetNaturalArea();
    List<TilePosition> GetScoutingTargets();
}