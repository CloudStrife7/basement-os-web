using UnityEngine;

namespace SymbolicGraphics
{
    /// <summary>
    /// Comprehensive Unicode character library for symbolic rendering.
    /// Unicode has 143,859 characters - this expands beyond basic blocks!
    /// Different character sets create different textures and visual effects.
    /// </summary>
    public static class UnicodeLibrary
    {
        #region Block Elements (U+2580-U+259F) - Already covered in SymbolicChars
        // See SymbolicChars for full block element set
        #endregion

        #region Box Drawing (U+2500-U+257F) - Partially covered
        // See SymbolicChars for common box drawing characters
        #endregion

        #region Geometric Shapes (U+25A0-U+25FF)
        public const char BLACK_SQUARE = '■';              // U+25A0
        public const char WHITE_SQUARE = '□';              // U+25A1
        public const char SQUARE_WITH_ROUNDED_CORNERS = '▢'; // U+25A2
        public const char SQUARE_VERTICAL_FILL = '▥';      // U+25A5
        public const char SQUARE_HORIZONTAL_FILL = '▤';    // U+25A4
        public const char SQUARE_DIAGONAL_UL_TO_LR = '▨';  // U+25A8
        public const char BLACK_SMALL_SQUARE = '▪';        // U+25AA
        public const char WHITE_SMALL_SQUARE = '▫';        // U+25AB

        public const char BLACK_RECTANGLE = '▬';           // U+25AC
        public const char WHITE_RECTANGLE = '▭';           // U+25AD

        public const char BLACK_TRIANGLE_UP = '▲';         // U+25B2
        public const char WHITE_TRIANGLE_UP = '△';         // U+25B3
        public const char BLACK_TRIANGLE_RIGHT = '▶';      // U+25B6
        public const char WHITE_TRIANGLE_RIGHT = '▷';      // U+25B7
        public const char BLACK_TRIANGLE_DOWN = '▼';       // U+25BC
        public const char WHITE_TRIANGLE_DOWN = '▽';       // U+25BD
        public const char BLACK_TRIANGLE_LEFT = '◀';       // U+25C0
        public const char WHITE_TRIANGLE_LEFT = '◁';       // U+25C1

        public const char BLACK_CIRCLE = '●';              // U+25CF
        public const char WHITE_CIRCLE = '○';              // U+25CB
        public const char CIRCLE_DOTTED = '◌';             // U+25CC
        public const char CIRCLE_WITH_DOT = '◉';           // U+25C9
        public const char BULLSEYE = '◎';                  // U+25CE

        public const char BLACK_DIAMOND = '◆';             // U+25C6
        public const char WHITE_DIAMOND = '◇';             // U+25C7
        public const char LOZENGE = '◊';                   // U+25CA
        #endregion

        #region Symbols for Legacy Computing (U+1FB00-U+1FBFF)
        // Sextant patterns for 2×3 pixel grids within single character!
        // Allows for much higher resolution symbolic rendering
        // Note: These are multi-byte UTF-16 characters, use string literals instead
        // public const char SEXTANT_1 = '🬀';     // U+1FB00 - Cannot use char literal
        // public const char SEXTANT_2 = '🬁';     // U+1FB01
        // public const char SEXTANT_12 = '🬂';    // U+1FB02
        // public const char SEXTANT_3 = '🬃';     // U+1FB03
        // ... (Full sextant set: 64 combinations!)
        // Use string literals for these: "\U0001FB00", "\U0001FB01", etc.
        #endregion

        #region Braille Patterns (U+2800-U+28FF)
        // 2x4 dot matrix - 256 combinations!
        // Perfect for high-density textures
        public const char BRAILLE_BLANK = '⠀';            // U+2800
        public const char BRAILLE_DOTS_1 = '⠁';           // U+2801
        public const char BRAILLE_DOTS_12 = '⠃';          // U+2803
        public const char BRAILLE_DOTS_14 = '⠉';          // U+2809
        public const char BRAILLE_DOTS_145 = '⠙';         // U+2819
        public const char BRAILLE_DOTS_1245 = '⠛';        // U+281B
        public const char BRAILLE_FULL = '⣿';             // U+28FF
        // Use GetBrailleChar(bool[8]) for programmatic access
        #endregion

