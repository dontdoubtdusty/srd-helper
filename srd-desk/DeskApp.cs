using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using Color = Raylib_cs.Color;    // System.Windows.Forms (the Open dialog) also has Color and Image
using Image = Raylib_cs.Image;

// The window and the main loop: sets up Raylib and ImGui, lays out the five panels, opens files
// (command line, drag and drop, the Open dialog), and runs the optional screenshot test script.
public sealed class DeskApp
{
    public const string Version = "SRD Desk 0.1";

    private const float TopH = 44f;
    private const float ListW = 290f;
    private const float ConsoleH = 210f;

    private readonly Desk desk = new();

    // Command line options used for automatic checks: --shot file.png [--script "cmd;cmd"].
    public string? OpenPath;
    public string? ShotPath;
    public string? Script;

    public unsafe void Run()
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(1500, 900, Version);
        Raylib.SetWindowMinSize(1000, 600);
        Assets.LoadMascot();
        Raylib.SetExitKey(KeyboardKey.Null);    // Esc must not close the app
        Raylib.SetTargetFPS(60);

        rlImGui.BeginInitImGui();
        ImGui.StyleColorsDark();
        Theme.Use(desk.Settings.Theme);
        Fonts.Load(Fonts.ScaleOf(desk.Settings.TextSize));
        rlImGui.EndInitImGui();
        Fonts.Release();
        Theme.Apply();
        ImGui.GetIO().NativePtr->IniFilename = null;    // panels are fixed, so no layout file

        if (!string.IsNullOrEmpty(OpenPath)) desk.Open(OpenPath);

        int frame = 0;
        while (!Raylib.WindowShouldClose())
        {
            frame++;
            desk.Tick(Raylib.GetFrameTime());
            HandleDroppedFiles();
            if (desk.WantOpenDialog) { desk.WantOpenDialog = false; ShowOpenDialog(); }
            if (ShotPath != null && frame == 3 && Script != null) RunScript(Script);
            if (Raylib.IsKeyPressed(KeyboardKey.F1)) desk.ShowGuide = !desk.ShowGuide;
            HandleUndoKeys();
            ApplyLook();
            RunTestClick(frame);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color((byte)(Theme.Bg.X * 255), (byte)(Theme.Bg.Y * 255), (byte)(Theme.Bg.Z * 255), (byte)255));
            rlImGui.Begin();
            DrawPanels();
            rlImGui.End();
            Raylib.EndDrawing();

