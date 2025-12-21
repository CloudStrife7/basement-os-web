 using UdonSharp;
  using UnityEngine;
  using VRC.SDKBase;
  using VRC.SDK3.Persistence;

  /// <summary>
  /// DT_CacheManager - Performance caching system for Quest optimization
  /// Caches DateTime values and PlayerData queries to reduce expensive API calls
  /// Part of DOSTerminal modular architecture (Phase 3)
  /// </summary>
  public class DT_CacheManager : UdonSharpBehaviour
  {
      [Header("Cache Settings")]
      [Tooltip("PlayerData cache duration in seconds")]
      public float playerDataCacheExpiration = 5f;
      [Tooltip("DateTime cache update interval in seconds")]
      public float dateTimeCacheInterval = 1f;
      [Tooltip("Enable cache optimizations")]
      public bool enableCaching = true;

      [Header("Debug")]
      public bool enableDebugLogging = false;

      // DateTime Caching System
      private string cachedCurrentTime = "";
      private string cachedCurrentDate = "";
      private float lastTimeUpdate = 0f;

      // PlayerData Caching System (20 entry LRU cache)
      private string[] playerDataCacheKeys = new string[20];
      private string[] playerDataCacheValues = new string[20];
      private float[] playerDataCacheTimestamps = new float[20];
      private int playerDataCacheCount = 0;
      private float lastPlayerDataCacheClean = 0f;

      // =================================================================
      // PUBLIC API - INITIALIZATION
      // =================================================================

      /// <summary>
      /// Initialize all caching systems
      /// Called by Core Controller on Start()
      /// </summary>
      public void InitializeCaches()
      {
          // Initialize DateTime cache
          UpdateTimeCache();

          // Initialize PlayerData cache arrays
          for (int i = 0; i < playerDataCacheKeys.Length; i++)
          {
              playerDataCacheKeys[i] = "";
              playerDataCacheValues[i] = "";
              playerDataCacheTimestamps[i] = 0f;
          }
          playerDataCacheCount = 0;

          LogDebug("🗃️ Cache system initialized");
      }

      // =================================================================
      // PUBLIC API - DATETIME CACHE
      // =================================================================

      /// <summary>
      /// Update cached DateTime values
      /// Call this periodically (e.g., once per second) instead of per-frame
      /// </summary>
      public void UpdateTimeCache()
      {
          if (Time.time - lastTimeUpdate >= dateTimeCacheInterval)
          {
              cachedCurrentTime = System.DateTime.Now.ToString("h:mm:ss tt");
              cachedCurrentDate = System.DateTime.Now.ToString("MM.dd.yyyy");
              lastTimeUpdate = Time.time;

              LogDebug($"🕐 Time cache updated: {cachedCurrentTime}");
          }
      }

      /// <summary>
      /// Get cached current time string
      /// </summary>
      public string GetCachedTime()
      {
          return cachedCurrentTime;
      }

      /// <summary>
      /// Get cached current date string
      /// </summary>
      public string GetCachedDate()
      {
          return cachedCurrentDate;
      }

      // =================================================================
      // PUBLIC API - PLAYERDATA CACHE
      // =================================================================

      /// <summary>
      /// Get cached PlayerData string value with expiration
      /// Reduces expensive PlayerData API calls
      /// </summary>
      public string GetCachedPlayerDataString(string key, string defaultValue = "")
      {
          if (!enableCaching)
          {
              // Fallback to direct PlayerData access if caching disabled
              VRCPlayerApi currentUser = Networking.LocalPlayer;
              if (Utilities.IsValid(currentUser) && PlayerData.HasKey(currentUser,
  key))
              {
                  return PlayerData.GetString(currentUser, key);
              }
              return defaultValue;
          }

          // Check cache first
          for (int i = 0; i < playerDataCacheCount; i++)
          {
              if (playerDataCacheKeys[i] == key)
              {
                  // Check if cache entry is still valid
                  if (Time.time - playerDataCacheTimestamps[i] <
  playerDataCacheExpiration)
                  {
                      return playerDataCacheValues[i];
                  }
                  else
                  {
                      // Cache expired, remove this entry
                      RemoveCacheEntry(i);
                      break;
                  }
              }
          }

          // Not in cache or expired, fetch from PlayerData
          VRCPlayerApi player = Networking.LocalPlayer;
          string value = defaultValue;

          if (Utilities.IsValid(player) && PlayerData.HasKey(player, key))
          {
              value = PlayerData.GetString(player, key);
          }

          // Add to cache
          AddToCache(key, value);

          return value;
      }

      /// <summary>
      /// Get cached PlayerData int value
      /// </summary>
      public int GetCachedPlayerDataInt(string key, int defaultValue = 0)
      {
          string stringValue = GetCachedPlayerDataString(key,
  defaultValue.ToString());

          int result;
          if (int.TryParse(stringValue, out result))
          {
              return result;
          }

          return defaultValue;
      }

      // =================================================================
      // PUBLIC API - MAINTENANCE
      // =================================================================

      /// <summary>
      /// Clean expired cache entries
      /// Should be called periodically (every 30 seconds)
      /// </summary>
      public void MaintenanceCaches()
      {
          if (Time.time - lastPlayerDataCacheClean > 30f)
          {
              int removedCount = 0;

              // Remove expired entries (iterate backwards for safe removal)
              for (int i = playerDataCacheCount - 1; i >= 0; i--)
              {
                  if (Time.time - playerDataCacheTimestamps[i] >
  playerDataCacheExpiration)
                  {
                      RemoveCacheEntry(i);
                      removedCount++;
                  }
              }

              if (removedCount > 0)
              {
                  LogDebug($"🧹 Cleaned {removedCount} expired cache entries");
              }

              lastPlayerDataCacheClean = Time.time;
          }

          // Schedule next maintenance
          SendCustomEventDelayedSeconds("MaintenanceCaches", 30f);
      }

      // =================================================================
      // PRIVATE HELPERS
      // =================================================================

      /// <summary>
      /// Add entry to PlayerData cache (LRU eviction)
      /// </summary>
      private void AddToCache(string key, string value)
      {
          // If cache is full, remove oldest entry (index 0)
          if (playerDataCacheCount >= playerDataCacheKeys.Length)
          {
              RemoveCacheEntry(0);
          }

          // Add new entry
          playerDataCacheKeys[playerDataCacheCount] = key;
          playerDataCacheValues[playerDataCacheCount] = value;
          playerDataCacheTimestamps[playerDataCacheCount] = Time.time;
          playerDataCacheCount++;
      }

      /// <summary>
      /// Remove cache entry by index and shift remaining entries
      /// </summary>
      private void RemoveCacheEntry(int index)
      {
          if (index < 0 || index >= playerDataCacheCount) return;

          // Shift all entries after this index down
          for (int i = index; i < playerDataCacheCount - 1; i++)
          {
              playerDataCacheKeys[i] = playerDataCacheKeys[i + 1];
              playerDataCacheValues[i] = playerDataCacheValues[i + 1];
              playerDataCacheTimestamps[i] = playerDataCacheTimestamps[i + 1];
          }

          playerDataCacheCount--;
      }

      /// <summary>
      /// Debug logging helper
      /// </summary>
      private void LogDebug(string message)
      {
          if (enableDebugLogging)
          {
              Debug.Log($"[DT_CacheManager] {message}");
          }
      }
  }