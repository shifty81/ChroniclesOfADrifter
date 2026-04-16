using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Result of a buy or sell transaction
/// </summary>
public enum TradeResult
{
    Success,
    InsufficientFunds,
    InsufficientStock,
    InventoryFull,
    ItemNotAvailable,
    InvalidQuantity
}

/// <summary>
/// A single listing in a shop: item type, base price, and current stock.
/// </summary>
public class ShopListing
{
    public TileType ItemType { get; set; }
    public int BasePrice { get; set; }
    public int Stock { get; set; }
    /// <summary>-1 means unlimited stock</summary>
    public int MaxStock { get; set; }
    public float RestockTimer { get; set; }

    public ShopListing(TileType item, int basePrice, int stock = -1)
    {
        ItemType = item;
        BasePrice = basePrice;
        Stock = stock < 0 ? int.MaxValue : stock;
        MaxStock = stock < 0 ? -1 : stock;
        RestockTimer = 0f;
    }

    public bool IsUnlimited => MaxStock < 0;
}

/// <summary>
/// System that manages NPC merchant trading: buying from and selling to the player.
/// Buy prices are pulled from the NPCComponent.ShopPrices dictionary; sell prices are
/// 50 % of buy price by default. A player reputation modifier adjusts all prices.
/// Shops restock periodically.
/// </summary>
public class TradingSystem : ISystem
{
    // How often (in game seconds) a merchant restocks limited items
    private const float RESTOCK_INTERVAL = 120f;
    // Fraction of the buy price that the player receives when selling
    private const float SELL_PRICE_RATIO = 0.5f;

