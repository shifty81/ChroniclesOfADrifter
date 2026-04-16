namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Position component - represents 2D position
/// </summary>
public class PositionComponent : IComponent
{
    public float X { get; set; }
    public float Y { get; set; }

    /// <summary>
    /// Whether this entity is active. Inactive entities are skipped by most
    /// systems and used by <see cref="ChroniclesOfADrifter.ECS.Systems.ObjectPoolSystem"/>
    /// to mark pooled entities that are currently not in use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public PositionComponent(float x = 0, float y = 0)
    {
        X = x;
        Y = y;
    }
}
