using UdonSharp;
using TMPro;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace SymbolicGraphics.Examples
{
    /// <summary>
    /// Arkanoid/Breakout game using symbolic rendering (unicode blocks)
    /// Network synced - all players see the same game state
    /// Controls: Click to start, A/D to move paddle, Space to exit
    /// </summary>
    public class SymbolicBreakout : UdonSharpBehaviour
    {
        [Header("Display")]
        public TextMeshProUGUI gameDisplay;

        [Header("Game Settings")]
        public int gameWidth = 40;
        public int gameHeight = 25;
        public float ballSpeed = 8f;
        public float paddleSpeed = 15f;

        [Header("Audio (Optional)")]
        public AudioSource audioSource;
        public AudioClip paddleHitSound;
        public AudioClip brickHitSound;
        public AudioClip wallHitSound;

        // =================================================================
        // NETWORK SYNCED VARIABLES
        // =================================================================
        [UdonSynced] private bool gameActive = false;
        [UdonSynced] private int paddleX = 20;
        [UdonSynced] private int ballX_synced = 200; // *10 for precision
        [UdonSynced] private int ballY_synced = 150; // *10 for precision
        [UdonSynced] private float ballVelX = 1f;
        [UdonSynced] private float ballVelY = -1f;
        [UdonSynced] private int score = 0;
        [UdonSynced] private string brickData = ""; // Encoded brick state

        // Local float positions for smooth physics
        private float ballX = 20f;
        private float ballY = 15f;

        // High scores (network synced)
        [UdonSynced] private int highScore1 = 0;
        [UdonSynced] private string highScoreName1 = "";
        [UdonSynced] private int highScore2 = 0;
        [UdonSynced] private string highScoreName2 = "";
        [UdonSynced] private int highScore3 = 0;
        [UdonSynced] private string highScoreName3 = "";

        // =================================================================
        // LOCAL VARIABLES
        // =================================================================
        private VRCPlayerApi localPlayer;
        private bool playerLocked = false;
        private float lastInputTime = 0f;
        private const float inputCooldown = 0.05f;

        private bool[] bricks; // 1D array instead of 2D (UdonSharp limitation)
        private const int BRICK_ROWS = 5;
        private const int BRICK_COLS = 10;
        private const int BRICK_START_Y = 2;

        private const int PADDLE_WIDTH = 6;
        private const int PADDLE_Y = 22;

        // Continuous paddle movement
        private float currentHorizontalInput = 0f;

        // Platform detection
        private bool isOnQuest = false;

        private void Start()
        {
            if (gameDisplay == null)
            {
                Debug.LogError("[SymbolicBreakout] gameDisplay is null!");
                return;
            }

            localPlayer = Networking.LocalPlayer;

            #if UNITY_ANDROID
            isOnQuest = true;
            #endif

            InitializeBricks();
            RenderGame();
        }

        private void Update()
        {
            if (!gameActive) return;
            if (!Networking.IsOwner(gameObject)) return;

            float deltaTime = Time.deltaTime;

            // Update paddle position continuously based on input
            if (currentHorizontalInput != 0f)
            {
                float paddleMove = currentHorizontalInput * paddleSpeed * deltaTime;
                float newPaddleX = paddleX + paddleMove;

                // Clamp to game bounds
                if (newPaddleX < 0) newPaddleX = 0;
                if (newPaddleX > gameWidth - PADDLE_WIDTH) newPaddleX = gameWidth - PADDLE_WIDTH;

                paddleX = Mathf.RoundToInt(newPaddleX);
            }

            // Update ball physics
            ballX += ballVelX * ballSpeed * deltaTime;
            ballY += ballVelY * ballSpeed * deltaTime;

            // Sync to network (with precision)
            ballX_synced = Mathf.RoundToInt(ballX * 10f);
            ballY_synced = Mathf.RoundToInt(ballY * 10f);

            // Wall collision (left/right)
            if (ballX < 1)
            {
                ballX = 1;
                ballVelX = Mathf.Abs(ballVelX);
                PlaySound(wallHitSound);
            }
            if (ballX > gameWidth - 2)
            {
                ballX = gameWidth - 2;
                ballVelX = -Mathf.Abs(ballVelX);
                PlaySound(wallHitSound);
            }

            // Ceiling collision
            if (ballY < 1)
            {
                ballY = 1;
                ballVelY = Mathf.Abs(ballVelY);
                PlaySound(wallHitSound);
            }

            // Paddle collision - check range around paddle Y
            int ballIntX = Mathf.RoundToInt(ballX);
            int ballIntY = Mathf.RoundToInt(ballY);

            if (ballIntY >= PADDLE_Y - 1 && ballIntY <= PADDLE_Y + 1 &&
                ballIntX >= paddleX && ballIntX < paddleX + PADDLE_WIDTH &&
                ballVelY > 0) // Only bounce if moving downward
            {
                ballY = PADDLE_Y - 1; // Position above paddle
                ballVelY = -Mathf.Abs(ballVelY);

                // Add horizontal velocity based on hit position
                float hitPos = (ballIntX - paddleX) / (float)PADDLE_WIDTH; // 0 to 1
                ballVelX = (hitPos - 0.5f) * 2f; // -1 to 1

                PlaySound(paddleHitSound);
            }

            // Brick collision
            CheckBrickCollision(ballIntX, ballIntY);

            // Ball fell off bottom - Game Over
            if (ballY > gameHeight + 2)
            {
                GameOver();
            }

            RequestSerialization();
        }

        private void FixedUpdate()
        {
            if (gameActive)
            {
                RenderGame();
            }
        }

        // =================================================================
        // VRCHAT INTERACTION
        // =================================================================

        public override void Interact()
        {
            if (localPlayer == null) return;

            // Start game and lock player
            if (!gameActive && !playerLocked)
            {
                TakeOwnership();
                StartGame();

                localPlayer.Immobilize(true);
                playerLocked = true;
                Debug.Log("[SymbolicBreakout] Player locked - game started");
            }
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            if (!playerLocked) return;
            if (!Networking.IsOwner(gameObject)) return;

            // Store continuous input value (-1 to 1)
            currentHorizontalInput = value;
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (!value) return; // Only on key down
            if (!playerLocked) return;
            if (Time.time - lastInputTime < inputCooldown) return;

            // Space exits and unlocks player
            ExitGame();
            lastInputTime = Time.time;
        }

        // =================================================================
        // GAME LOGIC
        // =================================================================

        private void TakeOwnership()
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(localPlayer, gameObject);
            }
        }

        private void StartGame()
        {
            InitializeBricks();
            paddleX = gameWidth / 2 - PADDLE_WIDTH / 2;
            ballX = gameWidth / 2;
            ballY = PADDLE_Y - 2;
            ballVelX = 1f;
            ballVelY = -1f;
            score = 0;
            gameActive = true;
            EncodeBricks();
            RequestSerialization();

            Debug.Log("[SymbolicBreakout] Game started!");
        }

        private void ExitGame()
        {
            if (localPlayer != null && playerLocked)
            {
                localPlayer.Immobilize(false);
                playerLocked = false;
                Debug.Log("[SymbolicBreakout] Player unlocked");
            }

            if (Networking.IsOwner(gameObject))
            {
                gameActive = false;
                RequestSerialization();
            }

            RenderGame();
        }

        private void GameOver()
        {
            gameActive = false;
            Debug.Log("[SymbolicBreakout] Game Over! Score: " + score);

            // Update high scores
            UpdateHighScores();

            RequestSerialization();
        }

        private void UpdateHighScores()
        {
            if (localPlayer == null) return;
            string playerName = localPlayer.displayName;

            // Check if current score beats any high score
            if (score > highScore1)
            {
                // Shift scores down
                highScore3 = highScore2;
                highScoreName3 = highScoreName2;
                highScore2 = highScore1;
                highScoreName2 = highScoreName1;
                highScore1 = score;
                highScoreName1 = playerName;
            }
            else if (score > highScore2)
            {
                highScore3 = highScore2;
                highScoreName3 = highScoreName2;
                highScore2 = score;
                highScoreName2 = playerName;
            }
            else if (score > highScore3)
            {
                highScore3 = score;
                highScoreName3 = playerName;
            }
        }

        private void InitializeBricks()
        {
            bricks = new bool[BRICK_ROWS * BRICK_COLS];
            for (int i = 0; i < bricks.Length; i++)
            {
                bricks[i] = true;
            }
        }

        private int GetBrickIndex(int row, int col)
        {
            return row * BRICK_COLS + col;
        }

        private void CheckBrickCollision(int ballIntX, int ballIntY)
        {
            // Calculate brick position
            int brickWidth = gameWidth / BRICK_COLS;
            int row = (ballIntY - BRICK_START_Y) / 2;
            int col = ballIntX / brickWidth;

            if (row >= 0 && row < BRICK_ROWS && col >= 0 && col < BRICK_COLS)
            {
                int index = GetBrickIndex(row, col);
                if (bricks[index])
                {
                    bricks[index] = false;
                    score += 10;
                    ballVelY = -ballVelY;
                    PlaySound(brickHitSound);
                    EncodeBricks();

                    // Check win condition
                    if (CheckAllBricksDestroyed())
                    {
                        GameOver();
                    }
                }
            }
        }

        private bool CheckAllBricksDestroyed()
        {
            for (int i = 0; i < bricks.Length; i++)
            {
                if (bricks[i]) return false;
            }
            return true;
        }

        // =================================================================
        // NETWORK SYNC
        // =================================================================

        private void EncodeBricks()
        {
            // Encode brick state as string (50 chars, 1 per brick)
            string encoded = "";
            for (int i = 0; i < bricks.Length; i++)
            {
                encoded += bricks[i] ? "1" : "0";
            }
            brickData = encoded;
        }

        private void DecodeBricks()
        {
            if (brickData.Length != BRICK_ROWS * BRICK_COLS) return;

            for (int i = 0; i < bricks.Length; i++)
            {
                bricks[i] = (brickData[i] == '1');
            }
        }

        public override void OnDeserialization()
        {
            // Decode ball position from synced integers
            ballX = ballX_synced / 10f;
            ballY = ballY_synced / 10f;

            DecodeBricks();
            RenderGame();
        }

        // =================================================================
        // RENDERING
        // =================================================================

        private void RenderGame()
        {
            if (gameDisplay == null) return;

            string output = "";

            if (!gameActive)
            {
                output += "<color=#00FFFF>SYMBOLIC BREAKOUT</color>\n\n";

                if (score > 0)
                {
                    output += "<color=#FF0000>GAME OVER</color>\n";
                    output += "<color=#FFFF00>Score: " + score + "</color>\n\n";
                    string exitButton = isOnQuest ? "Jump Button" : "SPACE BAR";
                    output += exitButton + " to Exit\n\n";
                }
                else
                {
                    output += "Click screen to play\n";
                    if (isOnQuest)
                    {
                        output += "Joystick (left/right) to move paddle\n";
                        output += "Jump Button to exit\n\n";
                    }
                    else
                    {
                        output += "A/D keys or Joystick to move paddle\n";
                        output += "Space to exit\n\n";
                    }
                }

                // High Scores
                output += CenterText("<color=#FFFF00>HIGH SCORES</color>", gameWidth) + "\n";
                output += BuildHighScoreLine(1, highScore1, highScoreName1);
                output += BuildHighScoreLine(2, highScore2, highScoreName2);
                output += BuildHighScoreLine(3, highScore3, highScoreName3);

                gameDisplay.text = output;
                return;
            }

            // Top border
            output += "<color=#00AAFF>";
            for (int i = 0; i < gameWidth; i++) output += "═";
            output += "</color>\n";

            // Game area
            for (int y = 0; y < gameHeight; y++)
            {
                for (int x = 0; x < gameWidth; x++)
                {
                    // Bricks
                    int brickWidth = gameWidth / BRICK_COLS;
                    int brickRow = (y - BRICK_START_Y) / 2;
                    int brickCol = x / brickWidth;

                    bool isBrick = false;
                    if (brickRow >= 0 && brickRow < BRICK_ROWS && brickCol >= 0 && brickCol < BRICK_COLS)
                    {
                        if ((y - BRICK_START_Y) % 2 == 0)
                        {
                            int brickIndex = GetBrickIndex(brickRow, brickCol);
                            isBrick = bricks[brickIndex];
                        }
                    }

                    // Ball
                    int ballIntX = Mathf.RoundToInt(ballX);
                    int ballIntY = Mathf.RoundToInt(ballY);
                    bool isBall = (x == ballIntX && y == ballIntY);

                    // Paddle
                    bool isPaddle = (y == PADDLE_Y && x >= paddleX && x < paddleX + PADDLE_WIDTH);

                    if (isBall)
                    {
                        output += "<color=#FFFFFF>●</color>";
                    }
                    else if (isPaddle)
                    {
                        output += "<color=#00FF00>█</color>";
                    }
                    else if (isBrick)
                    {
                        // Color bricks by row
                        string brickColor = GetBrickColor(brickRow);
                        output += "<color=" + brickColor + ">█</color>";
                    }
                    else
                    {
                        output += " ";
                    }
                }
                output += "\n";
            }

            // Bottom border
            output += "<color=#00AAFF>";
            for (int i = 0; i < gameWidth; i++) output += "═";
            output += "</color>\n";

            // Score
            output += "<color=#FFFF00>SCORE: " + score + "</color>";

            gameDisplay.text = output;
        }

        private string GetBrickColor(int row)
        {
            if (row == 0) return "#FF0000"; // Red
            if (row == 1) return "#FF8800"; // Orange
            if (row == 2) return "#FFFF00"; // Yellow
            if (row == 3) return "#00FF00"; // Green
            return "#00FFFF"; // Cyan
        }

        private string BuildHighScoreLine(int rank, int scoreValue, string playerName)
        {
            if (scoreValue == 0) return "";

            string line = "";
            string rankStr = rank + ".";
            string scoreStr = "Score " + scoreValue.ToString("D4");

            // Left side: rank and score
            line += "<color=#FFAA00>" + rankStr + " " + scoreStr + "</color>";

            // Right side: player name (padded to align right)
            int padding = gameWidth - rankStr.Length - scoreStr.Length - playerName.Length - 2;
            for (int i = 0; i < padding; i++)
            {
                line += " ";
            }
            line += "<color=#00FF00>" + playerName + "</color>\n";

            return line;
        }

        private string CenterText(string text, int width)
        {
            // Strip rich text tags to get actual text length
            string plainText = StripRichTextTags(text);
            int textLength = plainText.Length;

            if (textLength >= width) return text;

            int padding = (width - textLength) / 2;
            string line = "";
            for (int i = 0; i < padding; i++)
            {
                line += " ";
            }
            line += text;
            return line;
        }

        private string StripRichTextTags(string text)
        {
            string result = "";
            bool inTag = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '<')
                {
                    inTag = true;
                }
                else if (c == '>')
                {
                    inTag = false;
                }
                else if (!inTag)
                {
                    result += c;
                }
            }

            return result;
        }

        // =================================================================
        // AUDIO
        // =================================================================

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
