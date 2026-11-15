// Watched folders: every few seconds Kettle looks in them for thread exports (.srd.json) you have not opened yet.
// Opening one moves it into Kettle's own threads folder, so it leaves the inbox and the watched folder stays tidy.
// The scan runs on a background task (Task.Run: a thread-pool job, like a fire-and-forget async method in C#) so big
// files never stall the window; its finished list is swapped in on the next frame.
public sealed class Inbox
{
    // One unopened export: the newest file for its thread.
    public sealed class Item
    {
        public string Path = "";
        public string Key = "";            // reddit thread id
        public DateTime Written;
        public RecentInfo Info = new();
        public int HotSpots;
    }

    public const float ScanEvery = 10f;    // seconds between scans

    public List<Item> Items { get; private set; } = new();
    public bool Scanning => task != null;
    public int Scans { get; private set; }

    private Task<List<Item>>? task;
    private float wait;                      // seconds until the next scan; 0 scans on the first frame
    // Files already read, by path and write time, so a scan only opens files that are new or changed.
    private readonly Dictionary<string, Item?> known = new();

    // Kettle's own folder for exports you have opened from the inbox.
    public static string ThreadsFolder
    {
        get { string dir = System.IO.Path.Combine(Draft.DataFolder, "threads"); Directory.CreateDirectory(dir); return dir; }
    }

    // Called every frame: picks up a finished scan and starts the next one when it is due.
    public void Tick(float dt, AppSettings settings)
    {
        if (task != null)
        {
            if (!task.IsCompleted) return;
            // A scan started before a file was opened or dismissed would bring it back, so its result is dropped.
            if (taskGeneration == generation && task.IsCompletedSuccessfully) Items = task.Result;
            task = null;
            Scans++;
        }
        wait -= dt;
        if (wait > 0) return;
        wait = ScanEvery;
        if (settings.WatchFolders.Count == 0) { Items = new(); return; }
        var folders = settings.WatchFolders.ToList();
        var dismissed = settings.InboxDismissed.ToHashSet(StringComparer.OrdinalIgnoreCase);
        taskGeneration = generation;
        task = Task.Run(() => Scan(folders, dismissed));
    }

    private int generation, taskGeneration;   // bumped on every change; a scan from an older generation is stale

    // Scan again on the next frame (after a folder is added or removed, or a file is opened or dismissed).
    public void Refresh() { generation++; wait = 0; }

    // Scan right now on this thread (the screenshot tests use it so the list is filled before the picture).
    public void ScanNow(AppSettings settings)
    {
        generation++;
        Items = Scan(settings.WatchFolders.ToList(), settings.InboxDismissed.ToHashSet(StringComparer.OrdinalIgnoreCase));
        wait = ScanEvery;
    }

    // The key used to remember a dismissed file: its path and write time, so a newer export with the same name shows again.
    public static string DismissKey(Item it) => it.Path + "|" + it.Written.Ticks;

    // Find the unopened exports: newest file per thread, skipping threads Kettle already holds a copy at least as new of,
    // and files you dismissed. Best drama first.
    private List<Item> Scan(List<string> folders, HashSet<string> dismissed)
    {
        // What the threads folder already holds, newest write time per thread.
        var have = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        foreach (string f in SafeFiles(ThreadsFolder))
        {
            string key = KeyFromName(f);
            DateTime t = File.GetLastWriteTimeUtc(f);
            if (!have.TryGetValue(key, out DateTime h) || t > h) have[key] = t;
        }

        var best = new Dictionary<string, Item>(StringComparer.OrdinalIgnoreCase);
        foreach (string folder in folders)
        {
            foreach (string f in SafeFiles(folder))
            {
                Item? it = Read(f);
                if (it == null || dismissed.Contains(DismissKey(it))) continue;
                if (have.TryGetValue(it.Key, out DateTime h) && h >= it.Written) continue;
                if (!best.TryGetValue(it.Key, out Item? b) || it.Written > b.Written) best[it.Key] = it;
            }
        }
        return best.Values.OrderByDescending(x => x.Info.Pct ?? -1).ThenByDescending(x => x.Written).ToList();
    }

    // The .srd.json files in a folder (none when the folder is missing or unreadable).
    private static IEnumerable<string> SafeFiles(string folder)
    {
        try { return Directory.Exists(folder) ? Directory.GetFiles(folder, "*.srd.json") : Array.Empty<string>(); }
        catch (Exception) { return Array.Empty<string>(); }
    }

    // Read one export once per write time. A file that fails to load (still downloading, or not a thread) is tried again next scan.
    private Item? Read(string file)
    {
        DateTime written;
        try { written = File.GetLastWriteTimeUtc(file); } catch (Exception) { return null; }
        string cacheKey = file + "|" + written.Ticks;
        lock (known) { if (known.TryGetValue(cacheKey, out Item? hit)) return hit; }
        try
        {
            ThreadData data = ThreadData.Load(file);
            var it = new Item
            {
                Path = file, Written = written, Key = Draft.KeyOf(data.ThreadUrl),
                Info = AppSettings.Describe(data), HotSpots = Math.Min(data.FileHotCount, ThreadData.HotCap),
            };
            lock (known) known[cacheKey] = it;
            return it;
        }
        catch (Exception) { return null; }
    }

    // The thread id in a file Kettle saved as srd-<id>.srd.json.
    private static string KeyFromName(string file)
    {
        string name = System.IO.Path.GetFileName(file);
        name = name.EndsWith(".srd.json", StringComparison.OrdinalIgnoreCase) ? name[..^9] : name;
        return name.StartsWith("srd-", StringComparison.OrdinalIgnoreCase) ? name[4..] : name;
    }

    // Move an export into the threads folder as srd-<id>.srd.json (replacing an older copy). Returns the new path,
    // or null with the reason when the file is gone or still in use (a download that has not finished).
    public static string? Take(Item it, out string why)
    {
        why = "";
        string dest = System.IO.Path.Combine(ThreadsFolder, "srd-" + it.Key + ".srd.json");
        if (!File.Exists(it.Path)) { why = "That file is gone"; return null; }
        try { File.Move(it.Path, dest, true); return dest; }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { why = "File still in use; try again"; return null; }
    }
}
