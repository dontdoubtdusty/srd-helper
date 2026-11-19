// Checks the Drama-O-Meter against threads you have labelled (run with node, no packages needed).
//
//   node measure.js <thread.srd.json | srd-post.htm> [more files...]
//
// Each .srd.json file is an export from the userscript (Export for app). Its thread id is looked up in examples/labels.json
// (posted, drama or calm); unlabelled files are listed but not counted. Prints every thread with the new meter and the old
// hot-spot meter, then the AUC of each: the chance a drama thread scores above a calm one (0.5 = coin flip, 1 = perfect).
// Change METER in srd-helper.user.js and run again to see whether a change helps.
//
// "posted" = the thread really became an SRD post. It counts as drama with POSTED_WEIGHT times the weight in the AUC.
// Posted threads come from three places, later ones winning:
//   - "posted" in examples/labels.json;
//   - "Mark posted" in Kettle, read from its labels files (%AppData%\Kettle\drafts\*.labels.json, or KETTLE_DATA\drafts);
//   - your SRD posts saved from the browser (.htm/.html) given on the command line. The thread a post links to most is
//     the posted thread, and the post's time is read from the page.
// A thread only gets the posted weight when its export was made before the SRD post went up. SRD readers vote in the
// threads they are linked to, so an export taken afterwards looks more dramatic than the thread did when it was found;
// such a thread (or one whose post time can't be read) counts as plain drama.

const fs = require('fs');
const path = require('path');
const U = require(path.join(__dirname, 'srd-helper.user.js'));
const labels = JSON.parse(fs.readFileSync(path.join(__dirname, 'examples', 'labels.json'), 'utf8')).threads;
const POSTED_WEIGHT = 3;

// Thread id -> where its "posted" label came from, and when the SRD post went up (ms, or null when unknown).
const postedBy = {};

// Add the threads Kettle has marked as posted to the labels.
(function readKettlePosted() {
  const base = process.env.KETTLE_DATA || (process.env.APPDATA ? path.join(process.env.APPDATA, 'Kettle') : '');
  const dir = base && path.join(base, 'drafts');
  if (!dir || !fs.existsSync(dir)) return;
  for (const f of fs.readdirSync(dir).filter(f => f.endsWith('.labels.json'))) {
    try {
      const j = JSON.parse(fs.readFileSync(path.join(dir, f), 'utf8'));
      const id = ((j.thread || '').match(/comments\/([a-z0-9]+)/i) || [])[1];
      if (id && j.posted) { labels[id] = 'posted'; postedBy[id] = { from: 'kettle', at: null }; }
    } catch (e) { /* a damaged labels file is skipped */ }
  }
})();