            if (ShotPath != null && frame == (clickX >= 0 ? 14 : pressAltZ ? 20 : 8))
            {
                Image img = Raylib.LoadImageFromScreen();
                Raylib.ExportImage(img, ShotPath);
                Raylib.UnloadImage(img);
                break;
            }
        }

        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }

    // Alt+Z undoes the last draft change, Alt+Shift+Z redoes it. Ignored while typing in a text box.
    private void HandleUndoKeys()
    {
        bool alt = Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt);
        if (!alt || !Raylib.IsKeyPressed(KeyboardKey.Z) || ImGui.GetIO().WantTextInput) return;
        bool shift = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
        if (shift) desk.RedoDraft(); else desk.UndoDraft();
    }

    // Apply a changed color scheme or text size from the Options menu (the fonts are rebuilt at the new size).
    private void ApplyLook()
    {
        if (Theme.Name != desk.Settings.Theme) { Theme.Use(desk.Settings.Theme); Theme.Apply(); }
        float scale = Fonts.ScaleOf(desk.Settings.TextSize);
        if (Math.Abs(Fonts.Scale - scale) > 0.001f) Fonts.Rebuild(scale);
    }

    // Place the five panels: top bar, tree list, detail, draft (full height on the right), console (under list and detail).
    private void DrawPanels()
    {
        float w = Raylib.GetScreenWidth(), h = Raylib.GetScreenHeight();
        float grow = Math.Max(1f, Fonts.Scale);    // bigger text needs slightly bigger bars and columns
        float topH = TopH * grow, listW = ListW * grow;
        float draftW = Math.Clamp(w * 0.29f, 340f, 430f) * Math.Min(grow, 1.15f);
        float bodyH = h - topH;
        float consoleH = Math.Min(ConsoleH * grow, bodyH * 0.4f);
        float mainW = w - draftW;

        TopBarPanel.Draw(desk, w, topH);
        TreeListPanel.Draw(desk, new Vector2(0, topH), new Vector2(listW, bodyH - consoleH));
        DetailPanel.Draw(desk, new Vector2(listW, topH), new Vector2(mainW - listW, bodyH - consoleH));
        ConsolePanel.Draw(desk, new Vector2(0, h - consoleH), new Vector2(mainW, consoleH));
        DraftPanel.Draw(desk, new Vector2(mainW, topH), new Vector2(draftW, bodyH));
        OptionsPanel.Draw(desk, w, h);
        GuidePanel.Draw(desk, w, h);
    }

    // Open the first dropped .json file.
    private unsafe void HandleDroppedFiles()
    {
        if (!Raylib.IsFileDropped()) return;
        FilePathList list = Raylib.LoadDroppedFiles();
        try
        {
            for (int i = 0; i < list.Count; i++)
            {
                string? path = Marshal.PtrToStringUTF8((IntPtr)list.Paths[i]);
                if (path != null && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) { desk.Open(path); break; }
            }
        }
        finally { Raylib.UnloadDroppedFiles(list); }
    }

    // The standard Windows Open dialog (blocks the loop while it is up, which is fine).
    private void ShowOpenDialog()
    {
        using var dialog = new System.Windows.Forms.OpenFileDialog
        {
            Title = "Open a thread file",
            Filter = "SRD thread files (*.srd.json;*.json)|*.srd.json;*.json|All files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads",
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) desk.Open(dialog.FileName);
    }

    // Test hook: a real mouse click at a window position (used by --script click:x:y). The cursor moves at frame 4,
    // presses at frame 6 and releases at frame 8, so hover-only controls have time to appear.
    private int clickX = -1, clickY = -1;

    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);

    private bool pressAltZ;
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);

    private void RunTestClick(int frame)
    {
        // Test hook: a real Alt+Z key press (--script altz).
        if (pressAltZ)
        {
            // First click an empty spot so the window has keyboard focus.
            if (frame == 4) { unsafe { SetForegroundWindow((IntPtr)Raylib.GetWindowHandle()); } SetCursorPos((int)Raylib.GetWindowPosition().X + 500, (int)Raylib.GetWindowPosition().Y + 880); }
            if (frame == 5) mouse_event(0x0002, 0, 0, 0, UIntPtr.Zero);
            if (frame == 6) mouse_event(0x0004, 0, 0, 0, UIntPtr.Zero);
            if (frame == 10) keybd_event(0x12, 0, 0, UIntPtr.Zero);
            if (frame == 11) keybd_event(0x5A, 0, 0, UIntPtr.Zero);
            if (frame == 13) { keybd_event(0x5A, 0, 2, UIntPtr.Zero); keybd_event(0x12, 0, 2, UIntPtr.Zero); }
        }
        if (clickX < 0) return;
        Vector2 win = Raylib.GetWindowPosition();
        if (frame == 4) SetCursorPos((int)win.X + clickX, (int)win.Y + clickY);
        if (frame == 6) mouse_event(0x0002, 0, 0, 0, UIntPtr.Zero);    // left down
        if (frame == 8) mouse_event(0x0004, 0, 0, 0, UIntPtr.Zero);    // left up
    }

    // Test hook: run a few Desk actions before the screenshot (tree:1;chain:0;pick:2;addpicked;ctab:heated;jump:4;source).
    private void RunScript(string script)
    {
        foreach (string step in script.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = step.Split(':');
            string arg = parts.Length > 1 ? parts[1] : "";
            int.TryParse(arg, out int n);
            switch (parts[0])
            {
                case "tree": desk.SelectTree(n); break;
                case "chain": if (desk.Data != null) desk.AddChainFrom(desk.Data.ById[n]); break;
                case "pick": desk.Picked.Add(n); break;
                case "link": desk.Linked.Add(n); desk.Picked.Add(n); break;
                case "addpicked": desk.AddPicked(); break;
                case "addall": desk.AddAll(); break;
                case "ctab": desk.ConsoleTab = arg; break;
                case "jump": desk.Jump(n); break;
                case "fold": desk.Folded.Add(n); break;
                case "source": desk.SourceTab = true; break;
                case "click": clickX = n; int.TryParse(parts.Length > 2 ? parts[2] : "", out clickY); break;
                case "altz": pressAltZ = true; break;
                case "undo": desk.UndoDraft(); break;
                case "redo": desk.RedoDraft(); break;
                case "options": desk.OpenOptions = true; break;
                case "guide": desk.ShowGuide = true; desk.GuideSection = n; break;
                case "theme": desk.Settings.Theme = arg; break;
                case "size": desk.Settings.TextSize = arg; break;
                case "header": desk.Draft.AddHeader(arg); break;
                case "menu": desk.OpenHeaderMenu = true; break;
                case "giant": desk.Draft.AddHeader(desk.Draft.NextGiantHeader()); break;
                case "sub": desk.Draft.AddHeader(desk.Draft.NextSubthreadHeader()); break;
                case "alltrees": desk.HotTreesOnly = false; break;
                case "labels": desk.SaveLabels(); break;
                case "text": { Block b = desk.Draft.AddText(); b.Md = "A note of my own."; break; }
                case "skiptree": desk.ToggleTreeSkip(n); break;
                case "skiphot": if (n < (desk.Data?.HotRows.Count ?? 0)) desk.Draft.ToggleSkip(desk.Data!.HotRows[n].Link); break;
                // trimopen:block:quote opens trim mode; cut:block:quote:sentence cuts one sentence.
                case "trimopen": { int.TryParse(parts.Length > 2 ? parts[2] : "", out int q); if (n < desk.Draft.Blocks.Count) desk.Trimming.Add(desk.Draft.Blocks[n].Id + ":" + q); break; }
                case "cut":
                    {
                        int.TryParse(parts.Length > 2 ? parts[2] : "", out int q); int.TryParse(parts.Length > 3 ? parts[3] : "", out int s);
                        if (n < desk.Draft.Blocks.Count) desk.Draft.ToggleCut(desk.Draft.Blocks[n], q, s);
                        break;
                    }
            }
        }
    }
}
