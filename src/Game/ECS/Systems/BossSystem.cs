using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Boss attack patterns that vary by phase
/// </summary>
public enum BossAttackPattern
{
    /// <summary>Standard melee attack</summary>
    MeleeSwing,
    /// <summary>Charge toward player</summary>
    Charge,
    /// <summary>Area-of-effect stomp/slam attack</summary>
    AreaSlam,
    /// <summary>Enraged flurry of attacks</summary>
    Enrage,
    /// <summary>Summon minions to aid the boss</summary>
    SummonMinions
}

/// <summary>
/// System managing boss encounters and special boss mechanics.
/// Handles boss AI, attack patterns, arena boundaries, health display, and rewards.
/// </summary>
public class BossSystem : ISystem
{
    private Dictionary<Entity, bool> bossEncounterStarted = new();
    private Dictionary<Entity, float> bossAttackTimers = new();
    private Dictionary<Entity, float> bossPatternTimers = new();
    private Dictionary<Entity, BossAttackPattern> bossCurrentPattern = new();
    private float healthBarDisplayTimer = 0f;
    private const float HEALTH_BAR_DISPLAY_INTERVAL = 2f;
    
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
        
        // Update boss AI and attack patterns
        UpdateBossAI(world, deltaTime);
        
        // Enforce arena boundaries on player during encounters
        EnforceArenaBoundaries(world);
        
        // Display boss health bars periodically
        healthBarDisplayTimer += deltaTime;
        if (healthBarDisplayTimer >= HEALTH_BAR_DISPLAY_INTERVAL)
        {
            healthBarDisplayTimer = 0f;
            DisplayBossHealthBars(world);
        }
        
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
        
        // Initialize attack timers
        bossAttackTimers[bossEntity] = 0f;
        bossPatternTimers[bossEntity] = 0f;
        bossCurrentPattern[bossEntity] = BossAttackPattern.MeleeSwing;
        
        // Trigger epic camera shake
        TriggerCameraShake(world, ShakeIntensity.Heavy);
        
