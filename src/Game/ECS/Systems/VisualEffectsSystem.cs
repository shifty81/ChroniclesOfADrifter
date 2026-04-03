using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that handles all visual effects: attack animations,
/// environmental animations, and status effect visuals.
/// Renders visual feedback for game events and world ambiance.
/// </summary>
public class VisualEffectsSystem : ISystem
{
    private Random _random = new Random();

    public void Initialize(World world)
    {
        Console.WriteLine("[VisualEffects] Visual effects system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        UpdateAttackAnimations(world, deltaTime);
        UpdateEnvironmentalAnimations(world, deltaTime);
        UpdateStatusEffectVisuals(world, deltaTime);
    }

    /// <summary>
    /// Update and render attack animations
    /// </summary>
    private void UpdateAttackAnimations(World world, float deltaTime)
    {
        foreach (var entity in world.GetEntitiesWithComponent<AttackAnimationComponent>())
        {
            var attackAnim = world.GetComponent<AttackAnimationComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);

            if (attackAnim == null || position == null || !attackAnim.IsPlaying)
                continue;

            // Advance animation timer
            attackAnim.ElapsedTime += deltaTime;

            // Check if animation has finished
            if (attackAnim.Progress >= 1f)
            {
                attackAnim.Stop();
                continue;
            }

            // Render based on attack type
            switch (attackAnim.CurrentAttack)
            {
                case AttackAnimationType.MeleeSwing:
                    RenderMeleeSwing(position, attackAnim);
                    break;
                case AttackAnimationType.MeleeThrust:
                    RenderMeleeThrust(position, attackAnim);
                    break;
                case AttackAnimationType.BowDraw:
                    RenderBowDraw(position, attackAnim);
                    break;
                case AttackAnimationType.SpellCast:
                    RenderSpellCast(position, attackAnim);
                    break;
            }
        }
    }

    /// <summary>
    /// Render a melee swing arc animation
    /// </summary>
    private static void RenderMeleeSwing(PositionComponent position, AttackAnimationComponent anim)
    {
        float progress = anim.Progress;
        float halfArc = anim.SwingArc * 0.5f;
        float startAngle = anim.AttackAngle - halfArc;

        // Ease-out for natural swing feel
        float easedProgress = 1f - (1f - progress) * (1f - progress);

        // Current angle in the swing
        float currentAngle = startAngle + (anim.SwingArc * easedProgress);
        float alpha = anim.EffectA * (1f - progress * 0.5f);

        // Draw arc segments with trail
        if (anim.ShowTrail)
        {
            for (int i = 0; i < anim.TrailSegments; i++)
            {
                float trailProgress = Math.Max(0f, easedProgress - (i * 0.08f));
                float trailAngle = startAngle + (anim.SwingArc * trailProgress);
                float trailAlpha = alpha * (1f - (i * 0.15f));
                if (trailAlpha <= 0f) continue;

                float tx = position.X + MathF.Cos(trailAngle) * anim.AttackReach;
                float ty = position.Y + MathF.Sin(trailAngle) * anim.AttackReach;

                EngineInterop.Renderer_DrawRect(
                    tx - anim.ArcWidth * 0.5f,
                    ty - anim.ArcWidth * 0.5f,
                    anim.ArcWidth,
                    anim.ArcWidth,
                    anim.EffectR, anim.EffectG, anim.EffectB, trailAlpha
                );
            }
        }

        // Draw main arc point
        float x = position.X + MathF.Cos(currentAngle) * anim.AttackReach;
        float y = position.Y + MathF.Sin(currentAngle) * anim.AttackReach;
        float size = anim.ArcWidth * 1.5f;

        EngineInterop.Renderer_DrawRect(
            x - size * 0.5f, y - size * 0.5f,
            size, size,
            anim.EffectR, anim.EffectG, anim.EffectB, alpha
        );
    }

