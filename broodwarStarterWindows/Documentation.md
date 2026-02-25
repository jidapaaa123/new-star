# StarCraft 2 Bot Assistant - Documentation

## Architecture Overview

The application follows a **multi-layered architecture** with clear separation of concerns across three main projects:

- **MAUI Mobile App**: Cross-platform UI (iOS/Android) that provides real-time dashboard, analytics, and match history views using the MVVM pattern with CommunityToolkit
- **ASP.NET Core Web API**: RESTful backend that manages bot control commands, match data retrieval, and game state monitoring
- **Shared Library**: Contains common models, service interfaces, and view models reused across the mobile and web projects

Data flows from the MAUI app through HTTP requests to the Web API, which interfaces with the bot in Shared. All match history and game statistics are persisted in a SQLite database and accessible through Entity Framework Core repositories.

## Database Schema

### Matches Table

The `Matches` table stores historical data for each game session:

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| **Id** | int | Primary Key, Auto-increment | Unique identifier for each match |
| **StartTime** | DateTime | NOT NULL | UTC timestamp when the match began |
| **EndTime** | DateTime | Nullable | UTC timestamp when the match ended (null if ongoing) |
| **Result** | string | NOT NULL, Default: "Ongoing" | Match outcome: "Win", "Loss", or "Ongoing" |
| **FinalWorkerCount** | int | Default: 0 | Total worker units at end of match |
| **FinalMilitaryCount** | int | Default: 0 | Total military units at end of match |
| **FinalMinerals** | int | Default: 0 | Remaining mineral resources at end |
| **FinalGas** | int | Default: 0 | Remaining gas resources at end |
| **DidExpand** | bool | Default: false | Whether the bot expanded to a second base |
| **UpgradesCompleted** | int | Default: 0 | Count of technology upgrades completed |

**Example Query:**
```sql
SELECT Id, StartTime, EndTime, Result, FinalWorkerCount, FinalMilitaryCount 
FROM Matches 
WHERE Result = 'Win' 
ORDER BY StartTime DESC;
```

**Key Relationships:**
- Each `Match` represents one complete game session  
- Analytics are derived by aggregating across all matches (win rate, expansion rate, average game duration)  
- It's inaccurate because MyStarcraftBot isn't logic-ing anything to feed the correct data. I'll fix this later... but the structure is set up  


# (Event types logged)
### GameEvents Table

The `GameEvents` table logs specific in-game events:

| Column | Type | Description |
|--------|------|-------------|
| **Id** | int | Primary Key |
| **MatchId** | int | Foreign key to `Matches` |
| **Timestamp** | DateTime | When the event occurred |
| **EventType** | string | Type of event: `expansion`, `upgrade`, `scout`, `attack`, `supply_blocked` |
| **Description** | string | Additional event details |

# Build Strategies

The bot supports four distinct build order strategies:

- **Default**: Balanced build emphasizing marines, vultures, and siege tanks. Includes cloaking tech and defensive bunkers.
- **Aggressive**: Early double barracks into marine rush with stim packs. Prioritizes early military pressure over economy.
- **Economic**: Expands to a second base early (Command Center at 14 SCVs). Scales economy before military investment.
- **Defensive**: Heavy emphasis on bunkers and siege tanks. Builds engineering bay for defensive upgrades. Sacrifices aggression for base security.

Each strategy is triggered based on game state.  

# How SignalR is Implemented  
It's a built-in service provided by ASPNET.Core. I made my on GameState Hub that the bot can send messages 
to whenever it wants to update the game state. The GameWorker class is what checks the bot state and
orders the Hub to broadcast things. The MAUI app listens to this hub and updates the UI accordingly. 

## How to run locally
1) Ensuring the bot connects to Starcraft  
- Run ./setup.ps1 inside broodwarStarterWindows  
- copy the path and enter to open ChaosLauncher  
- ChaosLauncher -> "Settings" -> paste the path  
- uncheck Warn about Missing Privileges  
2) Configure the startup project to set both the Web and MauiApp project to Start  
3) ... and it should run. If the database is acting off, check the path of the database in Web/Program.cs (start it up, close it, and then scroll up to the top of the terminal to see the printed path). It should adapt to the local path, though  
