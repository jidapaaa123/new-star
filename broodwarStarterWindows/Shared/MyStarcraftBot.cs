using BWAPI.NET;
using BWEM.NET;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;
using Shared.Models;
using Shared.MyLogic;
using System.Collections.Concurrent;
using System.Numerics;
using System.Xml.Linq;

namespace Shared;

// library from https://www.nuget.org/packages/BWAPI.NET

public class MyStarcraftBot : DefaultBWListener
{
    private BWClient? _bwClient = null;
    private ILogger<MyStarcraftBot> _logger;

    public Game? Game => _bwClient?.Game;

    public bool IsRunning { get; private set; } = false;
    public bool InGame { get; private set; } = false;
    public int? GameSpeedToSet { get; set; } = null;

    public event Action? StatusChanged;
    public GameStrategy? Strategy { get; set; } = null;
    public GameAdapter? GameAdapter => Game is null ? null : new GameAdapter(Game);
    public PlayerAdapter? PlayerAdapter
    {
        get
        {
            if (Game is null)
                return null;
            Player self = Game.Self();
            return new PlayerAdapter(self);
        }
    }
    public IConstructionManager ConstructionManager { get; } = new ConstructionManager();
    public IProductionManager ProductionManager { get; } = new ProductionManager();
    public MapManager MapManager { get; private set; }
    public List<ScoutLocation>? PotentialBases { get; private set; } = null;

    public MyStarcraftBot(ILogger<MyStarcraftBot> logger)
    {
        _logger = logger;
    }

    private ConcurrentQueue<BotCommand> _pendingCommands = new();
    public IMyUnit? ScoutUnit;
    private int _scoutTargetIndex = 0;
    private bool _scoutEnabled = true;

    private HashSet<Unit> _offenseTeam = new HashSet<Unit>();
    private bool _attackEnemyBaseEnabled = false;
    private bool _botActive = true;

    public void EnqueueCommand(BotCommand command)
    {
        _pendingCommands.Enqueue(command);
        Console.WriteLine($"Command '{command}' queued!"); // This will print from the API call
    }

    public void Connect()
    {
        _bwClient = new BWClient(this);
        var _ = Task.Run(() => _bwClient.StartGame());
        IsRunning = true;
        StatusChanged?.Invoke();
    }

    public void Disconnect()
    {
        if (_bwClient != null)
        {
            (_bwClient as IDisposable)?.Dispose();
        }
        _bwClient = null;
        IsRunning = false;
        InGame = false;
        StatusChanged?.Invoke();
    }

    // Bot Callbacks below
    public override void OnStart()
    {
        InGame = true;
        StatusChanged?.Invoke();
        Game?.EnableFlag(Flag.UserInput); // let human control too

        _logger.LogInformation("JIDAPA : Game Started");

        SetDefaultStrategy();
        SendText($"Hey! The game Began with {Strategy?.Name} Strategy! (from OnStart())");
    }

    public override void OnEnd(bool isWinner)
    {
        InGame = false;
        StatusChanged?.Invoke();
    }

    public override void OnFrame()
    {
        if (Game == null)
            return;
        if (GameSpeedToSet != null)
        {
            Game.SetLocalSpeed(GameSpeedToSet.Value);
            GameSpeedToSet = null;
        }

        if (MapManager is null || !MapManager.IsInitialized)
            MapManager = new(Game);

        TilePosition ccTile = PlayerAdapter?.GetBases().FirstOrDefault()?.GetTilePosition() ?? TilePosition.None;

        Game.DrawTextScreen(100, 130, $"Supply: {PlayerAdapter.GetSupplyUsed()} / {PlayerAdapter.SupplyTotal()}");
        Game.DrawTextScreen(100, 140, $"OtherBases: {PotentialBases?.Count}");
        Game.DrawTextScreen(100, 150, $"ScoutUnitNull?: {ScoutUnit is null}");
        if (ScoutUnit is not null) Game.DrawTextScreen(100, 160, $"ScoutUnitAvailable: {HelperLogic.IsAvailable(ConstructionManager, ScoutUnit)}");
        if (ScoutUnit is not null) Game.DrawTextScreen(100, 170, $"IsScouting: {ScoutUnit.IsScouting()}");
        Game.DrawTextScreen(100, 190, $"Construction Queue: {ConstructionManager.PendingConstructionOrders.Count}");
        Game.DrawTextScreen(100, 200, $"SCV Config: {Strategy?.SCVConfig ?? -999}");



        // Check if the API dropped anything in the mailbox
        while (_pendingCommands.TryDequeue(out var command))
        {
            bool buildOrderIncomplete =
                Strategy is not null &&
                Strategy.CurrentBuildOrderIndex < Strategy.BuildOrderItems.Count;

            if (buildOrderIncomplete && (command.Type == BotCommandType.ManageBunkerProduction
                                        || command.Type == BotCommandType.ManageSupplyDepotProduction))
            {
                SendText("Cannot process ChokepointBuildings command while build order is incomplete.");
                break;
            }

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
                case BotCommandType.ToggleAttackEnemyBase:
                    _attackEnemyBaseEnabled = !_attackEnemyBaseEnabled;
                    break;
                case BotCommandType.ScoutMap:
                    _scoutEnabled = true;
                    SendText("Scouting enabled.");
                    break;
                case BotCommandType.TogglePauseBot:
                    _botActive = !_botActive;
                    break;
            }
        }

