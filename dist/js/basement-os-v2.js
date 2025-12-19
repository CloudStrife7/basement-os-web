/**
 * BASEMENT OS WEB EMULATOR v2 - INDUSTRIAL MILITARY THEME
 * Extended with demo apps: GLITCH, SETTINGS, PROGRESS
 */
class BasementOSv2 {
    constructor(containerId) {
        this.container = document.getElementById(containerId);
        this.screen = this.container.querySelector('.terminal-screen');

        // === CORE STATE ===
        this.booted = false;
        this.currentApp = null;
        this.isCursorVisible = true;

        // Screen Buffer
        this.SCREEN_WIDTH = 80;

        // === SHELL STATE ===
        this.menuNames = ['DASHBOARD', 'SETTINGS', 'GLITCH', 'PROGRESS', 'STATS', 'ABOUT'];
        this.menuTypes = ['<APP>', '<CFG>', '<FX>', '<BAR>', '<DAT>', '<DOC>'];
        this.menuDescriptions = [
            'System overview and status',
            'Toggle switches & configuration',
            'Visual glitch effect demos',
            'Animated progress bars',
            'System statistics',
            'About this terminal'
        ];
        this.menuCursor = 0;

        // === SETTINGS STATE ===
        this.settings = {
            voiceover: true,
            gridOverlay: false,
            debugStream: false,
            scanlines: true,
            audioFeedback: true,
            nightMode: false
        };
        this.settingsCursor = 0;
        this.settingsKeys = ['voiceover', 'gridOverlay', 'debugStream', 'scanlines', 'audioFeedback', 'nightMode'];
        this.settingsLabels = ['VOICEOVER MODE', 'GRID OVERLAY', 'DEBUG STREAM', 'SCANLINES', 'AUDIO FEEDBACK', 'NIGHT MODE'];

        // === PROGRESS STATE ===
        this.progressBars = [
            { label: 'SYSTEM UPLOAD', value: 0, target: 100, speed: 0.8, status: 'UPLOADING...' },
            { label: 'DATA SYNC', value: 0, target: 72, speed: 0.5, status: 'SYNCING...' },
            { label: 'MEMORY DEFRAG', value: 0, target: 45, speed: 0.3, status: 'PROCESSING...' },
            { label: 'NETWORK SCAN', value: 0, target: 88, speed: 1.2, status: 'SCANNING...' }
        ];

        // === GLITCH STATE ===
        this.glitchText = 'SYSTEM OPERATIONAL - ALL SECTORS NOMINAL - UPLINK STABLE';
        this.glitchFrame = 0;

        // Colors for Industrial Military theme
        this.C = {
            struct: '#064E3B',
            prim: '#10B981',
            high: '#34D399',
            dim: '#022C22',
            err: '#F87171'
        };

        this.init();
    }

    // =======================================================================
    // INITIALIZATION
    // =======================================================================

    async init() {
        this.renderBootSequence();

        setTimeout(() => {
            this.booted = true;
            this.currentApp = 'SHELL';
            this.attachInput();
            this.startCursorBlink();
            this.render();
        }, 1500);
    }

    // =======================================================================
    // SCREEN RENDERING
    // =======================================================================

    render() {
        if (!this.booted) return;

        const content = this.getActiveAppContent();
        const header = this.renderHeader();
        const footer = this.renderFooter();

        let buffer = header + '\n' + content + '\n' + footer;

        this.screen.innerHTML = buffer;
    }

    renderHeader() {
        return `<span style="color:${this.C.struct}">╔══════════════════════════════════════════════════════════════════════════════╗</span>
<span style="color:${this.C.struct}">║</span> <span style="color:${this.C.high}">BASEMENT_OS v5.1 // AUTO-DIRECTOR</span>                                 <span style="color:${this.C.high}">[ONLINE]</span> <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">╚══════════════════════════════════════════════════════════════════════════════╝</span>`;
    }

    renderFooter() {
        const cursor = this.isCursorVisible ? '_' : ' ';
        return `<span style="color:${this.C.dim}">─────────────────────────────────────────────────────────────────────────────────</span>
<span style="color:${this.C.struct}">C:\\BASEMENT></span><span style="color:${this.C.high}">${cursor}</span>`;
    }

    getActiveAppContent() {
        switch (this.currentApp) {
            case 'SHELL': return this.renderShell();
            case 'DASHBOARD': return this.renderDashboard();
            case 'SETTINGS': return this.renderSettings();
            case 'GLITCH': return this.renderGlitch();
            case 'PROGRESS': return this.renderProgress();
            case 'STATS': return this.renderStats();
            case 'ABOUT': return this.renderAbout();
            default: return 'NO APP LOADED';
        }
    }

