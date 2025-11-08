using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that manages parallax scrolling effects to create depth illusion
/// </summary>
public class ParallaxSystem : ISystem
{
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        var camera = CameraSystem.GetActiveCamera(world);
        if (camera == null)
            return;
        
        // Update all parallax layers
        foreach (var entity in world.GetEntitiesWithComponent<ParallaxLayerComponent>())
        {
            var layer = world.GetComponent<ParallaxLayerComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            
            if (layer == null || position == null || !layer.IsVisible)
                continue;
            
            // Update auto-scroll accumulator
            layer.AccumulatedTime += deltaTime;
            
            // Calculate parallax offset based on camera position
            float parallaxOffsetX = camera.X * layer.ParallaxFactor;
            float parallaxOffsetY = camera.Y * layer.ParallaxFactor;
            
            // Add auto-scroll offset
            float autoScrollOffsetX = layer.AutoScrollX * layer.AccumulatedTime;
            float autoScrollOffsetY = layer.AutoScrollY * layer.AccumulatedTime;
            
            // Set layer position (this is a visual offset, not the entity's actual position)
            // The rendering system should use this to offset the rendering
            position.X = parallaxOffsetX + autoScrollOffsetX;
            position.Y = parallaxOffsetY + autoScrollOffsetY;
        }
    }
    
    /// <summary>
    /// Create a parallax layer entity
    /// </summary>
    public static Entity CreateParallaxLayer(World world, string name, float parallaxFactor, int zOrder, 
        float autoScrollX = 0f, float autoScrollY = 0f)
    {
        var entity = world.CreateEntity();
        
        world.AddComponent(entity, new ParallaxLayerComponent(name, parallaxFactor, zOrder)
        {
            AutoScrollX = autoScrollX,
            AutoScrollY = autoScrollY
        });
        
        world.AddComponent(entity, new PositionComponent());
        
        return entity;
    }
    
    /// <summary>
    /// Create a parallax layer with visual properties
    /// </summary>
    public static Entity CreateParallaxLayer(World world, string name, float parallaxFactor, int zOrder,
        ParallaxVisualType visualType, ConsoleColor color, float density = 0.3f,
        float autoScrollX = 0f, float autoScrollY = 0f)
    {
        var entity = world.CreateEntity();
        
        world.AddComponent(entity, new ParallaxLayerComponent(name, parallaxFactor, zOrder)
        {
            AutoScrollX = autoScrollX,
            AutoScrollY = autoScrollY,
            VisualType = visualType,
            Color = color,
            Density = density
        });
        
        world.AddComponent(entity, new PositionComponent());
        
        return entity;
    }
}
