using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the branching NPC Dialogue Tree system
/// </summary>
public static class DialogueSystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Dialogue Tree System Test");
        Console.WriteLine("=======================================\n");

        RunSimpleLinearDialogueTest();
        RunBranchingDialogueTest();
        RunMerchantDialogueTest();
        RunQuestDialogueTest();
        RunConversationHistoryTest();
        RunInvalidChoiceTest();
        RunEndConversationTest();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Dialogue Tests Completed");
        Console.WriteLine("=======================================\n");
    }

    private static void RunSimpleLinearDialogueTest()
    {
        Console.WriteLine("[Test] Simple Linear Dialogue");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var npc = world.CreateEntity();
        world.AddComponent(npc, new DialogueTreeComponent());

        var tree = DialogueSystem.BuildSimpleGreeting("Guard Bob",
            new[] { "Halt! Who goes there?", "Oh, it's you. Pass along.", "Safe travels!" });

        bool started = DialogueSystem.StartConversation(world, npc, tree);
        System.Diagnostics.Debug.Assert(started, "Conversation should start");
        System.Diagnostics.Debug.Assert(DialogueSystem.IsInConversation(world, npc), "Should be in conversation");

        var line1 = DialogueSystem.GetCurrentLine(world, npc);
        System.Diagnostics.Debug.Assert(line1 != null && line1.Text == "Halt! Who goes there?", "First line should match");
        Console.WriteLine($"  NPC: {line1!.Text}");

        DialogueSystem.AdvanceLine(world, npc);
        var line2 = DialogueSystem.GetCurrentLine(world, npc);
        System.Diagnostics.Debug.Assert(line2 != null && line2.Text == "Oh, it's you. Pass along.", "Second line should match");
        Console.WriteLine($"  NPC: {line2!.Text}");

        DialogueSystem.AdvanceLine(world, npc);
        var line3 = DialogueSystem.GetCurrentLine(world, npc);
        System.Diagnostics.Debug.Assert(line3 != null && line3.Text == "Safe travels!", "Third line should match");
        Console.WriteLine($"  NPC: {line3!.Text}");

        DialogueSystem.AdvanceLine(world, npc); // ends conversation
        System.Diagnostics.Debug.Assert(!DialogueSystem.IsInConversation(world, npc), "Conversation should have ended");

        Console.WriteLine("  ✅ Simple linear dialogue flows correctly\n");
    }

    private static void RunBranchingDialogueTest()
    {
        Console.WriteLine("[Test] Branching Dialogue");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var npc = world.CreateEntity();
        world.AddComponent(npc, new DialogueTreeComponent());

        // Build a simple branch tree manually
        var tree = new DialogueTree("branch_test", "start");

        var startNode = new DialogueNode("start");
        startNode.AddLine(new DialogueLine("Elder", "Do you want to hear the legend?")
            .AddChoice(new DialogueChoice("Yes, tell me!", "legend"))
            .AddChoice(new DialogueChoice("No thanks.", "goodbye")));
        tree.AddNode(startNode);

        var legendNode = new DialogueNode("legend");
        legendNode.AddLine(new DialogueLine("Elder", "Long ago, a hero rose...", true));
        tree.AddNode(legendNode);

        var goodbyeNode = new DialogueNode("goodbye");
        goodbyeNode.AddLine(new DialogueLine("Elder", "Perhaps another time.", true));
        tree.AddNode(goodbyeNode);

        DialogueSystem.StartConversation(world, npc, tree);

        // Advance past prompt line (which has choices)
        DialogueSystem.AdvanceLine(world, npc);
        System.Diagnostics.Debug.Assert(DialogueSystem.IsAwaitingChoice(world, npc), "Should be awaiting choice");

        // Select choice 0 ("Yes, tell me!")
        DialogueSystem.SelectChoice(world, npc, 0);
        System.Diagnostics.Debug.Assert(!DialogueSystem.IsAwaitingChoice(world, npc), "Should no longer await choice");
        System.Diagnostics.Debug.Assert(DialogueSystem.IsInConversation(world, npc), "Should still be in conversation");

        var legendLine = DialogueSystem.GetCurrentLine(world, npc);
        System.Diagnostics.Debug.Assert(legendLine != null && legendLine.Text.StartsWith("Long ago"), "Should be on legend branch");
        Console.WriteLine($"  NPC: {legendLine!.Text}");

        DialogueSystem.AdvanceLine(world, npc);
        System.Diagnostics.Debug.Assert(!DialogueSystem.IsInConversation(world, npc), "Conversation should have ended");

        Console.WriteLine("  ✅ Branching dialogue follows correct path\n");
    }

    private static void RunMerchantDialogueTest()
    {
        Console.WriteLine("[Test] Merchant Dialogue Factory");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var npc = world.CreateEntity();
        world.AddComponent(npc, new DialogueTreeComponent());

        var tree = DialogueSystem.BuildMerchantDialogue("Merchant Maya");
        DialogueSystem.StartConversation(world, npc, tree);

        System.Diagnostics.Debug.Assert(DialogueSystem.IsInConversation(world, npc), "Should be in conversation");

        var line = DialogueSystem.GetCurrentLine(world, npc);
        System.Diagnostics.Debug.Assert(line != null && line.Text.Contains("Merchant Maya"), "Greeting should mention NPC name");
        Console.WriteLine($"  NPC: {line!.Text}");

        // Advance to choices
        DialogueSystem.AdvanceLine(world, npc);
        System.Diagnostics.Debug.Assert(DialogueSystem.IsAwaitingChoice(world, npc), "Should await player choice");

        // Choose "Goodbye"
        DialogueSystem.SelectChoice(world, npc, 2); // 3rd choice = Goodbye
        var farewellNode = DialogueSystem.GetCurrentLine(world, npc);
        System.Diagnostics.Debug.Assert(farewellNode != null, "Should be on farewell node");
        Console.WriteLine($"  NPC: {farewellNode!.Text}");

        Console.WriteLine("  ✅ Merchant dialogue factory works correctly\n");
    }

    private static void RunQuestDialogueTest()
    {
        Console.WriteLine("[Test] Quest Dialogue Factory");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var npc = world.CreateEntity();
        world.AddComponent(npc, new DialogueTreeComponent());

        bool questAccepted = false;
        var tree = DialogueSystem.BuildQuestDialogue(
            "Elder Yara",
            "Retrieve the Crystal",
            "The ancient crystal has been stolen! Please retrieve it from the cave.",
            onAccept: () => questAccepted = true
        );

        DialogueSystem.StartConversation(world, npc, tree);
        var intro = DialogueSystem.GetCurrentLine(world, npc);
        System.Diagnostics.Debug.Assert(intro != null && intro.Text.Contains("crystal"), "Intro should mention the quest");
        Console.WriteLine($"  NPC: {intro!.Text}");

        // Advance to choices
        DialogueSystem.AdvanceLine(world, npc);

        // Accept quest (choice 0)
        DialogueSystem.SelectChoice(world, npc, 0);
        System.Diagnostics.Debug.Assert(questAccepted, "OnAccept callback should have fired");

        var reply = DialogueSystem.GetCurrentLine(world, npc);
        System.Diagnostics.Debug.Assert(reply != null, "Should be on accepted node");
        Console.WriteLine($"  NPC: {reply!.Text}");

        Console.WriteLine("  ✅ Quest dialogue factory and callbacks work correctly\n");
    }

    private static void RunConversationHistoryTest()
    {
        Console.WriteLine("[Test] Conversation History");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var npc = world.CreateEntity();
        world.AddComponent(npc, new DialogueTreeComponent());

        var tree = DialogueSystem.BuildSimpleGreeting("Narrator",
            new[] { "Line one.", "Line two.", "Line three." });

        DialogueSystem.StartConversation(world, npc, tree);
        DialogueSystem.AdvanceLine(world, npc);
        DialogueSystem.AdvanceLine(world, npc);
        DialogueSystem.AdvanceLine(world, npc);

        var dtComp = world.GetComponent<DialogueTreeComponent>(npc)!;
        System.Diagnostics.Debug.Assert(dtComp.ConversationHistory.Count >= 2, "History should contain past lines");
        Console.WriteLine($"  History ({dtComp.ConversationHistory.Count} entries):");
        foreach (var h in dtComp.ConversationHistory)
            Console.WriteLine($"    {h}");

        Console.WriteLine("  ✅ Conversation history records lines correctly\n");
    }

    private static void RunInvalidChoiceTest()
    {
        Console.WriteLine("[Test] Invalid Choice Handling");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var npc = world.CreateEntity();
        world.AddComponent(npc, new DialogueTreeComponent());

        var tree = DialogueSystem.BuildMerchantDialogue("Test Merchant");
        DialogueSystem.StartConversation(world, npc, tree);
        DialogueSystem.AdvanceLine(world, npc); // get to choices

        // Try an out-of-range choice
        bool result = DialogueSystem.SelectChoice(world, npc, 99);
        System.Diagnostics.Debug.Assert(!result, "Out-of-range choice should return false");
        System.Diagnostics.Debug.Assert(DialogueSystem.IsInConversation(world, npc), "Should still be in conversation");

        Console.WriteLine("  ✅ Invalid choices are handled gracefully\n");
    }

    private static void RunEndConversationTest()
    {
        Console.WriteLine("[Test] Force End Conversation");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var npc = world.CreateEntity();
        world.AddComponent(npc, new DialogueTreeComponent());

        var tree = DialogueSystem.BuildSimpleGreeting("Stranger",
            new[] { "I have much to say..." });

        DialogueSystem.StartConversation(world, npc, tree);
        System.Diagnostics.Debug.Assert(DialogueSystem.IsInConversation(world, npc), "Should be in conversation");

        DialogueSystem.EndConversation(world, npc);
        System.Diagnostics.Debug.Assert(!DialogueSystem.IsInConversation(world, npc), "Conversation should be ended");

        Console.WriteLine("  ✅ EndConversation terminates dialogue correctly\n");
    }
}
