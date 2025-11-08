using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Movement system - updates position based on velocity
/// </summary>
public class MovementSystem : ISystem
{
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        foreach (var entity in world.GetEntitiesWithComponent<VelocityComponent>())
        {
            var position = world.GetComponent<PositionComponent>(entity);
            var velocity = world.GetComponent<VelocityComponent>(entity);
            
            if (position != null && velocity != null)
            {
                position.X += velocity.VX * deltaTime;
                position.Y += velocity.VY * deltaTime;
                
                // Clamp to world boundaries (matching the 1920x1080 resolution)
                position.X = Math.Clamp(position.X, 0, 1920);
                position.Y = Math.Clamp(position.Y, 0, 1080);
            }
        }
    }
}
