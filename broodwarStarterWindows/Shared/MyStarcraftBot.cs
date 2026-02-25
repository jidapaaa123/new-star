using BWAPI.NET;
using BWEM.NET;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Data;
using Shared.DataAdapters;
using Shared.Interfaces;
using Shared.Models;
using Shared.Services;
using Shared.Wrappers;
using System.Collections.Concurrent;
using System.Numerics;
using System.Xml.Linq;

namespace Shared;

// library from https://www.nuget.org/packages/BWAPI.NET

public class MyStarcraftBot : DefaultBWListener
{
    private BWClient? _bwClient = null;
    private ILogger<MyStarcraftBot> _logger;
    private IMatchRepository _matchRepository;
    private IGameEventRepository _gameEventRepository;

    public Match Match { get; set; } = new Match();
    public Game? Game => _bwClient?.Game;
    public bool IsRunning { get; private set; } = false;
    public bool InGame { get; private set; } = false;
    public int? GameSpeedToSet { get; set; } = null;

    public event Action? StatusChanged;
    public GameStrategy? Strategy { get; set; } = null;
    public MyGame? MyGame => Game is null ? null : new MyGame(new GameData(Game));
    public MyPlayer? MyPlayer
    {
        get
        {
            if (Game is null)
                return null;
            Player self = Game.Self();
            return new MyPlayer(new PlayerData(self));
        }
    }
    public IConstructionManager ConstructionManager { get; } = new ConstructionManager();
    public IProductionManager ProductionManager { get; } = new ProductionManager();
    public MapManager MapManager { get; private set; }
    public ScoutingManager? ScoutingManager { get; private set; }
    public List<ScoutLocation>? PotentialBases => ScoutingManager?.PotentialBases;

    public WorkerDispatcher WorkerDispatcher = new();

    public MyStarcraftBot(ILogger<MyStarcraftBot> logger, IMatchRepository matchRepository, IGameEventRepository gameEventRepository)
    {
        _logger = logger;
        _matchRepository = matchRepository;
        _gameEventRepository = gameEventRepository;
        _offenseTeamManager = new OffenseTeamManager();
    }

    private ConcurrentQueue<BotCommand> _pendingCommands = new();

    private OffenseTeamManager _offenseTeamManager;
    private bool _botActive = true;
    private TilePosition? _expandLocation = null;
    private HashSet<TilePosition> _enemyFoundLocations = new();
    private bool _supplyBlockedLogged = false;

    /// <summary>
    /// Gets the current game state for broadcasting to clients.
    /// </summary>
    public GameStateDto GetCurrentGameState()
    {
        int workerCount = MyPlayer?.GetWorkerUnits().Count ?? 0;
        int militaryCount = MyPlayer?.GetUnits()
            .Count(u => !u.GetUnitType().IsBuilding() && !u.GetUnitType().IsWorker()) ?? 0;
        int minerals = MyPlayer?.Minerals() ?? 0;
        int gas = MyPlayer?.Gas() ?? 0;
        int supplyUsed = MyPlayer?.GetSupplyUsed() ?? 0;
        int supplyTotal = MyPlayer?.SupplyTotal() ?? 0;
        Strategy strategyMode = Strategy?.CurrentStrategy ?? Models.Strategy.Default;
        bool hasExpanded = MyPlayer?.GetBases().Count > 1;
        bool enemyScouted = PotentialBases?.Any(b => b.EnemyFound) ?? false;

        return new GameStateDto
        {
            WorkerCount = workerCount,
            MilitaryCount = militaryCount,
            Minerals = minerals,
            Gas = gas,
            SupplyUsed = supplyUsed,
            SupplyTotal = supplyTotal,
            StrategyMode = strategyMode,
            HasExpanded = hasExpanded,
            EnemyScouted = enemyScouted,
            IsRunning = IsRunning,
            InGame = InGame,
            LastUpdated = DateTime.UtcNow
        };
    }

    public void EnqueueCommand(BotCommand command)
    {
        _pendingCommands.Enqueue(command);
        Console.WriteLine($"Command '{command}' queued!"); // This will print from the API call
    }

    public void Connect()
    {
         _bwClient = new BWClient(this);
         IsRunning = true;
         _bwClient.StartGame();
    }

