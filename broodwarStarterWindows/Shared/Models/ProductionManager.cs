using BWAPI.NET;
using Shared.Interfaces;
using Shared.MyLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ProductionManager : IProductionManager
    {
        public void ConfigTrainSCV(IMyGame game, IMyPlayer player, IConstructionManager constructionManager, GameStrategy strategy)
        {
            var cc = player?.GetBases().FirstOrDefault();
            if (cc == null)
                return;

            bool isTraining = cc.IsTraining();
            int currentSCVCount = HelperLogic.TotalSCVIncludingInQueue(game, player);
            bool canAfford = player.EnoughAvailableMaterialsToBuild(UnitType.Terran_SCV, constructionManager);

            if (canAfford && !isTraining && currentSCVCount < strategy.SCVConfig)
            {
                cc.Train(UnitType.Terran_SCV);
            }
        }
        public void ConfigTrainMarine(IMyGame game, IMyPlayer player, IConstructionManager constructionManager, GameStrategy strategy)
        {
            var barrack = player?.GetUnits().FirstOrDefault(u => u.GetUnitType() == UnitType.Terran_Barracks);
            if (barrack == null)
                return;

            bool isTraining = barrack.IsTraining();
            int currentMarineCount = HelperLogic.TotalMarinesIncludingInQueue(game, player);
            bool canAfford = player.EnoughAvailableMaterialsToBuild(UnitType.Terran_Marine, constructionManager);

            if (canAfford && !isTraining && currentMarineCount < strategy.MarineConfig)
            {
                barrack.Train(UnitType.Terran_Marine);
            }
        }

        public void ConfigTrainVulture(IMyGame game, IMyPlayer player, IConstructionManager constructionManager, GameStrategy strategy)
        {
            var factory = player?.GetUnits().FirstOrDefault(u => u.GetUnitType() == UnitType.Terran_Factory);
            if (factory == null)
                return;

            bool isTraining = factory.IsTraining();
            int currentCount = HelperLogic.TotalVulturesIncludingInQueue(game, player);
            bool canAfford = player.EnoughAvailableMaterialsToBuild(UnitType.Terran_Vulture, constructionManager);

            if (canAfford && !isTraining && currentCount < strategy.VultureConfig)
            {
                factory.Train(UnitType.Terran_Vulture);
            }
        }

        public void DefaultTrainWraith(IMyGame game, IMyPlayer player, IConstructionManager constructionManager)
        {
            var starport = player?.GetUnits().FirstOrDefault(u => u.GetUnitType() == UnitType.Terran_Starport);
            if (starport == null || starport.UnderlyingUnit.GetAddon() == null)
                return;

            bool isTraining = starport.IsTraining();
            bool canAfford = player.EnoughAvailableMaterialsToBuild(UnitType.Terran_Wraith, constructionManager);

            if (canAfford && !isTraining)
            {
                starport.Train(UnitType.Terran_Wraith);
            }
        }
    }
}