        if (!_botActive)
            return;

        if (_scoutEnabled)
            UpdateScouting();

        if (ScoutUnit != null)
        {
            Game.DrawCircleMap(ScoutUnit.GetPosition().x, ScoutUnit.GetPosition().y, 8, Color.Blue, true);
        }

        int? id = ConstructionManager.GetPendingWorkerId();
        if (id != null)
        {
            var worker = PlayerAdapter?.GetWorkerUnits().First(u => u.GetID() == id.Value);
            var position = worker.GetPosition();
            Game.DrawCircleMap(position.x, position.y, 16, Color.Red, true);
            Game.DrawTextScreen(100, 90, $"Worker Order: {worker.GetOrder()}");
            Game.DrawTextScreen(100, 100, $"Worker Target: {worker.GetOrderTarget()}");
            Game.DrawTextScreen(100, 110, $"IsAvailable: {HelperLogic.IsAvailable(ConstructionManager, worker)}");
            Game.DrawTextScreen(100, 120, $"IsScout: {worker.IsScouting()}");


            if (worker.GetOrder() != Order.PlaceBuilding)
            {
                ConstructionManager.RecalibrateWorker();
            }
        }

        if (Strategy == null || Strategy.IsPaused)
            return;


        if (Strategy.IdleWorkersSentToGatherMaterials)
        {
            orderIdleUnitsToGatherMaterials();
        }

        ProductionManager.ConfigTrainSCV(GameAdapter, PlayerAdapter, ConstructionManager, Strategy);
        ProductionManager.ConfigTrainMarine(GameAdapter, PlayerAdapter, ConstructionManager, Strategy);
        ProductionManager.ConfigTrainVulture(GameAdapter, PlayerAdapter, ConstructionManager, Strategy);
        ProductionManager.DefaultTrainWraith(GameAdapter, PlayerAdapter, ConstructionManager);


        if (ConstructionManager.PendingConstructionOrders.Count == 0)
        {
            TryAdvanceBuildOrder();
        }

        if (_attackEnemyBaseEnabled)
        {
            AttackEnemyBase();
        }
        else
        {
            ManageAndRallyOffenseTeam();
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
        if (MapManager == null)
        {
            return;
        }

        ManageOffenseTeam();
        _logger.LogInformation($"Offense Team Size: {_offenseTeam.Count}");
        ChokePoint? choke = MapManager.GetMainChokepoint();
        if (choke is null)
        {
            _logger.LogInformation($"Offense Team Size: {_offenseTeam.Count}");
            return;
        }

        Position rallyPoint = choke.Center.ToPosition();
        var cc = PlayerAdapter?.GetBases()[0];

        foreach (var u in _offenseTeam) 
        {
            if (u.IsSelected())
            {
                continue;
            }
            if (u.GetDistance(rallyPoint) > 32) // Only move if not already there
            {
                u.Attack(rallyPoint);
                // Draw a line from the unit to where it's TRYING to go
                Game.DrawLineMap(u.GetPosition(), rallyPoint, Color.Green);
            }
        }
    }

    /// <summary>
    /// Conditionally... of course
    /// </summary>
    public void AttackEnemyBase()
    {
        if (Game is null || PlayerAdapter is null)
            return;

        if (PotentialBases is null)
        {
            SendText("No potential bases known.");
            return;
        }

        ScoutLocation? enemyBase = PotentialBases.FirstOrDefault(b => b.EnemyFound);

        if (enemyBase is null)
        {
            SendText("No enemy base found during scouting yet.");
            return;
        }

        var team = _offenseTeam;
        // "A-Move" to the enemy base
        foreach (var u in team)
        {
            u.Attack(enemyBase.TilePosition.ToPosition());
        }

        var enemies = Game.Enemy().GetUnits();
        if (enemies.Any())
        {
            foreach (var u in team)
            {
                if (u.CanCloak())
                {
                    u.Cloak();
                }
                u.Attack(enemies[0]);
            }
        }

        if (team.Any(u => u.GetHitPoints() < u.GetHitPoints() * 0.5))
        {
            _attackEnemyBaseEnabled = false;
            foreach (var u in team)
            {
                u.Attack(Game.Self().GetStartLocation().ToPosition());
            }

            return;
        }
        // Threshold: 15 energy (enough to stay cloaked for a few more seconds)
        int energyThreshold = 15;

        if (team.Any(u => u.IsCloaked() && u.GetEnergy() < energyThreshold))
        {
            _attackEnemyBaseEnabled = false;
            foreach (var u in team)
            {
                if (u.CanCloak())
                {
                    u.Decloak();
                }
                u.Attack(Game.Self().GetStartLocation().ToPosition());
            }
        }
    }