    // Bot Callbacks below
    public override void OnStart()
    {
        InGame = true;
        Game?.EnableFlag(Flag.UserInput); // let human control too

        _logger.LogInformation("JIDAPA : Game Started");

        // Create and save the match at the start of the game so events can be logged with a valid match ID
        try
        {
            Match.StartTime = DateTime.UtcNow;
            Match.Result = "Ongoing";
            _matchRepository.CreateMatchAsync(Match).Wait();
            _logger.LogInformation($"Match created with ID: {Match.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create match at game start");
        }

        SetDefaultStrategy();
        LogGameEvent("start", $"Hey! The game Began with {Strategy?.Name} Strategy! (from OnStart())");
        
        if (Game is not null)
        {
            MapManager = new(new GameData(Game));
            ScoutingManager = new ScoutingManager(new MyGame(new GameData(Game)), MapManager);
            ScoutingManager.UpdateScouting(true);
        }
        

    }

    public override void OnEnd(bool isWinner)
    {
        InGame = false;

        try
        {
            // Populate match end data
            Match.EndTime = DateTime.UtcNow;
            Match.Result = isWinner ? "Win" : "Loss";
            Match.FinalWorkerCount = MyPlayer?.GetWorkerUnits().Count ?? 0;
            Match.FinalMinerals = MyPlayer?.Minerals() ?? 0;
            Match.FinalGas = MyPlayer?.Gas() ?? 0;
            Match.FinalMilitaryCount = MyPlayer?.GetUnits().Count(u => u.GetUnitType().IsBuilding() == false && u.GetUnitType().IsWorker() == false) ?? 0;
            Match.UpgradesCompleted = 0;

            // Update the existing match (created in OnStart)
            _matchRepository.UpdateMatchAsync(Match.Id, Match).Wait();
            _logger.LogInformation("Match updated in database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update match in database");
        }
    }

    public override void OnFrame()
    {
        if (Game == null)
            return;
        
        HandleGameState();
        UpdateDebugDisplay();
        ProcessPendingCommands();
        
        if (!_botActive)
            return;

        ManageGameplay();
    }

    /// <summary>
    /// Handles core game state: game speed and map initialization.
    /// </summary>
    private void HandleGameState()
    {
        if (GameSpeedToSet != null)
        {
            Game!.SetLocalSpeed(GameSpeedToSet.Value);
            GameSpeedToSet = null;
        }

        if (MapManager is null || !MapManager.IsInitialized)
            MapManager = new(new GameData(Game!));
    }

    /// <summary>
    /// Updates all debug visualization on screen.
    /// </summary>
    private void UpdateDebugDisplay()
    {
        Game!.DrawTextScreen(100, 130, $"Supply: {MyPlayer?.GetSupplyUsed()} / {MyPlayer?.SupplyTotal()}");
        Game!.DrawTextScreen(100, 140, $"OtherBases: {PotentialBases?.Count}");
        Game!.DrawTextScreen(100, 150, $"ScoutUnitNull?: {ScoutingManager?.ScoutUnit is null}");
        Game!.DrawTextScreen(100, 160, $"Current Strategy: {Strategy?.CurrentStrategy}");
        Game!.DrawTextScreen(100, 170, $"Gas Gatherers: {Strategy?.GasGatherConfig} | Min Minerals: {Strategy?.MinimumMineralGatherConfig}");
        Game!.DrawTextScreen(100, 190, $"Construction Queued: {ConstructionManager.PendingConstructionOrder is not null}");
        Game!.DrawTextScreen(100, 200, $"Build Order Index: {Strategy?.CurrentBuildOrderIndex}");
        Game!.DrawTextScreen(100, 210, $"Build Order Type: {ConstructionManager.PendingConstructionOrder?.BuildingType}");
    }

    /// <summary>
    /// Processes all pending commands from the command queue.
    /// </summary>
    private void ProcessPendingCommands()
    {
        while (_pendingCommands.TryDequeue(out var command))
        {
            switch (command.Type)
            {
                case BotCommandType.ManageBunkerProduction:
                    //ManageBunkerProduction();
                    break;
                case BotCommandType.ManageSupplyDepotProduction:
                    //ManageSupplyDepotProduction();
                    break;
                case BotCommandType.ToggleStrategy:
                    ToggleStrategy();
                    break;
                case BotCommandType.ChangeStrategy:
                    if (command.StrategyType.HasValue && Strategy != null)
                    {
                        Strategy.ChangeStrategy(command.StrategyType.Value);
                    }
                    break;
                case BotCommandType.ToggleAttackEnemyBase:
                    _offenseTeamManager.ToggleAttackMode();
                    break;
                case BotCommandType.ScoutMap:
                    ScoutingManager?.UpdateScouting(true);
                    break;
                case BotCommandType.TogglePauseBot:
                    _botActive = !_botActive;
                    break;
                case BotCommandType.Expand:
                    OnExpand();
                    break;
            }
        }
    }

