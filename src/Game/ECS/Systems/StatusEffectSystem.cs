using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that manages status effects - applies damage over time,
/// handles speed modifications, and removes expired effects
/// </summary>
public class StatusEffectSystem : ISystem
{
    public void Initialize(World world)
    {
        Console.WriteLine("[StatusEffects] Status effect system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        foreach (var entity in world.GetEntitiesWithComponent<StatusEffectComponent>())
        {
            var effects = world.GetComponent<StatusEffectComponent>(entity);
            var health = world.GetComponent<HealthComponent>(entity);

            if (effects == null) continue;

            // Process each active effect
            for (int i = effects.ActiveEffects.Count - 1; i >= 0; i--)
            {
                var effect = effects.ActiveEffects[i];

                // Apply damage over time
                if (effect.DamagePerSecond > 0 && health != null && health.IsAlive)
                {
                    float damage = effect.DamagePerSecond * deltaTime;
                    health.Damage(damage);

                    // Log periodic damage (every ~1 second via integer check)
                    float prevTime = effect.RemainingTime + deltaTime;
                    if ((int)prevTime != (int)effect.RemainingTime && effect.RemainingTime > 0)
                    {
                        Console.WriteLine($"[StatusEffects] {effect.Type} dealt {effect.DamagePerSecond:F1} damage to entity {entity.Id} (Health: {health.CurrentHealth:F0}/{health.MaxHealth})");
                    }
                }

                // Tick remaining time
                effect.RemainingTime -= deltaTime;

                // Remove expired effects
                if (effect.IsExpired)
                {
                    Console.WriteLine($"[StatusEffects] {effect.Type} expired on entity {entity.Id}");
                    effects.ActiveEffects.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// Apply a status effect to an entity
    /// </summary>
    public static void ApplyEffect(World world, Entity target, StatusEffectType type, float duration, float damagePerSecond = 0f)
    {
        var effects = world.GetComponent<StatusEffectComponent>(target);
        if (effects == null)
        {
            effects = new StatusEffectComponent();
            world.AddComponent(target, effects);
        }

        var effect = new StatusEffect(type, duration, damagePerSecond);
        effects.ApplyEffect(effect);
        Console.WriteLine($"[StatusEffects] Applied {type} to entity {target.Id} for {duration:F1}s");
    }

    /// <summary>
    /// Remove a specific effect type from an entity
    /// </summary>
    public static void RemoveEffect(World world, Entity target, StatusEffectType type)
    {
        var effects = world.GetComponent<StatusEffectComponent>(target);
        if (effects != null)
        {
            effects.RemoveEffect(type);
            Console.WriteLine($"[StatusEffects] Removed {type} from entity {target.Id}");
        }
    }
}
