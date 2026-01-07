import { c as createComponent, a as renderTemplate, r as renderComponent, m as maybeRenderHead } from '../chunks/astro/server_BbYws21E.mjs';
import 'piccolore';
import { $ as $$BaseLayout } from '../chunks/BaseLayout_Co5m605m.mjs';
/* empty css                                 */
export { renderers } from '../renderers.mjs';

var __freeze = Object.freeze;
var __defProp = Object.defineProperty;
var __template = (cooked, raw) => __freeze(__defProp(cooked, "raw", { value: __freeze(raw || cooked.slice()) }));
var _a;
const $$Codex = createComponent(($$result, $$props, $$slots) => {
  return renderTemplate(_a || (_a = __template(["", `  <script>
  /**
   * THE CODEX - Journal Page
   * 
   * Easter egg: The signature "Z" hints at more entries
   * Console message for explorers
   */
  (function() {
    console.log('%c\u{1F4DC} THE CODEX', 'color: #8b4513; font-size: 16px; font-weight: bold;');
    console.log('%cSeptember 2004 - Z\\'s first journal entry', 'color: #666;');
    console.log('%c"What if something outside of the game is reading back?"', 'color: #888; font-style: italic;');
    console.log('');
    console.log('%cMore entries exist. Keep looking.', 'color: #444;');
  })();
<\/script>`], ["", `  <script>
  /**
   * THE CODEX - Journal Page
   * 
   * Easter egg: The signature "Z" hints at more entries
   * Console message for explorers
   */
  (function() {
    console.log('%c\u{1F4DC} THE CODEX', 'color: #8b4513; font-size: 16px; font-weight: bold;');
    console.log('%cSeptember 2004 - Z\\\\'s first journal entry', 'color: #666;');
    console.log('%c"What if something outside of the game is reading back?"', 'color: #888; font-style: italic;');
    console.log('');
    console.log('%cMore entries exist. Keep looking.', 'color: #444;');
  })();
<\/script>`])), renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": "The Codex", "data-astro-cid-7bfwr7ng": true }, { "default": ($$result2) => renderTemplate` ${maybeRenderHead()}<div class="journal-page" data-astro-cid-7bfwr7ng> <!-- Paper texture overlay --> <div class="paper-texture" data-astro-cid-7bfwr7ng></div> <!-- Journal content --> <article class="journal-entry" data-astro-cid-7bfwr7ng> <header class="entry-header" data-astro-cid-7bfwr7ng> <span class="entry-date" data-astro-cid-7bfwr7ng>September 2004</span> </header> <div class="entry-content" data-astro-cid-7bfwr7ng> <p data-astro-cid-7bfwr7ng>
Look, this isn't some new idea. People have been breaking games in ways they weren't meant to run since the 1980's. Tilt a cartridge just right, and you don't just glitch the game—you make it start reading memory it was never supposed to. That's the real trick. It's not about what the hardware was designed to do, it's about what it can do when you push it past its limits.
</p> <p data-astro-cid-7bfwr7ng>
Old consoles are perfect for this. No modern security patches, no cloud syncing, no hidden firmware updates trying to keep everything locked down. Just raw circuits, reading and executing whatever they're given. And when they start pulling from memory addresses they shouldn't be? That's when things get interesting.
</p> <p data-astro-cid-7bfwr7ng>
We already know certain games act weird when pushed in just the right way. Speedrunners have walked into the end credits of games by rewriting code on the fly, just by moving in specific ways. MissingNo in Pokémon exists because the game expects data where there is none, so it just makes something up. What if that doesn't just happen inside the game? What if, for just a few frames, something outside of the game is reading back?
</p> </div> <footer class="entry-signature" data-astro-cid-7bfwr7ng> <span class="signature" data-astro-cid-7bfwr7ng>Z</span> </footer> </article> <!-- Navigation hint --> <div class="journal-nav" data-astro-cid-7bfwr7ng> <span class="nav-hint" data-astro-cid-7bfwr7ng>[ Entry 1 of ? ]</span> </div> </div> ` }));
}, "C:/Users/cloud/basement-os-web/src/pages/codex.astro", void 0);

const $$file = "C:/Users/cloud/basement-os-web/src/pages/codex.astro";
const $$url = "/codex";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$Codex,
  file: $$file,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
