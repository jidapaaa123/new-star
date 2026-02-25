using BWAPI.NET;

namespace Shared.Models
{
    public class BuildOrderItem
    {
        public TechType TechType { get; set; } = TechType.None;
        public UnitType UnitType { get; set; } = UnitType.None;
        public Dictionary<UnitType, int> UnitThreshold { get; set; } = new()
        {
            {UnitType.Terran_SCV, 0},
            {UnitType.Terran_Marine, 0},
            {UnitType.Terran_Vulture, 0},
        };

    }
}
