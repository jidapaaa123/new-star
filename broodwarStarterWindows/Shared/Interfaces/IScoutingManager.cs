using BWAPI.NET;
using Shared.Models;
using Shared.Wrappers;

namespace Shared.Interfaces
{
    public interface IScoutingManager
    {
        MyGame? Game { get; }
        IMyUnit? ScoutUnit { get; }
        List<ScoutLocation>? PotentialBases { get; }
        bool IsScoutingEnabled { get; }
        void UpdateScouting(bool scoutingFlag);
    }
}
