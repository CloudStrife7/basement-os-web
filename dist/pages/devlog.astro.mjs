import { d as createAstro, c as createComponent, m as maybeRenderHead, e as addAttribute, a as renderTemplate, b as renderScript, r as renderComponent } from '../chunks/astro/server_BbYws21E.mjs';
import 'piccolore';
import { $ as $$BaseLayout } from '../chunks/BaseLayout_Co5m605m.mjs';
import { $ as $$TerminalShell } from '../chunks/TerminalShell_-RdpUSL0.mjs';
import { g as getCollection, $ as $$DevlogEntry } from '../chunks/_astro_content_CFgDY_-6.mjs';
import 'clsx';
/* empty css                                 */
export { renderers } from '../renderers.mjs';

const $$Astro$1 = createAstro("https://basementos.com");
const $$DevlogTOC = createComponent(($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro$1, $$props, $$slots);
  Astro2.self = $$DevlogTOC;
  const { posts } = Astro2.props;
  const grouped = posts.reduce((acc, post) => {
    const year = post.data.date.getFullYear();
    const month = post.data.date.toLocaleString("default", { month: "long" });
    if (!acc[year]) acc[year] = {};
    if (!acc[year][month]) acc[year][month] = [];
    acc[year][month].push(post);
    return acc;
  }, {});
  const years = Object.keys(grouped).map(Number).sort((a, b) => b - a);
  return renderTemplate`${maybeRenderHead()}<div class="devlog-toc" data-astro-cid-czoakvlo> <div class="toc-header" data-astro-cid-czoakvlo>📑 Table of Contents</div> <ul class="toc-tree" data-astro-cid-czoakvlo> ${years.map((year) => renderTemplate`<li class="toc-year" data-astro-cid-czoakvlo> <div class="toc-year-header" onclick="this.parentElement.querySelector('.toc-year-content').classList.toggle('collapsed'); this.querySelector('.toc-year-toggle').textContent = this.querySelector('.toc-year-toggle').textContent === '▶' ? '▼' : '▶';" data-astro-cid-czoakvlo> <span class="toc-year-toggle" data-astro-cid-czoakvlo>▼</span> <span data-astro-cid-czoakvlo>${year}</span> </div> <div class="toc-year-content" data-astro-cid-czoakvlo> ${Object.keys(grouped[year]).map((month) => renderTemplate`<div class="toc-month" data-astro-cid-czoakvlo> <div class="toc-month-header" onclick="this.nextElementSibling.classList.toggle('collapsed'); this.querySelector('.toc-month-toggle').textContent = this.querySelector('.toc-month-toggle').textContent === '▶' ? '▼' : '▶';" data-astro-cid-czoakvlo> <span class="toc-month-toggle" data-astro-cid-czoakvlo>▼</span> <span data-astro-cid-czoakvlo>${month}</span> </div> <div class="toc-month-entries" data-astro-cid-czoakvlo> ${grouped[year][month].map((post) => renderTemplate`<div class="toc-entry" data-astro-cid-czoakvlo> <a${addAttribute(`#entry-${post.slug}`, "href")} data-astro-cid-czoakvlo> <span data-astro-cid-czoakvlo> ${post.data.type === "milestone" && renderTemplate`<span class="milestone-badge" data-astro-cid-czoakvlo>★</span>`} ${post.data.title} </span> <span class="date" data-astro-cid-czoakvlo>${post.data.date.getDate().toString().padStart(2, "0")}</span> </a> </div>`)} </div> </div>`)} </div> </li>`)} </ul> </div> `;
}, "C:/Users/cloud/basement-os-web/src/components/DevlogTOC.astro", void 0);

