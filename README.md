# SRD Helper

A userscript that helps find and write up Reddit drama for SubredditDrama-style posts. It reads only the page you
already have open (no API, no background requests) and never clicks, scrolls or loads anything for you. The one
exception is Expand all on old.reddit, which clicks collapsed comments once when you press it.

- **old.reddit.com:** the Popcornometer panel. A drama percentage for the thread (how much of it is fighting: back-and-forth replies, downvotes on both sides, controversial and removed comments, as shares of the loaded comments), hot spots marked on the page, a list of the
  "load more" buttons worth pressing (lit up for you to press), Expand all, and Export for app (the whole loaded
  thread, scored, as a `.srd.json` file for the SRD Desk app).
- **new reddit:** a small readout with an estimate, and a button that opens the same thread on old.reddit sorted by
  controversial (new tab, only when you click).
- **Subreddit listings (old.reddit):** a badge per thread and a ranked list of the busiest threads.

## Install
1. Install a userscript manager (Tampermonkey or Violentmonkey).
2. Open [`srd-helper.user.js`](https://raw.githubusercontent.com/dontdoubtdusty/srd-helper/main/srd-helper.user.js)
   (raw). The manager offers to install it.
3. Updates: the script points its manager at the same raw file, so new versions are offered automatically once they are
   pushed to `main` with a higher `@version`. The panel header shows the installed version.

## Files
| File | Job |
|---|---|
| `srd-helper.user.js` | The userscript. Settings (`CONFIG`), the meter weights (`METER`) and the hot-spot weights (`DRAMA`) are at the top. |
| `measure.js` | `node measure.js <exports...>`: scores threads labelled drama or calm in `examples/labels.json` and prints how well the meter separates them (AUC), next to the old meter. Run it after changing `METER`. |
| `calibrate.js` | `node calibrate.js <thread.srd.json> <saved post .htm or app .labels.json>`: where the comments you linked rank under the current hot-spot scores. |
| `brainstorm.md` | Decisions, rules and history. |
| `examples/`, `mockups/` | Reference material. |
| `srd-desk/` | SRD Desk, the Windows app that opens the exported threads (C# .NET 10). See `srd-desk/README.md`. |

The full guide (both halves, with pictures) is in the SRD Desk app: `srd-desk/resources/guide/GUIDE.md`, or F1 in the app.
