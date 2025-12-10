#r "src/Game/bin/Release/net9.0/ChroniclesOfADrifter.dll"

using ChroniclesOfADrifter.Tests;

try
{
    CustomizationEnhancementTests.RunAllTests();
    Console.WriteLine("\n✅ ALL TESTS PASSED!\n");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ TEST FAILED: {ex.Message}\n");
    Console.WriteLine(ex.StackTrace);
}
