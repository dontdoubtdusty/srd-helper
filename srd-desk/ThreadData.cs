using System.Text.Json;
using System.Text.Json.Serialization;

// The thread file exported by the SRD Helper userscript (*.srd.json). Version 2 holds the whole loaded thread with a
// drama score per comment and the ranked hot spots; version 1 held only the trees with chains. Plain data plus a few
// lookups; no drawing and no editing here.

public class Tag
{
    [JsonPropertyName("k")] public string K { get; set; } = "";
    [JsonPropertyName("t")] public string T { get; set; } = "";
}

// One comment and its replies.
public class Node
{
    public int Id { get; set; }
    public string Author { get; set; } = "";
    public bool Op { get; set; }
    public int? Score { get; set; }
    public string Text { get; set; } = "";
    public string Link { get; set; } = "";
    public bool Chain { get; set; }
    public bool Controv { get; set; }
    public int Heat { get; set; }
    public List<string> Words { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();
    public List<Node> Kids { get; set; } = new();
    // Version 2: the drama score of this comment with its replies, its rank among the hot spots (0 = not a hot spot),
    // when it was written (seconds since 1970), replies still hidden behind "load more", and the numbers behind the score.
    public double D { get; set; }
    public int Hot { get; set; }
    public long? T { get; set; }
    public int More { get; set; }
    public NodeMetrics? M { get; set; }
}

// What a drama score is made of (version 2 files).
public class NodeMetrics
{
    public int Size { get; set; }
    public int Levels { get; set; }
    public int People { get; set; }
    public int Heated { get; set; }
    public int Neg { get; set; }
    public int Duel { get; set; }
}

// Per-tree numbers for the tree list, worked out once when the file is loaded.
public class TreeInfo
{
    public int Index;
    public int Count;
    public int People;
    public int Chains;
    public int HotCount;     // hot spots in this tree
    public int BestHot;      // best (lowest) hot spot rank in this tree, 0 when none
    public double BestD;     // drama score of that hot spot
}

public class MeterInfo
{
    public double? Pct { get; set; }
    public bool Plus { get; set; }
    public string Text { get; set; } = "";
    public string Tier { get; set; } = "none";
    // What the percentage is made of (userscript 0.5 and later): "27% back-and-forth · ... · old 82%". Empty in older files.
    public string Why { get; set; } = "";
}

public class StripItem
{
    public int Id { get; set; }
    public string T { get; set; } = "";
}

public class Strip
{
    public List<StripItem> Crowd { get; set; } = new();
    public List<StripItem> Duels { get; set; } = new();
    public List<StripItem> Op { get; set; } = new();
    public List<StripItem> Hot { get; set; } = new();
    public int Heated { get; set; }
}

public class ThreadStats
{
    public int Shown { get; set; }
    public int Tops { get; set; }
    public int Comments { get; set; }
    public int Loaded { get; set; }
    public int Chains { get; set; }
    public int Hot { get; set; }
}

// One row of the Chains table.
public class ChainRow
{
    public Node Head = null!;
    public int Levels;
    public int Comments;
    public int People;
}

public class ThreadData
{
    public string Format { get; set; } = "";
    public int Version { get; set; }
    public string Title { get; set; } = "";
    public string ThreadUrl { get; set; } = "";
    public string Op { get; set; } = "";
    public int MaxChain { get; set; } = 6;
    public MeterInfo? Meter { get; set; }
    public ThreadStats Stats { get; set; } = new();
    public List<Node> Roots { get; set; } = new();
    public Strip Strip { get; set; } = new();

    // Lookups built after loading.
    [JsonIgnore] public Dictionary<int, Node> ById { get; } = new();
    [JsonIgnore] public Dictionary<int, int> DepthOf { get; } = new();
    [JsonIgnore] public Dictionary<int, int> RootOf { get; } = new();
    [JsonIgnore] public Dictionary<int, Node?> ParentOf { get; } = new();
    [JsonIgnore] public List<Node> Everything { get; } = new();
    [JsonIgnore] public List<ChainRow> ChainRows { get; } = new();
    [JsonIgnore] public List<Node> HotRows { get; } = new();           // hot spot heads, best first
    [JsonIgnore] public List<TreeInfo> Trees { get; } = new();         // one per root, same order
    [JsonIgnore] public Dictionary<string, Node> ByLink { get; } = new();
    [JsonIgnore] public Dictionary<int, Node> SpotOf { get; } = new();   // comment id -> the hot spot head it sits under

    // True when the file carries drama scores (version 2 and later).
    [JsonIgnore] public bool Scored => Version >= 2;

