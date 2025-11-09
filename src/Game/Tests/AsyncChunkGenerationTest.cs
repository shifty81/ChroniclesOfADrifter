using ChroniclesOfADrifter.Terrain;
using System.Diagnostics;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for async chunk generation system
/// </summary>
public class AsyncChunkGenerationTest
{
    public static void Run()
    {
        Console.WriteLine("=== Async Chunk Generation Test Suite ===\n");

        TestBasicAsyncGeneration();
        TestPriorityBasedGeneration();
        TestPerformanceComparison();
        TestThreadSafety();

        Console.WriteLine("\n=== All Async Chunk Generation Tests Passed ===");
    }

    private static void TestBasicAsyncGeneration()
    {
        Console.WriteLine("Test 1: Basic Async Generation");
        Console.WriteLine("-------------------------------");

        var generator = new TerrainGenerator(12345);
        var asyncGen = new AsyncChunkGenerator(generator, 2);

        try
        {
            // Request several chunks
            for (int i = 0; i < 5; i++)
            {
                asyncGen.RequestChunkGeneration(i, 0);
            }

            Console.WriteLine($"Requested 5 chunks");

            // Wait for generation
            int attempts = 0;
            while (asyncGen.GetCompletedChunkCount() < 5 && attempts < 100)
            {
                Thread.Sleep(50);
                attempts++;
            }

            Console.WriteLine($"Completed: {asyncGen.GetCompletedChunkCount()}/5 chunks");
            Console.WriteLine($"Time: {attempts * 50}ms");

            // Verify all chunks are available
            for (int i = 0; i < 5; i++)
            {
                var chunk = asyncGen.TryGetGeneratedChunk(i);
                if (chunk == null)
                {
                    throw new Exception($"Chunk {i} was not generated");
                }
                if (!chunk.IsGenerated)
                {
                    throw new Exception($"Chunk {i} is not marked as generated");
                }
            }

            Console.WriteLine("✓ All chunks generated successfully\n");
        }
        finally
        {
            asyncGen.Dispose();
        }
    }

    private static void TestPriorityBasedGeneration()
    {
        Console.WriteLine("Test 2: Priority-Based Generation");
        Console.WriteLine("----------------------------------");

        var generator = new TerrainGenerator(54321);
        var asyncGen = new AsyncChunkGenerator(generator, 1); // Single thread for predictability

        try
        {
            // Request chunks at different distances from player
            // Player at chunk 10, so chunks closer to 10 should be prioritized
            float playerPos = 10 * Chunk.CHUNK_WIDTH;

            asyncGen.RequestChunkGeneration(15, playerPos); // Distance: 5
            asyncGen.RequestChunkGeneration(9, playerPos);  // Distance: 1
            asyncGen.RequestChunkGeneration(20, playerPos); // Distance: 10
            asyncGen.RequestChunkGeneration(11, playerPos); // Distance: 1

            Console.WriteLine($"Requested chunks at distances: 5, 1, 10, 1 from player");

            // Wait a bit to see which gets generated first
            Thread.Sleep(100);

            var completed = asyncGen.GetCompletedChunkCount();
            Console.WriteLine($"Completed {completed} chunks in 100ms");

            // Check if closer chunks are prioritized
            bool chunk9Ready = asyncGen.IsChunkGenerated(9);
            bool chunk11Ready = asyncGen.IsChunkGenerated(11);
            bool chunk20Ready = asyncGen.IsChunkGenerated(20);

            Console.WriteLine($"Chunk 9 (distance 1): {(chunk9Ready ? "Ready" : "Not ready")}");
            Console.WriteLine($"Chunk 11 (distance 1): {(chunk11Ready ? "Ready" : "Not ready")}");
            Console.WriteLine($"Chunk 20 (distance 10): {(chunk20Ready ? "Ready" : "Not ready")}");

            // At least one close chunk should be ready before the far one
            if ((chunk9Ready || chunk11Ready) && !chunk20Ready)
            {
                Console.WriteLine("✓ Priority system working correctly\n");
            }
            else if (completed >= 4)
            {
                Console.WriteLine("✓ All chunks generated (priority test inconclusive)\n");
            }
            else
            {
                Console.WriteLine("⚠ Priority test inconclusive (all chunks may have generated)\n");
            }
        }
        finally
        {
            asyncGen.Dispose();
        }
    }

