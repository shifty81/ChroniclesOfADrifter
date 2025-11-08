namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Player tag component - marks an entity as the player
/// 
/// Note: The player character is approximately 2.5 blocks tall (80 pixels at 32px/block).
/// See GameConstants.PlayerHeightInBlocks for the canonical reference value.
/// Use GameConstants.PlayerCollisionWidth and PlayerCollisionHeight for collision setup.
/// </summary>
public class PlayerComponent : IComponent
{
    public float Speed { get; set; } = 5.0f;
}
