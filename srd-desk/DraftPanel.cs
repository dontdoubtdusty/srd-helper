using System.Numerics;
using System.Text.RegularExpressions;
using ImGuiNET;

// Right column: the draft post. Preview shows each chain with a username badge per quote; Source shows the
// exact markdown that Copy puts on the clipboard.
public static class DraftPanel
{
    // Height of the tab and button area. The buttons wrap onto more lines in a narrow column, so it is measured
    // each frame and used by the next one.
    private static float headH = 76f;
    private static bool typing;               // a text box is being typed in: one undo step covers the whole edit
    private static Vector2 headerMenuPos;    // where the Header button is, so its popup opens right under it

    public static void Draw(Desk d, Vector2 pos, Vector2 size)
    {
        ImGui.SetNextWindowPos(pos);
        ImGui.SetNextWindowSize(size);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Theme.Pn);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.Begin("draft", Panels.Fixed);
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        dl.AddLine(pos, new Vector2(pos.X, pos.Y + size.Y), Theme.U(Theme.Ln));

        // Header: tabs and buttons, wrapping onto a second line when the column is narrow.
        ImGui.BeginChild("dhead", new Vector2(0, headH), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar);
        ImGui.SetCursorPos(new Vector2(14, 8));
        if (Ui.TabLabel("tprev", "PREVIEW", !d.SourceTab)) d.SourceTab = false;
        ImGui.SameLine(0, 14);
        if (Ui.TabLabel("tsrc", "SOURCE", d.SourceTab)) d.SourceTab = true;
        ImGui.SameLine(0, 14);
        ImGui.PushFont(Fonts.Label);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Theme.Mut, d.Draft.Blocks.Count + (d.Draft.Blocks.Count == 1 ? " block" : " blocks"));
        ImGui.PopFont();

        // Undo and Redo, right-aligned on the tab row (Alt+Z and Alt+Shift+Z do the same).
        float pad = ImGui.GetStyle().FramePadding.X * 2f;
        float undoW = ImGui.CalcTextSize("Undo").X + pad, redoW = ImGui.CalcTextSize("Redo").X + pad;
        ImGui.SameLine(size.X - 12f - undoW - redoW - ImGui.GetStyle().ItemSpacing.X);
        ImGui.BeginDisabled(!d.Draft.CanUndo);
        if (ImGui.Button("Undo")) d.UndoDraft();
        ImGui.EndDisabled();
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ImGui.SetTooltip("Undo the last change to the draft (Alt+Z)");
        ImGui.SameLine();
        ImGui.BeginDisabled(!d.Draft.CanRedo);
        if (ImGui.Button("Redo")) d.RedoDraft();
        ImGui.EndDisabled();
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ImGui.SetTooltip("Redo (Alt+Shift+Z)");
        ImGui.SetCursorPos(new Vector2(14, 36));
        float edge = pos.X + size.X - 12f;
        bool first = true;
        if (Ui.Flow("Add picked", edge, ref first)) d.AddPicked();
        if (Ui.Flow(d.AddAllLabel, edge, ref first)) d.AddAll();
        bool headerClicked = Ui.Flow("Header", edge, ref first);
        headerMenuPos = new Vector2(ImGui.GetItemRectMin().X, ImGui.GetItemRectMax().Y + 4f);
        if (headerClicked) d.OpenHeaderMenu = true;
        if (Ui.Flow("Add text", edge, ref first)) { Block b = d.Draft.AddText(); d.Editing.Add(b.Id); d.SourceTab = false; }
        if (Ui.Flow("Copy", edge, ref first, true)) d.CopyDraft();
        if (Ui.Flow("Clear", edge, ref first)) { if (d.Draft.Blocks.Count > 0) d.ConfirmClear = true; }
        headH = ImGui.GetCursorPosY() + 2f;
        ImGui.EndChild();
        dl.AddLine(new Vector2(pos.X, pos.Y + headH), new Vector2(pos.X + size.X, pos.Y + headH), Theme.U(Theme.Ln));

        if (d.SourceTab) DrawSource(d);
        else DrawBlocks(d);

        DrawClearConfirm(d);
        DrawHeaderMenu(d);
        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleColor();
    }

    private static void DrawSource(Desk d)
    {
        // The exact markdown that Copy produces, wrapped to the column (a text box cannot wrap long lines).
        string text = d.Draft.Combine();
        ImGui.SetCursorPos(new Vector2(0, headH + 2f));
        ImGui.BeginChild("source", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.None);
        ImGui.SetCursorPos(new Vector2(14, 10));
        ImGui.PushFont(Fonts.Label);
        if (text.Length == 0) ImGui.TextColored(Theme.Mut, "Nothing to copy yet.");
        else Ui.Body(text, ImGui.GetWindowWidth() - 30f, 0, Theme.Tx);
        ImGui.PopFont();
        ImGui.Dummy(new Vector2(0, 30));
        ImGui.EndChild();
    }

    private static void DrawBlocks(Desk d)
    {
        ImGui.SetCursorPos(new Vector2(0, headH + 2f));
        ImGui.BeginChild("blocks", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.None);
        ImGui.SetCursorPos(new Vector2(12, 12));
        if (d.Draft.Blocks.Count == 0)
        {
            ImGui.PushTextWrapPos(ImGui.GetWindowWidth() - 16f);
            ImGui.TextColored(Theme.Mut, "Nothing in the draft yet. Use Add chain on a hot spot (bottom), \"+ chain\" on a comment to add its main line, or tick \"pick\" on comments and press Add picked. Then reorder, edit and trim here.");
            ImGui.PopTextWrapPos();
        }
        else
        {
            Block? toRemove = null; Block? toMove = null; int moveBy = 0;
            for (int i = 0; i < d.Draft.Blocks.Count; i++)
            {
                Block b = d.Draft.Blocks[i];
                DrawCard(d, b, i, out bool remove, out int move);
                if (remove) toRemove = b;
                if (move != 0) { toMove = b; moveBy = move; }
            }
            if (toRemove != null) { d.Draft.Remove(toRemove); d.Editing.Remove(toRemove.Id); }
            if (toMove != null) d.Draft.Move(toMove, moveBy);
        }
        ImGui.Dummy(new Vector2(0, 40));
        ImGui.EndChild();
    }

    // One block: a header with move/edit/delete, an optional markdown editor, and the preview.
    private static void DrawCard(Desk d, Block b, int index, out bool remove, out int move)
    {
        remove = false; move = 0;
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        float cardW = ImGui.GetContentRegionAvail().X - 12f;
        Vector2 p = ImGui.GetCursorScreenPos();
        dl.ChannelsSplit(2);
        dl.ChannelsSetCurrent(1);
        ImGui.BeginGroup();
        ImGui.PushID(b.Id);

        // Header line.
        ImGui.Dummy(new Vector2(0, 0));
        ImGui.SetCursorScreenPos(new Vector2(p.X + 10f, p.Y + 4f));
        ImGui.PushFont(Fonts.Label);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Theme.Mut, (b.Kind + " " + (index + 1)).ToUpperInvariant());
        if (b.Edited) { ImGui.SameLine(0, 8); ImGui.TextColored(Theme.Warn, "edited"); if (ImGui.IsItemHovered()) ImGui.SetTooltip("Edited by hand: usernames are not shown in the preview"); }
        ImGui.PopFont();
        float btnW = 26f, editW = 52f;
        ImGui.SameLine(cardW - btnW * 3f - editW - 14f);
        if (index == 0) ImGui.BeginDisabled();
        if (ImGui.Button("^", new Vector2(btnW, 0))) move = -1;
        if (index == 0) ImGui.EndDisabled();
        ImGui.SameLine(0, 2);
        bool last = index == d.Draft.Blocks.Count - 1;
        if (last) ImGui.BeginDisabled();
        if (ImGui.Button("v", new Vector2(btnW, 0))) move = 1;
        if (last) ImGui.EndDisabled();
        ImGui.SameLine(0, 2);
        bool editing = d.Editing.Contains(b.Id);
        if (ImGui.Button(editing ? "done" : "edit", new Vector2(editW, 0))) { if (!d.Editing.Remove(b.Id)) d.Editing.Add(b.Id); }
        ImGui.SameLine(0, 2);
        if (ImGui.Button("x", new Vector2(btnW, 0))) remove = true;

        ImGui.SetCursorScreenPos(new Vector2(p.X + 10f, ImGui.GetCursorScreenPos().Y + 2f));
        float inner = cardW - 20f;

        // Markdown editor.
        if (editing && b.Kind == "header")
        {
            string title = b.HeaderText;
            float boldW = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemInnerSpacing.X + ImGui.CalcTextSize("bold").X + 8f;
            ImGui.SetNextItemWidth(inner - boldW);
            if (ImGui.InputText("##hed", ref title, 200)) { CheckpointOnce(d); b.HeaderText = title; d.Draft.Save(); }
            if (ImGui.IsItemDeactivated()) typing = false;
            ImGui.SameLine();
            bool bold = b.Bold;
            if (ImGui.Checkbox("bold", ref bold)) { d.Draft.Checkpoint(); b.Bold = bold; d.Draft.Save(); }
        }
        else if (editing)
        {
            string md = b.Md;
            ImGui.PushFont(Fonts.Label);
            int lines = Math.Clamp(md.Split('\n').Length + 1, 4, 14);
            if (ImGui.InputTextMultiline("##ed", ref md, 1 << 16, new Vector2(inner, lines * ImGui.GetTextLineHeight() + 12f))) { CheckpointOnce(d); b.Md = md; d.Draft.Save(); }
            if (ImGui.IsItemDeactivated()) typing = false;
            ImGui.PopFont();
            ImGui.SetCursorScreenPos(new Vector2(p.X + 10f, ImGui.GetCursorScreenPos().Y + 2f));
        }

        // Preview.
        if (string.IsNullOrWhiteSpace(b.Md)) ImGui.TextColored(Theme.Mut, "empty");
        else if (b.Kind == "header") DrawHeaderPreview(b, inner, p.X + 10f);
        else if (b.Who != null && !b.Edited) DrawChainPreview(b, inner, p.X + 10f);
        else DrawMarkdownPreview(b.Md, inner, p.X + 10f);
        ImGui.Dummy(new Vector2(0, 6));

        ImGui.PopID();
        ImGui.EndGroup();
        Vector2 q = new Vector2(p.X + cardW, ImGui.GetItemRectMax().Y);
        dl.ChannelsSetCurrent(0);
        dl.AddRectFilled(p, q, Theme.U(Theme.Bg), 6f);
        dl.AddRect(p, q, Theme.U(Theme.Ln), 6f);
        dl.ChannelsMerge();
        ImGui.SetCursorScreenPos(new Vector2(p.X, q.Y + 8f));
        ImGui.Dummy(new Vector2(0, 0));
    }

    // Save one undo step at the first keystroke of an edit, not one per character.
    private static void CheckpointOnce(Desk d)
    {
        if (typing) return;
        d.Draft.Checkpoint();
        typing = true;
    }

    // A header: its title on its own line, in the accent color when bold and plain text color otherwise.
    private static void DrawHeaderPreview(Block b, float width, float left)
    {
        ImGui.SetCursorScreenPos(new Vector2(left, ImGui.GetCursorScreenPos().Y));
        Ui.Body(b.HeaderText, width, 0, b.Bold ? Theme.Acc : Theme.Tx);
    }

    // The popup behind the Header button: presets (click to add, x to forget), a custom header box, and the template.
    private static void DrawHeaderMenu(Desk d)
    {
        if (d.OpenHeaderMenu) { ImGui.OpenPopup("headermenu"); d.OpenHeaderMenu = false; d.HeaderInput = ""; }
        if (ImGui.IsPopupOpen("headermenu")) ImGui.SetNextWindowPos(headerMenuPos, ImGuiCond.Appearing);
        if (!ImGui.BeginPopup("headermenu")) return;

        ImGui.TextColored(Theme.Mut, "Add a header line");
        ImGui.Separator();
        string? add = null, forget = null;
        bool addBold = false;

        // Numbered headers: the next giant thread, or the next subthread under the current giant thread.
        string giant = d.Draft.NextGiantHeader(), sub = d.Draft.NextSubthreadHeader();
        if (ImGui.Button(giant)) { add = giant; addBold = d.HeaderBold; }
        ImGui.SameLine();
        if (ImGui.Button(sub)) { add = sub; addBold = d.HeaderBold; }
        ImGui.Separator();

        // Presets: click to add; B switches that preset between bold and plain; x removes it.
        foreach (string h in d.Settings.HeaderPresets.ToList())
        {
            ImGui.PushID(h);
            bool isBold = d.Settings.IsBold(h);
            if (ImGui.Selectable(isBold ? h + "  (bold)" : h, false, ImGuiSelectableFlags.None, new Vector2(230, 0))) { add = h; addBold = isBold; }
            ImGui.SameLine();
            if (isBold) ImGui.PushStyleColor(ImGuiCol.Button, Theme.WithAlpha(Theme.Acc, 0.45f));
            if (ImGui.SmallButton("B")) d.Settings.ToggleBold(h);
            if (isBold) ImGui.PopStyleColor();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(isBold ? "Added in bold; click for plain" : "Added as plain text; click for bold");
            ImGui.SameLine();
            if (ImGui.SmallButton("x")) forget = h;
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Remove this preset");
            ImGui.PopID();
        }
        if (forget != null) d.Settings.ForgetHeader(forget);

        // A typed header is added and remembered as a preset.
        ImGui.Separator();
        if (ImGui.IsWindowAppearing()) ImGui.SetKeyboardFocusHere();
        ImGui.SetNextItemWidth(230);
        bool enter = ImGui.InputTextWithHint("##hnew", "Custom header", ref d.HeaderInput, 200, ImGuiInputTextFlags.EnterReturnsTrue);
        ImGui.SameLine();
        if ((ImGui.Button("Add") || enter) && d.HeaderInput.Trim().Length > 0)
        {
            add = d.HeaderInput.Trim(); addBold = d.HeaderBold;
            d.Settings.RememberHeader(add);
            if (d.HeaderBold && !d.Settings.IsBold(add)) d.Settings.ToggleBold(add);
        }
        ImGui.Checkbox("bold", ref d.HeaderBold);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Add custom and numbered headers in bold (presets use their own B)");

        // The template: your usual headers and text notes, copied into every new draft.
        ImGui.Separator();
        if (ImGui.Button("Save headers and text as template"))
            d.SetStatus("Template saved: " + d.Settings.SaveTemplate(d.Draft) + " blocks");
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("New drafts start with the headers and text notes of this draft (chains are left out)");
        bool noTemplate = d.Settings.Template.Count == 0;
        if (noTemplate) ImGui.BeginDisabled();
        if (ImGui.Button("Insert template into this draft")) { d.Draft.InsertTemplate(d.Settings); d.SetStatus("Template inserted"); ImGui.CloseCurrentPopup(); }
        if (noTemplate) ImGui.EndDisabled();

        if (add != null)
        {
            d.Draft.AddHeader(add, addBold);
            d.SourceTab = false;
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndPopup();
    }

    // A chain with who said what: a colored username badge over each quote, indented by nesting level.
    private static void DrawChainPreview(Block b, float width, float left)
    {
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        Vector4[] bars = { Theme.Acc, Theme.Ok, Theme.Duel };
        foreach (Who w in b.Who!)
        {
            float indent = (w.L - 1) * 14f;
            Vector2 top = new Vector2(left + indent, ImGui.GetCursorScreenPos().Y);
            ImGui.SetCursorScreenPos(new Vector2(left + indent + 12f, top.Y));
            if (w.C)
            {
                // The comment the chain replies to: muted, so the fight itself stands out.
                Ui.Chip("replying to", Theme.Mut);
                ImGui.SameLine(0, 8);
            }
            Ui.Uname(w.A);
            if (w.Op) { ImGui.SameLine(0, 6); Ui.OpChip(); }
            ImGui.SameLine(0, 8);
            Ui.Score(w.S);
            if (w.K) { ImGui.SameLine(0, 8); Ui.Chip("link", Theme.Acc); }
            ImGui.SetCursorScreenPos(new Vector2(left + indent + 12f, ImGui.GetCursorScreenPos().Y));
            float wrap = Math.Max(100f, width - indent - 14f);
            Ui.Body(w.T, wrap, 0, w.C ? Theme.Mut : w.K ? Theme.Acc : Theme.Tx);
            float bottom = ImGui.GetCursorScreenPos().Y;
            dl.AddRectFilled(new Vector2(top.X, top.Y + 2f), new Vector2(top.X + 3f, bottom - 2f), Theme.U(w.C ? Theme.Mut : bars[Math.Min(w.L, 3) - 1]), 1.5f);
            ImGui.SetCursorScreenPos(new Vector2(left, bottom + 6f));
        }
    }

    // Plain preview for text notes and hand-edited chains: nested quote bars and paragraphs, links shown by their text.
    private static void DrawMarkdownPreview(string md, float width, float left)
    {
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        Vector4[] bars = { Theme.Acc, Theme.Ok, Theme.Duel };
        var paras = new List<(int level, string text)>();
        var cur = new List<string>();
        int curLevel = 0;
        void Flush() { if (cur.Count > 0) { paras.Add((curLevel, string.Join(" ", cur))); cur.Clear(); } }
        foreach (string line in md.Split('\n'))
        {
            Match m = Regex.Match(line, @"^(>+)\s?(.*)$");
            int level = m.Success ? m.Groups[1].Length : 0;
            string text = m.Success ? m.Groups[2].Value : line;
            if (level != curLevel) { Flush(); curLevel = level; }
            if (text.Trim().Length == 0) Flush(); else cur.Add(text);
        }
        Flush();
        foreach (var (level, text) in paras)
        {
            string shown = Regex.Replace(text, @"\[((?:[^\[\]]|\[[^\]]*\])*)\]\((https?://[^)\s]+)\)", "$1");
            bool hasLink = shown != text;
            float indent = Math.Max(0, level - 1) * 14f;
            Vector2 top = new Vector2(left + indent, ImGui.GetCursorScreenPos().Y);
            ImGui.SetCursorScreenPos(new Vector2(left + indent + (level > 0 ? 12f : 0f), top.Y));
            Ui.Body(shown, Math.Max(100f, width - indent - 14f), 0, hasLink ? Theme.Acc : Theme.Tx);
            if (level > 0) dl.AddRectFilled(new Vector2(top.X, top.Y + 2f), new Vector2(top.X + 3f, ImGui.GetCursorScreenPos().Y - 2f), Theme.U(bars[Math.Min(level, 3) - 1]), 1.5f);
            ImGui.SetCursorScreenPos(new Vector2(left, ImGui.GetCursorScreenPos().Y + 4f));
        }
    }

    private static void DrawClearConfirm(Desk d)
    {
        if (d.ConfirmClear) { ImGui.OpenPopup("Clear draft?"); d.ConfirmClear = false; }
        if (ImGui.BeginPopupModal("Clear draft?", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("Remove every block from the draft?");
            if (ImGui.Button("Clear it", new Vector2(110, 0))) { d.Draft.Clear(); d.Editing.Clear(); ImGui.CloseCurrentPopup(); }
            ImGui.SameLine();
            if (ImGui.Button("Keep", new Vector2(110, 0))) ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }
    }
}
