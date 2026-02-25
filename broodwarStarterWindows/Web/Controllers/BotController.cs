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
        private readonly IGameEventRepository _gameEventRepository;
        private readonly IMatchRepository _matchRepository;

        public BotController(MyStarcraftBot myStarcraftBot, IGameEventRepository gameEventRepository, IMatchRepository matchRepository)
        {
            _myStarcraftBot = myStarcraftBot;
            _gameEventRepository = gameEventRepository;
            _matchRepository = matchRepository;
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
        public ActionResult<BotStatusDto> GetBotStatus()
        {
            return Ok(new BotStatusDto
            {
                IsConnected = _myStarcraftBot.IsRunning,
                IsInGame = _myStarcraftBot.InGame,
                CurrentFrame = _myStarcraftBot.Game?.GetFrameCount() ?? 0,
                GameTime = _myStarcraftBot.Game != null ? TimeSpan.FromSeconds(_myStarcraftBot.Game.GetFrameCount() / 24.0) : TimeSpan.Zero
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

            var playerBases = _myStarcraftBot.MyPlayer?.GetBases() ?? new List<IMyUnit>();
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

            var allUnits = _myStarcraftBot.MyPlayer?.GetUnits() ?? new List<IMyUnit>();
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
                IsScouting = _myStarcraftBot.ScoutingManager?.ScoutUnit != null
            });
        }

        [HttpGet("construction")]
        public ActionResult<ConstructionDto> GetConstruction()
        {
            if (_myStarcraftBot.Game == null)
                return StatusCode(503, new { message = "Game not connected" });

            return Ok(new ConstructionDto
            {
                HasWorkerAssigned = _myStarcraftBot.ConstructionManager.PendingConstructionOrder?.Worker != null
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

        [HttpPut("strategy")]
        public ActionResult SetStrategy([FromBody] SetStrategyRequest request)
        {
            if (request?.Strategy == null)
            {
                return BadRequest("Strategy cannot be null");
            }

            _myStarcraftBot.EnqueueCommand(new BotCommand()
            {
                Type = BotCommandType.ChangeStrategy,
                StrategyType = request.Strategy
            });
            return Ok($"Command to change strategy to {request.Strategy} sent to bot.");
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

        [HttpPost("expand")]
        public ActionResult Expand()
        {
            _myStarcraftBot.EnqueueCommand(new BotCommand()
            {
                Type = BotCommandType.Expand,
            });
            return Ok("Command to Expand sent to bot.");
        }

        [HttpGet("matches/{id}/events")]
        public async Task<ActionResult<List<GameEvent>>> GetMatchEvents(int id)
        {
            try
            {
                var events = await _gameEventRepository.GetGameEventsByMatchAsync(id);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving events: {ex.Message}" });
            }
        }

        [HttpGet("matches/latest")]
        public async Task<ActionResult<Match>> GetLatestMatch()
        {
            try
            {
                var allMatches = await _matchRepository.GetAllMatchesAsync();
                var latestMatch = allMatches.OrderByDescending(m => m.StartTime).FirstOrDefault();
                if (latestMatch == null)
                {
                    return NotFound(new { message = "No matches found" });
                }
                return Ok(latestMatch);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving latest match: {ex.Message}" });
            }
        }
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
        public bool HasWorkerAssigned { get; set; }
    }

    public class BotStatusDto
    {
        public bool IsConnected { get; set; }
        public bool IsInGame { get; set; }
        public int CurrentFrame { get; set; }
        public TimeSpan GameTime { get; set; }
    }

    public class SetStrategyRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("strategy")]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public Strategy? Strategy { get; set; }
    }
}
