using System.Numerics;
using System.Text.RegularExpressions;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

// The in-app guide: reads resources/guide/GUIDE.md (the same file that is shown on disk) and draws it in a window.
// Each "## " heading is a page in the list on the left. It understands the small part of Markdown the guide uses:
// headings, paragraphs, bullets, numbered steps, tables, images and "> " tips from Poppy.
public static class GuidePanel
{
    private enum Kind { Heading, Para, Bullet, Step, Table, Image, Tip }

    private class Item
    {
        public Kind Kind;
        public string Text = "";        // heading, paragraph, bullet, tip text; image: the caption
        public string Path = "";        // image file; for a tip, Poppy's picture (empty = the usual one)
        public int Number;              // step number
        public List<string[]> Rows = new();
    }

    private class Section
    {
        public string Title = "";
        public List<Item> Items = new();
    }

    private static List<Section>? sections;
    private static readonly Dictionary<string, Texture2D> pictures = new();
    private static int shownSection = -1;

    private static string GuideDir => Path.Combine(AppContext.BaseDirectory, "resources", "guide");

    public static void Draw(Desk d, float screenW, float screenH)
    {
        if (!d.ShowGuide) return;
        sections ??= Load();

        float grow = Math.Max(1f, Fonts.Scale);
        Vector2 size = new Vector2(Math.Min(1100f * grow, screenW - 60f), Math.Min(820f * grow, screenH - 60f));
        ImGui.SetNextWindowPos(new Vector2(screenW / 2f, screenH / 2f), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(size, ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(new Vector2(600, 400), new Vector2(float.MaxValue, float.MaxValue));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1f);
        bool open = true;
        bool visible = ImGui.Begin("SRD Desk guide", ref open, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings);
        ImGui.PopStyleVar(2);
        if (!open) d.ShowGuide = false;
        if (visible) DrawContents(d);
        ImGui.End();
    }

    private static void DrawContents(Desk d)
    {
        d.GuideSection = Math.Clamp(d.GuideSection, 0, sections!.Count - 1);
        float navW = 210f * Math.Max(1f, Fonts.Scale * 0.9f);

        // Page list.
        ImGui.PushStyleColor(ImGuiCol.ChildBg, Theme.Pn);
        ImGui.BeginChild("guidenav", new Vector2(navW, 0), ImGuiChildFlags.None, ImGuiWindowFlags.None);
        ImGui.Dummy(new Vector2(0, 6));
        ImGui.SetCursorPosX(10f);
        Assets.DrawMascot(44f);
        ImGui.Dummy(new Vector2(0, 4));
        for (int i = 0; i < sections.Count; i++)
        {
            ImGui.SetCursorPosX(10f);
            if (ImGui.Selectable(sections[i].Title + "##nav" + i, d.GuideSection == i, ImGuiSelectableFlags.None, new Vector2(navW - 20f, 0))) d.GuideSection = i;
        }
        ImGui.EndChild();
        ImGui.PopStyleColor();

        // The page.
        ImGui.SameLine(0, 0);
        ImGui.BeginChild("guidepage", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.None);
        if (shownSection != d.GuideSection) { ImGui.SetScrollY(0f); shownSection = d.GuideSection; }
        ImGui.SetCursorPos(new Vector2(28, 20));
        ImGui.BeginGroup();
        float width = Math.Min(ImGui.GetContentRegionAvail().X - 36f, 760f * Math.Max(1f, Fonts.Scale * 0.9f));
        ImGui.PushFont(Fonts.Label);
        ImGui.TextColored(Theme.Acc, sections[d.GuideSection].Title.ToUpperInvariant());
        ImGui.PopFont();
        ImGui.Dummy(new Vector2(0, 6));
        foreach (Item it in sections[d.GuideSection].Items) DrawItem(it, width);
        ImGui.Dummy(new Vector2(0, 40));
        ImGui.EndGroup();
        ImGui.EndChild();
    }

    // One item of the page, drawn at the given text width.
    private static void DrawItem(Item it, float width)
    {
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        switch (it.Kind)
        {
            case Kind.Heading:
                ImGui.Dummy(new Vector2(0, 8));
                ImGui.PushFont(Fonts.Label);
                ImGui.TextColored(Theme.Tx, it.Text);
                ImGui.PopFont();
                ImGui.Dummy(new Vector2(0, 2));
                break;
            case Kind.Para:
                Ui.Body(it.Text, width, 0, Theme.Tx);
                ImGui.Dummy(new Vector2(0, 6));
                break;
            case Kind.Bullet:
            {
                Vector2 p = ImGui.GetCursorScreenPos();
                dl.AddCircleFilled(new Vector2(p.X + 8f, p.Y + ImGui.GetTextLineHeight() / 2f), 2.6f, Theme.U(Theme.Acc));
                ImGui.SetCursorScreenPos(new Vector2(p.X + 22f, p.Y));
                Ui.Body(it.Text, width - 22f, 0, Theme.Tx);
                ImGui.SetCursorScreenPos(new Vector2(p.X, ImGui.GetCursorScreenPos().Y + 3f));
                break;
            }
            case Kind.Step:
            {
                Vector2 p = ImGui.GetCursorScreenPos();
                ImGui.PushFont(Fonts.Label);
                dl.AddText(p, Theme.U(Theme.Acc), it.Number + ".");
                ImGui.PopFont();
                ImGui.SetCursorScreenPos(new Vector2(p.X + 28f, p.Y));
                Ui.Body(it.Text, width - 28f, 0, Theme.Tx);
                ImGui.SetCursorScreenPos(new Vector2(p.X, ImGui.GetCursorScreenPos().Y + 4f));
                break;
            }
            case Kind.Table:
                DrawTable(it, width);
                ImGui.Dummy(new Vector2(0, 8));
                break;
            case Kind.Image:
                DrawImage(it, width);
                break;
            case Kind.Tip:
                DrawTip(it, width);
                break;
        }
    }

    // A table: the first row is the header; cells wrap.
    private static void DrawTable(Item it, float width)
    {
        if (it.Rows.Count == 0) return;
        int cols = it.Rows[0].Length;
        if (!ImGui.BeginTable("t" + it.GetHashCode(), cols, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp, new Vector2(width, 0))) return;
        for (int c = 0; c < cols; c++) ImGui.TableSetupColumn(it.Rows[0][c], ImGuiTableColumnFlags.WidthStretch, c == cols - 1 ? 3f : 1.4f);
        ImGui.TableHeadersRow();
        for (int r = 1; r < it.Rows.Count; r++)
        {
            ImGui.TableNextRow();
            for (int c = 0; c < cols; c++)
            {
                ImGui.TableSetColumnIndex(c);
                ImGui.TextWrapped(c < it.Rows[r].Length ? it.Rows[r][c] : "");
            }
        }
        ImGui.EndTable();
    }

    // A screenshot, scaled down to fit the page, with its caption underneath.
    private static void DrawImage(Item it, float width)
    {
        Texture2D? tex = Picture(it.Path);
        if (tex == null) { ImGui.TextColored(Theme.Mut, "(missing picture: " + it.Path + ")"); return; }
        Texture2D t = tex.Value;
        float w = Math.Min(width, t.Width), h = w * t.Height / t.Width;
        ImGui.Dummy(new Vector2(0, 2));
        Vector2 q = ImGui.GetCursorScreenPos();
        rlImGui.ImageSize(t, new Vector2(w, h));
        ImGui.GetWindowDrawList().AddRect(q, new Vector2(q.X + w, q.Y + h), Theme.U(Theme.Ln), 4f);
        if (it.Text.Length > 0)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Theme.Mut);
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + width);
            ImGui.TextUnformatted(it.Text);
            ImGui.PopTextWrapPos();
            ImGui.PopStyleColor();
        }
        ImGui.Dummy(new Vector2(0, 8));
    }

    // A tip from Poppy: his picture on the left (the tip can name another picture of him) and the text in a tinted box.
    private static void DrawTip(Item it, float width)
    {
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        float picH = 64f;
        Texture2D? tex = it.Path.Length > 0 ? Picture(it.Path) : null;
        float picW = tex != null ? Math.Min(picH * tex.Value.Width / tex.Value.Height, 90f) : picH * 0.9f;
        Vector2 p = ImGui.GetCursorScreenPos();
        float textW = width - picW - 44f;
        float textH = ImGui.CalcTextSize(it.Text, false, textW).Y;
        float boxH = Math.Max(picH, textH) + 20f;
        dl.AddRectFilled(p, new Vector2(p.X + width, p.Y + boxH), Theme.U(Theme.WithAlpha(Theme.Acc, 0.12f)), 8f);
        dl.AddRectFilled(p, new Vector2(p.X + 3f, p.Y + boxH), Theme.U(Theme.Acc), 1.5f);
        ImGui.SetCursorScreenPos(new Vector2(p.X + 14f, p.Y + (boxH - picH) / 2f));
        if (tex != null) rlImGui.ImageSize(tex.Value, new Vector2(picW, picH));
        else Assets.DrawMascot(picH * 0.9f);
        ImGui.SetCursorScreenPos(new Vector2(p.X + 28f + picW, p.Y + 10f));
        Ui.Body(it.Text, textW, 0, Theme.Tx);
        ImGui.SetCursorScreenPos(new Vector2(p.X, p.Y + boxH));
        ImGui.Dummy(new Vector2(width, 10f));
    }

    // Load a picture from the guide folder once (smaller copies are built so it looks clean when scaled down).
    private static Texture2D? Picture(string name)
    {
        if (pictures.TryGetValue(name, out Texture2D t)) return t;
        string file = Path.Combine(GuideDir, name.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(file)) return null;
        t = Raylib.LoadTexture(file);
        Raylib.GenTextureMipmaps(ref t);
        Raylib.SetTextureFilter(t, TextureFilter.Trilinear);
        pictures[name] = t;
        return t;
    }

    // Read GUIDE.md into pages. Everything before the first "## " heading is the first page, "Welcome".
    private static List<Section> Load()
    {
        var result = new List<Section>();
        string file = Path.Combine(GuideDir, "GUIDE.md");
        var cur = new Section { Title = "Welcome" };
        result.Add(cur);
        if (!File.Exists(file))
        {
            cur.Items.Add(new Item { Kind = Kind.Para, Text = "The guide file is missing (resources/guide/GUIDE.md)." });
            return result;
        }

        var para = new List<string>();
        Item? list = null;
        Item? table = null;
        Item? tip = null;
        void Flush()
        {
            if (para.Count > 0) { cur.Items.Add(new Item { Kind = Kind.Para, Text = Plain(string.Join(" ", para)) }); para.Clear(); }
            list = null; table = null; tip = null;
        }

        foreach (string raw in File.ReadAllLines(file))
        {
            string line = raw.TrimEnd();
            if (line.StartsWith("# ")) { Flush(); continue; }    // the document title is the window title
            if (line.StartsWith("## ")) { Flush(); cur = new Section { Title = line[3..].Trim() }; result.Add(cur); continue; }
            if (line.StartsWith("### ")) { Flush(); cur.Items.Add(new Item { Kind = Kind.Heading, Text = Plain(line[4..]) }); continue; }
            if (line.Trim().Length == 0) { Flush(); continue; }

            Match img = Regex.Match(line, @"^!\[(.*?)\]\((.+?)\)$");
            if (img.Success) { Flush(); cur.Items.Add(new Item { Kind = Kind.Image, Text = Plain(img.Groups[1].Value), Path = img.Groups[2].Value }); continue; }

            if (line.StartsWith(">"))
            {
                string text = Plain(line.TrimStart('>').Trim());
                if (tip == null)
                {
                    // A tip may start with a picture: "> ![](poppy-wave.png) text".
                    Match pic = Regex.Match(line.TrimStart('>').Trim(), @"^!\[[^\]]*\]\((.+?)\)\s*(.*)$");
                    string tipPic = pic.Success ? pic.Groups[1].Value : "";
                    if (pic.Success) text = Plain(pic.Groups[2].Value);
                    Flush(); tip = new Item { Kind = Kind.Tip, Text = text, Path = tipPic }; cur.Items.Add(tip);
                }
                else tip.Text += " " + text;
                continue;
            }
            tip = null;

            if (line.StartsWith("|"))
            {
                string[] cells = line.Trim().Trim('|').Split('|').Select(c => Plain(c.Trim())).ToArray();
                if (cells.All(c => Regex.IsMatch(c, @"^:?-+:?$"))) continue;    // the --- row
                if (table == null) { Flush(); table = new Item { Kind = Kind.Table }; cur.Items.Add(table); }
                table.Rows.Add(cells);
                continue;
            }

            Match bullet = Regex.Match(line, @"^[-*]\s+(.*)$");
            Match number = Regex.Match(line, @"^(\d+)\.\s+(.*)$");
            if (bullet.Success) { Flush(); list = new Item { Kind = Kind.Bullet, Text = Plain(bullet.Groups[1].Value) }; cur.Items.Add(list); continue; }
            if (number.Success) { Flush(); list = new Item { Kind = Kind.Step, Number = int.Parse(number.Groups[1].Value), Text = Plain(number.Groups[2].Value) }; cur.Items.Add(list); continue; }
            if (list != null && raw.StartsWith("  ")) { list.Text += " " + Plain(line.Trim()); continue; }    // a wrapped list line

            para.Add(line.Trim());
        }
        Flush();
        if (result.Count > 1 && result[0].Items.Count == 0) result.RemoveAt(0);    // nothing before the first "## " heading
        return result;
    }

    // Strip the Markdown marks the window cannot show: bold, code ticks, and links (keeps the link text).
    private static string Plain(string s)
    {
        s = Regex.Replace(s, @"\[([^\]]*)\]\([^)]*\)", "$1");
        return s.Replace("**", "").Replace("`", "").Replace("\\|", "|");
    }
}
