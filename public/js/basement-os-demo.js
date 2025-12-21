/**
 * BasementOS Demo - Web Terminal Emulator
 * Simulates VRChat BasementOS terminal with horizontal-only borders
 */

class BasementOSDemo {
    constructor(containerId) {
        this.container = document.getElementById(containerId);
        this.booted = false;
        this.currentApp = 'SHELL';
        this.menuCursor = 0;
        this.isCursorVisible = true;
        this.cursorInterval = null;
        this.tickerInterval = null;
        this.rssLine = 'BASEMENT OS // LIVE FEED // Welcome to the simulation...';
        this.tickerOffset = 0;

        // Menu data
        this.menuNames = ['DASHBOARD', 'STATS', 'GAMES', 'TALES', 'WEATHER', 'GITHUB'];
        this.menuTypes = ['<APP>', '<APP>', '<DIR>', '<APP>', '<APP>', '<WEB>'];
        this.menuDescriptions = [
            'System status and player info',
            'Personal statistics and achievements',
            'Game cartridge selection',
            'Interactive story experience',
            'Weather sensor data',
            'Repository information'
        ];

        // Dashboard data
        this.dashboardData = {
            memoryStatus: '[OK]',
            weatherStatus: '[OK]',
            mailStatus: '[--]',
            onlineCount: 3,
            onlinePlayers: ['Player1', 'Player2', 'Player3'],
            playerName: 'GUEST',
            playerRank: 'VISITOR',
            playerScore: 0,
            playerVisits: 1,
            weatherLocation: 'BASEMENT',
            weatherTemp: '72°F',
            weatherCondition: 'Clear'
        };

        // Stats data
        this.statsData = {
            totalVisits: 42,
            timePlayed: '2h 15m',
            gamerScore: 150,
            maxGamerScore: 420,
            achievements: [
                { name: 'First Steps', progress: 1.0, complete: true },
                { name: 'Explorer', progress: 0.6, complete: false },
                { name: 'Completionist', progress: 0.25, complete: false }
            ]
        };

        // Games data
        this.gameNames = ['PONG', 'SNAKE', 'TETRIS', 'ASTEROIDS'];
        this.gameDescriptions = [
            'Classic paddle game',
            'Eat, grow, survive',
            'Block stacking puzzle',
            'Space rock destruction'
        ];
        this.gameEnabled = [true, true, false, false];
        this.gameCursor = 0;

        // Tales data
        this.talesStory = [
            'You wake up in a dark basement. A computer screen glows in the corner.',
            'You approach the computer. It displays a terminal interface.',
            'The terminal welcomes you to BASEMENT OS.'
        ];
        this.talesCurrentNode = 0;
        this.talesSelectedChoice = 0;
        this.talesChoiceCounts = [2, 2, 1];
        this.talesChoicesA = ['Look around', 'Approach computer', 'Continue'];
        this.talesChoicesB = ['Stay still', 'Examine the room', null];
        this.talesChoicesC = [null, null, null];
        this.talesChoicesD = [null, null, null];

        // Weather data
        this.weatherData = {
            location: 'BASEMENT LAB',
            temperature: '72°F / 22°C',
            condition: 'Simulated Clear',
            online: true
        };

        // GitHub data
        this.githubTabs = ['README', 'CHANGELOG', 'ISSUES', 'THANKS'];
        this.githubCurrentTab = 0;
        this.githubScrollOffset = 0;
        this.githubContent = {
            readme: 'BASEMENT OS\n\nA VRChat terminal simulation.\n\nFeatures:\n- Interactive terminal UI\n- Multiple applications\n- Persistent player data\n- Weather integration',
            changelog: 'v2.0.0 - Complete rewrite\nv1.5.0 - Added weather\nv1.0.0 - Initial release',
            issues: 'No open issues.\n\nAll systems operational.',
            thanks: 'Thank you for visiting!\n\nSpecial thanks to all contributors.'
        };
    }

    init() {
        this.showBootSequence();
    }

    showBootSequence() {
        const bootLines = [
            'BASEMENT OS v2.0',
            'Initializing hardware...',
            'Loading kernel modules...',
            'Starting services...',
            'Connecting to simulation...',
            '',
            'BOOT COMPLETE',
            ''
        ];

        let lineIndex = 0;
        let bootText = '';

        const bootInterval = setInterval(() => {
            if (lineIndex < bootLines.length) {
                bootText += bootLines[lineIndex] + '\n';
                this.container.innerText = bootText;
                lineIndex++;
            } else {
                clearInterval(bootInterval);
                this.booted = true;
                this.startCursorBlink();
                this.startTicker();
                this.render();
            }
        }, 200);
    }