    public const string FormatName = "srd-thread";

    // Read a thread file. Throws with a readable message when the file is not one of ours.
    public static ThreadData Load(string path)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        ThreadData? data = JsonSerializer.Deserialize<ThreadData>(File.ReadAllText(path), options);
        if (data == null || data.Format != FormatName) throw new InvalidDataException("Not an SRD thread file (missing format \"" + FormatName + "\").");
        if (data.Roots.Count == 0) throw new InvalidDataException("The file has no comment trees.");
        data.Index();
        return data;
    }

    // Fill the lookups: parents, depths, which tree each comment is in, and the chain rows.
    private void Index()
    {
        for (int i = 0; i < Roots.Count; i++) IndexNode(Roots[i], null, 0, i);
        foreach (Node n in Everything.Where(x => x.Chain))
        {
            ChainRows.Add(new ChainRow { Head = n, Levels = Levels(n), Comments = Count(n), People = Flat(n).Select(x => x.Author).Distinct().Count() });
        }
        HotRows.AddRange(Everything.Where(x => x.Hot > 0).OrderBy(x => x.Hot));
        foreach (Node h in HotRows) foreach (Node x in Flat(h)) SpotOf[x.Id] = h;
        for (int i = 0; i < Roots.Count; i++)
        {
            List<Node> all = Flat(Roots[i]);
            List<Node> hot = all.Where(x => x.Hot > 0).OrderBy(x => x.Hot).ToList();
            Trees.Add(new TreeInfo
            {
                Index = i, Count = all.Count, People = all.Select(x => x.Author).Distinct().Count(), Chains = all.Count(x => x.Chain),
                HotCount = hot.Count, BestHot = hot.Count > 0 ? hot[0].Hot : 0, BestD = hot.Count > 0 ? hot[0].D : 0,
            });
        }
    }

    private void IndexNode(Node n, Node? parent, int depth, int root)
    {
        ById[n.Id] = n;
        DepthOf[n.Id] = depth;
        RootOf[n.Id] = root;
        ParentOf[n.Id] = parent;
        Everything.Add(n);
        if (!string.IsNullOrEmpty(n.Link))
        {
            // Older exports lack ?context=9; the old form stays a key too so drafts saved with it still match.
            string original = n.Link;
            n.Link = ContextLink(original);
            ByLink[original] = n;
            ByLink[n.Link] = n;
        }
        foreach (Node k in n.Kids) IndexNode(k, n, depth + 1, root);
    }

    // A comment link that opens with the whole chain above it: any query or anchor replaced by ?context=9.
    public static string ContextLink(string link)
    {
        int cut = link.IndexOfAny(new[] { '?', '#' });
        return (cut >= 0 ? link[..cut] : link) + "?context=9";
    }

    // A comment and everything below it, in reading order.
    public static List<Node> Flat(Node n)
    {
        var list = new List<Node>();
        void Walk(Node x) { list.Add(x); foreach (Node k in x.Kids) Walk(k); }
        Walk(n);
        return list;
    }

    // Levels in the deepest reply path (the comment itself counts).
    public static int Levels(Node n) => 1 + (n.Kids.Count > 0 ? n.Kids.Max(Levels) : 0);

    public static int Count(Node n) => 1 + n.Kids.Sum(Count);

    public int People(Node n) => Flat(n).Select(x => x.Author).Distinct().Count();

    // A short reason line for a hot spot: what its drama score is made of.
    public static string Why(Node n)
    {
        if (n.M == null) return "";
        var parts = new List<string> { n.M.People + " people", n.M.Levels + " levels", n.M.Size + " comments" };
        if (n.M.Heated > 0) parts.Add(n.M.Heated + " heated");
        if (n.M.Neg > 0) parts.Add(n.M.Neg + " negative");
        if (n.M.Duel >= 4) parts.Add(n.M.Duel + "-reply duel");
        if (n.More > 0) parts.Add(n.More + " not loaded");
        return string.Join(", ", parts);
    }

    // Follow the main line of a fight: at each level the reply with the deepest, then biggest, branch.
    public static List<Node> MainPath(Node head, int max)
    {
        var path = new List<Node> { head };
        Node cur = head;
        while (path.Count < max)
        {
            Node? next = cur.Kids.Where(k => !string.IsNullOrEmpty(k.Text))
                .OrderByDescending(Levels).ThenByDescending(Count).FirstOrDefault();
            if (next == null) break;
            cur = next;
            path.Add(cur);
        }
        return path;
    }
}