    public void Initialize(World world)
    {
        Console.WriteLine("[Trading] Trading system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        foreach (var entity in world.GetEntitiesWithComponent<NPCComponent>())
        {
            var npc = world.GetComponent<NPCComponent>(entity);
            if (npc == null || npc.Role != NPCRole.Merchant && npc.Role != NPCRole.Blacksmith && npc.Role != NPCRole.Innkeeper)
                continue;

            // Advance restock timers and replenish limited stock
            RestockShop(npc, deltaTime);
        }
    }

    // -----------------------------------------------------------------------
    // Public trade API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Player buys <paramref name="quantity"/> of <paramref name="item"/> from the
    /// given merchant entity. Reputation in range [-1, 1] adjusts prices:
    ///   +1 → 20 % discount, -1 → 20 % surcharge.
    /// </summary>
    public static TradeResult BuyFromMerchant(
        World world,
        Entity merchantEntity,
        Entity playerEntity,
        TileType item,
        int quantity,
        float reputationModifier = 0f)
    {
        if (quantity <= 0) return TradeResult.InvalidQuantity;

        var npc = world.GetComponent<NPCComponent>(merchantEntity);
        var playerInventory = world.GetComponent<InventoryComponent>(playerEntity);
        var playerCurrency = world.GetComponent<CurrencyComponent>(playerEntity);

        if (npc == null || playerInventory == null || playerCurrency == null)
            return TradeResult.ItemNotAvailable;

        if (!npc.ShopPrices.TryGetValue(item, out int basePrice))
            return TradeResult.ItemNotAvailable;

        // Check stock
        if (!npc.ShopInventory.TryGetValue(item, out int stock) || stock < quantity)
            return TradeResult.InsufficientStock;

        int finalPrice = CalculateBuyPrice(basePrice, quantity, reputationModifier);

        if (!playerCurrency.HasGold(finalPrice))
            return TradeResult.InsufficientFunds;

        if (!playerInventory.AddItem(item, quantity))
            return TradeResult.InventoryFull;

        playerCurrency.RemoveGold(finalPrice);
        npc.ShopInventory[item] -= quantity;

        Console.WriteLine($"[Trading] Bought {quantity}x {item} from {npc.Name} for {finalPrice}g");
        return TradeResult.Success;
    }

    /// <summary>
    /// Player sells <paramref name="quantity"/> of <paramref name="item"/> to the
    /// given merchant entity. Sell price is 50 % of the merchant's buy price
    /// (adjusted by reputation).
    /// </summary>
    public static TradeResult SellToMerchant(
        World world,
        Entity merchantEntity,
        Entity playerEntity,
        TileType item,
        int quantity,
        float reputationModifier = 0f)
    {
        if (quantity <= 0) return TradeResult.InvalidQuantity;

        var npc = world.GetComponent<NPCComponent>(merchantEntity);
        var playerInventory = world.GetComponent<InventoryComponent>(playerEntity);
        var playerCurrency = world.GetComponent<CurrencyComponent>(playerEntity);

        if (npc == null || playerInventory == null || playerCurrency == null)
            return TradeResult.ItemNotAvailable;

        if (!playerInventory.HasItem(item, quantity))
            return TradeResult.InsufficientStock;

        // Merchants buy at SELL_PRICE_RATIO of their own sell price; if they
        // don't carry the item they still accept it at a generic rate.
        npc.ShopPrices.TryGetValue(item, out int basePrice);
        if (basePrice == 0) basePrice = 5; // generic floor

        int sellPrice = CalculateSellPrice(basePrice, quantity, reputationModifier);

        playerInventory.RemoveItem(item, quantity);
        playerCurrency.AddGold(sellPrice);

        // The merchant gains stock (capped at reasonable limit)
        if (npc.ShopInventory.ContainsKey(item))
            npc.ShopInventory[item] = Math.Min(npc.ShopInventory[item] + quantity, 999);
        else
            npc.ShopInventory[item] = Math.Min(quantity, 999);

        Console.WriteLine($"[Trading] Sold {quantity}x {item} to {npc.Name} for {sellPrice}g");
        return TradeResult.Success;
    }

    /// <summary>Display the shop catalogue for a merchant NPC.</summary>
    public static void DisplayShopCatalogue(World world, Entity merchantEntity)
    {
        var npc = world.GetComponent<NPCComponent>(merchantEntity);
        if (npc == null) return;

        Console.WriteLine($"\n=== {npc.Name}'s Shop ===");
        Console.WriteLine($"{"Item",-20} {"Price",8} {"Stock",8}");
        Console.WriteLine(new string('-', 40));

        foreach (var (item, price) in npc.ShopPrices)
        {
            int stock = npc.ShopInventory.TryGetValue(item, out int s) ? s : 0;
            string stockStr = stock == int.MaxValue ? "∞" : stock.ToString();
            Console.WriteLine($"{item,-20} {price,7}g {stockStr,8}");
        }

        Console.WriteLine(new string('=', 40) + "\n");
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static int CalculateBuyPrice(int basePrice, int quantity, float reputation)
    {
        // reputation ∈ [-1, 1] → price multiplier ∈ [1.2, 0.8]
        float multiplier = 1f - reputation * 0.2f;
        return Math.Max(1, (int)MathF.Round(basePrice * quantity * multiplier));
    }

    private static int CalculateSellPrice(int basePrice, int quantity, float reputation)
    {
        float multiplier = SELL_PRICE_RATIO + reputation * 0.1f; // up to 60 % with max rep
        multiplier = Math.Clamp(multiplier, 0.1f, 0.9f);
        return Math.Max(1, (int)MathF.Round(basePrice * quantity * multiplier));
    }

    private static void RestockShop(NPCComponent npc, float deltaTime)
    {
        // Use a simple per-NPC restock approach: advance a timer stored in the
        // NPCComponent's CurrentActivity field encoded as a float-in-string (pragmatic).
        // In a real implementation this would be a dedicated field; here we track
        // restock independently per merchant using a static dictionary.
        RestockTimers.TryGetValue(npc, out float timer);
        timer += deltaTime;

        if (timer >= RESTOCK_INTERVAL)
        {
            timer = 0f;
            foreach (var item in npc.ShopPrices.Keys)
            {
                if (!npc.ShopInventory.ContainsKey(item)) continue;
                // Only restock limited items that have a defined max
                if (npc.ShopInventory[item] == int.MaxValue) continue;

                // Restore up to 5 units per restock cycle
                int restockAmt = Math.Min(5, 20 - npc.ShopInventory[item]);
                if (restockAmt > 0)
                    npc.ShopInventory[item] += restockAmt;
            }
        }

        RestockTimers[npc] = timer;
    }

    // Lightweight per-instance restock timer storage
    private static readonly Dictionary<NPCComponent, float> RestockTimers = new();

    // -----------------------------------------------------------------------
    // Factory helper: create a pre-stocked merchant NPC entity
    // -----------------------------------------------------------------------

    /// <summary>
    /// Create a general goods merchant with a default inventory.
    /// </summary>
    public static Entity CreateGeneralsMerchant(World world, float x, float y)
    {
        var entity = world.CreateEntity();
        world.AddComponent(entity, new PositionComponent(x, y));

        var npc = new NPCComponent("Trader", NPCRole.Merchant);

        var stockItems = new[]
        {
            (TileType.Wood,       5,  10),
            (TileType.Stone,      3,  20),
            (TileType.Torch,      8,   5),
            (TileType.Iron,      15,   8),
            (TileType.Coal,       4,  15),
            (TileType.WoodPlank,  6,  12),
            (TileType.Cobblestone,4,  15),
        };

        foreach (var (item, price, stock) in stockItems)
        {
            npc.ShopPrices[item] = price;
            npc.ShopInventory[item] = stock;
        }

        world.AddComponent(entity, npc);
        return entity;
    }

    /// <summary>
    /// Create a blacksmith merchant with tools and ore.
    /// </summary>
    public static Entity CreateBlacksmith(World world, float x, float y)
    {
        var entity = world.CreateEntity();
        world.AddComponent(entity, new PositionComponent(x, y));

        var npc = new NPCComponent("Blacksmith", NPCRole.Blacksmith);

        var stockItems = new[]
        {
            (TileType.Iron,      20,  10),
            (TileType.Gold,      35,   5),
            (TileType.IronOre,   12,  15),
            (TileType.GoldOre,   25,   5),
            (TileType.Coal,       6,  20),
        };

        foreach (var (item, price, stock) in stockItems)
        {
            npc.ShopPrices[item] = price;
            npc.ShopInventory[item] = stock;
        }

        world.AddComponent(entity, npc);
        return entity;
    }
}
