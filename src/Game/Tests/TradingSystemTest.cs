using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the Trading system (NPC merchant buy/sell)
/// </summary>
public static class TradingSystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Trading System Test");
        Console.WriteLine("=======================================\n");

        TestBuyFromMerchant();
        TestSellToMerchant();
        TestInsufficientFunds();
        TestInsufficientStock();
        TestReputationPricing();
        TestShopCatalogue();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Trading System Tests Passed");
        Console.WriteLine("=======================================\n");
    }

    // -----------------------------------------------------------------------

    private static World CreateWorld()
    {
        var world = new World();
        return world;
    }

    private static (Entity merchant, Entity player) SetupTrade(World world)
    {
        var merchant = TradingSystem.CreateGeneralsMerchant(world, 5f, 5f);
        var player = world.CreateEntity();
        world.AddComponent(player, new PositionComponent(5f, 5f));
        world.AddComponent(player, new InventoryComponent(40));
        world.AddComponent(player, new CurrencyComponent(200));
        return (merchant, player);
    }

    private static void TestBuyFromMerchant()
    {
        Console.WriteLine("[Test] Buy From Merchant");
        Console.WriteLine("----------------------------------------");

        var world = CreateWorld();
        var (merchant, player) = SetupTrade(world);

        var currency = world.GetComponent<CurrencyComponent>(player)!;
        var inventory = world.GetComponent<InventoryComponent>(player)!;

        int goldBefore = currency.Gold;

        var result = TradingSystem.BuyFromMerchant(world, merchant, player, TileType.Wood, 3);

        System.Diagnostics.Debug.Assert(result == TradeResult.Success,
            $"Expected Success but got {result}");
        System.Diagnostics.Debug.Assert(currency.Gold < goldBefore,
            "Gold should decrease after purchase");
        System.Diagnostics.Debug.Assert(inventory.HasItem(TileType.Wood, 3),
            "Inventory should contain 3 wood after purchase");

        Console.WriteLine($"  Bought 3x Wood. Gold remaining: {currency.Gold}");
        Console.WriteLine("✓ Buy from merchant working\n");
    }

    private static void TestSellToMerchant()
    {
        Console.WriteLine("[Test] Sell To Merchant");
        Console.WriteLine("----------------------------------------");

        var world = CreateWorld();
        var (merchant, player) = SetupTrade(world);

        var currency = world.GetComponent<CurrencyComponent>(player)!;
        var inventory = world.GetComponent<InventoryComponent>(player)!;
        inventory.AddItem(TileType.Stone, 10);

        int goldBefore = currency.Gold;

        var result = TradingSystem.SellToMerchant(world, merchant, player, TileType.Stone, 5);

        System.Diagnostics.Debug.Assert(result == TradeResult.Success,
            $"Expected Success but got {result}");
        System.Diagnostics.Debug.Assert(currency.Gold > goldBefore,
            "Gold should increase after selling");
        System.Diagnostics.Debug.Assert(!inventory.HasItem(TileType.Stone, 10),
            "Inventory should have fewer stones after selling");

        Console.WriteLine($"  Sold 5x Stone. Gold gained: {currency.Gold - goldBefore}");
        Console.WriteLine("✓ Sell to merchant working\n");
    }

    private static void TestInsufficientFunds()
    {
        Console.WriteLine("[Test] Insufficient Funds");
        Console.WriteLine("----------------------------------------");

        var world = CreateWorld();
        var (merchant, player) = SetupTrade(world);

        // Drain all gold
        var currency = world.GetComponent<CurrencyComponent>(player)!;
        currency.RemoveGold(200);

        var result = TradingSystem.BuyFromMerchant(world, merchant, player, TileType.Iron, 1);

        System.Diagnostics.Debug.Assert(result == TradeResult.InsufficientFunds,
            $"Expected InsufficientFunds but got {result}");

        Console.WriteLine("  Correctly rejected purchase with no gold.");
        Console.WriteLine("✓ Insufficient funds handled correctly\n");
    }

    private static void TestInsufficientStock()
    {
        Console.WriteLine("[Test] Insufficient Stock");
        Console.WriteLine("----------------------------------------");

        var world = CreateWorld();
        var (merchant, player) = SetupTrade(world);

        // Try to buy more than the merchant has
        var result = TradingSystem.BuyFromMerchant(world, merchant, player, TileType.Iron, 9999);

        System.Diagnostics.Debug.Assert(result == TradeResult.InsufficientStock,
            $"Expected InsufficientStock but got {result}");

        Console.WriteLine("  Correctly rejected purchase beyond available stock.");
        Console.WriteLine("✓ Insufficient stock handled correctly\n");
    }

    private static void TestReputationPricing()
    {
        Console.WriteLine("[Test] Reputation-based Pricing");
        Console.WriteLine("----------------------------------------");

        var world1 = CreateWorld();
        var (merchant1, player1) = SetupTrade(world1);

        var world2 = CreateWorld();
        var (merchant2, player2) = SetupTrade(world2);

        int goldBefore1 = world1.GetComponent<CurrencyComponent>(player1)!.Gold;
        int goldBefore2 = world2.GetComponent<CurrencyComponent>(player2)!.Gold;

        // Good reputation → cheaper
        TradingSystem.BuyFromMerchant(world1, merchant1, player1, TileType.Wood, 1, reputationModifier: 1f);
        // Bad reputation → more expensive
        TradingSystem.BuyFromMerchant(world2, merchant2, player2, TileType.Wood, 1, reputationModifier: -1f);

        int spent1 = goldBefore1 - world1.GetComponent<CurrencyComponent>(player1)!.Gold;
        int spent2 = goldBefore2 - world2.GetComponent<CurrencyComponent>(player2)!.Gold;

        System.Diagnostics.Debug.Assert(spent1 <= spent2,
            "Good reputation should result in equal or lower prices");

        Console.WriteLine($"  Reputation +1: paid {spent1}g, Reputation -1: paid {spent2}g");
        Console.WriteLine("✓ Reputation pricing working correctly\n");
    }

    private static void TestShopCatalogue()
    {
        Console.WriteLine("[Test] Shop Catalogue Display");
        Console.WriteLine("----------------------------------------");

        var world = CreateWorld();
        var merchant = TradingSystem.CreateGeneralsMerchant(world, 0f, 0f);

        // Just ensure it runs without exception
        TradingSystem.DisplayShopCatalogue(world, merchant);

        var blacksmith = TradingSystem.CreateBlacksmith(world, 10f, 10f);
        TradingSystem.DisplayShopCatalogue(world, blacksmith);

        Console.WriteLine("✓ Shop catalogue display working\n");
    }
}
