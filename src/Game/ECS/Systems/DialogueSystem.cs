using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Manages branching NPC dialogue trees.
/// Other systems start conversations by calling StartConversation().
/// </summary>
public class DialogueSystem : ISystem
{
    public void Initialize(World world)
    {
        Console.WriteLine("[Dialogue] Dialogue tree system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        // Nothing to tick every frame; dialogue is event-driven.
        // Could add auto-advance timers here if desired.
    }

    // -----------------------------------------------------------------------
    // Conversation management
    // -----------------------------------------------------------------------

    /// <summary>Begin a conversation between the player and an NPC entity.</summary>
    public static bool StartConversation(World world, Entity npcEntity, DialogueTree tree)
    {
        var dtComp = world.GetComponent<DialogueTreeComponent>(npcEntity);
        if (dtComp == null) return false;

        bool started = dtComp.StartDialogue(tree);
        if (started)
            Console.WriteLine($"[Dialogue] Conversation started: {tree.TreeId}");
        return started;
    }

    /// <summary>
    /// Return the current line of dialogue for the NPC entity.
    /// Returns null if no conversation is active.
    /// </summary>
    public static DialogueLine? GetCurrentLine(World world, Entity npcEntity)
    {
        return world.GetComponent<DialogueTreeComponent>(npcEntity)?.GetCurrentLine();
    }

    /// <summary>Advance past the current line. Returns true while still in conversation.</summary>
    public static bool AdvanceLine(World world, Entity npcEntity)
    {
        var dtComp = world.GetComponent<DialogueTreeComponent>(npcEntity);
        if (dtComp == null) return false;

        bool more = dtComp.AdvanceLine();
        if (!more && !dtComp.IsInConversation)
            Console.WriteLine("[Dialogue] Conversation ended.");
        return more || dtComp.IsAwaitingChoice;
    }

    /// <summary>Select a player choice by index (0-based among available choices).</summary>
    public static bool SelectChoice(World world, Entity npcEntity, int choiceIndex)
    {
        var dtComp = world.GetComponent<DialogueTreeComponent>(npcEntity);
        if (dtComp == null) return false;

        bool ok = dtComp.SelectChoice(choiceIndex);
        if (!ok)
            Console.WriteLine($"[Dialogue] Invalid choice index {choiceIndex}");
        return ok;
    }

    /// <summary>End the current conversation immediately.</summary>
    public static void EndConversation(World world, Entity npcEntity)
    {
        world.GetComponent<DialogueTreeComponent>(npcEntity)?.EndDialogue();
        Console.WriteLine("[Dialogue] Conversation ended by system.");
    }

    /// <summary>Whether the entity is currently in conversation.</summary>
    public static bool IsInConversation(World world, Entity npcEntity)
    {
        return world.GetComponent<DialogueTreeComponent>(npcEntity)?.IsInConversation ?? false;
    }

    /// <summary>Whether the conversation is waiting for the player to pick a choice.</summary>
    public static bool IsAwaitingChoice(World world, Entity npcEntity)
    {
        return world.GetComponent<DialogueTreeComponent>(npcEntity)?.IsAwaitingChoice ?? false;
    }

    // -----------------------------------------------------------------------
    // Factory helpers to build common dialogue trees
    // -----------------------------------------------------------------------

    /// <summary>Build a simple linear greeting dialogue (no branching).</summary>
    public static DialogueTree BuildSimpleGreeting(string npcName, IEnumerable<string> lines)
    {
        var tree = new DialogueTree($"greeting_{npcName}", "start");
        var node = new DialogueNode("start");
        var lineList = lines.ToList();
        for (int i = 0; i < lineList.Count; i++)
        {
            bool last = i == lineList.Count - 1;
            node.AddLine(new DialogueLine(npcName, lineList[i], last));
        }
        tree.AddNode(node);
        return tree;
    }

    /// <summary>Build a merchant dialogue tree with buy/sell/farewell branches.</summary>
    public static DialogueTree BuildMerchantDialogue(string npcName)
    {
        var tree = new DialogueTree($"merchant_{npcName}", "start");

        var greeting = new DialogueNode("start");
        greeting.AddLine(new DialogueLine(npcName, $"Welcome! I'm {npcName}. What can I do for you?")
            .AddChoice(new DialogueChoice("I'd like to buy something.", "buy"))
            .AddChoice(new DialogueChoice("I'd like to sell something.", "sell"))
            .AddChoice(new DialogueChoice("Goodbye.", "end")));
        tree.AddNode(greeting);

        var buy = new DialogueNode("buy");
        buy.AddLine(new DialogueLine(npcName, "Sure! Take a look at my wares.", false)
            .AddChoice(new DialogueChoice("Actually, I changed my mind.", "start"))
            .AddChoice(new DialogueChoice("Goodbye.", "end")));
        tree.AddNode(buy);

        var sell = new DialogueNode("sell");
        sell.AddLine(new DialogueLine(npcName, "I'm always looking for good merchandise!", false)
            .AddChoice(new DialogueChoice("Let me see your prices first.", "buy"))
            .AddChoice(new DialogueChoice("Goodbye.", "end")));
        tree.AddNode(sell);

        var end = new DialogueNode("end");
        end.AddLine(new DialogueLine(npcName, "Safe travels, friend!", true));
        tree.AddNode(end);

        return tree;
    }

    /// <summary>Build a quest-giver dialogue tree with accept/decline branches.</summary>
    public static DialogueTree BuildQuestDialogue(string npcName, string questTitle,
        string questDescription, Action? onAccept = null)
    {
        var tree = new DialogueTree($"quest_{npcName}", "start");

        var intro = new DialogueNode("start");
        intro.AddLine(new DialogueLine(npcName,
            $"I need your help! {questDescription}")
            .AddChoice(new DialogueChoice($"I'll take the quest: {questTitle}.", "accepted",
                onSelected: onAccept))
            .AddChoice(new DialogueChoice("Maybe another time.", "declined")));
        tree.AddNode(intro);

        var accepted = new DialogueNode("accepted");
        accepted.AddLine(new DialogueLine(npcName, "Wonderful! I knew I could count on you.", true));
        tree.AddNode(accepted);

        var declined = new DialogueNode("declined");
        declined.AddLine(new DialogueLine(npcName, "I understand. Come back if you change your mind.", true));
        tree.AddNode(declined);

        return tree;
    }
}
