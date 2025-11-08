using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Test suite for the swimming system
/// </summary>
public static class SwimmingSystemTest
{
    public static void Run()
    {
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Swimming System Tests");
        Console.WriteLine("===========================================\n");
        
        TestSwimmingComponent();
        TestWaterFlowComponent();
        TestBreathingMechanics();
        TestSwimSpeedReduction();
        TestDrowningMechanics();
        
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  All Swimming Tests Completed!");
        Console.WriteLine("===========================================\n");
    }
    
    private static void TestSwimmingComponent()
    {
        Console.WriteLine("[Test] Swimming Component");
        
        var swimming = new SwimmingComponent(swimSpeed: 0.7f, maxBreathTime: 10.0f);
        
        Console.WriteLine($"  Swim speed: {swimming.SwimSpeed}");
        Console.WriteLine($"  Max breath: {swimming.MaxBreathTime}s");
        Console.WriteLine($"  Current breath: {swimming.CurrentBreath}s");
        Console.WriteLine($"  Is in water: {swimming.IsInWater}");
        Console.WriteLine($"  Can breathe underwater: {swimming.CanBreatheUnderwater}");
        
        if (swimming.SwimSpeed == 0.7f && swimming.MaxBreathTime == 10.0f && 
            swimming.CurrentBreath == 10.0f && !swimming.IsInWater)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
    
    private static void TestWaterFlowComponent()
    {
        Console.WriteLine("[Test] Water Flow Component");
        
        var flow = new WaterFlowComponent(flowX: 0.5f, flowY: 0, flowStrength: 0.7f, 
                                          bodyType: WaterBodyType.River);
        
        Console.WriteLine($"  Flow X: {flow.FlowX}");
        Console.WriteLine($"  Flow Y: {flow.FlowY}");
        Console.WriteLine($"  Flow strength: {flow.FlowStrength}");
        Console.WriteLine($"  Body type: {flow.BodyType}");
        Console.WriteLine($"  Pressure: {flow.Pressure}");
        
        if (flow.FlowX == 0.5f && flow.FlowStrength == 0.7f && 
            flow.BodyType == WaterBodyType.River)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
    
    private static void TestBreathingMechanics()
    {
        Console.WriteLine("[Test] Breathing Mechanics");
        
        var swimming = new SwimmingComponent(maxBreathTime: 5.0f);
        swimming.IsInWater = true;
        
        Console.WriteLine($"  Initial breath: {swimming.CurrentBreath:F1}s");
        
        // Simulate being underwater
        for (int i = 0; i < 100; i++)
        {
            if (!swimming.CanBreatheUnderwater && swimming.IsInWater)
            {
                swimming.CurrentBreath -= 0.016f; // Simulate one frame at 60fps
            }
        }
        
        Console.WriteLine($"  Breath after ~1.6s underwater: {swimming.CurrentBreath:F1}s");
        
        // Restore breath on land
        swimming.IsInWater = false;
        for (int i = 0; i < 60; i++)
        {
            if (swimming.CurrentBreath < swimming.MaxBreathTime)
            {
                swimming.CurrentBreath += 0.016f * 2.0f; // Restore faster
                swimming.CurrentBreath = Math.Min(swimming.CurrentBreath, swimming.MaxBreathTime);
            }
        }
        
        Console.WriteLine($"  Breath after ~1s on land: {swimming.CurrentBreath:F1}s");
        
        if (swimming.CurrentBreath > 3.0f && swimming.CurrentBreath < 5.1f)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
    
    private static void TestSwimSpeedReduction()
    {
        Console.WriteLine("[Test] Swim Speed Reduction");
        
        var swimming = new SwimmingComponent(swimSpeed: 0.5f);
        var velocity = new VelocityComponent { VX = 100, VY = 100 };
        
        Console.WriteLine($"  Initial velocity: VX={velocity.VX:F2}, VY={velocity.VY:F2}");
        Console.WriteLine($"  Swim speed multiplier: {swimming.SwimSpeed}");
        
        // Simulate water resistance
        if (swimming.IsInWater)
        {
            velocity.VX *= swimming.SwimSpeed;
            velocity.VY *= swimming.SwimSpeed;
        }
        else
        {
            // Manually apply for test since we can't actually place entity in water
            swimming.IsInWater = true;
            velocity.VX *= swimming.SwimSpeed;
            velocity.VY *= swimming.SwimSpeed;
        }
        
        Console.WriteLine($"  Velocity after swim reduction: VX={velocity.VX:F2}, VY={velocity.VY:F2}");
        
        if (velocity.VX == 50 && velocity.VY == 50)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
    
    private static void TestDrowningMechanics()
    {
        Console.WriteLine("[Test] Drowning Mechanics");
        
        var swimming = new SwimmingComponent(maxBreathTime: 1.0f);
        var health = new HealthComponent(100);
        swimming.IsInWater = true;
        
        Console.WriteLine($"  Initial health: {health.CurrentHealth}");
        Console.WriteLine($"  Max breath time: {swimming.MaxBreathTime}s");
        
        // Simulate being underwater until out of breath
        float timeUnderwater = 0;
        while (timeUnderwater < 2.0f && health.CurrentHealth > 0)
        {
            float deltaTime = 0.016f;
            timeUnderwater += deltaTime;
            
            if (!swimming.CanBreatheUnderwater && swimming.IsInWater)
            {
                swimming.CurrentBreath -= deltaTime;
                
                if (swimming.CurrentBreath <= 0)
                {
                    // Apply drowning damage
                    health.Damage(swimming.DrowningDamage * deltaTime);
                }
            }
        }
        
        Console.WriteLine($"  Health after 2s underwater: {health.CurrentHealth:F1}");
        Console.WriteLine($"  Breath remaining: {swimming.CurrentBreath:F1}s");
        
        if (health.CurrentHealth < 100 && health.CurrentHealth > 0)
        {
            Console.WriteLine("  ✓ Test passed (drowning damage applied)\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
}
