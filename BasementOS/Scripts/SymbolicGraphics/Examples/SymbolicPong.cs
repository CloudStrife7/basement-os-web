using UdonSharp;
using TMPro;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace SymbolicGraphics.Examples
{
    /// <summary>
    /// 2-Player Pong game using symbolic rendering (unicode blocks)
    /// Network synced - all players see the same game state
    /// Controls: Player 1 (W/S), Player 2 (Up/Down arrows), Space to exit
    /// </summary>
    public class SymbolicPong : UdonSharpBehaviour
    {
        [Header("Display")]
        public TextMeshProUGUI gameDisplay;

        [Header("Game Settings")]
        public int gameWidth = 60;
        public int gameHeight = 30;
        public float ballSpeed = 12f;
        public float paddleSpeed = 20f;
        public int winningScore = 5;

        [Header("Audio (Optional)")]
        public AudioSource audioSource;
        public AudioClip paddleHitSound;
        public AudioClip scoreSound;
        public AudioClip wallHitSound;

        // =================================================================
        // NETWORK SYNCED VARIABLES
        // =================================================================
        [UdonSynced] private bool gameActive = false;
        [UdonSynced] private int paddle1Y = 15; // Left paddle
        [UdonSynced] private int paddle2Y = 15; // Right paddle
        [UdonSynced] private int ballX_synced = 300; // *10 for precision
        [UdonSynced] private int ballY_synced = 150; // *10 for precision
        [UdonSynced] private float ballVelX = 1f;
        [UdonSynced] private float ballVelY = 0f;
        [UdonSynced] private int player1Score = 0;
        [UdonSynced] private int player2Score = 0;
        [UdonSynced] private string player1Name = "";
        [UdonSynced] private string player2Name = "";
        [UdonSynced] private bool player1Ready = false;
        [UdonSynced] private bool player2Ready = false;

        // Local float positions for smooth physics
        private float ballX = 30f;
        private float ballY = 15f;

        // =================================================================
        // LOCAL VARIABLES
        // =================================================================
        private VRCPlayerApi localPlayer;
        private bool isPlayer1 = false;
        private bool isPlayer2 = false;
        private bool playerLocked = false;
        private float lastInputTime = 0f;
        private const float inputCooldown = 0.05f;

        private const int PADDLE_HEIGHT = 5;
        private const int PADDLE1_X = 2;
        private int PADDLE2_X; // Calculated in Start

        // Continuous paddle movement
        private float currentVerticalInput = 0f;

        // Platform detection
        private bool isOnQuest = false;

        private void Start()
        {
            if (gameDisplay == null)
            {
                Debug.LogError("[SymbolicPong] gameDisplay is null!");
                return;
            }

            localPlayer = Networking.LocalPlayer;

            #if UNITY_ANDROID
            isOnQuest = true;
            #endif

            PADDLE2_X = gameWidth - 3;

            RenderGame();
        }

        private void Update()
        {
            if (!gameActive) return;

            float deltaTime = Time.deltaTime;

            // Keyboard input for paddle movement (desktop mode) - BEFORE ownership check
            if (playerLocked)
            {
                if (isPlayer1)
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        currentVerticalInput = -1f; // Negative is up
                    }
                    else if (Input.GetKey(KeyCode.S))
                    {
                        currentVerticalInput = 1f; // Positive is down
                    }
                    else
                    {
                        currentVerticalInput = 0f;
                    }
                }
                else if (isPlayer2)
                {
                    if (Input.GetKey(KeyCode.UpArrow))
                    {
                        currentVerticalInput = -1f;
                    }
                    else if (Input.GetKey(KeyCode.DownArrow))
                    {
                        currentVerticalInput = 1f;
                    }
                    else
                    {
                        currentVerticalInput = 0f;
                    }
                }
            }

            // Ball physics (owner only)
            if (!Networking.IsOwner(gameObject)) return;

            // Update ball physics
            ballX += ballVelX * ballSpeed * deltaTime;
            ballY += ballVelY * ballSpeed * deltaTime;

            // Sync to network (with precision)
            ballX_synced = Mathf.RoundToInt(ballX * 10f);
            ballY_synced = Mathf.RoundToInt(ballY * 10f);

            // Top/bottom wall collision
            if (ballY < 1)
            {
                ballY = 1;
                ballVelY = Mathf.Abs(ballVelY);
                PlaySound(wallHitSound);
            }
            if (ballY > gameHeight - 2)
            {
                ballY = gameHeight - 2;
                ballVelY = -Mathf.Abs(ballVelY);
                PlaySound(wallHitSound);
            }

            // Paddle collision
            int ballIntX = Mathf.RoundToInt(ballX);
            int ballIntY = Mathf.RoundToInt(ballY);

            // Player 1 paddle (left)
            if (ballIntX <= PADDLE1_X + 1 && ballIntX >= PADDLE1_X &&
                ballIntY >= paddle1Y && ballIntY < paddle1Y + PADDLE_HEIGHT &&
                ballVelX < 0)
            {
                ballX = PADDLE1_X + 2;
                ballVelX = Mathf.Abs(ballVelX);

                // Angle based on hit position
                float hitPos = (ballIntY - paddle1Y) / (float)PADDLE_HEIGHT;
                ballVelY = (hitPos - 0.5f) * 2f;

                PlaySound(paddleHitSound);
            }

            // Player 2 paddle (right)
            if (ballIntX >= PADDLE2_X - 1 && ballIntX <= PADDLE2_X &&
                ballIntY >= paddle2Y && ballIntY < paddle2Y + PADDLE_HEIGHT &&
                ballVelX > 0)
            {
                ballX = PADDLE2_X - 2;
                ballVelX = -Mathf.Abs(ballVelX);

                // Angle based on hit position
                float hitPos = (ballIntY - paddle2Y) / (float)PADDLE_HEIGHT;
                ballVelY = (hitPos - 0.5f) * 2f;

                PlaySound(paddleHitSound);
            }

            // Score points
            if (ballX < 0)
            {
                // Player 2 scores
                player2Score++;
                PlaySound(scoreSound);
                ResetBall();
                CheckWinCondition();
            }
            else if (ballX > gameWidth)
            {
                // Player 1 scores
                player1Score++;
                PlaySound(scoreSound);
                ResetBall();
                CheckWinCondition();
            }

            RequestSerialization();
        }

        private void FixedUpdate()
        {
            // Update paddle positions for assigned player
            if (gameActive && playerLocked)
            {
                if (isPlayer1)
                {
                    if (!Networking.IsOwner(gameObject))
                    {
                        Networking.SetOwner(localPlayer, gameObject);
                    }

                    if (currentVerticalInput != 0f)
                    {
                        float paddleMove = currentVerticalInput * paddleSpeed * Time.fixedDeltaTime;
                        float newPaddleY = paddle1Y + paddleMove;

                        if (newPaddleY < 0) newPaddleY = 0;
                        if (newPaddleY > gameHeight - PADDLE_HEIGHT) newPaddleY = gameHeight - PADDLE_HEIGHT;

                        paddle1Y = Mathf.RoundToInt(newPaddleY);
                        RequestSerialization();
                    }
                }
                else if (isPlayer2)
                {
                    if (!Networking.IsOwner(gameObject))
                    {
                        Networking.SetOwner(localPlayer, gameObject);
                    }

                    if (currentVerticalInput != 0f)
                    {
                        float paddleMove = currentVerticalInput * paddleSpeed * Time.fixedDeltaTime;
                        float newPaddleY = paddle2Y + paddleMove;

                        if (newPaddleY < 0) newPaddleY = 0;
                        if (newPaddleY > gameHeight - PADDLE_HEIGHT) newPaddleY = gameHeight - PADDLE_HEIGHT;

                        paddle2Y = Mathf.RoundToInt(newPaddleY);
                        RequestSerialization();
                    }
                }
            }

            // AI paddle for solo mode (follows ball)
            if (gameActive && player2Name == "WALL" && Networking.IsOwner(gameObject))
            {
                float targetY = ballY - (PADDLE_HEIGHT / 2);
                float aiSpeed = paddleSpeed * 0.7f;
                float aiMove = Mathf.Clamp(targetY - paddle2Y, -aiSpeed * Time.fixedDeltaTime, aiSpeed * Time.fixedDeltaTime);

                paddle2Y = Mathf.RoundToInt(Mathf.Clamp(paddle2Y + aiMove, 0, gameHeight - PADDLE_HEIGHT));
            }

            if (gameActive || player1Ready || player2Ready)
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

            // Join as player 1 or 2
            if (!player1Ready && !player2Ready)
            {
                // First player becomes player 1
                TakeOwnership();
                player1Name = localPlayer.displayName;
                player1Ready = true;
                isPlayer1 = true;

                localPlayer.Immobilize(true);
                playerLocked = true;
                RequestSerialization();
                Debug.Log("[SymbolicPong] Joined as Player 1");
            }
            else if (player1Ready && !player2Ready && !isPlayer1)
            {
                // Second player becomes player 2
                player2Name = localPlayer.displayName;
                player2Ready = true;
                isPlayer2 = true;

                localPlayer.Immobilize(true);
                playerLocked = true;
                RequestSerialization();

                // Start game when both players ready
                SendCustomEventDelayedSeconds("StartGame", 1f);
                Debug.Log("[SymbolicPong] Joined as Player 2 - Game starting!");
            }
            else if (player1Ready && !player2Ready && isPlayer1)
            {
                // Player 1 clicks again to play solo (against wall)
                player2Name = "WALL";
                player2Ready = true;
                RequestSerialization();

                SendCustomEventDelayedSeconds("StartGame", 1f);
                Debug.Log("[SymbolicPong] Solo mode - Playing against wall!");
            }
        }

        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            if (!playerLocked) return;
            currentVerticalInput = value;
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (!value) return;
            if (!playerLocked) return;
            if (Time.time - lastInputTime < inputCooldown) return;

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

        public void StartGame()
        {
            if (!player1Ready || !player2Ready) return;

            paddle1Y = gameHeight / 2 - PADDLE_HEIGHT / 2;
            paddle2Y = gameHeight / 2 - PADDLE_HEIGHT / 2;
            player1Score = 0;
            player2Score = 0;
            ResetBall();
            gameActive = true;
            RequestSerialization();

            Debug.Log("[SymbolicPong] Game started!");
        }

        private void ResetBall()
        {
            ballX = gameWidth / 2;
            ballY = gameHeight / 2;

            // Random direction
            ballVelX = (Random.value > 0.5f) ? 1f : -1f;
            ballVelY = (Random.value - 0.5f) * 0.5f;
        }

        private void CheckWinCondition()
        {
            if (player1Score >= winningScore || player2Score >= winningScore)
            {
                GameOver();
            }
        }

        private void ExitGame()
        {
            if (localPlayer != null && playerLocked)
            {
                localPlayer.Immobilize(false);
                playerLocked = false;
                Debug.Log("[SymbolicPong] Player unlocked");
            }

            if (isPlayer1)
            {
                player1Ready = false;
                player1Name = "";
                isPlayer1 = false;
            }
            else if (isPlayer2)
            {
                player2Ready = false;
                player2Name = "";
                isPlayer2 = false;
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
            Debug.Log("[SymbolicPong] Game Over!");
            RequestSerialization();
        }

        // =================================================================
        // NETWORK SYNC
        // =================================================================

        public override void OnDeserialization()
        {
            ballX = ballX_synced / 10f;
            ballY = ballY_synced / 10f;
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
                output += "<color=#B967FF>SYMBOLIC PONG</color>\n\n"; // Synthwave purple

                if (player1Score > 0 || player2Score > 0)
                {
                    // Game over screen
                    output += "<color=#FF0000>GAME OVER</color>\n\n";
                    if (player1Score >= winningScore)
                    {
                        output += "<color=#FFFF00>" + player1Name + " WINS!</color>\n";
                    }
                    else
                    {
                        output += "<color=#FFFF00>" + player2Name + " WINS!</color>\n";
                    }
                    output += "\nFinal Score:\n";
                    output += player1Name + ": " + player1Score + "\n";
                    output += player2Name + ": " + player2Score + "\n\n";

                    string exitButton = isOnQuest ? "Jump Button" : "SPACE BAR";
                    output += exitButton + " to Exit\n";
                }
                else
                {
                    // Waiting for players
                    output += "Click to join!\n\n";

                    if (player1Ready)
                    {
                        output += "<color=#00FF00>Player 1: " + player1Name + " READY</color>\n";
                        output += "\n<color=#FFFF00>Click again to play solo</color>\n";
                    }
                    else
                    {
                        output += "Player 1: Waiting...\n";
                    }

                    if (player2Ready)
                    {
                        output += "<color=#00FF00>Player 2: " + player2Name + " READY</color>\n";
                    }
                    else
                    {
                        output += "Player 2: Waiting...\n";
                    }

                    output += "\nControls:\n";
                    if (isOnQuest)
                    {
                        output += "Player 1: Left Joystick (up/down)\n";
                        output += "Player 2: Right Joystick (up/down)\n";
                        output += "Jump Button to exit\n";
                    }
                    else
                    {
                        output += "Player 1: W/S keys or Joystick\n";
                        output += "Player 2: Up/Down arrows or Joystick\n";
                        output += "Space to exit\n";
                    }
                }

                gameDisplay.text = output;
                return;
            }

            // Game active - render playing field

            // Top border (synthwave pink)
            output += "<color=#FF006E>";
            for (int i = 0; i < gameWidth; i++) output += "═";
            output += "</color>\n";

            // Game area
            for (int y = 0; y < gameHeight; y++)
            {
                for (int x = 0; x < gameWidth; x++)
                {
                    int ballIntX = Mathf.RoundToInt(ballX);
                    int ballIntY = Mathf.RoundToInt(ballY);

                    bool isBall = (x == ballIntX && y == ballIntY);
                    bool isPaddle1 = (x == PADDLE1_X && y >= paddle1Y && y < paddle1Y + PADDLE_HEIGHT);
                    bool isPaddle2 = (x == PADDLE2_X && y >= paddle2Y && y < paddle2Y + PADDLE_HEIGHT);
                    bool isCenterLine = (x == gameWidth / 2 && y % 2 == 0);

                    if (isBall)
                    {
                        output += "<color=#FFFFFF>●</color>";
                    }
                    else if (isPaddle1)
                    {
                        output += "<color=#00F5FF>█</color>"; // Synthwave cyan
                    }
                    else if (isPaddle2)
                    {
                        output += "<color=#FF006E>█</color>"; // Synthwave pink
                    }
                    else if (isCenterLine)
                    {
                        output += "<color=#B967FF>│</color>"; // Synthwave purple
                    }
                    else
                    {
                        output += " ";
                    }
                }
                output += "\n";
            }

            // Bottom border (synthwave pink)
            output += "<color=#FF006E>";
            for (int i = 0; i < gameWidth; i++) output += "═";
            output += "</color>\n";

            // Scores (synthwave colors)
            string scoreText = "<color=#00F5FF>" + player1Name + ": " + player1Score + "</color>    <color=#FF006E>" + player2Name + ": " + player2Score + "</color>";
            output += scoreText;

            gameDisplay.text = output;
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
