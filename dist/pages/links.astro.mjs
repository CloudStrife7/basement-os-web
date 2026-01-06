import { c as createComponent, r as renderComponent, a as renderTemplate, m as maybeRenderHead } from '../chunks/astro/server_BbYws21E.mjs';
import 'piccolore';
import { $ as $$BaseLayout } from '../chunks/BaseLayout_Co5m605m.mjs';
import { $ as $$TerminalShell } from '../chunks/TerminalShell_-RdpUSL0.mjs';
export { renderers } from '../renderers.mjs';

const $$Links = createComponent(($$result, $$props, $$slots) => {
  return renderTemplate`${renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": "BASEMENT OS | Links" }, { "default": ($$result2) => renderTemplate` ${renderComponent($$result2, "TerminalShell", $$TerminalShell, { "activeSection": "links" }, { "default": ($$result3) => renderTemplate` ${maybeRenderHead()}<div class="content-section active"> <h2 class="section-header">C:\\BASEMENT\\LINKS.NET</h2> <div class="info-grid"> <div class="info-card"> <h3>[ GitHub Repository ]</h3> <p> <a href="https://github.com/CloudStrife7/LL2PCVR" class="link" target="_blank">
github.com/CloudStrife7/LL2PCVR
</a> </p> <p class="text-dim mt-1">Source code, docs, and issues</p> </div> <div class="info-card"> <h3>[ VRChat World ]</h3> <p> <a href="https://vrchat.com/home/launch?worldId=wrld_7302897c-be0f-4037-ac67-76f0ea065c2b" class="link" target="_blank">
Lower Level 2.0
</a> </p> <p class="text-dim mt-1">Visit the basement in VR</p> </div> <div class="info-card"> <h3>[ Weather API ]</h3> <p> <a href="https://cloudstrife7.github.io/DOS-Terminal/api/weather/current.json" class="link" target="_blank">
Weather Endpoint
</a> </p> <p class="text-dim mt-1">Real-time weather JSON feed</p> </div> <div class="info-card"> <h3>[ This Blog's Source ]</h3> <p> <a href="https://github.com/CloudStrife7/basement-os-web" class="link" target="_blank">
basement-os-web
</a> </p> <p class="text-dim mt-1">The code behind this site</p> </div> <div class="info-card"> <h3>[ 📚 DeepWiki: This Site ]</h3> <p> <a href="https://app.devin.ai/wiki/CloudStrife7/basement-os-web" class="link" target="_blank">
basement-os-web Wiki
</a> </p> <p class="text-dim mt-1">AI-generated documentation</p> </div> <div class="info-card"> <h3>[ 📚 DeepWiki: LL2 ]</h3> <p> <a href="https://app.devin.ai/wiki/CloudStrife7/LL2PCVR" class="link" target="_blank">
Lower Level 2.0 Wiki
</a> </p> <p class="text-dim mt-1">VRChat world documentation</p> </div> <div class="info-card featured"> <h3>[ ☕ Support the Project ]</h3> <p> <a href="https://ko-fi.com/J3J11HLPQ7" class="link" target="_blank">
Ko-Fi
</a> </p> <p class="text-dim mt-1">Buy me a coffee to fuel development</p> </div> </div> </div> ` })} ` })}`;
}, "C:/Users/cloud/basement-os-web/src/pages/links.astro", void 0);

const $$file = "C:/Users/cloud/basement-os-web/src/pages/links.astro";
const $$url = "/links";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$Links,
  file: $$file,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