        #region Shade/Texture Characters
        // Light to dark progression (individual constants - UdonSharp doesn't support static readonly arrays)
        public const char SHADE_EMPTY = ' ';           // Empty
        public const char SHADE_LIGHT = '░';           // Light shade (U+2591)
        public const char SHADE_MEDIUM = '▒';          // Medium shade (U+2592)
        public const char SHADE_DARK = '▓';            // Dark shade (U+2593)
        public const char SHADE_FULL = '█';            // Full block (U+2588)

        // Dots for texture
        public const char DOT_EMPTY = ' ';             // Empty
        public const char DOT_MIDDLE = '·';            // Middle dot (U+00B7)
        public const char DOT_BULLET = '•';            // Bullet (U+2022)
        public const char DOT_CIRCLE = '●';            // Black circle (U+25CF)

        // Dithering patterns (same as shade gradient)
        public const char DITHER_0 = ' ';              // 0% fill
        public const char DITHER_25 = '░';             // 25% fill
        public const char DITHER_50 = '▒';             // 50% fill
        public const char DITHER_75 = '▓';             // 75% fill
        public const char DITHER_100 = '█';            // 100% fill
        #endregion

        #region Line Drawing (Various sets)
        // Thin lines
        public const char LIGHT_HORIZONTAL = '─';          // U+2500
        public const char LIGHT_VERTICAL = '│';            // U+2502
        public const char LIGHT_DOWN_RIGHT = '┌';          // U+250C
        public const char LIGHT_DOWN_LEFT = '┐';           // U+2510
        public const char LIGHT_UP_RIGHT = '└';            // U+2514
        public const char LIGHT_UP_LEFT = '┘';             // U+2518

        // Bold lines
        public const char HEAVY_HORIZONTAL = '━';          // U+2501
        public const char HEAVY_VERTICAL = '┃';            // U+2503
        public const char HEAVY_DOWN_RIGHT = '┏';          // U+250F
        public const char HEAVY_DOWN_LEFT = '┓';           // U+2513
        public const char HEAVY_UP_RIGHT = '┗';            // U+2517
        public const char HEAVY_UP_LEFT = '┛';             // U+251B

        // Curved lines (for organic shapes)
        public const char ARC_DOWN_RIGHT = '╭';            // U+256D
        public const char ARC_DOWN_LEFT = '╮';             // U+256E
        public const char ARC_UP_LEFT = '╯';               // U+256F
        public const char ARC_UP_RIGHT = '╰';              // U+2570
        #endregion

        #region Mathematical Symbols (U+2200-U+22FF)
        public const char PLUS_SIGN = '＋';                // U+FF0B (fullwidth)
        public const char MINUS_SIGN = '－';               // U+FF0D
        public const char MULTIPLICATION = '×';            // U+00D7
        public const char DIVISION = '÷';                  // U+00F7
        public const char EQUAL = '＝';                    // U+FF1D
        public const char NOT_EQUAL = '≠';                 // U+2260
        public const char APPROXIMATELY_EQUAL = '≈';       // U+2248
        public const char INFINITY = '∞';                  // U+221E
        #endregion

        #region Arrows (U+2190-U+21FF)
        public const char LEFTWARDS_ARROW = '←';           // U+2190
        public const char UPWARDS_ARROW = '↑';             // U+2191
        public const char RIGHTWARDS_ARROW = '→';          // U+2192
        public const char DOWNWARDS_ARROW = '↓';           // U+2193

        public const char LEFT_RIGHT_ARROW = '↔';          // U+2194
        public const char UP_DOWN_ARROW = '↕';             // U+2195

        public const char HEAVY_LEFTWARDS_ARROW = '⬅';    // U+2B05
        public const char HEAVY_RIGHTWARDS_ARROW = '➡';   // U+27A1
        public const char HEAVY_UPWARDS_ARROW = '⬆';      // U+2B06
        public const char HEAVY_DOWNWARDS_ARROW = '⬇';    // U+2B07
        #endregion

