using BWAPI.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class BotCommand
    {
        public BotCommandType Type { get; set; }
        public string Text { get; set; }        // For DrawText
        public UnitType TargetUnit { get; set; } // For "Build"
        public TilePosition TilePosition { get; set; } // For "Build"
        public int MaxRange { get; set; } // For "Build"   
    }

    public enum BotCommandType
    {
        ManageBunkerProduction,
        ManageSupplyDepotProduction,
        ToggleStrategy,
        ScoutMap,
        ToggleAttackEnemyBase,
        TogglePauseBot
    }
}
