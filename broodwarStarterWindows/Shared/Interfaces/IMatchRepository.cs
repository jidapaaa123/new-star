using Shared.Models;

namespace Shared.Interfaces
{
    public interface IMatchRepository
    {
        /// <summary>
        /// GET /api/matches - Retrieve all historical matches
        /// </summary>
        Task<IEnumerable<Match>> GetAllMatchesAsync();

        /// <summary>
        /// GET /api/matches/{id} - Retrieve a specific match by ID
        /// </summary>
        Task<Match?> GetMatchByIdAsync(int id);

        /// <summary>
        /// GET /api/matches/statistics - Retrieve aggregated match statistics
        /// </summary>
        Task<MatchStatistics> GetStatisticsAsync();

        /// <summary>
        /// POST /api/matches - Create and start a new match record
        /// </summary>
        Task<Match> CreateMatchAsync(Match match);

        /// <summary>
        /// PUT /api/matches/{id} - Update an existing match (typically when it ends)
        /// </summary>
        Task<Match> UpdateMatchAsync(int id, Match match);
    }
}
