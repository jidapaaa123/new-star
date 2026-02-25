namespace Shared.Models
{
    public class MatchStatistics
    {
        public int TotalMatches { get; set; }
        public int WonMatches { get; set; }
        public int LostMatches { get; set; }
        public double WinRate { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public double ExpansionRate { get; set; }
        public int TotalUpgradesCompleted { get; set; }
        public double AverageFinalWorkerCount { get; set; }
        public double AverageFinalMilitaryCount { get; set; }
        public double AverageFinalMinerals { get; set; }
        public double AverageFinalGas { get; set; }
    }
}
