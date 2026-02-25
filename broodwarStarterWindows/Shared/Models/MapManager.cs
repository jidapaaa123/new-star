using BWAPI;
using BWAPI.NET;
using BWEM;
using BWEM.NET;
using Shared.Interfaces;
using Shared.Wrappers;
using System;
using System.Linq;

public class MapManager : IMapManager
{
    public IGameData GameData { get; private set; }
    private Map? _map = null;
    private int _frameCount = 0;

    public bool IsInitialized { get; set; } = false;
    public MapManager(IGameData gameData)
    {
        GameData = gameData;
    }

    private void ensureMapInitialized()
    {
        if (IsInitialized || _map != null) return;

        if (GameData.Game == null)
            throw new InvalidOperationException("Cannot initialize map without a Game instance");

        _map = new Map(GameData.Game);
        _map.Initialize();
        IsInitialized = true;
    }

    public List<TilePosition> GetScoutingTargets()
    {
        TilePosition myStart = GameData.Self().GetStartLocation();

        // Get all possible starting locations from the game engine
        // and filter out our own base.
        return GameData.GetStartLocations()
            .Where(loc => loc != myStart)
            .ToList();
    }

    public ChokePoint? GetMainChokepoint()
    {
        ensureMapInitialized();

        // 1. Get the Area of our starting location (The Main)
        TilePosition startTile = GameData.Self().GetStartLocation();
        Area mainArea = _map.GetArea(startTile);

        // 2. Find the Natural expansion
        // We look for the base that isn't our start location, but is closest to it
        var naturalBase = _map.Bases
            .Where(b => b.Location != startTile)
            .OrderBy(b => b.Location.GetDistance(startTile))
            .FirstOrDefault();

        if (mainArea == null || naturalBase == null) return null;

        Area naturalArea = naturalBase.Area;

        // 3. Find the ChokePoint that connects Main Area to Natural Area
        // ChokePoints are essentially edges in a graph connecting two Areas
        return mainArea.ChokePoints
            .FirstOrDefault(cp => cp.Areas.First == naturalArea ||
                                 cp.Areas.Second == naturalArea);
    }

    public Area? GetNaturalArea()
    {
        ensureMapInitialized();

        TilePosition startTile = GameData.Self().GetStartLocation();

        // Find the base that is NOT our start location but is very close
        var naturalBase = _map.Bases
            .Where(b => b.Location != startTile)
            .OrderBy(b => b.Location.GetDistance(startTile))
            .FirstOrDefault();

        return naturalBase?.Area;
    }

    public TilePosition? GetNaturalExpansion()
    {
        ensureMapInitialized();

        TilePosition startTile = GameData.Self().GetStartLocation();

        // Find the closest base by ground distance, not as-the-crow-flies
        var naturalBase = GameData.GetStartLocations()
            .Where(b => b!= startTile)
            .OrderBy(b => 
                {
                    _map.GetPath(startTile.ToPosition(), b.ToPosition(), out int groundDistance);
                    return groundDistance;
                })
            .FirstOrDefault();

        return naturalBase;
    }

    public static MapManager CreateForTesting(IGameData gameData)
    {
        return new MapManager(gameData);
    }
}