using BWAPI.NET;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IProductionManager
    {
        void ConfigTrainType(UnitType type, IMyGame game, IConstructionManager constructionManager, GameStrategy strategy);

        //void ConfigTrainSCV(IMyGame game, IMyPlayer player, IConstructionManager constructionManager, GameStrategy strategy);
        //void ConfigTrainMarine(IMyGame gameAdapter, IMyPlayer playerAdapter, IConstructionManager constructionManager, GameStrategy strategy);
        //void ConfigTrainVulture(IMyGame gameAdapter, IMyPlayer playerAdapter, IConstructionManager constructionManager, GameStrategy strategy);
        void DefaultTrainWraith(IMyGame game, IMyPlayer player, IConstructionManager constructionManager);
    }
}
