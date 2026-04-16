using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Manages environmental hazards: activates/deactivates periodic traps,
/// detects entity overlap, and applies damage and status effects.
/// </summary>
public class HazardSystem : ISystem
{
    public void Initialize(World world)
    {
        Console.WriteLine("[Hazard] Environmental hazard system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        var hazardEntities = world.GetEntitiesWithComponent<HazardComponent>();

        foreach (var hazardEntity in hazardEntities)
        {
            var hazard = world.GetComponent<HazardComponent>(hazardEntity);
            var hazardPos = world.GetComponent<PositionComponent>(hazardEntity);
            if (hazard == null || hazardPos == null) continue;

            // Advance the phase timer for periodic hazards
            UpdateHazardPhase(hazard, deltaTime);

            // Advance per-trigger cooldown
            hazard.TimeSinceLastTrigger += deltaTime;

            if (!hazard.IsActive) continue;

            // Find all entities that can take damage inside the radius
            foreach (var victim in world.GetEntitiesWithComponent<HealthComponent>())
            {
                if (victim == hazardEntity) continue;

                var victimPos = world.GetComponent<PositionComponent>(victim);
                var health = world.GetComponent<HealthComponent>(victim);
                if (victimPos == null || health == null) continue;

                float dist = Distance(hazardPos, victimPos);
                if (dist > hazard.TriggerRadius) continue;

                // Bear traps trigger once then deactivate until reset
                if (hazard.Type == HazardType.BearTrap)
                {
                    if (!hazard.IsTriggered)
                    {
                        TriggerContact(world, victim, hazard, deltaTime);
                        hazard.IsTriggered = true;
                        hazard.IsActive = false;
                        hazard.ResetTimer = hazard.InactiveDuration;
                        Console.WriteLine($"[Hazard] Bear trap sprung!");
                    }
                    continue;
                }

                // For all other hazards respect the per-trigger cooldown
                if (hazard.TimeSinceLastTrigger >= hazard.TriggerCooldown)
                {
                    TriggerContact(world, victim, hazard, deltaTime);
                    hazard.TimeSinceLastTrigger = 0f;
                }
            }

            // Bear trap reset countdown
            if (hazard.Type == HazardType.BearTrap && hazard.IsTriggered)
            {
                hazard.ResetTimer -= deltaTime;
                if (hazard.ResetTimer <= 0f)
                {
                    hazard.IsTriggered = false;
                    hazard.IsActive = true;
                    Console.WriteLine("[Hazard] Bear trap reset.");
                }
            }
        }
    }

    // -----------------------------------------------------------------------

    /// <summary>
    /// Toggle periodic hazards (SpikeTrap, GasVent, etc.) between active/inactive phases.
    /// Continuous hazards (LavaPool, AcidPool) are always active and skipped here.
    /// </summary>
    private static void UpdateHazardPhase(HazardComponent hazard, float deltaTime)
    {
        if (hazard.Type == HazardType.BearTrap) return; // bear traps managed separately
        if (hazard.ActiveDuration == float.MaxValue) return; // continuous hazard

        hazard.PhaseTimer += deltaTime;

        if (hazard.IsActive)
        {
            if (hazard.PhaseTimer >= hazard.ActiveDuration)
            {
                hazard.IsActive = false;
                hazard.PhaseTimer = 0f;
            }
        }
        else
        {
            if (hazard.PhaseTimer >= hazard.InactiveDuration)
            {
                hazard.IsActive = true;
                hazard.PhaseTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Apply damage and optional status effect from a hazard to a victim entity.
    /// </summary>
    private static void TriggerContact(World world, Entity victim, HazardComponent hazard, float deltaTime)
    {
        var health = world.GetComponent<HealthComponent>(victim);
        if (health == null) return;

        float damage = hazard.DamagePerSecond * deltaTime;
        health.Damage(damage);

        // Apply status effect once per trigger
        if (hazard.AppliedStatusEffect.HasValue)
        {
            var statusComp = world.GetComponent<StatusEffectComponent>(victim);
            if (statusComp != null)
            {
                var effect = new StatusEffect(hazard.AppliedStatusEffect.Value, hazard.StatusEffectDuration);
                statusComp.ApplyEffect(effect);
            }
        }
    }

    /// <summary>Euclidean distance between two positions</summary>
    private static float Distance(PositionComponent a, PositionComponent b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    // -----------------------------------------------------------------------
    // Static factory helpers
    // -----------------------------------------------------------------------

    /// <summary>Create a spike-trap hazard entity at the given position</summary>
    public static Entity CreateSpikeTrap(World world, float x, float y)
        => CreateHazard(world, x, y, new HazardComponent(HazardType.SpikeTrap, dps: 25f, radius: 0.8f));

    /// <summary>Create a lava-pool hazard entity at the given position</summary>
    public static Entity CreateLavaPool(World world, float x, float y, float radius = 2f)
        => CreateHazard(world, x, y, new HazardComponent(HazardType.LavaPool, dps: 40f, radius: radius));

    /// <summary>Create an acid-pool hazard entity at the given position</summary>
    public static Entity CreateAcidPool(World world, float x, float y, float radius = 1.5f)
        => CreateHazard(world, x, y, new HazardComponent(HazardType.AcidPool, dps: 15f, radius: radius));

    /// <summary>Create a gas-vent hazard entity at the given position</summary>
    public static Entity CreateGasVent(World world, float x, float y)
        => CreateHazard(world, x, y, new HazardComponent(HazardType.GasVent, dps: 10f, radius: 2.5f));

    /// <summary>Create a bear-trap hazard entity at the given position</summary>
    public static Entity CreateBearTrap(World world, float x, float y)
        => CreateHazard(world, x, y, new HazardComponent(HazardType.BearTrap, dps: 30f, radius: 0.6f));

    /// <summary>Create an electric-fence hazard entity at the given position</summary>
    public static Entity CreateElectricFence(World world, float x, float y, float length = 3f)
        => CreateHazard(world, x, y, new HazardComponent(HazardType.ElectricFence, dps: 20f, radius: length));

    private static Entity CreateHazard(World world, float x, float y, HazardComponent hazard)
    {
        var entity = world.CreateEntity();
        world.AddComponent(entity, new PositionComponent(x, y));
        world.AddComponent(entity, hazard);
        return entity;
    }
}
