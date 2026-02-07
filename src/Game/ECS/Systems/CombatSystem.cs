using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Combat system - handles attacking and damage
/// </summary>
public class CombatSystem : ISystem
{
    private const int KEY_SPACE = 32;
    
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Update attack cooldowns
        foreach (var entity in world.GetEntitiesWithComponent<CombatComponent>())
        {
            var combat = world.GetComponent<CombatComponent>(entity);
            if (combat != null)
            {
                combat.TimeSinceLastAttack += deltaTime;
            }
        }
        
        // Handle player attacks
        foreach (var entity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            var combat = world.GetComponent<CombatComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            var respawn = world.GetComponent<RespawnComponent>(entity);
            
            // Can't attack while dead
            if (respawn != null && respawn.IsDead) continue;
            
            if (combat != null && position != null)
            {
                // Check for attack input
                if (EngineInterop.Input_IsKeyPressed(KEY_SPACE) && 
                    combat.TimeSinceLastAttack >= combat.AttackCooldown)
                {
                    // Find nearest enemy in range
                    Entity? targetEnemy = FindNearestEnemy(world, position, combat.AttackRange);
                    
                    if (targetEnemy != null)
                    {
                        var enemyHealth = world.GetComponent<HealthComponent>(targetEnemy.Value);
                        if (enemyHealth != null && enemyHealth.CurrentHealth > 0)
                        {
                            float damage = Math.Max(0, combat.AttackDamage);
                            enemyHealth.CurrentHealth = Math.Max(0, enemyHealth.CurrentHealth - damage);
                            Console.WriteLine($"[Combat] Player attacked enemy {targetEnemy.Value.Id} for {damage} damage! Health: {enemyHealth.CurrentHealth}/{enemyHealth.MaxHealth}");
                            
                            // Trigger screen shake on hit
                            TriggerCameraShake(world, ShakeIntensity.Light);
                            
                            if (enemyHealth.CurrentHealth <= 0)
                            {
                                Console.WriteLine($"[Combat] Enemy {targetEnemy.Value.Id} defeated!");
                                // Trigger heavier shake on kill
                                TriggerCameraShake(world, ShakeIntensity.Medium);
                            }
                        }
                    }
                    
                    combat.TimeSinceLastAttack = 0;
                }
            }
        }
        
        // Handle enemy attacks
        foreach (var entity in world.GetEntitiesWithComponent<ScriptComponent>())
        {
            var combat = world.GetComponent<CombatComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            var health = world.GetComponent<HealthComponent>(entity);
            
            // Only alive enemies can attack
            if (combat != null && position != null && health != null && health.CurrentHealth > 0)
            {
                // Check if player is in range
                foreach (var playerEntity in world.GetEntitiesWithComponent<PlayerComponent>())
                {
                    var playerPosition = world.GetComponent<PositionComponent>(playerEntity);
                    var playerHealth = world.GetComponent<HealthComponent>(playerEntity);
                    var playerRespawn = world.GetComponent<RespawnComponent>(playerEntity);
                    
                    // Skip if player is dead or invulnerable
                    if (playerRespawn != null && (playerRespawn.IsDead || playerRespawn.IsInvulnerable))
                        continue;
                    
                    if (playerPosition != null && playerHealth != null)
                    {
                        float distance = Distance(position, playerPosition);
                        
                        if (distance <= combat.AttackRange && combat.TimeSinceLastAttack >= combat.AttackCooldown)
                        {
                            float damage = Math.Max(0, combat.AttackDamage);
                            playerHealth.CurrentHealth = Math.Max(0, playerHealth.CurrentHealth - damage);
                            Console.WriteLine($"[Combat] Enemy {entity.Id} attacked player for {damage} damage! Player health: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
                            
                            // Trigger screen shake when player is hit
                            TriggerCameraShake(world, ShakeIntensity.Light);
                            
                            if (playerHealth.CurrentHealth <= 0)
                            {
                                // Heavy shake on player death
                                TriggerCameraShake(world, ShakeIntensity.Heavy);
                                // DeathSystem will handle the rest
                            }
                            
                            combat.TimeSinceLastAttack = 0;
                        }
                    }
                }
            }
        }
    }
    
    private Entity? FindNearestEnemy(World world, PositionComponent playerPos, float maxRange)
    {
        Entity? nearest = null;
        float nearestDistance = maxRange;
        
        foreach (var entity in world.GetEntitiesWithComponent<ScriptComponent>())
        {
            var enemyPos = world.GetComponent<PositionComponent>(entity);
            var health = world.GetComponent<HealthComponent>(entity);
            
            if (enemyPos != null && health != null && health.CurrentHealth > 0)
            {
                float distance = Distance(playerPos, enemyPos);
                
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = entity;
                }
            }
        }
        
        return nearest;
    }
    
    private float Distance(PositionComponent a, PositionComponent b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
    
    /// <summary>
    /// Shake intensity levels
    /// </summary>
    private enum ShakeIntensity
    {
        Light,
        Medium,
        Heavy
    }
    
    /// <summary>
    /// Trigger camera shake effect
    /// </summary>
    private void TriggerCameraShake(World world, ShakeIntensity intensity)
    {
        // Find the active camera
        foreach (var cameraEntity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            var camera = world.GetComponent<CameraComponent>(cameraEntity);
            if (camera != null && camera.IsActive)
            {
                // Trigger shake based on intensity
                switch (intensity)
                {
                    case ShakeIntensity.Light:
                        ScreenShakeSystem.TriggerLightShake(world, cameraEntity);
                        break;
                    case ShakeIntensity.Medium:
                        ScreenShakeSystem.TriggerMediumShake(world, cameraEntity);
                        break;
                    case ShakeIntensity.Heavy:
                        ScreenShakeSystem.TriggerHeavyShake(world, cameraEntity);
                        break;
                }
                break; // Only shake the first active camera
            }
        }
    }
}
