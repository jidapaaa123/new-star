using BWAPI.NET;
using Shared.Models;

namespace Shared.Interfaces
{
    public interface IOffenseManager
    {
        int OffenseTeamSize { get; }
        bool AttackEnemyBaseEnabled { get; set; }
        void ManageAndRallyTeam(IMyPlayer player, Game game, MapManager mapManager);
        void AttackEnemyBase(IMyPlayer player, Game game, List<ScoutLocation>? potentialBases);
    }
}
