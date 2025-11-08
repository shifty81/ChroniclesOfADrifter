namespace ChroniclesOfADrifter.ECS;

/// <summary>
/// Base interface for all systems
/// Systems contain logic that operates on entities with specific components
/// </summary>
public interface ISystem
{
    /// <summary>
    /// Called once when the system is added to the world
    /// </summary>
    void Initialize(World world);
    
    /// <summary>
    /// Called every frame to update the system
    /// </summary>
    void Update(World world, float deltaTime);
}
