/**
 * BASEMENT OS WEB EMULATOR
 * "Compiles" raw C# Unity scripts into a web runtime.
 */
class BasementOS {
    constructor(containerId) {
        this.container = document.getElementById(containerId);
        this.screen = this.container.querySelector('.terminal-screen');

        // System State
        this.booted = false;
        this.currentApp = null; // 'SHELL' or 'GITHUB'
        this.cursorBlink = true;

        // Shell State (Menus)
        this.menuItems = [];
        this.menuCursor = 0;

        // Content State (GitHub App)
        this.contentTabs = []; // README, CHANGELOG...
        this.currentTab = 0;
        this.scrollOffset = 0;
        this.contentLines = []; // Active file lines

        // Config
        this.repoBaseUrl = '/os-data'; // Where the CS files live

        this.init();
    }
    async init() {
        this.renderBootSequence();

        try {
            // "Compile" (Fetch & Parse)
            await this.loadShellData();
            await this.loadGitHubData();

            setTimeout(() => {
                this.booted = true;
                this.currentApp = 'SHELL';
                this.attachInput();
                this.render();
            }, 1500); // Fake boot delay

        } catch (e) {
            this.screen.innerText = `\n  FATAL ERROR: KERNEL PANIC\n  Failed to mount virtual volume.\n  ${e.message}`;
            console.error(e);
        }
    }
    // --- PARSERS (The "Compiler" Magic) ---
    async loadShellData() {
        // Fetches DT_Shell.cs and extracts menu config
        const text = await this.fetchSource('DT_Shell.cs');

        // Regex to find string arrays: public string[] menuNames = { ... };
        this.menuItems = this.extractArray(text, 'menuNames');
        this.menuTypes = this.extractArray(text, 'menuTypes');
        this.menuDesc = this.extractArray(text, 'menuDescriptions');

        // If parsing fails, fallback
        if (!this.menuItems.length) {
            this.menuItems = ["MANUAL MODE", "RETRY CONNECT"];
            this.menuTypes = ["<ERR>", "<ERR>"];
            this.menuDesc = ["Failed to parse C# source.", "Check network tab."];
        }
    }
    async loadGitHubData() {
        // Fetches DT_App_GitHub.cs for content
        const text = await this.fetchSource('DT_App_GitHub.cs');

        // Parse tabs and fallback content
        this.contentTabs = this.extractArray(text, 'tabNames');

        // Extract long string fields
        this.readme = this.extractField(text, 'fallbackReadme');
        this.changelog = this.extractField(text, 'fallbackChangelog');
        this.issues = this.extractField(text, 'fallbackIssues');
        this.thanks = this.extractField(text, 'fallbackThanks');
    }
    // Helper: Extracts string[] variable = { "A", "B" }
    extractArray(source, varName) {
        // Looks for: string[] varName = new string[] { "..." };
        // Simplified regex for the specific format in DT_Shell.cs
        // Note: This matches the loose format of the provided C# files
        const regex = new RegExp(`${varName}\\s*=\\s*(?:new string\\[\\])?\\s*\\{([^}]*)\\}`, 's');
        const match = source.match(regex);

        if (match && match[1]) {
            // Split by comma, strip quotes and whitespace
            return match[1].split(',')
                .map(s => s.trim().replace(/^"|"$/g, '').replace(/\\"/g, '"')) // Remove defining quotes
                .filter(s => s.length > 0);
        }
        return [];
    }
    // Helper: Extracts [SerializeField] private string varName = "...";
    extractField(source, varName) {
        // Matches: string varName = "CONTENT";
        // Handles multiline strings if C# uses @"" or \n
        // This is a naive parser for the "KISS" demo
        const regex = new RegExp(`${varName}\\s*=\\s*"([^;]*)";`, 's');
        // Handle verbatim strings @"..." if used, or standard strings
        // For the specific file structure provided, we look for the assignment

        // A more robust search for the example file:
        const simpleMatch = source.match(new RegExp(`${varName}\\s*=\\s*"((?:[^"\\\\]|\\\\.)*)"`));
        if (simpleMatch) return this.parseCSharpString(simpleMatch[1]);

        return "DATA CORRUPTED";
    }

    parseCSharpString(str) {
        return str.replace(/\\n/g, '\n').replace(/\\t/g, '\t');
    }
    async fetchSource(filename) {
        const response = await fetch(`${this.repoBaseUrl}/${filename}?v=${Date.now()}`);
        if (!response.ok) {
            // Try flat structure if subdir fails (common deployment issue)
            const flatResponse = await fetch(`${this.repoBaseUrl}/${filename}`);
            if (flatResponse.ok) return await flatResponse.text();
            throw new Error(`404: ${filename}`);
        }
        return await response.text();
    }
    // --- CORE KERNEL (Rendering) ---
    render() {
        if (!this.booted) return;
        let buffer = "";

        // 1. Header & Ticker
        buffer += ` BASEMENT OS v2.1 | MEM: 640K OK\n`;
        buffer += `════════════════════════════════════════════════════════════════════════════════\n`;
        buffer += ` *** REMOTE CONNECTION ESTABLISHED *** \n`;
        buffer += `════════════════════════════════════════════════════════════════════════════════\n`;
        // 2. App Content
        if (this.currentApp === 'SHELL') {
            buffer += this.renderShell();
        } else if (this.currentApp === 'GITHUB') {
            buffer += this.renderGitHub();
        }
        // 3. Footer
        buffer += `\n════════════════════════════════════════════════════════════════════════════════\n`;
        buffer += this.currentApp === 'SHELL'
            ? `C:\\BASEMENT> ${this.cursorBlink ? '_' : ' '}`
            : `C:\\BASEMENT\\APPS\\GITHUB.EXE [ESC: BACK]`;
        this.screen.innerText = buffer;
    }
    renderShell() {
        let out = " C:\\BASEMENT\\MENU\n\n";
        out += "   TYPE     NAME             DESCRIPTION\n";
        out += "   ----     ------------     ---------------------------------------\n";
        this.menuItems.forEach((name, i) => {
            const isSel = i === this.menuCursor;
            const marker = isSel ? ">" : " ";
            const type = (this.menuTypes[i] || "FILE").padEnd(8);
            const namePad = name.padEnd(16);
            const desc = this.menuDesc[i] || "";

            // Note: HTML/CSS handles the color, here we just do text layout
            // For true retro feel we'd inject spans, but innerText is safer.
            // visual marker is enough for now.
            out += ` ${marker} ${type} ${namePad} ${desc}\n`;
        });

        // Pads to fill screen
        const linesUsed = this.menuItems.length + 5;
        for (let k = linesUsed; k < 18; k++) out += "\n";

        return out;
    }
    renderGitHub() {
        let out = " ";
        // Tabs
        this.contentTabs.forEach((tab, i) => {
            if (i === this.currentTab) out += `>[${tab}]<  `;
            else out += ` [${tab}]   `;
        });
        out += "\n────────────────────────────────────────────────────────────────────────────────\n";
        // Current Content
        let content = "";
        switch (this.currentTab) {
            case 0: content = this.readme; break;
            case 1: content = this.changelog; break;
            case 2: content = this.issues; break;
            case 3: content = this.thanks; break;
        }
        // Word wrap/Chunk logic (Simplified)
        const lines = content.split('\n');
        const viewHeight = 15;

        const start = this.scrollOffset;
        const visibleParams = lines.slice(start, start + viewHeight);

        visibleParams.forEach(l => out += l + "\n");
        // Fill empty
        for (let k = visibleParams.length; k < viewHeight; k++) out += "\n";
        out += "────────────────────────────────────────────────────────────────────────────────\n";
        out += `              Line ${start}-${start + visibleParams.length} of ${lines.length} | UP/DOWN Scroll`;

        return out;
    }
    renderBootSequence() {
        let step = 0;
        const steps = [
            "POST... OK",
            "BIOS v4.5... LOADED",
            "Mouting Drive A: ...",
            "Reading FATTable...",
            "Loading COMMAND.COM..."
        ];

        const bootParams = setInterval(() => {
            this.screen.innerText += `\n${steps[step]}`;
            step++;
            if (step >= steps.length) clearInterval(bootParams);
        }, 200);
    }
    // --- INPUT ---
    attachInput() {
        // Prevent multiple listeners
        if (this._inputAttached) return;
        this._inputAttached = true;

        document.addEventListener('keydown', (e) => {
            if (!this.booted) return;
            // Route Input
            if (this.currentApp === 'SHELL') this.handleShellInput(e);
            else if (this.currentApp === 'GITHUB') this.handleGitHubInput(e);

            this.render();
        });
        // Blinking Cursor Timer
        setInterval(() => {
            this.cursorBlink = !this.cursorBlink;
            this.render(); // Re-render for blinking cursor
        }, 500);
    }
    handleShellInput(e) {
        if (e.key === 'ArrowUp') {
            this.menuCursor--;
            if (this.menuCursor < 0) this.menuCursor = this.menuItems.length - 1;
            e.preventDefault();
        } else if (e.key === 'ArrowDown') {
            this.menuCursor++;
            if (this.menuCursor >= this.menuItems.length) this.menuCursor = 0;
            e.preventDefault();
        } else if (e.key === 'Enter') {
            // "Launch" App
            const target = this.menuItems[this.menuCursor];
            if (target.includes("GITHUB")) {
                this.currentApp = 'GITHUB';
            }
        }
    }
    handleGitHubInput(e) {
        if (e.key === 'ArrowLeft') {
            this.currentTab--;
            if (this.currentTab < 0) this.currentTab = this.contentTabs.length - 1;
            this.scrollOffset = 0;
        } else if (e.key === 'ArrowRight') {
            this.currentTab++;
            if (this.currentTab >= this.contentTabs.length) this.currentTab = 0;
            this.scrollOffset = 0;
        } else if (e.key === 'ArrowUp') {
            if (this.scrollOffset > 0) this.scrollOffset--;
            e.preventDefault();
        } else if (e.key === 'ArrowDown') {
            this.scrollOffset++;
            e.preventDefault();
        } else if (e.key === 'Escape') {
            this.currentApp = 'SHELL';
        }
    }
}
