using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for animation enhancement features: attack animations,
/// environmental animations, and status effect visuals
/// </summary>
public static class AnimationEnhancementTest
{
    public static void Run()
    {
        Console.WriteLine("=== Animation Enhancement Tests ===\n");

        TestAttackAnimationComponent();
        TestMeleeSwingAnimation();
        TestSpellCastAnimation();
        TestAttackAnimationProgress();
        TestEnvironmentalAnimationCreation();
        TestWaterRippleAnimation();
        TestTorchFlickerAnimation();
        TestStatusEffectVisuals();
        TestPoisonVisual();
        TestMultipleStatusVisuals();
        TestStatusVisualRemoval();

        Console.WriteLine("\n=== Animation Enhancement Tests Complete ===");
    }

    private static void TestAttackAnimationComponent()
    {
        Console.Write("Test: Attack animation component creation... ");
        var anim = new AttackAnimationComponent();

        bool pass = anim.CurrentAttack == AttackAnimationType.None
                 && !anim.IsPlaying
                 && anim.Progress == 0f
                 && anim.AttackReach == 40f;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestMeleeSwingAnimation()
    {
        Console.Write("Test: Melee swing animation playback... ");
        var anim = new AttackAnimationComponent();
        anim.PlayMeleeSwing(0f, 0.25f);

        bool pass = anim.IsPlaying
                 && anim.CurrentAttack == AttackAnimationType.MeleeSwing
                 && anim.Duration == 0.25f
                 && anim.ElapsedTime == 0f;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestSpellCastAnimation()
    {
        Console.Write("Test: Spell cast animation playback... ");
        var anim = new AttackAnimationComponent();
        anim.PlaySpellCast(MathF.PI * 0.5f, 0.5f);

        bool pass = anim.IsPlaying
                 && anim.CurrentAttack == AttackAnimationType.SpellCast
                 && anim.Duration == 0.5f
                 && anim.EffectR < 0.5f  // Blue spell color
                 && anim.EffectB > 0.9f;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestAttackAnimationProgress()
    {
        Console.Write("Test: Attack animation progress tracking... ");
        var anim = new AttackAnimationComponent();
        anim.PlayMeleeSwing(0f, 1.0f);

        float progressStart = anim.Progress;
        anim.ElapsedTime = 0.5f;
        float progressMid = anim.Progress;
        anim.ElapsedTime = 1.0f;
        float progressEnd = anim.Progress;

        bool pass = progressStart == 0f
                 && Math.Abs(progressMid - 0.5f) < 0.01f
                 && Math.Abs(progressEnd - 1.0f) < 0.01f;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestEnvironmentalAnimationCreation()
    {
        Console.Write("Test: Environmental animation creation... ");
        var envAnim = new EnvironmentalAnimationComponent();

        bool pass = envAnim.AnimType == EnvironmentalAnimationType.None
                 && envAnim.Speed == 1f
                 && envAnim.Amplitude == 1f
                 && envAnim.IsActive
                 && envAnim.ScaleModifier == 1f
                 && envAnim.AlphaModifier == 1f;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestWaterRippleAnimation()
    {
        Console.Write("Test: Water ripple preset configuration... ");
        var water = EnvironmentalAnimationComponent.CreateWaterRipple();

        bool pass = water.AnimType == EnvironmentalAnimationType.WaterRipple
                 && water.Speed == 2f
                 && water.Amplitude == 2f
                 && water.TintB > water.TintR  // Blue-ish tint
                 && water.IsActive;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestTorchFlickerAnimation()
    {
        Console.Write("Test: Torch flicker preset configuration... ");
        var torch = EnvironmentalAnimationComponent.CreateTorchFlicker();

        bool pass = torch.AnimType == EnvironmentalAnimationType.TorchFlicker
                 && torch.Speed == 8f  // Fast flicker
                 && torch.TintR > torch.TintG  // Orange-ish
                 && torch.TintG > torch.TintB;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestStatusEffectVisuals()
    {
        Console.Write("Test: Status effect visual component creation... ");
        var visuals = new StatusEffectVisualComponent();

        bool pass = visuals.ActiveVisuals.Count == 0;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestPoisonVisual()
    {
        Console.Write("Test: Poison visual effect application... ");
        var visuals = new StatusEffectVisualComponent();
        visuals.AddPoisonVisual(5f);

        bool pass = visuals.ActiveVisuals.Count == 1
                 && visuals.HasVisual(StatusVisualType.PoisonBubbles)
                 && visuals.ActiveVisuals[0].R < 0.5f  // Green
                 && visuals.ActiveVisuals[0].G > 0.5f
                 && visuals.ActiveVisuals[0].Duration == 5f;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestMultipleStatusVisuals()
    {
        Console.Write("Test: Multiple status effect visuals... ");
        var visuals = new StatusEffectVisualComponent();
        visuals.AddPoisonVisual(5f);
        visuals.AddFireVisual(3f);
        visuals.AddFrozenVisual(4f);

        bool pass = visuals.ActiveVisuals.Count == 3
                 && visuals.HasVisual(StatusVisualType.PoisonBubbles)
                 && visuals.HasVisual(StatusVisualType.FireAura)
                 && visuals.HasVisual(StatusVisualType.IceCrystals);

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestStatusVisualRemoval()
    {
        Console.Write("Test: Status visual removal and clear... ");
        var visuals = new StatusEffectVisualComponent();
        visuals.AddPoisonVisual(5f);
        visuals.AddFireVisual(3f);
        visuals.AddStunVisual(2f);

        visuals.RemoveVisual(StatusVisualType.FireAura);
        bool afterRemove = visuals.ActiveVisuals.Count == 2
                        && !visuals.HasVisual(StatusVisualType.FireAura);

        visuals.ClearAll();
        bool afterClear = visuals.ActiveVisuals.Count == 0;

        bool pass = afterRemove && afterClear;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }
}
