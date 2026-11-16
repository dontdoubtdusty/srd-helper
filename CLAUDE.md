# CLAUDE.md

The shared rules in `D:\My Documents\rpret\GitHub\CLAUDE.md` also apply; this file wins where they disagree.

## Project
Reddit drama reader in two parts. Git repo, public on GitHub `dontdoubtdusty/srd-helper`, branch `main`. Commit and push only when asked.
- Rule: read-only, never automates Reddit (only exception: the "Expand all" button the user presses). See `brainstorm.md`.
- Reminder rule: whenever a change touches `srd-helper.user.js` (the Popcornometer), end the summary with a plain reminder that the browser copy needs updating: it only picks up the new `@version` after the change is pushed to `main`, then via the userscript manager's update check (or reinstall from the raw URL). If the change is not pushed yet, say so in the reminder.

## srd-helper.user.js (userscript, no API)
- Reads the open thread page. old.reddit: Popcornometer from thread shares (`METER`, `threadShares`: back-and-forth, both downvoted, controversial, removed inside arguments) with an explanation line; hot spots from a per-comment drama score (`DRAMA`, `scoreNodes`, `rankHotSpots`) marked on the page; a "To load" list of load-more buttons worth pressing; Expand all; and "Export for app" (whole loaded thread, format version 2, `srd-<id>.srd.json`) for Kettle.
- new reddit (www/new/sh): read-only estimate plus an "Open in old reddit" link (controversial, limit 500, new tab). No drafting in the browser.
- Must stay at the repo root: GitHub serves the `@updateURL` from there.
- Version: `@version` at the top and the panel constant below it; keep them in step (see `claude/versioning.md` in the root folder).
- Mascot is embedded as a data URI (`MASCOT`).
- Check: `node --check srd-helper.user.js`. Node `require` exposes the pure functions.
- `node calibrate.js <export> <saved post .htm | .labels.json>` checks the hot-spot scores against real picks. A pick outside every spot that heads one or more spots is reported as an "opener" (context above a split fight), not a miss.
- `node measure.js <exports...>` checks the meter against `examples/labels.json` (thread ids labelled posted/drama/calm) plus threads marked "Mark posted" in Kettle (read from its `drafts\*.labels.json`, `posted: true`); posted counts as drama with weight 3 (`POSTED_WEIGHT`) in the AUC. Saved SRD posts (`.htm`/`.html`, old or new reddit) can be passed too: the most-linked thread in the post body becomes posted, but only keeps the weight when its export's `exportedAt` is before the post time (SRD readers change the votes afterwards); otherwise it counts as plain drama. Kettle marks are trusted as exported before posting. The user's exports and converted saved pages live in `D:/Downloads` as `srd-*.srd.json` / `srd-*.page.json`.

## kettle/ (desktop app Kettle; called SRD Desk before 0.4.0)
- Windows app, C# .NET 10, Raylib-cs + ImGui.NET (same stack as birb-box). Opens `.srd.json` exports (version 2 with drama scores; version 1 still opens).
- Tree list (hot trees first), one tree at a time, Hot spots and chains tables, draft with username badges and per-quote trim (click sentences to cut, gaps become [...]; `Who.Cut`, `Trim` in `Draft.cs`), last quote linked, plain, bold or numbered headers, labels file on Copy for `calibrate.js`.
- Options window (4 color schemes, text size, chain limit), header blocks and a draft template, in-app Guide (F1) that renders `resources/guide/GUIDE.md` with screenshots.
- Version: `DeskApp.Version`.
- Run: the Kettle shortcut, made by `Build Kettle.bat` (publishes to `GitHub\Kettle`, outside the repo). Run it after meaningful changes. Data in `%AppData%\Kettle` (moved over once from the old `SrdDesk` folder).
- Hot spots: only the top 5 count (tables, tree list, chips, labels); "Show all" in the Hot spots tab brings back the rest the export holds (up to 15). `ThreadData.Rank(cap)`.
- New threads (`Inbox.cs`, `InboxPanel.cs`): Options > Watched folders; background scan every 10 s for `*.srd.json`, newest file per thread, hidden when `%AppData%\Kettle\threads` already holds a copy at least as new, or dismissed (`InboxDismissed`, path|write ticks). Opening one moves it to `threads\srd-<id>.srd.json`. Listed on the welcome screen and in the top bar "N new" popup. Exports opened this way leave `D:/Downloads`, so `calibrate.js`/`measure.js` inputs may now be in that threads folder.
- Test: `Kettle.exe samples\example.srd.json --shot out.png --script "chain:0;jump:5"`; `--selftest out.txt` (chain markdown must match the userscript's `buildChain`). Script steps also include `allhot`, `posted` (toggles Mark posted), `home` (closes the thread), `watch:<folder>` (adds and scans at once), `openinbox:<n>`, `inbox`. The `KETTLE_DATA` env var redirects settings and drafts (used by the screenshot tests). Crashes go to `crash.txt` next to the exe plus a message box.
- Mascot (Poppy, he/him) in `resources/mascot.png` and `.ico`.
- `README.md` (repo root and `kettle/`) are user-facing, format in `claude/readme-format.md` in the root folder; `kettle/docs/screenshot.png` is a `--shot` of the sample.
- Files: `Program.cs` entry, crash report, `SelfTest`; `DeskApp.cs` window, loop, layout, dialogs, test script; `Desk.cs` state and actions (no drawing); `ThreadData.cs` file model and lookups; `ChainBuilder.cs` port of `buildChain` (byte for byte); `Draft.cs` draft blocks, saving, `AppSettings`; `Theme.cs` schemes, fonts, `Assets`; `Ui.cs` shared drawing; one file per panel (`TopBar`, `TreeList`, `Detail`, `Draft`, `Console`, `Options`, `Guide`, `Inbox`); `samples/example.srd.json` invented thread.
