/**
 * BASEMENT OS WEB EMULATOR v2.1
 * 
 * Emulates the Unity DT_Core system in the browser.
 * Parses C# source files and renders the terminal exactly like the VRChat version.
 * 
 * Architecture mirrors:
 * - DT_Core.cs: Main kernel, screen buffer, input routing
 * - DT_Shell.cs: Main menu
 * - DT_Ticker.cs: RSS scrolling ticker
 * - DT_Format.cs: Box drawing utilities
 * - DT_Theme.cs: Color palette
 * - BIN/*: Individual applications
 */
class BasementOS {
    constructor(containerId) {
        this.container = document.getElementById(containerId);
        this.screen = this.container.querySelector('.terminal-screen');

        // === CORE STATE (from DT_Core.cs) ===
        this.booted = false;
        this.currentApp = null;
        this.isCursorVisible = true;

        // Screen Buffer (80x24 grid)
        this.SCREEN_WIDTH = 80;
        this.headerLine = "";
        this.rssLine = "";
        this.contentBuffer = "";
        this.footerLine = "";
        this.promptLine = "";

        // === TICKER STATE (from DT_Ticker.cs) ===
        this.tickerMessages = [];
        this.tickerFullText = "";
        this.tickerOffset = 0;
        this.tickerSpeed = 4.0; // chars per second

        // === SHELL STATE (from DT_Shell.cs) ===
        this.menuNames = [];
        this.menuTypes = [];
        this.menuDescriptions = [];
        this.menuCursor = 0;

        // === GITHUB APP STATE (from DT_App_GitHub.cs) ===
        this.githubTabs = [];
        this.githubCurrentTab = 0;
        this.githubScrollOffset = 0;
        this.githubContent = { readme: "", changelog: "", issues: "", thanks: "" };

        // === DASHBOARD APP STATE (from DT_App_Dashboard.cs) ===
        this.dashboardData = {
            playerName: "WebVisitor",
            playerRank: "Guest",
            playerScore: 0,
            playerVisits: 1,
            memoryStatus: "[OK]",
            weatherStatus: "[OK]",
            mailStatus: "[!!]",
            weatherTemp: "74°F",
            weatherCondition: "Clear",
            weatherLocation: "Chicago, IL",
            onlineCount: 1,
            onlinePlayers: [" > WebVisitor"]
        };

        // === GAMES APP STATE (from DT_App_Games.cs) ===
        this.gameNames = [];
        this.gameDescriptions = [];
        this.gameEnabled = [];
        this.gameCursor = 0;

        // === STATS APP STATE (from DT_App_Stats.cs) ===
        this.statsData = {
            achievements: [
                { name: "First Steps", progress: 1.0, complete: true },
                { name: "Regular Visitor", progress: 0.4, complete: false },
                { name: "Time Lord", progress: 0.1, complete: false },
                { name: "Explorer", progress: 0.5, complete: false },
                { name: "Social Butterfly", progress: 0.25, complete: false }
            ],
            gamerScore: 420,
            maxGamerScore: 1000,
            totalVisits: 42,
            timePlayed: "12h 30m"
        };

        // === TALES APP STATE (from DT_App_Tales.cs) ===
        this.talesStory = [];
        this.talesChoicesA = [];
        this.talesChoicesB = [];
        this.talesChoicesC = [];
        this.talesChoicesD = [];
        this.talesTargetsA = [];
        this.talesTargetsB = [];
        this.talesTargetsC = [];
        this.talesTargetsD = [];
        this.talesChoiceCounts = [];
        this.talesCurrentNode = 0;
        this.talesSelectedChoice = 0;

        // === WEATHER APP STATE (from DT_App_Weather.cs) ===
        this.weatherData = {
            location: "Chicago, IL",
            temperature: "74°F",
            condition: "Clear",
            online: true
        };

        // Config
        this.repoBaseUrl = '/os-data';

        this.init();
    }

    // =======================================================================
    // INITIALIZATION (mirrors DT_Core.Start())
    // =======================================================================

