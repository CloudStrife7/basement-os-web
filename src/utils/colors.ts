/**
 * Calculates the best contrast text color (black or white) for a given hex background color.
 * Uses standard luminance formula.
 * @param hexColor The background color in hex format (e.g., "ff0000", "#ff0000")
 * @returns "#000000" or "#ffffff"
 */
export function getContrastColor(hexColor: string): string {
    // Remove # if present
    const hex = hexColor.replace('#', '');

    // Convert to RGB
    const r = parseInt(hex.substr(0, 2), 16);
    const g = parseInt(hex.substr(2, 2), 16);
    const b = parseInt(hex.substr(4, 2), 16);

    // Calculate luminance
    // Formula: L = 0.299*R + 0.587*G + 0.114*B
    const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;

    // Return white for dark backgrounds, black for light
    // 0.5 is the standard threshold
    return luminance > 0.5 ? '#000000' : '#ffffff';
}