        #region Stars and Sparkles (U+2605-U+2606, U+2728-U+2734)
        public const char BLACK_STAR = '★';                // U+2605
        public const char WHITE_STAR = '☆';                // U+2606
        public const char SPARKLE = '✨';                   // U+2728
        public const char EIGHT_POINTED_STAR = '✳';        // U+2733
        public const char SNOWFLAKE = '❄';                 // U+2744
        #endregion

        #region Playing Card Suits (U+2660-U+2667)
        public const char BLACK_SPADE = '♠';               // U+2660
        public const char BLACK_CLUB = '♣';                // U+2663
        public const char BLACK_HEART = '♥';               // U+2665
        public const char BLACK_DIAMOND_SUIT = '♦';        // U+2666 (Playing card suit)
        public const char WHITE_SPADE = '♤';               // U+2664
        public const char WHITE_CLUB = '♧';                // U+2667
        public const char WHITE_HEART = '♡';               // U+2661
        public const char WHITE_DIAMOND_SUIT = '♢';        // U+2662 (Playing card suit)
        #endregion

        #region Music Symbols (U+2669-U+266F)
        public const char QUARTER_NOTE = '♩';              // U+2669
        public const char EIGHTH_NOTE = '♪';               // U+266A
        public const char BEAMED_EIGHTH_NOTES = '♫';       // U+266B
        public const char SIXTEENTH_NOTE = '♬';            // U+266C
        public const char FLAT_SIGN = '♭';                 // U+266D
        public const char NATURAL_SIGN = '♮';              // U+266E
        public const char SHARP_SIGN = '♯';                // U+266F
        #endregion

        #region Miscellaneous Symbols (U+2600-U+26FF)
        public const char SUN = '☀';                       // U+2600
        public const char CLOUD = '☁';                     // U+2601
        public const char UMBRELLA = '☂';                  // U+2602
        public const char SNOWMAN = '☃';                   // U+2603
        public const char LIGHTNING = '☇';                 // U+2607
        public const char SKULL_CROSSBONES = '☠';          // U+2620
        public const char RADIOACTIVE = '☢';               // U+2622
        public const char BIOHAZARD = '☣';                 // U+2623
        public const char PEACE = '☮';                     // U+262E
        public const char YIN_YANG = '☯';                  // U+262F
        public const char WARNING_SIGN = '⚠';              // U+26A0
        public const char HIGH_VOLTAGE = '⚡';              // U+26A1
        public const char GEAR = '⚙';                      // U+2699
        public const char ATOM = '⚛';                      // U+269B
        #endregion

        #region Emoji (Basic examples - thousands more available!)
        // Note: Some emojis may render as 2 characters wide
        // Note: Multi-byte UTF-16 characters cannot use char literals in C#
        // Use string literals instead: "\U0001F525", "\U0001F30A", etc.
        // public const char FIRE = '🔥';                     // U+1F525 - Cannot use char literal
        // public const char WATER_WAVE = '🌊';               // U+1F30A
        // public const char TREE = '🌳';                     // U+1F333
        // public const char MOUNTAIN = '⛰';                  // U+26F0 - This one might work (BMP)
        // public const char ROCKET = '🚀';                   // U+1F680
        // public const char SATELLITE = '🛰';                // U+1F6F0
        // public const char ALIEN = '👽';                    // U+1F47D
        // public const char ROBOT = '🤖';                    // U+1F916
        #endregion

        #region Texture Sets
        /// <summary>
        /// Get texture character for organic surfaces (grass, sand, etc)
        /// </summary>
        public static char GetOrganicTexture(float density, int variation)
        {
            char[] organicChars = new char[] { ' ', '·', ':', '∴', '░', '▒', '▓', '█' };
            int index = Mathf.Clamp((int)(density * organicChars.Length), 0, organicChars.Length - 1);
            return organicChars[index];
        }

