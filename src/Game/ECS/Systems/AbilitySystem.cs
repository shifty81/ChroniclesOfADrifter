using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Drives the player's special-ability lifecycle each frame:
///   • Energy regeneration at a configurable rate.
///   • Hotkey polling for ability activation (1-5 keys).
///   • Applies immediate gameplay effects when an ability fires
///     (dash velocity burst, magic-bolt projectile spawn, etc.).
///   • Manages cooldown tracking via game-time accumulation.
/// </summary>
public class AbilitySystem : ISystem
{
    // Energy regen: points per second
    private const float ENERGY_REGEN_PER_SECOND = 10f;

    // Hotkey→AbilityType mapping (keys '1'..'5')
    private static readonly Dictionary<int, AbilityType> KeyBindings = new()
    {
        { 49, AbilityType.Dash       },  // '1'
        { 50, AbilityType.SwordSpin  },  // '2'
        { 51, AbilityType.MagicBolt  },  // '3'
        { 52, AbilityType.BowCharge  },  // '4'
        { 53, AbilityType.TorchLight },  // '5'
    };

    private float _gameTime = 0f;
    private float _lastKeyPressTime = 0f;
    private const float KEY_COOLDOWN = 0.2f;

    public void Initialize(World world)
    {
        // Ensure every player entity starts with pre-defined abilities
        foreach (var entity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            var abilities = world.GetComponent<AbilityComponent>(entity);
            if (abilities != null)
                SeedDefaultAbilities(abilities);
        }

        Console.WriteLine("[Ability] Ability system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        _gameTime += deltaTime;
        _lastKeyPressTime += deltaTime;

        foreach (var entity in world.GetEntitiesWithComponent<AbilityComponent>())
        {
            var abilityComp = world.GetComponent<AbilityComponent>(entity);
            if (abilityComp == null) continue;

            // Regenerate energy
            RegenerateEnergy(abilityComp, deltaTime);

            // Poll hotkeys only for player entities
            if (world.GetComponent<PlayerComponent>(entity) != null)
                PollAbilityHotkeys(world, entity, abilityComp);
        }
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Attempt to activate an ability on an entity programmatically.
    /// Returns true if the ability fired successfully.
    /// </summary>
    public static bool ActivateAbility(World world, Entity entity, AbilityType abilityType, float gameTime)
    {
        var abilityComp = world.GetComponent<AbilityComponent>(entity);
        if (abilityComp == null) return false;

        if (!abilityComp.UseAbility(abilityType, gameTime)) return false;

        ApplyAbilityEffect(world, entity, abilityType);
        Console.WriteLine($"[Ability] Used: {abilityType}");
        return true;
    }

    /// <summary>
    /// Unlock an ability for an entity and log the event.
    /// </summary>
    public static void UnlockAbility(World world, Entity entity, AbilityType abilityType)
    {
        var abilityComp = world.GetComponent<AbilityComponent>(entity);
        if (abilityComp == null) return;

        abilityComp.UnlockAbility(abilityType);
        Console.WriteLine($"[Ability] Unlocked: {abilityType}");
    }

    /// <summary>Print all abilities and their current state for the given entity.</summary>
    public static void DisplayAbilities(World world, Entity entity)
    {
        var abilityComp = world.GetComponent<AbilityComponent>(entity);
        if (abilityComp == null) return;

        Console.WriteLine($"\n=== Abilities  (Energy: {abilityComp.CurrentEnergy}/{abilityComp.MaxEnergy}) ===");
        Console.WriteLine($"  {"Ability",-20} {"Unlocked",9} {"Energy",8} {"Cooldown",10}");
        Console.WriteLine($"  {new string('-', 52)}");

        foreach (var ability in abilityComp.GetAllAbilities())
        {
            string status = ability.IsUnlocked ? "✓" : "—";
            Console.WriteLine($"  {ability.Name,-20} {status,9} {ability.EnergyCost,8} {ability.Cooldown,9:F1}s");
        }
        Console.WriteLine("===========================================\n");
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static void RegenerateEnergy(AbilityComponent abilityComp, float deltaTime)
    {
        if (abilityComp.CurrentEnergy < abilityComp.MaxEnergy)
        {
            int regen = (int)MathF.Round(ENERGY_REGEN_PER_SECOND * deltaTime);
            if (regen > 0)
                abilityComp.RestoreEnergy(regen);
        }
    }

    private void PollAbilityHotkeys(World world, Entity entity, AbilityComponent abilityComp)
    {
        if (_lastKeyPressTime < KEY_COOLDOWN) return;

        try
        {
            foreach (var (key, abilityType) in KeyBindings)
            {
                if (EngineInterop.Input_IsKeyPressed(key))
                {
                    if (abilityComp.UseAbility(abilityType, _gameTime))
                    {
                        ApplyAbilityEffect(world, entity, abilityType);
                        Console.WriteLine($"[Ability] Activated via hotkey: {abilityType}");
                        _lastKeyPressTime = 0f;
                    }
                    break;
                }
            }
        }
        catch (DllNotFoundException)
        {
            // Engine not available (running in test/headless mode) — skip hotkey polling
        }
    }

    /// <summary>
    /// Apply the gameplay effect of an ability.
    /// Each ability type has a distinct immediate effect.
    /// </summary>
    private static void ApplyAbilityEffect(World world, Entity entity, AbilityType abilityType)
    {
        switch (abilityType)
        {
            case AbilityType.Dash:
                ApplyDash(world, entity);
                break;

            case AbilityType.SwordSpin:
                ApplySwordSpin(world, entity);
                break;

            case AbilityType.MagicBolt:
                ApplyMagicBolt(world, entity);
                break;

            case AbilityType.ShieldBash:
                ApplyShieldBash(world, entity);
                break;

            case AbilityType.TorchLight:
                ApplyTorchLight(world, entity);
                break;

            case AbilityType.BowCharge:
                ApplyBowCharge(world, entity);
                break;

            default:
                // Utility/exploration abilities (Swim, WallClimb, etc.) are
                // checked passively by other systems (SwimmingSystem, etc.)
                break;
        }
    }

    // -----------------------------------------------------------------------
    // Individual ability effects
    // -----------------------------------------------------------------------

    private static void ApplyDash(World world, Entity entity)
    {
        var velocity = world.GetComponent<VelocityComponent>(entity);
        if (velocity == null) return;

        // Boost velocity in current movement direction
        float speed = MathF.Sqrt(velocity.VX * velocity.VX + velocity.VY * velocity.VY);
        if (speed < 0.01f)
        {
            velocity.VX = 15f; // Dash right if standing still
        }
        else
        {
            float scale = 15f / speed;
            velocity.VX *= scale;
            velocity.VY *= scale;
        }

        Console.WriteLine("[Ability] DASH!");
    }

    private static void ApplySwordSpin(World world, Entity entity)
    {
        var pos = world.GetComponent<PositionComponent>(entity);
        if (pos == null) return;

        // Damage all hostile creatures within spin radius (2 tiles)
        const float SPIN_RADIUS = 64f; // 2 tiles at 32px each
        const float SPIN_DAMAGE = 35f;

        foreach (var other in world.GetEntitiesWithComponent<CreatureComponent>())
        {
            var creature = world.GetComponent<CreatureComponent>(other);
            var otherPos = world.GetComponent<PositionComponent>(other);
            var health = world.GetComponent<HealthComponent>(other);

            if (creature == null || !creature.IsHostile || otherPos == null || health == null) continue;

            float dx = otherPos.X - pos.X;
            float dy = otherPos.Y - pos.Y;
            if (dx * dx + dy * dy <= SPIN_RADIUS * SPIN_RADIUS)
            {
                health.Damage(SPIN_DAMAGE);
                Console.WriteLine($"[Ability] SwordSpin hit {creature.CreatureName} for {SPIN_DAMAGE} dmg");
            }
        }
    }

    private static void ApplyMagicBolt(World world, Entity entity)
    {
        var pos = world.GetComponent<PositionComponent>(entity);
        if (pos == null) return;

        // Spawn a magic bolt projectile travelling right
        var bolt = world.CreateEntity();
        world.AddComponent(bolt, new PositionComponent(pos.X + 20f, pos.Y));
        world.AddComponent(bolt, new VelocityComponent(300f, 0f));
        world.AddComponent(bolt, new ProjectileComponent(
            type: ProjectileType.FireBolt,
            damage: 50f,
            speed: 300f,
            dirX: 1f,
            dirY: 0f,
            ownerId: entity.Id,
            isPlayerOwned: true,
            lifetime: 2f
        ));

        Console.WriteLine("[Ability] Magic bolt fired!");
    }

    private static void ApplyShieldBash(World world, Entity entity)
    {
        var pos = world.GetComponent<PositionComponent>(entity);
        if (pos == null) return;

        const float BASH_RADIUS = 48f;
        const float BASH_DAMAGE = 20f;
        const float KNOCK_SPEED = 200f;

        foreach (var other in world.GetEntitiesWithComponent<CreatureComponent>())
        {
            var creature = world.GetComponent<CreatureComponent>(other);
            var otherPos = world.GetComponent<PositionComponent>(other);
            var health = world.GetComponent<HealthComponent>(other);
            var velocity = world.GetComponent<VelocityComponent>(other);

            if (creature == null || !creature.IsHostile || otherPos == null || health == null) continue;

            float dx = otherPos.X - pos.X;
            float dy = otherPos.Y - pos.Y;
            float dist2 = dx * dx + dy * dy;
            if (dist2 <= BASH_RADIUS * BASH_RADIUS)
            {
                health.Damage(BASH_DAMAGE);

                // Knockback
                if (velocity != null && dist2 > 0.01f)
                {
                    float dist = MathF.Sqrt(dist2);
                    velocity.VX += (dx / dist) * KNOCK_SPEED;
                    velocity.VY += (dy / dist) * KNOCK_SPEED;
                }
                Console.WriteLine($"[Ability] ShieldBash knocked back {creature.CreatureName}");
            }
        }
    }

    private static void ApplyTorchLight(World world, Entity entity)
    {
        // Toggle a light source component on the player entity
        var light = world.GetComponent<LightSourceComponent>(entity);
        if (light != null)
        {
            light.Radius = light.Radius > 0 ? 0f : 150f;
            Console.WriteLine($"[Ability] Torch {(light.Radius > 0 ? "ON" : "OFF")}");
        }
        else
        {
            world.AddComponent(entity, new LightSourceComponent(150f, 1.0f));
            Console.WriteLine("[Ability] Torch lit!");
        }
    }

    private static void ApplyBowCharge(World world, Entity entity)
    {
        var pos = world.GetComponent<PositionComponent>(entity);
        if (pos == null) return;

        // Charged arrow: faster, more damage than regular
        var arrow = world.CreateEntity();
        world.AddComponent(arrow, new PositionComponent(pos.X + 20f, pos.Y));
        world.AddComponent(arrow, new VelocityComponent(450f, 0f));
        world.AddComponent(arrow, new ProjectileComponent(
            type: ProjectileType.Arrow,
            damage: 80f,
            speed: 450f,
            dirX: 1f,
            dirY: 0f,
            ownerId: entity.Id,
            isPlayerOwned: true,
            lifetime: 3f
        ));

        Console.WriteLine("[Ability] Charged arrow fired!");
    }

    // -----------------------------------------------------------------------
    // Seed default abilities (called on Initialize)
    // -----------------------------------------------------------------------

    private static void SeedDefaultAbilities(AbilityComponent abilityComp)
    {
        var defaults = new[]
        {
            new Ability(AbilityType.Dash,          "Dash",         "Quick burst of speed",                    cooldown: 3f,  energyCost: 20),
            new Ability(AbilityType.SwordSpin,      "Sword Spin",   "Spin-attack all nearby enemies",          cooldown: 5f,  energyCost: 30),
            new Ability(AbilityType.MagicBolt,      "Magic Bolt",   "Fire a piercing magical projectile",      cooldown: 2f,  energyCost: 25),
            new Ability(AbilityType.ShieldBash,     "Shield Bash",  "Bash nearby enemies back",                cooldown: 4f,  energyCost: 15),
            new Ability(AbilityType.TorchLight,     "Torch Light",  "Toggle a light source",                   cooldown: 0.5f,energyCost: 5),
            new Ability(AbilityType.BowCharge,      "Bow Charge",   "Fire a high-damage charged arrow",        cooldown: 4f,  energyCost: 35),
            new Ability(AbilityType.Swim,           "Swim",         "Swim without stamina loss",               cooldown: 0f,  energyCost: 0),
            new Ability(AbilityType.WallClimb,      "Wall Climb",   "Climb vertical walls",                    cooldown: 0f,  energyCost: 0),
            new Ability(AbilityType.DoubleJump,     "Double Jump",  "Jump again while airborne",               cooldown: 1f,  energyCost: 10),
            new Ability(AbilityType.Glide,          "Glide",        "Slow fall by gliding",                    cooldown: 0f,  energyCost: 0),
            new Ability(AbilityType.Hookshot,       "Hookshot",     "Grapple to distant surfaces",             cooldown: 2f,  energyCost: 15),
            new Ability(AbilityType.BombCraft,      "Bomb Craft",   "Craft an explosive bomb",                 cooldown: 8f,  energyCost: 40),
            new Ability(AbilityType.WaterBreathing, "Water Breath", "Breathe underwater",                     cooldown: 0f,  energyCost: 0),
            new Ability(AbilityType.RevealSecrets,  "Reveal Secrets","Detect hidden passages",                 cooldown: 10f, energyCost: 20),
            new Ability(AbilityType.ReadAncientText,"Ancient Text",  "Read ancient inscriptions",              cooldown: 0f,  energyCost: 0),
            new Ability(AbilityType.OpenLockedDoors,"Lockpick",     "Open locked doors",                      cooldown: 0f,  energyCost: 0),
            new Ability(AbilityType.MineHardRocks,  "Hard Mining",  "Mine diamond-grade rocks",               cooldown: 0f,  energyCost: 0),
        };

        foreach (var ability in defaults)
            abilityComp.RegisterAbility(ability);
    }
}
