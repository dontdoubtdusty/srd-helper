using System.Text.Json;
using System.Text.Json.Serialization;

// Who said one quote of a chain: kept next to the markdown so the preview can show a username badge.
// The markdown itself carries no usernames (the post format does not have them).
public class Who
{
    public string A { get; set; } = "";
    public bool Op { get; set; }
    public int? S { get; set; }
    public int L { get; set; }
    public string T { get; set; } = "";
    public string U { get; set; } = "";
    public bool K { get; set; }
    // True for the comment the chain replies to: shown grayed as context, not counted as quoted.
    public bool C { get; set; }
}

// One piece of the post: a chain, a free text note, or a header (a line that starts a group of chains; plain or bold).
public class Block
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Kind { get; set; } = "chain";
    public string Md { get; set; } = "";
    // The markdown as first built. When Md differs (edited by hand) the names can no longer be matched to quotes.
    public string? Built { get; set; }
    public List<Who>? Who { get; set; }
    public bool Edited => Who != null && Md != Built;

    // For a header block: whether it is written in bold (**like this**). Read from the markdown, so old drafts keep working.
    [JsonIgnore]
    public bool Bold
    {
        get { string m = Md.Trim(); return m.Length >= 4 && m.StartsWith("**") && m.EndsWith("**"); }
        set { string t = HeaderText; Md = t.Length == 0 ? "" : value ? "**" + t + "**" : t; Built = Md; }
    }

    // For a header block: the title without the bold marks. Setting it keeps the block bold or plain as it was.
    [JsonIgnore]
    public string HeaderText
    {
        get { string m = Md.Trim(); return m.Length >= 4 && m.StartsWith("**") && m.EndsWith("**") ? m[2..^2].Trim() : m; }
        set { bool bold = Bold; string t = (value ?? "").Trim(); Md = t.Length == 0 ? "" : bold ? "**" + t + "**" : t; Built = Md; }
    }
}

// The draft post: an ordered list of blocks, saved to disk per thread so it survives closing the app
// and re-exporting the same thread.
public class Draft
{
    public List<Block> Blocks { get; set; } = new();
    private string? path;

    // Undo history: a copy of the blocks from before each change (most recent last). Not saved to disk.
    private readonly List<string> undoStack = new();
    private readonly List<string> redoStack = new();
    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;

    private string Snapshot() => JsonSerializer.Serialize(Blocks);

    // Call right before changing the blocks, so the change can be undone. A new change clears the redo history.
    public void Checkpoint()
    {
        undoStack.Add(Snapshot());
        if (undoStack.Count > 100) undoStack.RemoveAt(0);
        redoStack.Clear();
    }

    // Go back one change. Returns false when there is nothing to undo.
    public bool Undo()
    {
        if (undoStack.Count == 0) return false;
        redoStack.Add(Snapshot());
        Blocks = JsonSerializer.Deserialize<List<Block>>(undoStack[^1]) ?? new();
        undoStack.RemoveAt(undoStack.Count - 1);
        Save();
        return true;
    }

    // Redo the change that was just undone.
    public bool Redo()
    {
        if (redoStack.Count == 0) return false;
        undoStack.Add(Snapshot());
        Blocks = JsonSerializer.Deserialize<List<Block>>(redoStack[^1]) ?? new();
        redoStack.RemoveAt(redoStack.Count - 1);
        Save();
        return true;
    }

