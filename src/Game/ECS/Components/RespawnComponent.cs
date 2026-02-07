namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component for handling entity respawn logic and death penalties
/// </summary>
public class RespawnComponent : IComponent
{
    /// <summary>
    /// X coordinate of respawn point
    /// </summary>
    public float RespawnX { get; set; }
    
    /// <summary>
    /// Y coordinate of respawn point
    /// </summary>
    public float RespawnY { get; set; }
    
    /// <summary>
    /// Death penalty as percentage (0-100) applied to gold
    /// </summary>
    public float DeathPenaltyPercent { get; set; }
    
    /// <summary>
    /// Total number of deaths for statistics
    /// </summary>
    public int DeathCount { get; set; }
    
    /// <summary>
    /// Is the entity currently dead
    /// </summary>
    public bool IsDead { get; set; }
    
    /// <summary>
    /// Time remaining until respawn
    /// </summary>
    public float RespawnTimer { get; set; }
    
    /// <summary>
    /// Duration of invulnerability after respawn (seconds)
    /// </summary>
    public float InvulnerabilityDuration { get; set; }
    
    /// <summary>
    /// Time remaining for invulnerability
    /// </summary>
    public float InvulnerabilityTimer { get; set; }
    
    /// <summary>
    /// Is the entity currently invulnerable
    /// </summary>
    public bool IsInvulnerable => InvulnerabilityTimer > 0;
    
    public RespawnComponent(float respawnX, float respawnY, float deathPenaltyPercent = 10f, float invulnerabilityDuration = 2f)
    {
        RespawnX = respawnX;
        RespawnY = respawnY;
        DeathPenaltyPercent = Math.Clamp(deathPenaltyPercent, 0f, 100f);
        DeathCount = 0;
        IsDead = false;
        RespawnTimer = 0f;
        InvulnerabilityDuration = invulnerabilityDuration;
        InvulnerabilityTimer = 0f;
    }
    
    /// <summary>
    /// Update respawn point to a new location
    /// </summary>
    public void SetRespawnPoint(float x, float y)
    {
        RespawnX = x;
        RespawnY = y;
    }
}
