using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class GameStateDto
    {
        public int WorkerCount { get; set; }
        public int MilitaryCount { get; set; }

        public int Minerals { get; set; }
        public int Gas { get; set; }

        public int SupplyUsed { get; set; }
        public int SupplyTotal { get; set; }

        public Strategy StrategyMode { get; set; } = Strategy.Default;

        public bool HasExpanded { get; set; }
        public bool EnemyScouted { get; set; }
        public bool IsRunning { get; set; }
        public bool InGame { get; set; } = true;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
