// Checks the Drama-O-Meter against threads you have labelled (run with node, no packages needed).
//
//   node measure.js <thread.srd.json> [more files...]
//
// Each file is an export from the userscript (Export for app). Its thread id is looked up in examples/labels.json
// (drama or calm); unlabelled files are listed but not counted. Prints every thread with the new meter and the old
// hot-spot meter, then the AUC of each: the chance a drama thread scores above a calm one (0.5 = coin flip, 1 = perfect).
// Change METER in srd-helper.user.js and run again to see whether a change helps.

const fs = require('fs');
const path = require('path');
const U = require(path.join(__dirname, 'srd-helper.user.js'));
const labels = JSON.parse(fs.readFileSync(path.join(__dirname, 'examples', 'labels.json'), 'utf8')).threads;

const files = process.argv.slice(2);
if (!files.length) { console.log('usage: node measure.js <thread.srd.json> [more files...]'); process.exit(1); }

// A plain copy of an exported comment tree, in the shape the scoring functions take.
const plain = n => ({ author: n.author, score: n.score, controv: n.controv, heat: n.heat || 0, t: n.t, more: n.more || 0,
  text: n.text || '', kids: (n.kids || []).map(plain) });

const rows = [];
for (const f of files) {
  const d = JSON.parse(fs.readFileSync(f, 'utf8'));
  const id = ((d.threadUrl || '').match(/comments\/([a-z0-9]+)/i) || [])[1];
  const roots = d.roots.map(plain);
  const all = roots.reduce((a, r) => a.concat(U.flatten(r)), []);
  const hidden = all.filter(n => n.score === null).length;
  const s = U.threadShares(roots);
  U.scoreNodes(roots);
  const old = U.dramaPct(U.dramaTotal(U.rankHotSpots(roots)));
  rows.push({ id, label: labels[id] || '?', title: (d.title || path.basename(f)).slice(0, 40), n: all.length, s, old,
    skip: hidden > all.length / 2 ? 'scores hidden' : '' });
}

const tier = p => Math.round(p) >= U.CONFIG.dramaAt ? 'DRAMA' : Math.round(p) >= U.CONFIG.promisingAt ? 'promising' : '-';
rows.sort((a, b) => b.s.pct - a.s.pct);
for (const r of rows) {
  console.log(r.label.padEnd(5), r.title.padEnd(40), String(r.n).padStart(5), ' new', r.s.pct.toFixed(0).padStart(4), tier(r.s.pct).padEnd(9),
    ' old', r.old.toFixed(0).padStart(4), tier(r.old).padEnd(9), r.skip ? ' (not counted: ' + r.skip + ')' :
    `  bf ${(100 * r.s.backforth).toFixed(0)}% down ${(100 * r.s.bothDown).toFixed(1)}% ctv ${(100 * r.s.controv).toFixed(0)}% rem ${(100 * r.s.removed).toFixed(1)}%`);
}

// The chance a random drama thread outscores a random calm one, for one way of scoring.
const counted = rows.filter(r => !r.skip && r.label !== '?');
const pos = counted.filter(r => r.label === 'drama'), neg = counted.filter(r => r.label === 'calm');
function auc(get) {
  let w = 0;
  pos.forEach(p => neg.forEach(q => { w += get(p) > get(q) ? 1 : get(p) === get(q) ? 0.5 : 0; }));
  return pos.length && neg.length ? (w / (pos.length * neg.length)).toFixed(2) : 'n/a';
}
const hits = (get, want) => counted.filter(r => r.label === want && tier(get(r)) === 'DRAMA').length;
console.log(`\n${pos.length} drama, ${neg.length} calm counted.`);
console.log(`new meter: AUC ${auc(r => r.s.pct)}, DRAMA-FILLED on ${hits(r => r.s.pct, 'drama')}/${pos.length} drama and ${hits(r => r.s.pct, 'calm')}/${neg.length} calm`);
console.log(`old meter: AUC ${auc(r => r.old)}, DRAMA-FILLED on ${hits(r => r.old, 'drama')}/${pos.length} drama and ${hits(r => r.old, 'calm')}/${neg.length} calm`);