        /// <summary>
        /// Get metallic/technical texture character
        /// </summary>
        public static char GetMetallicTexture(float density, int variation)
        {
            char[] metallicChars = new char[] { ' ', '░', '▒', '▓', '▪', '▬', '■', '█' };
            int index = Mathf.Clamp((int)(density * metallicChars.Length), 0, metallicChars.Length - 1);
            return metallicChars[index];
        }

        /// <summary>
        /// Get water/liquid texture with wave patterns
        /// </summary>
        public static char GetWaterTexture(float density, int variation)
        {
            char[] waterChars = new char[] { ' ', '░', '≈', '∽', '≋', '▒', '▓', '█' };
            int index = Mathf.Clamp((int)(density * waterChars.Length), 0, waterChars.Length - 1);
            return waterChars[index];
        }

        /// <summary>
        /// Get cloudy/foggy texture
        /// </summary>
        public static char GetCloudTexture(float density, int variation)
        {
            char[] cloudChars = new char[] { ' ', '░', '▒', '▓', '☁', '█' };
            int index = Mathf.Clamp((int)(density * cloudChars.Length), 0, cloudChars.Length - 1);
            return cloudChars[index];
        }

        /// <summary>
        /// Get fire/plasma texture
        /// </summary>
        public static char GetFireTexture(float intensity, int variation)
        {
            // Note: Removed emoji (multi-byte char), use full block instead for max intensity
            char[] fireChars = new char[] { ' ', '·', '∴', '░', '▒', '▓', '█' };
            int index = Mathf.Clamp((int)(intensity * fireChars.Length), 0, fireChars.Length - 1);
            return fireChars[index];
        }
        #endregion

        #region Utility Methods

        /// <summary>
        /// Get Braille character from 8-dot pattern (2×4 grid)
        /// Allows for high-density pixel-perfect textures
        /// </summary>
        /// <param name="dots">8 boolean values for dots 1-8</param>
        public static char GetBrailleChar(bool[] dots)
        {
            if (dots.Length != 8) return BRAILLE_BLANK;

            int value = 0;
            int[] dotValues = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };

            for (int i = 0; i < 8; i++)
            {
                if (dots[i])
                {
                    value |= dotValues[i];
                }
            }

            return (char)(0x2800 + value);
        }

        /// <summary>
        /// Gets shade character based on brightness (0-1)
        /// </summary>
        public static char GetShadeCharacter(float brightness)
        {
            if (brightness >= 1.0f) return SHADE_FULL;
            if (brightness >= 0.75f) return SHADE_DARK;
            if (brightness >= 0.50f) return SHADE_MEDIUM;
            if (brightness >= 0.25f) return SHADE_LIGHT;
            return SHADE_EMPTY;
        }

        /// <summary>
        /// Gets appropriate character for surface normal (for 3D-like shading)
        /// </summary>
        public static char GetShadingCharacter(Vector3 normal, Vector3 lightDir)
        {
            float dot = Vector3.Dot(normal, lightDir);
            return GetShadeCharacter((dot + 1f) * 0.5f); // Remap -1..1 to 0..1
        }

        #endregion

        #region Font Atlas Recommendations

        /// <summary>
        /// Returns Unicode ranges to include in TMP Font Atlas for comprehensive support
        /// </summary>
        public static string[] GetRecommendedFontRanges()
        {
            return new string[]
            {
                "0020-007E",  // Basic Latin
                "00A0-00FF",  // Latin-1 Supplement
                "2500-257F",  // Box Drawing
                "2580-259F",  // Block Elements
                "25A0-25FF",  // Geometric Shapes
                "2600-26FF",  // Miscellaneous Symbols
                "2190-21FF",  // Arrows
                "2200-22FF",  // Mathematical Operators
                "2300-23FF",  // Miscellaneous Technical
                "2460-24FF",  // Enclosed Alphanumerics
                "2800-28FF",  // Braille Patterns
                "1FB00-1FBFF" // Symbols for Legacy Computing (sextants)
            };
        }

        #endregion
    }
}
