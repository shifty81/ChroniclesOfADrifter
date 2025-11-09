using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.UI;

namespace ChroniclesOfADrifter.UI;

/// <summary>
/// UI for displaying crafting recipes and crafting items
/// </summary>
public class CraftingUI : UIPanel
{
    private const float RECIPE_ITEM_HEIGHT = 60f;
    private const float RECIPE_ITEM_PADDING = 5f;
    private const float HEADER_HEIGHT = 40f;
    private const float BUTTON_WIDTH = 80f;
    private const float BUTTON_HEIGHT = 30f;
    
    private CraftingSystem? _craftingSystem;
    private InventoryComponent? _inventory;
    private List<UIRecipeItem> _recipeItems = new();
    private string _selectedCategory = "All";
    
    public CraftingUI(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        
        // Dark semi-transparent background
        BackgroundR = 0.1f;
        BackgroundG = 0.1f;
        BackgroundB = 0.15f;
        BackgroundA = 0.95f;
        
        // Lighter border
        BorderR = 0.5f;
        BorderG = 0.5f;
        BorderB = 0.5f;
        BorderA = 1.0f;
        BorderThickness = 3f;
    }
    
    /// <summary>
    /// Set the crafting system and inventory
    /// </summary>
    public void SetCraftingData(CraftingSystem craftingSystem, InventoryComponent inventory)
    {
        _craftingSystem = craftingSystem;
        _inventory = inventory;
        RefreshRecipes();
    }
    
    /// <summary>
    /// Refresh the list of recipes
    /// </summary>
    public void RefreshRecipes()
    {
        if (_craftingSystem == null || _inventory == null)
            return;
        
        // Clear existing recipe items
        foreach (var item in _recipeItems)
        {
            RemoveChild(item);
        }
        _recipeItems.Clear();
        
        // Get recipes to display
        var recipes = _selectedCategory == "All" 
            ? _craftingSystem.GetAllRecipes().ToList()
            : _craftingSystem.GetRecipesByCategory(_selectedCategory);
        
        // Create UI elements for each recipe
        float yOffset = HEADER_HEIGHT + RECIPE_ITEM_PADDING;
        
        foreach (var recipe in recipes)
        {
            var recipeItem = new UIRecipeItem(recipe, _craftingSystem, _inventory, this)
            {
                X = RECIPE_ITEM_PADDING * 2,
                Y = yOffset,
                Width = Width - RECIPE_ITEM_PADDING * 4,
                Height = RECIPE_ITEM_HEIGHT
            };
            
            _recipeItems.Add(recipeItem);
            AddChild(recipeItem);
            
            yOffset += RECIPE_ITEM_HEIGHT + RECIPE_ITEM_PADDING;
        }
    }
    
    /// <summary>
    /// Set the selected recipe category filter
    /// </summary>
    public void SetCategory(string category)
    {
        _selectedCategory = category;
        RefreshRecipes();
    }
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        
        // Periodically refresh to update craftable status
        RefreshRecipes();
    }
}

/// <summary>
/// UI element representing a single craftable recipe
/// </summary>
public class UIRecipeItem : UIElement
{
    private CraftingRecipe _recipe;
    private CraftingSystem _craftingSystem;
    private InventoryComponent _inventory;
    private CraftingUI _parentUI;
    private UIButton _craftButton;
    private bool _isHovered = false;
    
    public UIRecipeItem(CraftingRecipe recipe, CraftingSystem craftingSystem, 
                        InventoryComponent inventory, CraftingUI parentUI)
    {
        _recipe = recipe;
        _craftingSystem = craftingSystem;
        _inventory = inventory;
        _parentUI = parentUI;
        
        // Create craft button
        _craftButton = new UIButton
        {
            X = Width - 90f,
            Y = (Height - 30f) / 2,
            Width = 80f,
            Height = 30f,
            Text = "Craft",
            OnClickAction = OnCraftButtonClick
        };
        AddChild(_craftButton);
    }
    
    private void OnCraftButtonClick()
    {
        if (_craftingSystem.TryCraft(_recipe, _inventory))
        {
            Console.WriteLine($"Successfully crafted {_recipe.Name}!");
            _parentUI.RefreshRecipes();
        }
        else
        {
            Console.WriteLine($"Failed to craft {_recipe.Name}");
        }
    }
    
