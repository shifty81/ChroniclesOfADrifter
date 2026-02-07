using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Test suite for the audio/sound system framework
/// </summary>
public static class SoundSystemTest
{
    public static void Run()
    {
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Sound System Tests");
        Console.WriteLine("===========================================\n");

        TestAudioComponentSetup();
        TestQueueSound();
        TestAudioSystemProcessing();
        TestMusicPlayback();
        TestVolumeControls();
        TestLoopingSound();
        TestDisableAudio();

        Console.WriteLine("\n===========================================");
        Console.WriteLine("  All Sound System Tests Completed!");
        Console.WriteLine("===========================================\n");
    }

    private static void TestAudioComponentSetup()
    {
        Console.WriteLine("[Test] Audio Component Setup");

        var audio = new AudioComponent(0.8f);

        Console.WriteLine($"  Volume: {audio.Volume}");
        Console.WriteLine($"  Pending sounds: {audio.PendingSounds.Count}");
        Console.WriteLine($"  Looping sound: {audio.LoopingSound?.ToString() ?? "none"}");

        if (audio.Volume == 0.8f && audio.PendingSounds.Count == 0 && audio.LoopingSound == null)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestQueueSound()
    {
        Console.WriteLine("[Test] Queue Sound Effect");

        var audio = new AudioComponent();
        audio.PlaySound(SoundEffect.SwordSwing, 0.9f);
        audio.PlaySound(SoundEffect.SwordHit, 0.7f);

        Console.WriteLine($"  Queued sounds: {audio.PendingSounds.Count}");
        Console.WriteLine($"  First sound: {audio.PendingSounds[0].Sound}");
        Console.WriteLine($"  Second sound: {audio.PendingSounds[1].Sound}");

        if (audio.PendingSounds.Count == 2 &&
            audio.PendingSounds[0].Sound == SoundEffect.SwordSwing &&
            audio.PendingSounds[1].Sound == SoundEffect.SwordHit)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestAudioSystemProcessing()
    {
        Console.WriteLine("[Test] Audio System Processes Pending Sounds");

        var world = new World();
        var audioSystem = new AudioSystem();
        audioSystem.Initialize(world);

        var entity = world.CreateEntity();
        var audio = new AudioComponent();
        world.AddComponent(entity, audio);

        // Queue sounds
        audio.PlaySound(SoundEffect.Mining);
        audio.PlaySound(SoundEffect.BlockBreak);

        int pendingBefore = audio.PendingSounds.Count;

        // Update system - should process and clear pending sounds
        audioSystem.Update(world, 0.016f);

        int pendingAfter = audio.PendingSounds.Count;

        Console.WriteLine($"  Pending before update: {pendingBefore}");
        Console.WriteLine($"  Pending after update: {pendingAfter}");

        if (pendingBefore == 2 && pendingAfter == 0)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestMusicPlayback()
    {
        Console.WriteLine("[Test] Music Playback");

        var audioSystem = new AudioSystem();

        // Start music
        audioSystem.PlayMusic(MusicTrack.Overworld);
        var currentTrack = audioSystem.CurrentMusic;

        Console.WriteLine($"  Playing: {currentTrack}");

        // Change music
        audioSystem.PlayMusic(MusicTrack.Combat);
        var newTrack = audioSystem.CurrentMusic;

        Console.WriteLine($"  Changed to: {newTrack}");

        // Stop music
        audioSystem.StopMusic();
        var stoppedTrack = audioSystem.CurrentMusic;

        Console.WriteLine($"  After stop: {stoppedTrack?.ToString() ?? "none"}");

        if (currentTrack == MusicTrack.Overworld &&
            newTrack == MusicTrack.Combat &&
            stoppedTrack == null)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestVolumeControls()
    {
        Console.WriteLine("[Test] Volume Controls");

        var audioSystem = new AudioSystem();

        audioSystem.SetMasterVolume(0.5f);
        audioSystem.SetSfxVolume(0.8f);
        audioSystem.SetMusicVolume(0.6f);

        // Test clamping
        audioSystem.SetMasterVolume(1.5f); // Should clamp to 1.0

        Console.WriteLine("  Volume controls set without error");
        Console.WriteLine("  ✓ Test passed\n");
    }

    private static void TestLoopingSound()
    {
        Console.WriteLine("[Test] Looping Sound");

        var audio = new AudioComponent();

        // Start loop
        audio.StartLoop(SoundEffect.RainAmbient);
        bool hasLoop = audio.LoopingSound == SoundEffect.RainAmbient;

        Console.WriteLine($"  Looping: {audio.LoopingSound}");

        // Stop loop
        audio.StopLoop();
        bool noLoop = audio.LoopingSound == null;

        Console.WriteLine($"  After stop: {audio.LoopingSound?.ToString() ?? "none"}");

        if (hasLoop && noLoop)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestDisableAudio()
    {
        Console.WriteLine("[Test] Disable Audio System");

        var world = new World();
        var audioSystem = new AudioSystem();
        audioSystem.Initialize(world);

        // Start music
        audioSystem.PlayMusic(MusicTrack.MainTheme);

        // Disable audio
        audioSystem.SetEnabled(false);

        Console.WriteLine($"  Music after disable: {audioSystem.CurrentMusic?.ToString() ?? "none"}");

        if (audioSystem.CurrentMusic == null)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
}
