using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Character creator demo scene - demonstrates character customization system
/// </summary>
public class CharacterCreatorScene : Scene
{
    private Entity _previewCharacter;
    private CharacterCreatorComponent? _creator;
    private int _currentSkinToneIndex = 2; // medium
    private int _currentHairStyleIndex = 0; // short
    private int _currentBodyTypeIndex = 1; // average
    private int _currentColorPaletteIndex = 0;
    
    public override void OnLoad()
    {
        Console.WriteLine("[CharacterCreator] Loading character creator scene...");
        
        // Add systems
        World.AddSystem(new AnimationSystem());
        World.AddSystem(new CharacterRenderingSystem());
        World.AddSystem(new RenderingSystem());
        
        // Create character creator component
        var creatorEntity = World.CreateEntity();
        _creator = new CharacterCreatorComponent();
        World.AddComponent(creatorEntity, _creator);
        
        // Create preview character entity
        _previewCharacter = World.CreateEntity();
        
        // Add position (center of screen)
        World.AddComponent(_previewCharacter, new PositionComponent(960, 540));
        
        // Add appearance component with default settings
        var appearance = new CharacterAppearanceComponent
        {
            SkinTone = _creator.AvailableSkinTones[_currentSkinToneIndex],
            HairStyle = _creator.AvailableHairStyles[_currentHairStyleIndex],
            HairColor = new Color(100, 70, 40, 255),
            EyeColor = new Color(70, 130, 180, 255),
            BodyType = _creator.AvailableBodyTypes[_currentBodyTypeIndex]
        };
        
        // Add default clothing layers
        var shirtPalette = _creator.ColorPalettes[_currentColorPaletteIndex];
        appearance.ClothingLayers["shirt"] = new ClothingLayer(
            "shirt", 
            "tunic", 
            shirtPalette.PrimaryColor, 
            shirtPalette.SecondaryColor, 
            renderOrder: 1
        );
        
        appearance.ClothingLayers["pants"] = new ClothingLayer(
            "pants", 
            "trousers", 
            new Color(50, 50, 50, 255), 
            new Color(30, 30, 30, 255), 
            renderOrder: 0
        );
        
        appearance.ClothingLayers["boots"] = new ClothingLayer(
            "boots", 
            "leather", 
            new Color(80, 50, 20, 255), 
            new Color(60, 40, 15, 255), 
            renderOrder: 0
        );
        
        World.AddComponent(_previewCharacter, appearance);
        
        // Add animation components
        var animatedSprite = new AnimatedSpriteComponent(0, 64, 64)
        {
            Scale = 2.0f // High resolution sprite at 2x scale
        };
        
        // Define idle animation (example with 4 frames)
        animatedSprite.Animations["idle"] = new AnimationDefinition(
            frameIndices: new[] { 0, 1, 2, 3 },
            framesPerRow: 8,
            frameWidth: 64,
            frameHeight: 64
        );
        
        // Define walk animation (example with 8 frames)
        animatedSprite.Animations["walk"] = new AnimationDefinition(
            frameIndices: new[] { 8, 9, 10, 11, 12, 13, 14, 15 },
            framesPerRow: 8,
            frameWidth: 64,
            frameHeight: 64
        );
        
        World.AddComponent(_previewCharacter, animatedSprite);
        
        var animation = new AnimationComponent("idle", frameDuration: 0.15f, loop: true);
        World.AddComponent(_previewCharacter, animation);
        
        // Add a basic sprite component as fallback
        World.AddComponent(_previewCharacter, new SpriteComponent(0, 64, 64));
        
        Console.WriteLine("[CharacterCreator] Character creator loaded!");
        PrintCustomizationHelp();
    }
    
    private void PrintCustomizationHelp()
    {
        Console.WriteLine("\n=== Character Customization ===");
        Console.WriteLine("Controls:");
        Console.WriteLine("  1/2 - Change skin tone");
        Console.WriteLine("  3/4 - Change hair style");
        Console.WriteLine("  5/6 - Change body type");
        Console.WriteLine("  7/8 - Change clothing color palette");
        Console.WriteLine("  A   - Toggle armor (shows/hides clothing)");
        Console.WriteLine("  W   - Play walk animation");
        Console.WriteLine("  I   - Play idle animation");
        Console.WriteLine("  Q   - Quit");
        Console.WriteLine("================================\n");
    }
    
    public override void Update(float deltaTime)
    {
        // Handle customization input
        HandleCustomizationInput();
        
        // Update all systems
        base.Update(deltaTime);
    }
    
    private void HandleCustomizationInput()
    {
        if (_creator == null)
            return;
        
        var appearance = World.GetComponent<CharacterAppearanceComponent>(_previewCharacter);
        if (appearance == null)
            return;
        
        // Skin tone controls (1/2)
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true).Key;
            
