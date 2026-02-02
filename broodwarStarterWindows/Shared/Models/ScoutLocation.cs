using BWAPI.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ScoutLocation
    {
        public TilePosition TilePosition { get; set; }
        public bool IsExplored { get; set; }
        public bool EnemyFound { get; set; }

        public ScoutLocation(TilePosition pos) 
        { 
            TilePosition = pos; 
        }
    }
}
