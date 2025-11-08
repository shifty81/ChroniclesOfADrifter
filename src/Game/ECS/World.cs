namespace ChroniclesOfADrifter.ECS;

/// <summary>
/// Manages entities and their components
/// </summary>
public class World
{
    private int _nextEntityId = 1;
    private readonly Dictionary<int, HashSet<Type>> _entityComponents = new();
    private readonly Dictionary<Type, Dictionary<int, IComponent>> _componentsByType = new();
    private readonly List<ISystem> _systems = new();
    private readonly Dictionary<string, object> _sharedResources = new();
    
    /// <summary>
    /// Create a new entity
    /// </summary>
    public Entity CreateEntity()
    {
        var entity = new Entity(_nextEntityId++);
        _entityComponents[entity.Id] = new HashSet<Type>();
        return entity;
    }
    
    /// <summary>
    /// Destroy an entity and all its components
    /// </summary>
    public void DestroyEntity(Entity entity)
    {
        if (!_entityComponents.ContainsKey(entity.Id))
            return;
        
        // Remove all components
        foreach (var componentType in _entityComponents[entity.Id])
        {
            if (_componentsByType.TryGetValue(componentType, out var components))
            {
                components.Remove(entity.Id);
            }
        }
        
        _entityComponents.Remove(entity.Id);
    }
    
    /// <summary>
    /// Add a component to an entity
    /// </summary>
    public void AddComponent<T>(Entity entity, T component) where T : IComponent
    {
        var type = typeof(T);
        
        if (!_componentsByType.ContainsKey(type))
        {
            _componentsByType[type] = new Dictionary<int, IComponent>();
        }
        
        _componentsByType[type][entity.Id] = component;
        _entityComponents[entity.Id].Add(type);
    }
    
    /// <summary>
    /// Get a component from an entity
    /// </summary>
    public T? GetComponent<T>(Entity entity) where T : class, IComponent
    {
        var type = typeof(T);
        
        if (_componentsByType.TryGetValue(type, out var components))
        {
            if (components.TryGetValue(entity.Id, out var component))
            {
                return component as T;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if an entity has a component
    /// </summary>
    public bool HasComponent<T>(Entity entity) where T : IComponent
    {
        return _entityComponents.TryGetValue(entity.Id, out var components) 
            && components.Contains(typeof(T));
    }
    
    /// <summary>
    /// Remove a component from an entity
    /// </summary>
    public void RemoveComponent<T>(Entity entity) where T : IComponent
    {
        var type = typeof(T);
        
        if (_componentsByType.TryGetValue(type, out var components))
        {
            components.Remove(entity.Id);
        }
        
        if (_entityComponents.TryGetValue(entity.Id, out var entityComps))
        {
            entityComps.Remove(type);
        }
    }
    
    /// <summary>
    /// Get all entities that have a specific component
    /// </summary>
    public IEnumerable<Entity> GetEntitiesWithComponent<T>() where T : IComponent
    {
        var type = typeof(T);
        
        if (_componentsByType.TryGetValue(type, out var components))
        {
            foreach (var entityId in components.Keys)
            {
                yield return new Entity(entityId);
            }
        }
    }
    
    /// <summary>
    /// Add a system to the world
    /// </summary>
    public void AddSystem(ISystem system)
    {
        _systems.Add(system);
        system.Initialize(this);
    }
    
    /// <summary>
    /// Update all systems
    /// </summary>
    public void Update(float deltaTime)
    {
        foreach (var system in _systems)
        {
            system.Update(this, deltaTime);
        }
    }
    
    /// <summary>
    /// Set a shared resource that can be accessed by systems
    /// </summary>
    public void SetSharedResource<T>(string key, T resource) where T : class
    {
        _sharedResources[key] = resource;
    }
    
    /// <summary>
    /// Get a shared resource by key
    /// </summary>
    public T? GetSharedResource<T>(string key) where T : class
    {
        if (_sharedResources.TryGetValue(key, out var resource))
        {
            return resource as T;
        }
        return null;
    }
}
