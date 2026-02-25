using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Match
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int FinalWorkerCount { get; set; }
        public int FinalMilitaryCount { get; set; }
        public int FinalMinerals { get; set; }
        public int FinalGas { get; set; }
        public bool DidExpand { get; set; }
        public int UpgradesCompleted { get; set; }
        public string Result { get; set; } // "Win", "Loss", "Ongoing"
    }
}