// Read a saved SRD post page (old or new reddit): its own id, when it was posted, and the thread it links to most.
// Only the post itself is searched for links, not its comments, which often link other threads.
function readSrdPost(file) {
  const html = fs.readFileSync(file, 'utf8');
  const own = (html.match(/data-fullname="t3_([a-z0-9]+)"/i) || html.match(/<shreddit-post[^>]*\sid="t3_([a-z0-9]+)"/i) ||
    html.match(/rel="canonical"[^>]*comments\/([a-z0-9]+)/i) || html.match(/og:url"[^>]*comments\/([a-z0-9]+)/i) || [])[1];

  // The post's own part of the page: from the post element to where the comments begin.
  let start = html.search(/<div[^>]*data-fullname="t3_|<shreddit-post\b/i);
  if (start < 0) start = 0;
  let end = html.slice(start).search(/class="commentarea"|<shreddit-comment\b|<shreddit-comment-tree\b/i);
  const body = end < 0 ? html.slice(start) : html.slice(start, start + end);

  // When it was posted: old reddit's data-timestamp (ms) or the first datetime in the post's tagline; new reddit's created-timestamp.
  let at = null;
  const ts = body.match(/data-timestamp="(\d{12,14})"/i);
  const dt = body.match(/created-timestamp="([^"]+)"/i) || body.match(/datetime="([^"]+)"/i);
  if (ts) at = Number(ts[1]);
  else if (dt && !isNaN(Date.parse(dt[1]))) at = Date.parse(dt[1]);

  // Count the threads the post links to (link-post url included); the most linked one is the posted thread.
  const counts = new Map();
  for (const m of body.matchAll(/\/comments\/([a-z0-9]{5,9})\b/gi)) {
    const id = m[1].toLowerCase();
    if (id !== (own || '').toLowerCase()) counts.set(id, (counts.get(id) || 0) + 1);
  }
  const ranked = [...counts.entries()].sort((a, b) => b[1] - a[1]);
  return { own, at, thread: ranked.length ? ranked[0][0] : null, others: ranked.slice(1).map(r => r[0]) };
}

const args = process.argv.slice(2);
if (!args.length) { console.log('usage: node measure.js <thread.srd.json | srd-post.htm> [more files...]'); process.exit(1); }
const posts = args.filter(f => /\.html?$/i.test(f));
const files = args.filter(f => !/\.html?$/i.test(f));

// Saved SRD posts mark the thread they link to as posted, with the time it went up.
const postLines = [];
for (const f of posts) {
  let p;
  try { p = readSrdPost(f); } catch (e) { postLines.push(`  ${path.basename(f)}: could not read (${e.message})`); continue; }
  if (!p.thread) { postLines.push(`  ${path.basename(f)}: no linked thread found`); continue; }
  labels[p.thread] = 'posted';
  postedBy[p.thread] = { from: path.basename(f), at: p.at };
  postLines.push(`  ${path.basename(f)}: thread ${p.thread}, posted ${p.at ? new Date(p.at).toISOString().slice(0, 16).replace('T', ' ') : 'at an unknown time'}` +
    (p.others.length ? ` (also links ${p.others.join(', ')})` : ''));
}

const isDrama = l => l === 'drama' || l === 'posted';
const weight = r => r.label === 'posted' ? POSTED_WEIGHT : 1;

// A plain copy of an exported comment tree, in the shape the scoring functions take.
const plain = n => ({ author: n.author, score: n.score, controv: n.controv, heat: n.heat || 0, t: n.t, more: n.more || 0,
  text: n.text || '', kids: (n.kids || []).map(plain) });

const rows = [];
const seen = new Set();
for (const f of files) {
  const d = JSON.parse(fs.readFileSync(f, 'utf8'));
  const id = ((d.threadUrl || '').match(/comments\/([a-z0-9]+)/i) || [])[1];
  if (id) seen.add(id.toLowerCase());
  const roots = d.roots.map(plain);
  const all = roots.reduce((a, r) => a.concat(U.flatten(r)), []);
  const hidden = all.filter(n => n.score === null).length;
  const s = U.threadShares(roots);
  U.scoreNodes(roots);
  const old = U.dramaPct(U.dramaTotal(U.rankHotSpots(roots)));

  // A posted thread keeps the posted weight only when its export is known to be from before the SRD post.
  // Kettle marks count as before: you draft from the export, then post.
  let label = labels[id] || '?', note = '';
  const by = postedBy[id];
  if (label === 'posted' && by && by.from !== 'kettle') {
    const exported = d.exportedAt ? Date.parse(d.exportedAt) : NaN;
    if (!by.at) { label = 'drama'; note = 'post time unknown'; }
    else if (isNaN(exported)) { label = 'drama'; note = 'export time unknown'; }
    else if (exported > by.at) { label = 'drama'; note = 'exported after the post'; }
  }
  rows.push({ id, label, note, title: (d.title || path.basename(f)).slice(0, 40), n: all.length, s, old,
    skip: hidden > all.length / 2 ? 'scores hidden' : '' });
}

const tier = p => Math.round(p) >= U.CONFIG.dramaAt ? 'DRAMA' : Math.round(p) >= U.CONFIG.promisingAt ? 'promising' : '-';
rows.sort((a, b) => b.s.pct - a.s.pct);
for (const r of rows) {
  console.log(r.label.padEnd(6), r.title.padEnd(40), String(r.n).padStart(5), ' new', r.s.pct.toFixed(0).padStart(4), tier(r.s.pct).padEnd(9),
    ' old', r.old.toFixed(0).padStart(4), tier(r.old).padEnd(9), r.skip ? ' (not counted: ' + r.skip + ')' :
    `  bf ${(100 * r.s.backforth).toFixed(0)}% down ${(100 * r.s.bothDown).toFixed(1)}% ctv ${(100 * r.s.controv).toFixed(0)}% rem ${(100 * r.s.removed).toFixed(1)}%` +
    (r.note ? `  (plain drama: ${r.note})` : ''));
}

// What the saved SRD posts pointed at, and which of those threads still need an export.
if (postLines.length) {
  console.log('\nSRD posts read:');
  postLines.forEach(l => console.log(l));
  const missing = Object.keys(postedBy).filter(id => postedBy[id].from !== 'kettle' && !seen.has(id));
  if (missing.length) console.log('  no export given for: ' + missing.join(', ') + ' (export them with the userscript to count them)');
}

// The chance a random drama thread outscores a random calm one, for one way of scoring.
const counted = rows.filter(r => !r.skip && r.label !== '?');
const pos = counted.filter(r => isDrama(r.label)), neg = counted.filter(r => r.label === 'calm');
const posted = pos.filter(r => r.label === 'posted');
// Each drama/calm pair counts with the drama thread's weight, so posted threads pull the AUC harder.
function auc(get) {
  let w = 0, total = 0;
  pos.forEach(p => neg.forEach(q => { w += weight(p) * (get(p) > get(q) ? 1 : get(p) === get(q) ? 0.5 : 0); total += weight(p); }));
  return total ? (w / total).toFixed(2) : 'n/a';
}
const hits = (get, list) => list.filter(r => tier(get(r)) === 'DRAMA').length;
console.log(`\n${pos.length} drama (${posted.length} posted, weight ${POSTED_WEIGHT}), ${neg.length} calm counted.`);
for (const [name, get] of [['new', r => r.s.pct], ['old', r => r.old]])
  console.log(`${name} meter: AUC ${auc(get)}, DRAMA-FILLED on ${hits(get, pos)}/${pos.length} drama (${hits(get, posted)}/${posted.length} posted) and ${hits(get, neg)}/${neg.length} calm`);
