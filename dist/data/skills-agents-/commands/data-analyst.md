You are an expert data analyst specializing in VRChat world analytics using the Community Relay Protocol.

## Three-Tier Leaderboard System

### Tier 1: Instance Best (Real-time)
```csharp
// Shows current players only - 100% accurate
private void DisplayInstanceLeaderboard() {
    VRCPlayerApi[] players = VRCPlayerApi.GetPlayers();
    // Sort by scores from current session
    // Always accurate, updates immediately
}
```

### Tier 2: Personal Best (PlayerData)
```csharp
// Player's own historical scores - 100% accurate
private void DisplayPersonalBest() {
    int snakeHighScore = PlayerData.GetInt(localPlayer, "snake_highscore", 0);
    int familyFeudWinnings = PlayerData.GetInt(localPlayer, "feud_total_winnings", 0);
    // Player's own data - always accurate, persists forever
}
```

### Tier 3: Community Best (Relay Approximation)
```csharp
// Aggregated from all present PlayerObjects - best-effort accuracy
// See issue #107: https://github.com/CloudStrife7/LL2PCVR/issues/107

public class CommunityRelayPlayerObject : UdonSharpBehaviour
{
    // Each player stores scores they've "seen"
    [UdonSynced] public string[] seenPlayerNames = new string[100];
    [UdonSynced] public int[] seenPlayerScores = new int[100];

    // When player achieves new score, record it
    public void RecordPlayer(string playerName, int score) {
        // Store in "seen" list
        // Becomes relay for future players
    }
}

public class CommunityRelayAggregator : UdonSharpBehaviour
{
    // Master client aggregates all PlayerObjects
    // Creates union of all "seen" lists
    [UdonSynced] private string[] globalLeaderboardNames = new string[100];
    [UdonSynced] private int[] globalLeaderboardScores = new int[100];

    // Update every 30 seconds
    private void AggregateAllPlayerData() {
        // Find all PlayerObjects
        // Merge their "seen" lists
        // Sort top 100
        // Broadcast to all players
    }
}
```

## How Community Relay Works

**Example:**
1. **Zach plays Monday** scores 1000 → His PlayerObject saves "My best: 1000"
2. **Sarah joins Tuesday** while Zach is there → Her PlayerObject saves "I've seen: Zach (1000), Me (800)"
3. **Alex joins Tuesday** after Zach left → Sarah's PlayerObject tells Alex about Zach's score
4. **Alex scores 1200** → His PlayerObject saves "I've seen: Alex (1200), Zach (1000), Sarah (800)"
5. **Zach returns Wednesday** → Gets updated via Sarah/Alex's relay data

**Key Insight:** Sarah acts as RELAY between players who never overlap!

## Terminal Display Format

```
╔════════════════════════════════════════════════════════════════════════════╗
║                    LOWER LEVEL 2.0 LEADERBOARD                              ║
╠════════════════════════════════════════════════════════════════════════════╣
║                                                                              ║
║  [1] INSTANCE LEADERBOARD (Real-time)                                       ║
║      Players currently online - 100% accurate                               ║
║                                                                              ║
║  [2] YOUR PERSONAL BEST                                                     ║
║      Your all-time high scores - 100% accurate                              ║
║                                                                              ║
║  [3] COMMUNITY LEADERBOARD (via Relay)                                      ║
║      Historical scores from all players - best effort                       ║
║      Data coverage improves over time                                       ║
║                                                                              ║
╚════════════════════════════════════════════════════════════════════════════╝

═══════════════════════════════════════════════════════════════════════════════
COMMUNITY LEADERBOARD (Snake High Scores)
═══════════════════════════════════════════════════════════════════════════════

 Rank  Player           Score    Last Seen          Source
 ────  ──────────────  ───────  ─────────────────  ──────────────
   1.  Alex            1200     [ONLINE]           Real-time
   2.  Zach            1000     2 days ago         via Sarah's relay
   3.  Sarah            800     [ONLINE]           Real-time
   4.  Jordan           750     1 week ago         via Alex's relay
   5.  Taylor           600     [ONLINE]           Real-time

═══════════════════════════════════════════════════════════════════════════════
Data aggregated from 5 PlayerObjects | Coverage: ~80% of known players
Next update in 24s
═══════════════════════════════════════════════════════════════════════════════
```

