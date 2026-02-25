using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces;
using Shared.Models;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : Controller
    {
        private readonly IMatchRepository _matchRepository;

        public MatchesController(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Match>>> GetAllMatches()
        {
            try
            {
                var matches = await _matchRepository.GetAllMatchesAsync();
                return Ok(matches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Match>> GetMatchById(int id)
        {
            try
            {
                var match = await _matchRepository.GetMatchByIdAsync(id);
                if (match == null)
                {
                    return NotFound($"Match with ID {id} not found.");
                }
                return Ok(match);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<MatchStatistics>> GetStatistics()
        {
            try
            {
                var statistics = await _matchRepository.GetStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Match>> CreateMatch([FromBody] Match match)
        {
            try
            {
                if (match == null)
                {
                    return BadRequest("Match cannot be null.");
                }

                match.StartTime = DateTime.UtcNow;
                match.Result = "Ongoing";

                var createdMatch = await _matchRepository.CreateMatchAsync(match);
                return CreatedAtAction(nameof(GetMatchById), new { id = createdMatch.Id }, createdMatch);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Match>> UpdateMatch(int id, [FromBody] Match match)
        {
            try
            {
                if (match == null)
                {
                    return BadRequest("Match cannot be null.");
                }

                if (id != match.Id && match.Id != 0)
                {
                    return BadRequest("ID mismatch between route and body.");
                }

                match.Id = id;
                match.EndTime = DateTime.UtcNow;

                var updatedMatch = await _matchRepository.UpdateMatchAsync(id, match);
                return Ok(updatedMatch);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