    startCursorBlink() {
        this.cursorInterval = setInterval(() => {
            this.isCursorVisible = !this.isCursorVisible;
            if (this.booted) this.render();
        }, 500);
    }

    startTicker() {
        const fullTicker = '          -- CloudStrife signed in --          -- xAngelx unlocked: First Steps 10G --          -- Weather: Clear 72°F --          -- Status: ONLINE --          -- NightOwl earned: Night Owl 25G --          -- UPTIME: Eternal --          -- Player1 signed in --          ';
        this.tickerInterval = setInterval(() => {
            this.tickerOffset = (this.tickerOffset + 1) % fullTicker.length;
            this.rssLine = fullTicker.substring(this.tickerOffset) + fullTicker.substring(0, this.tickerOffset);
            if (this.booted) this.render();
        }, 150);
    }

    destroy() {
        if (this.cursorInterval) clearInterval(this.cursorInterval);
        if (this.tickerInterval) clearInterval(this.tickerInterval);
        this.booted = false;
    }

    handleKeyDown(e) {
        if (!this.booted) return;

        switch (this.currentApp) {
            case 'SHELL':
                this.handleShellInput(e);
                break;
            case 'GITHUB':
                this.handleGitHubInput(e);
                break;
            case 'GAMES':
                this.handleGamesInput(e);
                break;
            case 'TALES':
                this.handleTalesInput(e);
                break;
            default:
                if (e.key === 'Escape') {
                    this.currentApp = 'SHELL';
                    this.render();
                }
                break;
        }
    }

    handleShellInput(e) {
        switch (e.key) {
            case 'ArrowUp':
                this.menuCursor = Math.max(0, this.menuCursor - 1);
                break;
            case 'ArrowDown':
                this.menuCursor = Math.min(this.menuNames.length - 1, this.menuCursor + 1);
                break;
            case 'Enter':
                this.launchApp(this.menuNames[this.menuCursor]);
                break;
        }
        this.render();
    }

    handleGitHubInput(e) {
        switch (e.key) {
            case 'ArrowLeft':
                this.githubCurrentTab = Math.max(0, this.githubCurrentTab - 1);
                this.githubScrollOffset = 0;
                break;
            case 'ArrowRight':
                this.githubCurrentTab = Math.min(this.githubTabs.length - 1, this.githubCurrentTab + 1);
                this.githubScrollOffset = 0;
                break;
            case 'ArrowUp':
                this.githubScrollOffset = Math.max(0, this.githubScrollOffset - 1);
                break;
            case 'ArrowDown':
                this.githubScrollOffset++;
                break;
            case 'Escape':
                this.currentApp = 'SHELL';
                break;
        }
        this.render();
    }

    handleGamesInput(e) {
        switch (e.key) {
            case 'ArrowUp':
                this.gameCursor = Math.max(0, this.gameCursor - 1);
                break;
            case 'ArrowDown':
                this.gameCursor = Math.min(this.gameNames.length - 1, this.gameCursor + 1);
                break;
            case 'Escape':
                this.currentApp = 'SHELL';
                break;
        }
        this.render();
    }

    handleTalesInput(e) {
        const choiceCount = this.talesChoiceCounts[this.talesCurrentNode] || 0;
        switch (e.key) {
            case 'ArrowUp':
                this.talesSelectedChoice = Math.max(0, this.talesSelectedChoice - 1);
                break;
            case 'ArrowDown':
                this.talesSelectedChoice = Math.min(choiceCount - 1, this.talesSelectedChoice + 1);
                break;
            case 'Enter':
                if (this.talesCurrentNode < this.talesStory.length - 1) {
                    this.talesCurrentNode++;
                    this.talesSelectedChoice = 0;
                }
                break;
            case 'Escape':
                this.currentApp = 'SHELL';
                break;
        }
        this.render();
    }

    launchApp(appName) {
        this.currentApp = appName;
        this.render();
    }

    // =======================================================================
    // MAIN RENDER - Horizontal borders only
    // =======================================================================

