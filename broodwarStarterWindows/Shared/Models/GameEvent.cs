namespace Shared.Models
{
    public class GameEvent
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty; // "expansion", "upgrade", "scout", "attack", "supply_blocked"
        public string Description { get; set; } = string.Empty;
    }
}
