import { d as createAstro, c as createComponent, r as renderComponent, b as renderScript, a as renderTemplate, m as maybeRenderHead } from '../chunks/astro/server_BbYws21E.mjs';
import 'piccolore';
import { $ as $$BaseLayout } from '../chunks/BaseLayout_Co5m605m.mjs';
import { $ as $$TerminalShell } from '../chunks/TerminalShell_-RdpUSL0.mjs';
/* empty css                                   */
export { renderers } from '../renderers.mjs';

const $$Astro = createAstro("https://basementos.com");
const $$Roadmap = createComponent(($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro, $$props, $$slots);
  Astro2.self = $$Roadmap;
  return renderTemplate`${renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": "BASEMENT OS | Roadmap", "data-astro-cid-khueswxm": true }, { "default": ($$result2) => renderTemplate` ${renderComponent($$result2, "TerminalShell", $$TerminalShell, { "activeSection": "roadmap", "data-astro-cid-khueswxm": true }, { "default": ($$result3) => renderTemplate` ${maybeRenderHead()}<div class="content-section active" data-astro-cid-khueswxm> <h2 class="section-header" data-astro-cid-khueswxm>C:\\BASEMENT\\ROADMAP.TXT</h2> <div class="mission-statement" data-astro-cid-khueswxm> <h3 data-astro-cid-khueswxm>Development Roadmap</h3> <p data-astro-cid-khueswxm>Live project status synced from GitHub Issues every 6 hours. Track what's being worked on, what's coming next, and the full backlog.</p> </div> <!-- CURRENT FOCUS --> <details class="roadmap-section section-primary" open data-astro-cid-khueswxm> <summary data-astro-cid-khueswxm>[ Current Focus ]</summary> <p class="section-desc" data-astro-cid-khueswxm>High priority items actively being worked on</p> <div id="current-focus" class="issues-grid" data-astro-cid-khueswxm> <div class="loading-indicator" data-astro-cid-khueswxm>Loading from GitHub...</div> </div> </details> <!-- MILESTONES --> <details class="roadmap-section section-primary" data-astro-cid-khueswxm> <summary data-astro-cid-khueswxm>[ Milestones ]</summary> <p class="section-desc" data-astro-cid-khueswxm>Major releases and feature sets</p> <div id="milestones" class="milestones-grid" data-astro-cid-khueswxm> <div class="loading-indicator" data-astro-cid-khueswxm>Loading from GitHub...</div> </div> </details> <!-- COMING SOON --> <details class="roadmap-section section-secondary" data-astro-cid-khueswxm> <summary data-astro-cid-khueswxm>[ Coming Soon ]</summary> <p class="section-desc" data-astro-cid-khueswxm>Medium priority - planned for upcoming releases</p> <div id="coming-soon" class="issues-grid" data-astro-cid-khueswxm> <div class="loading-indicator" data-astro-cid-khueswxm>Loading from GitHub...</div> </div> </details> <!-- BACKLOG --> <details class="roadmap-section section-muted" data-astro-cid-khueswxm> <summary data-astro-cid-khueswxm>[ Backlog ]</summary> <p class="section-desc" data-astro-cid-khueswxm>Ideas and enhancements for the future</p> <div id="backlog" class="issues-grid" data-astro-cid-khueswxm> <div class="loading-indicator" data-astro-cid-khueswxm>Loading from GitHub...</div> </div> </details> <!-- RECENTLY COMPLETED --> <details class="roadmap-section section-completed" data-astro-cid-khueswxm> <summary data-astro-cid-khueswxm>[ Recently Completed ]</summary> <p class="section-desc" data-astro-cid-khueswxm>Shipped in the last 30 days</p> <div id="completed" class="issues-grid" data-astro-cid-khueswxm> <div class="loading-indicator" data-astro-cid-khueswxm>Loading from GitHub...</div> </div> </details> <div class="separator" data-astro-cid-khueswxm>────────────────────────────────────────────────────────────────────</div> <div class="cta-section" data-astro-cid-khueswxm> <p style="color: var(--text-secondary); margin-bottom: 15px;" data-astro-cid-khueswxm>Want to suggest a feature or report a bug?</p> <a href="mailto:admin@BasementOS.com" class="cta-button-small" data-astro-cid-khueswxm> <span data-astro-cid-khueswxm>@</span> <span data-astro-cid-khueswxm>E-MAIL ME</span> </a> </div> </div> ` })} ` })}  ${renderScript($$result, "C:/Users/cloud/basement-os-web/src/pages/roadmap.astro?astro&type=script&index=0&lang.ts")}`;
}, "C:/Users/cloud/basement-os-web/src/pages/roadmap.astro", void 0);

const $$file = "C:/Users/cloud/basement-os-web/src/pages/roadmap.astro";
const $$url = "/roadmap";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$Roadmap,
  file: $$file,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