    /// <summary>
    /// Manages all active gameplay: scouting, construction, production, and build orders.
    /// </summary>
    private void ManageGameplay()
    {
        ManageScouting();
        ManageConstructionWorker();
        
        if (Strategy == null || Strategy.IsPaused)
            return;

        ManageStrategy();
    }

    /// <summary>
    /// Manages scouting: updates scout targets and visualizes scout unit.
    /// </summary>
    private void ManageScouting()
    {
        if (ScoutingManager is null)
            return;

        ScoutingManager.UpdateScouting(ScoutingManager.IsScoutingEnabled);

        if (ScoutingManager.ScoutUnit != null)
        {
            Game!.DrawCircleMap(ScoutingManager.ScoutUnit.GetPosition().x, ScoutingManager.ScoutUnit.GetPosition().y, 8, Color.Blue, true);
        }

        // Check if enemy was just scouted
        if (ScoutingManager.PotentialBases != null)
        {
            foreach (var location in ScoutingManager.PotentialBases)
            {
                if (location.EnemyFound && !_enemyFoundLocations.Contains(location.TilePosition))
                {
                    _enemyFoundLocations.Add(location.TilePosition);
                    LogGameEvent("scout", $"Enemy found at location ({location.TilePosition.X}, {location.TilePosition.Y})");
                }
            }
        }
    }

    /// <summary>
    /// Manages construction worker: recalibrates if needed and visualizes worker debug info.
    /// </summary>
    private void ManageConstructionWorker()
    {
        int? id = ConstructionManager.PendingConstructionOrder?.Worker.GetID();
        if (id == null)
            return;

        var worker = MyPlayer?.GetWorkerUnits().FirstOrDefault(u => u.GetID() == id.Value);
        if (worker == null)
            return;

        var position = worker.GetPosition();
        Game!.DrawCircleMap(position.x, position.y, 16, Color.Red, true);
        Game!.DrawTextScreen(100, 90, $"Worker Order: {worker.GetOrder()}");
        Game!.DrawTextScreen(100, 100, $"Target Type: {ConstructionManager.PendingConstructionOrder.BuildingType}");
        Game!.DrawTextScreen(100, 120, $"IsScout: {worker.IsScouting()}");

        // Recalibrate worker if they're not placing a building
        if (worker.GetOrder() != Order.PlaceBuilding)
        {
            ConstructionManager.RecalibrateWorker(new GameData(Game));
        }
    }

    /// <summary>
    /// Manages strategy execution: worker assignment, production, and build orders.
    /// </summary>
    private void ManageStrategy()
    {
        if (Strategy.IdleWorkersSentToGatherMaterials)
        {
            orderIdleUnitsToGatherMaterials();
        }

        // Check if supply is low and insert Supply Depot if needed
        int currentSupply = MyPlayer?.GetSupplyUsed() ?? 0;
        int maxSupply = MyPlayer?.SupplyTotal() ?? 0;
        
        // Log supply blocked event
        if (currentSupply >= maxSupply && !_supplyBlockedLogged)
        {
            _supplyBlockedLogged = true;
            LogGameEvent("supply_blocked", $"Supply blocked at {currentSupply}/{maxSupply}");
        }
        else if (currentSupply < maxSupply)
        {
            _supplyBlockedLogged = false;
        }
        
        Strategy.InsertSupplyDepotIfLow(currentSupply, maxSupply);

        // Manage unit production
        ProductionManager.ConfigTrainType(UnitType.Terran_SCV, MyGame, ConstructionManager, Strategy);
        ProductionManager.ConfigTrainType(UnitType.Terran_Marine, MyGame, ConstructionManager, Strategy);
        ProductionManager.ConfigTrainType(UnitType.Terran_Vulture, MyGame, ConstructionManager, Strategy);
        ProductionManager.DefaultTrainWraith(MyGame, MyPlayer, ConstructionManager);

        // Advance build order if nothing is being constructed
        if (ConstructionManager.PendingConstructionOrder is null)
        {
            TryAdvanceBuildOrder();
        }
    }

    public bool ManageBunkerProduction()
    {
        if (MapManager == null)
        {
            return false;
        }

        var choke = MapManager.GetMainChokepoint();
        if (choke == null) return false;

        var type = UnitType.Terran_Bunker;

        return OnConstructCommand(type, choke.Center.ToTilePosition(), 8, false);
    }