    async init() {
        this.renderBootSequence();

        try {
            // Load all data sources
            await this.loadTickerData();
            await this.loadShellData();
            await this.loadGitHubData();
            await this.loadGamesData();
            await this.loadTalesData();

            // Boot complete
            setTimeout(() => {
                this.booted = true;
                this.currentApp = 'SHELL';

                // Initialize screen buffer (from DT_Core.BootSequence)
                this.headerLine = "BASEMENT OS v2.1 | MEM: 640K OK";
                this.footerLine = "BIOS Check... OK. Initializing Core...";
                this.promptLine = "C:\\BASEMENT>";

                this.attachInput();
                this.startTicker();
                this.startCursorBlink();
                this.render();
            }, 1500);

        } catch (e) {
            this.screen.innerText = `\n  FATAL ERROR: KERNEL PANIC\n  Failed to mount virtual volume.\n  ${e.message}`;
            console.error(e);
        }
    }

    // =======================================================================
    // C# FILE PARSERS
    // =======================================================================

    async fetchSource(filename) {
        const response = await fetch(`${this.repoBaseUrl}/${filename}?v=${Date.now()}`);
        if (!response.ok) throw new Error(`404: ${filename}`);
        return await response.text();
    }

    extractArray(source, varName) {
        // Matches: varName = new type[] { ... } or varName = { ... }
        const regex = new RegExp(`${varName}\\s*=\\s*(?:new [a-zA-Z0-9\\[\\]<>]*\\s*)?\\{([^}]*)\\}`, 's');
        const match = source.match(regex);
        if (match && match[1]) {
            // Parse respecting quoted strings
            const result = [];
            let inQuote = false;
            let buffer = "";
            for (let i = 0; i < match[1].length; i++) {
                const char = match[1][i];
                if (char === '"' && match[1][i - 1] !== '\\') inQuote = !inQuote;
                if (char === ',' && !inQuote) {
                    result.push(buffer.trim());
                    buffer = "";
                } else {
                    buffer += char;
                }
            }
            if (buffer.trim()) result.push(buffer.trim());

            return result.map(s => s
                .trim()
                .replace(/^"|"$/g, '')
                .replace(/\\"/g, '"')
                .replace(/\\n/g, '\n')
                .replace(/\\t/g, '\t')
            ).filter(s => s.length > 0);
        }
        return [];
    }

    extractField(source, varName) {
        // Simple string: varName = "value";
        const regex = new RegExp(`${varName}\\s*=\\s*"((?:[^"\\\\]|\\\\.)*)"`);
        const match = source.match(regex);
        if (match) return match[1].replace(/\\n/g, '\n').replace(/\\t/g, '\t');
        return "";
    }

    // =======================================================================
    // DATA LOADERS
    // =======================================================================

    async loadTickerData() {
        const text = await this.fetchSource('DT_Ticker.cs');
        this.tickerMessages = this.extractArray(text, 'feedMessages');
        if (this.tickerMessages.length === 0) {
            this.tickerMessages = ["WELCOME TO BASEMENT OS", "SYSTEM STATUS: NOMINAL"];
        }
        this.buildTickerText();
    }

    async loadShellData() {
        const text = await this.fetchSource('DT_Shell.cs');
        this.menuNames = this.extractArray(text, 'menuNames');
        this.menuTypes = this.extractArray(text, 'menuTypes');
        this.menuDescriptions = this.extractArray(text, 'menuDescriptions');
    }

    async loadGitHubData() {
        const text = await this.fetchSource('DT_App_GitHub.cs');
        this.githubTabs = this.extractArray(text, 'tabNames');
        this.githubContent.readme = this.extractField(text, 'fallbackReadme');
        this.githubContent.changelog = this.extractField(text, 'fallbackChangelog');
        this.githubContent.issues = this.extractField(text, 'fallbackIssues');
        this.githubContent.thanks = this.extractField(text, 'fallbackThanks');
    }

    async loadGamesData() {
        const text = await this.fetchSource('DT_App_Games.cs');
        this.gameNames = this.extractArray(text, 'gameNames');
        this.gameDescriptions = this.extractArray(text, 'gameDescriptions');
        const enabled = this.extractArray(text, 'gameEnabled');
        this.gameEnabled = enabled.map(e => e.toLowerCase() === 'true');
    }

    async loadTalesData() {
        const text = await this.fetchSource('DT_App_Tales.cs');
        this.talesStory = this.extractArray(text, 'storyTexts');
        this.talesChoicesA = this.extractArray(text, 'choiceATexts');
        this.talesChoicesB = this.extractArray(text, 'choiceBTexts');
        this.talesChoicesC = this.extractArray(text, 'choiceCTexts');
        this.talesChoicesD = this.extractArray(text, 'choiceDTexts');
        this.talesTargetsA = this.extractArray(text, 'choiceATargets').map(x => parseInt(x));
        this.talesTargetsB = this.extractArray(text, 'choiceBTargets').map(x => parseInt(x));
        this.talesTargetsC = this.extractArray(text, 'choiceCTargets').map(x => parseInt(x));
        this.talesTargetsD = this.extractArray(text, 'choiceDTargets').map(x => parseInt(x));
        this.talesChoiceCounts = this.extractArray(text, 'choiceCounts').map(x => parseInt(x));
    }

    // =======================================================================
    // TICKER SYSTEM (from DT_Ticker.cs)
    // =======================================================================

    buildTickerText() {
        this.tickerFullText = this.tickerMessages.join(" *** ") + " *** ";
    }

    startTicker() {
        setInterval(() => {
            this.tickerOffset += this.tickerSpeed * 0.05;
            if (this.tickerOffset >= this.tickerFullText.length) {
                this.tickerOffset = 0;
            }
            this.updateRssLine();
            this.render();
        }, 50);
    }

    updateRssLine() {
        if (!this.tickerFullText) {
            this.rssLine = " ".repeat(this.SCREEN_WIDTH);
            return;
        }
        const start = Math.floor(this.tickerOffset) % this.tickerFullText.length;
        let result = "";
        for (let i = 0; i < this.SCREEN_WIDTH; i++) {
            const pos = (start + i) % this.tickerFullText.length;
            result += this.tickerFullText[pos];
        }
        this.rssLine = result;
    }

    // =======================================================================
    // SCREEN RENDERING (from DT_Core.RefreshDisplay)
    // =======================================================================

    render() {
        if (!this.booted) return;

        // Get content from active app
        this.contentBuffer = this.getActiveAppContent();

        // Update prompt based on current app
        if (this.currentApp === 'SHELL') {
            this.promptLine = "C:\\BASEMENT>";
        } else {
            this.promptLine = `C:\\BASEMENT\\APP>`;
        }

        // Assemble screen buffer (matches DT_Core.RefreshDisplay exactly)
        const separator = '═'.repeat(this.SCREEN_WIDTH);

        let buffer = "";
        buffer += this.headerLine + "\n";
        buffer += separator + "\n";
        buffer += this.rssLine + "\n";
        buffer += separator + "\n";
        buffer += this.contentBuffer + "\n";
        buffer += separator + "\n";
        buffer += this.footerLine + "\n";
        buffer += this.promptLine + (this.isCursorVisible ? "_" : " ");

        this.screen.innerText = buffer;
    }

    getActiveAppContent() {
        switch (this.currentApp) {
            case 'SHELL': return this.renderShell();
            case 'GITHUB': return this.renderGitHub();
            case 'DASHBOARD': return this.renderDashboard();
            case 'GAMES': return this.renderGames();
            case 'STATS': return this.renderStats();
            case 'TALES': return this.renderTales();
            case 'WEATHER': return this.renderWeather();
            default: return "NO APP LOADED";
        }
    }

    // =======================================================================
    // APP RENDERERS (port GetDisplayContent() from each app)
    // =======================================================================

    // --- DT_Shell.GetDisplayContent() ---
    renderShell() {
        let out = " C:\\BASEMENT\\MENU\n\n";
        out += "   TYPE     NAME             DESCRIPTION\n";
        out += "   ----     ------------     ---------------------------------------\n";

        for (let i = 0; i < this.menuNames.length; i++) {
            const isSel = i === this.menuCursor;
            const marker = isSel ? ">" : " ";
            const type = (this.menuTypes[i] || "<DIR>").padEnd(8);
            const name = (this.menuNames[i] || "UNKNOWN").padEnd(16);
            const desc = this.menuDescriptions[i] || "";
            out += ` ${marker} ${type} ${name} ${desc}\n`;
        }

        // Pad to fill content area
        const lines = out.split('\n').length;
        for (let i = lines; i < 16; i++) out += "\n";

        return out;
    }

    // --- DT_App_GitHub.GetDisplayContent() ---
    renderGitHub() {
        let out = " ";
        for (let i = 0; i < this.githubTabs.length; i++) {
            if (i === this.githubCurrentTab) out += `>[${this.githubTabs[i]}]<  `;
            else out += `[${this.githubTabs[i]}]  `;
        }
        out += "\n────────────────────────────────────────────────────────────────────────────────\n";

        let content = "";
        switch (this.githubCurrentTab) {
            case 0: content = this.githubContent.readme; break;
            case 1: content = this.githubContent.changelog; break;
            case 2: content = this.githubContent.issues; break;
            case 3: content = this.githubContent.thanks; break;
        }

        const lines = content.split('\n');
        const visibleLines = 12;
        const maxScroll = Math.max(0, lines.length - visibleLines);
        if (this.githubScrollOffset > maxScroll) this.githubScrollOffset = maxScroll;

        const visible = lines.slice(this.githubScrollOffset, this.githubScrollOffset + visibleLines);
        visible.forEach(l => out += l + "\n");

        // Line indicator
        out += "\n────────────────────────────────────────────────────────────────────────────────\n";
        out += `              Lines ${this.githubScrollOffset + 1}-${this.githubScrollOffset + visible.length} of ${lines.length} | UP/DOWN Scroll`;

        return out;
    }

    // --- DT_App_Dashboard.GetDisplayContent() ---
    renderDashboard() {
        const d = this.dashboardData;
        const leftW = 38;

        const left = [
            "SYSTEM STATUS",
            `${d.memoryStatus} Memory Check`,
            `${d.weatherStatus} Weather Sensors`,
            `${d.mailStatus} Mail Server (Offline)`,
            "",
            "PLAYER STATS",
            `Rank:      ${d.playerRank}`,
            `Score:     ${d.playerScore}G / 420G`,
            `Visits:    ${d.playerVisits}`,
            "",
            "[>] SYSTEM MENU (Open)",
            "    Press [ACCEPT] to continue",
            "",
            "    [Quick Launch: Snake]",
            "    [Quick Launch: Arcade]",
            ""
        ];

        const right = [
            `WHO IS ONLINE (${d.onlineCount})`,
            ...d.onlinePlayers.slice(0, 4),
            "", "",
            `WEATHER (${d.weatherLocation})`,
            `${d.weatherTemp}  ${d.weatherCondition}`,
            "", "", "", "", "", "", "", ""
        ];

        let out = "";
        for (let i = 0; i < 16; i++) {
            const l = (left[i] || "").padEnd(leftW);
            const r = right[i] || "";
            out += `${l}    ${r}\n`;
        }
        return out;
    }

    // --- DT_App_Games.GetDisplayContent() ---
    renderGames() {
        if (this.gameNames.length === 0) {
            return "\n\n   ERROR: No game cartridges found.\n   Configure gameObjects in Inspector.";
        }

        let out = " SELECT CARTRIDGE\n";
        out += "═".repeat(this.SCREEN_WIDTH) + "\n\n";

        for (let i = 0; i < this.gameNames.length; i++) {
            const isSel = i === this.gameCursor;
            const cursor = isSel ? " > " : "   ";
            const idx = `[${i + 1}]`;
            const name = this.gameNames[i] || "UNKNOWN";
            out += `${cursor}${idx}  ${name}\n`;

            const desc = this.gameDescriptions[i] || "No description.";
            out += `         ${desc}\n`;

            const enabled = this.gameEnabled[i] !== false;
            if (!enabled) {
                out += "         Status: Coming Soon\n";
            } else {
                out += "         Status: Ready\n";
            }
            out += "\n";
        }

        return out;
    }

    // --- DT_App_Stats.GetDisplayContent() ---
    renderStats() {
        const s = this.statsData;
        let out = "";

        out += " ╔══════════════════════════════════════════════════════════════════════════════╗\n";
        out += " ║  PERSONAL STATISTICS                                                         ║\n";
        out += " ╠══════════════════════════════════════════════════════════════════════════════╣\n";
        out += " ║                                                                              ║\n";

        const pName = this.dashboardData.playerName.padEnd(28);
        const pRank = this.dashboardData.playerRank.padEnd(24);
        out += ` ║  Player: ${pName}Rank: ${pRank}║\n`;

        const vText = `Total Visits: ${s.totalVisits}`.padEnd(34);
        const tText = `Time Played: ${s.timePlayed}`.padEnd(42);
        out += ` ║  ${vText}${tText}║\n`;

        out += " ║                                                                              ║\n";
        out += " ║  ACHIEVEMENT PROGRESS                                                        ║\n";
        out += " ║  ────────────────────────────────────────────────────────────────────────    ║\n";

        for (const ach of s.achievements) {
            const bar = this.generateProgressBar(ach.progress, 22);
            const pct = Math.floor(ach.progress * 100).toString().padStart(3) + "%";
            const status = ach.complete ? "COMPLETE" : "";
            const name = ach.name.padEnd(20, '.');
            out += ` ║  ${name}${bar}  ${pct}  ${status.padEnd(18)}║\n`;
        }

        out += " ║                                                                              ║\n";
        const gsText = `Gamerscore: ${s.gamerScore}G / ${s.maxGamerScore}G`.padEnd(74);
        out += ` ║  ${gsText}║\n`;
        out += " ╚══════════════════════════════════════════════════════════════════════════════╝\n";

        return out;
    }

    generateProgressBar(percent, width) {
        const barWidth = width - 2;
        const filled = Math.floor(percent * barWidth);
        let bar = "[";
        for (let i = 0; i < barWidth; i++) {
            bar += i < filled ? "█" : "░";
        }
        bar += "]";
        return bar;
    }

    // --- DT_App_Tales.GetDisplayContent() ---
    renderTales() {
        if (this.talesStory.length === 0) return "LOADING TALES...";

        const node = this.talesCurrentNode;
        const text = this.talesStory[node] || "END OF LINE.";

        let out = ` *** STORY LOADED: "The Basement" (Chapter 1) ***\n\n`;
        out += this.wordWrap(text, 70) + "\n\n";
        out += " WHAT DO YOU DO?\n\n";

        // Build choices
        const choiceCount = this.talesChoiceCounts[node] || 0;
        const choices = [];
        if (choiceCount >= 1 && this.talesChoicesA[node]) choices.push({ l: 'A', t: this.talesChoicesA[node] });
        if (choiceCount >= 2 && this.talesChoicesB[node]) choices.push({ l: 'B', t: this.talesChoicesB[node] });
        if (choiceCount >= 3 && this.talesChoicesC[node]) choices.push({ l: 'C', t: this.talesChoicesC[node] });
        if (choiceCount >= 4 && this.talesChoicesD[node]) choices.push({ l: 'D', t: this.talesChoicesD[node] });

        for (let i = 0; i < choices.length; i++) {
            const isSel = i === this.talesSelectedChoice;
            const marker = isSel ? ">" : " ";
            out += ` ${marker} [${choices[i].l}] ${choices[i].t}\n`;
        }

        return out;
    }

    wordWrap(text, maxWidth) {
        if (!text) return "";
        const words = text.replace(/\n/g, " \n ").split(' ');
        let out = " ";
        let line = "";

        for (const word of words) {
            if (word === "\n") {
                out += line + "\n ";
                line = "";
                continue;
            }
            if ((line + word).length > maxWidth) {
                out += line + "\n ";
                line = "";
            }
            line += word + " ";
        }
        return out + line;
    }

    // --- DT_App_Weather.GetDisplayContent() ---
    renderWeather() {
        const w = this.weatherData;
        let out = ` WEATHER SENSORS - ${w.location}\n`;
        out += "════════════════════════════════════════════════════════════════════════════\n\n";

        // ASCII art
        const art = [
            "  .-~~~-.  ",
            ".-~ ~ ~-.  ",
            "(   O   )  ",
            " '-.___.-' "
        ];

        out += `      ${art[0]}                   CURRENT CONDITIONS\n`;
        out += `    ${art[1]}                   ──────────────────\n`;
        out += `    ${art[2]}                   Temperature: ${w.temperature}\n`;
        out += `    ${art[3]}                   Condition:   ${w.condition}\n`;
        out += `                                       Status:      ${w.online ? "Online" : "Offline"}\n\n`;

        out += "  FORECAST\n";
        out += "  ────────────────────────────────────────────────────────────────────────\n";
        out += "  TODAY      Tomorrow    Wednesday   Thursday    Friday\n";
        out += `   ${w.temperature}       --°F        --°F        --°F       --°F\n`;
        out += "  Sunny       Pending     Pending     Pending    Pending\n";
        out += "\n\n  [USE] Refresh  [LEFT] Back\n";

        return out;
    }

    // =======================================================================
    // INPUT HANDLING (from DT_Core input methods)
    // =======================================================================

    attachInput() {
        if (this._inputAttached) return;
        this._inputAttached = true;

        document.addEventListener('keydown', (e) => {
            if (!this.booted) return;

            // Global escape to return to shell
            if (this.currentApp !== 'SHELL' && e.key === 'Escape') {
                this.currentApp = 'SHELL';
                this.footerLine = "Returned to Shell.";
                this.render();
                return;
            }

            this.routeInput(e);
            this.render();
        });
    }

    routeInput(e) {
        switch (this.currentApp) {
            case 'SHELL': this.handleShellInput(e); break;
            case 'GITHUB': this.handleGitHubInput(e); break;
            case 'DASHBOARD': this.handleDashboardInput(e); break;
            case 'GAMES': this.handleGamesInput(e); break;
            case 'STATS': this.handleStatsInput(e); break;
            case 'TALES': this.handleTalesInput(e); break;
            case 'WEATHER': this.handleWeatherInput(e); break;
        }
    }

    handleShellInput(e) {
        if (e.key === 'ArrowUp') {
            this.menuCursor--;
            if (this.menuCursor < 0) this.menuCursor = this.menuNames.length - 1;
            e.preventDefault();
        } else if (e.key === 'ArrowDown') {
            this.menuCursor++;
            if (this.menuCursor >= this.menuNames.length) this.menuCursor = 0;
            e.preventDefault();
        } else if (e.key === 'Enter') {
            const target = this.menuNames[this.menuCursor];
            this.launchApp(target);
        }
    }

    launchApp(appName) {
        this.currentApp = appName.toUpperCase();
        this.footerLine = `Mounting virtual volume... ${appName}`;

        // Reset app state on open (OnAppOpen)
        switch (this.currentApp) {
            case 'GITHUB':
                this.githubCurrentTab = 0;
                this.githubScrollOffset = 0;
                break;
            case 'GAMES':
                this.gameCursor = 0;
                break;
            case 'TALES':
                this.talesCurrentNode = 0;
                this.talesSelectedChoice = 0;
                break;
        }
    }

    handleGitHubInput(e) {
        if (e.key === 'ArrowLeft') {
            this.githubCurrentTab--;
            if (this.githubCurrentTab < 0) this.githubCurrentTab = this.githubTabs.length - 1;
            this.githubScrollOffset = 0;
        } else if (e.key === 'ArrowRight') {
            this.githubCurrentTab++;
            if (this.githubCurrentTab >= this.githubTabs.length) this.githubCurrentTab = 0;
            this.githubScrollOffset = 0;
        } else if (e.key === 'ArrowUp') {
            if (this.githubScrollOffset > 0) this.githubScrollOffset--;
            e.preventDefault();
        } else if (e.key === 'ArrowDown') {
            this.githubScrollOffset++;
            e.preventDefault();
        }
    }

    handleDashboardInput(e) {
        if (e.key === 'Enter') {
            this.currentApp = 'SHELL';
            this.footerLine = "Returned to Shell.";
        }
    }

    handleGamesInput(e) {
        if (e.key === 'ArrowUp') {
            this.gameCursor--;
            if (this.gameCursor < 0) this.gameCursor = this.gameNames.length - 1;
            e.preventDefault();
        } else if (e.key === 'ArrowDown') {
            this.gameCursor++;
            if (this.gameCursor >= this.gameNames.length) this.gameCursor = 0;
            e.preventDefault();
        } else if (e.key === 'Enter') {
            const enabled = this.gameEnabled[this.gameCursor] !== false;
            if (enabled) {
                this.footerLine = `Launching ${this.gameNames[this.gameCursor]}... (Not implemented in web version)`;
            } else {
                this.footerLine = "Game not available yet.";
            }
        }
    }

    handleStatsInput(e) {
        if (e.key === 'Enter') {
            this.currentApp = 'SHELL';
        }
    }

    handleTalesInput(e) {
        const node = this.talesCurrentNode;
        const count = this.talesChoiceCounts[node] || 0;

        if (e.key === 'ArrowUp') {
            this.talesSelectedChoice--;
            if (this.talesSelectedChoice < 0) this.talesSelectedChoice = count - 1;
            e.preventDefault();
        } else if (e.key === 'ArrowDown') {
            this.talesSelectedChoice++;
            if (this.talesSelectedChoice >= count) this.talesSelectedChoice = 0;
            e.preventDefault();
        } else if (e.key === 'Enter') {
            let target = -1;
            if (this.talesSelectedChoice === 0) target = this.talesTargetsA[node];
            else if (this.talesSelectedChoice === 1) target = this.talesTargetsB[node];
            else if (this.talesSelectedChoice === 2) target = this.talesTargetsC[node];
            else if (this.talesSelectedChoice === 3) target = this.talesTargetsD[node];

            if (target === -1 || isNaN(target)) {
                this.currentApp = 'SHELL';
                this.talesCurrentNode = 0;
            } else {
                this.talesCurrentNode = target;
                this.talesSelectedChoice = 0;
            }
        }
    }

    handleWeatherInput(e) {
        if (e.key === 'Enter') {
            this.footerLine = "Refreshing weather data...";
        }
    }

    // =======================================================================
    // CURSOR BLINK (from DT_Core.Update)
    // =======================================================================

    startCursorBlink() {
        setInterval(() => {
            this.isCursorVisible = !this.isCursorVisible;
            this.render();
        }, 500);
    }

    // =======================================================================
    // BOOT SEQUENCE
    // =======================================================================

    renderBootSequence() {
        const steps = [
            "POST... OK",
            "BIOS v4.5... LOADED",
            "Mounting Drive A: ...",
            "Reading FATTable...",
            "Loading COMMAND.COM...",
            "BASEMENT OS v2.1"
        ];

        let step = 0;
        const bootInterval = setInterval(() => {
            this.screen.innerText += `\n${steps[step]}`;
            step++;
            if (step >= steps.length) clearInterval(bootInterval);
        }, 200);
    }
}