    // =======================================================================
    // APP RENDERERS
    // =======================================================================

    renderShell() {
        let out = `\n<span style="color:${this.C.high}"> C:\\BASEMENT\\MENU</span>\n\n`;
        out += `<span style="color:${this.C.struct}">   TYPE     NAME             DESCRIPTION</span>\n`;
        out += `<span style="color:${this.C.struct}">   ────     ────────────     ───────────────────────────────────────</span>\n`;

        for (let i = 0; i < this.menuNames.length; i++) {
            const isSel = i === this.menuCursor;
            const marker = isSel ? `<span style="color:${this.C.high}">></span>` : ' ';
            const type = this.menuTypes[i].padEnd(8);
            const name = this.menuNames[i].padEnd(16);
            const desc = this.menuDescriptions[i];

            if (isSel) {
                out += ` ${marker} <span style="color:${this.C.high}">${type}</span> <span style="color:${this.C.high}">${name}</span> <span style="color:${this.C.prim}">${desc}</span>\n`;
            } else {
                out += ` ${marker} <span style="color:${this.C.struct}">${type}</span> <span style="color:${this.C.prim}">${name}</span> <span style="color:${this.C.struct}">${desc}</span>\n`;
            }
        }

        out += `\n<span style="color:${this.C.dim}">  [↑/↓] Navigate   [ENTER] Select   [ESC] Back</span>`;
        return out;
    }

    renderDashboard() {
        return `
<span style="color:${this.C.struct}">╔══</span> <span style="color:${this.C.high}">[SYSTEM DASHBOARD]</span> <span style="color:${this.C.struct}">════════════════════════════════════════════════════════╗</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.struct}">[ SYSTEM STATUS ]</span>                                                          <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">KERNEL:</span> <span style="color:${this.C.high}">OPERATIONAL</span>     <span style="color:${this.C.prim}">MEMORY:</span> <span style="color:${this.C.high}">64KB OK</span>     <span style="color:${this.C.prim}">UPTIME:</span> <span style="color:${this.C.high}">847:23:01</span>      <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.struct}">[ NETWORK ]</span>                                                                <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">UPLINK:</span> <span style="color:${this.C.high}">CONNECTED</span>      <span style="color:${this.C.prim}">LATENCY:</span> <span style="color:${this.C.high}">23ms</span>       <span style="color:${this.C.prim}">PACKETS:</span> <span style="color:${this.C.high}">1,247,832</span>    <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.struct}">[ WARNINGS ]</span>                                                               <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.err}">! SECTOR 7 - LIGHTING CONDITIONS SUB-OPTIMAL</span>                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.err}">! AUDIO STREAM - MINOR CORRUPTION DETECTED</span>                                <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">╚══════════════════════════════════════════════════════════════════════════════╝</span>

<span style="color:${this.C.dim}">  [ESC] Return to Menu</span>`;
    }

    renderSettings() {
        let out = `
<span style="color:${this.C.struct}">╔══</span> <span style="color:${this.C.high}">[SYSTEM SETTINGS]</span> <span style="color:${this.C.struct}">═════════════════════════════════════════════════════════╗</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
`;

        for (let i = 0; i < this.settingsKeys.length; i++) {
            const key = this.settingsKeys[i];
            const label = this.settingsLabels[i].padEnd(20);
            const isOn = this.settings[key];
            const isSel = i === this.settingsCursor;

            const dots = '.'.repeat(25);

            let onStyle, offStyle;
            if (isOn) {
                onStyle = `<span style="color:${this.C.high}">█ ON █</span>`;
                offStyle = `<span style="color:${this.C.dim}">░ OFF ░</span>`;
            } else {
                onStyle = `<span style="color:${this.C.dim}">░ ON ░</span>`;
                offStyle = `<span style="color:${this.C.high}">█ OFF █</span>`;
            }

            // Error state for debug stream
            if (key === 'debugStream' && this.settings[key]) {
                onStyle = `<span style="color:${this.C.err}">█ ERR █</span>`;
            }

            const marker = isSel ? `<span style="color:${this.C.high}">></span>` : ' ';
            const labelColor = isSel ? this.C.high : this.C.prim;

            out += `<span style="color:${this.C.struct}">║</span> ${marker} <span style="color:${labelColor}">${label}</span> <span style="color:${this.C.struct}">${dots}</span> ${onStyle} ${offStyle} <span style="color:${this.C.struct}">║</span>\n`;
        }

        out += `<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">╚══════════════════════════════════════════════════════════════════════════════╝</span>

<span style="color:${this.C.dim}">  [↑/↓] Navigate   [ENTER] Toggle   [ESC] Back</span>`;
        return out;
    }