    public void ManageOffenseTeam()
    {
        if (Game is null)
            return;

        // Clean up dead units
        _offenseTeam.RemoveWhere(u => !u.Exists() || u.GetHitPoints() <= 0);

        // Recruit units: ALL wraiths, ALL vultures, keep marines at home for now?
        // Should put this in config with Strategy... should I be clean or should I be fast?
        // It's 4 hours before it's due, I think I'll be fast
        // The Game
        var allWraiths = Game.Self().GetUnits()
            .Where(u => u.GetUnitType() == UnitType.Terran_Wraith);
        var allVultures = Game.Self().GetUnits()
            .Where(u => u.GetUnitType() == UnitType.Terran_Vulture);
        var allMarines = Game.Self().GetUnits()
            .Where(u => u.GetUnitType() == UnitType.Terran_Marine);

        foreach (var w in allWraiths) _offenseTeam.Add(w);
        foreach (var v in allVultures) _offenseTeam.Add(v);
        foreach (var m in allMarines) _offenseTeam.Add(m);
    }

    public void UpdateScouting()
    {
        if (Game is null || PlayerAdapter is null)
            return;

        if (PotentialBases is null)
            PotentialBases = MapManager.GetScoutingTargets().Select(p => new ScoutLocation(p)).ToList();

        if (!tryEnsureScoutUnitIsSet(PlayerAdapter)) 
            return;

        if (Game.Enemy().GetUnits().Any())
        {
            PotentialBases[_scoutTargetIndex].IsExplored = true;
            PotentialBases[_scoutTargetIndex].EnemyFound= true;

            proceedToNextScoutingLocation(PotentialBases);
            return;
        }

        Position targetPosition = PotentialBases[_scoutTargetIndex].TilePosition.ToPosition();
        bool reachedTarget = ScoutUnit.GetDistance(targetPosition) <= 200;

        if (!reachedTarget)
        {
            ScoutUnit.Move(targetPosition);
        }
        else
        {
            PotentialBases[_scoutTargetIndex].IsExplored = true;
            PotentialBases[_scoutTargetIndex].EnemyFound = false;

            proceedToNextScoutingLocation(PotentialBases);
        }
    }

    private bool tryEnsureScoutUnitIsSet(IMyPlayer player)
    {
        if (ScoutUnit == null)
        {
            ScoutUnit = player.GetWorkerUnits().FirstOrDefault(u => HelperLogic.IsAvailable(ConstructionManager, u));
            ScoutUnit?.SetScouting();
        }
        
        return ScoutUnit != null;
    }

    private void proceedToNextScoutingLocation(List<ScoutLocation> locations)
    {
        if (Game is null || ScoutUnit is null)
            return;

        int nextIndex = locations.FindIndex(s => !s.IsExplored);
        bool notFound = nextIndex == -1;

        if (notFound)
        {
            SendText("Scouting complete. I better get going home!");
            Position home = Game.Self().GetStartLocation().ToPosition();
            ScoutUnit.Move(home);
            ScoutUnit.UnsetScouting();
            ScoutUnit = null;
            _scoutEnabled = false;
        }
        else
        {
            _scoutTargetIndex = nextIndex;
        }
    }

    public void SetDefaultStrategy()
    {
        if (Game is null)
            return;
        Strategy = new(new GameAdapter(Game));
    }

    /// <summary>
    /// Advances the build order by attempting to build the next structure. Will not let you
    /// go past the end of the build order list.
    /// </summary>
    public void TryAdvanceBuildOrder()
    {
        if (Game == null || PlayerAdapter == null || GameAdapter == null)
            return;
        if (Strategy is null)
            return;

        int nextIndex = Strategy.CurrentBuildOrderIndex;
        var buildOrder = Strategy.BuildOrderItems;
        if (nextIndex >= buildOrder.Count)
            return;

        var nextBuildItem = buildOrder[nextIndex];
        UnitType targetType = nextBuildItem.UnitType;

        bool inProgress = OnConstructCommand(targetType, Strategy.InitialPosition, Strategy.MaxRange, true);
        if (inProgress)
        {
            _logger.LogInformation($"Build{targetType} in progress...");
            
            if (!targetType.IsAddon())
            {
                Strategy.SetWorkerAssignedToCurrentStep();
            }
        }
    }


