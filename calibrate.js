// Calibration check for the drama model (run with node, no packages needed).
//
//   node calibrate.js <thread.srd.json> <post.htm | thread.labels.json> [more pairs...] [--top N]
//
// For each pair: the exported thread from the userscript, and either a saved SRD post (the page saved from your
// browser) or the labels file SRD Desk writes when you mark comments as used (which also lists the hot spots you skipped). It re-scores the thread with the
// current settings in srd-helper.user.js (so you can change DRAMA there and run again), then reports where the
// comments you actually linked landed: inside a hot spot or not, and how high they rank.
// Old exports (version 1) hold only the trees with chains, so most links will be "not in export" for those.

const fs = require('fs');
const path = require('path');
const U = require(path.join(__dirname, 'srd-helper.user.js'));

const args = process.argv.slice(2);
let topN = 10;
const files = [];
for (let i = 0; i < args.length; i++) {
  if (args[i] === '--top') topN = parseInt(args[++i], 10) || topN;
  else files.push(args[i]);
}
if (files.length < 2 || files.length % 2) {
  console.log('usage: node calibrate.js <thread.srd.json> <post.htm | thread.labels.json> [more pairs...] [--top N]');
  process.exit(1);
}

// The comment id at the end of a permalink (.../comments/<thread>/<slug>/<id>/).
const idOf = link => ((link || '').match(/\/comments\/[a-z0-9]+\/[^/]*\/([a-z0-9]+)\/?(?:[?#].*)?$/i) || [])[1];

// The comment ids a finished post links to, for one thread. Handles both permalink shapes Reddit uses.
function linkedIds(file, threadId) {
  const text = fs.readFileSync(file, 'utf8');
  if (/\.json$/i.test(file)) {
    const j = JSON.parse(text);
    // Only the linked quotes, so a labels file counts the same comments a saved post would show.
    return new Set((j.used || []).filter(u => u.linked).map(u => idOf(u.link)).filter(Boolean));
  }
  const ids = new Set();
  const re = new RegExp('comments/' + threadId + '/(?:[^/"\'\\s]*|comment)/([a-z0-9]{5,9})', 'g');
  let m;
  while ((m = re.exec(text))) ids.add(m[1]);
  return ids;
}

// The hot spots you skipped in SRD Desk (labels files only): ids of their head comments.
function skippedIds(file) {
  if (!/\.json$/i.test(file)) return [];
  return (JSON.parse(fs.readFileSync(file, 'utf8')).skipped || []).map(s => idOf(s.link)).filter(Boolean);
}

let total = { linked: 0, present: 0, inSpot: 0, inTop: 0, skipped: 0, skippedTop: 0 };
for (let f = 0; f < files.length; f += 2) {
  const thread = JSON.parse(fs.readFileSync(files[f], 'utf8'));
  const threadId = (thread.threadUrl.match(/comments\/([a-z0-9]+)/i) || [])[1];
  const all = U.scoreNodes(thread.roots);
  const spots = U.rankHotSpots(thread.roots);
  const pct = U.dramaPct(U.dramaTotal(spots));
  const byId = new Map(all.map(n => [idOf(n.link), n]));
  const spotOf = new Map();
  spots.forEach(s => U.flatten(s).forEach(n => spotOf.set(n, s)));
  const sorted = all.slice().sort((a, b) => b.d - a.d);
  const ids = linkedIds(files[f + 1], threadId);

  console.log('\n== ' + path.basename(files[f]) + ' (export version ' + (thread.version || 1) + ')');
  console.log('   ' + all.length + ' comments in the file, ' + spots.length + ' hot spots, meter ' + Math.round(pct) + '% (file said ' + Math.round(thread.meter.pct || 0) + '%)');
  console.log('   ' + ids.size + ' linked comments in ' + path.basename(files[f + 1]));
  let present = 0, inSpot = 0, inTop = 0;
  ids.forEach(id => {
    const n = byId.get(id);
    if (!n) { console.log('   ' + id + '  not in export'); return; }
    present++;
    const s = spotOf.get(n);
    if (s) inSpot++;
    if (s && s.hot <= topN) inTop++;
    console.log('   ' + id + '  drama ' + n.d.toFixed(0).padStart(3) + ', rank ' + String(sorted.indexOf(n) + 1).padStart(4) + ' of ' + all.length +
      ', ' + (s ? 'in hot spot #' + s.hot : 'outside the hot spots') + (n.kids.length ? '' : ', no replies (end of a chain)') + ', score ' + n.score);
  });
  console.log('   found: ' + present + ' of ' + ids.size + ' in the file; ' + inSpot + ' inside a hot spot; ' + inTop + ' inside the top ' + topN);
  // Skipped spots: where the model ranks the disputes you looked at and decided against. High ranks mean it overrates them.
  const skipped = skippedIds(files[f + 1]);
  if (skipped.length) {
    let top = 0;
    const ranks = skipped.map(id => { const n = byId.get(id); if (n && n.hot && n.hot <= topN) top++; return n ? (n.hot ? '#' + n.hot : 'not a spot now') : 'not in export'; });
    console.log('   skipped ' + skipped.length + ' hot spots, now ranked: ' + ranks.join(', ') + '; ' + top + ' inside the top ' + topN);
    total.skipped += skipped.length; total.skippedTop += top;
  }
  total.linked += ids.size; total.present += present; total.inSpot += inSpot; total.inTop += inTop;
}
if (files.length > 2) {
  console.log('\n== All threads: ' + total.present + ' of ' + total.linked + ' linked comments in the files; ' +
    total.inSpot + ' inside a hot spot (' + (total.present ? Math.round(100 * total.inSpot / total.present) : 0) + '%); ' +
    total.inTop + ' inside the top ' + topN + '.' + (total.skipped ? ' Skipped spots: ' + total.skippedTop + ' of ' + total.skipped + ' inside the top ' + topN + '.' : ''));
}
