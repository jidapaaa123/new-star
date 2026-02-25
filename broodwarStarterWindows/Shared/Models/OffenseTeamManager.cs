using BWAPI.NET;
using BWEM.NET;
using Microsoft.Extensions.Logging;
using Shared.DataAdapters;
using Shared.Wrappers;

namespace Shared.Models;

public class OffenseTeamManager
{
    // Constants extracted from magic numbers
    private const int RALLY_DISTANCE = 32;
    private const double HEALTH_RETREAT_THRESHOLD = 0.5;
    private const int CLOAK_ENERGY_THRESHOLD = 15;

    // State
    private HashSet<Unit> _team = new();
    private bool _attackEnemyBaseEnabled = false;
    private readonly ILogger<OffenseTeamManager>? _logger;

    public bool IsAttackingEnemyBase => _attackEnemyBaseEnabled;
    public int TeamSize => _team.Count;

    public OffenseTeamManager(ILogger<OffenseTeamManager>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Toggle whether the offense team should attack the enemy base or not.
    /// </summary>
    public void ToggleAttackMode()
    {
        _attackEnemyBaseEnabled = !_attackEnemyBaseEnabled;
    }

    /// <summary>
    /// Manage the offense team: clean up dead units and recruit new units from the game.
    /// </summary>
    public void ManageTeam(Game game, MyPlayer? myPlayer)
    {
        if (game is null)
            return;

        CleanUpDeadUnits();
        RecruitUnitsFromGame(game);
    }

    /// <summary>
    /// Rally the offense team at a chokepoint for defensive positioning.
    /// </summary>
    public void RallyAtChokepoint(Game game, MapManager? mapManager)
    {
        if (mapManager is null || game is null)
            return;

        ChokePoint? choke = mapManager.GetMainChokepoint();
        if (choke == null)
        {
            _logger?.LogInformation($"Offense Team Size: {TeamSize}");
            return;
        }

        Position rallyPoint = choke.Center.ToPosition();

        foreach (var u in _team)
        {
            if (u.IsSelected())
            {
                continue;
            }
            if (u.GetDistance(rallyPoint) > RALLY_DISTANCE) // Only move if not already there
            {
                u.Attack(rallyPoint);
                // Draw a line from the unit to where it's TRYING to go
                game.DrawLineMap(u.GetPosition(), rallyPoint, Color.Green);
            }
        }
    }

    /// <summary>
    /// Command the offense team to attack the enemy base. Handles movement, cloaking, and retreat logic.
    /// </summary>
    public void AttackEnemyBase(Game game, MyPlayer? myPlayer, List<ScoutLocation>? potentialBases)
    {
        if (game is null)
            return;

        if (potentialBases is null || potentialBases.Count == 0)
            return;

        ScoutLocation? enemyBase = FindEnemyBase(potentialBases);
        if (enemyBase is null)
            return;

        // Move team to enemy base
        ScoutLocation? enemyBaseLocation = FindEnemyBase(potentialBases);
        if (enemyBaseLocation is not null)
        {
            MoveTeamToEnemyBase(game, enemyBaseLocation);
        }

        // Attack visible enemies
        var enemies = game.Enemy().GetUnits();
        if (enemies.Any())
        {
            AttackVisibleEnemies(enemies);
        }

        // Check retreat conditions
        if (ShouldRetreatDueToHealth())
        {
            if (myPlayer is not null)
                RetireTeamToBase(game, myPlayer);
            _attackEnemyBaseEnabled = false;
            return;
        }

        if (ShouldRetreatDueToCloakingEnergy())
        {
            if (myPlayer is not null)
                RetireTeamToBase(game, myPlayer);
            _attackEnemyBaseEnabled = false;
        }
    }

    // ========== Private Helper Methods ==========

    private void CleanUpDeadUnits()
    {
        _team.RemoveWhere(u => !u.Exists() || u.GetHitPoints() <= 0);
    }

    private void RecruitUnitsFromGame(Game game)
    {
        var allWraiths = game.Self().GetUnits()
            .Where(u => u.GetUnitType() == UnitType.Terran_Wraith);
        var allVultures = game.Self().GetUnits()
            .Where(u => u.GetUnitType() == UnitType.Terran_Vulture);
        var allMarines = game.Self().GetUnits()
            .Where(u => u.GetUnitType() == UnitType.Terran_Marine);

        foreach (var w in allWraiths) _team.Add(w);
        foreach (var v in allVultures) _team.Add(v);
        foreach (var m in allMarines) _team.Add(m);
    }

    private ScoutLocation? FindEnemyBase(List<ScoutLocation> potentialBases)
    {
        return potentialBases.FirstOrDefault(b => b.EnemyFound);
    }

    private void MoveTeamToEnemyBase(Game game, ScoutLocation enemyBase)
    {
        foreach (var u in _team)
        {
            u.Attack(enemyBase.TilePosition.ToPosition());
        }
    }

    private void AttackVisibleEnemies(IEnumerable<Unit> enemies)
    {
        foreach (var u in _team)
        {
            if (u.CanCloak())
            {
                u.Cloak();
            }
            u.Attack(enemies.First());
        }
    }

    private bool ShouldRetreatDueToHealth()
    {
        return _team.Any(u => u.GetHitPoints() < u.GetHitPoints() * HEALTH_RETREAT_THRESHOLD);
    }

    private bool ShouldRetreatDueToCloakingEnergy()
    {
        return _team.Any(u => u.IsCloaked() && u.GetEnergy() < CLOAK_ENERGY_THRESHOLD);
    }

    private void RetireTeamToBase(Game game, MyPlayer myPlayer)
    {
        Position baseLocation = game.Self().GetStartLocation().ToPosition();
        foreach (var u in _team)
        {
            if (u.CanCloak())
            {
                u.Decloak();
            }
            u.Attack(baseLocation);
        }
    }
}
