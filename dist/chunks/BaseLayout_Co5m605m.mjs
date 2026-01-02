import { d as createAstro, c as createComponent, e as addAttribute, b as renderScript, a as renderTemplate, j as renderSlot, i as renderHead, r as renderComponent } from './astro/server_BbYws21E.mjs';
import 'piccolore';
/* empty css                                */
import 'clsx';

const $$Astro$1 = createAstro("https://basementos.com");
const $$ClientRouter = createComponent(($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro$1, $$props, $$slots);
  Astro2.self = $$ClientRouter;
  const { fallback = "animate" } = Astro2.props;
  return renderTemplate`<meta name="astro-view-transitions-enabled" content="true"><meta name="astro-view-transitions-fallback"${addAttribute(fallback, "content")}>${renderScript($$result, "C:/Users/cloud/basement-os-web/node_modules/astro/components/ClientRouter.astro?astro&type=script&index=0&lang.ts")}`;
}, "C:/Users/cloud/basement-os-web/node_modules/astro/components/ClientRouter.astro", void 0);

var __freeze = Object.freeze;
var __defProp = Object.defineProperty;
var __template = (cooked, raw) => __freeze(__defProp(cooked, "raw", { value: __freeze(cooked.slice()) }));
var _a;
const $$Astro = createAstro("https://basementos.com");
const $$BaseLayout = createComponent(($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro, $$props, $$slots);
  Astro2.self = $$BaseLayout;
  const { title, description = "Basement OS - An interactive dev blog for the Lower Level 2.0 VRChat world project" } = Astro2.props;
  return renderTemplate(_a || (_a = __template(['<html lang="en" data-theme="synthwave"> <head><meta charset="UTF-8"><meta name="description"', '><meta name="viewport" content="width=device-width"><link rel="icon" type="image/jpeg" href="/favicon.jpg"><meta name="generator"', "><title>", "</title>", "<script>\n            // Disable browser's scroll restoration to prevent progress bar on scroll\n            // Astro's ClientRouter calls history.replaceState during scroll which\n            // triggers native mobile browser progress indicators\n            if ('scrollRestoration' in history) {\n                history.scrollRestoration = 'manual';\n            }\n        <\/script><script>\n            // Immediately invoked function to set the theme before FOUC\n            (function() {\n                try {\n                    const savedTheme = localStorage.getItem('basement-os-theme') || 'synthwave';\n                    document.documentElement.setAttribute('data-theme', savedTheme);\n                } catch (e) {\n                    console.error('Theme detection failed', e);\n                }\n            })();\n        <\/script>", "</head> <body> ", " </body></html>"])), addAttribute(description, "content"), addAttribute(Astro2.generator, "content"), title, renderComponent($$result, "ClientRouter", $$ClientRouter, {}), renderHead(), renderSlot($$result, $$slots["default"]));
}, "C:/Users/cloud/basement-os-web/src/layouts/BaseLayout.astro", void 0);

export { $$BaseLayout as $ };
