# SRD Desk

Desktop app for building quote-chain posts from Reddit threads. It opens thread files exported by the SRD Helper
userscript (`../srd-helper.user.js` in the same repo, exports from old.reddit) and gives you the Inspector view: a list of comment trees (hot
ones first), one tree at a time, a table of hot spots, chains and category winners, and the draft post with a username
badge on every quote. Version 2 files (userscript 0.4) hold the whole loaded thread with drama scores; version 1 files
still open, without the hot spot views.

C# / .NET 10, Raylib-cs (window), Dear ImGui via ImGui.NET and rlImgui-cs (all the panels). Same stack as birb-box.

The full guide (with screenshots, and Poppy) is `resources/guide/GUIDE.md`. In the app press **F1** or the **Guide** button; it shows the same file.

## Use
1. On an old.reddit thread, press **Export for app** in the SRD Helper panel. A `srd-<id>.srd.json` file downloads.
2. Open it in SRD Desk: drag it onto the window, press **Open file...**, or pass it on the command line.
3. Add chains (**Add chain** on a row of the Hot spots tab, `+ chain` on a comment, tick `pick` / `link` then **Add picked**, or **Add all hot spots**), reorder and edit in the draft, press **Copy**. The last quote of each chain is the link.
4. Headers: press **Header** to add a header line (plain by default) at the end of the draft, then add the chains that belong under it. The popup offers the numbered headers `Giant drama thread #N:` and `Subthread N:`, presets (click to add, `B` for bold, `x` to remove), a custom box (typed headers are remembered as presets), and the template.
5. Made a mistake? **Undo** and **Redo** (above the draft) or Alt+Z and Alt+Shift+Z step back and forward through draft changes.
6. **Copy** also writes `drafts\<thread id>.labels.json` (the comments the post uses, with their drama scores) for `../calibrate.js`.

The template is your usual skeleton: **Save headers and text as template** stores the headers and text notes of the current draft (chains are left out), and every new draft starts with them. **Insert template into this draft** adds them to an existing draft. Presets and the template live in `%AppData%\SrdDesk\settings.json`.

The copied markdown is the exact post format (`Source` tab shows it). Usernames appear only in the Preview.
Drafts are saved per thread in `%AppData%\SrdDesk\drafts` and come back when you open the same thread again.

**Options** (top bar): four color schemes (Dusk, Mist, Paper, Sage), text size (Small to Extra large), the longest chain that `+ chain` / **Add all chains** build, and the saved-data folder. Settings are saved in `%AppData%\SrdDesk\settings.json`.

## Build
- `dotnet run` or `dotnet build` while developing.
- `Build SRD Desk.bat` publishes a Release copy to `..\..\SRD Desk` (next to the repo, outside it) and adds Desktop and Start menu shortcuts (needs the .NET 10 runtime).
- Compile check: `dotnet build`. Crashes go to `crash.txt` next to the exe plus a message box.
- Set `SRDDESK_DATA` to a folder to keep settings and drafts away from `%AppData%\SrdDesk` (the automatic screenshots do this).
- Automatic checks: `SrdDesk.exe samples\example.srd.json --shot out.png --script "chain:0;jump:5;ctab:duels"` renders one frame
  after running the script and exits. `--selftest out.txt` writes the chain markdown for a fixed sample (compare with the userscript's `buildChain`).

## Files
| File | Job |
|---|---|
| `Program.cs` | Entry point, crash report, command line, `SelfTest` |
| `DeskApp.cs` | Window, main loop, panel layout, drag and drop, Open dialog, test script |
| `Desk.cs` | Screen state and actions (open, select, jump, add chains, copy); no drawing |
| `ThreadData.cs` | The thread file model, lookups, tree helpers (`Levels`, `Count`, `MainPath`) |
| `ChainBuilder.cs` | Chain markdown; port of the userscript's `buildChain` (checked byte for byte) |
| `Draft.cs` | Draft blocks (chain, text, header), per-thread saving, `AppSettings` (recent files, header presets, template) |
| `Theme.cs` | Color schemes and ImGui style, fonts (rebuilt when the text size changes), `Assets` |
| `Ui.cs` | Shared drawing helpers: badges, wrapped text, tabs, flowing buttons |
| `TopBarPanel.cs`, `TreeListPanel.cs`, `DetailPanel.cs`, `DraftPanel.cs`, `ConsolePanel.cs` | The five panels |
| `OptionsPanel.cs` | The Options window |
| `GuidePanel.cs` | The Guide window: reads `resources/guide/GUIDE.md` and its pictures |
| `resources/guide/` | `GUIDE.md`, the screenshots it uses and the extra pictures of Poppy |
| `resources/mascot.png`, `mascot.ico` | The mascot: window icon, top bar, welcome screen; the `.ico` is the exe and shortcut icon |
| `samples/example.srd.json` | An invented thread for trying the app (version 2, scored by the userscript's drama model) |

Known limits: emoji and CJK text show as `?` (the fonts cover Latin, Cyrillic and common punctuation). No file association yet
(use "Open with" or drag and drop).