            switch (key)
            {
                case ConsoleKey.D1:
                    _currentSkinToneIndex = (_currentSkinToneIndex - 1 + _creator.AvailableSkinTones.Count) % _creator.AvailableSkinTones.Count;
                    appearance.SkinTone = _creator.AvailableSkinTones[_currentSkinToneIndex];
                    Console.WriteLine($"[CharacterCreator] Skin tone: {appearance.SkinTone}");
                    break;
                    
                case ConsoleKey.D2:
                    _currentSkinToneIndex = (_currentSkinToneIndex + 1) % _creator.AvailableSkinTones.Count;
                    appearance.SkinTone = _creator.AvailableSkinTones[_currentSkinToneIndex];
                    Console.WriteLine($"[CharacterCreator] Skin tone: {appearance.SkinTone}");
                    break;
                    
                case ConsoleKey.D3:
                    _currentHairStyleIndex = (_currentHairStyleIndex - 1 + _creator.AvailableHairStyles.Count) % _creator.AvailableHairStyles.Count;
                    appearance.HairStyle = _creator.AvailableHairStyles[_currentHairStyleIndex];
                    Console.WriteLine($"[CharacterCreator] Hair style: {appearance.HairStyle}");
                    break;
                    
                case ConsoleKey.D4:
                    _currentHairStyleIndex = (_currentHairStyleIndex + 1) % _creator.AvailableHairStyles.Count;
                    appearance.HairStyle = _creator.AvailableHairStyles[_currentHairStyleIndex];
                    Console.WriteLine($"[CharacterCreator] Hair style: {appearance.HairStyle}");
                    break;
                    
                case ConsoleKey.D5:
                    _currentBodyTypeIndex = (_currentBodyTypeIndex - 1 + _creator.AvailableBodyTypes.Count) % _creator.AvailableBodyTypes.Count;
                    appearance.BodyType = _creator.AvailableBodyTypes[_currentBodyTypeIndex];
                    Console.WriteLine($"[CharacterCreator] Body type: {appearance.BodyType}");
                    break;
                    
                case ConsoleKey.D6:
                    _currentBodyTypeIndex = (_currentBodyTypeIndex + 1) % _creator.AvailableBodyTypes.Count;
                    appearance.BodyType = _creator.AvailableBodyTypes[_currentBodyTypeIndex];
                    Console.WriteLine($"[CharacterCreator] Body type: {appearance.BodyType}");
                    break;
                    
                case ConsoleKey.D7:
                    _currentColorPaletteIndex = (_currentColorPaletteIndex - 1 + _creator.ColorPalettes.Count) % _creator.ColorPalettes.Count;
                    ApplyColorPalette(appearance, _creator.ColorPalettes[_currentColorPaletteIndex]);
                    Console.WriteLine($"[CharacterCreator] Color palette: {_creator.ColorPalettes[_currentColorPaletteIndex].Name}");
                    break;
                    
                case ConsoleKey.D8:
                    _currentColorPaletteIndex = (_currentColorPaletteIndex + 1) % _creator.ColorPalettes.Count;
                    ApplyColorPalette(appearance, _creator.ColorPalettes[_currentColorPaletteIndex]);
                    Console.WriteLine($"[CharacterCreator] Color palette: {_creator.ColorPalettes[_currentColorPaletteIndex].Name}");
                    break;
                    
                case ConsoleKey.A:
                    if (string.IsNullOrEmpty(appearance.EquippedArmor))
                    {
                        appearance.EquippedArmor = "iron_armor";
                        Console.WriteLine("[CharacterCreator] Armor equipped - clothing hidden");
                    }
                    else
                    {
                        appearance.EquippedArmor = null;
                        Console.WriteLine("[CharacterCreator] Armor removed - clothing visible");
                    }
                    CharacterRenderingSystem.UpdateClothingVisibility(appearance);
                    break;
                    
                case ConsoleKey.W:
                    AnimationSystem.PlayAnimation(World, _previewCharacter, "walk");
                    Console.WriteLine("[CharacterCreator] Playing walk animation");
                    break;
                    
                case ConsoleKey.I:
                    AnimationSystem.PlayAnimation(World, _previewCharacter, "idle");
                    Console.WriteLine("[CharacterCreator] Playing idle animation");
                    break;
            }
        }
    }
    
    private void ApplyColorPalette(CharacterAppearanceComponent appearance, ColorPalette palette)
    {
        if (appearance.ClothingLayers.ContainsKey("shirt"))
        {
            appearance.ClothingLayers["shirt"].PrimaryColor = palette.PrimaryColor;
            appearance.ClothingLayers["shirt"].SecondaryColor = palette.SecondaryColor;
        }
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[CharacterCreator] Unloading character creator scene...");
    }
}
