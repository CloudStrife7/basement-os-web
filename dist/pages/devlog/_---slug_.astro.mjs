import { d as createAstro, c as createComponent, r as renderComponent, a as renderTemplate, m as maybeRenderHead } from '../../chunks/astro/server_BbYws21E.mjs';
import 'piccolore';
import { g as getCollection, $ as $$DevlogEntry } from '../../chunks/_astro_content_CFgDY_-6.mjs';
import { $ as $$BaseLayout } from '../../chunks/BaseLayout_Co5m605m.mjs';
import { $ as $$TerminalShell } from '../../chunks/TerminalShell_-RdpUSL0.mjs';
export { renderers } from '../../renderers.mjs';

const $$Astro = createAstro("https://basementos.com");
async function getStaticPaths() {
  const posts = await getCollection("devlog");
  return posts.map((entry) => ({
    params: { slug: entry.slug },
    props: { entry }
  }));
}
const $$ = createComponent(async ($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro, $$props, $$slots);
  Astro2.self = $$;
  const { entry } = Astro2.props;
  const { Content } = await entry.render();
  return renderTemplate`${renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": `BASEMENT OS | ${entry.data.title}` }, { "default": async ($$result2) => renderTemplate` ${renderComponent($$result2, "TerminalShell", $$TerminalShell, { "activeSection": "devlog" }, { "default": async ($$result3) => renderTemplate` ${maybeRenderHead()}<div class="content-section active"> <!-- Render the entry using the component which handles title/date/tags/content --> ${renderComponent($$result3, "DevlogEntry", $$DevlogEntry, { "entry": entry })} </div> ` })} ` })}`;
}, "C:/Users/cloud/basement-os-web/src/pages/devlog/[...slug].astro", void 0);

const $$file = "C:/Users/cloud/basement-os-web/src/pages/devlog/[...slug].astro";
const $$url = "/devlog/[...slug]";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$,
  file: $$file,
  getStaticPaths,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