    public bool ManageSupplyDepotProduction()
    {
        if (MapManager == null)
        {
            return false;
        }

        var choke = MapManager.GetMainChokepoint();
        if (choke == null) return false;

        var type = UnitType.Terran_Supply_Depot;

        return OnConstructCommand(type, choke.Center.ToTilePosition(), 8, false);
    }

    public void ToggleStrategy()
    {
        if (Strategy is not null)
        {
            Strategy.IsPaused = !Strategy.IsPaused;
        }
    }

    public void ManageAndRallyOffenseTeam()
    {
        if (MapManager == null || Game == null)
        {
            return;
        }

        _offenseTeamManager.ManageTeam(Game, MyPlayer);
        _logger.LogInformation($"Offense Team Size: {_offenseTeamManager.TeamSize}");
        _offenseTeamManager.RallyAtChokepoint(Game, MapManager);
    }

    /// <summary>
    /// Conditionally command the offense team to attack the enemy base.
    /// </summary>
    public void AttackEnemyBase()
    {
        if (Game is null || MyPlayer is null)
            return;

        _offenseTeamManager.AttackEnemyBase(Game, MyPlayer, PotentialBases);
    }

    public void OnExpand()
    {
        if (Game is null || MyPlayer is null || MapManager is null || Strategy is null)
            return;

        var naturalExpansion = MapManager.GetNaturalExpansion();
        if (naturalExpansion is null)
        {
            return;
        }

        // Store the expansion location
        _expandLocation = naturalExpansion.Value;

        // Create a Command Center build order item and insert it at the current position
        var expandItem = new BuildOrderItem 
        { 
            UnitType = UnitType.Terran_Command_Center, 
            TechType = TechType.None 
        };

        Strategy.InsertBuildOrderItemAtCurrentIndex(expandItem);
        
        // Log expansion event
        LogGameEvent("expansion", $"Expansion started at position ({naturalExpansion.Value.X}, {naturalExpansion.Value.Y})");
    }

    public void ManageOffenseTeam()
    {
        if (Game is null)
            return;

        _offenseTeamManager.ManageTeam(Game, MyPlayer);
    }

    public void SetDefaultStrategy()
    {
        if (Game is null)
            return;
        Strategy = new(new MyGame(new GameData(Game)));
    }

    /// <summary>
    /// Advances the build order by attempting to build the next structure. Will not let you
    /// go past the end of the build order list.
    /// </summary>
    public void TryAdvanceBuildOrder()
    {
        if (Game == null || MyPlayer == null || MyGame == null)
            return;
        if (Strategy is null)
            return;

        var nextBuildItem = Strategy.GetCurrentBuildOrderItem();
        if (nextBuildItem == null)
            return;

        UnitType targetType = nextBuildItem.UnitType;
        TechType techType = nextBuildItem.TechType;
        bool isResearch = techType != TechType.None;

        // Use expansion location for Command Centers, otherwise use strategy's initial position
        TilePosition initialPosition = targetType == UnitType.Terran_Command_Center && _expandLocation.HasValue
            ? _expandLocation.Value
            : Strategy.InitialPosition;

        bool inProgress = isResearch ?
              OnResearchCommand(techType)
            : OnConstructCommand(targetType, initialPosition, Strategy.MaxRange, true);
        if (inProgress)
        {
            _logger.LogInformation($"Build{targetType} in progress...");
            
            if (!(targetType.IsAddon() || isResearch))
            {
                Strategy.SetWorkerAssignedToCurrentStep();
            }
        }
    }

    public bool OnResearchCommand(TechType techType)
    {
        bool researching = MyPlayer.TryResearch(techType, ConstructionManager);
        if (researching)
        {
            Strategy?.CompletedBuildOrderStep();
            LogGameEvent("upgrade", $"Research started: {techType}");
        }
        return researching;
    }

    public bool OnConstructCommand(UnitType targetType, TilePosition desiredPosition, int maxRange, bool isFromBuildOrder)
    {
        if (Game == null || MyPlayer == null || MyGame == null)
            return false;

        if (Strategy is not null && Strategy.WorkerAssignedToCurrentStep)
        {
            return false;
        }

        TilePosition buildLocation = MyGame.GetBuildLocation(targetType, desiredPosition, maxRange);
        var invalidPositionTypes = StaticGameInfo.InvalidPositionTypes();

        if (invalidPositionTypes.Contains(buildLocation))
        {
            return false;
        }
        else
        {

            bool success = MyPlayer.TryConstruct(ConstructionManager, targetType, buildLocation, isFromBuildOrder);
            if (success && targetType.IsAddon() && isFromBuildOrder)
            {
                Strategy?.CompletedBuildOrderStep();
            }
            return success;
        }

    }

