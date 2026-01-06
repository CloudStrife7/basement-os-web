/**
 * Centralized Mode State logic for BasementOS
 * Handles state persistence, DOM updates, and cross-component synchronization.
 */

export type AudienceMode = 'player' | 'developer' | 'both';

export const MODES: AudienceMode[] = ['player', 'developer', 'both'];

export function getSavedMode(): AudienceMode {
    if (typeof localStorage === 'undefined') return 'both';
    const saved = localStorage.getItem('basement-os-mode');
    return (MODES.includes(saved as AudienceMode) ? saved : 'both') as AudienceMode;
}

export function setAudienceMode(mode: AudienceMode) {
    if (typeof localStorage === 'undefined') return;

    localStorage.setItem('basement-os-mode', mode);

    // Update DOM classes for CSS visibility rules
    document.documentElement.classList.remove('mode-player', 'mode-developer', 'mode-both');
    document.documentElement.classList.add(`mode-${mode}`);

    // Dispatch custom event for vanilla components to listen to
    window.dispatchEvent(new CustomEvent('basement-os-mode-change', {
        detail: { mode }
    }));

    // Update dynamic links (like Roadmap)
    updateDynamicLinks(mode);
}

export function updateDynamicLinks(mode: AudienceMode) {
    const roadmapLink = document.getElementById('roadmap-link') as HTMLAnchorElement | null;
    if (roadmapLink) {
        if (mode === 'player') roadmapLink.href = '/roadmap/player';
        else if (mode === 'developer') roadmapLink.href = '/roadmap/dev';
        else roadmapLink.href = '/roadmap';
    }
}

// Initial setup to run on page load
export function initModeState() {
    const mode = getSavedMode();
    setAudienceMode(mode);
}
