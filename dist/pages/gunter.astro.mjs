import { c as createComponent, a as renderTemplate, r as renderComponent, m as maybeRenderHead } from '../chunks/astro/server_BbYws21E.mjs';
import 'piccolore';
import { $ as $$BaseLayout } from '../chunks/BaseLayout_Co5m605m.mjs';
/* empty css                                  */
export { renderers } from '../renderers.mjs';

var __freeze = Object.freeze;
var __defProp = Object.defineProperty;
var __template = (cooked, raw) => __freeze(__defProp(cooked, "raw", { value: __freeze(raw || cooked.slice()) }));
var _a;
const $$Gunter = createComponent(($$result, $$props, $$slots) => {
  return renderTemplate(_a || (_a = __template(["", `  <script>
  /**
   * \u256D\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u256E
   * \u2502  GUNTER PROTOCOL - Glitch Effects Engine                    \u2502
   * \u2502  v2.0.0.5 (The 2005 release, obviously)                     \u2502
   * \u2570\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u256F
   * 
   * Hidden Commands (for those who grep):
   * - Type "iddqd" anywhere: Nothing happens. This isn't DOOM.
   * - Type "konami": Also nothing. But we appreciate the attempt.
   * - Type "xyzzy": A hollow voice says "Plugh."
   * 
   * Secret: The \u{1F422} at the bottom isn't random.
   * "Slow and steady wins the race" - but also:
   * "Turtle" -> "Tuttle" -> Brazil (1985) -> bureaucratic dystopia
   * -> The absurdity of hunting eggs in a simulation within a game
   *     documented on a website you're reading right now.
   * 
   * Or it's just a cute turtle. You decide.
   * 
   * P.S. - The weather in Lower Level is real. Check Fond du Lac.
   *        When it rains there, it rains in the basement.
   */

  (function() {
    'use strict';

    // ==========================================
    // Configuration
    // ==========================================
    const CONFIG = {
      // Matrix rain settings
      matrix: {
        fontSize: 14,
        fallSpeed: 0.5,        // Very slow
        density: 0.03,         // Sparse columns
        flickerChance: 0.001   // Rare white flicker
      },
      // Text glitch settings
      glitch: {
        chance: 0.005,         // 0.5% chance per character
        interval: [8000, 15000], // Every 8-15 seconds
        duration: 150,         // ms
        maxChars: 2            // Max simultaneous glitched chars
      },
      // Scan line settings
      scanLine: {
        interval: [45000, 90000] // Every 45-90 seconds
      },
      // Chromatic aberration
      chromatic: {
        scrollChance: 0.05     // 1 in 20 scrolls
      }
    };

    // Glitch characters pool
    const GLITCH_CHARS = '\u0337\u0338\u0335\u0336\u0322\u0321\u0328\u031B\u0353\u0320\u0323\u0324\u0325\u0326\u0329\u032A\u032B\u032C\u032D\u032E\u032F\u0330\u0331\u0332\u0333\u0300\u0301\u0302\u0303\u0304\u0305\u0306\u0307\u0308\u0309\u030A\u030B\u030C\u030D\u030E\u030F\u0310\u0311\u0312\u0313\u0314\u0315\u031A\u0358\u035C\u0362\u035C\u035F\u0360\u0361\u0345@#$%^&*\u2588\u2593\u2592\u2591';

    // State
    let effectsEnabled = true;
    let animationFrameId = null;

    // ==========================================
    // Matrix Rain Effect
    // ==========================================
    function initMatrixRain() {
      const canvas = document.getElementById('matrix-rain');
      if (!canvas) return;

      const ctx = canvas.getContext('2d');
      
      function resize() {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
      }
      resize();
      window.addEventListener('resize', resize);

      const columns = [];
      const fontSize = CONFIG.matrix.fontSize;

      function initColumns() {
        const columnCount = Math.floor(canvas.width / fontSize);
        columns.length = 0;
        for (let i = 0; i < columnCount; i++) {
          // Random starting positions
          columns.push({
            y: Math.random() * canvas.height,
            speed: 0.3 + Math.random() * 0.4,
            active: Math.random() < CONFIG.matrix.density
          });
        }
      }
      initColumns();
      window.addEventListener('resize', initColumns);

      function draw() {
        if (!effectsEnabled) {
          ctx.clearRect(0, 0, canvas.width, canvas.height);
          animationFrameId = requestAnimationFrame(draw);
          return;
        }

        // Fade effect
        ctx.fillStyle = 'rgba(10, 10, 10, 0.05)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        ctx.font = fontSize + 'px monospace';

        columns.forEach((col, i) => {
          if (!col.active) return;

          // Random binary character
          const char = Math.random() > 0.5 ? '0' : '1';
          
          // Rare white flicker
          if (Math.random() < CONFIG.matrix.flickerChance) {
            ctx.fillStyle = '#ffffff';
          } else {
            ctx.fillStyle = '#00ff00';
          }

          ctx.fillText(char, i * fontSize, col.y);

          // Reset or continue falling
          if (col.y > canvas.height && Math.random() > 0.99) {
            col.y = 0;
            col.active = Math.random() < CONFIG.matrix.density;
          } else {
            col.y += col.speed;
          }
        });

        animationFrameId = requestAnimationFrame(draw);
      }

      draw();
    }

    // ==========================================
    // Text Glitch Effect
    // ==========================================
    function initTextGlitch() {
      const glitchElements = document.querySelectorAll('[data-glitch="true"]');
      if (!glitchElements.length) return;

      function glitchText() {
        if (!effectsEnabled) {
          scheduleNextGlitch();
          return;
        }

        // Pick random element
        const el = glitchElements[Math.floor(Math.random() * glitchElements.length)];
        const text = el.textContent;
        const glitchCount = Math.floor(Math.random() * CONFIG.glitch.maxChars) + 1;

        // Find random positions to glitch
        const positions = [];
        for (let i = 0; i < glitchCount; i++) {
          const pos = Math.floor(Math.random() * text.length);
          if (text[pos] !== ' ' && Math.random() < CONFIG.glitch.chance * 100) {
            positions.push(pos);
          }
        }

        if (positions.length === 0) {
          scheduleNextGlitch();
          return;
        }

        // Apply glitch
        let glitchedText = text.split('');
        positions.forEach(pos => {
          const glitchChar = GLITCH_CHARS[Math.floor(Math.random() * GLITCH_CHARS.length)];
          glitchedText[pos] = glitchChar;
        });
        el.textContent = glitchedText.join('');

        // Restore after duration
        setTimeout(() => {
          el.textContent = text;
        }, CONFIG.glitch.duration);

        scheduleNextGlitch();
      }

      function scheduleNextGlitch() {
        const delay = CONFIG.glitch.interval[0] + 
          Math.random() * (CONFIG.glitch.interval[1] - CONFIG.glitch.interval[0]);
        setTimeout(glitchText, delay);
      }

      scheduleNextGlitch();
    }

    // ==========================================
    // Scan Line Effect
    // ==========================================
    function initScanLine() {
      const scanLine = document.getElementById('scan-line');
      if (!scanLine) return;

      function triggerScan() {
        if (!effectsEnabled) {
          scheduleNextScan();
          return;
        }

        scanLine.classList.add('active');
        setTimeout(() => {
          scanLine.classList.remove('active');
        }, 800);

        scheduleNextScan();
      }

      function scheduleNextScan() {
        const delay = CONFIG.scanLine.interval[0] + 
          Math.random() * (CONFIG.scanLine.interval[1] - CONFIG.scanLine.interval[0]);
        setTimeout(triggerScan, delay);
      }

      // Initial delay before first scan
      setTimeout(triggerScan, 10000);
    }

    // ==========================================
    // Chromatic Aberration on Scroll
    // ==========================================
    function initChromaticScroll() {
      let lastScrollY = window.scrollY;
      let scrollTimeout;

      window.addEventListener('scroll', () => {
        if (!effectsEnabled) return;

        clearTimeout(scrollTimeout);
        scrollTimeout = setTimeout(() => {
          const scrollDelta = Math.abs(window.scrollY - lastScrollY);
          
          if (scrollDelta > 50 && Math.random() < CONFIG.chromatic.scrollChance) {
            document.body.classList.add('chromatic-shift');
            setTimeout(() => {
              document.body.classList.remove('chromatic-shift');
            }, 150);
          }
          
          lastScrollY = window.scrollY;
        }, 50);
      });
    }

    // ==========================================
    // Effects Toggle
    // ==========================================
    function initToggle() {
      const toggle = document.getElementById('effects-toggle');
      if (!toggle) return;

      toggle.addEventListener('click', () => {
        effectsEnabled = !effectsEnabled;
        toggle.classList.toggle('disabled', !effectsEnabled);
        
        // Update ARIA
        toggle.setAttribute('aria-pressed', effectsEnabled);
        
        // Store preference
        try {
          localStorage.setItem('gunter-effects', effectsEnabled ? 'on' : 'off');
        } catch (e) {}
      });

      // Check stored preference
      try {
        const stored = localStorage.getItem('gunter-effects');
        if (stored === 'off') {
          effectsEnabled = false;
          toggle.classList.add('disabled');
        }
      } catch (e) {}
    }

    // ==========================================
    // Initialize All Effects
    // ==========================================
    function init() {
      // Check for reduced motion preference
      if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        effectsEnabled = false;
        const toggle = document.getElementById('effects-toggle');
        if (toggle) toggle.classList.add('disabled');
      }

      initToggle();
      initMatrixRain();
      initTextGlitch();
      initScanLine();
      initChromaticScroll();

      // \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
      // Console Easter Eggs for the curious
      // "The devtools are the first dungeon." - The Codex
      // \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
      console.log('%c\u{1F422} GUNTER PROTOCOL ACTIVE', 
        'color: #00ff00; font-size: 20px; font-weight: bold; text-shadow: 0 0 10px #00ff00;');
      console.log('%c"The basement remembers. Do you?"', 
        'color: #508050; font-style: italic; font-size: 12px;');
      console.log('');
      console.log('%c\u250C\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510', 'color: #00aa00;');
      console.log('%c\u2502  You found the first layer.                 \u2502', 'color: #00aa00;');
      console.log('%c\u2502                                             \u2502', 'color: #00aa00;');
      console.log('%c\u2502  Hint: The GitHub commits tell a story.    \u2502', 'color: #00aa00;');
      console.log('%c\u2502  Search for "3AM" in the commit history.   \u2502', 'color: #00aa00;');
      console.log('%c\u2502                                             \u2502', 'color: #00aa00;');
      console.log('%c\u2502  Next clue: What\\'s in /terminal?           \u2502', 'color: #00aa00;');
      console.log('%c\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518', 'color: #00aa00;');
      console.log('');
      console.log('%cAchievement Progress: SOURCE_DIVER [1/3]', 'color: #ffaa00;');
      console.log('%c"Read the comments, check the console, find the turtle."', 'color: #508050;');
      
      // Hidden: Type "gunter" in console to reveal more
      window.gunter = function() {
        console.log('%c\u{1F3AE} ACHIEVEMENT UNLOCKED: CONSOLE_COWBOY', 'color: #00ff00; font-size: 16px;');
        console.log('%c"You typed the magic word."', 'color: #508050;');
        console.log('');
        console.log('%cReal talk: This whole project was built with AI.', 'color: #00ff00;');
        console.log('%c18,000+ lines of UdonSharp. 365+ Claude conversations.', 'color: #00ff00;');
        console.log('%cThe story is at: basementos.com/story', 'color: #00ff00;');
        console.log('');
        console.log('%cNow go visit the actual basement:', 'color: #00aa00;');
        console.log('%chttps://vrchat.com/home/launch?worldId=wrld_7302897c-be0f-4037-ac67-76f0ea065c2b', 'color: #00ffff;');
        return '\u{1F422}';
      };
    }

    // Run on DOM ready
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', init);
    } else {
      init();
    }
  })();
<\/script>`], ["", `  <script>
  /**
   * \u256D\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u256E
   * \u2502  GUNTER PROTOCOL - Glitch Effects Engine                    \u2502
   * \u2502  v2.0.0.5 (The 2005 release, obviously)                     \u2502
   * \u2570\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u256F
   * 
   * Hidden Commands (for those who grep):
   * - Type "iddqd" anywhere: Nothing happens. This isn't DOOM.
   * - Type "konami": Also nothing. But we appreciate the attempt.
   * - Type "xyzzy": A hollow voice says "Plugh."
   * 
   * Secret: The \u{1F422} at the bottom isn't random.
   * "Slow and steady wins the race" - but also:
   * "Turtle" -> "Tuttle" -> Brazil (1985) -> bureaucratic dystopia
   * -> The absurdity of hunting eggs in a simulation within a game
   *     documented on a website you're reading right now.
   * 
   * Or it's just a cute turtle. You decide.
   * 
   * P.S. - The weather in Lower Level is real. Check Fond du Lac.
   *        When it rains there, it rains in the basement.
   */

  (function() {
    'use strict';

    // ==========================================
    // Configuration
    // ==========================================
    const CONFIG = {
      // Matrix rain settings
      matrix: {
        fontSize: 14,
        fallSpeed: 0.5,        // Very slow
        density: 0.03,         // Sparse columns
        flickerChance: 0.001   // Rare white flicker
      },
      // Text glitch settings
      glitch: {
        chance: 0.005,         // 0.5% chance per character
        interval: [8000, 15000], // Every 8-15 seconds
        duration: 150,         // ms
        maxChars: 2            // Max simultaneous glitched chars
      },
      // Scan line settings
      scanLine: {
        interval: [45000, 90000] // Every 45-90 seconds
      },
      // Chromatic aberration
      chromatic: {
        scrollChance: 0.05     // 1 in 20 scrolls
      }
    };

    // Glitch characters pool
    const GLITCH_CHARS = '\u0337\u0338\u0335\u0336\u0322\u0321\u0328\u031B\u0353\u0320\u0323\u0324\u0325\u0326\u0329\u032A\u032B\u032C\u032D\u032E\u032F\u0330\u0331\u0332\u0333\u0300\u0301\u0302\u0303\u0304\u0305\u0306\u0307\u0308\u0309\u030A\u030B\u030C\u030D\u030E\u030F\u0310\u0311\u0312\u0313\u0314\u0315\u031A\u0358\u035C\u0362\u035C\u035F\u0360\u0361\u0345@#$%^&*\u2588\u2593\u2592\u2591';

    // State
    let effectsEnabled = true;
    let animationFrameId = null;

    // ==========================================
    // Matrix Rain Effect
    // ==========================================
    function initMatrixRain() {
      const canvas = document.getElementById('matrix-rain');
      if (!canvas) return;

      const ctx = canvas.getContext('2d');
      
      function resize() {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
      }
      resize();
      window.addEventListener('resize', resize);

      const columns = [];
      const fontSize = CONFIG.matrix.fontSize;

      function initColumns() {
        const columnCount = Math.floor(canvas.width / fontSize);
        columns.length = 0;
        for (let i = 0; i < columnCount; i++) {
          // Random starting positions
          columns.push({
            y: Math.random() * canvas.height,
            speed: 0.3 + Math.random() * 0.4,
            active: Math.random() < CONFIG.matrix.density
          });
        }
      }
      initColumns();
      window.addEventListener('resize', initColumns);

      function draw() {
        if (!effectsEnabled) {
          ctx.clearRect(0, 0, canvas.width, canvas.height);
          animationFrameId = requestAnimationFrame(draw);
          return;
        }

        // Fade effect
        ctx.fillStyle = 'rgba(10, 10, 10, 0.05)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        ctx.font = fontSize + 'px monospace';

        columns.forEach((col, i) => {
          if (!col.active) return;

          // Random binary character
          const char = Math.random() > 0.5 ? '0' : '1';
          
          // Rare white flicker
          if (Math.random() < CONFIG.matrix.flickerChance) {
            ctx.fillStyle = '#ffffff';
          } else {
            ctx.fillStyle = '#00ff00';
          }

          ctx.fillText(char, i * fontSize, col.y);

          // Reset or continue falling
          if (col.y > canvas.height && Math.random() > 0.99) {
            col.y = 0;
            col.active = Math.random() < CONFIG.matrix.density;
          } else {
            col.y += col.speed;
          }
        });

        animationFrameId = requestAnimationFrame(draw);
      }

      draw();
    }

    // ==========================================
    // Text Glitch Effect
    // ==========================================
    function initTextGlitch() {
      const glitchElements = document.querySelectorAll('[data-glitch="true"]');
      if (!glitchElements.length) return;

      function glitchText() {
        if (!effectsEnabled) {
          scheduleNextGlitch();
          return;
        }

        // Pick random element
        const el = glitchElements[Math.floor(Math.random() * glitchElements.length)];
        const text = el.textContent;
        const glitchCount = Math.floor(Math.random() * CONFIG.glitch.maxChars) + 1;

        // Find random positions to glitch
        const positions = [];
        for (let i = 0; i < glitchCount; i++) {
          const pos = Math.floor(Math.random() * text.length);
          if (text[pos] !== ' ' && Math.random() < CONFIG.glitch.chance * 100) {
            positions.push(pos);
          }
        }

        if (positions.length === 0) {
          scheduleNextGlitch();
          return;
        }

        // Apply glitch
        let glitchedText = text.split('');
        positions.forEach(pos => {
          const glitchChar = GLITCH_CHARS[Math.floor(Math.random() * GLITCH_CHARS.length)];
          glitchedText[pos] = glitchChar;
        });
        el.textContent = glitchedText.join('');

        // Restore after duration
        setTimeout(() => {
          el.textContent = text;
        }, CONFIG.glitch.duration);

        scheduleNextGlitch();
      }

      function scheduleNextGlitch() {
        const delay = CONFIG.glitch.interval[0] + 
          Math.random() * (CONFIG.glitch.interval[1] - CONFIG.glitch.interval[0]);
        setTimeout(glitchText, delay);
      }

      scheduleNextGlitch();
    }

    // ==========================================
    // Scan Line Effect
    // ==========================================
    function initScanLine() {
      const scanLine = document.getElementById('scan-line');
      if (!scanLine) return;

      function triggerScan() {
        if (!effectsEnabled) {
          scheduleNextScan();
          return;
        }

        scanLine.classList.add('active');
        setTimeout(() => {
          scanLine.classList.remove('active');
        }, 800);

        scheduleNextScan();
      }

      function scheduleNextScan() {
        const delay = CONFIG.scanLine.interval[0] + 
          Math.random() * (CONFIG.scanLine.interval[1] - CONFIG.scanLine.interval[0]);
        setTimeout(triggerScan, delay);
      }

      // Initial delay before first scan
      setTimeout(triggerScan, 10000);
    }

    // ==========================================
    // Chromatic Aberration on Scroll
    // ==========================================
    function initChromaticScroll() {
      let lastScrollY = window.scrollY;
      let scrollTimeout;

      window.addEventListener('scroll', () => {
        if (!effectsEnabled) return;

        clearTimeout(scrollTimeout);
        scrollTimeout = setTimeout(() => {
          const scrollDelta = Math.abs(window.scrollY - lastScrollY);
          
          if (scrollDelta > 50 && Math.random() < CONFIG.chromatic.scrollChance) {
            document.body.classList.add('chromatic-shift');
            setTimeout(() => {
              document.body.classList.remove('chromatic-shift');
            }, 150);
          }
          
          lastScrollY = window.scrollY;
        }, 50);
      });
    }

    // ==========================================
    // Effects Toggle
    // ==========================================
    function initToggle() {
      const toggle = document.getElementById('effects-toggle');
      if (!toggle) return;

      toggle.addEventListener('click', () => {
        effectsEnabled = !effectsEnabled;
        toggle.classList.toggle('disabled', !effectsEnabled);
        
        // Update ARIA
        toggle.setAttribute('aria-pressed', effectsEnabled);
        
        // Store preference
        try {
          localStorage.setItem('gunter-effects', effectsEnabled ? 'on' : 'off');
        } catch (e) {}
      });

      // Check stored preference
      try {
        const stored = localStorage.getItem('gunter-effects');
        if (stored === 'off') {
          effectsEnabled = false;
          toggle.classList.add('disabled');
        }
      } catch (e) {}
    }

    // ==========================================
    // Initialize All Effects
    // ==========================================
    function init() {
      // Check for reduced motion preference
      if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        effectsEnabled = false;
        const toggle = document.getElementById('effects-toggle');
        if (toggle) toggle.classList.add('disabled');
      }

      initToggle();
      initMatrixRain();
      initTextGlitch();
      initScanLine();
      initChromaticScroll();

      // \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
      // Console Easter Eggs for the curious
      // "The devtools are the first dungeon." - The Codex
      // \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
      console.log('%c\u{1F422} GUNTER PROTOCOL ACTIVE', 
        'color: #00ff00; font-size: 20px; font-weight: bold; text-shadow: 0 0 10px #00ff00;');
      console.log('%c"The basement remembers. Do you?"', 
        'color: #508050; font-style: italic; font-size: 12px;');
      console.log('');
      console.log('%c\u250C\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510', 'color: #00aa00;');
      console.log('%c\u2502  You found the first layer.                 \u2502', 'color: #00aa00;');
      console.log('%c\u2502                                             \u2502', 'color: #00aa00;');
      console.log('%c\u2502  Hint: The GitHub commits tell a story.    \u2502', 'color: #00aa00;');
      console.log('%c\u2502  Search for "3AM" in the commit history.   \u2502', 'color: #00aa00;');
      console.log('%c\u2502                                             \u2502', 'color: #00aa00;');
      console.log('%c\u2502  Next clue: What\\\\'s in /terminal?           \u2502', 'color: #00aa00;');
      console.log('%c\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518', 'color: #00aa00;');
      console.log('');
      console.log('%cAchievement Progress: SOURCE_DIVER [1/3]', 'color: #ffaa00;');
      console.log('%c"Read the comments, check the console, find the turtle."', 'color: #508050;');
      
      // Hidden: Type "gunter" in console to reveal more
      window.gunter = function() {
        console.log('%c\u{1F3AE} ACHIEVEMENT UNLOCKED: CONSOLE_COWBOY', 'color: #00ff00; font-size: 16px;');
        console.log('%c"You typed the magic word."', 'color: #508050;');
        console.log('');
        console.log('%cReal talk: This whole project was built with AI.', 'color: #00ff00;');
        console.log('%c18,000+ lines of UdonSharp. 365+ Claude conversations.', 'color: #00ff00;');
        console.log('%cThe story is at: basementos.com/story', 'color: #00ff00;');
        console.log('');
        console.log('%cNow go visit the actual basement:', 'color: #00aa00;');
        console.log('%chttps://vrchat.com/home/launch?worldId=wrld_7302897c-be0f-4037-ac67-76f0ea065c2b', 'color: #00ffff;');
        return '\u{1F422}';
      };
    }

    // Run on DOM ready
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', init);
    } else {
      init();
    }
  })();
<\/script>`])), renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": "???", "data-astro-cid-purez4in": true }, { "default": ($$result2) => renderTemplate` ${maybeRenderHead()}<div class="gunter-page" id="gunter-page" data-astro-cid-purez4in> <!-- Matrix Binary Rain Background --> <canvas id="matrix-rain" class="matrix-canvas" data-astro-cid-purez4in></canvas> <!-- Accessibility Toggle --> <button class="effects-toggle" id="effects-toggle" title="Toggle glitch effects" data-astro-cid-purez4in> <span class="toggle-icon" data-astro-cid-purez4in>◉</span> <span class="toggle-text" data-astro-cid-purez4in>EFFECTS</span> </button> <!-- Main Content --> <div class="gunter-content" data-astro-cid-purez4in> <!-- Hero Section --> <header class="gunter-hero" data-astro-cid-purez4in> <div class="glitch-container" data-astro-cid-purez4in> <h1 class="glitch-text" data-text="GUNTER PROTOCOL DETECTED" data-astro-cid-purez4in>GUNTER PROTOCOL DETECTED</h1> </div> <p class="hero-subtitle" data-astro-cid-purez4in>Egg Hunters of the Forgotten Millennium</p> <div class="scanline" data-astro-cid-purez4in></div> </header> <!-- Main Narrative --> <main class="gunter-narrative" data-astro-cid-purez4in> <section class="codex-section" data-astro-cid-purez4in> <p class="narrative-text" data-glitch="true" data-astro-cid-purez4in>
Hello.
</p> <p class="narrative-text" data-glitch="true" data-astro-cid-purez4in>
If you're seeing this, I'm probably still alive—just busy debugging something at 3AM. But I wanted to leave this message for those who find it. The curious ones. The ones who <code data-astro-cid-purez4in>view-source</code>. The ones who remember.
</p> <p class="narrative-text" data-glitch="true" data-astro-cid-purez4in>
I grew up in a basement in Wisconsin. Shag carpet. Wood paneling. A CRT TV that took 30 seconds to warm up. We played Halo 2 on system link. We burned mix CDs with Sharpie labels. We left away messages on AIM that were basically poetry. <span class="highlight" data-astro-cid-purez4in>It was the golden age</span>, and we didn't even know it.
</p> <p class="narrative-text" data-glitch="true" data-astro-cid-purez4in>
So I rebuilt it. Every detail I could remember. And then I hid things inside it. <span class="highlight" data-astro-cid-purez4in>Nineteen achievements</span>. Four hundred and twenty Gamerscore. But that's just what's on the surface.
</p> <p class="narrative-text" data-glitch="true" data-astro-cid-purez4in> <span class="highlight" data-astro-cid-purez4in>How do you earn the other 580?</span> </p> <p class="narrative-text" data-glitch="true" data-astro-cid-purez4in>
This website documents how I built it—with AI, with obsession, with far too many late nights. The devlog entries are breadcrumbs. The GitHub commits are clues. The terminal commands respond to things you might remember from 2005. <span class="highlight" data-astro-cid-purez4in>Study them.</span> </p> <p class="narrative-text" data-glitch="true" data-astro-cid-purez4in>
The first player to reach <span class="highlight" data-astro-cid-purez4in>1000G</span> wins something real. I'm not saying what. But it exists. And I'm watching.
</p> <p class="narrative-text" data-glitch="true" data-astro-cid-purez4in>
I'm not offering fortune or fame. Just the satisfaction of finding something hidden. The joy of remembering an era that shaped us. And maybe—if you're very lucky—a turtle.
</p> <p class="narrative-text final-line" data-glitch="true" data-astro-cid-purez4in>
The basement is waiting. Come find what I've hidden.
</p> </section> <!-- Separator --> <div class="terminal-separator" data-astro-cid-purez4in> <span data-astro-cid-purez4in>═══════════════════ PROTOCOL INITIALIZED ═══════════════════</span> </div> <!-- Call to Action --> <section class="cta-section" data-astro-cid-purez4in> <p class="cta-text" data-astro-cid-purez4in>The hunt begins now.</p> <a href="/codex" class="cta-button" data-astro-cid-purez4in> <span class="button-text" data-astro-cid-purez4in>READ THE CODEX</span> <span class="button-cursor" data-astro-cid-purez4in>_</span> </a> </section> <!-- Footer --> <footer class="gunter-footer" data-astro-cid-purez4in> <p class="footer-text" data-astro-cid-purez4in>The basement remembers. Do you?</p> <p class="footer-emoji" data-astro-cid-purez4in>🐢</p> </footer> </main> </div> <!-- Scan Line Overlay --> <div class="scan-line-overlay" id="scan-line" data-astro-cid-purez4in></div> </div> ` }));
}, "C:/Users/cloud/basement-os-web/src/pages/gunter.astro", void 0);

const $$file = "C:/Users/cloud/basement-os-web/src/pages/gunter.astro";
const $$url = "/gunter";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$Gunter,
  file: $$file,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