    renderGlitch() {
        // Generate glitched text
        const glitchChars = '#$@%&*!?';
        let text = this.glitchText;

        // Random character replacement
        if (Math.random() < 0.3) {
            const pos = Math.floor(Math.random() * text.length);
            const char = glitchChars[Math.floor(Math.random() * glitchChars.length)];
            text = text.slice(0, pos) + char + text.slice(pos + 1);
        }

        // Color bleed effect
        let coloredText = '';
        for (let i = 0; i < text.length; i++) {
            if (Math.random() < 0.02) {
                coloredText += `<span style="color:${this.C.err}">${text[i]}</span>`;
            } else {
                coloredText += text[i];
            }
        }

        // Glitch line samples
        const sample1 = this.addGlitch('KERNEL_PANIC_CHECK... OK');
        const sample2 = this.addGlitch('MEMORY ALLOCATION... 64KB');
        const sample3 = this.addGlitch('RENDER PIPELINE... ACTIVE');

        return `
<span style="color:${this.C.struct}">╔══</span> <span style="color:${this.C.high}">[GLITCH DEMONSTRATION]</span> <span style="color:${this.C.struct}">════════════════════════════════════════════════╗</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.struct}">[ LIVE CORRUPTION FEED ]</span>                                                   <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">${coloredText}</span>         <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.struct}">[ SAMPLE GLITCHES ]</span>                                                        <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  ${sample1}                                       <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  ${sample2}                                       <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  ${sample3}                                     <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.err}">█ TRANSMISSION </span><span style="color:${this.C.err};animation:blink 0.5s infinite">█</span><span style="color:${this.C.err}"> INTERRUPTED █</span>                                          <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">╚══════════════════════════════════════════════════════════════════════════════╝</span>

<span style="color:${this.C.dim}">  Glitches refresh every 100ms   [ESC] Back</span>`;
    }

    addGlitch(text) {
        const glitchChars = '#$@%&';
        let result = '';
        for (let i = 0; i < text.length; i++) {
            if (Math.random() < 0.05) {
                result += `<span style="color:${this.C.err}">${glitchChars[Math.floor(Math.random() * glitchChars.length)]}</span>`;
            } else {
                result += `<span style="color:${this.C.prim}">${text[i]}</span>`;
            }
        }
        return result;
    }

    renderProgress() {
        let out = `
<span style="color:${this.C.struct}">╔══</span> <span style="color:${this.C.high}">[PROGRESS MONITOR]</span> <span style="color:${this.C.struct}">════════════════════════════════════════════════════╗</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
`;

        for (const bar of this.progressBars) {
            const pct = Math.floor(bar.value);
            const filled = Math.floor(bar.value / 2);
            const empty = 50 - filled;

            const filledStr = '█'.repeat(filled);
            const emptyStr = '░'.repeat(empty);

            const color = bar.value >= bar.target ? this.C.high : (bar.value < 30 ? this.C.err : this.C.prim);
            const status = bar.value >= bar.target ? 'COMPLETE' : bar.status;

            out += `<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">${bar.label.padEnd(15)}</span>                                                      <span style="color:${this.C.struct}">║</span>\n`;
            out += `<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.struct}">[</span><span style="color:${color}">${filledStr}</span><span style="color:${this.C.dim}">${emptyStr}</span><span style="color:${this.C.struct}">]</span> <span style="color:${color}">${String(pct).padStart(3)}%</span> <span style="color:${this.C.struct}">${status}</span>     <span style="color:${this.C.struct}">║</span>\n`;
            out += `<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>\n`;
        }

        out += `<span style="color:${this.C.struct}">╚══════════════════════════════════════════════════════════════════════════════╝</span>

<span style="color:${this.C.dim}">  Progress bars animate in real-time   [ESC] Back</span>`;
        return out;
    }

    renderStats() {
        return `
<span style="color:${this.C.struct}">╔══</span> <span style="color:${this.C.high}">[SYSTEM STATISTICS]</span> <span style="color:${this.C.struct}">═══════════════════════════════════════════════════════╗</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">CPU LOAD:</span>     <span style="color:${this.C.high}">47%</span>                                                        <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">MEMORY:</span>       <span style="color:${this.C.high}">32KB / 64KB</span>                                                <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">DISK:</span>         <span style="color:${this.C.high}">1.2MB / 10MB</span>                                               <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">NETWORK:</span>      <span style="color:${this.C.high}">1.2 Mbps ↑ / 4.8 Mbps ↓</span>                                    <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">TEMPERATURE:</span>  <span style="color:${this.C.err}">78°C</span>                                                       <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">╚══════════════════════════════════════════════════════════════════════════════╝</span>

<span style="color:${this.C.dim}">  [ESC] Return to Menu</span>`;
    }

