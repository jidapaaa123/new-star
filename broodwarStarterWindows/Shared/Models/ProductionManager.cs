using BWAPI.NET;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ProductionManager : IProductionManager
    {
        public void ConfigTrainType(UnitType type, IMyGame game, IConstructionManager constructionManager, GameStrategy strategy)
        {
            UnitType productionBuildingType = type.WhatBuilds().First;
            IMyPlayer? player = game.Self();

            var productionBuilding = player?.GetUnits().Where(u => u.GetUnitType() == productionBuildingType).FirstOrDefault();
            if (productionBuilding == null || player is null)
                return;

            bool isTraining = productionBuilding.IsTraining();
            int totalUnits = player.TotalUnitsIncludingInQueue(type);
            bool canAfford = player.EnoughAvailableMaterialsToBuild(UnitType.Terran_SCV, constructionManager);
            int targetConfig = strategy.GetCurrentUnitThresholds()?[type] ?? 0;

            if (canAfford && !isTraining && totalUnits < targetConfig)
            {
                productionBuilding.Train(type);
            }
        }

        public void DefaultTrainWraith(IMyGame game, IMyPlayer player, IConstructionManager constructionManager)
        {
            var starport = player?.GetUnits().FirstOrDefault(u => u.GetUnitType() == UnitType.Terran_Starport);
            if (starport == null || starport.Unit.GetAddon() == null)
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
