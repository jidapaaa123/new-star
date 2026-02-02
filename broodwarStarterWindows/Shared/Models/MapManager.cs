using BWAPI;
using BWAPI.NET;
using BWEM;
using BWEM.NET;
using System;
using System.Linq;

public class MapManager
{
    private readonly Game _game;
    private readonly Map _map;
    private int _frameCount = 0;

    public bool IsInitialized { get; set; } = false;
    public MapManager(Game game)
    {
        _game = game;
        _map = new Map(game);

        _map.Initialize();

        IsInitialized = true;
    }

    public void CompleteInitialization()
    {
        if (IsInitialized)
            return;

        // Add a frame counter to allow more time for BWEM to initialize
        if (_frameCount < 10)
        {
            _frameCount++;
            return;
        }

        // Only attempt initialization if the game has basic data available
        if (_game.Self().GetUnits().Count == 0 && _game.GetNeutralUnits().Count == 0)
            return;

        try
        {
            _map.FindBasesForStartingLocations();
            _map.EnableAutomaticPathAnalysis();
            IsInitialized = true;
        }
        catch (NullReferenceException ex)
        {
            // If initialization fails, we'll retry on next call
            Console.WriteLine("Map initialization failed, will retry: " + ex.Message);
        }
    }

    public List<TilePosition> GetScoutingTargets()
    {
        TilePosition myStart = _game.Self().GetStartLocation();

        // Get all possible starting locations from the game engine
        // and filter out our own base.
        return _game.GetStartLocations()
            .Where(loc => loc != myStart)
            .ToList();
    }

    public ChokePoint? GetMainChokepoint()
    {
        CompleteInitialization();

        // 1. Get the Area of our starting location (The Main)
        TilePosition startTile = _game.Self().GetStartLocation();
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
        while (!IsInitialized)
            CompleteInitialization();

        TilePosition startTile = _game.Self().GetStartLocation();

        // Find the base that is NOT our start location but is very close
        var naturalBase = _map.Bases
            .Where(b => b.Location != startTile)
            .OrderBy(b => b.Location.GetDistance(startTile))
            .FirstOrDefault();

        return naturalBase?.Area;
    }
}