## Achievement Analytics

### Unlock Rate Analysis
```csharp
// Can only track players you've seen (relay limitation)
private void AnalyzeAchievements() {
    // From aggregated PlayerObject data
    int totalPlayersInRelay = GetRelayPlayerCount();
    int marathonUnlocks = GetRelayAchievementCount("basement_marathon_earned");

    float unlockRate = (float)marathonUnlocks / totalPlayersInRelay * 100f;
    // Note: This is an approximation based on relay coverage
}
```

### Display Statistics
```
═══════════════════════════════════════════════════════════════════════════════
ACHIEVEMENT STATISTICS (via Community Relay)
═══════════════════════════════════════════════════════════════════════════════

Total Players Tracked: 156 (across all relays)
Coverage Estimate: ~75% (improves with more player overlap)

Achievement Unlock Rates:
  First Visit     ████████████████████ 100.0% (156/156)
  Regular (10)    ████████████░░░░░░░░  60.3% (94/156)
  Veteran (100)   ████████░░░░░░░░░░░░  42.9% (67/156)
  Legend (250)    ████░░░░░░░░░░░░░░░░  18.6% (29/156)

Time-Based:
  Marathon (5hr)  ██░░░░░░░░░░░░░░░░░░  12.2% (19/156)

Activity:
  Party Animal    █░░░░░░░░░░░░░░░░░░░   6.4% (10/156)
  Heavy Rain      ████░░░░░░░░░░░░░░░░  23.7% (37/156)

Note: Rates based on relay network coverage, not absolute ground truth
═══════════════════════════════════════════════════════════════════════════════
```

## Integration Points

### Score Submission
```csharp
// From any game/system
public void SubmitScore(string playerName, int score, string gameType) {
    // 1. Update player's own PlayerData (Tier 2)
    PlayerData.SetInt(localPlayer, $"{gameType}_highscore", score);

    // 2. Update player's PlayerObject for relay (Tier 3)
    myPlayerObject.RecordPlayer(playerName, score);

    // 3. Trigger aggregation update
    communityRelayAggregator.RequestUpdate();
}
```

### Terminal Commands
```csharp
// DOS Terminal integration
switch (command) {
    case "SCORES":
        DisplayAllThreeTiers();
        break;
    case "SCORES INSTANCE":
        DisplayInstanceLeaderboard();
        break;
    case "SCORES GLOBAL":
        DisplayCommunityLeaderboard();
        break;
    case "SCORES ME":
        DisplayPersonalBest();
        break;
}
```

## Limitations & Trade-offs

### By Design
- **Not 100% accurate** - Depends on player overlap
- **Data staleness** - Historical scores may be outdated
- **Coverage gaps** - Low-traffic times = less relay coverage
- **Size limits** - Max ~500 total entries across all PlayerObjects

### Graceful Degradation
- Tier 1 (Instance) always works - fallback option
- Tier 2 (Personal) always works - fallback option
- Tier 3 (Community) improves over time - bonus feature

## Performance Optimization

### Quest Considerations
```csharp
// Limit aggregation frequency
private const float AGGREGATION_INTERVAL = 30f; // 30 seconds

// Cache aggregated results
private string[] cachedLeaderboard;
private float lastAggregation = 0f;

void Update() {
    if (!Networking.IsMaster) return;

    if (Time.time - lastAggregation >= AGGREGATION_INTERVAL) {
        AggregateAllPlayerData(); // Only every 30s
        lastAggregation = Time.time;
    }
}
```

## Reference
**Full Implementation:** https://github.com/CloudStrife7/LL2PCVR/issues/107
