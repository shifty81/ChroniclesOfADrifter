namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component for spawn points and spawnable areas
/// Defines where and what creatures can spawn
/// </summary>
public class SpawnPointComponent : IComponent
{
    public CreatureType CreatureType { get; set; }
    public float SpawnRadius { get; set; }
    public int MaxCreatures { get; set; }
    public float RespawnTime { get; set; }  // Time in seconds before respawn
    public float TimeSinceLastSpawn { get; set; }
    public int CurrentCreatureCount { get; set; }
    
    public SpawnPointComponent(CreatureType creatureType, float radius = 500f, int maxCreatures = 3, float respawnTime = 30f)
    {
        CreatureType = creatureType;
        SpawnRadius = radius;
        MaxCreatures = maxCreatures;
        RespawnTime = respawnTime;
        TimeSinceLastSpawn = respawnTime; // Ready to spawn immediately
        CurrentCreatureCount = 0;
    }
}
