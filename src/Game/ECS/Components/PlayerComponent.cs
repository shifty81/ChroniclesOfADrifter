namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Player tag component - marks an entity as the player
/// </summary>
public class PlayerComponent : IComponent
{
    public float Speed { get; set; } = 5.0f;
}
