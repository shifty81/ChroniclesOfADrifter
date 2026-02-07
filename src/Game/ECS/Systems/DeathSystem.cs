using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System for handling entity death and respawn logic
/// </summary>
public class DeathSystem : ISystem
{
    private const float RESPAWN_DELAY_SECONDS = 3f; // Seconds until respawn
    private int _lastCountdownSecond = -1;
    private LootDropSystem? lootDropSystem;
    
    public void Initialize(World world)
    {
        Console.WriteLine("[DeathSystem] Initialized");
    }
    
    public void SetLootDropSystem(LootDropSystem system)
    {
        lootDropSystem = system;
    }
    
    public void Update(World world, float deltaTime)
    {
        // Process all entities with health and respawn components
        foreach (var entity in world.GetEntitiesWithComponent<HealthComponent>())
        {
            var health = world.GetComponent<HealthComponent>(entity);
            var respawn = world.GetComponent<RespawnComponent>(entity);
            
            if (health == null) continue;
            
            // Update invulnerability timer if active
            if (respawn != null && respawn.InvulnerabilityTimer > 0)
            {
                respawn.InvulnerabilityTimer = Math.Max(0, respawn.InvulnerabilityTimer - deltaTime);
            }
            
            // Check for death
            if (health.CurrentHealth <= 0 && !health.IsAlive)
            {
                var isPlayer = world.GetComponent<PlayerComponent>(entity) != null;
                
                if (isPlayer && respawn != null)
                {
                    HandlePlayerDeath(world, entity, respawn, deltaTime);
                }
                else if (!isPlayer)
                {
                    HandleEnemyDeath(world, entity);
                }
            }
        }
    }
    
    private void HandlePlayerDeath(World world, Entity entity, RespawnComponent respawn, float deltaTime)
    {
        // First time dying - initialize death state
        if (!respawn.IsDead)
        {
            respawn.IsDead = true;
            respawn.DeathCount++;
            respawn.RespawnTimer = RESPAWN_DELAY_SECONDS;
            _lastCountdownSecond = (int)Math.Ceiling(RESPAWN_DELAY_SECONDS);
            
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         YOU DIED!                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            
            // Apply death penalty
            ApplyDeathPenalty(world, entity, respawn);
            
            Console.WriteLine($"[DeathSystem] Respawning in {_lastCountdownSecond} seconds...");
        }
        
        // Update respawn timer
        respawn.RespawnTimer -= deltaTime;
        
        // Show countdown
        int currentSecond = (int)Math.Ceiling(respawn.RespawnTimer);
        if (currentSecond > 0 && currentSecond != _lastCountdownSecond)
        {
            _lastCountdownSecond = currentSecond;
            Console.WriteLine($"[DeathSystem] Respawning in {currentSecond}...");
        }
        
        // Respawn when timer expires
        if (respawn.RespawnTimer <= 0)
        {
            RespawnPlayer(world, entity, respawn);
        }
    }
    
    private void ApplyDeathPenalty(World world, Entity entity, RespawnComponent respawn)
    {
        var currency = world.GetComponent<CurrencyComponent>(entity);
        
        if (currency != null && respawn.DeathPenaltyPercent > 0)
        {
            int goldBefore = currency.Gold;
            int goldLost = (int)(goldBefore * (respawn.DeathPenaltyPercent / 100f));
            
            if (goldLost > 0)
            {
                currency.RemoveGold(goldLost);
                Console.WriteLine($"[DeathSystem] Lost {goldLost} gold ({respawn.DeathPenaltyPercent}% penalty). Remaining: {currency.Gold}");
            }
            else
            {
                Console.WriteLine("[DeathSystem] No gold to lose.");
            }
        }
    }
    
    private void RespawnPlayer(World world, Entity entity, RespawnComponent respawn)
    {
        Console.WriteLine("\n[DeathSystem] Respawning player...");
        
        // Reset death state
        respawn.IsDead = false;
        respawn.RespawnTimer = 0f;
        _lastCountdownSecond = -1;
        
        // Restore health
        var health = world.GetComponent<HealthComponent>(entity);
        if (health != null)
        {
            health.CurrentHealth = health.MaxHealth;
            Console.WriteLine($"[DeathSystem] Health restored to {health.MaxHealth}");
        }
        
        // Move to respawn point
        var position = world.GetComponent<PositionComponent>(entity);
        if (position != null)
        {
            position.X = respawn.RespawnX;
            position.Y = respawn.RespawnY;
            Console.WriteLine($"[DeathSystem] Respawned at ({respawn.RespawnX}, {respawn.RespawnY})");
        }
        
        // Reset velocity
        var velocity = world.GetComponent<VelocityComponent>(entity);
        if (velocity != null)
        {
            velocity.VX = 0;
            velocity.VY = 0;
        }
        
        // Grant invulnerability
        respawn.InvulnerabilityTimer = respawn.InvulnerabilityDuration;
        Console.WriteLine($"[DeathSystem] Invulnerable for {respawn.InvulnerabilityDuration} seconds");
        
        Console.WriteLine($"[DeathSystem] Total deaths: {respawn.DeathCount}");
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    RESPAWN COMPLETE                              ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝\n");
    }
    
    private void HandleEnemyDeath(World world, Entity entity)
    {
        // Remove enemy from world
        var position = world.GetComponent<PositionComponent>(entity);
        if (position != null)
        {
            Console.WriteLine($"[DeathSystem] Enemy {entity.Id} defeated at ({position.X}, {position.Y})");
            
            // Spawn loot drops
            var lootDrop = world.GetComponent<LootDropComponent>(entity);
            if (lootDrop != null && lootDropSystem != null)
            {
                lootDropSystem.QueueLootDrop(position.X, position.Y, lootDrop);
            }
            
            // Award XP to player
            var creature = world.GetComponent<CreatureComponent>(entity);
            if (creature != null && creature.ExperienceValue > 0)
            {
                foreach (var playerEntity in world.GetEntitiesWithComponent<PlayerComponent>())
                {
                    ExperienceSystem.AwardCombatXP(world, playerEntity, creature.ExperienceValue);
                    break;
                }
            }
        }
        
        // Destroy the entity
        world.DestroyEntity(entity);
    }
}
