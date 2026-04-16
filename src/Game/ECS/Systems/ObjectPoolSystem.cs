using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Lightweight object pool for a single entity component type.
/// Entities are created up-front, marked inactive, and recycled on release.
/// </summary>
public class EntityPool
{
    private readonly World _world;
    private readonly Queue<Entity> _available;
    private readonly HashSet<Entity> _active;
    private readonly string _poolName;
    private int _totalCreated;

    public string Name => _poolName;
    public int ActiveCount => _active.Count;
    public int AvailableCount => _available.Count;
    public int TotalCapacity => _totalCreated;

    public EntityPool(World world, string name, int initialCapacity = 32)
    {
        _world = world;
        _poolName = name;
        _available = new Queue<Entity>(initialCapacity);
        _active = new HashSet<Entity>();
        _totalCreated = 0;

        Expand(initialCapacity);
    }

    /// <summary>
    /// Acquire an entity from the pool. Returns null if the pool is exhausted
    /// and cannot expand.
    /// </summary>
    public Entity? Acquire()
    {
        if (_available.Count == 0)
        {
            // Expand by 50 % of current capacity (at least 8)
            int growth = Math.Max(8, _totalCreated / 2);
            Expand(growth);
        }

        var entity = _available.Dequeue();
        _active.Add(entity);

        // Mark the entity as active in the world
        SetActive(_world, entity, true);
        return entity;
    }

    /// <summary>
    /// Return an entity to the pool. The entity is disabled in the world.
    /// </summary>
    public void Release(Entity entity)
    {
        if (!_active.Contains(entity)) return;

        _active.Remove(entity);
        SetActive(_world, entity, false);
        _available.Enqueue(entity);
    }

    /// <summary>Return all currently active entities to the pool.</summary>
    public void ReleaseAll()
    {
        foreach (var entity in _active.ToList())
            Release(entity);
    }

    public IReadOnlyCollection<Entity> ActiveEntities => _active;

    // -----------------------------------------------------------------------

    private void Expand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var entity = _world.CreateEntity();
            SetActive(_world, entity, false);
            _available.Enqueue(entity);
            _totalCreated++;
        }
    }

    /// <summary>
    /// Enable or disable an entity by toggling its PositionComponent visibility flag.
    /// In a full renderer this would mark the entity as active/inactive in the ECS.
    /// Here we use a simple approach compatible with the existing architecture.
    /// </summary>
    private static void SetActive(World world, Entity entity, bool active)
    {
        var pos = world.GetComponent<PositionComponent>(entity);
        if (pos != null)
            pos.IsActive = active;
    }
}

/// <summary>
/// Manages named pools of reusable entities to avoid per-frame allocations.
/// Provides pools for common short-lived objects like projectiles and particles.
/// </summary>
public class ObjectPoolSystem : ISystem
{
    private readonly Dictionary<string, EntityPool> _pools = new();

    // Built-in pool names exposed as constants
    public const string PROJECTILE_POOL = "Projectiles";
    public const string PARTICLE_POOL   = "Particles";
    public const string LOOT_POOL       = "LootDrops";
    public const string DAMAGE_TEXT_POOL = "DamageNumbers";

    // Diagnostics
    public int TotalPoolCount => _pools.Count;
    public int TotalActiveEntities => _pools.Values.Sum(p => p.ActiveCount);
    public int TotalAvailableEntities => _pools.Values.Sum(p => p.AvailableCount);

    public void Initialize(World world)
    {
        // Pre-allocate standard pools
        CreatePool(PROJECTILE_POOL, world, initialCapacity: 64);
        CreatePool(PARTICLE_POOL,   world, initialCapacity: 128);
        CreatePool(LOOT_POOL,       world, initialCapacity: 32);
        CreatePool(DAMAGE_TEXT_POOL, world, initialCapacity: 16);

        Console.WriteLine($"[ObjectPool] Initialized {_pools.Count} pools " +
                          $"({TotalAvailableEntities} entities pre-allocated)");
    }

    public void Update(World world, float deltaTime)
    {
        // Auto-return expired entities from the particle pool
        ReturnExpiredParticles(world);

        // Auto-return expired projectiles
        ReturnExpiredProjectiles(world);
    }

    // -----------------------------------------------------------------------
    // Pool management
    // -----------------------------------------------------------------------

    /// <summary>Create or replace a named pool.</summary>
    public EntityPool CreatePool(string name, World world, int initialCapacity = 32)
    {
        var pool = new EntityPool(world, name, initialCapacity);
        _pools[name] = pool;
        return pool;
    }

    /// <summary>Retrieve a pool by name, or null if it does not exist.</summary>
    public EntityPool? GetPool(string name)
        => _pools.TryGetValue(name, out var pool) ? pool : null;

    // -----------------------------------------------------------------------
    // Convenience acquire/release
    // -----------------------------------------------------------------------

    /// <summary>Acquire an entity from the named pool.</summary>
    public Entity? Acquire(string poolName)
        => _pools.TryGetValue(poolName, out var pool) ? pool.Acquire() : null;

    /// <summary>Return an entity to whichever pool owns it.</summary>
    public void Release(Entity entity)
    {
        foreach (var pool in _pools.Values)
        {
            if (pool.ActiveEntities.Contains(entity))
            {
                pool.Release(entity);
                return;
            }
        }
    }

    /// <summary>Release all entities in every pool.</summary>
    public void ReleaseAll()
    {
        foreach (var pool in _pools.Values)
            pool.ReleaseAll();
    }

    // -----------------------------------------------------------------------
    // Diagnostic output
    // -----------------------------------------------------------------------

    public void PrintStats()
    {
        Console.WriteLine("\n=== Object Pool Statistics ===");
        foreach (var pool in _pools.Values)
        {
            Console.WriteLine($"  {pool.Name,-20} active={pool.ActiveCount,4}  " +
                              $"available={pool.AvailableCount,4}  total={pool.TotalCapacity,4}");
        }
        Console.WriteLine($"  {"TOTAL",-20} active={TotalActiveEntities,4}  " +
                          $"available={TotalAvailableEntities,4}\n");
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private void ReturnExpiredParticles(World world)
    {
        var particlePool = GetPool(PARTICLE_POOL);
        if (particlePool == null) return;

        foreach (var entity in particlePool.ActiveEntities.ToList())
        {
            var emitter = world.GetComponent<ParticleEmitterComponent>(entity);
            // Return particle emitters that have stopped emitting and are not looping
            if (emitter != null && !emitter.IsEmitting && !emitter.IsLooping)
                particlePool.Release(entity);
        }
    }

    private void ReturnExpiredProjectiles(World world)
    {
        var projectilePool = GetPool(PROJECTILE_POOL);
        if (projectilePool == null) return;

        foreach (var entity in projectilePool.ActiveEntities.ToList())
        {
            var proj = world.GetComponent<ProjectileComponent>(entity);
            if (proj != null && proj.IsExpired)
                projectilePool.Release(entity);
        }
    }
}
