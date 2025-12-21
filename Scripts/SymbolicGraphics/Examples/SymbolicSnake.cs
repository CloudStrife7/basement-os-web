using UdonSharp;
using TMPro;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace SymbolicGraphics.Examples
{
    /// <summary>
    /// Nokia 3310-style Snake game with Tournament Mode
    /// Solo play with high scores OR bracket-style tournament (up to 6 players)
    /// Controls: WASD or Quest joystick, Space to exit
    /// </summary>
    public class SymbolicSnake : UdonSharpBehaviour
    {
        [Header("Display")]
        public TextMeshProUGUI gameDisplay;

        [Header("Game Settings")]
        public int gameWidth = 50;
        public int gameHeight = 30;
        public int defaultSnakeLength = 3;
        public float defaultSpeed = 0.15f;
        public float speedIncreasePerFruit = 0.01f;
        public float bonusItemDuration = 5f;

        [Header("Audio (Optional)")]
        public AudioSource audioSource;
        public AudioClip eatSound;
        public AudioClip bonusSound;
        public AudioClip dieSound;

        // =================================================================
        // NETWORK SYNCED VARIABLES
        // =================================================================
        [UdonSynced] private int gameMode = 0; // 0=menu, 1=solo, 2=tournament
        [UdonSynced] private int tournamentPlayers = 2;
        [UdonSynced] private int currentPlayerIndex = 0;
        [UdonSynced] private bool gameActive = false;
        [UdonSynced] private int currentScore = 0;

        // Tournament data
        [UdonSynced] private string[] playerNames = new string[6];
        [UdonSynced] private int[] playerScores = new int[6];
        [UdonSynced] private bool[] playerEliminated = new bool[6];
        [UdonSynced] private int tournamentRound = 0;

        // High scores (solo mode)
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
        private bool isOnQuest = false;

        // Snake game state
        private int[] snakeX;
        private int[] snakeY;
        private int snakeLength;
        private int dirX = 1;
        private int dirY = 0;
        private int nextDirX = 1;
        private int nextDirY = 0;
        private float moveTimer = 0f;
        private float currentSpeed;

        // Food
        private int fruitX;
        private int fruitY;
        private int bonusX = -1;
        private int bonusY = -1;
        private float bonusTimer = 0f;

        // Menu navigation
        private int menuSelection = 0; // 0=solo, 1=tournament

        private void Start()
        {
            if (gameDisplay == null)
            {
                Debug.LogError("[SymbolicSnake] gameDisplay is null!");
                return;
            }

            localPlayer = Networking.LocalPlayer;

            #if UNITY_ANDROID
            isOnQuest = true;
            #endif

            snakeX = new int[500];
            snakeY = new int[500];

            RenderMenu();
        }

        private void Update()
        {
            if (gameActive && playerLocked)
            {
                moveTimer += Time.deltaTime;

                if (moveTimer >= currentSpeed)
                {
                    moveTimer = 0f;
                    MoveSnake();
                }
            }
        }

        private void FixedUpdate()
        {
            if (gameActive || gameMode != 0)
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

            if (gameMode == 0) // Main menu
            {
                TakeOwnership();
                if (menuSelection == 0)
                {
                    // Start solo mode
                    gameMode = 1;
                    StartSoloGame();
                }
                else
                {
                    // Enter tournament setup
                    gameMode = 2;
                }
                RequestSerialization();
            }
            else if (gameMode == 2 && !gameActive) // Tournament waiting
            {
                // Player ready to start their turn
                if (currentPlayerIndex < tournamentPlayers)
                {
                    playerNames[currentPlayerIndex] = localPlayer.displayName;
                    localPlayer.Immobilize(true);
                    playerLocked = true;
                    SendCustomEventDelayedSeconds("StartTournamentRound", 2f);
                }
            }
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            if (gameMode == 0) // Menu navigation
            {
                if (value > 0.5f && menuSelection == 0)
                {
                    menuSelection = 1;
                    RenderMenu();
                }
                else if (value < -0.5f && menuSelection == 1)
                {
                    menuSelection = 0;
                    RenderMenu();
                }
            }
            else if (gameActive && playerLocked) // Snake movement
            {
                if (value > 0.5f && dirX == 0)
                {
                    nextDirX = 1;
                    nextDirY = 0;
                }
                else if (value < -0.5f && dirX == 0)
                {
                    nextDirX = -1;
                    nextDirY = 0;
                }
            }
        }

        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            if (gameMode == 2 && !gameActive) // Tournament player count
            {
                if (value > 0.5f && tournamentPlayers < 6)
                {
                    tournamentPlayers++;
                    RequestSerialization();
                    RenderGame();
                }
                else if (value < -0.5f && tournamentPlayers > 2)
                {
                    tournamentPlayers--;
                    RequestSerialization();
                    RenderGame();
                }
            }
            else if (gameActive && playerLocked) // Snake movement
            {
                if (value > 0.5f && dirY == 0)
                {
                    nextDirX = 0;
                    nextDirY = -1;
                }
                else if (value < -0.5f && dirY == 0)
                {
                    nextDirX = 0;
                    nextDirY = 1;
                }
            }
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (!value) return;
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

        private void StartSoloGame()
        {
            InitializeSnake();
            SpawnFruit();
            currentScore = 0;
            gameActive = true;

            if (localPlayer != null)
            {
                localPlayer.Immobilize(true);
                playerLocked = true;
            }

            Debug.Log("[SymbolicSnake] Solo game started!");
        }

        public void StartTournamentRound()
        {
            InitializeSnake();
            SpawnFruit();
            currentScore = 0;
            gameActive = true;

            Debug.Log("[SymbolicSnake] Tournament round started for " + playerNames[currentPlayerIndex]);
        }

        private void InitializeSnake()
        {
            snakeLength = defaultSnakeLength;
            currentSpeed = defaultSpeed;

            int startX = gameWidth / 2;
            int startY = gameHeight / 2;

            for (int i = 0; i < snakeLength; i++)
            {
                snakeX[i] = startX - i;
                snakeY[i] = startY;
            }

            dirX = 1;
            dirY = 0;
            nextDirX = 1;
            nextDirY = 0;

            bonusX = -1;
            bonusY = -1;
            bonusTimer = 0f;
        }

        private void MoveSnake()
        {
            // Update direction
            dirX = nextDirX;
            dirY = nextDirY;

            // Calculate new head position
            int newHeadX = snakeX[0] + dirX;
            int newHeadY = snakeY[0] + dirY;

            // Wall collision
            if (newHeadX < 0 || newHeadX >= gameWidth || newHeadY < 0 || newHeadY >= gameHeight)
            {
                GameOver();
                return;
            }

            // Self collision
            for (int i = 0; i < snakeLength; i++)
            {
                if (snakeX[i] == newHeadX && snakeY[i] == newHeadY)
                {
                    GameOver();
                    return;
                }
            }

            // Move snake body
            for (int i = snakeLength - 1; i > 0; i--)
            {
                snakeX[i] = snakeX[i - 1];
                snakeY[i] = snakeY[i - 1];
            }

            snakeX[0] = newHeadX;
            snakeY[0] = newHeadY;

            // Check fruit collision
            if (newHeadX == fruitX && newHeadY == fruitY)
            {
                snakeLength++;
                currentScore += 10;
                currentSpeed = Mathf.Max(0.05f, currentSpeed - speedIncreasePerFruit);
                SpawnFruit();
                PlaySound(eatSound);

                // Spawn bonus occasionally
                if (Random.value > 0.7f)
                {
                    SpawnBonus();
                }

                RequestSerialization();
            }

            // Check bonus collision
            if (bonusX >= 0 && newHeadX == bonusX && newHeadY == bonusY)
            {
                currentScore += 50;
                bonusX = -1;
                bonusY = -1;
                PlaySound(bonusSound);
                RequestSerialization();
            }

            // Update bonus timer
            if (bonusX >= 0)
            {
                bonusTimer -= currentSpeed;
                if (bonusTimer <= 0f)
                {
                    bonusX = -1;
                    bonusY = -1;
                }
            }
        }

        private void SpawnFruit()
        {
            bool validPosition = false;
            int attempts = 0;

            while (!validPosition && attempts < 100)
            {
                fruitX = Random.Range(0, gameWidth);
                fruitY = Random.Range(0, gameHeight);

                validPosition = true;
                for (int i = 0; i < snakeLength; i++)
                {
                    if (snakeX[i] == fruitX && snakeY[i] == fruitY)
                    {
                        validPosition = false;
                        break;
                    }
                }
                attempts++;
            }
        }

        private void SpawnBonus()
        {
            bool validPosition = false;
            int attempts = 0;

            while (!validPosition && attempts < 100)
            {
                bonusX = Random.Range(0, gameWidth);
                bonusY = Random.Range(0, gameHeight);

                validPosition = true;
                for (int i = 0; i < snakeLength; i++)
                {
                    if (snakeX[i] == bonusX && snakeY[i] == bonusY)
                    {
                        validPosition = false;
                        break;
                    }
                }

                if (bonusX == fruitX && bonusY == fruitY)
                {
                    validPosition = false;
                }

                attempts++;
            }

            bonusTimer = bonusItemDuration;
        }

        private void GameOver()
        {
            gameActive = false;
            PlaySound(dieSound);

            if (gameMode == 1) // Solo mode
            {
                UpdateHighScores();
            }
            else if (gameMode == 2) // Tournament
            {
                playerScores[currentPlayerIndex] = currentScore;
                currentPlayerIndex++;

                if (currentPlayerIndex >= tournamentPlayers)
                {
                    EliminateLosers();
                }
            }

            RequestSerialization();
            Debug.Log("[SymbolicSnake] Game Over! Score: " + currentScore);
        }

        private void UpdateHighScores()
        {
            if (localPlayer == null) return;
            string playerName = localPlayer.displayName;

            if (currentScore > highScore1)
            {
                highScore3 = highScore2;
                highScoreName3 = highScoreName2;
                highScore2 = highScore1;
                highScoreName2 = highScoreName1;
                highScore1 = currentScore;
                highScoreName1 = playerName;
            }
            else if (currentScore > highScore2)
            {
                highScore3 = highScore2;
                highScoreName3 = highScoreName2;
                highScore2 = currentScore;
                highScoreName2 = playerName;
            }
            else if (currentScore > highScore3)
            {
                highScore3 = currentScore;
                highScoreName3 = playerName;
            }
        }

        private void EliminateLosers()
        {
            // Find lowest score
            int lowestScore = 999999;
            for (int i = 0; i < tournamentPlayers; i++)
            {
                if (!playerEliminated[i] && playerScores[i] < lowestScore)
                {
                    lowestScore = playerScores[i];
                }
            }

            // Eliminate players with lowest score
            for (int i = 0; i < tournamentPlayers; i++)
            {
                if (!playerEliminated[i] && playerScores[i] == lowestScore)
                {
                    playerEliminated[i] = true;
                }
            }

            // Check if only one player left
            int activePlayers = 0;
            for (int i = 0; i < tournamentPlayers; i++)
            {
                if (!playerEliminated[i]) activePlayers++;
            }

            if (activePlayers <= 1)
            {
                tournamentRound = 999; // Tournament complete
            }
            else
            {
                tournamentRound++;
                currentPlayerIndex = 0; // Reset for next round
            }
        }

        private void ExitGame()
        {
            if (localPlayer != null && playerLocked)
            {
                localPlayer.Immobilize(false);
                playerLocked = false;
            }

            if (Networking.IsOwner(gameObject))
            {
                gameActive = false;
                gameMode = 0;
                menuSelection = 0;
                currentPlayerIndex = 0;
                tournamentRound = 0;

                for (int i = 0; i < 6; i++)
                {
                    playerNames[i] = "";
                    playerScores[i] = 0;
                    playerEliminated[i] = false;
                }

                RequestSerialization();
            }

            RenderMenu();
        }

        // =================================================================
        // NETWORK SYNC
        // =================================================================

        public override void OnDeserialization()
        {
            RenderGame();
        }

        // =================================================================
        // RENDERING
        // =================================================================

        private void RenderMenu()
        {
            if (gameDisplay == null) return;

            string output = "<color=#00FF00>SYMBOLOGY SNAKE</color>\n";
            output += "<color=#FFFF00>Tournament Edition</color>\n\n";

            string arrow1 = (menuSelection == 0) ? "<color=#FF0000>></color> " : "  ";
            string arrow2 = (menuSelection == 1) ? "<color=#FF0000>></color> " : "  ";

            output += arrow1 + "Single Player\n";
            output += arrow2 + "Tournament Mode\n\n";

            if (isOnQuest)
            {
                output += "Controls: Joystick (up/down) to select\n";
                output += "Click to start\n";
            }
            else
            {
                output += "Controls: Arrow Keys or Joystick to select\n";
                output += "Click to start\n";
            }

            gameDisplay.text = output;
        }

        private void RenderGame()
        {
            if (gameDisplay == null) return;

            string output = "";

            if (gameMode == 2 && !gameActive) // Tournament setup
            {
                output += "<color=#00FF00>TOURNAMENT SETUP</color>\n\n";
                output += "Players: " + tournamentPlayers + " (Up/Down to change)\n\n";
                output += "Click when ready to start!\n\n";

                if (currentPlayerIndex > 0)
                {
                    output += "<color=#FFFF00>Scores:</color>\n";
                    for (int i = 0; i < currentPlayerIndex; i++)
                    {
                        string strikethrough = playerEliminated[i] ? "<s>" : "";
                        string strikethroughEnd = playerEliminated[i] ? "</s>" : "";
                        output += strikethrough + playerNames[i] + ": " + playerScores[i] + strikethroughEnd + "\n";
                    }
                }

                gameDisplay.text = output;
                return;
            }

            if (!gameActive && gameMode == 1) // Solo game over
            {
                output += "<color=#FF0000>GAME OVER</color>\n";
                output += "<color=#FFFF00>Score: " + currentScore + "</color>\n\n";

                output += "<color=#00FF00>HIGH SCORES</color>\n";
                if (highScore1 > 0) output += "1. " + highScoreName1 + " - " + highScore1 + "\n";
                if (highScore2 > 0) output += "2. " + highScoreName2 + " - " + highScore2 + "\n";
                if (highScore3 > 0) output += "3. " + highScoreName3 + " - " + highScore3 + "\n";

                string exitButton = isOnQuest ? "Jump" : "Space";
                output += "\n" + exitButton + " to exit";

                gameDisplay.text = output;
                return;
            }

            if (!gameActive && gameMode == 2) // Tournament over
            {
                output += "<color=#FFFF00>TOURNAMENT COMPLETE!</color>\n\n";

                for (int i = 0; i < tournamentPlayers; i++)
                {
                    if (!playerEliminated[i])
                    {
                        output += "<color=#00FF00>WINNER: " + playerNames[i] + "</color>\n";
                        break;
                    }
                }

                output += "\n<color=#FFFF00>Final Scores:</color>\n";
                for (int i = 0; i < tournamentPlayers; i++)
                {
                    string strikethrough = playerEliminated[i] ? "<s>" : "";
                    string strikethroughEnd = playerEliminated[i] ? "</s>" : "";
                    output += strikethrough + playerNames[i] + ": " + playerScores[i] + strikethroughEnd + "\n";
                }

                gameDisplay.text = output;
                return;
            }

            // Active game rendering
            for (int y = 0; y < gameHeight; y++)
            {
                for (int x = 0; x < gameWidth; x++)
                {
                    bool isSnake = false;
                    bool isHead = false;

                    for (int i = 0; i < snakeLength; i++)
                    {
                        if (snakeX[i] == x && snakeY[i] == y)
                        {
                            isSnake = true;
                            if (i == 0) isHead = true;
                            break;
                        }
                    }

                    bool isFruit = (x == fruitX && y == fruitY);
                    bool isBonus = (x == bonusX && y == bonusY);

                    if (isHead)
                    {
                        output += "<color=#00FF00>O</color>";
                    }
                    else if (isSnake)
                    {
                        output += "<color=#00AA00>█</color>";
                    }
                    else if (isFruit)
                    {
                        output += "<color=#FF0000>●</color>";
                    }
                    else if (isBonus)
                    {
                        output += "<color=#FFFF00>$</color>";
                    }
                    else
                    {
                        output += " ";
                    }
                }
                output += "\n";
            }

            output += "<color=#FFFFFF>Score: " + currentScore + "</color>";

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