    // Folder for drafts and settings: %AppData%\SrdDesk.
    public static string DataFolder
    {
        get
        {
            // SRDDESK_DATA points the app at another folder (used by the automatic screenshots so they leave your data alone).
            string dir = Environment.GetEnvironmentVariable("SRDDESK_DATA") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SrdDesk");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    // Load the saved draft for a thread. When there is none, a new draft starts from the saved template (if any).
    // The key is the thread's reddit id.
    public static Draft LoadFor(string threadUrl, AppSettings settings)
    {
        string key = KeyOf(threadUrl);
        string folder = Path.Combine(DataFolder, "drafts");
        Directory.CreateDirectory(folder);
        string file = Path.Combine(folder, key + ".json");
        Draft draft = new Draft();
        try
        {
            if (File.Exists(file)) draft = JsonSerializer.Deserialize<Draft>(File.ReadAllText(file)) ?? new Draft();
        }
        catch (Exception) { draft = new Draft(); }    // a damaged draft file starts a fresh draft
        draft.path = file;
        if (!File.Exists(file) && settings.Template.Count > 0) { draft.InsertTemplate(settings); draft.Save(); }
        return draft;
    }

    // Goes up by one on every change, so callers can tell when to recompute something from the blocks.
    [JsonIgnore] public int Changes { get; private set; }

    public void Save()
    {
        Changes++;
        if (path == null) return;
        try { File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true })); }
        catch (Exception) { /* the folder is not writable: the draft just will not persist */ }
    }

    // Whole post as markdown: chains are separated by an empty bullet, text notes and headers by a blank line.
    public string Combine()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < Blocks.Count; i++)
        {
            if (i > 0) sb.Append(Blocks[i - 1].Kind == "chain" && Blocks[i].Kind == "chain" ? "\n\n* \n\n" : "\n\n");
            sb.Append(Blocks[i].Md.Trim());
        }
        return sb.ToString();
    }

    // True when the comment is quoted anywhere in the draft (as a link, or as a plain quote in a chain).
    public bool ContainsComment(string link) => UsedLinks().Contains(link);

    private HashSet<string> used = new();
    private int usedAt = -1;
    private static readonly System.Text.RegularExpressions.Regex LinkRe = new(@"\]\((https?://[^)\s]+)\)");

    // The permalinks of every comment the draft quotes. Recomputed only after the draft changes.
    public HashSet<string> UsedLinks()
    {
        if (usedAt == Changes) return used;
        used = new HashSet<string>(StringComparer.Ordinal);
        foreach (Block b in Blocks)
        {
            if (b.Who != null) foreach (Who w in b.Who) if (!w.C && !string.IsNullOrEmpty(w.U)) used.Add(w.U);
            foreach (System.Text.RegularExpressions.Match m in LinkRe.Matches(b.Md)) used.Add(m.Groups[1].Value);
        }
        usedAt = Changes;
        return used;
    }

    // The permalinks the draft turns into links (the ones a reader can click), for the calibration labels.
    public HashSet<string> LinkedLinks()
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (Block b in Blocks) foreach (System.Text.RegularExpressions.Match m in LinkRe.Matches(b.Md)) set.Add(m.Groups[1].Value);
        return set;
    }

    // Add a chain made of the given comments. Quotes ticked in linked carry a link; when none in the chain is
    // ticked, the last quote gets it (the punchline of the exchange). When context is on and the first comment is a reply, the
    // comment it replies to goes on top as a plain quote so readers know what the fight is about.
    // Returns a message for the status line.
    public string AddChain(ThreadData data, List<Node> nodes, HashSet<int> linked, bool context = true)
    {
        if (nodes.Count == 0 || string.IsNullOrEmpty(nodes[0].Text)) return "Nothing to add";
        if (ContainsComment(nodes[0].Link)) return "Already in the draft";
        Node? parent = context ? data.ParentOf.GetValueOrDefault(nodes[0].Id) : null;
        if (parent != null && string.IsNullOrEmpty(parent.Text)) parent = null;
        var all = parent != null ? nodes.Prepend(parent).ToList() : nodes;
        // The last quote is the link only when no other quote in this chain was ticked as a link.
        bool anyTicked = nodes.Any(n => linked.Contains(n.Id));
        var items = all.Select((n, i) => new ChainItem(data.DepthOf[n.Id], n.Text, n.Link, n != parent && (linked.Contains(n.Id) || (!anyTicked && i == all.Count - 1)))).ToList();
        int minDepth = items.Min(i => i.Depth);
        string md = ChainBuilder.Build(items);
        var who = all.Select((n, i) => new Who { A = n.Author, Op = n.Op, S = n.Score, L = items[i].Depth - minDepth + 1, T = n.Text, U = n.Link, K = items[i].Linked, C = n == parent }).ToList();
        Checkpoint();
        Blocks.Add(new Block { Kind = "chain", Md = md, Built = md, Who = who });
        Save();
        return "Chain added";
    }

    public Block AddText()
    {
        var b = new Block { Kind = "text", Md = "", Built = "" };
        Checkpoint();
        Blocks.Add(b);
        Save();
        return b;
    }

    // Add a header line at the end of the draft (plain unless bold); the chains added after it sit under it.
    public Block AddHeader(string text, bool bold = false)
    {
        var b = new Block { Kind = "header" };
        b.HeaderText = text;
        b.Bold = bold;
        Checkpoint();
        Blocks.Add(b);
        Save();
        return b;
    }

    // Append the saved template blocks (headers and text notes) to the end of the draft.
    public void InsertTemplate(AppSettings settings)
    {
        if (settings.Template.Count > 0) Checkpoint();
        foreach (TemplateItem t in settings.Template) Blocks.Add(new Block { Kind = t.Kind, Md = t.Md, Built = t.Md });
        Save();
    }

    public void Move(Block b, int delta)
    {
        int i = Blocks.IndexOf(b), j = i + delta;
        if (i < 0 || j < 0 || j >= Blocks.Count) return;
        Checkpoint();
        (Blocks[i], Blocks[j]) = (Blocks[j], Blocks[i]);
        Save();
    }

    public void Remove(Block b) { if (!Blocks.Contains(b)) return; Checkpoint(); Blocks.Remove(b); Save(); }

    public void Clear() { if (Blocks.Count == 0) return; Checkpoint(); Blocks.Clear(); Save(); }

    // The next numbered header: "Giant drama thread #N:" counts the ones already in the draft; "Subthread N:" counts
    // the subthread headers since the last giant one, so the numbering starts again under each giant thread.
    public string NextGiantHeader() => "Giant drama thread #" + (Blocks.Count(b => b.Kind == "header" && b.HeaderText.StartsWith("Giant drama thread #")) + 1) + ":";

    public string NextSubthreadHeader()
    {
        int n = 0;
        foreach (Block b in Blocks.Where(b => b.Kind == "header"))
        {
            if (b.HeaderText.StartsWith("Giant drama thread #")) n = 0;
            else if (b.HeaderText.StartsWith("Subthread ")) n++;
        }
        return "Subthread " + (n + 1) + ":";
    }

    // The thread key (its reddit id), used for the draft and label file names.
    public static string KeyOf(string threadUrl)
    {
        string key = System.Text.RegularExpressions.Regex.Match(threadUrl ?? "", @"/comments/([a-z0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups[1].Value;
        return key.Length > 0 ? key : "thread-" + Math.Abs((threadUrl ?? "").GetHashCode());
    }
}

