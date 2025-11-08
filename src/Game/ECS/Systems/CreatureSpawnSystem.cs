using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that handles creature spawning based on biome, depth, and spawn points
/// </summary>
public class CreatureSpawnSystem : ISystem
{
    private Random random;
    private int worldSeed;
    
    public CreatureSpawnSystem(int seed)
    {
        this.worldSeed = seed;
        this.random = new Random(seed);
    }
    
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Update spawn points
        var spawnPoints = world.GetEntitiesWithComponent<SpawnPointComponent>();
        
        foreach (var spawnPointEntity in spawnPoints)
        {
            var spawnPoint = world.GetComponent<SpawnPointComponent>(spawnPointEntity);
            var position = world.GetComponent<PositionComponent>(spawnPointEntity);
            
            if (spawnPoint == null || position == null) continue;
            
            spawnPoint.TimeSinceLastSpawn += deltaTime;
            
            // Check if we should spawn
            if (spawnPoint.CurrentCreatureCount < spawnPoint.MaxCreatures && 
                spawnPoint.TimeSinceLastSpawn >= spawnPoint.RespawnTime)
            {
                // Spawn a creature
                SpawnCreature(world, spawnPoint.CreatureType, position.X, position.Y, spawnPoint.SpawnRadius);
                spawnPoint.CurrentCreatureCount++;
                spawnPoint.TimeSinceLastSpawn = 0;
            }
        }
    }
    
    /// <summary>
    /// Spawns a creature at the specified location with optional random offset
    /// </summary>
    public Entity SpawnCreature(World world, CreatureType type, float x, float y, float spawnRadius = 0)
    {
        // Apply random offset within spawn radius
        if (spawnRadius > 0)
        {
            float angle = (float)(random.NextDouble() * Math.PI * 2);
            float distance = (float)(random.NextDouble() * spawnRadius);
            x += MathF.Cos(angle) * distance;
            y += MathF.Sin(angle) * distance;
        }
        
        // Create entity
        Entity creature = world.CreateEntity();
        
        // Add position
        world.AddComponent(creature, new PositionComponent(x, y));
        
        // Add velocity
        world.AddComponent(creature, new VelocityComponent());
        
        // Get creature stats based on type
        var (name, isHostile, health, damage, speed, agroRange, xp, scriptPath) = GetCreatureStats(type);
        
        // Add creature component
        world.AddComponent(creature, new CreatureComponent(type, name, isHostile, agroRange, xp));
        
        // Add health
        world.AddComponent(creature, new HealthComponent(health));
        
        // Add combat component if hostile
        if (isHostile)
        {
            world.AddComponent(creature, new CombatComponent(damage, 75f, 0.5f));
        }
        
        // Add collision
        world.AddComponent(creature, new CollisionComponent(32f, 32f, 0, 0, false, true, true, CollisionLayer.Enemy, CollisionLayer.All));
        
        // Add sprite (using placeholder)
        world.AddComponent(creature, new SpriteComponent(
            0,  // TextureId - 0 for placeholder
            64, 64
        ));
        
        // Add Lua script for AI behavior - ScriptSystem will handle loading
        if (scriptPath != null)
        {
            world.AddComponent(creature, new ScriptComponent(scriptPath));
        }
        
        Console.WriteLine($"[Spawn] {name} spawned at ({x:F1}, {y:F1})");
        
        return creature;
    }
    
    /// <summary>
    /// Spawns creatures appropriate for the given biome and depth
    /// </summary>
    public void SpawnByBiomeAndDepth(World world, BiomeType biome, int depth, float x, float y, int count = 1)
    {
        var possibleCreatures = GetCreaturesForBiomeAndDepth(biome, depth);
        
        if (possibleCreatures.Count == 0) return;
        
        for (int i = 0; i < count; i++)
        {
            // Randomly select a creature type from possibilities
            CreatureType type = possibleCreatures[random.Next(possibleCreatures.Count)];
            
            // Random offset to avoid stacking
            float offsetX = (float)(random.NextDouble() * 200 - 100);
            float offsetY = (float)(random.NextDouble() * 200 - 100);
            
            SpawnCreature(world, type, x + offsetX, y + offsetY);
        }
    }
    
    /// <summary>
    /// Get creature types that can spawn in the given biome and depth
    /// </summary>
    private List<CreatureType> GetCreaturesForBiomeAndDepth(BiomeType biome, int depth)
    {
        var creatures = new List<CreatureType>();
        
        // Surface creatures (depth 0-3)
        if (depth <= 3)
        {
            switch (biome)
            {
                case BiomeType.Plains:
                    creatures.Add(CreatureType.Rabbit);
                    creatures.Add(CreatureType.Deer);
                    creatures.Add(CreatureType.Bird);
                    break;
                    
                case BiomeType.Forest:
                    creatures.Add(CreatureType.Deer);
                    creatures.Add(CreatureType.Bird);
                    creatures.Add(CreatureType.Wolf);
                    break;
                    
                case BiomeType.Desert:
                    creatures.Add(CreatureType.Bandit);
                    break;
                    
                case BiomeType.Snow:
                    creatures.Add(CreatureType.Rabbit);
                    creatures.Add(CreatureType.Wolf);
                    break;
                    
                case BiomeType.Swamp:
                    creatures.Add(CreatureType.Goblin);
                    break;
                    
                case BiomeType.Rocky:
                    creatures.Add(CreatureType.Goblin);
                    creatures.Add(CreatureType.Bandit);
                    break;
                    
                case BiomeType.Jungle:
                    creatures.Add(CreatureType.Bird);
                    creatures.Add(CreatureType.Goblin);
                    break;
                    
                case BiomeType.Beach:
                    creatures.Add(CreatureType.Bird);
                    break;
            }
        }
        // Shallow underground (depth 4-8)
        else if (depth <= 8)
        {
            creatures.Add(CreatureType.Rat);
            creatures.Add(CreatureType.CaveBat);
            creatures.Add(CreatureType.CaveSpider);
        }
        // Deep underground (depth 9-14)
        else if (depth <= 14)
        {
            creatures.Add(CreatureType.CaveBat);
            creatures.Add(CreatureType.CaveSpider);
            creatures.Add(CreatureType.Zombie);
            creatures.Add(CreatureType.Skeleton);
        }
        // Very deep underground (depth 15-19)
        else if (depth <= 19)
        {
            creatures.Add(CreatureType.Skeleton);
            creatures.Add(CreatureType.Zombie);
            if (biome == BiomeType.Snow)
            {
                creatures.Add(CreatureType.IceElemental);
                creatures.Add(CreatureType.Yeti);
            }
        }
        
        return creatures;
    }
    
    /// <summary>
    /// Gets creature statistics based on type
    /// Returns: (name, isHostile, health, damage, speed, agroRange, xp, scriptPath)
    /// </summary>
    private (string, bool, float, float, float, float, int, string?) GetCreatureStats(CreatureType type)
    {
        return type switch
        {
            // Passive creatures
            CreatureType.Rabbit => ("Rabbit", false, 10f, 0f, 120f, 0f, 5, "enemies/passive_animal.lua"),
            CreatureType.Deer => ("Deer", false, 30f, 0f, 100f, 0f, 10, "enemies/passive_animal.lua"),
            CreatureType.Bird => ("Bird", false, 5f, 0f, 150f, 0f, 3, "enemies/passive_animal.lua"),
            
            // Basic hostiles
            CreatureType.Goblin => ("Goblin", true, 50f, 15f, 80f, 300f, 20, "enemies/goblin_ai.lua"),
            CreatureType.Bandit => ("Bandit", true, 60f, 20f, 85f, 350f, 25, "enemies/goblin_ai.lua"),
            CreatureType.Wolf => ("Wolf", true, 40f, 18f, 110f, 400f, 18, "enemies/goblin_ai.lua"),
            
            // Underground creatures
            CreatureType.Rat => ("Giant Rat", true, 25f, 10f, 90f, 200f, 12, "enemies/goblin_ai.lua"),
            CreatureType.CaveBat => ("Cave Bat", true, 20f, 12f, 130f, 250f, 15, "enemies/goblin_ai.lua"),
            CreatureType.CaveSpider => ("Cave Spider", true, 35f, 15f, 95f, 280f, 18, "enemies/goblin_ai.lua"),
            
            // Deep underground
            CreatureType.Skeleton => ("Skeleton", true, 80f, 25f, 70f, 300f, 35, "enemies/goblin_ai.lua"),
            CreatureType.Zombie => ("Zombie", true, 100f, 30f, 60f, 250f, 40, "enemies/goblin_ai.lua"),
            CreatureType.IceElemental => ("Ice Elemental", true, 120f, 35f, 75f, 350f, 50, "enemies/goblin_ai.lua"),
            
            // Bosses
            CreatureType.GoblinChief => ("Goblin Chief", true, 200f, 40f, 90f, 400f, 100, "enemies/goblin_ai.lua"),
            CreatureType.Yeti => ("Yeti", true, 300f, 50f, 80f, 500f, 150, "enemies/goblin_ai.lua"),
            CreatureType.DragonBoss => ("Dragon", true, 500f, 75f, 100f, 600f, 250, "enemies/goblin_ai.lua"),
            
            _ => ("Unknown", false, 10f, 0f, 50f, 0f, 1, null)
        };
    }
    
    /// <summary>
    /// Gets the sprite path for a creature type
    /// </summary>
    private string GetCreatureSpritePath(CreatureType type)
    {
        return type switch
        {
            CreatureType.Rabbit => "assets/creatures/rabbit.png",
            CreatureType.Deer => "assets/creatures/deer.png",
            CreatureType.Bird => "assets/creatures/bird.png",
            CreatureType.Goblin => "assets/creatures/goblin.png",
            CreatureType.Bandit => "assets/creatures/bandit.png",
            CreatureType.Wolf => "assets/creatures/wolf.png",
            CreatureType.Rat => "assets/creatures/rat.png",
            CreatureType.CaveBat => "assets/creatures/bat.png",
            CreatureType.CaveSpider => "assets/creatures/spider.png",
            CreatureType.Skeleton => "assets/creatures/skeleton.png",
            CreatureType.Zombie => "assets/creatures/zombie.png",
            CreatureType.IceElemental => "assets/creatures/ice_elemental.png",
            CreatureType.GoblinChief => "assets/creatures/goblin_chief.png",
            CreatureType.Yeti => "assets/creatures/yeti.png",
            CreatureType.DragonBoss => "assets/creatures/dragon.png",
            _ => "assets/placeholder.png"
        };
    }
}
