namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// A single line of NPC dialogue with optional conditions and actions
/// </summary>
public class DialogueLine
{
    public string Text { get; set; }
    public string Speaker { get; set; }
    public List<DialogueChoice> Choices { get; private set; }
    public bool EndsConversation { get; set; }

    public DialogueLine(string speaker, string text, bool endsConversation = false)
    {
        Speaker = speaker;
        Text = text;
        EndsConversation = endsConversation;
        Choices = new List<DialogueChoice>();
    }

    public DialogueLine AddChoice(DialogueChoice choice)
    {
        Choices.Add(choice);
        return this;
    }
}

/// <summary>
/// A player choice within a dialogue
/// </summary>
public class DialogueChoice
{
    public string Text { get; set; }
    public string NextNodeId { get; set; }
    public Func<bool>? Condition { get; set; }
    public Action? OnSelected { get; set; }

    public DialogueChoice(string text, string nextNodeId,
        Func<bool>? condition = null, Action? onSelected = null)
    {
        Text = text;
        NextNodeId = nextNodeId;
        Condition = condition;
        OnSelected = onSelected;
    }

    public bool IsAvailable() => Condition == null || Condition();
}

/// <summary>
/// A node in the dialogue tree
/// </summary>
public class DialogueNode
{
    public string NodeId { get; set; }
    public List<DialogueLine> Lines { get; private set; }

    public DialogueNode(string nodeId)
    {
        NodeId = nodeId;
        Lines = new List<DialogueLine>();
    }

    public DialogueNode AddLine(DialogueLine line)
    {
        Lines.Add(line);
        return this;
    }
}

/// <summary>
/// A complete dialogue tree for an NPC
/// </summary>
public class DialogueTree
{
    public string TreeId { get; set; }
    public Dictionary<string, DialogueNode> Nodes { get; private set; }
    public string StartNodeId { get; set; }

    public DialogueTree(string treeId, string startNodeId = "start")
    {
        TreeId = treeId;
        StartNodeId = startNodeId;
        Nodes = new Dictionary<string, DialogueNode>();
    }

    public DialogueTree AddNode(DialogueNode node)
    {
        Nodes[node.NodeId] = node;
        return this;
    }

    public DialogueNode? GetNode(string nodeId)
    {
        return Nodes.TryGetValue(nodeId, out var node) ? node : null;
    }
}

/// <summary>
/// Component that extends NPC with a branching dialogue tree
/// </summary>
public class DialogueTreeComponent : IComponent
{
    public DialogueTree? ActiveTree { get; private set; }
    public string? CurrentNodeId { get; private set; }
    public int CurrentLineIndex { get; private set; }
    public bool IsInConversation { get; private set; }
    public bool IsAwaitingChoice { get; private set; }

    // History of dialogue lines shown
    public List<string> ConversationHistory { get; private set; }

    public DialogueTreeComponent()
    {
        ConversationHistory = new List<string>();
    }

    /// <summary>Start a dialogue tree</summary>
    public bool StartDialogue(DialogueTree tree)
    {
        ActiveTree = tree;
        CurrentNodeId = tree.StartNodeId;
        CurrentLineIndex = 0;
        IsInConversation = true;
        IsAwaitingChoice = false;
        ConversationHistory.Clear();
        return true;
    }

    /// <summary>Get the current dialogue line; null if at end of node</summary>
    public DialogueLine? GetCurrentLine()
    {
        if (!IsInConversation || ActiveTree == null || CurrentNodeId == null) return null;
        var node = ActiveTree.GetNode(CurrentNodeId);
        if (node == null || CurrentLineIndex >= node.Lines.Count) return null;
        return node.Lines[CurrentLineIndex];
    }

    /// <summary>Advance to next line; returns false if node is finished</summary>
    public bool AdvanceLine()
    {
        if (!IsInConversation || ActiveTree == null || CurrentNodeId == null) return false;
        var node = ActiveTree.GetNode(CurrentNodeId);
        if (node == null) { EndDialogue(); return false; }

        var line = GetCurrentLine();
        if (line != null)
            ConversationHistory.Add($"{line.Speaker}: {line.Text}");

        CurrentLineIndex++;

        if (CurrentLineIndex >= node.Lines.Count)
        {
            // Check if last line had choices or ends conversation
            if (line != null)
            {
                if (line.EndsConversation) { EndDialogue(); return false; }
                if (line.Choices.Count > 0) { IsAwaitingChoice = true; return false; }
            }
            EndDialogue();
            return false;
        }

        // If new current line has choices, signal waiting
        var nextLine = GetCurrentLine();
        if (nextLine != null && nextLine.Choices.Count > 0)
            IsAwaitingChoice = true;

        return true;
    }

    /// <summary>Select a player choice to branch to a new node</summary>
    public bool SelectChoice(int choiceIndex)
    {
        if (!IsAwaitingChoice || ActiveTree == null || CurrentNodeId == null) return false;

        var node = ActiveTree.GetNode(CurrentNodeId);
        if (node == null) return false;

        var line = node.Lines.Count > 0 ? node.Lines[CurrentLineIndex - 1] : null;
        if (line == null) return false;

        var available = line.Choices.Where(c => c.IsAvailable()).ToList();
        if (choiceIndex < 0 || choiceIndex >= available.Count) return false;

        var choice = available[choiceIndex];
        ConversationHistory.Add($"[Player]: {choice.Text}");
        choice.OnSelected?.Invoke();

        // End the conversation if the next node doesn't exist in the tree
        if (!ActiveTree.Nodes.ContainsKey(choice.NextNodeId))
        {
            EndDialogue();
            return true;
        }

        CurrentNodeId = choice.NextNodeId;
        CurrentLineIndex = 0;
        IsAwaitingChoice = false;
        return true;
    }

    public void EndDialogue()
    {
        IsInConversation = false;
        IsAwaitingChoice = false;
        ActiveTree = null;
        CurrentNodeId = null;
        CurrentLineIndex = 0;
    }
}
