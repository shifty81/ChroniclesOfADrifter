using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System managing boss encounters and special boss mechanics
/// </summary>
public class BossSystem : ISystem
{
    private Dictionary<Entity, bool> bossEncounterStarted = new();
    
    public void Initialize(World world)
    {
        Console.WriteLine("[Boss] Boss system initialized");
    }
    
    public void Update(World world, float deltaTime)
    {
        // Update boss phases based on health
        UpdateBossPhases(world);
        
        // Handle boss encounter triggers
        CheckBossEncounters(world);
        
        // Handle boss defeat
        CheckBossDefeats(world);
    }
    
    /// <summary>
    /// Update boss phases based on health percentage
    /// </summary>
    private void UpdateBossPhases(World world)
    {
        foreach (var entity in world.GetEntitiesWithComponent<BossComponent>())
        {
            var boss = world.GetComponent<BossComponent>(entity);
            var health = world.GetComponent<HealthComponent>(entity);
            
            if (boss != null && health != null && !boss.IsDefeated)
            {
                float healthPercentage = (float)health.CurrentHealth / health.MaxHealth;
                var previousPhase = boss.CurrentPhase;
                
                boss.UpdatePhase(healthPercentage);
                
                // Trigger phase transition effects
                if (boss.CurrentPhase != previousPhase)
                {
                    OnBossPhaseChange(world, entity, boss, previousPhase);
                }
            }
        }
    }
    
