using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that manages XP gain, level ups, and applies stat bonuses to player.
/// Provides static methods for other systems to award XP.
/// </summary>
public class ExperienceSystem : ISystem
{
    public void Initialize(World world)
    {
        Console.WriteLine("[XP] Experience system initialized");
    }
    
    public void Update(World world, float deltaTime)
    {
        // Apply stat bonuses each frame (ensures bonuses stay applied after loading)
        foreach (var entity in world.GetEntitiesWithComponent<ExperienceComponent>())
        {
            var xp = world.GetComponent<ExperienceComponent>(entity);
            if (xp == null) continue;
            
            ApplyStatBonuses(world, entity, xp);
        }
    }
    
    /// <summary>
    /// Awards XP to an entity (typically the player) from defeating an enemy
    /// </summary>
    public static int AwardCombatXP(World world, Entity playerEntity, int xpAmount)
    {
        var xp = world.GetComponent<ExperienceComponent>(playerEntity);
        if (xp == null || xpAmount <= 0) return 0;
        
        int levelsGained = xp.AddXP(xpAmount);
        
        Console.WriteLine($"[XP] +{xpAmount} XP ({xp.CurrentXP}/{xp.GetXPForNextLevel()} to level {xp.Level + 1})");
        
        if (levelsGained > 0)
        {
            OnLevelUp(world, playerEntity, xp, levelsGained);
        }
        
        return levelsGained;
    }
    
    /// <summary>
    /// Awards XP to an entity from completing a quest
    /// </summary>
    public static int AwardQuestXP(World world, Entity playerEntity, int xpAmount)
    {
        var xp = world.GetComponent<ExperienceComponent>(playerEntity);
        if (xp == null || xpAmount <= 0) return 0;
        
        int levelsGained = xp.AddXP(xpAmount);
        
        Console.WriteLine($"[XP] +{xpAmount} quest XP ({xp.CurrentXP}/{xp.GetXPForNextLevel()} to level {xp.Level + 1})");
        
        if (levelsGained > 0)
        {
            OnLevelUp(world, playerEntity, xp, levelsGained);
        }
        
        return levelsGained;
    }
    
    /// <summary>
    /// Handles level up event
    /// </summary>
    private static void OnLevelUp(World world, Entity entity, ExperienceComponent xp, int levelsGained)
    {
        Console.WriteLine("\n╔══════════════════════════════════════╗");
        Console.WriteLine($"║         LEVEL UP! Level {xp.Level}!         ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine($"  Attack Bonus: +{xp.AttackBonus:F0}");
        Console.WriteLine($"  Defense Bonus: +{xp.DefenseBonus:F0}");
        Console.WriteLine($"  Max Health Bonus: +{xp.MaxHealthBonus:F0}");
        if (xp.SpeedBonus > 0)
        {
            Console.WriteLine($"  Speed Bonus: +{xp.SpeedBonus:F0}");
        }
        Console.WriteLine();
        
        // Apply bonuses immediately
        ApplyStatBonuses(world, entity, xp);
        
        // Heal to full on level up
        var health = world.GetComponent<HealthComponent>(entity);
        if (health != null)
        {
            health.CurrentHealth = health.MaxHealth;
            Console.WriteLine($"[XP] Health fully restored!");
        }
        
        // Trigger screen shake for feedback
        foreach (var cameraEntity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            var camera = world.GetComponent<CameraComponent>(cameraEntity);
            if (camera != null && camera.IsActive)
            {
                ScreenShakeSystem.TriggerMediumShake(world, cameraEntity);
                break;
            }
        }
    }
    
    /// <summary>
    /// Applies stat bonuses from experience to the entity's other components
    /// </summary>
    private static void ApplyStatBonuses(World world, Entity entity, ExperienceComponent xp)
    {
        // Apply max health bonus
        var health = world.GetComponent<HealthComponent>(entity);
        if (health != null)
        {
            float baseMaxHealth = 100f; // Base max health
            float newMaxHealth = baseMaxHealth + xp.MaxHealthBonus;
            if (Math.Abs(health.MaxHealth - newMaxHealth) > 0.01f)
            {
                float healthPercent = health.CurrentHealth / health.MaxHealth;
                health.MaxHealth = newMaxHealth;
                health.CurrentHealth = healthPercent * newMaxHealth;
            }
        }
    }
}
