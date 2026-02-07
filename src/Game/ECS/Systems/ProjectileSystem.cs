using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that manages projectile movement, collision detection, and lifetime
/// </summary>
public class ProjectileSystem : ISystem
{
    private readonly List<Entity> _toDestroy = new();

    public void Initialize(World world)
    {
        Console.WriteLine("[Projectiles] Projectile system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        _toDestroy.Clear();

        // Update ranged attack cooldowns
        foreach (var entity in world.GetEntitiesWithComponent<RangedCombatComponent>())
        {
            var ranged = world.GetComponent<RangedCombatComponent>(entity);
            if (ranged != null)
            {
                ranged.TimeSinceLastRangedAttack += deltaTime;
            }
        }

        // Update projectiles
        foreach (var entity in world.GetEntitiesWithComponent<ProjectileComponent>())
        {
            var projectile = world.GetComponent<ProjectileComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);

            if (projectile == null || position == null) continue;

            // Move projectile
            position.X += projectile.DirectionX * projectile.Speed * deltaTime;
            position.Y += projectile.DirectionY * projectile.Speed * deltaTime;

            // Decrease lifetime
            projectile.RemainingLifetime -= deltaTime;

            // Check if expired
            if (projectile.IsExpired)
            {
                _toDestroy.Add(entity);
                continue;
            }

            // Check collision with targets
            CheckProjectileCollisions(world, entity, projectile, position);
        }

        // Destroy expired/hit projectiles
        foreach (var entity in _toDestroy)
        {
            world.DestroyEntity(entity);
        }
    }

    private void CheckProjectileCollisions(World world, Entity projectileEntity, ProjectileComponent projectile, PositionComponent projPos)
    {
        const float hitRadius = 30f;

        if (projectile.IsPlayerOwned)
        {
            // Check collision with enemies
            foreach (var enemy in world.GetEntitiesWithComponent<ScriptComponent>())
            {
                if (enemy.Id == projectile.OwnerId) continue;

                var enemyPos = world.GetComponent<PositionComponent>(enemy);
                var enemyHealth = world.GetComponent<HealthComponent>(enemy);

                if (enemyPos == null || enemyHealth == null || !enemyHealth.IsAlive) continue;

                float distance = Distance(projPos, enemyPos);
                if (distance <= hitRadius)
                {
                    // Hit enemy
                    enemyHealth.Damage(projectile.Damage);
                    Console.WriteLine($"[Projectiles] {projectile.Type} hit enemy {enemy.Id} for {projectile.Damage} damage! Health: {enemyHealth.CurrentHealth:F0}/{enemyHealth.MaxHealth}");

                    // Apply status effect if applicable
                    if (projectile.AppliesEffect.HasValue)
                    {
                        StatusEffectSystem.ApplyEffect(world, enemy, projectile.AppliesEffect.Value, projectile.EffectDuration);
                    }

                    // Trigger camera shake
                    TriggerCameraShake(world);

                    _toDestroy.Add(projectileEntity);
                    return;
                }
            }
        }
        else
        {
            // Check collision with player
            foreach (var player in world.GetEntitiesWithComponent<PlayerComponent>())
            {
                var playerPos = world.GetComponent<PositionComponent>(player);
                var playerHealth = world.GetComponent<HealthComponent>(player);
                var playerRespawn = world.GetComponent<RespawnComponent>(player);

                if (playerPos == null || playerHealth == null) continue;
                if (playerRespawn != null && (playerRespawn.IsDead || playerRespawn.IsInvulnerable)) continue;

                float distance = Distance(projPos, playerPos);
                if (distance <= hitRadius)
                {
                    playerHealth.Damage(projectile.Damage);
                    Console.WriteLine($"[Projectiles] {projectile.Type} hit player for {projectile.Damage} damage! Health: {playerHealth.CurrentHealth:F0}/{playerHealth.MaxHealth}");

                    if (projectile.AppliesEffect.HasValue)
                    {
                        StatusEffectSystem.ApplyEffect(world, player, projectile.AppliesEffect.Value, projectile.EffectDuration);
                    }

                    TriggerCameraShake(world);

                    _toDestroy.Add(projectileEntity);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Fire a projectile from an entity toward a target position
    /// </summary>
    public static Entity? FireProjectile(World world, Entity owner, float targetX, float targetY, RangedCombatComponent ranged, bool isPlayerOwned)
    {
        if (!ranged.CanAttack()) return null;
        if (!ranged.ConsumeAmmo()) return null;

        var ownerPos = world.GetComponent<PositionComponent>(owner);
        if (ownerPos == null) return null;

        // Calculate direction
        float dx = targetX - ownerPos.X;
        float dy = targetY - ownerPos.Y;
        float dist = MathF.Sqrt(dx * dx + dy * dy);

        if (dist < 0.001f) return null;

        float dirX = dx / dist;
        float dirY = dy / dist;

        // Create projectile entity
        var projectileEntity = world.CreateEntity();
        world.AddComponent(projectileEntity, new PositionComponent(ownerPos.X, ownerPos.Y));
        world.AddComponent(projectileEntity, new ProjectileComponent(
            ranged.ProjectileType,
            ranged.ProjectileDamage,
            ranged.ProjectileSpeed,
            dirX,
            dirY,
            owner.Id,
            isPlayerOwned,
            ranged.ProjectileLifetime
        ));

        ranged.TimeSinceLastRangedAttack = 0;

        Console.WriteLine($"[Projectiles] {ranged.ProjectileType} fired by entity {owner.Id} toward ({targetX:F0}, {targetY:F0})");
        return projectileEntity;
    }

    private float Distance(PositionComponent a, PositionComponent b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private void TriggerCameraShake(World world)
    {
        foreach (var cameraEntity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            var camera = world.GetComponent<CameraComponent>(cameraEntity);
            if (camera != null && camera.IsActive)
            {
                ScreenShakeSystem.TriggerLightShake(world, cameraEntity);
                break;
            }
        }
    }
}