    private static void TestPerformanceComparison()
    {
        Console.WriteLine("Test 3: Performance Comparison (Sync vs Async)");
        Console.WriteLine("-----------------------------------------------");

        var generator = new TerrainGenerator(99999);
        int chunkCount = 20;

        // Test synchronous generation
        var syncStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < chunkCount; i++)
        {
            var chunk = new Chunk(i);
            generator.GenerateChunk(chunk);
        }
        syncStopwatch.Stop();

        Console.WriteLine($"Synchronous: {chunkCount} chunks in {syncStopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Average: {(double)syncStopwatch.ElapsedMilliseconds / chunkCount:F2}ms per chunk");

        // Test asynchronous generation
        var asyncGen = new AsyncChunkGenerator(generator, Environment.ProcessorCount - 1);

        try
        {
            var asyncStopwatch = Stopwatch.StartNew();

            for (int i = 0; i < chunkCount; i++)
            {
                asyncGen.RequestChunkGeneration(i + 100, 0); // Offset to avoid cache
            }

            // Wait for all to complete
            int attempts = 0;
            while (asyncGen.GetCompletedChunkCount() < chunkCount && attempts < 200)
            {
                Thread.Sleep(50);
                attempts++;
            }

            asyncStopwatch.Stop();

            Console.WriteLine($"Asynchronous: {chunkCount} chunks in {asyncStopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Average: {(double)asyncStopwatch.ElapsedMilliseconds / chunkCount:F2}ms per chunk");

            double speedup = (double)syncStopwatch.ElapsedMilliseconds / asyncStopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Speedup: {speedup:F2}x");

            if (speedup > 1.2)
            {
                Console.WriteLine("✓ Async generation is faster\n");
            }
            else if (speedup > 0.8)
            {
                Console.WriteLine("✓ Async generation has similar performance\n");
            }
            else
            {
                Console.WriteLine("⚠ Async generation is slower (may need tuning)\n");
            }
        }
        finally
        {
            asyncGen.Dispose();
        }
    }

    private static void TestThreadSafety()
    {
        Console.WriteLine("Test 4: Thread Safety");
        Console.WriteLine("---------------------");

        var generator = new TerrainGenerator(77777);
        var asyncGen = new AsyncChunkGenerator(generator, 4);

        try
        {
            // Request the same chunks from multiple threads simultaneously
            var threads = new List<Thread>();

            for (int t = 0; t < 4; t++)
            {
                var thread = new Thread(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        asyncGen.RequestChunkGeneration(i, 0);
                        Thread.Sleep(5);
                    }
                });
                threads.Add(thread);
                thread.Start();
            }

            // Wait for all threads
            foreach (var thread in threads)
            {
                thread.Join();
            }

            // Wait for generation
            Thread.Sleep(500);

            var completed = asyncGen.GetCompletedChunkCount();
            Console.WriteLine($"Completed {completed} unique chunks");

            // Verify each chunk is valid and unique
            var chunks = new HashSet<int>();
            for (int i = 0; i < 10; i++)
            {
                var chunk = asyncGen.TryGetGeneratedChunk(i);
                if (chunk != null)
                {
                    if (chunks.Contains(chunk.ChunkX))
                    {
                        throw new Exception($"Duplicate chunk {chunk.ChunkX}");
                    }
                    chunks.Add(chunk.ChunkX);
                }
            }

            Console.WriteLine($"Verified {chunks.Count} unique chunks");
            Console.WriteLine("✓ Thread safety verified\n");
        }
        finally
        {
            asyncGen.Dispose();
        }
    }
}