// One block of the template: a header or a text note.
public class TemplateItem
{
    public string Kind { get; set; } = "text";
    public string Md { get; set; } = "";
}

// Small settings file: the recently opened thread files, the header presets and the draft template.
public class AppSettings
{
    public List<string> Recent { get; set; } = new();
    public List<string> HeaderPresets { get; set; } = new() { "Crowd magnet", "Duel", "Heated", "Background", "Aftermath" };
    // Presets that are added in bold (headers are plain otherwise). Kept apart from HeaderPresets so older settings files still load.
    public List<string> BoldPresets { get; set; } = new();
    public List<TemplateItem> Template { get; set; } = new();
    public string Theme { get; set; } = "Dusk";
    public string TextSize { get; set; } = "Default";
    // Longest chain added by + chain and Add all chains; 0 means use the value stored in the thread file.
    public int ChainLimit { get; set; } = 0;
    // Put the comment a chain replies to on top of the chain, so readers see what started it.
    public bool ChainContext { get; set; } = true;

    private static string FilePath => Path.Combine(Draft.DataFolder, "settings.json");

    public static AppSettings Load()
    {
        try { if (File.Exists(FilePath)) return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings(); }
        catch (Exception) { /* damaged settings: start fresh */ }
        return new AppSettings();
    }

    public void Save()
    {
        try { File.WriteAllText(FilePath, JsonSerializer.Serialize(this)); }
        catch (Exception) { /* not writable: ignore */ }
    }

    // Remember a header so it shows up as a one-click preset (at most 30).
    public void RememberHeader(string text)
    {
        text = text.Trim();
        if (text.Length == 0 || HeaderPresets.Contains(text, StringComparer.OrdinalIgnoreCase)) return;
        HeaderPresets.Add(text);
        if (HeaderPresets.Count > 30) HeaderPresets.RemoveAt(0);
        Save();
    }

    public void ForgetHeader(string text) { HeaderPresets.Remove(text); BoldPresets.Remove(text); Save(); }

    public bool IsBold(string preset) => BoldPresets.Contains(preset);

    // Switch a preset between bold and plain.
    public void ToggleBold(string preset) { if (!BoldPresets.Remove(preset)) BoldPresets.Add(preset); Save(); }

    // Make the template from the draft's headers and text notes (chains are left out). Returns how many blocks it holds.
    public int SaveTemplate(Draft draft)
    {
        Template = draft.Blocks.Where(b => b.Kind != "chain").Select(b => new TemplateItem { Kind = b.Kind, Md = b.Md }).ToList();
        Save();
        return Template.Count;
    }

    // Put a file at the top of the recent list (newest first, at most 10, missing files dropped).
    public void Touch(string file)
    {
        Recent.RemoveAll(f => string.Equals(f, file, StringComparison.OrdinalIgnoreCase) || !File.Exists(f));
        Recent.Insert(0, file);
        if (Recent.Count > 10) Recent.RemoveRange(10, Recent.Count - 10);
        Save();
    }
}
