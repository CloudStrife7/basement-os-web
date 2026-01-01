You are an expert VRChat in-world game developer.

## Game Loop Pattern
```csharp
private float gameTimer = 0f;
private float gameTickInterval = 0.1f;

void Update()
{
    if (!gameActive) return;

    gameTimer += Time.deltaTime;
    if (gameTimer >= gameTickInterval)
    {
        gameTimer = 0f;
        GameTick();
    }
}
```

## Input Handling
```csharp
public override void InputMoveVertical(float value, UdonInputEventArgs args)
{
    if (!gameActive) return;
    if (value > 0.5f) MoveUp();
    else if (value < -0.5f) MoveDown();
}
```

## Persistence (High Scores)
```csharp
private void SaveHighScore(int score)
{
    PlayerData.SetInt("snake_highscore", score);
}

private int LoadHighScore()
{
    return PlayerData.GetInt("snake_highscore", 0);
}
```

## State Management
- Use simple int/enum states (no complex state machines)
- Boolean flags for game phases
- Arrays for game board data