    /// <summary>
    /// Check if player enters boss arena
    /// </summary>
    private void CheckBossEncounters(World world)
    {
        foreach (var playerEntity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            var playerPos = world.GetComponent<PositionComponent>(playerEntity);
            if (playerPos == null) continue;
            
            foreach (var bossEntity in world.GetEntitiesWithComponent<BossComponent>())
            {
                var boss = world.GetComponent<BossComponent>(bossEntity);
                var bossHealth = world.GetComponent<HealthComponent>(bossEntity);
                
                if (boss != null && bossHealth != null && 
                    !boss.IsDefeated && bossHealth.CurrentHealth > 0)
                {
                    if (boss.IsInArena(playerPos.X, playerPos.Y))
                    {
                        if (!bossEncounterStarted.GetValueOrDefault(bossEntity, false))
                        {
                            StartBossEncounter(world, playerEntity, bossEntity, boss);
                            bossEncounterStarted[bossEntity] = true;
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Start a boss encounter
    /// </summary>
    private void StartBossEncounter(World world, Entity playerEntity, Entity bossEntity, BossComponent boss)
    {
        Console.WriteLine("\n╔══════════════════════════════════════╗");
        Console.WriteLine($"║  BOSS ENCOUNTER: {boss.BossName}");
        Console.WriteLine("╚══════════════════════════════════════╝\n");
        
        // Trigger epic camera shake
        TriggerCameraShake(world, ShakeIntensity.Heavy);
        
        // Play boss music (would integrate with audio system)
        // AudioSystem.PlayMusic("boss_theme");
    }
    
    /// <summary>
    /// Handle boss phase transitions
    /// </summary>
    private void OnBossPhaseChange(World world, Entity bossEntity, BossComponent boss, BossPhase previousPhase)
    {
        Console.WriteLine($"\n[Boss] {boss.BossName} enters {boss.CurrentPhase}!");
        
        // Trigger screen shake
        TriggerCameraShake(world, ShakeIntensity.Heavy);
        
        // Boss behavior changes would be handled by AI scripts
        // For now, just increase attack power
        var combat = world.GetComponent<CombatComponent>(bossEntity);
        if (combat != null)
        {
            switch (boss.CurrentPhase)
            {
                case BossPhase.Phase2:
                    combat.AttackDamage = (int)(combat.AttackDamage * 1.25f);
                    combat.AttackCooldown *= 0.8f; // Faster attacks
                    break;
                case BossPhase.Phase3:
                    combat.AttackDamage = (int)(combat.AttackDamage * 1.5f);
                    combat.AttackCooldown *= 0.6f; // Even faster
                    break;
            }
        }
    }
    
    /// <summary>
    /// Check for boss defeats and award rewards
    /// </summary>
    private void CheckBossDefeats(World world)
    {
        foreach (var bossEntity in world.GetEntitiesWithComponent<BossComponent>())
        {
            var boss = world.GetComponent<BossComponent>(bossEntity);
            var health = world.GetComponent<HealthComponent>(bossEntity);
            
            if (boss != null && health != null && !boss.IsDefeated && health.CurrentHealth <= 0)
            {
                OnBossDefeated(world, bossEntity, boss);
            }
        }
    }
    
    /// <summary>
    /// Handle boss defeat and award rewards
    /// </summary>
    private void OnBossDefeated(World world, Entity bossEntity, BossComponent boss)
    {
        boss.Defeat();
        
        Console.WriteLine("\n╔══════════════════════════════════════╗");
        Console.WriteLine($"║  VICTORY! {boss.BossName} DEFEATED!");
        Console.WriteLine("╚══════════════════════════════════════╝\n");
        
        // Trigger victory camera shake
        TriggerCameraShake(world, ShakeIntensity.Heavy);
        
        // Award rewards to all players
        foreach (var playerEntity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            AwardBossRewards(world, playerEntity, boss);
        }
        
        bossEncounterStarted.Remove(bossEntity);
    }
    
    /// <summary>
    /// Award boss rewards to player
    /// </summary>
    private void AwardBossRewards(World world, Entity playerEntity, BossComponent boss)
    {
        // Award gold
        var currency = world.GetComponent<CurrencyComponent>(playerEntity);
        if (currency != null && boss.GoldReward > 0)
        {
            currency.AddGold(boss.GoldReward);
            Console.WriteLine($"[Boss] Received {boss.GoldReward} gold!");
        }
        
        // Award items
        var inventory = world.GetComponent<InventoryComponent>(playerEntity);
        if (inventory != null && boss.ItemDrops.Count > 0)
        {
            foreach (var (item, quantity) in boss.ItemDrops)
            {
                inventory.AddItem(item, quantity);
                Console.WriteLine($"[Boss] Received {quantity}x {item}!");
            }
        }
        
        // Unlock ability
        var abilities = world.GetComponent<AbilityComponent>(playerEntity);
        if (abilities != null && boss.AbilityReward.HasValue)
        {
            abilities.UnlockAbility(boss.AbilityReward.Value);
            Console.WriteLine($"[Boss] Unlocked new ability: {boss.AbilityReward.Value}!");
        }
        
        // Unlock area
        if (boss.UnlockArea != null)
        {
            Console.WriteLine($"[Boss] New area unlocked: {boss.UnlockArea}!");
            // Would integrate with world progression system
        }
    }
    
    /// <summary>
    /// Create a boss entity in the world
    /// </summary>
    public static Entity CreateBoss(World world, BossType type, string name, float x, float y, 
        float arenaX, float arenaY, float arenaWidth, float arenaHeight)
    {
        var bossEntity = world.CreateEntity();
        
        // Add boss component
        var boss = new BossComponent(type, name)
        {
            ArenaX = arenaX,
            ArenaY = arenaY,
            ArenaWidth = arenaWidth,
            ArenaHeight = arenaHeight
        };
        world.AddComponent(bossEntity, boss);
        
        // Add position
        world.AddComponent(bossEntity, new PositionComponent(x, y));
        
        // Add high health
        world.AddComponent(bossEntity, new HealthComponent(500, 500));
        
        // Add combat with strong attack
        world.AddComponent(bossEntity, new CombatComponent
        {
            AttackDamage = 25,
            AttackRange = 100f,
            AttackCooldown = 2.0f
        });
        
        // Add collision
        world.AddComponent(bossEntity, new CollisionComponent(64, 64, CollisionLayer.Enemy));
        
        Console.WriteLine($"[Boss] Created boss '{name}' at ({x}, {y})");
        
        return bossEntity;
    }
    
    /// <summary>
    /// Shake intensity
    /// </summary>
    private enum ShakeIntensity
    {
        Light,
        Medium,
        Heavy
    }
    
    /// <summary>
    /// Trigger camera shake
    /// </summary>
    private void TriggerCameraShake(World world, ShakeIntensity intensity)
    {
        foreach (var cameraEntity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            var camera = world.GetComponent<CameraComponent>(cameraEntity);
            if (camera != null && camera.IsActive)
            {
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
                break;
            }
        }
    }
}