    render() {
        if (!this.booted) return;

        const content = this.getActiveAppContent();
        const W = 80;

        // Build header
        const title = 'BASEMENT OS // VERSION 2';
        const status = '[SIMULATION ONLINE]';
        const headerContent = ' ' + title + ' '.repeat(W - title.length - status.length - 2) + status;

        // Build ticker
        const tickerContent = ' ' + this.rssLine.substring(0, W - 2);

        // Build footer
        const cursor = this.isCursorVisible ? '█' : ' ';
        const prompt = `C:\\BASEMENT>${cursor}`;
        const help = '[↑/↓] Navigate  [Enter] Select  [Esc] Back';
        const footerLine = prompt + ' '.repeat(W - prompt.length - help.length) + help;

        // Assemble buffer - horizontal borders only
        let buffer = '';
        buffer += '═'.repeat(W) + '\n';
        buffer += headerContent + '\n';
        buffer += '═'.repeat(W) + '\n';
        buffer += tickerContent + '\n';
        buffer += '─'.repeat(W) + '\n';
        buffer += content;
        buffer += '═'.repeat(W) + '\n';
        buffer += footerLine;

        this.container.innerText = buffer;
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
            default: return 'NO APP LOADED\n';
        }
    }

    // =======================================================================
    // APP RENDERERS - No vertical borders
    // =======================================================================

    renderShell() {
        let out = '';
        out += '\n';
        out += ' C:\\BASEMENT\\MENU\n';
        out += '\n';
        out += '   TYPE     NAME             DESCRIPTION\n';
        out += '   ────     ────────────     ───────────────────────────────────────\n';

        for (let i = 0; i < this.menuNames.length; i++) {
            const isSel = i === this.menuCursor;
            const marker = isSel ? '>' : ' ';
            const type = (this.menuTypes[i] || '<DIR>').padEnd(8);
            const name = (this.menuNames[i] || 'UNKNOWN').padEnd(16);
            const desc = (this.menuDescriptions[i] || '').substring(0, 35);
            out += ` ${marker} ${type} ${name} ${desc}\n`;
        }

        // Pad to fill area
        const linesNeeded = 10 - this.menuNames.length;
        for (let i = 0; i < linesNeeded; i++) {
            out += '\n';
        }

        return out;
    }

    renderGitHub() {
        let out = '';
        out += '\n';

        let tabLine = ' ';
        for (let i = 0; i < this.githubTabs.length; i++) {
            const marker = i === this.githubCurrentTab ? '*' : ' ';
            tabLine += `[${marker}${this.githubTabs[i]}${marker}]  `;
        }
        out += tabLine + '\n';
        out += ' ────────────────────────────────────────────────────────────────────────────\n';

        let content = "";
        switch (this.githubCurrentTab) {
            case 0: content = this.githubContent.readme; break;
            case 1: content = this.githubContent.changelog; break;
            case 2: content = this.githubContent.issues; break;
            case 3: content = this.githubContent.thanks; break;
        }

        const lines = content.split('\n');
        const visibleLines = 8;
        const visible = lines.slice(this.githubScrollOffset, this.githubScrollOffset + visibleLines);
        visible.forEach(l => {
            out += ' ' + l.substring(0, 76) + '\n';
        });

        for (let i = visible.length; i < visibleLines; i++) {
            out += '\n';
        }

        const scrollInfo = `Lines ${this.githubScrollOffset + 1}-${this.githubScrollOffset + visible.length} of ${lines.length}  [←/→] Tabs  [↑/↓] Scroll`;
        out += ' ' + scrollInfo + '\n';

        return out;
    }

    renderDashboard() {
        const d = this.dashboardData;
        let out = '';

        out += '\n';
        out += '  [SYSTEM STATUS]                    [WHO IS ONLINE]\n';
        out += `  ${d.memoryStatus} Memory Check               ${d.onlineCount} users connected\n`;
        out += `  ${d.weatherStatus} Weather Sensors            ${d.onlinePlayers[0] || ''}\n`;
        out += `  ${d.mailStatus} Mail Server (Offline)\n`;
        out += '\n';
        out += '  [PLAYER STATS]                     [WEATHER]\n';
        out += `  Rank:   ${d.playerRank.padEnd(12)}          ${d.weatherLocation}\n`;
        out += `  Score:  ${String(d.playerScore).padEnd(4)}G / 420G         ${d.weatherTemp}  ${d.weatherCondition}\n`;
        out += `  Visits: ${d.playerVisits}\n`;

        return out;
    }

    renderGames() {
        let out = '';

        if (this.gameNames.length === 0) {
            out += '\n';
            out += '   ERROR: No game cartridges found.\n';
            out += '\n';
            return out;
        }

        out += '\n';
        out += ' SELECT CARTRIDGE\n';
        out += '\n';

        for (let i = 0; i < Math.min(this.gameNames.length, 4); i++) {
            const isSel = i === this.gameCursor;
            const cursor = isSel ? '>' : ' ';
            const name = this.gameNames[i] || 'UNKNOWN';
            const desc = (this.gameDescriptions[i] || 'No description.').substring(0, 50);
            const enabled = this.gameEnabled[i] !== false;
            const status = enabled ? 'Ready' : 'Coming Soon';

            out += `  ${cursor} [${i + 1}] ${name.padEnd(20)} ${status}\n`;
            out += `        ${desc}\n`;
        }

        // Pad remaining
        const linesUsed = 3 + (Math.min(this.gameNames.length, 4) * 2);
        for (let i = linesUsed; i < 11; i++) {
            out += '\n';
        }

        return out;
    }

    renderStats() {
        const s = this.statsData;
        let out = '';

        out += '\n';
        out += '  [PERSONAL STATISTICS]\n';
        out += '\n';
        out += `  Player: ${this.dashboardData.playerName.padEnd(20)} Rank: ${this.dashboardData.playerRank}\n`;
        out += `  Total Visits: ${String(s.totalVisits).padEnd(10)} Time Played: ${s.timePlayed}\n`;
        out += '\n';
        out += '  [ACHIEVEMENTS]\n';

        for (const ach of s.achievements.slice(0, 3)) {
            const bar = this.generateProgressBar(ach.progress, 20);
            const pct = Math.floor(ach.progress * 100).toString().padStart(3) + '%';
            const status = ach.complete ? '✓' : ' ';
            out += `  ${ach.name.padEnd(18)} ${bar} ${pct} ${status}\n`;
        }

        out += '\n';
        out += `  Gamerscore: ${s.gamerScore}G / ${s.maxGamerScore}G\n`;

        return out;
    }

    generateProgressBar(percent, width) {
        const filled = Math.floor(percent * width);
        return '[' + '█'.repeat(filled) + '░'.repeat(width - filled) + ']';
    }

    renderTales() {
        let out = '';

        if (this.talesStory.length === 0) {
            out += ' LOADING TALES...\n';
            return out;
        }

        const node = this.talesCurrentNode;
        const text = this.talesStory[node] || "END OF LINE.";
        out += '\n';
        out += '  *** STORY: "The Basement" ***\n';
        out += '\n';

        const wrapped = this.wordWrap(text, 70);
        const lines = wrapped.split('\n').slice(0, 4);
        lines.forEach(l => {
            out += '  ' + l + '\n';
        });

        out += '\n';
        out += '  WHAT DO YOU DO?\n';

        const choiceCount = this.talesChoiceCounts[node] || 0;
        const choices = [];
        if (choiceCount >= 1 && this.talesChoicesA[node]) choices.push({ l: 'A', t: this.talesChoicesA[node] });
        if (choiceCount >= 2 && this.talesChoicesB[node]) choices.push({ l: 'B', t: this.talesChoicesB[node] });
        if (choiceCount >= 3 && this.talesChoicesC[node]) choices.push({ l: 'C', t: this.talesChoicesC[node] });
        if (choiceCount >= 4 && this.talesChoicesD[node]) choices.push({ l: 'D', t: this.talesChoicesD[node] });

        for (let i = 0; i < choices.length; i++) {
            const isSel = i === this.talesSelectedChoice;
            const marker = isSel ? '>' : ' ';
            const choiceText = choices[i].t.substring(0, 60);
            out += `  ${marker} [${choices[i].l}] ${choiceText}\n`;
        }

        return out;
    }

    wordWrap(text, maxWidth) {
        if (!text) return "";
        const words = text.replace(/\n/g, " \n ").split(' ');
        let out = "";
        let line = "";

        for (const word of words) {
            if (word === "\n") {
                out += line + "\n";
                line = "";
                continue;
            }
            if ((line + word).length > maxWidth) {
                out += line + "\n";
                line = "";
            }
            line += word + " ";
        }
        return out + line;
    }

    renderWeather() {
        const w = this.weatherData;
        let out = '';

        out += '\n';
        out += `  [WEATHER SENSORS - ${w.location}]\n`;
        out += '\n';
        out += '       .-~~~-.                    CURRENT CONDITIONS\n';
        out += '     .-~     ~-.                  ──────────────────\n';
        out += `     (    ☀    )                  Temperature: ${w.temperature}\n`;
        out += `      '-._____.-'                  Condition:   ${w.condition}\n`;
        out += `                                   Status:      ${w.online ? 'Online' : 'Offline'}\n`;
        out += '\n';
        out += '  Data refreshes automatically from VRChat world sensors.\n';

        return out;
    }
}

// Keyboard event binding
document.addEventListener('keydown', (e) => {
    if (window.basementOSInstance) {
        window.basementOSInstance.handleKeyDown(e);
    }
});

// Export for use
window.BasementOSDemo = BasementOSDemo;