    public bool OnConstructCommand(UnitType targetType, TilePosition desiredPosition, int maxRange, bool isFromBuildOrder)
    {
        _logger.LogInformation($"Command to Build{targetType}");
        if (Game == null || PlayerAdapter == null || GameAdapter == null)
            return false;

        if (Strategy is not null && Strategy.WorkerAssignedToCurrentStep)
        {
            return false;
        }

        TilePosition buildLocation = GameAdapter.GetBuildLocation(targetType, desiredPosition, maxRange);
        var invalidPositionTypes = HelperLogic.InvalidPositionTypes();

        if (invalidPositionTypes.Contains(buildLocation))
        {
            return false;
        }
        else
        {

            bool success = PlayerAdapter.TryConstruct(ConstructionManager, targetType, buildLocation, isFromBuildOrder);
            if (success && targetType.IsAddon() && isFromBuildOrder)
            {
                Strategy?.CompletedBuildOrderStep();
            }
            return success;
        }

    }

    public void SendText(string text)
    {
        _logger.LogInformation("SendText called");
        if (Game == null)
            return;
        var gameAdapter = new GameAdapter(Game);
        gameAdapter.SendText(text);
    }

    public override void OnUnitComplete(Unit unit) 
    {

    }

    public override void OnUnitDestroy(Unit unit) 
    {
        if (Game == null || PlayerAdapter == null)
            return;

        bool wasAWorker = unit.GetUnitType().IsWorker();
        bool wasAlly = unit.GetPlayer() == Game.Self();
        bool wasInConstructionOrder = ConstructionManager.PendingConstructionOrders.Any(o => o.Worker?.GetID() == unit.GetID());

        if (wasAWorker && wasAlly && wasInConstructionOrder)
        {
            var workerAdapter = new UnitAdapter(unit);
            ConstructionManager.RemoveWorkerConstructionOrder(workerAdapter);
        }
    }

    public override void OnUnitMorph(Unit unit)
    {
        if (Game == null || PlayerAdapter == null)
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
        if (Game == null || PlayerAdapter == null)
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
        if (Game == null || PlayerAdapter == null)
            return null;

        bool isAssignedWorker(IMyUnit w) => w.GetOrderTarget()?.GetID() == buildSite.GetID();
        return PlayerAdapter
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
            var order = ConstructionManager.OrderOfWorker(worker);
            if (order is null)
            {
                SendText($"Player ordered {buildSite.GetUnitType()}");
                return;
            }

            if (order.IsFromBuildOrder)
            {
                Strategy!.CompletedBuildOrderStep();
                _logger.LogInformation($"Build order advanced to index {Strategy.CurrentBuildOrderIndex}");
                ConstructionManager.RemoveWorkerConstructionOrder(worker);
            }
        }
        else
        {
            throw new NullReferenceException("Assigned worker for construction not found???");
        }
    }

    private void orderIdleUnitsToGatherMaterials()
    {
        if (Game == null || PlayerAdapter == null || GameAdapter == null)
            return;


        var bases = PlayerAdapter.GetBases();
        var nearestMineral = GameAdapter.ClosestInstanceOfTo(GameAdapter.GetMinerals(), bases[0]);
        var availableWorkers = PlayerAdapter.GetWorkerUnits().Where(u => HelperLogic.IsAvailable(ConstructionManager, u)).ToList();

        if (availableWorkers.Count == 0)
            return;

        PlayerAdapter.SendTheseWorkersToGatherAt(ConstructionManager, availableWorkers, nearestMineral);

        IMyUnit? refinery = PlayerAdapter.GetUnits().Where(u => u.GetUnitType().IsRefinery() && !u.IsConstructing()).FirstOrDefault() ?? null;

        bool hasRefinery = refinery != null;
        int gasConfig = Strategy?.GasGatherConfig ?? HelperLogic.GasGatherConfigDefault;
        int gasWorkersAmt = hasRefinery ? gasConfig : 0;
        int mineralWorkersAmt = availableWorkers.Count - gasWorkersAmt;

        List<IMyUnit> gasWorkers = availableWorkers.GetRange(0, gasWorkersAmt);
        List<IMyUnit> mineralWorkers = availableWorkers.GetRange(gasWorkersAmt, mineralWorkersAmt);

        if (hasRefinery)
        {
            PlayerAdapter.SendTheseWorkersToGatherAt(ConstructionManager, gasWorkers, refinery!);
        }
    }

    public override void OnUnitRenegade(Unit unit) { }

    public override void OnSaveGame(string gameName) { }

    public override void OnUnitDiscover(Unit unit) { }
}