        // Display initial health bar
        var health = world.GetComponent<HealthComponent>(bossEntity);
        if (health != null)
        {
            DisplayBossHealthBar(boss.BossName, health.CurrentHealth, health.MaxHealth, boss.CurrentPhase);
        }
    }
    
    /// <summary>
    /// Updates boss AI behavior including movement toward player and attack patterns
    /// </summary>
    private void UpdateBossAI(World world, float deltaTime)
    {
        foreach (var bossEntity in world.GetEntitiesWithComponent<BossComponent>())
        {
            if (!bossEncounterStarted.GetValueOrDefault(bossEntity, false))
                continue;
            
            var boss = world.GetComponent<BossComponent>(bossEntity);
            var health = world.GetComponent<HealthComponent>(bossEntity);
            var bossPos = world.GetComponent<PositionComponent>(bossEntity);
            var bossCombat = world.GetComponent<CombatComponent>(bossEntity);
            
            if (boss == null || health == null || bossPos == null || bossCombat == null || 
                boss.IsDefeated || health.CurrentHealth <= 0)
                continue;
            
            // Update attack timer
            if (!bossAttackTimers.ContainsKey(bossEntity))
                bossAttackTimers[bossEntity] = 0f;
            bossAttackTimers[bossEntity] += deltaTime;
            
            // Update pattern timer
            if (!bossPatternTimers.ContainsKey(bossEntity))
                bossPatternTimers[bossEntity] = 0f;
            bossPatternTimers[bossEntity] += deltaTime;
            
            // Switch attack pattern every 8 seconds
            if (bossPatternTimers[bossEntity] >= 8f)
            {
                bossPatternTimers[bossEntity] = 0f;
                SelectNextAttackPattern(bossEntity, boss);
            }
            
            // Find nearest player
            Entity? nearestPlayer = null;
            float nearestDist = float.MaxValue;
            
            foreach (var playerEntity in world.GetEntitiesWithComponent<PlayerComponent>())
            {
                var playerPos = world.GetComponent<PositionComponent>(playerEntity);
                if (playerPos == null) continue;
                
                float dx = playerPos.X - bossPos.X;
                float dy = playerPos.Y - bossPos.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestPlayer = playerEntity;
                }
            }
            
            if (nearestPlayer == null) continue;
            
            var targetPos = world.GetComponent<PositionComponent>(nearestPlayer.Value);
            if (targetPos == null) continue;
            
            // Execute current attack pattern
            var pattern = bossCurrentPattern.GetValueOrDefault(bossEntity, BossAttackPattern.MeleeSwing);
            ExecuteAttackPattern(world, bossEntity, boss, bossPos, targetPos, nearestDist, pattern, deltaTime);
        }
    }
    
    /// <summary>
    /// Selects the next attack pattern based on boss phase
    /// </summary>
    private void SelectNextAttackPattern(Entity bossEntity, BossComponent boss)
    {
        var patterns = GetPatternsForPhase(boss.CurrentPhase);
        var random = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
        var newPattern = patterns[random.Next(patterns.Count)];
        bossCurrentPattern[bossEntity] = newPattern;
        Console.WriteLine($"[Boss] {boss.BossName} switches to {newPattern} attack!");
    }
    
    /// <summary>
    /// Gets available attack patterns for the current boss phase
    /// </summary>
    private static List<BossAttackPattern> GetPatternsForPhase(BossPhase phase)
    {
        return phase switch
        {
            BossPhase.Phase1 => new List<BossAttackPattern> 
            { 
                BossAttackPattern.MeleeSwing, 
                BossAttackPattern.MeleeSwing,
                BossAttackPattern.Charge 
            },
            BossPhase.Phase2 => new List<BossAttackPattern> 
            { 
                BossAttackPattern.MeleeSwing, 
                BossAttackPattern.Charge, 
                BossAttackPattern.AreaSlam,
                BossAttackPattern.SummonMinions
            },
            BossPhase.Phase3 => new List<BossAttackPattern> 
            { 
                BossAttackPattern.Charge, 
                BossAttackPattern.AreaSlam, 
                BossAttackPattern.Enrage,
                BossAttackPattern.SummonMinions
            },
            _ => new List<BossAttackPattern> { BossAttackPattern.MeleeSwing }
        };
    }
    
    /// <summary>
    /// Executes the current attack pattern
    /// </summary>
    private void ExecuteAttackPattern(World world, Entity bossEntity, BossComponent boss,
        PositionComponent bossPos, PositionComponent targetPos, float distance,
        BossAttackPattern pattern, float deltaTime)
    {
        float dx = targetPos.X - bossPos.X;
        float dy = targetPos.Y - bossPos.Y;
        float moveSpeed = 80f; // Base boss movement speed
        
        switch (pattern)
        {
            case BossAttackPattern.MeleeSwing:
                // Move toward player and attack when in range
                if (distance > 60f)
                {
                    float norm = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (norm > 0)
                    {
                        bossPos.X += (dx / norm) * moveSpeed * deltaTime;
                        bossPos.Y += (dy / norm) * moveSpeed * deltaTime;
                    }
                }
                break;
                
            case BossAttackPattern.Charge:
                // Fast charge toward player
                if (distance > 40f)
                {
                    float norm = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (norm > 0)
                    {
                        float chargeSpeed = moveSpeed * 2.5f;
                        bossPos.X += (dx / norm) * chargeSpeed * deltaTime;
                        bossPos.Y += (dy / norm) * chargeSpeed * deltaTime;
                    }
                }
                else
                {
                    TriggerCameraShake(world, ShakeIntensity.Medium);
                }
                break;
                
            case BossAttackPattern.AreaSlam:
                // Move to player then slam (AoE damage)
                if (distance > 80f)
                {
                    float norm = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (norm > 0)
                    {
                        bossPos.X += (dx / norm) * moveSpeed * 1.5f * deltaTime;
                        bossPos.Y += (dy / norm) * moveSpeed * 1.5f * deltaTime;
                    }
                }
                else if (bossAttackTimers.GetValueOrDefault(bossEntity, 0f) >= 1.5f)
                {
                    // Slam attack - damage all players in radius
                    bossAttackTimers[bossEntity] = 0f;
                    TriggerCameraShake(world, ShakeIntensity.Heavy);
                    Console.WriteLine($"[Boss] {boss.BossName} performs GROUND SLAM!");
                }
                break;
                
            case BossAttackPattern.Enrage:
                // Fast movement and attacks during enrage
                if (distance > 30f)
                {
                    float norm = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (norm > 0)
                    {
                        float enrageSpeed = moveSpeed * 3f;
                        bossPos.X += (dx / norm) * enrageSpeed * deltaTime;
                        bossPos.Y += (dy / norm) * enrageSpeed * deltaTime;
                    }
                }
                break;
                
            case BossAttackPattern.SummonMinions:
                // Stay at distance and periodically summon
                if (distance < 150f)
                {
                    // Move away from player
                    float norm = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (norm > 0)
                    {
                        bossPos.X -= (dx / norm) * moveSpeed * 0.5f * deltaTime;
                        bossPos.Y -= (dy / norm) * moveSpeed * 0.5f * deltaTime;
                    }
                }
                
                if (bossAttackTimers.GetValueOrDefault(bossEntity, 0f) >= 5f)
                {
                    bossAttackTimers[bossEntity] = 0f;
                    Console.WriteLine($"[Boss] {boss.BossName} SUMMONS MINIONS!");
                    // Spawn goblin minions near the boss
                    SpawnBossMinion(world, bossPos.X + 50, bossPos.Y);
                    SpawnBossMinion(world, bossPos.X - 50, bossPos.Y);
                }
                break;
        }
        
        // Keep boss within arena
        ClampToArena(bossPos, boss);
    }
    
    /// <summary>
    /// Spawns a minion near the boss
    /// </summary>
    private static void SpawnBossMinion(World world, float x, float y)
    {
        var minion = world.CreateEntity();
        world.AddComponent(minion, new CreatureComponent(CreatureType.Goblin, "Boss Minion", true, 200f, 5));
        world.AddComponent(minion, new PositionComponent(x, y));
        world.AddComponent(minion, new VelocityComponent());
        world.AddComponent(minion, new HealthComponent(15));
        world.AddComponent(minion, new CombatComponent(damage: 5f, range: 50f, cooldown: 2f));
        world.AddComponent(minion, new CollisionComponent(28, 28, layer: CollisionLayer.Enemy));
    }
    
    /// <summary>
    /// Clamps a position to within the boss arena bounds
    /// </summary>
    private static void ClampToArena(PositionComponent pos, BossComponent boss)
    {
        pos.X = Math.Max(boss.ArenaX, Math.Min(pos.X, boss.ArenaX + boss.ArenaWidth));
        pos.Y = Math.Max(boss.ArenaY, Math.Min(pos.Y, boss.ArenaY + boss.ArenaHeight));
    }
    
    /// <summary>
    /// Enforces arena boundaries, keeping player inside during active encounters
    /// </summary>
    private void EnforceArenaBoundaries(World world)
    {
        foreach (var playerEntity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            var playerPos = world.GetComponent<PositionComponent>(playerEntity);
            if (playerPos == null) continue;
            
            foreach (var bossEntity in world.GetEntitiesWithComponent<BossComponent>())
            {
                if (!bossEncounterStarted.GetValueOrDefault(bossEntity, false))
                    continue;
                
                var boss = world.GetComponent<BossComponent>(bossEntity);
                var health = world.GetComponent<HealthComponent>(bossEntity);
                
                if (boss == null || health == null || boss.IsDefeated || health.CurrentHealth <= 0)
                    continue;
                
                // If encounter is active, keep player within arena
                if (boss.IsInArena(playerPos.X, playerPos.Y))
                    continue;
                
                // Clamp player to arena
                ClampToArena(playerPos, boss);
            }
        }
    }
    
    /// <summary>
    /// Displays health bars for all active boss encounters
    /// </summary>
    private void DisplayBossHealthBars(World world)
    {
        foreach (var bossEntity in world.GetEntitiesWithComponent<BossComponent>())
        {
            if (!bossEncounterStarted.GetValueOrDefault(bossEntity, false))
                continue;
            
            var boss = world.GetComponent<BossComponent>(bossEntity);
            var health = world.GetComponent<HealthComponent>(bossEntity);
            
            if (boss == null || health == null || boss.IsDefeated || health.CurrentHealth <= 0)
                continue;
            
            DisplayBossHealthBar(boss.BossName, health.CurrentHealth, health.MaxHealth, boss.CurrentPhase);
        }
    }
    
    /// <summary>
    /// Displays a visual health bar for a boss in the console
    /// </summary>
    private static void DisplayBossHealthBar(string bossName, float currentHealth, float maxHealth, BossPhase phase)
    {
        float healthPercent = Math.Max(0, currentHealth / maxHealth);
        int barWidth = 30;
        int filledWidth = (int)(healthPercent * barWidth);
        
        string bar = new string('█', filledWidth) + new string('░', barWidth - filledWidth);
        string phaseText = phase switch
        {
            BossPhase.Phase1 => "",
            BossPhase.Phase2 => " [ENRAGED]",
            BossPhase.Phase3 => " [DESPERATE]",
            _ => ""
        };
        
        Console.WriteLine($"  ♦ {bossName}{phaseText}");
        Console.WriteLine($"  [{bar}] {currentHealth:F0}/{maxHealth:F0} HP");
    }
    
    /// <summary>
    /// Handle boss phase transitions
    /// </summary>
    private void OnBossPhaseChange(World world, Entity bossEntity, BossComponent boss, BossPhase previousPhase)
    {
        Console.WriteLine($"\n[Boss] {boss.BossName} enters {boss.CurrentPhase}!");
        
        // Trigger screen shake
        TriggerCameraShake(world, ShakeIntensity.Heavy);
        
        // Boss behavior changes
        var combat = world.GetComponent<CombatComponent>(bossEntity);
        if (combat != null)
        {
            switch (boss.CurrentPhase)
            {
                case BossPhase.Phase2:
                    combat.AttackDamage = (int)(combat.AttackDamage * 1.25f);
                    combat.AttackCooldown *= 0.8f; // Faster attacks
                    Console.WriteLine($"[Boss] {boss.BossName} grows stronger! Attack +25%, Speed +20%");
                    break;
                case BossPhase.Phase3:
                    combat.AttackDamage = (int)(combat.AttackDamage * 1.5f);
                    combat.AttackCooldown *= 0.6f; // Even faster
                    Console.WriteLine($"[Boss] {boss.BossName} is DESPERATE! Attack +50%, Speed +40%");
                    break;
            }
        }
        
        // Force a pattern switch on phase change
        SelectNextAttackPattern(bossEntity, boss);
        
        // Display updated health bar
        var health = world.GetComponent<HealthComponent>(bossEntity);
        if (health != null)
        {
            DisplayBossHealthBar(boss.BossName, health.CurrentHealth, health.MaxHealth, boss.CurrentPhase);
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
        
        // Clean up encounter state
        bossEncounterStarted.Remove(bossEntity);
        bossAttackTimers.Remove(bossEntity);
        bossPatternTimers.Remove(bossEntity);
        bossCurrentPattern.Remove(bossEntity);
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
        
        // Award XP
        if (boss.ExperienceReward > 0)
        {
            ExperienceSystem.AwardCombatXP(world, playerEntity, boss.ExperienceReward);
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
        }
    }
    
    /// <summary>
    /// Create a boss entity in the world with full configuration
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
        world.AddComponent(bossEntity, new HealthComponent(500));
        
        // Add combat with strong attack
        world.AddComponent(bossEntity, new CombatComponent
        {
            AttackDamage = 25,
            AttackRange = 100f,
            AttackCooldown = 2.0f
        });
        
        // Add collision
        world.AddComponent(bossEntity, new CollisionComponent(64, 64, layer: CollisionLayer.Enemy));
        
        Console.WriteLine($"[Boss] Created boss '{name}' at ({x}, {y})");
        Console.WriteLine($"[Boss] Arena: ({arenaX}, {arenaY}) {arenaWidth}x{arenaHeight}");
        
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
