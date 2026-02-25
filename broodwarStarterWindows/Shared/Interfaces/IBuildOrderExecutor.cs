using Shared.Models;

namespace Shared.Interfaces
{
    public interface IBuildOrderExecutor
    {
        void TryAdvanceBuildOrder(GameStrategy strategy, IMyGame game, IMyPlayer player, IConstructionManager constructionManager);
    }
}
