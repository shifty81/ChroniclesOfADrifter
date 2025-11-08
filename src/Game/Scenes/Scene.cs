using ChroniclesOfADrifter.ECS;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Base class for all game scenes
/// </summary>
public abstract class Scene
{
    public World World { get; private set; }
    
    public Scene()
    {
        World = new World();
    }
    
    /// <summary>
    /// Called when the scene is loaded
    /// </summary>
    public abstract void OnLoad();
    
    /// <summary>
    /// Called when the scene is unloaded
    /// </summary>
    public abstract void OnUnload();
    
    /// <summary>
    /// Update the scene
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        World.Update(deltaTime);
    }
}
