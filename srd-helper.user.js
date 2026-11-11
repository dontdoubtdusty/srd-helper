// ==UserScript==
// @name         SRD Helper
// @namespace    srd-helper
// @version      0.4.2
// @description  Drama triage for reddit threads: scores the open page, shows the hot spots, exports them for SRD Desk. Reads the open page only; no API.
// @updateURL    https://raw.githubusercontent.com/dontdoubtdusty/srd-helper/main/srd-helper.user.js
// @downloadURL  https://raw.githubusercontent.com/dontdoubtdusty/srd-helper/main/srd-helper.user.js
// @icon         data:image/png;base64,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
// @match        https://old.reddit.com/*
// @match        https://www.reddit.com/*
// @match        https://new.reddit.com/*
// @match        https://sh.reddit.com/*
// @grant        none
// ==/UserScript==

// NO AUTOMATION RULE: this script only reads what is already on the page. It must never click, scroll, load,
// expand, post or fetch anything on the user's behalf (no "load more" pressing, no crawling, no background requests).
// One exception, decided by the user: the "Expand all" button, which clicks collapsed comments on old.reddit only when
// the user presses it. No timers, no loops, no "load more". On new reddit the script only reads the page and offers one
// link ("Open in old reddit") that opens a new tab when the user clicks it; it never redirects by itself.
// Reddit can flag automated activity on an account. Anything that would act for the user does not belong here.