    public override void OnMouseEnter()
    {
        _isHovered = true;
    }
    
    public override void OnMouseExit()
    {
        _isHovered = false;
    }
    
    protected override void OnRender()
    {
        float absX = GetAbsoluteX();
        float absY = GetAbsoluteY();
        
        bool canCraft = _craftingSystem.CanCraft(_recipe, _inventory);
        
        // Background color based on craftable status
        float bgR = _isHovered ? 0.25f : 0.2f;
        float bgG = canCraft ? (_isHovered ? 0.3f : 0.25f) : (_isHovered ? 0.25f : 0.2f);
        float bgB = _isHovered ? 0.25f : 0.2f;
        
        ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
            absX, absY, Width, Height, bgR, bgG, bgB, 1.0f);
        
        // Border
        float borderSize = 1f;
        ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
            absX, absY, Width, borderSize, 0.4f, 0.4f, 0.4f, 1.0f);
        ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
            absX + Width - borderSize, absY, borderSize, Height, 0.4f, 0.4f, 0.4f, 1.0f);
        ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
            absX, absY + Height - borderSize, Width, borderSize, 0.4f, 0.4f, 0.4f, 1.0f);
        ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
            absX, absY, borderSize, Height, 0.4f, 0.4f, 0.4f, 1.0f);
        
        // Output item representation (left side)
        float iconSize = 40f;
        float iconX = absX + 10f;
        float iconY = absY + (Height - iconSize) / 2;
        
        // Get color for output item
        var (itemR, itemG, itemB) = GetItemColor(_recipe.Output);
        ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
            iconX, iconY, iconSize, iconSize, itemR, itemG, itemB, 1.0f);
        
        // Draw ingredients (middle section)
        float ingredientX = absX + 70f;
        float ingredientY = absY + 10f;
        float ingredientSize = 15f;
        float ingredientSpacing = 20f;
        
        int ingredientIndex = 0;
        foreach (var ingredient in _recipe.Ingredients)
        {
            var (ingR, ingG, ingB) = GetItemColor(ingredient.Key);
            bool hasEnough = _inventory.GetItemCount(ingredient.Key) >= ingredient.Value;
            
            // Dim the color if we don't have enough
            if (!hasEnough)
            {
                ingR *= 0.5f;
                ingG *= 0.5f;
                ingB *= 0.5f;
            }
            
            ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
                ingredientX + ingredientIndex * ingredientSpacing, 
                ingredientY, 
                ingredientSize, ingredientSize, 
                ingR, ingG, ingB, 1.0f);
            
            ingredientIndex++;
        }
        
        // Enable/disable craft button based on craftable status
        _craftButton.IsEnabled = canCraft;
        
        // TODO: Draw recipe name and ingredient counts when text rendering is available
    }
    
    private static (float r, float g, float b) GetItemColor(TileType itemType)
    {
        return itemType switch
        {
            TileType.Wood or TileType.WoodPlank => (0.6f, 0.4f, 0.2f),  // Brown
            TileType.Stone or TileType.Cobblestone => (0.5f, 0.5f, 0.5f),  // Gray
            TileType.Dirt => (0.4f, 0.3f, 0.2f),  // Dark brown
            TileType.Iron or TileType.IronOre => (0.7f, 0.7f, 0.7f),  // Light gray
            TileType.Gold or TileType.GoldOre => (1.0f, 0.84f, 0.0f),  // Gold
            TileType.Coal or TileType.CoalOre => (0.2f, 0.2f, 0.2f),  // Black
            TileType.CopperOre => (0.8f, 0.5f, 0.3f),  // Copper
            TileType.Brick => (0.7f, 0.3f, 0.2f),  // Red-brown
            TileType.Torch => (1.0f, 0.9f, 0.3f),  // Yellow
            TileType.TreeOak or TileType.TreePine or TileType.TreePalm => (0.3f, 0.6f, 0.3f),  // Green
            _ => (0.5f, 0.5f, 0.5f)  // Default gray
        };
    }
}