    renderAbout() {
        return `
<span style="color:${this.C.struct}">╔══</span> <span style="color:${this.C.high}">[ABOUT THIS TERMINAL]</span> <span style="color:${this.C.struct}">═════════════════════════════════════════════════════╗</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.high}">BASEMENT_OS v5.1 - INDUSTRIAL MILITARY EDITION</span>                            <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">This terminal demonstrates the Industrial Military aesthetic</span>             <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">designed for VRChat/Udon TextMeshPro integration.</span>                         <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.struct}">[ COLOR PALETTE ]</span>                                                          <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.struct}">Structure:</span> <span style="color:${this.C.struct}">#064E3B</span>  (borders, inactive)                                <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.prim}">Primary:</span>   <span style="color:${this.C.prim}">#10B981</span>  (body text)                                       <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.high}">Highlight:</span> <span style="color:${this.C.high}">#34D399</span>  (headers, active)                                   <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>  <span style="color:${this.C.err}">Error:</span>     <span style="color:${this.C.err}">#F87171</span>  (warnings, glitches)                                  <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">║</span>                                                                              <span style="color:${this.C.struct}">║</span>
<span style="color:${this.C.struct}">╚══════════════════════════════════════════════════════════════════════════════╝</span>

<span style="color:${this.C.dim}">  [ESC] Return to Menu</span>`;
    }

    // =======================================================================
    // INPUT HANDLING
    // =======================================================================

    attachInput() {
        if (this._inputAttached) return;
        this._inputAttached = true;

        document.addEventListener('keydown', (e) => {
            if (!this.booted) return;

            if (this.currentApp !== 'SHELL' && e.key === 'Escape') {
                this.currentApp = 'SHELL';
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
            case 'SETTINGS': this.handleSettingsInput(e); break;
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

    handleSettingsInput(e) {
        if (e.key === 'ArrowUp') {
            this.settingsCursor--;
            if (this.settingsCursor < 0) this.settingsCursor = this.settingsKeys.length - 1;
            e.preventDefault();
        } else if (e.key === 'ArrowDown') {
            this.settingsCursor++;
            if (this.settingsCursor >= this.settingsKeys.length) this.settingsCursor = 0;
            e.preventDefault();
        } else if (e.key === 'Enter') {
            const key = this.settingsKeys[this.settingsCursor];
            this.settings[key] = !this.settings[key];
        }
    }

    launchApp(appName) {
        this.currentApp = appName.toUpperCase();

        // Start animations for specific apps
        if (this.currentApp === 'GLITCH') {
            this.startGlitchAnimation();
        } else if (this.currentApp === 'PROGRESS') {
            this.startProgressAnimation();
        }
    }

    // =======================================================================
    // ANIMATIONS
    // =======================================================================

    startGlitchAnimation() {
        if (this._glitchInterval) clearInterval(this._glitchInterval);
        this._glitchInterval = setInterval(() => {
            if (this.currentApp === 'GLITCH') {
                this.glitchFrame++;
                this.render();
            } else {
                clearInterval(this._glitchInterval);
            }
        }, 100);
    }

    startProgressAnimation() {
        if (this._progressInterval) clearInterval(this._progressInterval);

        // Reset progress bars
        this.progressBars.forEach(bar => bar.value = 0);

        this._progressInterval = setInterval(() => {
            if (this.currentApp === 'PROGRESS') {
                let allComplete = true;
                this.progressBars.forEach(bar => {
                    if (bar.value < bar.target) {
                        bar.value += bar.speed;
                        if (bar.value > bar.target) bar.value = bar.target;
                        allComplete = false;
                    }
                });
                this.render();

                // Reset when all complete
                if (allComplete) {
                    setTimeout(() => {
                        this.progressBars.forEach(bar => bar.value = 0);
                    }, 2000);
                }
            } else {
                clearInterval(this._progressInterval);
            }
        }, 50);
    }

    // =======================================================================
    // CURSOR BLINK
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
            `<span style="color:${this.C.prim}">POST... OK</span>`,
            `<span style="color:${this.C.prim}">BIOS v4.5... LOADED</span>`,
            `<span style="color:${this.C.prim}">Mounting Drive A: ...</span>`,
            `<span style="color:${this.C.high}">BASEMENT_OS v5.1 - INDUSTRIAL MILITARY</span>`
        ];

        let step = 0;
        const bootInterval = setInterval(() => {
            this.screen.innerHTML += `\n${steps[step]}`;
            step++;
            if (step >= steps.length) clearInterval(bootInterval);
        }, 300);
    }
}