// Everything is wrapped in a function so nothing leaks into the page's global scope
// (like keeping a class internal instead of public).
(function () {
  'use strict';

  // The mascot picture (64 x 56 PNG, transparent background), used in the panel headers.
  const MASCOT = 'data:image/png;base64,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';

  // Shown in the panel; keep in step with @version above.
  const VERSION = '0.4.2';

  // ---- Settings you may want to change ----
  const CONFIG = {
    chainMinDepth: 3,     // a "chain" = a negative-score comment with at least this many levels (itself included)
    minChainAuthors: 2,   // distinct authors a chain needs (2 = at least two people arguing, not one person replying to themselves)
    minChainComments: 4,  // total comments in a chain, side branches included (width as well as depth)
    minThreadComments: 20, // loaded comments needed before the panel will judge a thread at all
    promisingAt: 50,      // meter percentage where the verdict becomes "Promising"
    dramaAt: 85,          // meter percentage where the verdict becomes "DRAMA-FILLED"
    loadMoreWorth: 12,    // a "load more" button scoring at least this (replies x context) is highlighted on the page
    loadMoreMinReplies: 3, // ... and it must hide at least this many replies
    linkHost: 'www.reddit.com', // host used for comment permalinks (check the sub's rules; np.reddit.com is the alternative)
    maxAutoChainLength: 6, // longest chain SRD Desk builds by default (written into the export; the app can override it)
    // Listing pages (a subreddit, multireddit, r/all):
    listMinComments: 50,  // threads with fewer comments are ignored by the ranking
    listHotRatio: 1.0,    // comments per point at or above this gets a red badge (more comments than points)
    listWarmRatio: 0.5,   // ... at or above this gets an amber badge
    listTopN: 10,         // how many threads the ranked list shows
    minCoverage: 0.5,          // below this share loaded, "below threshold" is reported as unreliable
    // Focus view categories:
    focusTopN: 3,         // winners shown per category
    crowdMinAuthors: 3,   // "Crowd Magnet" needs at least this many distinct commenters
    duelMinComments: 4,   // "Duel" needs at least this many comments alternating between the same two people
    duelHeatWeight: 0.5,  // each heated comment in a duel counts as this many extra comments when ranking
    // Words and phrases that mark a comment as heated (matched as whole words; plurals and -ed/-ing/-ly endings are allowed). Edit freely.
    heatWords: ['idiot', 'idiotic', 'stupid', 'moron', 'moronic', 'dumb', 'dumbass', 'liar', 'lying', 'shut up', 'pathetic', 'clown', 'delusional',
      'ignorant', 'troll', 'trash', 'garbage', 'bullshit', 'fuck', 'fucking', 'shit', 'asshole', 'bitch', 'loser', 'braindead', 'brain dead',
      'hypocrite', 'disgusting', 'toxic', 'nobody asked', 'get a life', 'touch grass', 'go away', 'grow up', 'you people', 'bad faith', 'gaslight'],
  };

  // Turn a score label into a number; null when hidden or unknown.
  function parseScore(text) {
    if (text === null || text === undefined) return null;
    const m = String(text).replace(/,/g, '').match(/-?\d+/);
    return m ? parseInt(m[0], 10) : null;
  }

  // Build the markdown for one chain. Items are in thread order: { depth, text, url, linked }.
  // Nesting level is each comment's depth relative to the shallowest picked comment.
  function buildChain(items) {
    if (!items.length) return '';
    const minDepth = Math.min(...items.map(i => i.depth));
    const out = [];
    let prevLevel = 0;
    items.forEach((it, idx) => {
      const level = it.depth - minDepth + 1;
      const prefix = '>'.repeat(level) + ' ';
      const paras = it.text.split(/\n\n+/).map(p => p.replace(/\s+/g, ' ').trim()).filter(Boolean);
      if (idx > 0) out.push('>'.repeat(Math.min(prevLevel, level)));
      if (it.linked) {
        // A linked quote is one line: the comment text is the link text.
        out.push(prefix + '[' + paras.join(' ') + '](' + it.url + ')');
      } else {
        paras.forEach((p, i) => {
          if (i > 0) out.push('>'.repeat(level));
          out.push(prefix + p);
        });
      }
      prevLevel = level;
    });
    return out.join('\n');
  }

  // ---- Focus-view analysis: plain functions on a comment tree of { author, text, kids, heat } ----

  // One regex that matches any heat word as a whole word, with common endings allowed.
  function makeHeatRegex(words) {
    const esc = w => w.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    return new RegExp('\\b(?:' + words.map(esc).join('|') + ')(?:s|es|ed|ing|ly|er)?\\b', 'gi');
  }
  const heatHits = (text, re) => ((text || '').match(re) || []).map(s => s.toLowerCase());

  // A comment and everything below it, as a flat list.
  function flatten(node, out = []) { out.push(node); node.kids.forEach(k => flatten(k, out)); return out; }

  // Distinct commenters in a comment's subtree (all deleted accounts count as one).
  function authorSet(node) { return new Set(flatten(node).map(n => n.author || '[deleted]')); }

  // Pick the best candidates without letting two picks share a comment.
  function pickNonOverlapping(cands, nodesOf, topN) {
    const chosen = [], taken = new Set();
    for (const c of cands) {
      if (chosen.length >= topN) break;
      const nodes = nodesOf(c);
      if (nodes.some(n => taken.has(n))) continue;
      nodes.forEach(n => taken.add(n));
      chosen.push(c);
    }
    return chosen;
  }

  // Crowd Magnet: the comments whose subtree holds the most distinct commenters.
  function rankCrowd(roots, topN, minAuthors) {
    const cands = [];
    roots.forEach(r => flatten(r).forEach(n => {
      const authors = authorSet(n).size;
      if (authors >= minAuthors) cands.push({ node: n, authors, size: flatten(n).length });
    }));
    cands.sort((x, y) => (y.authors - x.authors) || (y.size - x.size));
    return pickNonOverlapping(cands, c => flatten(c.node), topN);
  }

  // Longest reply line starting at node where its author and one other author keep alternating.
  function runFrom(node, a, b) {
    let best = [node];
    node.kids.forEach(k => {
      if (k.author === b) {
        const sub = runFrom(k, b, a);
        if (sub.length + 1 > best.length) best = [node].concat(sub);
      }
    });
    return best;
  }

  // Duel: the longest back-and-forth between exactly two people, with heated words adding weight.
  function rankDuels(roots, topN, minComments, heatWeight) {
    const cands = [];
    roots.forEach(r => flatten(r).forEach(s => {
      if (!s.author || s.author === '[deleted]') return;
      const others = new Set(s.kids.map(k => k.author).filter(a => a && a !== s.author && a !== '[deleted]'));
      others.forEach(b => {
        const path = runFrom(s, s.author, b);
        if (path.length < minComments) return;
        const heat = path.filter(x => x.heat > 0).length; // heated comments in the run, not word hits
        cands.push({ path, a: s.author, b, heat, score: path.length + heatWeight * heat });
      });
    }));
    cands.sort((x, y) => (y.score - x.score) || (y.path.length - x.path.length));
    return pickNonOverlapping(cands, c => c.path, topN);
  }

  // ---- Drama model: scores every comment, finds the hot spots, and rates the thread ----
  // Works on plain nodes { author, score, kids, controv, heat, t, more }. A node gets d (its drama score), hot (rank among
  // the thread's hot spots, 1 = best) and info (the numbers behind d). Pure functions, so they can be tested without a browser.

  // How much each ingredient counts. The numbers were tuned on real threads; calibrate.js shows how well they match your picks.
  const DRAMA = {
    level: 1.6,        // per reply level below the comment
    people: 4.5,       // times ln(distinct commenters)
    size: 3.0,         // times ln(comments in the subtree)
    heat: 1.2,         // per heated comment (a comment counts at most 3 times)
    controv: 1.0,      // per comment Reddit marks controversial
    negative: 1.6,     // per negative comment, times ln(1 + how far below zero)
    duel: 0.9,         // per comment of a two-person back-and-forth beyond the second
    fast: 2.0,         // flat bonus when 6+ comments land within an hour
    hidden: 0.15,      // per reply still hidden behind "load more" (capped)
    minLevels: 3, minPeople: 2, minSize: 4, // what a hot spot needs at least
    topSpots: 15,      // how many hot spots are ranked
    scale: 200,        // meter: total / scale goes through 1 - exp(-x) to give 0..100; set so the example threads land where they feel right
  };

  // Annotate every node with d and its parts, bottom-up. Returns the flat list of all nodes (parents before kids).
  function scoreNodes(roots, K = DRAMA) {
    const all = [];
    roots.forEach(r => flatten(r, all));
    // Walk backwards so every reply is finished before its parent.
    const parent = new Map();
    all.forEach(n => n.kids.forEach(k => parent.set(k, n)));
    for (let i = all.length - 1; i >= 0; i--) {
      const n = all[i];
      const own = {
        heat: Math.min(n.heat || 0, 3),
        controv: n.controv ? 1 : 0,
        neg: n.score !== null && n.score < 0 ? Math.log(1 + Math.abs(n.score)) : 0,
        negCount: n.score !== null && n.score < 0 ? 1 : 0,
        more: n.more || 0,
      };
      n.info = {
        size: 1, levels: 1, people: new Set([n.author || '[deleted]']),
        heated: own.heat > 0 ? 1 : 0, heatPts: own.heat, controv: own.controv, neg: own.neg, negCount: own.negCount, more: own.more,
        tmin: n.t || null, tmax: n.t || null,
      };
    }
    for (let i = all.length - 1; i >= 0; i--) {
      const n = all[i], f = n.info, up = parent.get(n);
      if (up) {
        const u = up.info;
        u.size += f.size; u.levels = Math.max(u.levels, f.levels + 1);
        f.people.forEach(p => u.people.add(p));
        u.heated += f.heated; u.heatPts += f.heatPts; u.controv += f.controv; u.neg += f.neg; u.negCount += f.negCount; u.more += f.more;
        if (f.tmin) u.tmin = u.tmin ? Math.min(u.tmin, f.tmin) : f.tmin;
        if (f.tmax) u.tmax = u.tmax ? Math.max(u.tmax, f.tmax) : f.tmax;
      }
    }
    all.forEach(n => {
      const f = n.info;
      // Longest two-person back-and-forth starting here.
      let duel = 1;
      if (n.author && n.author !== '[deleted]') {
        new Set(n.kids.map(k => k.author).filter(a => a && a !== n.author && a !== '[deleted]'))
          .forEach(b => { duel = Math.max(duel, runFrom(n, n.author, b).length); });
      }
      f.duel = duel;
      f.fast = f.size >= 6 && f.tmin && f.tmax && (f.tmax - f.tmin) <= 3600 ? 1 : 0;
      const parts = {
        depth: K.level * (f.levels - 1),
        people: K.people * Math.log(f.people.size),
        size: K.size * Math.log(f.size),
        heat: K.heat * f.heatPts,
        controv: K.controv * f.controv,
        negative: K.negative * f.neg,
        duel: K.duel * Math.max(0, duel - 2),
        fast: K.fast * f.fast,
        hidden: K.hidden * Math.min(f.more, 40),
      };
      f.parts = parts;
      n.d = Object.values(parts).reduce((a, b) => a + b, 0);
      f.peopleN = f.people.size;
    });
    return all;
  }

  // Pick the hot spots: split a tree into the disputes that score highest on their own. A comment is kept as a whole
  // when it scores more than the best split of its replies; otherwise its replies compete separately. Sets node.hot.
  function rankHotSpots(roots, K = DRAMA) {
    const ok = n => n.info.levels >= K.minLevels && n.info.peopleN >= K.minPeople && n.info.size >= K.minSize;
    const best = new Map(); // node -> { score, spots }
    const all = [];
    roots.forEach(r => flatten(r, all));
    for (let i = all.length - 1; i >= 0; i--) {
      const n = all[i];
      let split = 0, spots = [];
      n.kids.forEach(k => { const b = best.get(k); split += b.score; spots = spots.concat(b.spots); });
      if (ok(n) && n.d >= split) best.set(n, { score: n.d, spots: [n] });
      else best.set(n, { score: split, spots });
    }
    const spots = [];
    roots.forEach(r => spots.push(...best.get(r).spots));
    spots.sort((a, b) => b.d - a.d);
    all.forEach(n => { n.hot = 0; });
    spots.slice(0, K.topSpots).forEach((n, i) => { n.hot = i + 1; });
    return spots.slice(0, K.topSpots);
  }

  // The thread's drama total: the summed score of its hot spots.
  function dramaTotal(spots) { return spots.reduce((a, n) => a + n.d, 0); }

  // Total -> 0..100 percent (saturating, so a huge thread cannot run away).
  function dramaPct(total, K = DRAMA) { return Math.max(0, 100 * (1 - Math.exp(-total / K.scale))); }

  // How worth pressing a "load more" button is. replies = hidden replies, parent = the comment it sits under (a scored
  // node or null for the top level), spot = the parent lies inside a hot spot. Returns { worth, why }.
  function loadWorth(replies, parent, inSpot) {
    const why = [];
    let ctx = 1;
    if (parent && parent.info) {
      const f = parent.info;
      if (parent.score !== null && parent.score < 0) { ctx += 0.8; why.push('negative'); }
      if (parent.controv) { ctx += 0.8; why.push('controversial'); }
      if (parent.heat) { ctx += 0.6; why.push('heated'); }
      if (f.peopleN >= 3) { ctx += 0.7; why.push(f.peopleN + ' people already'); }
      if (f.levels >= 3) { ctx += 0.5; why.push(f.levels + ' levels deep'); }
    }
    if (inSpot) { ctx += 1.0; why.push('inside a hot spot'); }
    let worth = replies * ctx;
    if (!parent) { worth *= 0.5; why.push('top level'); }
    return { worth, why };
  }

  // ---- Drama-O-Meter: the verdict for the percentage ----

  // Everything the panel and the export need to show the verdict: words, color tier, and the percentage.
  // stats.pct is the drama percentage from the hot spots. pct is null when the thread cannot be judged
  // (too small, scores hidden, too little loaded).
  function meterState(stats, cfg) {
    const coverage = stats.total ? Math.min(stats.loaded / stats.total, 1) : 1;
    let cls = '', text = 'Below your threshold', judged = true;
    if (stats.loaded < cfg.minThreadComments) {
      judged = false;
      if (stats.more) { cls = 'srd-unknown'; text = 'Only ' + stats.loaded + ' comments loaded: open more yourself'; }
      else text = 'Thread too small to judge (' + stats.loaded + ' comments)';
    } else if (stats.hidden > stats.loaded / 2) { judged = false; cls = 'srd-unknown'; text = 'Scores mostly hidden: cannot judge'; }
    // Tiers compare the rounded percentage, the number people see (49.6 shows as 50% and counts as 50).
    else if (Math.round(stats.pct) >= cfg.dramaAt) { cls = 'srd-hot'; text = 'DRAMA-FILLED'; }
    else if (Math.round(stats.pct) >= cfg.promisingAt) { cls = 'srd-good'; text = 'Promising'; }
    // A "below threshold" verdict means little when most of the thread has not loaded.
    if (judged && !cls && stats.more && coverage < cfg.minCoverage) {
      cls = 'srd-unknown';
      text = 'Below threshold so far: only ' + Math.round(coverage * 100) + '% loaded';
    }
    const tier = cls === 'srd-hot' ? 'hot' : (cls === 'srd-good' ? 'good' : (cls === 'srd-unknown' ? 'unknown' : 'none'));
    return {
      coverage, cls, text, tier, judged,
      pct: judged ? stats.pct : null,
      plus: !!stats.more, // more comments are still unloaded, so the percentage can only go up
    };
  }

  // ---- Small shared helper ----

  // Escape text for use inside HTML.
  function esc(s) { return String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;'); }

  // When loaded outside a browser (the node test), export the pure functions and stop.
  if (typeof document === 'undefined') {
    module.exports = { parseScore, buildChain, CONFIG, makeHeatRegex, heatHits, flatten, rankCrowd, rankDuels, esc, meterState,
      DRAMA, scoreNodes, rankHotSpots, dramaTotal, dramaPct, loadWorth };
    return;
  }

  // ---- Readers: answer questions about a comment element ----
  // The panel code below asks these objects instead of touching Reddit's markup directly.
  // OLD is the full reader (old.reddit). NEW only reads: it is used for the estimate on new reddit.

  const isOldSite = location.hostname === 'old.reddit.com';

  // Old reddit: div.thing.comment, children inside .child > .sitetable.
  const OLD = {
    name: 'old',
    present: () => !!document.querySelector('.commentarea'),
    all: () => Array.from(document.querySelectorAll('.commentarea .thing.comment')),
    // The comment this one is a reply to (null for a top-level comment).
    parent: el => { const p = el.parentElement; return p ? p.closest('.thing.comment') : null; },
    score(el) {
      const s = el.querySelector(':scope > .entry .tagline .score.unvoted') || el.querySelector(':scope > .entry .tagline .score');
      if (!s) return null;
      const t = parseScore(s.getAttribute('title'));
      return t !== null ? t : parseScore(s.textContent);
    },
    text(el) {
      const md = el.querySelector(':scope > .entry .usertext-body .md');
      if (!md) return '';
      const paras = Array.from(md.children).map(c => c.textContent.trim()).filter(Boolean);
      return paras.length ? paras.join('\n\n') : md.textContent.trim();
    },
    permalink: el => el.getAttribute('data-permalink') || '',
    author: el => el.getAttribute('data-author') || '',
    // When the comment was written, in seconds since 1970 (null when the page does not say).
    time(el) {
      const t = el.querySelector(':scope > .entry .tagline time[datetime]');
      const ms = t ? Date.parse(t.getAttribute('datetime')) : NaN;
      return isNaN(ms) ? null : Math.round(ms / 1000);
    },
    collapsed: el => el.classList.contains('collapsed'),
    controversial: el => !!(el.querySelector(':scope > .entry.controversial') || el.classList.contains('controversial')),
    // "Load more comments (N replies)" buttons with their reply counts.
    // "Continue this thread" links open another page, so they are not buttons we can press.
    moreButtons() {
      return Array.from(document.querySelectorAll('.commentarea .morecomments a'))
        .filter(a => !a.closest('.deepthread') && !/continue this thread/i.test(a.textContent))
        .map(a => {
          const m = a.textContent.match(/(\d+)\s+repl/i);
          return { el: a, replies: m ? parseInt(m[1], 10) : 0 };
        });
    },
    // "Continue this thread" links sit in .deepthread, so this does not have to test the text of every link in the thread.
    deepCount: () => document.querySelectorAll('.commentarea .deepthread a').length,
    // The thread's author, and whether a comment was written by them (Reddit's own "submitter" marker also counts).
    // Pass the author in when asking about many comments, so the page is not searched for it each time.
    postAuthor() { const t = document.querySelector('#siteTable .thing.link'); return t ? (t.getAttribute('data-author') || '') : ''; },
    isOp(el, op = this.postAuthor()) {
      const a = el.getAttribute('data-author');
      return !!(op && op !== '[deleted]' && a === op) || !!el.querySelector(':scope > .entry .tagline a.author.submitter');
    },
    totalComments() { const t = document.querySelector('#siteTable .thing.link'); return t ? (parseScore(t.getAttribute('data-comments-count')) || 0) : 0; },
    // Where the small "HOT" badge goes inside a comment.
    badgeHost: el => el.querySelector(':scope > .entry .tagline'),
  };

  // New reddit (the "shreddit" page): <shreddit-comment> elements nest their replies, and carry author, score, depth and
  // permalink as attributes. Written against a saved page; if Reddit renames these, the estimate shows "no comments found".
  const NEW = {
    name: 'new',
    present: () => !!document.querySelector('shreddit-comment'),
    all: () => Array.from(document.querySelectorAll('shreddit-comment')),
    parent: el => { const p = el.parentElement; return p ? p.closest('shreddit-comment') : null; },
    score: el => parseScore(el.getAttribute('score')),
    text(el) {
      const box = el.querySelector(':scope > [slot="comment"]');
      if (!box) return '';
      const paras = Array.from(box.querySelectorAll('p')).map(p => p.textContent.replace(/\s+/g, ' ').trim()).filter(Boolean);
      return paras.length ? paras.join('\n\n') : box.textContent.replace(/\s+/g, ' ').trim();
    },
    permalink: el => el.getAttribute('permalink') || '',
    author: el => el.getAttribute('author') || '',
    time(el) { const ms = Date.parse(el.getAttribute('created') || ''); return isNaN(ms) ? null : Math.round(ms / 1000); },
    collapsed: el => el.hasAttribute('collapsed'),
    controversial: () => false,
    moreButtons: () => [],
    deepCount: () => 0,
    postAuthor() { const p = document.querySelector('shreddit-post'); return p ? (p.getAttribute('author') || '') : ''; },
    isOp(el, op = this.postAuthor()) { const a = el.getAttribute('author'); return !!(op && op !== '[deleted]' && a === op); },
    totalComments() { const p = document.querySelector('shreddit-post'); return p ? (parseScore(p.getAttribute('comment-count')) || 0) : 0; },
    badgeHost: () => null,
  };

  // The comment reader for the open page, or null when it is not a thread page.
  const reader = () => {
    if (isOldSite) return OLD.present() ? OLD : null;
    return NEW.present() ? NEW : null;
  };
  const onThread = () => /\/comments\//.test(location.pathname);

  // Read every comment once into a plain tree, so nothing has to search the page again. Each entry holds the comment
  // element, score, author, parent, kids, levels (comments in the deepest reply path) and size (comments in the subtree).
  // The list is in page order, so a parent always comes before its replies.
  function buildInfo(R, comments) {
    const info = new Map();
    comments.forEach(el => info.set(el, { el, score: R.score(el), author: R.author(el) || '[deleted]', parent: null, kids: [], levels: 1, size: 1 }));
    comments.forEach(el => {
      const p = R.parent(el), node = info.get(el);
      const up = p ? info.get(p) : null;
      if (up) { node.parent = up; up.kids.push(node); }
    });
    for (let i = comments.length - 1; i >= 0; i--) {
      const node = info.get(comments[i]), up = node.parent;
      if (!up) continue;
      up.size += node.size;
      if (node.levels + 1 > up.levels) up.levels = node.levels + 1;
    }
    return info;
  }

  // Distinct authors in a comment's subtree (all deleted accounts count as one).
  function authorCount(node) {
    const set = new Set();
    (function add(n) { set.add(n.author); n.kids.forEach(add); })(node);
    return set.size;
  }

  // Copy a live comment (and its replies) into a plain node that can be scored, rendered and saved.
  // n is an entry from buildInfo; ctx holds the shared pieces (reader, chain heads, controversial set, heat regex, id counter, OP name).
  // The live element stays on the node as el (and is left out of the export).
  function toNode(ctx, n) {
    const el = n.el, text = ctx.R.text(el);
    const words = heatHits(text, ctx.re);
    const node = {
      id: ctx.counter.n++,
      author: n.author,
      op: ctx.R.isOp(el, ctx.op),
      score: n.score,
      text,
      link: 'https://' + CONFIG.linkHost + ctx.R.permalink(el),
      chain: ctx.heads.has(el),
      controv: ctx.controv.has(el),
      heat: words.length,
      words,
      tags: [],
      t: ctx.R.time(el),
      more: 0,
      kids: n.kids.map(k => toNode(ctx, k)),
      el,
    };
    ctx.byEl.set(el, node);
    return node;
  }

  // Read the whole page: the classic chain flags, the plain comment tree, drama scores and hot spots, and the totals.
  // Chain heads are negative comments with enough depth, size and distinct authors, not nested in another chain.
  function analyze(R, comments = R.all()) {
    const info = buildInfo(R, comments);
    const buttons = R.moreButtons();
    const stats = {
      loaded: comments.length,
      collapsed: comments.filter(R.collapsed).length,
      more: buttons.length,
      total: R.totalComments(),
      deep: R.deepCount(),
      negative: 0, hidden: 0, chains: 0, controversial: 0,
    };
    const heads = new Set();   // classic chain-head comments
    const controv = new Set(); // comments Reddit marks controversial

    function walk(node, inChain) {
      const el = node.el, score = node.score;
      if (score === null) stats.hidden++;
      else if (score < 0) stats.negative++;
      if (R.controversial(el)) { stats.controversial++; controv.add(el); }
      let next = inChain;
      if (!inChain && score !== null && score < 0 && node.levels >= CONFIG.chainMinDepth && node.size >= CONFIG.minChainComments &&
          authorCount(node) >= CONFIG.minChainAuthors) { stats.chains++; heads.add(el); next = true; }
      node.kids.forEach(k => walk(k, next));
    }
    const tops = comments.filter(el => !info.get(el).parent);
    tops.forEach(t => walk(info.get(t), false));

    // The plain tree for the whole loaded thread, then the drama scores.
    const op = R.postAuthor();
    const ctx = { R, heads, controv, re: makeHeatRegex(CONFIG.heatWords), counter: { n: 0 }, op, byEl: new Map() };
    const roots = tops.map(t => toNode(ctx, info.get(t)));
    buttons.forEach(b => { const p = R.parent(b.el); const node = p && ctx.byEl.get(p); if (node) node.more += b.replies; });
    scoreNodes(roots);
    const spots = rankHotSpots(roots);
    stats.hot = spots.length;
    stats.dramaTotal = dramaTotal(spots);
    stats.pct = dramaPct(stats.dramaTotal);
    // New reddit does not show its "load more" buttons to us, so compare against the comment count it reports.
    if (R.name === 'new' && stats.total > stats.loaded * 1.1) stats.more = 1;
    return { stats, roots, spots, heads, controv, buttons, byEl: ctx.byEl, op };
  }

  // ---- Panel UI ----

  const css = document.createElement('style');
  css.textContent = `
    #srd-panel, #srd-lpanel { --pn:#141a21; --pn2:#1b222b; --ln:#2a323d; --tx:#d5dce5; --mut:#8a95a5; --acc:#4cb8a6; --hot:#f0734f; --warn:#e5b454; --ok:#5cc08a; }
    @media (prefers-color-scheme: light) { #srd-panel, #srd-lpanel { --pn:#fbfbfc; --pn2:#eef0f3; --ln:#d6dbe2; --tx:#1f262e; --mut:#5e6978; --acc:#0e8573; --hot:#c0410c; --warn:#946200; --ok:#1b7a47; } }
    #srd-panel, #srd-lpanel { position:fixed; top:60px; right:12px; z-index:99999; background:var(--pn); color:var(--tx); border:1px solid var(--ln); border-radius:8px;
      box-shadow:0 6px 24px rgba(0,0,0,.28); font:12.5px/1.5 system-ui,-apple-system,"Segoe UI",Roboto,sans-serif; max-height:86vh; overflow:auto;
      scrollbar-width:thin; scrollbar-color:var(--ln) transparent; box-sizing:border-box; }
    #srd-panel { width:320px; } #srd-lpanel { width:350px; }
    #srd-panel *, #srd-lpanel * { box-sizing:border-box; }
    .srd-head { display:flex; align-items:center; justify-content:space-between; gap:8px; padding:9px 12px; border-bottom:1px solid var(--ln); }
    .srd-brand, .srd-sec h5, .srd-kv .k, #srd-panel button, #srd-lpanel button { font-family:ui-monospace,"SF Mono",Menlo,Consolas,"Liberation Mono",monospace; }
    .srd-brand { font-size:11px; font-weight:700; letter-spacing:.12em; text-transform:uppercase; color:var(--acc); }
    .srd-subt { font-size:11.5px; color:var(--mut); }
    #srd-body, #srd-lbody { padding:2px 12px 12px; }
    .srd-sec { padding:11px 0; border-bottom:1px solid var(--ln); } .srd-sec:last-child { border-bottom:0; padding-bottom:0; }
    .srd-sec h5 { margin:0 0 8px; font-size:10.5px; font-weight:700; letter-spacing:.1em; text-transform:uppercase; color:var(--mut); }
    .srd-btns { display:flex; flex-wrap:wrap; align-items:center; gap:6px; }
    #srd-panel button, #srd-lpanel button { font-size:11.5px; color:var(--tx); background:var(--pn2); border:1px solid var(--ln); border-radius:5px; padding:4px 9px; margin:0; cursor:pointer;
      transition:border-color .12s, color .12s; }
    #srd-panel button:hover, #srd-lpanel button:hover { border-color:var(--acc); }
    #srd-panel button.srd-pri { color:var(--acc); border-color:var(--acc); } #srd-lpanel button.srd-on { color:var(--acc); border-color:var(--acc); background:var(--pn); }
    #srd-panel button:focus-visible, #srd-lpanel button:focus-visible, .srd-go:focus-visible { outline:2px solid var(--acc); outline-offset:1px; }
    #srd-toggle { padding:1px 8px !important; }
    .srd-status { font-size:11.5px; color:var(--mut); } #srd-msg { font-size:11.5px; color:var(--acc); }
.srd-meter { --tier:var(--mut); margin:8px 0 2px; padding:5px 10px 6px; border:1px solid var(--ln); border-radius:8px; background:var(--pn2); text-align:center;
      transition:border-color .3s, background .3s; }
    .srd-face { display:none; }
    .srd-meter[data-face="bored"] .srd-face.bored, .srd-meter[data-face="curious"] .srd-face.curious, .srd-meter[data-face="interested"] .srd-face.interested,
    .srd-meter[data-face="wow"] .srd-face.wow, .srd-meter[data-face="unsure"] .srd-face.unsure { display:inline; }
    .srd-meter.t-good { --tier:var(--ok); } .srd-meter.t-hot { --tier:var(--hot); } .srd-meter.t-unknown { --tier:var(--warn); }
    .srd-meter.t-good, .srd-meter.t-unknown { border-color:var(--tier); background:color-mix(in srgb, var(--tier) 9%, var(--pn)); }
    .srd-meter.t-hot { border-color:var(--tier); background:color-mix(in srgb, var(--tier) 14%, var(--pn)); }
    .srd-awning { position:relative; padding:9px 10px; border-radius:8px 8px 0 0; border-bottom:0 !important; justify-content:center;
      background:repeating-linear-gradient(90deg,#b81f33 0 11px,#f6edcf 11px 22px); }
    .srd-plate { display:flex; flex-direction:column; align-items:center; padding:3px 16px 5px; border-radius:16px; background:#f6edcf; box-shadow:0 0 0 3px #f6edcf; }
    .srd-mname { font:900 23px/1.05 "Arial Black",Impact,system-ui,sans-serif; letter-spacing:.02em; color:#ffc400; -webkit-text-stroke:4px #b81f33; paint-order:stroke fill; }
    .srd-mver { margin-top:2px; font:700 9.5px ui-monospace,"SF Mono",Menlo,Consolas,monospace; letter-spacing:.1em; text-transform:uppercase; color:#0b5f7a; }
    .srd-puff { position:absolute; top:50%; width:24px; height:22px; margin-top:-11px; }
    .srd-puff.l { left:8px; } .srd-puff.r { right:8px; }
    .srd-mascot { position:absolute; left:6px; top:50%; margin-top:-17.5px; box-sizing:content-box; padding:3px; border-radius:50%; background:#f6edcf; }
    .srd-brand-icon { height:18px; width:auto; margin-right:6px; vertical-align:middle; }
    #srd-panel .srd-awning #srd-toggle { position:absolute; right:6px; top:6px; background:#f6edcf; color:#5a1a1a; border-color:#5a1a1a; }
    .srd-gauge { display:block; width:auto; max-width:100%; height:56px; margin:2px auto 0; overflow:visible; }
    .srd-g-row { display:flex; justify-content:center; align-items:baseline; gap:8px; margin-top:2px; }
    .srd-track { fill:var(--ln); }
    .srd-tick { stroke:var(--mut); stroke-width:1.5; stroke-linecap:round; opacity:.6; }
    #srd-g-num { font:700 22px/1.1 ui-monospace,"SF Mono",Menlo,Consolas,monospace; color:var(--tx); }
    .srd-word { font-size:13px; font-weight:700; color:var(--tier); }
    .srd-meter.t-hot .srd-word { letter-spacing:.1em; text-transform:uppercase; }
    .srd-cap { margin-top:1px; font-size:11px; color:var(--mut); }
    .srd-kv { display:grid; grid-template-columns:1fr auto; gap:5px 12px; align-items:baseline; }
    .srd-kv .k { font-size:11.5px; color:var(--mut); } .srd-kv .v { font-weight:600; text-align:right; } .srd-kv .v.hi { color:var(--hot); }
    .srd-kv .n { grid-column:1 / -1; font-size:11.5px; color:var(--mut); margin-top:-2px; }
    .srd-bar { grid-column:1 / -1; height:4px; border-radius:2px; background:var(--pn2); overflow:hidden; } .srd-bar > i { display:block; height:100%; background:var(--acc); }
    #srd-panel.srd-min #srd-body { display:none; } #srd-panel.srd-min .srd-head { border-bottom:0; }
    /* Hot spots on the page: three strengths. The tint is a gradient layered over the comment's own background. */
    .thing.comment.srd-hot1 { border-left:7px solid #ff3b2f !important; background-image:linear-gradient(rgba(255,59,47,.17),rgba(255,59,47,.17)) !important; }
    .thing.comment.srd-hot2 { border-left:5px solid #ff8a1f !important; background-image:linear-gradient(rgba(255,138,31,.13),rgba(255,138,31,.13)) !important; }
    .thing.comment.srd-hot3 { border-left:3px solid #e5b454 !important; background-image:linear-gradient(rgba(229,180,84,.08),rgba(229,180,84,.08)) !important; }
    .srd-badge.srd-spotbadge { display:inline-flex; align-items:center; gap:4px; margin:0 0 0 8px; padding:1px 8px; border-radius:10px; font:700 10.5px ui-monospace,Menlo,Consolas,monospace; letter-spacing:.04em; color:#fff; border:0; }
    .srd-spotbadge i { font-style:normal; font-weight:600; opacity:.85; }
    .srd-spotbadge.t1 { background:#e32d22; box-shadow:0 0 8px rgba(255,59,47,.6); } .srd-spotbadge.t2 { background:#d9701a; } .srd-spotbadge.t3 { background:#8a7330; }
    .srd-err { margin:10px 0 0; padding:8px 10px; border:1px solid var(--hot); border-radius:6px; background:color-mix(in srgb, var(--hot) 16%, var(--pn)); color:var(--tx); font:11.5px/1.45 system-ui,sans-serif; word-break:break-word; }
    .srd-err[hidden] { display:none; }
    /* Repeated here with !important because some page styles override SVG fill attributes. */
    #srd-panel .srd-gfill { fill:url(#srd-g-grad) !important; }
    #srd-msg { margin-left:4px; }
    .srd-stamp { position:absolute; top:8px; left:50%; margin-left:-40px; padding:1px 7px; border:2px solid #ffd24a; border-radius:4px; transform:rotate(-4deg);
      font:900 10px/1.3 "Arial Black",Impact,system-ui,sans-serif; letter-spacing:.14em; text-transform:uppercase; color:#ffd24a; opacity:.9; }
    .srd-est { margin-top:8px; padding:6px 9px; border-left:3px solid var(--warn); border-radius:3px; background:var(--pn2); font:11.5px/1.45 system-ui,sans-serif; color:var(--mut); }
    .srd-go { display:flex; align-items:center; gap:10px; padding:11px 13px; border-radius:8px; text-decoration:none !important; color:#fff !important;
      background:linear-gradient(135deg,#e8402a,#f08a2c); box-shadow:0 3px 14px rgba(240,115,79,.4); transition:transform .12s, box-shadow .12s; }
    .srd-go:hover { transform:translateY(-1px); box-shadow:0 5px 18px rgba(240,115,79,.6); } .srd-go svg { flex:none; }
    .srd-go span { display:flex; flex-direction:column; font:800 14px/1.2 system-ui,sans-serif; letter-spacing:.01em; }
    .srd-go small { margin-top:2px; font:500 11px/1.3 system-ui,sans-serif; opacity:.92; }
    .srd-go.srd-go-sm { padding:8px 11px; } .srd-go.srd-go-sm span { font-size:13px; }
    /* Already in drama mode: a quiet, unclickable confirmation instead of the orange button. */
    .srd-go.srd-go-on { pointer-events:none; background:var(--pn2); box-shadow:none; color:var(--ok) !important; border:1px solid var(--ok); }
    .srd-more-spot { background:#ffe600 !important; color:#000 !important; outline:3px solid #ff2d95 !important; outline-offset:3px; border-radius:4px; font-weight:700 !important; box-shadow:0 0 14px 4px rgba(255,45,149,.65); }
    .srd-more-spot * { color:#000 !important; }
    .srd-more-list { display:grid; grid-template-columns:auto 1fr; gap:4px 10px; font-size:11.5px; }
    .srd-more-list .c { font-weight:700; text-align:right; color:var(--mut); } .srd-more-list .c.good { color:var(--hot); } .srd-more-list .w { color:var(--mut); }
    .srd-more-list .w small { display:block; font-size:10.5px; color:#6f8a7e; }
    .srd-controv { outline: 2px dashed #8a63c9; outline-offset: 2px; }

    /* Lower half: a soft dark "terminal" in both color schemes, so it contrasts with the popcorn-stand top. */
    :is(#srd-panel, #srd-lpanel) .srd-bd { --pn:#171d26; --pn2:#212a37; --ln:#303b4a; --tx:#cfd8e3; --mut:#7f8ea3; --acc:#5fd3bc; --hot:#ff8a65; --warn:#f0c060; --ok:#7fd99a;
      background:var(--pn); color:var(--tx); padding:2px 12px 12px; border-radius:0 0 8px 8px; font-family:ui-monospace,"SF Mono",Menlo,Consolas,"Liberation Mono",monospace; font-size:12px; }
    :is(#srd-panel, #srd-lpanel) .srd-bd .srd-sec h5 { text-transform:lowercase; letter-spacing:.04em; font-size:11.5px; color:var(--acc); }
    :is(#srd-panel, #srd-lpanel) .srd-bd .srd-sec h5::before { content:"$ "; color:var(--warn); }
    :is(#srd-panel, #srd-lpanel) .srd-bd .srd-kv .k { color:#8fb8e6; } :is(#srd-panel, #srd-lpanel) .srd-bd .srd-kv .k::after { content:":"; color:var(--mut); }
    :is(#srd-panel, #srd-lpanel) .srd-bd .srd-kv .v { color:var(--warn); font-weight:600; } :is(#srd-panel, #srd-lpanel) .srd-bd .srd-kv .v.hi { color:var(--hot); }
    :is(#srd-panel, #srd-lpanel) .srd-bd .srd-kv .n { color:#6f8a7e; } :is(#srd-panel, #srd-lpanel) .srd-bd .srd-kv .n::before { content:"// "; }
    :is(#srd-panel, #srd-lpanel) .srd-bd .srd-status { color:#6f8a7e; } :is(#srd-panel, #srd-lpanel) .srd-bd .srd-status:not(:empty)::before { content:"// "; }
    :is(#srd-panel, #srd-lpanel) .srd-bd button { border-radius:4px; } :is(#srd-panel, #srd-lpanel) .srd-bd button { display:inline-flex; align-items:center; gap:6px; } .srd-i { flex:none; color:var(--warn); }
    :is(#srd-panel, #srd-lpanel) .srd-bd .srd-bar > i { background:linear-gradient(90deg,#5fd3bc,#8fe3a8); }
    /* Top half: the popcorn-stand meter ticket. */
    #srd-panel #srd-meter { --pn:#3a2024; --pn2:#2a1a1f; --tx:#fff1cf; --mut:#e4c99a; --ok:#9be37a; --hot:#ff7a4a; --warn:#ffd24a;
      margin:8px 0 4px; border:2px dashed #f2b632; background:linear-gradient(180deg,#432428,#2c1a1f); position:relative; font-family:system-ui,-apple-system,"Segoe UI",Roboto,sans-serif; }
    #srd-panel #srd-meter.t-good { box-shadow:0 0 12px color-mix(in srgb,var(--ok) 35%,transparent); }
    #srd-panel #srd-meter.t-hot { box-shadow:0 0 16px color-mix(in srgb,var(--hot) 55%,transparent); }
    #srd-panel #srd-meter.t-unknown { box-shadow:0 0 12px color-mix(in srgb,var(--warn) 30%,transparent); }
    #srd-panel #srd-g-num { color:#ffd24a; text-shadow:0 2px 0 #b81f33; font-family:"Arial Black",Impact,system-ui,sans-serif; font-size:23px; }
    #srd-panel #srd-meter .srd-word { font-family:"Arial Black",Impact,system-ui,sans-serif; font-weight:900; font-size:14px; }
    #srd-panel #srd-meter .srd-cap { color:#e4c99a; font-family:system-ui,sans-serif; }
    #srd-panel .srd-confetti { position:absolute; width:14px; height:13px; opacity:.9; } #srd-panel .srd-confetti.a { left:8px; top:6px; transform:rotate(-14deg); } #srd-panel .srd-confetti.b { right:10px; bottom:24px; transform:rotate(12deg); }
  `;
  document.head.appendChild(css);

  // A short mark on the gauge at p percent, so the tier boundaries (50 and 85) are visible.
  function gaugeTick(p) {
    const x = (38 + 124 * p / 100).toFixed(1);
    return '<line class="srd-tick" x1="' + x + '" y1="50" x2="' + x + '" y2="54"/>';
  }

  const panel = document.createElement('div');
  panel.id = 'srd-panel';
  panel.innerHTML = `
    <div class="srd-head srd-awning">
      <img class="srd-mascot" src="${MASCOT}" alt="" width="32" height="29">
      <div class="srd-plate"><span class="srd-mname">Popcornometer</span><span class="srd-mver">Version ${VERSION}</span></div>
      <button id="srd-toggle" title="Collapse panel">_</button>
    </div>
    <div id="srd-body" class="srd-bd">
      <div class="srd-meter t-none" id="srd-meter" data-face="bored">
        <svg class="srd-confetti a" viewBox="0 0 24 22" aria-hidden="true"><g stroke="#5a1a1a" stroke-width="3"><circle cx="7" cy="13" r="5.5"/><circle cx="12" cy="8" r="6"/><circle cx="17" cy="13" r="5.5"/></g><g fill="#fbf3d6"><circle cx="7" cy="13" r="5.5"/><circle cx="12" cy="8" r="6"/><circle cx="17" cy="13" r="5.5"/><rect x="7" y="12" width="10" height="5"/></g></svg><svg class="srd-confetti b" viewBox="0 0 24 22" aria-hidden="true"><g stroke="#5a1a1a" stroke-width="3"><circle cx="7" cy="13" r="5.5"/><circle cx="12" cy="8" r="6"/><circle cx="17" cy="13" r="5.5"/></g><g fill="#fbf3d6"><circle cx="7" cy="13" r="5.5"/><circle cx="12" cy="8" r="6"/><circle cx="17" cy="13" r="5.5"/><rect x="7" y="12" width="10" height="5"/></g></svg>
        <svg class="srd-gauge" viewBox="0 0 200 54" role="img" aria-label="Popcornometer">
          <defs>
            <linearGradient id="srd-g-grad" gradientUnits="userSpaceOnUse" x1="38" y1="0" x2="162" y2="0">
              <stop offset="0" stop-color="#f7d04a"/><stop offset=".5" stop-color="#f08a2c"/><stop offset="1" stop-color="#e0301e"/>
            </linearGradient>
          </defs>
          <!-- left character: reacts to the meter (one face visible at a time, chosen by data-face on the meter) -->
          <g>
            <circle cx="16" cy="36" r="14" fill="#f6c744" stroke="#d9962a" stroke-width="1.5"/>
            <rect x="4" y="38" width="5" height="3" fill="#f09a6a" opacity=".7"/><rect x="23" y="38" width="5" height="3" fill="#f09a6a" opacity=".7"/>
            <!-- bored: heavy flat lids, flat mouth -->
            <g class="srd-face bored">
              <rect x="6" y="34" width="8" height="2.6" fill="#7a4a1c"/><rect x="18" y="34" width="8" height="2.6" fill="#7a4a1c"/>
              <rect x="6" y="29.5" width="8" height="1.6" fill="#b9792a"/><rect x="18" y="29.5" width="8" height="1.6" fill="#b9792a"/>
              <rect x="11" y="42" width="10" height="2.4" fill="#7a4a1c"/>
            </g>
            <!-- curious: round eyes, one brow lifted, small mouth -->
            <g class="srd-face curious">
              <circle cx="10" cy="34" r="2.3" fill="#7a4a1c"/><circle cx="22" cy="34" r="2.3" fill="#7a4a1c"/>
              <rect x="6" y="29.5" width="8" height="1.6" fill="#b9792a"/><rect x="18" y="26.5" width="8" height="1.6" fill="#b9792a" transform="rotate(-8 22 27.3)"/>
              <rect x="12" y="42" width="8" height="2.2" fill="#7a4a1c"/>
            </g>
            <!-- interested: bigger eyes, both brows up, small open mouth -->
            <g class="srd-face interested">
              <circle cx="10" cy="34" r="3.8" fill="#fffaf0" stroke="#7a4a1c" stroke-width="1"/><circle cx="22" cy="34" r="3.8" fill="#fffaf0" stroke="#7a4a1c" stroke-width="1"/>
              <circle cx="10.6" cy="34.4" r="1.8" fill="#7a4a1c"/><circle cx="22.6" cy="34.4" r="1.8" fill="#7a4a1c"/>
              <rect x="6" y="27" width="8" height="1.6" fill="#b9792a"/><rect x="18" y="27" width="8" height="1.6" fill="#b9792a"/>
              <ellipse cx="16" cy="43" rx="2.6" ry="2.8" fill="#7a4a1c"/>
            </g>
            <!-- intrigued: wide eyes, brows high, big open mouth -->
            <g class="srd-face wow">
              <circle cx="10" cy="33.5" r="4.8" fill="#fffaf0" stroke="#7a4a1c" stroke-width="1"/><circle cx="22" cy="33.5" r="4.8" fill="#fffaf0" stroke="#7a4a1c" stroke-width="1"/>
              <circle cx="10" cy="33.5" r="2.5" fill="#7a4a1c"/><circle cx="22" cy="33.5" r="2.5" fill="#7a4a1c"/>
              <circle cx="9" cy="32.4" r=".9" fill="#fffaf0"/><circle cx="21" cy="32.4" r=".9" fill="#fffaf0"/>
              <rect x="5.5" y="24.5" width="9" height="1.6" fill="#b9792a"/><rect x="17.5" y="24.5" width="9" height="1.6" fill="#b9792a"/>
              <ellipse cx="16" cy="43.5" rx="4" ry="4.4" fill="#7a4a1c"/><ellipse cx="16" cy="45.5" rx="2.4" ry="1.8" fill="#d8645a"/>
            </g>
            <!-- unsure (cannot judge): dot eyes, one brow slanted, wavy mouth -->
            <g class="srd-face unsure">
              <circle cx="10" cy="34" r="2.3" fill="#7a4a1c"/><circle cx="22" cy="34" r="2.3" fill="#7a4a1c"/>
              <rect x="6" y="29.5" width="8" height="1.6" fill="#b9792a" transform="rotate(10 10 30.3)"/><rect x="18" y="27" width="8" height="1.6" fill="#b9792a" transform="rotate(-10 22 27.8)"/>
              <path d="M9.5 43 q3.2 -2.6 6.5 0 t6.5 0" fill="none" stroke="#7a4a1c" stroke-width="2" stroke-linecap="round"/>
            </g>
          </g>
          <!-- the bar: a wedge that grows taller and redder -->
          <polygon class="srd-track" points="38,48 162,48 162,12"/>
          <polygon id="srd-g-fill" class="srd-gfill" points="38,48 38,48 38,48" fill="url(#srd-g-grad)"/>
          ${gaugeTick(50)}${gaugeTick(85)}
          <!-- 100% marker: striped popcorn bucket heaped with popcorn -->
          <g>
            <polygon points="167,27 199,27 195,52 171,52" fill="#fffaf0"/>
            <polygon points="167,27 172.3,27 175,52 171,52" fill="#e8402a"/>
            <polygon points="177.7,27 183,27 183,52 179,52" fill="#e8402a"/>
            <polygon points="188.3,27 193.7,27 191,52 187,52" fill="#e8402a"/>
            <g fill="#f5b82a" stroke="#f5b82a" stroke-width="2.6" stroke-linejoin="round">
              <circle cx="172" cy="24" r="4.5"/><circle cx="180" cy="22" r="5"/><circle cx="189" cy="23" r="5"/><circle cx="195" cy="25" r="4"/>
              <circle cx="176" cy="16" r="4.6"/><circle cx="185" cy="14" r="5"/><circle cx="192" cy="17" r="4.4"/><circle cx="181" cy="9.5" r="3.8"/>
            </g>
            <g fill="#fcdc72">
              <circle cx="172" cy="24" r="4.5"/><circle cx="180" cy="22" r="5"/><circle cx="189" cy="23" r="5"/><circle cx="195" cy="25" r="4"/>
              <circle cx="176" cy="16" r="4.6"/><circle cx="185" cy="14" r="5"/><circle cx="192" cy="17" r="4.4"/><circle cx="181" cy="9.5" r="3.8"/>
            </g>
            <g fill="#e08a2a"><path d="M178 20l3 1-1 3z"/><path d="M187 12l3 1-2 3z"/><path d="M190 21l3 1-2 3z"/><path d="M174 15l2.5 1-1.5 2.5z"/></g>
          </g>
        </svg>
        <div class="srd-g-row"><div id="srd-g-num">&ndash;</div><div class="srd-word" id="srd-g-word"></div></div>
        <div class="srd-cap" id="srd-g-cap"></div>
        ${isOldSite ? '' : '<div class="srd-stamp">estimate</div>'}
      </div>
      <div id="srd-err" class="srd-err" hidden></div>
      <section class="srd-sec"><h5>Thread</h5><div id="srd-stats"></div></section>
      ${isOldSite ? `
      <section class="srd-sec"><h5>Drama mode</h5>
        <a id="srd-old" class="srd-go srd-go-sm" href="#" target="_blank" rel="noopener"><svg class="srd-i" viewBox="0 0 16 16" width="14" height="14" aria-hidden="true"><path d="M9 2.5h4.5V7M13.5 2.5L7 9M11 10v3.5H2.5V5H6" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg><span id="srd-old-label">Reopen in drama mode<small>most controversial first, up to 500 comments, new tab</small></span></a>
      </section>
      <section class="srd-sec" id="srd-more-sec"><h5>To load</h5><div id="srd-more"></div></section>
      <section class="srd-sec"><h5>Page</h5>
        <div class="srd-btns"><button id="srd-refresh"><svg class="srd-i" viewBox="0 0 16 16" width="12" height="12" aria-hidden="true"><path d="M13.5 8a5.5 5.5 0 1 1-1.7-4M13 2v3.5H9.5" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"/></svg>Refresh</button><button id="srd-expand" title="Open every collapsed comment (never presses load more)"><svg class="srd-i" viewBox="0 0 16 16" width="12" height="12" aria-hidden="true"><path d="M3 6l5 5 5-5" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"/></svg>Expand all</button></div>
        <div class="srd-status">only clicks when you press Expand all</div>
      </section>
      <section class="srd-sec"><h5>Export</h5>
        <div class="srd-btns"><button class="srd-pri" id="srd-export" title="Download the whole loaded thread, scored, as a file you open in SRD Desk"><svg class="srd-i" viewBox="0 0 16 16" width="12" height="12" aria-hidden="true"><path d="M8 2v8M4.5 7L8 10.5 11.5 7M3 13.5h10" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"/></svg>Export for app</button><span id="srd-msg"></span></div>
      </section>` : `
      <section class="srd-sec"><h5>Full reading</h5>
        <a id="srd-old" class="srd-go" href="#" target="_blank" rel="noopener"><svg class="srd-i" viewBox="0 0 16 16" width="14" height="14" aria-hidden="true"><path d="M9 2.5h4.5V7M13.5 2.5L7 9M11 10v3.5H2.5V5H6" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg><span>Open in old reddit<small>drama mode: most controversial first, up to 500 comments</small></span></a>
        <div class="srd-status">opens a new tab only when you click</div>
      </section>
      <section class="srd-sec"><div class="srd-btns"><button id="srd-refresh"><svg class="srd-i" viewBox="0 0 16 16" width="12" height="12" aria-hidden="true"><path d="M13.5 8a5.5 5.5 0 1 1-1.7-4M13 2v3.5H9.5" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"/></svg>Refresh</button><span id="srd-msg"></span></div></section>`}
    </div>`;
  document.body.appendChild(panel);

  const $ = id => document.getElementById(id);
  const say = m => { const el = $('srd-msg'); if (!el) return; el.textContent = m; setTimeout(() => { el.textContent = ''; }, 4000); };

  // Grow the filled wedge of the gauge to p percent, easing from where it was. The wedge shape is drawn from points
  // (not clipped or sized with CSS), because some pages break SVG clipping and sizes with their own styles.
  let gaugeNow = 0, gaugeAnim = 0, gaugeEnd = 0;
  function setGauge(p) {
    const from = gaugeNow, start = performance.now(), ms = 700;
    const draw = v => {
      const x = 38 + 124 * v / 100, y = 48 - 36 * v / 100;
      $('srd-g-fill').setAttribute('points', '38,48 ' + x.toFixed(1) + ',48 ' + x.toFixed(1) + ',' + y.toFixed(1));
    };
    cancelAnimationFrame(gaugeAnim);
    const step = now => {
      const t = Math.max(0, Math.min(1, (now - start) / ms)), e = 1 - Math.pow(1 - t, 3); // the frame time can be a little before start
      gaugeNow = from + (p - from) * e;
      draw(gaugeNow);
      if (t < 1) gaugeAnim = requestAnimationFrame(step);
    };
    gaugeAnim = requestAnimationFrame(step);
    // Frames pause in background tabs; make sure the gauge still ends in the right place.
    clearTimeout(gaugeEnd);
    gaugeEnd = setTimeout(() => { cancelAnimationFrame(gaugeAnim); gaugeNow = p; draw(p); }, ms + 60);
  }

  // Drive the gauge: arc fill, needle, big number, verdict words and the one-line explanation.
  // Values are set through the style API so strict page policies do not block them.
  function renderMeter(m, stats) {
    $('srd-meter').className = 'srd-meter t-' + m.tier;
    const p = m.pct === null ? 0 : Math.min(100, m.pct);
    // The left character's mood: unsure when the meter cannot judge, otherwise bored below 50%, then curious,
    // interested from 70%, and intrigued from 85% (the DRAMA-FILLED line).
    $('srd-meter').setAttribute('data-face', m.cls === 'srd-unknown' ? 'unsure' : Math.round(p) >= 85 ? 'wow' : Math.round(p) >= 70 ? 'interested' : Math.round(p) >= 50 ? 'curious' : 'bored');
    setGauge(p);
    $('srd-g-num').textContent = m.pct === null ? (m.cls === 'srd-unknown' ? '?' : '–') : Math.round(m.pct) + '%' + (m.plus ? '+' : '');
    $('srd-g-word').textContent = m.text;
    $('srd-g-cap').textContent = m.pct === null ? '' :
      stats.hot + (stats.hot === 1 ? ' hot spot' : ' hot spots') + (m.plus ? ' so far' : '') +
      ' · promising ' + CONFIG.promisingAt + '%, drama ' + CONFIG.dramaAt + '%';
  }

  // Show a problem inside the panel instead of failing silently (the browser console is easy to miss).
  function showError(e, what) {
    const box = $('srd-err');
    if (!box) return;
    const line = String((e && e.stack) || e).split('\n').slice(0, 3).join(' | ');
    box.textContent = 'SRD Helper hit a problem' + (what ? ' while ' + what : '') + ': ' + (e && e.message ? e.message : e) + '  [' + line.slice(0, 200) + ']';
    box.hidden = false;
    if (typeof console !== 'undefined') console.error('SRD Helper', e);
  }
  const clearError = () => { const box = $('srd-err'); if (box && !box.hidden) box.hidden = true; };

  // Run a button action; an error shows in the panel rather than disappearing.
  const guard = (what, fn) => () => { try { fn(); clearError(); } catch (e) { showError(e, what); } };

  // The newest analysis, shared by the panel, the page marks and the export.
  let current = null;

  // Recompute stats and update the panel and the page. The panel hides itself off thread pages.
  function refresh() {
    const R = reader();
    const show = onThread() && (isOldSite ? !!R : true);
    panel.style.display = show ? '' : 'none';
    if (!show) { current = null; return; }
    // Keep the drama mode link pointing at the current thread. On old reddit, when the page already is in drama mode
    // (sorted by controversial, limit 500), the button says so instead of offering the same page again.
    const go = $('srd-old');
    if (go) {
      const u = oldRedditUrl();
      if (u && go.getAttribute('href') !== u) go.setAttribute('href', u);
      if (isOldSite) {
        const q = new URLSearchParams(location.search);
        const on = q.get('sort') === 'controversial' && q.get('limit') === '500';
        const label = on ? 'Drama mode is on<small>this page is sorted by controversial, up to 500 comments</small>'
          : 'Reopen in drama mode<small>most controversial first, up to 500 comments, new tab</small>';
        if ($('srd-old-label').innerHTML !== label) $('srd-old-label').innerHTML = label;
        go.classList.toggle('srd-go-on', on);
      }
    }
    if (!R) {
      current = null;
      renderMeter({ tier: 'unknown', cls: 'srd-unknown', pct: null, text: 'No comments found yet' }, { hot: 0 });
      $('srd-stats').innerHTML = '<div class="srd-status">New reddit has not loaded any comments here yet. Scroll down so it loads some, then press Refresh.</div>';
      return;
    }
    const A = analyze(R);
    current = A;
    const { stats } = A;
    // Verdict words and colors come from meterState, so the gauge, the words and the tiers always agree.
    // On new reddit the share loaded is always small, so the estimate is shown anyway instead of "too little loaded".
    const m = meterState(stats, isOldSite ? CONFIG : Object.assign({}, CONFIG, { minCoverage: 0 }));
    const covPct = Math.round(m.coverage * 100);
    const notes = [];
    if (stats.collapsed) notes.push(`${stats.collapsed} collapsed`);
    if (stats.more && isOldSite) notes.push(`${stats.more} "load more" left`);
    if (stats.deep) notes.push(`${stats.deep} deeper branch${stats.deep > 1 ? 'es' : ''} need their own page`);
    renderMeter(m, stats);
    const top = A.spots[0];
    const statsHtml = '<div class="srd-kv">' +
      `<span class="k">Loaded</span><span class="v">${stats.loaded}${stats.total ? ` / ~${stats.total}` : ''}</span>` +
      (stats.total ? '<span class="srd-bar"><i id="srd-bar-i"></i></span>' : '') +
      (notes.length ? `<span class="n">${notes.join(' &middot; ')}</span>` : '') +
      `<span class="k">Negative comments</span><span class="v">${stats.negative}</span>` +
      (stats.hidden ? `<span class="n">${stats.hidden} with hidden or missing scores</span>` : '') +
      `<span class="k">Hot spots</span><span class="v${stats.hot ? ' hi' : ''}">${stats.hot}</span>` +
      (top ? `<span class="n">best: ${top.info.size} comments, ${top.info.peopleN} people, ${top.info.levels} levels, drama ${Math.round(top.d)}</span>` : '') +
      (isOldSite ? `<span class="k">Marked controversial</span><span class="v">${stats.controversial}</span>` : '') +
      '</div>' +
      (isOldSite ? '' : '<div class="srd-est">Estimate. New reddit loads only part of a thread and hides most downvoted replies, so the real number is usually higher. Open the thread in old reddit for the full reading.</div>');
    // Rewrite the stats block only when it changed.
    if ($('srd-stats').innerHTML !== statsHtml) $('srd-stats').innerHTML = statsHtml;
    const barFill = $('srd-bar-i');
    if (barFill) barFill.style.width = covPct + '%'; // set through the style API so strict page policies do not block it
    if (isOldSite) { paintPage(R, A); renderMoreList(R, A); }
  }

  // Mark the page: hot spots in three strengths with a small badge, Reddit-controversial comments with a dashed outline.
  const spotBadges = new WeakMap();
  const FLAME = '<svg viewBox="0 0 16 16" width="11" height="11" aria-hidden="true"><path d="M8 1c.4 2.3-1.1 3.4-2.3 4.8C4.6 7.1 4 8.3 4 9.7 4 12.1 5.7 14 8 14s4-1.900 4-4.300c0-1.500-.7-2.700-1.500-3.600-.1 1-.5 1.700-1.100 2C9.700 5.800 9.300 3 8 1z" fill="currentColor"/></svg>';
  function paintPage(R, A) {
    const rankOf = new Map();
    A.spots.forEach(n => rankOf.set(n.el, n));
    A.byEl.forEach((node, el) => {
      const hot = rankOf.get(el);
      const tier = hot ? (hot.hot <= 3 ? 1 : hot.hot <= 10 ? 2 : 3) : 0;
      el.classList.toggle('srd-hot1', tier === 1);
      el.classList.toggle('srd-hot2', tier === 2);
      el.classList.toggle('srd-hot3', tier === 3);
      el.classList.toggle('srd-controv', node.controv);
      let b = spotBadges.get(el);
      if (!hot) { if (b && b.parentNode) b.remove(); return; }
      if (!b) { b = document.createElement('span'); spotBadges.set(el, b); }
      const html = FLAME + ' HOT #' + hot.hot + ' <i>drama ' + Math.round(hot.d) + '</i>';
      if (b.innerHTML !== html) b.innerHTML = html;
      const cls = 'srd-badge srd-spotbadge t' + tier;
      if (b.className !== cls) b.className = cls;
      if (!b.parentNode) { const host = R.badgeHost(el); if (host) host.appendChild(b); }
    });
  }

  // The "To load" list: which "load more" buttons are worth pressing. Read only: it does not scroll or click.
  // A button is worth pressing when the replies it hides sit under a comment that is already a fight (see loadWorth).
  // The good ones are lit up on the page and listed first; you press them yourself, then press Refresh.
  function renderMoreList(R, A) {
    const inSpot = new Set();
    A.spots.forEach(s => flatten(s).forEach(n => inSpot.add(n)));
    const rows = A.buttons.map(b => {
      const p = R.parent(b.el), node = p ? A.byEl.get(p) : null;
      const w = loadWorth(b.replies, node, !!node && inSpot.has(node));
      return { el: b.el, replies: b.replies, node, worth: w.worth, why: w.why,
        good: b.replies >= CONFIG.loadMoreMinReplies && w.worth >= CONFIG.loadMoreWorth };
    }).sort((a, b) => b.worth - a.worth);
    const lit = new Set(rows.filter(r => r.good).map(r => r.el));
    document.querySelectorAll('.srd-more-spot').forEach(a => { if (!lit.has(a)) a.classList.remove('srd-more-spot'); });
    lit.forEach(a => a.classList.add('srd-more-spot'));
    const shown = rows.slice(0, 8);
    const total = rows.reduce((n, r) => n + r.replies, 0);
    const html = !rows.length ? '<div class="srd-status">nothing left to load</div>' :
      '<div class="srd-more-list">' + shown.map(r => {
        const who = r.node ? esc(r.node.author) + (r.node.score !== null ? ' (' + r.node.score + ')' : '') : 'the top level';
        return `<span class="c${r.good ? ' good' : ''}">${r.replies || '?'}</span>` +
          `<span class="w">${r.replies === 1 ? 'reply' : 'replies'} under ${who}` +
          (r.why.length ? `<small>${esc(r.why.join(', '))}</small>` : '') + '</span>';
      }).join('') + '</div>' +
      `<div class="srd-status">${rows.length} button${rows.length > 1 ? 's' : ''}, about ${total} replies${rows.length > shown.length ? ', best ' + shown.length + ' shown' : ''}. ` +
      (lit.size ? `${lit.size} lit up on the page (worth ${CONFIG.loadMoreWorth}+): press them yourself, then Refresh.` : 'None look worth pressing.') + '</div>';
    const box = $('srd-more');
    if (box && box.innerHTML !== html) box.innerHTML = html;
  }

  // One full update of the panel, the page marks and the listing badges. Any error shows in the panel.
  function refreshAll() {
    try { refresh(); refreshListing(); clearError(); } catch (e) { showError(e, 'reading the page'); }
  }

  // The same thread on old.reddit, sorted by controversial with up to 500 comments. Used only for the link in the panel.
  function oldRedditUrl() {
    const m = location.pathname.match(/^((?:\/(?:r|user)\/[^/]+)?)\/comments\/([a-z0-9]+)(?:\/([^/]+))?/i);
    if (!m) return '';
    const slug = m[3] && m[3] !== 'comment' ? m[3] + '/' : '';
    return 'https://old.reddit.com' + m[1] + '/comments/' + m[2] + '/' + slug + '?sort=controversial&limit=500';
  }

  // ---- Export: turns the page into a thread file for SRD Desk ----

  // A comment as written to the file: everything except the live page element and the working numbers.
  function pack(n) {
    return {
      id: n.id, author: n.author, op: n.op, score: n.score, text: n.text, link: n.link,
      chain: n.chain, controv: n.controv, heat: n.heat, words: n.words, tags: n.tags,
      d: Math.round(n.d * 10) / 10, hot: n.hot || 0, t: n.t, more: n.more,
      m: { size: n.info.size, levels: n.info.levels, people: n.info.peopleN, heated: n.info.heated, neg: n.info.negCount, duel: n.info.duel },
      kids: n.kids.map(pack),
    };
  }

  // Everything the export needs: the whole loaded thread with drama scores, category winners as tags, and the summary strip.
  // Returns null when no comments are loaded.
  function workspaceData(R) {
    const A = analyze(R);
    const { stats, roots, spots } = A;
    if (!roots.length) return null;
    const crowd = rankCrowd(roots, CONFIG.focusTopN, CONFIG.crowdMinAuthors);
    const duels = rankDuels(roots, CONFIG.focusTopN, CONFIG.duelMinComments, CONFIG.duelHeatWeight);
    const addTag = (n, k, t) => n.tags.push({ k, t });
    crowd.forEach((c, i) => addTag(c.node, 'crowd', `Crowd Magnet #${i + 1}: ${c.authors} commenters`));
    duels.forEach((d, i) => d.path.forEach((n, j) => j === 0
      ? addTag(n, 'duel', `Duel #${i + 1}: ${d.a} vs ${d.b}, ${d.path.length} comments${d.heat ? `, ${d.heat} heated` : ''}`)
      : addTag(n, 'duelsmall', `Duel #${i + 1}`)));
    const everything = roots.reduce((a, r) => a.concat(flatten(r)), []);
    const meter = meterState(stats, CONFIG);
    const short = n => (n.text || '[no text]').replace(/\s+/g, ' ').slice(0, 38) + (n.text && n.text.length > 38 ? '...' : '');
    return {
      meter: { pct: meter.pct === null ? null : Math.round(meter.pct * 10) / 10, plus: meter.plus, text: meter.text, tier: meter.tier },
      title: document.title,
      threadUrl: 'https://' + CONFIG.linkHost + location.pathname,
      stats: { shown: roots.length, tops: roots.length, comments: everything.length, loaded: stats.loaded, chains: stats.chains, hot: spots.length },
      roots: roots.map(pack),
      strip: {
        crowd: crowd.map((c, i) => ({ id: c.node.id, t: `#${i + 1} ${c.node.author}: ${c.authors} commenters` })),
        duels: duels.map((d, i) => ({ id: d.path[0].id, t: `#${i + 1} ${d.a} vs ${d.b}: ${d.path.length} comments${d.heat ? `, ${d.heat} heated` : ''}` })),
        heated: everything.filter(n => n.heat).length,
        op: everything.filter(n => n.op).slice(0, 8).map(n => ({ id: n.id, t: short(n) })),
        hot: spots.map(n => ({ id: n.id, t: `#${n.hot} ${n.author}: drama ${Math.round(n.d)}, ${n.info.size} comments, ${n.info.peopleN} people` })),
      },
      op: A.op,
      maxChain: CONFIG.maxAutoChainLength,
    };
  }

  // Download the thread as a file for SRD Desk (the standalone app): the whole loaded thread with its drama scores.
  // It only writes out what is already on the page, when you press the button.
  function exportThread() {
    const R = reader();
    const data = R && workspaceData(R);
    if (!data) { say('No comments to export'); return; }
    const out = Object.assign({ format: 'srd-thread', version: 2, exportedAt: new Date().toISOString() }, data);
    const a = document.createElement('a');
    a.href = URL.createObjectURL(new Blob([JSON.stringify(out)], { type: 'application/json' }));
    a.download = 'srd-' + (location.pathname.split('/')[4] || 'thread') + '.srd.json';
    document.body.appendChild(a);
    a.click();
    a.remove();
    say('Exported for SRD Desk');
  }

  // ---- Listing pages: rank threads by how much argument they hold per point ----

  // Reader for a feed of threads (same idea as the comment reader above).
  const LIST_OLD = {
    items: () => Array.from(document.querySelectorAll('#siteTable > .thing.link')).filter(e => e.getAttribute('data-promoted') !== 'true'),
    read(el) {
      const t = el.querySelector('a.title');
      return {
        title: t ? t.textContent : '',
        sub: el.getAttribute('data-subreddit-prefixed') || ('r/' + (el.getAttribute('data-subreddit') || '')),
        score: parseScore(el.getAttribute('data-score')),
        comments: parseScore(el.getAttribute('data-comments-count')),
        ts: parseInt(el.getAttribute('data-timestamp'), 10) || null,
        path: el.getAttribute('data-permalink') || '',
        ratio: null, // old.reddit listings do not carry the upvote ratio
      };
    },
    place(el, badge) { (el.querySelector('.entry') || el).appendChild(badge); },
  };
  const listReader = () => {
    if (onThread()) return null;
    return LIST_OLD.items().length ? LIST_OLD : null;
  };

  let listSort = 'ratio'; // ratio = comments per point, velocity = comments per hour, upvote = lowest upvote %
  const badges = new WeakMap();

  // Per-thread numbers used for badges and ranking.
  function listStats(it) {
    const comments = it.comments || 0;
    const hours = it.ts ? Math.max((Date.now() - it.ts) / 3600000, 0.5) : null;
    return { comments, perPoint: comments / Math.max(it.score || 0, 1), perHour: hours ? comments / hours : 0, hours };
  }

  const lpanel = document.createElement('div');
  lpanel.id = 'srd-lpanel';
  lpanel.style.display = 'none';
  lpanel.innerHTML = `
    <div class="srd-head srd-awning">
      <img class="srd-mascot" src="${MASCOT}" alt="" width="32" height="29">
      <div class="srd-plate"><span class="srd-mname">SRD Helper</span><span class="srd-mver">Busiest threads &middot; ${VERSION}</span></div>
    </div>
    <div id="srd-lbody" class="srd-bd">
      <div class="srd-btns"><button data-sort="ratio">comments / point</button><button data-sort="velocity">comments / hour</button><button data-sort="upvote" id="srd-sort-upvote">lowest upvote %</button></div>
      <div id="srd-lrank"></div>
    </div>`;
  document.body.appendChild(lpanel);
  css.textContent += `
    #srd-lrank { margin-top:8px; } #srd-lrank > p { color:var(--mut); }
    #srd-lpanel .srd-row { margin:8px 0; padding:8px 10px; border:1px solid var(--ln); border-left:3px solid var(--ln); border-radius:6px; background:var(--pn2); }
    #srd-lpanel .srd-row.hot { border-left-color:var(--hot); } #srd-lpanel .srd-row.warm { border-left-color:var(--warn); }
    #srd-lpanel .srd-row a { color:var(--tx); text-decoration:none; font-weight:600; } #srd-lpanel .srd-row a:hover { color:var(--acc); }
    #srd-lpanel .srd-row small { display:block; margin-top:2px; font-size:11.5px; color:var(--mut); }
    .srd-badge { display:inline-block; margin:2px 0; padding:1px 8px; border-radius:10px; border:1px solid #c9cfd8; font:600 11px ui-monospace,Menlo,Consolas,monospace; background:#eceff3; color:#4a5461; }
    .srd-badge.warm { background:#f7efd2; border-color:#e0c968; color:#6b5200; } .srd-badge.hot { background:#f6dccf; border-color:#e59a7c; color:#8a2a0a; }
    @media (prefers-color-scheme: dark) {
      .srd-badge { background:#222a33; border-color:#344050; color:#aab4c0; }
      .srd-badge.warm { background:#352f1d; border-color:#6e5c24; color:#e5c36a; } .srd-badge.hot { background:#3a2119; border-color:#7d3d27; color:#f0936f; }
    }
  `;
  lpanel.addEventListener('click', e => {
    const s = e.target && e.target.getAttribute && e.target.getAttribute('data-sort');
    if (s) { listSort = s; refreshListing(); }
  });

  // Add a badge to every thread and rebuild the ranked list. Writes only on change so it does not retrigger itself.
  function refreshListing() {
    const L = listReader();
    lpanel.style.display = L ? '' : 'none';
    if (!L) return;
    const rows = [];
    L.items().forEach(el => {
      const it = L.read(el);
      const st = listStats(it);
      if (st.comments < CONFIG.listMinComments) return;
      const cls = st.perPoint >= CONFIG.listHotRatio ? 'hot' : (st.perPoint >= CONFIG.listWarmRatio ? 'warm' : '');
      const text = `${st.comments} comments, ${st.perPoint.toFixed(2)}/point` + (st.hours ? `, ${st.perHour.toFixed(0)}/hr` : '') + (it.ratio !== null ? `, ${Math.round(it.ratio * 100)}% upvoted` : '');
      let b = badges.get(el);
      if (!b) { b = document.createElement('div'); b.className = 'srd-badge'; badges.set(el, b); L.place(el, b); }
      if (b.textContent !== text) b.textContent = text;
      const want = 'srd-badge ' + cls;
      if (b.className !== want.trim()) b.className = want.trim();
      rows.push({ it, st, cls });
    });
    const haveRatio = rows.some(r => r.it.ratio !== null);
    $('srd-sort-upvote').style.display = haveRatio ? '' : 'none';
    if (listSort === 'upvote' && !haveRatio) listSort = 'ratio';
    const key = { ratio: r => -r.st.perPoint, velocity: r => -r.st.perHour, upvote: r => (r.it.ratio === null ? 2 : r.it.ratio) }[listSort];
    rows.sort((a, b) => key(a) - key(b));
    lpanel.querySelectorAll('button[data-sort]').forEach(b => b.classList.toggle('srd-on', b.getAttribute('data-sort') === listSort));
    const rankHtml = rows.length ? rows.slice(0, CONFIG.listTopN).map(r =>
      `<div class="srd-row ${r.cls}"><a href="${esc(location.origin + r.it.path)}" target="_blank" rel="noopener">${esc(r.it.title.slice(0, 90))}</a>` +
      `<small>${esc(r.it.sub)}: ${r.st.comments} comments, ${r.st.perPoint.toFixed(2)}/point${r.st.hours ? `, ${r.st.perHour.toFixed(0)}/hr, ${r.st.hours < 48 ? r.st.hours.toFixed(1) + 'h old' : Math.round(r.st.hours / 24) + 'd old'}` : ''}</small></div>`).join('')
      : `<p>No threads with ${CONFIG.listMinComments}+ comments loaded on this page.</p>`;
    if ($('srd-lrank').innerHTML !== rankHtml) $('srd-lrank').innerHTML = rankHtml;
  }

  // ---- Wiring ----

  $('srd-toggle').addEventListener('click', () => panel.classList.toggle('srd-min'));
  $('srd-refresh').addEventListener('click', refreshAll);
  if (isOldSite) {
    $('srd-export').addEventListener('click', guard('exporting', exportThread));
    // Expand all, one press (old.reddit only): click the "[+]" of every collapsed comment.
    // Never presses "load more".
    $('srd-expand').addEventListener('click', guard('expanding', () => {
      if (!reader()) return;
      let n = 0;
      document.querySelectorAll('.commentarea .thing.comment.collapsed > .entry a.expand').forEach(a => { a.click(); n++; });
      say(n ? 'Expanded ' + n : 'Nothing collapsed');
      setTimeout(refreshAll, 300);
    }));
  }

  // Comments appear later (load more, expand, navigating to a new thread), so re-run on page changes, debounced.
  // Changes the script makes itself (its panels and the badges it adds) are ignored, so an update does not set off a second one.
  // New reddit changes constantly, so it waits a little longer.
  const isOurs = n => n.nodeType === 1 && n.classList.contains('srd-badge');
  let timer = null;
  new MutationObserver(records => {
    const external = records.some(r => !(panel.contains(r.target) || lpanel.contains(r.target)) &&
      !(r.removedNodes.length === 0 && r.addedNodes.length > 0 && Array.from(r.addedNodes).every(isOurs)));
    if (!external) return;
    clearTimeout(timer);
    timer = setTimeout(refreshAll, isOldSite ? 400 : 900);
  }).observe(document.body, { childList: true, subtree: true });

  refreshAll();
})();
