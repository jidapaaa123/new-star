using BWAPI.NET;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Interfaces;
using Shared.Models;
using Web.Services;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotController : Controller
    {
        private readonly MyStarcraftBot _myStarcraftBot;

        public BotController(MyStarcraftBot myStarcraftBot)
        {
            _myStarcraftBot = myStarcraftBot;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            return "Hello World!";
        }
        
        [HttpGet("bye")]
        public ActionResult<string> Bye()
        {
            return "Bye World!";
        }

        [HttpGet("status")]
        public ActionResult<GameStatusDto> GetGameStatus()
        {
            if (_myStarcraftBot.Game == null)
                return StatusCode(503, new { message = "Game not connected" });

            return Ok(new GameStatusDto
            {
                IsRunning = _myStarcraftBot.IsRunning,
                InGame = _myStarcraftBot.InGame,
                Supply = _myStarcraftBot.PlayerAdapter?.GetSupplyUsed() ?? 0,
                SupplyTotal = _myStarcraftBot.PlayerAdapter?.SupplyTotal() ?? 0,
                Minerals = _myStarcraftBot.PlayerAdapter?.Minerals() ?? 0,
                Gas = _myStarcraftBot.PlayerAdapter?.Gas() ?? 0,
                Workers = _myStarcraftBot.PlayerAdapter?.GetWorkerUnits().Count ?? 0
            });
        }

        [HttpGet("strategy")]
        public ActionResult<StrategyDto> GetStrategy()
        {
            if (_myStarcraftBot.Game == null)
                return StatusCode(503, new { message = "Game not connected" });

            if (_myStarcraftBot.Strategy == null)
                return Ok(new StrategyDto { Name = "None", IsPaused = false, BuildOrderIndex = 0, BuildOrderCount = 0 });

            return Ok(new StrategyDto
            {
                Name = _myStarcraftBot.Strategy.Name,
                IsPaused = _myStarcraftBot.Strategy.IsPaused,
                BuildOrderIndex = _myStarcraftBot.Strategy.CurrentBuildOrderIndex,
                BuildOrderCount = _myStarcraftBot.Strategy.BuildOrderItems.Count
            });
        }

        [HttpGet("bases")]
        public ActionResult<BasesDto> GetBases()
        {
            if (_myStarcraftBot.Game == null)
                return StatusCode(503, new { message = "Game not connected" });

            var playerBases = _myStarcraftBot.PlayerAdapter?.GetBases() ?? new List<IMyUnit>();
            var potentialBases = _myStarcraftBot.PotentialBases ?? new List<ScoutLocation>();

            return Ok(new BasesDto
            {
                PlayerBases = playerBases.Count,
                ExploredBases = potentialBases.Count(b => b.IsExplored),
                EnemyBasesFound = potentialBases.Count(b => b.EnemyFound),
                TotalPotentialBases = potentialBases.Count
            });
        }

        [HttpGet("units")]
        public ActionResult<UnitsDto> GetUnits()
        {
            if (_myStarcraftBot.Game == null)
                return StatusCode(503, new { message = "Game not connected" });

            var allUnits = _myStarcraftBot.PlayerAdapter?.GetUnits() ?? new List<IMyUnit>();
            var marines = allUnits.Count(u => u.GetUnitType() == UnitType.Terran_Marine);
            var vultures = allUnits.Count(u => u.GetUnitType() == UnitType.Terran_Vulture);
            var wraiths = allUnits.Count(u => u.GetUnitType() == UnitType.Terran_Wraith);
            var scvs = allUnits.Count(u => u.GetUnitType().IsWorker());

            return Ok(new UnitsDto
            {
                Total = allUnits.Count(),
                Marines = marines,
                Vultures = vultures,
                Wraiths = wraiths,
                SCVs = scvs,
                IsScouting = _myStarcraftBot.ScoutUnit != null
            });
        }

        [HttpGet("construction")]
        public ActionResult<ConstructionDto> GetConstruction()
        {
            if (_myStarcraftBot.Game == null)
                return StatusCode(503, new { message = "Game not connected" });

            return Ok(new ConstructionDto
            {
                PendingOrders = _myStarcraftBot.ConstructionManager.PendingConstructionOrders.Count,
                HasWorkerAssigned = _myStarcraftBot.ConstructionManager.GetPendingWorkerId() != null
            });
        }
    

        [HttpPost("chokebunker")]
        public ActionResult ManageBunkerProduction()
        {
            _myStarcraftBot.EnqueueCommand(new BotCommand()
            {
                Type = BotCommandType.ManageBunkerProduction,
            });
            return Ok("Command to ManageBunkerProduction sent to bot.");
        }

        [HttpPost("chokedepot")]
        public ActionResult ManageSupplyDepotProduction()
        {
            _myStarcraftBot.EnqueueCommand(new BotCommand()
            {
                Type = BotCommandType.ManageSupplyDepotProduction,
            });
            return Ok("Command to ManageSupplyDepotProduction sent to bot.");
        }

        [HttpPost("togglestrat")]
        public ActionResult ToggleStrategy()
        {
            _myStarcraftBot.EnqueueCommand(new BotCommand()
            {
                Type = BotCommandType.ToggleStrategy,
            });
            return Ok("Command to ToggleStrategy sent to bot.");
        }

        [HttpPost("toggleattackenemybase")]
        public ActionResult AttackEnemyBase()
        {
            _myStarcraftBot.EnqueueCommand(new BotCommand()
            {
                Type = BotCommandType.ToggleAttackEnemyBase,
            });
            return Ok("Command to AttackEnemyBase sent to bot.");
        }

        [HttpPost("scoutmap")]
        public ActionResult ScoutMap()
        {
            _myStarcraftBot.EnqueueCommand(new BotCommand()
            {
                Type = BotCommandType.ScoutMap,
            });
            return Ok("Command to ScoutMap sent to bot.");
        }

        [HttpPost("togglepausebot")]
        public ActionResult TogglePauseBot()
        {
            _myStarcraftBot.EnqueueCommand(new BotCommand()
            {
                Type = BotCommandType.TogglePauseBot,
            });
            return Ok("Command to TogglePauseBot sent to bot.");
        }
    }

    public class GameStatusDto
    {
        public bool IsRunning { get; set; }
        public bool InGame { get; set; }
        public int Supply { get; set; }
        public int SupplyTotal { get; set; }
        public int Minerals { get; set; }
        public int Gas { get; set; }
        public int Workers { get; set; }
    }

    public class StrategyDto
    {
        public string Name { get; set; }
        public bool IsPaused { get; set; }
        public int BuildOrderIndex { get; set; }
        public int BuildOrderCount { get; set; }
    }

    public class BasesDto
    {
        public int PlayerBases { get; set; }
        public int ExploredBases { get; set; }
        public int EnemyBasesFound { get; set; }
        public int TotalPotentialBases { get; set; }
    }

    public class UnitsDto
    {
        public int Total { get; set; }
        public int Marines { get; set; }
        public int Vultures { get; set; }
        public int Wraiths { get; set; }
        public int SCVs { get; set; }
        public bool IsScouting { get; set; }
    }

    public class ConstructionDto
    {
        public int PendingOrders { get; set; }
        public bool HasWorkerAssigned { get; set; }
    }
}
