using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Bridges the world-level <see cref="WeatherSystem"/> with the ECS each frame.
///
/// Effects applied:
///   • Player movement speed is multiplied by the weather's movement-speed factor.
///   • Visibility range on <see cref="LightingComponent"/> is scaled by the weather's visibility factor.
///   • Damaging weather (sandstorm, heavy storm) applies direct health damage per second.
///   • Sandstorms and heavy storms apply the Blinded / Poison status effect to the player.
///   • Weather tint is stored for the renderer to use.
/// </summary>
public class WeatherEffectSystem : ISystem
{
    private WeatherSystem? _weatherSystem;

    // Base movement speed stored per player entity so we can restore it
    private readonly Dictionary<Entity, float> _basePlayerSpeed = new();

    // Tint broadcast so renderers can pick it up
    public (float r, float g, float b, float a) CurrentTint { get; private set; } = (1f, 1f, 1f, 0f);

    public void Initialize(World world)
    {
        _weatherSystem = world.GetSharedResource<WeatherSystem>("WeatherSystem");

        // Store base player speeds before any weather modification
        foreach (var entity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            var player = world.GetComponent<PlayerComponent>(entity);
            if (player != null)
                _basePlayerSpeed[entity] = player.Speed;
        }

        Console.WriteLine("[WeatherEffect] Weather effect system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        if (_weatherSystem == null) return;

        float speedMult       = _weatherSystem.GetMovementSpeedMultiplier();
        float visibilityMult  = _weatherSystem.GetVisibilityMultiplier();
        bool  isDamaging      = _weatherSystem.IsDamagingWeather();
        float damagePerSecond = _weatherSystem.GetWeatherDamagePerSecond();
        CurrentTint           = _weatherSystem.GetWeatherTint();

        ApplyMovementEffect(world, speedMult);
        ApplyVisibilityEffect(world, visibilityMult);

        if (isDamaging)
            ApplyWeatherDamage(world, deltaTime, damagePerSecond);

        ApplyWeatherStatusEffects(world, deltaTime);
    }

    // -----------------------------------------------------------------------
    // Effect application helpers
    // -----------------------------------------------------------------------

    /// <summary>Scale player speed by the current weather movement multiplier.</summary>
    private void ApplyMovementEffect(World world, float speedMultiplier)
    {
        foreach (var entity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            var player = world.GetComponent<PlayerComponent>(entity);
            if (player == null) continue;

            // Ensure base speed is cached
            if (!_basePlayerSpeed.TryGetValue(entity, out float baseSpeed))
            {
                baseSpeed = player.Speed;
                _basePlayerSpeed[entity] = baseSpeed;
            }

            player.Speed = baseSpeed * speedMultiplier;
        }

        // Also affect NPC movement via velocity magnitude (passive NPCs keep moving)
        foreach (var entity in world.GetEntitiesWithComponent<NPCComponent>())
        {
            var velocity = world.GetComponent<VelocityComponent>(entity);
            if (velocity == null) continue;

            float currentSpeed = MathF.Sqrt(velocity.VX * velocity.VX + velocity.VY * velocity.VY);
            if (currentSpeed < 0.01f) continue;

            float targetSpeed = currentSpeed * speedMultiplier;
            float scale = targetSpeed / currentSpeed;
            velocity.VX *= scale;
            velocity.VY *= scale;
        }
    }

    /// <summary>Scale LightingComponent visibility range by weather visibility multiplier.</summary>
    private static void ApplyVisibilityEffect(World world, float visibilityMultiplier)
    {
        foreach (var entity in world.GetEntitiesWithComponent<LightingComponent>())
        {
            var lighting = world.GetComponent<LightingComponent>(entity);
            if (lighting == null) continue;

            // Clamp visibility range; WeatherSystem returns 0.1–1.0
            lighting.VisibilityMultiplier = visibilityMultiplier;
        }
    }

    /// <summary>Apply weather damage to player entities each frame.</summary>
    private static void ApplyWeatherDamage(World world, float deltaTime, float damagePerSecond)
    {
        float damage = damagePerSecond * deltaTime;

        foreach (var entity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            var health = world.GetComponent<HealthComponent>(entity);
            if (health == null) continue;

            health.Damage(damage);

            if (damage > 0.01f)
                Console.WriteLine($"[WeatherEffect] Weather damage: {damage:F2} DPS applied");
        }
    }

    /// <summary>
    /// Apply status effects caused by specific weather conditions.
    /// Sandstorm → Blinded (modelled as Stunned), Heavy Storm → Poison (cold damage).
    /// </summary>
    private void ApplyWeatherStatusEffects(World world, float deltaTime)
    {
        if (_weatherSystem == null) return;

        var weatherType = _weatherSystem.CurrentWeather;
        var intensity   = _weatherSystem.CurrentIntensity;

        // Only apply status effects for heavy weather
        if (intensity != WeatherIntensity.Heavy) return;

        StatusEffectType? effectType = weatherType switch
        {
            WeatherType.Sandstorm => StatusEffectType.Stunned,
            WeatherType.Storm     => StatusEffectType.Poison,
            _                    => (StatusEffectType?)null
        };

        if (effectType == null) return;

        foreach (var entity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            var statusComp = world.GetComponent<StatusEffectComponent>(entity);
            if (statusComp == null) continue;

            // Refresh the effect every tick (duration 3s, so it stays active while weather persists)
            if (!statusComp.HasEffect(effectType.Value))
            {
                var effect = new StatusEffect(effectType.Value, duration: 3f);
                statusComp.ApplyEffect(effect);
                Console.WriteLine($"[WeatherEffect] Applied {effectType.Value} from {weatherType}");
            }
        }
    }

    // -----------------------------------------------------------------------
    // Public display helper
    // -----------------------------------------------------------------------

    /// <summary>Print a summary of current weather effects to the console.</summary>
    public void DisplayCurrentEffects()
    {
        if (_weatherSystem == null)
        {
            Console.WriteLine("[WeatherEffect] No weather system attached.");
            return;
        }

        float speed = _weatherSystem.GetMovementSpeedMultiplier();
        float vis   = _weatherSystem.GetVisibilityMultiplier();
        bool  dmg   = _weatherSystem.IsDamagingWeather();
        float dps   = _weatherSystem.GetWeatherDamagePerSecond();
        var tint    = CurrentTint;

        Console.WriteLine("\n=== Current Weather Effects ===");
        Console.WriteLine($"  Weather:    {_weatherSystem.CurrentWeather} ({_weatherSystem.CurrentIntensity})");
        Console.WriteLine($"  Move Speed: {speed * 100:F0}%");
        Console.WriteLine($"  Visibility: {vis * 100:F0}%");
        Console.WriteLine($"  Damaging:   {dmg} ({dps:F1} DPS)");
        Console.WriteLine($"  Tint:       R={tint.r:F2} G={tint.g:F2} B={tint.b:F2}");
        Console.WriteLine("================================\n");
    }
}
