using System.Diagnostics;

// Everything the screen shows or changes: the open thread, which tree is selected, folds, ticks, the draft,
// and the status line. The panels read this and call its methods; nothing here draws.
public class Desk
{
    public ThreadData? Data;
    public string? FilePath;
    public Draft Draft = new();
    public AppSettings Settings = AppSettings.Load();

    public int SelectedTree;
    public readonly HashSet<int> Folded = new();      // comments whose replies are hidden
    public readonly HashSet<int> Expanded = new();    // long comments shown in full
    public readonly HashSet<int> Picked = new();
    public readonly HashSet<int> Linked = new();
    public string ConsoleTab = "chains";
    public bool HotTreesOnly = true;                  // tree list: only trees with a hot spot (version 2 files)
    public bool HeaderBold;                           // Header menu: add custom and numbered headers in bold
    public bool SourceTab;
    public readonly HashSet<string> Editing = new();  // draft blocks open in the markdown editor
    public bool ConfirmClear;
    public bool OpenOptions;
    public bool ShowGuide;
    public int GuideSection;
    public bool OpenHeaderMenu;                       // set by the Header button; the popup is drawn at window level
    public string HeaderInput = "";                 // the custom header being typed in that popup

    public string Status = "";
    private float statusLeft;
    public int FlashId = -1;
    public float FlashLeft;
    public int ScrollTo = -1;                         // comment the detail pane should scroll to next frame
    public bool WantOpenDialog;

    // Per-frame timers (status fades, the jump flash).
    public void Tick(float dt)
    {
        if (statusLeft > 0) { statusLeft -= dt; if (statusLeft <= 0) Status = ""; }
        if (FlashLeft > 0) FlashLeft -= dt;
    }

    // The longest chain to add: the Options value, or the one stored in the thread file.
    public int ChainLimit => Settings.ChainLimit > 0 ? Settings.ChainLimit : (Data?.MaxChain ?? 6);

    public void SetStatus(string text) { Status = text; statusLeft = 3f; }

    // Open a thread file and its saved draft. Returns false (with a status message) when the file cannot be read.
    public bool Open(string path)
    {
        try
        {
            ThreadData data = ThreadData.Load(path);
            Data = data;
            FilePath = path;
            Draft = Draft.LoadFor(data.ThreadUrl, Settings);
            SelectedTree = 0;
            Folded.Clear(); Expanded.Clear(); Picked.Clear(); Linked.Clear(); Editing.Clear();
            ConsoleTab = data.HotRows.Count > 0 ? "hot" : "chains";
            HotTreesOnly = data.HotRows.Count > 0;
            SelectedTree = data.HotRows.Count > 0 ? data.RootOf[data.HotRows[0].Id] : 0;
            Settings.Touch(path);
            SetStatus("Opened " + Path.GetFileName(path));
            return true;
        }
        catch (Exception e)
        {
            SetStatus("Could not open: " + e.Message);
            return false;
        }
    }

    public void SelectTree(int i)
    {
        if (Data == null || i < 0 || i >= Data.Roots.Count) return;
        SelectedTree = i;
    }

    // Jump to a comment: switch to its tree, unfold its parents, and scroll its top to the top of the pane.
    public void Jump(int id)
    {
        if (Data == null || !Data.RootOf.TryGetValue(id, out int tree)) return;
        SelectedTree = tree;
        for (Node? p = Data.ParentOf[id]; p != null; p = Data.ParentOf[p.Id]) Folded.Remove(p.Id);
        ScrollTo = id;
        FlashId = id;
        FlashLeft = 1.5f;
    }

    // The comment's main line as a chain.
    public void AddChainFrom(Node head)
    {
        if (Data == null) return;
        SetStatus(Draft.AddChain(Data, ThreadData.MainPath(head, ChainLimit), Linked));
    }

    // The ticked comments as one chain, in thread order (ids are numbered in reading order).
    public void AddPicked()
    {
        if (Data == null) return;
        var nodes = Picked.OrderBy(i => i).Where(Data.ById.ContainsKey).Select(i => Data.ById[i]).ToList();
        if (nodes.Count == 0) { SetStatus("Nothing ticked"); return; }
        SetStatus(Draft.AddChain(Data, nodes, Linked));
        Picked.Clear();
        Linked.Clear();
    }

    // Every hot spot (or, in files without drama scores, every highlighted chain) that is not in the draft yet, best first.
    public void AddAll()
    {
        if (Data == null) return;
        int added = 0;
        IEnumerable<Node> heads = Data.HotRows.Count > 0 ? Data.HotRows : Data.Everything.Where(x => x.Chain);
        foreach (Node n in heads)
        {
            if (Draft.ContainsComment(n.Link)) continue;
            if (Draft.AddChain(Data, ThreadData.MainPath(n, ChainLimit), new HashSet<int>()) == "Chain added") added++;
        }
        SetStatus(added > 0 ? "Added " + added + " chains" : "No new chains");
    }

    // Label for the Add all button: hot spots when the file has them.
    public string AddAllLabel => Data != null && Data.HotRows.Count > 0 ? "Add all hot spots" : "Add all chains";

    // Undo or redo the last change to the draft.
    public void UndoDraft() => SetStatus(Draft.Undo() ? "Undone" : "Nothing to undo");
    public void RedoDraft() => SetStatus(Draft.Redo() ? "Redone" : "Nothing to redo");

    public void CopyDraft()
    {
        ImGuiNET.ImGui.SetClipboardText(Draft.Combine());
        SaveLabels();
        SetStatus("Copied");
    }

    // Write which comments the post uses next to the draft (<thread id>.labels.json), with their drama scores.
    // calibrate.js in the userscript folder reads these to check how well the drama score finds what you pick.
    public void SaveLabels()
    {
        if (Data == null) return;
        try
        {
            HashSet<string> linked = Draft.LinkedLinks();
            var used = Draft.UsedLinks().Where(Data.ByLink.ContainsKey).Select(l =>
            {
                Node n = Data.ByLink[l];
                return new { link = l, author = n.Author, score = n.Score, d = n.D, hot = n.Hot, spot = Data.SpotOf.TryGetValue(n.Id, out Node? h) ? h.Hot : 0, linked = linked.Contains(l) };
            }).ToList();
            var labels = new { thread = Data.ThreadUrl, title = Data.Title, version = Data.Version, savedAt = DateTime.UtcNow.ToString("o"), used };
            string folder = Path.Combine(global::Draft.DataFolder, "drafts");
            File.WriteAllText(Path.Combine(folder, global::Draft.KeyOf(Data.ThreadUrl) + ".labels.json"),
                System.Text.Json.JsonSerializer.Serialize(labels, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception) { /* labels are a bonus; never block copying */ }
    }

    // Open a folder in Explorer.
    public void OpenFolder(string folder)
    {
        try { Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true }); }
        catch (Exception) { SetStatus("Could not open the folder"); }
    }

    // Open a link in the default browser.
    public void OpenUrl(string url)
    {
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return;
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Exception) { SetStatus("Could not open the link"); }
    }
}
