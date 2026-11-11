# SubredditDrama workflow helper: brainstorm

Last updated: 2026-11-10. Status: version 0.2 built and running (`srd-helper.user.js`). User confirmed version 0.1 works on old.reddit; version 0.2 additions (new Reddit, highlighting, auto-draft, link ticks) are only tested in a simulated browser.

## The user's process

1. Discovery (manual): find controversial or hot threads in big subs. Smaller subs give better drama. The signal looked for: many comment chains with several replies and negative scores.
2. Collect a link per candidate thread.
3. Screen each link, with an online markdown editor open to preview the final post. Reject or keep.
4. With enough keepers, format: pick chains, copy each comment's permalink, nest the quotes, trim with `[...]`, write short framing lines.

Example of the finished format: `examples/wdnlwt-taylor-swift-post.md` (rebuilt from a saved copy of the post page).

Format rules, as the user describes them:
- Each chain is a nested blockquote (`>` per reply level). Chains are separated by an empty bullet (`* `).
- The top comment of a chain carries the link. Replies are plain text.
- A reply gets a link only when the user decides it should (the "link" tick).
- Long comments are trimmed with `[...]`. Trimming is done by editing the draft.

## Hard constraints

- No Reddit API access, and the user is not a mod (so no Devvit). Do not plan around the API.
- Fetches from outside the browser are blocked (403 on `.json`, and Claude's fetch tool refuses reddit.com). Claude cannot read a Reddit thread directly.
- Therefore the tool runs inside the user's own logged-in browser, on pages the user has open, and reads the page (DOM), not the API.
- Do not auto-post, and do not crawl threads automatically at scale. Human-picked quality is the point, and automation risks the account.
- Do not hard-code the sub's rules from memory. Title format, link format and ban lists come from the current sidebar.

## Decisions

- Form: a userscript (JavaScript injected into matching pages by a browser extension such as Tampermonkey). No server, no API.
- Layout: old.reddit.com only (new Reddit support was removed on 2026-11-10; the user only uses old.reddit). One reader (`OLD`) behind the shared core, and the userscript only matches `old.reddit.com`.
- Focus: screening (step 3) and formatting (step 4). Drafting titles and framing text is not a bottleneck, so it is not built.
- Storage: browser local storage only (the draft). Saving files was dropped as extra work.
- Link rule: top comment linked; replies plain unless the user ticks "link". Chosen over automatic reply-linking because any automatic rule over-links long chains.

## What is built (version 0.2)

1. Triage panel: comments loaded and collapsed, "load more" buttons left, negative-score comments, chains (a negative-score comment with at least `chainMinDepth` reply levels), deepest chain. Shows "scores hidden" instead of zero when the sub hides scores.
2. Colored verdict banner: gray (below threshold), green (promising), red-orange "DRAMA-FILLED", amber (scores hidden, cannot judge).
3. Highlighting: chain heads get a red-orange left edge; comments Reddit marks controversial get a dashed purple outline (old.reddit only, marker detection unconfirmed).
4. Focus view workspace (replaced the in-page dimming and the static page): "Open focus view" opens a full-screen layer over the Reddit page (Esc or Close returns). A shadow root keeps Reddit's styles out. A new browser tab was rejected because Reddit's page security policy likely blocks scripts in a tab created from the page; the layer runs in the userscript's own context.
   - Left: only the top-level trees that contain a chain, as foldable comments with scores, category badges, chain-start edge, per-comment "pick", "link" and "+ chain" controls, and a summary strip with jump links.
   - Right: the draft as ordered cards (chain or free text), each shown as a rendered preview (nested quotes, links, `* ` separators) with move up/down, edit-markdown and delete. Tabs: Preview and Source (the whole post as copyable markdown). Buttons: Add picked, Add all chains, Add text, Copy, Clear. Copy joins chains with `* ` and text notes with blank lines.
   - The draft is saved as you type (browser storage, key `srdHelperBlocks`) and survives closing the workspace and moving between threads. It is separate from the small panel's draft box.
   - "Save view" writes the same workspace as a standalone .html file with the editor working, by copying the workspace functions in as source text. Those functions must stay self-contained (see the comment above them in the script).
   - Look: dark by default with a light variant, monospace labels, thin borders, outlined tag chips, no animation beyond a short highlight on jump. Checked by screenshot in Edge.
(REMOVED in v0.3, see the automation rule below) 5. Expand all: old.reddit opens collapsed comments. New Reddit only presses "more replies" buttons (press again after they load); collapsed comments are not reopened there.
6. Pick and link ticks: per comment. "Add ticked as chain" builds the nested quotes and appends them to the draft.
7. Auto-draft all chains: one chain per highlighted chain, following the deepest then biggest branch up to `maxAutoChainLength`; skips chains already in the draft.
8. Editable draft box that persists across page loads and threads. Copy and clear buttons.
   Focus view categories (top 3 each, no overlapping picks, shown as a colored badge on the comment plus a summary strip with jump links):
   - Crowd Magnet: the comment whose subtree holds the most distinct commenters (minimum `crowdMinAuthors`, 3).
   - Duel: the longest reply line where exactly two people alternate (minimum `duelMinComments`, 4). Ranked by comments plus `duelHeatWeight` (0.5) per heated comment; every comment in a duel carries a small "Duel #n" tag.
   - Heated: any comment containing a word or phrase from the editable `heatWords` list gets a "Heated xN" tag (hover shows the words). Whole-word matching with common endings, so "assume" and "trolley" do not match. A word list cannot read sarcasm or quoted insults, so treat it as a hint.
   Tested on a synthetic thread only. A three-person rotation correctly does not count as a duel.
9. Listing ranker (discovery, no API): on any subreddit, multireddit or r/all listing, every thread with `listMinComments`+ comments gets a badge (comments, comments per point, comments per hour, upvote % when Reddit includes it), colored amber or red by comments per point. A panel lists the top `listTopN` threads, sortable by comments per point, comments per hour, or lowest upvote %. Skips promoted posts. Reads only what is on screen. Tested on synthetic listings only; new Reddit feeds load lazily, so only loaded threads are ranked.

10. Hidden comments (the main weakness: a thread can look dull only because the good part has not loaded):
   - Coverage meter in the panel: loaded comments out of the thread's reported total (the total includes deleted and removed comments, so 100% is rarely reachable). A "below threshold" verdict becomes "below threshold so far: only N% loaded" when coverage is under `minCoverage` (50%) and "load more" buttons remain.
(REMOVED in v0.3, see the automation rule below)    - "Load more comments" button: presses "load more" buttons one at a time, buttons under negative comments first (where the drama is), then the largest by reply count. Paced by `loadDelayMs` (1.2 s) and waits for each batch to arrive. Stops at `loadTargetCoverage` (85%), `loadMaxClicks` (40), nothing left, or the Stop button. One open thread at a time, user-started; this is the same click a person would make, but it is automation, so keep the pace and caps modest.
   - "Continue this thread" links open a separate page and cannot be auto-loaded; the panel counts them ("N deeper branches need their own page").
   - Tested on a simulated old.reddit thread only. The old.reddit button markup (`.morecomments a`) is from memory; new Reddit uses `faceplate-partial` buttons and does not expose reply counts, so there the ordering is only by "under a negative comment".
(REMOVED in v0.3)    - Collapsed comments: Expand all works on old.reddit; new Reddit collapse handling is still unknown.
   - Not built: loading more threads on listing pages (old.reddit paginates, new Reddit scrolls forever). The ranker only sees threads already on screen.

11. Panel and ranker look (restyled after "wall of text" feedback): the thread panel is split into labeled sections (Thread, Load, View, Draft) with the verdict banner on top, key-value rows with small grey notes instead of stacked sentences, and a thin coverage bar. Soft dark palette with a light variant that follows the system setting; same palette as the workspace. The listing panel uses the same cards and the badges are soft tinted pills. Checked by screenshot in Edge in both schemes.

12. OP marking and focus-view restyle (the "wall of text" feedback was about the focus view; the panel restyle in item 11 is kept too):
   - OP: the thread author is read from the page (old.reddit: the post's `data-author`; new Reddit: the post's `author`). A comment counts as OP if its author matches, if new Reddit marks it `is-op`, or if old.reddit marks it with the "submitter" class. OP comments get a gold "OP" chip, a gold name and a thin gold side line; tree headers say "(OP)"; the summary strip has an OP row with jump links to their comments.
   - Reading layout: one soft card per top-level tree with a small header (who started it, comment count, people count); comment text limited to about 72 characters wide with generous line height; long comments clamp to a few lines with a "show more" toggle (only comments that really overflow get one); "Fold replies" and "Unfold all" buttons; pick, link, "+ chain" and "reddit" controls appear on hover instead of sitting on every line (always visible on touch screens). Slightly softer text and border colors than before.
   - Checked by screenshot in Edge in both color schemes. Hover states were not screenshotted.

13. Drama-O-Meter (replaces the verdict banner in the panel): a semicircle gauge with a needle that glides to the value, a big percentage, then the same verdict words and tier colors as before (gray "Below your threshold", green "Promising", red-orange "DRAMA-FILLED", amber for cannot-judge states). Two tick marks sit on the 50% and 85% tier boundaries. A one-line caption says what the tiers need ("promising at 3, drama-filled at 6").
   - The percentage is derived from the chain count only, so it always agrees with the words: 0 chains is 0%, `goodThreadChains` (3) is exactly 50% (Promising starts), `hotThreadChains` (6) is exactly 85% (DRAMA-FILLED starts), and it reaches 100% at twice the hot threshold. In between it is a straight line. Change the two thresholds in `CONFIG` and the whole meter follows.
   - Not judged (too small, scores mostly hidden, too little loaded): no number, empty gauge, needle hidden, "?" or a dash.
   - "88%+" when "load more" buttons are left: more comments can only add chains, so the figure is a floor.
   - Heat words and negative-comment share are deliberately not blended into the percentage; they stay as separate numbers. Blending them would make the meter disagree with the words.
   - The workspace header shows the same verdict as a small chip (percentage plus words).
   - The meter was called "Dram-O-Meter" in the request; the label reads "Drama-O-Meter". Easy to change.
   - Tested: scoring boundaries exactly, gauge state for several chain counts, and screenshots in Edge in both color schemes.

## Finding drama without the API

- Best current approach: browse a multireddit of favorite (smaller) subs sorted by controversial or top, and let the ranker order the threads. Open the promising ones, where the thread panel takes over.
- Considered and not built: screening a pasted link list by loading each thread from inside the browser. It would remove most manual screening, but it is untested, automates page loads against Reddit's rules, and puts the account at risk. Only revisit if the user explicitly accepts that.
- Claude cannot browse Reddit itself (blocked). Claude in Chrome could drive the user's browser but has the same automation concern and is slow; not planned.

## Settings (top of the script, `CONFIG`)

A chain is a negative-score comment whose subtree passes all of: `chainMinDepth` (3 levels), `minChainComments` (4 comments in total, so side branches count as width), `minChainAuthors` (2 distinct authors, deleted accounts count as one). Tested on a synthetic thread only.

Thread-level: `minThreadComments` (20 loaded comments before any verdict; below that the panel says "too small to judge", or "open more yourself" if more comments are still unloaded), `goodThreadChains` (3), `hotThreadChains` (6).

Other: `maxAutoChainLength` (6), `linkHost`, `draftKey`.

Known weaknesses: "authors" counts everyone in the subtree, so a pile-on of many users still passes. The minimum thread size uses loaded comments, which understates big threads on new Reddit until more are loaded.

Candidates not yet built, in suggested order:
1. Negative-score cutoff (for example, at or below -3). Today any score below 0 counts, which is noisy.
2. Weight for Reddit-marked controversial comments (old.reddit only, marker detection unconfirmed).
3. Back-and-forth measure (author alternation along the main line) as a stricter version of distinct authors.

Deliberately not planned: a single combined "drama score". It hides which setting did what.

## Open and unverified

- Old.reddit selectors came from knowledge of its markup. Version 0.1 works there per the user. The controversial marker selector is a guess and may never match.
- New Reddit reader was checked against one saved post page (25 comments) in a simulated browser: comments, depth, text, permalinks and chain building work. Saved pages keep comments inside streaming `<template>` tags; a live page is assumed to have them unpacked. Negative scores and chain detection were only tested on a synthetic thread.
- New Reddit loads replies lazily, so counts there may reflect a partial thread. The panel says when "load more" buttons remain.
- The user's own screening threshold is not known. Defaults are guesses.
- Link host (`www.reddit.com` vs `np.reddit.com`): check the sub's current rules.

## Ideas not pursued

- Polling subs and ranking threads (needs the API).
- Duplicate check against recent SRD posts (needs fetching; could be a manual search link later).
- Snapshot on keep: the saved page already works as a snapshot. Dropped.
- Saving drafts or kept threads to files: dropped as extra work.
- RSS feeds for discovery: untested, expected to be rate limited. Last priority.
- Auto-linking replies by branch size or subtree size: rejected in favor of the manual "link" tick.

14. Popcornometer (v0.3, replaces the Drama-O-Meter gauge; "Prototype Version 0.3" shows in the header). The arc is now a video-game power bar: a wedge that grows taller and redder (yellow to orange to red) as the percentage rises. The pixel face on the left reacts to the meter (static swap, no animation): bored below 50%, curious 50-70%, interested 70-85%, intrigued (wide eyes, open mouth) from 85%, and a puzzled "unsure" face when the meter cannot judge (scores hidden, too few comments). It was a plain bored face marking 0% and a red-and-white striped popcorn bucket heaped with popcorn (after a reference image the user supplied, redrawn from scratch) marks 100%, both redrawn as inline SVG (no embedded images). Tick marks stay at 50% and 85%. Verdict words and colors are unchanged.
    - Panel styling is split in two moods: the top (red/cream awning header with yellow "Popcornometer" lettering, popcorn puffs, a dashed gold ticket around the meter) is loud and popcorny; the lower half (thread stats, load, view, draft) is a soft dark terminal with `$ section` headings, `key: value` rows, and `//` comment notes, and stays dark in both color schemes.
    - The header was styled after a retro popcorn-bag poster the user supplied (stripes, outlined yellow lettering); redrawn from scratch, nothing copied from the stock image.

## Rule: no automation (added v0.3)
The script is read-only. It must never click, scroll, load, expand, post or fetch anything on the user's behalf. Reddit can flag automated activity on an account, and the user has no API access and is not a mod, so an account flag is the real cost. Removed in v0.3: the "Load more comments" button (pressed "load more" buttons on a timer) and "Expand all" (on new Reddit it pressed the same network buttons). The only click the script makes is the "Save view" file download, which runs when the user presses that button. The meter now says "open more yourself" when too few comments are loaded. Rejected for the same reason: bulk link-list screening and any background crawling. Not to be re-added without the user deciding it again with this risk in mind.

Exception (user decided 2026-11-10): "Expand all" is back, old.reddit only (the user only uses old.reddit). One press clicks every collapsed comment's expander, once, no timer or loop. New Reddit has no Expand all (its version pressed network "more replies" buttons and was dropped). The timed "Load more comments" button and "load more" pressing stay removed.

Note (2026-11-10): all new Reddit code is removed (reader, listing reader, @match lines). Older entries above that mention new Reddit are history only.

Decision (2026-11-10): the focus view uses the "Dusk" scheme only (mid-tone blue-gray: bg #2d3640, panels #343f4a/#3d4a57, accent #7cc4e8). The light variant was removed from the workspace. Layout chosen from the mockups: "Inspector" (tree list left, one tree in the middle, draft right, chains table underneath); mockups in `mockups/focus-mockups.html`. Layout built (2026-11-10): tree list, one tree at a time, draft on the right with a username badge per quote (each name has its own color; the markdown output is unchanged), and a bottom table with tabs Chains, Crowd Magnet, Duels, OP, Heated. Chains added before this change have no stored usernames and show the plain preview.

Decision (2026-11-10): the in-page focus view and "Save view" were deleted. The panel now has one button, "Export for app", which downloads the thread file that the standalone app `../srd-desk` opens. Reason: the overlay was a second app living inside Reddit's page (about 500 lines, fought Reddit's CSS and scroll); the app has real windows, file saving and no page to disturb. The export only writes data the page already holds, when pressed. Mascot (supplied by the user) is used as the app icon, in the app's top bar and welcome screen, and in the panel headers (embedded as a small data URI).

Efficiency pass (2026-11-10): the page is read once per update into a plain tree (`buildInfo`: score, author, kids, levels, size). Chain detection, the export and the auto-draft all use that tree instead of searching the page again (before: several selector searches per comment, repeated per level). Updates now share one comment list, and the script ignores the pick boxes and badges it inserts itself, so one page change causes one update instead of several. Checked against the previous version on generated threads (60 to 900 comments): identical stats, chain highlights and auto-drafts.

- 2026-11-10: added the "To load" panel section (read-only list of load-more buttons with reply counts, outlined on the page). Decided against a Load more button and against any timing or jitter to look human: each press sends a real request, and a script cannot be made safe by disguising it. Rule v0.3 stands (no scrolling, clicking or loading except Expand all).

## Version 0.4 (2026-11-11)
Decisions by the user, after studying four exports and their finished SRD posts (PCM rules, antiwork, Chappelle/Minneapolis, Brittney Griner swap, cringereels). These supersede older entries above where they disagree: "old.reddit only" (Decisions), "top comment linked" (Decisions), and "Deliberately not planned: a single combined drama score" (Settings).

- Drama score on every comment (`DRAMA` and `scoreNodes`): levels, distinct people, size, heated comments, controversial marks, negative depth, two-person duels, a burst of 6+ replies within an hour, and replies hidden behind "load more". Hot spots (`rankHotSpots`): a fight counts whole when it scores higher than its branches would separately; the best 15 are ranked. The meter is the summed hot spot score through 1 - exp(-total / 200), tiers at 50% and 85% (compared after rounding, so the number shown and the words agree).
- Page marks: hot spots #1-3 red edge and tint, #4-10 orange, #11-15 thin amber, each with a HOT badge. The old chain edge is gone from the page (the chain flag stays in the export).
- "Load more" worth rule (`loadWorth`): replies x context, context rising for a negative, controversial or heated parent, 3+ people or 3+ levels under it, or a parent inside a hot spot; halved at the top level. Lit up on the page at `loadMoreWorth` (12) and `loadMoreMinReplies` (3). Still read-only: the user presses the buttons.
- The panel lost its draft box, pick and link boxes, Auto-draft and Copy (the app does that). Left: meter, stats, To load, Refresh, Expand all, Export. Errors show in a red box in the panel.
- Export version 2: the whole loaded thread (not only chain trees), with d, hot, t (time), more and m (metrics) per comment and `strip.hot`. Reason: in the old exports only 16 of 73 comments the user linked were present.
- New reddit is back, read-only: a readout with the estimate (ESTIMATE stamp) and one link, "Open in old reddit", to the same thread with `?sort=controversial&limit=500` in a new tab. It never redirects by itself. The reader was written against a saved shreddit page (comments are `<shreddit-comment>` with author, score, depth, permalink, created attributes, replies nested inside).
- Updates: `@updateURL` and `@downloadURL` point at raw.githubusercontent.com/dontdoubtdusty/srd-helper/main; the panel shows the version.
- App: link on the last quote of a chain (the punchline), plain headers by default with bold per preset, numbered headers "Giant drama thread #N:" and "Subthread N:", hot trees first, a Hot spots tab, "used" chips, and a labels file on Copy for `calibrate.js`.

Calibration so far (`node calibrate.js`): all 16 linked comments present in the old exports fall inside a hot spot, 15 inside the top 10. This proves little: the old exports were pre-filtered to chain trees, where hot spots cover 93-100% of comments anyway. A real check needs version 2 exports paired with posts or labels.