    /// <summary>
    /// Render a melee thrust animation
    /// </summary>
    private static void RenderMeleeThrust(PositionComponent position, AttackAnimationComponent anim)
    {
        float progress = anim.Progress;

        // Thrust extends and retracts
        float thrustProgress = progress < 0.4f
            ? progress / 0.4f           // extend
            : 1f - ((progress - 0.4f) / 0.6f);  // retract

        float reach = anim.AttackReach * thrustProgress;
        float alpha = anim.EffectA * (1f - progress * 0.3f);

        // Draw thrust line segments
        int segments = 4;
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            float segReach = reach * t;
            float segSize = anim.ArcWidth * (1f - t * 0.3f);

            float x = position.X + MathF.Cos(anim.AttackAngle) * segReach;
            float y = position.Y + MathF.Sin(anim.AttackAngle) * segReach;

            EngineInterop.Renderer_DrawRect(
                x - segSize * 0.5f, y - segSize * 0.5f,
                segSize, segSize,
                anim.EffectR, anim.EffectG, anim.EffectB, alpha * (0.5f + t * 0.5f)
            );
        }
    }

    /// <summary>
    /// Render a bow draw animation
    /// </summary>
    private static void RenderBowDraw(PositionComponent position, AttackAnimationComponent anim)
    {
        float progress = anim.Progress;

        // Draw bow string pull-back effect
        float drawBack = progress < 0.8f ? progress / 0.8f : 1f;
        float stringOffset = drawBack * 10f;
        float alpha = anim.EffectA * 0.7f;

        // Bow body (perpendicular to aim direction)
        float perpAngle = anim.AttackAngle + MathF.PI * 0.5f;
        float bowSize = 20f;

        float bx = position.X + MathF.Cos(anim.AttackAngle) * 15f;
        float by = position.Y + MathF.Sin(anim.AttackAngle) * 15f;

        // Draw bow endpoints
        for (int i = -1; i <= 1; i += 2)
        {
            float endX = bx + MathF.Cos(perpAngle) * bowSize * 0.5f * i;
            float endY = by + MathF.Sin(perpAngle) * bowSize * 0.5f * i;

            EngineInterop.Renderer_DrawRect(
                endX - 2f, endY - 2f, 4f, 4f,
                anim.EffectR, anim.EffectG, anim.EffectB, alpha
            );
        }

        // Draw arrow (when fully drawn)
        if (progress > 0.3f)
        {
            float arrowAlpha = alpha * Math.Min(1f, (progress - 0.3f) / 0.3f);
            float arrowX = bx - MathF.Cos(anim.AttackAngle) * stringOffset;
            float arrowY = by - MathF.Sin(anim.AttackAngle) * stringOffset;

            EngineInterop.Renderer_DrawRect(
                arrowX - 1.5f, arrowY - 1.5f, 3f, 3f,
                1f, 0.9f, 0.5f, arrowAlpha
            );
        }
    }

    /// <summary>
    /// Render a spell cast animation (expanding circle effect)
    /// </summary>
    private static void RenderSpellCast(PositionComponent position, AttackAnimationComponent anim)
    {
        float progress = anim.Progress;

        // Expanding ring effect
        float radius = anim.AttackReach * progress;
        float alpha = anim.EffectA * (1f - progress);

        // Draw ring as a series of points
        int points = 12;
        for (int i = 0; i < points; i++)
        {
            float angle = (MathF.PI * 2f / points) * i + progress * 3f;
            float x = position.X + MathF.Cos(angle) * radius;
            float y = position.Y + MathF.Sin(angle) * radius;

            float size = 3f * (1f - progress * 0.5f);

            EngineInterop.Renderer_DrawRect(
                x - size * 0.5f, y - size * 0.5f,
                size, size,
                anim.EffectR, anim.EffectG, anim.EffectB, alpha
            );
        }
    }

    /// <summary>
    /// Update and render environmental animations
    /// </summary>
    private void UpdateEnvironmentalAnimations(World world, float deltaTime)
    {
        foreach (var entity in world.GetEntitiesWithComponent<EnvironmentalAnimationComponent>())
        {
            var envAnim = world.GetComponent<EnvironmentalAnimationComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);

            if (envAnim == null || position == null || !envAnim.IsActive)
                continue;

            // Advance timer
            envAnim.Timer += deltaTime * envAnim.Speed;

            float phase = envAnim.Timer + envAnim.PhaseOffset;

            switch (envAnim.AnimType)
            {
                case EnvironmentalAnimationType.WaterRipple:
                    envAnim.OffsetX = MathF.Sin(phase * 2f) * envAnim.Amplitude;
                    envAnim.OffsetY = MathF.Cos(phase * 1.5f) * envAnim.Amplitude * 0.5f;
                    envAnim.AlphaModifier = 0.7f + MathF.Sin(phase * 3f) * 0.15f;
                    break;

                case EnvironmentalAnimationType.LavaFlow:
                    envAnim.OffsetX = MathF.Sin(phase * 0.8f) * envAnim.Amplitude;
                    envAnim.OffsetY = -MathF.Abs(MathF.Sin(phase * 0.5f)) * envAnim.Amplitude;
                    envAnim.TintR = 0.8f + MathF.Sin(phase * 2f) * 0.2f;
                    envAnim.TintG = 0.3f + MathF.Sin(phase * 3f) * 0.15f;
                    envAnim.AlphaModifier = 0.85f + MathF.Sin(phase) * 0.15f;
                    break;

                case EnvironmentalAnimationType.TorchFlicker:
                    envAnim.ScaleModifier = 0.9f + MathF.Sin(phase * 8f) * envAnim.Amplitude * 0.1f;
                    envAnim.AlphaModifier = 0.7f + MathF.Sin(phase * 6f + 0.5f) * 0.3f;
                    envAnim.TintR = 1f;
                    envAnim.TintG = 0.7f + MathF.Sin(phase * 4f) * 0.15f;
                    envAnim.TintB = 0.2f + MathF.Sin(phase * 7f) * 0.1f;
                    break;

                case EnvironmentalAnimationType.GrassWave:
                    envAnim.OffsetX = MathF.Sin(phase) * envAnim.Amplitude;
                    envAnim.OffsetY = 0f;
                    envAnim.ScaleModifier = 1f + MathF.Sin(phase * 0.5f) * 0.05f;
                    break;

                case EnvironmentalAnimationType.LeafFall:
                    envAnim.OffsetX = MathF.Sin(phase * 1.5f) * envAnim.Amplitude;
                    envAnim.OffsetY += deltaTime * 20f * envAnim.Speed;
                    envAnim.AlphaModifier = Math.Max(0f, 1f - (envAnim.OffsetY / 200f));
                    if (envAnim.AlphaModifier <= 0f)
                    {
                        envAnim.OffsetY = 0f; // Reset
                    }
                    break;

                case EnvironmentalAnimationType.Sparkle:
                    envAnim.AlphaModifier = MathF.Abs(MathF.Sin(phase * envAnim.Speed));
                    envAnim.ScaleModifier = 0.5f + envAnim.AlphaModifier * 0.5f;
                    break;

                case EnvironmentalAnimationType.Steam:
                    envAnim.OffsetY -= deltaTime * 15f * envAnim.Speed;
                    envAnim.OffsetX = MathF.Sin(phase * 2f) * envAnim.Amplitude * 0.5f;
                    envAnim.AlphaModifier = Math.Max(0f, 1f + (envAnim.OffsetY / 100f));
                    if (envAnim.AlphaModifier <= 0f)
                    {
                        envAnim.OffsetY = 0f;
                    }
                    break;

                case EnvironmentalAnimationType.Smoke:
                    envAnim.OffsetY -= deltaTime * 10f * envAnim.Speed;
                    envAnim.OffsetX = MathF.Sin(phase) * envAnim.Amplitude;
                    envAnim.ScaleModifier = 1f + Math.Abs(envAnim.OffsetY) * 0.005f;
                    envAnim.AlphaModifier = Math.Max(0f, 0.6f + (envAnim.OffsetY / 150f));
                    if (envAnim.AlphaModifier <= 0f)
                    {
                        envAnim.OffsetY = 0f;
                    }
                    break;
            }

            // Render environmental effect overlay
            RenderEnvironmentalEffect(position, envAnim);
        }
    }

    /// <summary>
    /// Render an environmental animation effect
    /// </summary>
    private static void RenderEnvironmentalEffect(PositionComponent position, EnvironmentalAnimationComponent envAnim)
    {
        float x = position.X + envAnim.OffsetX;
        float y = position.Y + envAnim.OffsetY;
        float size = 4f * envAnim.ScaleModifier;
        float alpha = 0.4f * envAnim.AlphaModifier;

        if (alpha <= 0.01f) return;

        EngineInterop.Renderer_DrawRect(
            x - size * 0.5f, y - size * 0.5f,
            size, size,
            envAnim.TintR, envAnim.TintG, envAnim.TintB, alpha
        );
    }

    /// <summary>
    /// Update and render status effect visuals
    /// </summary>
    private void UpdateStatusEffectVisuals(World world, float deltaTime)
    {
        foreach (var entity in world.GetEntitiesWithComponent<StatusEffectVisualComponent>())
        {
            var visuals = world.GetComponent<StatusEffectVisualComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);

            if (visuals == null || position == null)
                continue;

            // Update and render each active visual
            for (int i = visuals.ActiveVisuals.Count - 1; i >= 0; i--)
            {
                var visual = visuals.ActiveVisuals[i];
                visual.Timer += deltaTime;

                if (!visual.IsActive)
                {
                    visuals.ActiveVisuals.RemoveAt(i);
                    continue;
                }

                RenderStatusVisual(position, visual);
            }
        }
    }

    /// <summary>
    /// Render a status effect visual indicator
    /// </summary>
    private void RenderStatusVisual(PositionComponent position, StatusEffectVisualComponent.ActiveStatusVisual visual)
    {
        float pulse = visual.PulseValue;

        switch (visual.VisualType)
        {
            case StatusVisualType.PoisonBubbles:
                RenderBubbleEffect(position, visual, pulse);
                break;

            case StatusVisualType.FireAura:
                RenderAuraEffect(position, visual, pulse);
                break;

            case StatusVisualType.BleedDrops:
                RenderDropEffect(position, visual);
                break;

            case StatusVisualType.IceCrystals:
                RenderCrystalEffect(position, visual, pulse);
                break;

            case StatusVisualType.StunStars:
                RenderStarEffect(position, visual);
                break;
        }
    }

    /// <summary>
    /// Render floating bubble effect (poison)
    /// </summary>
    private void RenderBubbleEffect(PositionComponent position, StatusEffectVisualComponent.ActiveStatusVisual visual, float pulse)
    {
        int bubbles = 3;
        for (int i = 0; i < bubbles; i++)
        {
            float angle = (MathF.PI * 2f / bubbles) * i + visual.Timer * 2f;
            float radius = 15f + pulse * 5f;
            float x = position.X + MathF.Cos(angle) * radius;
            float y = position.Y + MathF.Sin(angle) * radius - visual.Timer * 5f % 20f;
            float size = 3f + pulse * 2f;
            float alpha = visual.Intensity * 0.6f;

            EngineInterop.Renderer_DrawRect(
                x - size * 0.5f, y - size * 0.5f,
                size, size,
                visual.R, visual.G, visual.B, alpha
            );
        }
    }

    /// <summary>
    /// Render pulsing aura effect (fire)
    /// </summary>
    private static void RenderAuraEffect(PositionComponent position, StatusEffectVisualComponent.ActiveStatusVisual visual, float pulse)
    {
        float auraSize = 30f + pulse * 10f;
        float alpha = visual.Intensity * 0.25f * (0.5f + pulse * 0.5f);

        EngineInterop.Renderer_DrawRect(
            position.X - auraSize * 0.5f,
            position.Y - auraSize * 0.5f,
            auraSize, auraSize,
            visual.R, visual.G, visual.B, alpha
        );
    }

    /// <summary>
    /// Render dripping drop effect (bleed)
    /// </summary>
    private void RenderDropEffect(PositionComponent position, StatusEffectVisualComponent.ActiveStatusVisual visual)
    {
        float dropY = (visual.Timer * 30f) % 25f;
        float alpha = visual.Intensity * 0.7f * (1f - dropY / 25f);
        float size = 2f;

        for (int i = 0; i < 2; i++)
        {
            float offsetX = (i == 0) ? -8f : 8f;
            EngineInterop.Renderer_DrawRect(
                position.X + offsetX - size * 0.5f,
                position.Y + dropY - size * 0.5f,
                size, size,
                visual.R, visual.G, visual.B, alpha
            );
        }
    }

    /// <summary>
    /// Render crystal shimmer effect (frozen)
    /// </summary>
    private static void RenderCrystalEffect(PositionComponent position, StatusEffectVisualComponent.ActiveStatusVisual visual, float pulse)
    {
        // Blue tint overlay
        float overlaySize = 28f;
        float overlayAlpha = visual.Intensity * 0.15f * (0.7f + pulse * 0.3f);

        EngineInterop.Renderer_DrawRect(
            position.X - overlaySize * 0.5f,
            position.Y - overlaySize * 0.5f,
            overlaySize, overlaySize,
            visual.R, visual.G, visual.B, overlayAlpha
        );

        // Crystal sparkle points
        int crystals = 4;
        for (int i = 0; i < crystals; i++)
        {
            float angle = (MathF.PI * 2f / crystals) * i + visual.Timer * 0.5f;
            float radius = 12f;
            float x = position.X + MathF.Cos(angle) * radius;
            float y = position.Y + MathF.Sin(angle) * radius;
            float size = 2f * (pulse * 0.5f + 0.5f);
            float alpha = visual.Intensity * 0.8f * pulse;

            EngineInterop.Renderer_DrawRect(
                x - size * 0.5f, y - size * 0.5f,
                size, size,
                1f, 1f, 1f, alpha
            );
        }
    }

    /// <summary>
    /// Render spinning star effect (stun)
    /// </summary>
    private void RenderStarEffect(PositionComponent position, StatusEffectVisualComponent.ActiveStatusVisual visual)
    {
        int stars = 3;
        float orbitRadius = 14f;
        float starSize = 3f;

        for (int i = 0; i < stars; i++)
        {
            float angle = (MathF.PI * 2f / stars) * i + visual.Timer * 4f;
            float x = position.X + MathF.Cos(angle) * orbitRadius;
            float y = position.Y - 20f + MathF.Sin(angle) * orbitRadius * 0.4f;
            float alpha = visual.Intensity * 0.9f;

            EngineInterop.Renderer_DrawRect(
                x - starSize * 0.5f, y - starSize * 0.5f,
                starSize, starSize,
                visual.R, visual.G, visual.B, alpha
            );
        }
    }
}