    public void SendText(string text)
    {
        if (Game == null)
            return;
        var gameAdapter = new MyGame(new GameData(Game));
        gameAdapter.SendText(text);
    }

    public override void OnUnitComplete(Unit unit) 
    {

    }

    public override void OnUnitDestroy(Unit unit) 
    {
        if (Game == null || MyPlayer == null)
            return;

        bool wasAWorker = unit.GetUnitType().IsWorker();
        bool wasAlly = unit.GetPlayer() == Game.Self();
        bool isInConstructionOrder = ConstructionManager.PendingConstructionOrder?.Worker.GetID() == unit.GetID();

        if (wasAWorker && wasAlly && isInConstructionOrder)
        {
            var workerAdapter = new MyUnit(unit);
            ConstructionManager.RemovePendingConstructionOrder();
        }
    }

    public override void OnUnitMorph(Unit unit)
    {
        if (Game == null || MyPlayer == null)
            return;

        bool isRefinery = unit.GetUnitType() == UnitType.Terran_Refinery;
        bool isInConstruction = unit.IsConstructing();
        bool isAlly = unit.GetPlayer() == Game?.Self();

        if (isRefinery && isAlly && isInConstruction)
        {
            onWorkerHasStartedConstruction(unit);
        }
    }

    public override void OnSendText(string text) { }

    public override void OnReceiveText(Player player, string text) { }

    public override void OnPlayerLeft(Player player) { }

    public override void OnNukeDetect(Position target) { }

    public override void OnUnitEvade(Unit unit) { }

    public override void OnUnitShow(Unit unit) { }

    public override void OnUnitHide(Unit unit) { }

    public override void OnUnitCreate(Unit unit) 
    {
        if (Game == null || MyPlayer == null)
            return;

        bool isABuilding = unit.GetUnitType().IsBuilding();
        bool isInConstruction = unit.IsConstructing();
        bool isAlly = unit.GetPlayer() == Game?.Self();

        if (isABuilding && isAlly && isInConstruction)
        {
            onWorkerHasStartedConstruction(unit);
        }
    }

    private IMyUnit? workerUnitOfThisBuildSite(Unit buildSite)
    {
        if (Game == null || MyPlayer == null)
            return null;

        bool isAssignedWorker(IMyUnit w) => w.GetOrderTarget()?.GetID() == buildSite.GetID();
        return MyPlayer
                .GetWorkerUnits()
                .FirstOrDefault(isAssignedWorker);
    }

    private void onWorkerHasStartedConstruction(Unit buildSite)
    {
        var buildingType = buildSite.GetUnitType();
        if (buildingType.IsAddon())
            return;

        var worker = workerUnitOfThisBuildSite(buildSite);

        if (worker != null)
        {
            var order = ConstructionManager.PendingConstructionOrder;
            if (order is null)
            {
                //SendText($"Player ordered {buildSite.GetUnitType()}");
                return;
            }

            if (order.IsFromBuildOrder)
            {
                Strategy!.CompletedBuildOrderStep();
                _logger.LogInformation($"Build order advanced to index {Strategy.CurrentBuildOrderIndex}");
                ConstructionManager.RemovePendingConstructionOrder();
            }
        }
        else
        {
            throw new NullReferenceException("Worker for construction site not found???");
        }
    }

    private void orderIdleUnitsToGatherMaterials()
    {
        if (Game == null || MyGame == null || Strategy == null)
            return;

        WorkerDispatcher.OrderIdleUnitsToGatherMaterials(MyGame, ConstructionManager, Strategy);
    }

    /// <summary>
    /// Logs a game event to the database asynchronously.
    /// </summary>
    private async void LogGameEvent(string eventType, string description)
    {
        try
        {
            var gameEvent = new GameEvent
            {
                MatchId = Match.Id,
                Timestamp = DateTime.UtcNow,
                EventType = eventType,
                Description = description
            };
            await _gameEventRepository.CreateGameEventAsync(gameEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to log game event: {eventType}");
        }
    }

    public override void OnUnitRenegade(Unit unit) { }

    public override void OnSaveGame(string gameName) { }

    public override void OnUnitDiscover(Unit unit) { }
}