const $$Astro = createAstro("https://basementos.com");
const $$TimelineScrubber = createComponent(($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro, $$props, $$slots);
  Astro2.self = $$TimelineScrubber;
  const { posts } = Astro2.props;
  const grouped = posts.reduce((acc, post) => {
    const year = post.data.date.getFullYear();
    const monthNum = post.data.date.getMonth();
    const month = post.data.date.toLocaleString("default", { month: "short" });
    if (!acc[year]) acc[year] = {};
    if (!acc[year][monthNum]) {
      acc[year][monthNum] = {
        name: month,
        entries: []
      };
    }
    acc[year][monthNum].entries.push(post);
    return acc;
  }, {});
  const years = Object.keys(grouped).map(Number).sort((a, b) => b - a);
  const allItems = [];
  years.forEach((year) => {
    Object.keys(grouped[year]).map(Number).sort((a, b) => b - a).forEach((monthNum) => {
      allItems.push({
        year,
        monthNum,
        monthName: grouped[year][monthNum].name,
        slug: grouped[year][monthNum].entries[0]?.slug || ""
      });
    });
  });
  return renderTemplate`<!-- Timeline Scrubber - Full Height Vertical -->${maybeRenderHead()}<div class="timeline-scrubber" id="timeline-scrubber" data-astro-cid-ghvfjqv5> <!-- Current position indicator (moves with scroll) --> <div class="timeline-current" id="timeline-current" data-astro-cid-ghvfjqv5> <span class="timeline-month-label" id="timeline-month-label" data-astro-cid-ghvfjqv5></span> <span class="timeline-year-label" id="timeline-year-label" data-astro-cid-ghvfjqv5></span> </div> <!-- Vertical track with dots --> <div class="timeline-track" id="timeline-track" data-astro-cid-ghvfjqv5> <div class="timeline-line" data-astro-cid-ghvfjqv5></div> ${allItems.map((item, index) => renderTemplate`<a${addAttribute(`#entry-${item.slug}`, "href")} class="timeline-dot"${addAttribute(item.year, "data-year")}${addAttribute(item.monthNum, "data-month")}${addAttribute(item.monthName, "data-month-name")}${addAttribute(index, "data-index")}${addAttribute(`top: ${index / Math.max(allItems.length - 1, 1) * 100}%`, "style")} data-astro-cid-ghvfjqv5> <span class="dot-marker" data-astro-cid-ghvfjqv5>•</span> <span class="dot-label" data-astro-cid-ghvfjqv5>${item.year}</span> </a>`)} </div> </div>  ${renderScript($$result, "C:/Users/cloud/basement-os-web/src/components/TimelineScrubber.astro?astro&type=script&index=0&lang.ts")}`;
}, "C:/Users/cloud/basement-os-web/src/components/TimelineScrubber.astro", void 0);

const $$Index = createComponent(async ($$result, $$props, $$slots) => {
  const posts = (await getCollection("devlog")).sort(
    (a, b) => b.data.date.valueOf() - a.data.date.valueOf()
  );
  return renderTemplate`${renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": "BASEMENT OS | Devlog" }, { "default": async ($$result2) => renderTemplate` ${renderComponent($$result2, "TerminalShell", $$TerminalShell, { "activeSection": "devlog" }, { "default": async ($$result3) => renderTemplate` ${maybeRenderHead()}<div class="content-section active"> <h2 class="section-header">C:\\BASEMENT\\DEVLOG.LOG</h2> ${renderComponent($$result3, "DevlogTOC", $$DevlogTOC, { "posts": posts })} <div class="devlog-feed"> ${posts.map((post) => renderTemplate`${renderComponent($$result3, "DevlogEntry", $$DevlogEntry, { "entry": post })}`)} </div> </div>  ${renderComponent($$result3, "TimelineScrubber", $$TimelineScrubber, { "posts": posts })} ` })} ` })}`;
}, "C:/Users/cloud/basement-os-web/src/pages/devlog/index.astro", void 0);

const $$file = "C:/Users/cloud/basement-os-web/src/pages/devlog/index.astro";
const $$url = "/devlog";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$Index,
  file: $$file,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
