/// <summary>
  /// DT_WeatherModule.cs - Weather API Integration Module
  /// Part of DOSTerminalController modularization (Phase 4 of 8)
  ///
  /// PURPOSE:
  /// Handles live weather data fetching, parsing, and display integration
  /// Manages weather API polling with platform-specific intervals (Quest: 10min, Desktop: 2min)
  /// Integrates with rain shader and achievement system for weather-based events
  ///
  /// DEPENDENCIES:
  /// - DT_RainShader: Rain effect control based on weather conditions
  /// - DT_CacheManager (optional): DateTime caching for Quest optimization
  /// - AchievementTracker (optional): Heavy Rain achievement checking via CheckWeatherAchievements()
  /// - VRCStringDownloader: Async weather API fetching
  ///
  /// EXTRACTED FROM: DOSTerminalController.cs lines 111-113, 127-129, 168-169, 952-985, 1001-1006, 1503-1643
  /// </summary>

  using UdonSharp;
  using UnityEngine;
  using VRC.SDKBase;
  using VRC.Udon;
  using VRC.SDK3.StringLoading;
  using LowerLevel.Achievements;

  namespace LowerLevel.Terminal
  {
      [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
      public class DT_WeatherModule : UdonSharpBehaviour
      {
          [Header("Weather API Configuration")]
          [Tooltip("GitHub Pages weather API endpoint")]
          public VRCUrl weatherApiUrl = new
  VRCUrl("https://cloudstrife7.github.io/DOS-Terminal/api/weather/current.json");

          [Header("Weather Settings")]
          [Tooltip("Enable simple weather display")]
          public bool enableSimpleWeather = true;

          [Tooltip("Weather update interval for Quest (seconds) - default 600 = 10 minutes")]
          public float questWeatherInterval = 600f;

          [Tooltip("Weather update interval for Desktop (seconds) - default 120 = 2 minutes")]
          public float desktopWeatherInterval = 120f;

          [Tooltip("Enable Quest-specific optimizations (disables weather on Quest if false)")]
          public bool enableQuestOptimizations = true;

          [Header("Module Dependencies")]
          [Tooltip("Rain shader module for visual effects")]
          public DT_RainShader rainShaderModule;

          [Tooltip("Optional: Cache manager for Quest optimizations")]
          public DT_CacheManager cacheManager;

          [Tooltip("Optional: AchievementTracker for Heavy Rain achievement checking")]
          public AchievementTracker achievementTracker;

          // =================================================================
          // INTERNAL STATE
          // =================================================================

          // Platform detection (set by parent controller)
          [System.NonSerialized] public bool isOnQuest = false;

          // Weather data
          private string currentWeatherTemp = "74°F";
          private string currentWeatherCondition = "Clear";
          private bool weatherOnline = true;
          private float lastWeatherUpdate = 0f;
          private float weatherUpdateInterval = 120f;

          // Debug logging
          private bool enableDebugLogging = true;

          // =================================================================
          // PUBLIC API
          // =================================================================

          /// <summary>
          /// Initialize weather updates with platform-specific intervals
          /// Called by DOSTerminalController after platform detection
          /// </summary>
          public void StartWeatherUpdates()
          {
              if (enableSimpleWeather)
              {
                  // Set platform-specific interval
                  weatherUpdateInterval = isOnQuest ? questWeatherInterval : desktopWeatherInterval;

                  string platform = isOnQuest ? "Quest" : "Desktop";
                  LogDebug("🌤️ Starting weather updates (interval: " + weatherUpdateInterval + "s for " + platform + ")");
                  FetchWeatherData();
              }
          }

          /// <summary>
          /// Initiates asynchronous download of weather data from the configured weather API URL.
          /// Uses VRCStringDownloader to fetch real-time weather JSON from GitHub Pages endpoint.
          /// Includes platform-specific optimizations: Quest checks enableQuestOptimizations flag.
          /// Success/error callbacks: OnStringLoadSuccess() and OnStringLoadError().
          /// Only executes if enableSimpleWeather is true.
          /// Auto-schedules next fetch based on platform-specific interval (Quest: 10min, Desktop: 2min).
          /// </summary>
          public void FetchWeatherData()
          {
              if (!enableSimpleWeather) return;

              // Quest safety check
              if (isOnQuest && !enableQuestOptimizations)
              {
                  LogDebug("🌤️ Weather disabled on Quest due to optimization settings");
                  return;
              }

              LogDebug($"🌤️ Fetching weather from: {weatherApiUrl}");
              VRCStringDownloader.LoadUrl(weatherApiUrl, (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);
          }

          /// <summary>
          /// Get current weather status string for display
          /// Returns "Weather Offline" if weather is disabled or unavailable
          /// </summary>
          public string GetWeatherStatus()
          {
              if (enableSimpleWeather && weatherOnline)
              {
                  return $"{currentWeatherTemp} {currentWeatherCondition}";
              }
              return "Weather Offline";
          }

          /// <summary>
          /// Get current temperature string
          /// </summary>
          public string GetTemperature()
          {
              return currentWeatherTemp;
          }

          /// <summary>
          /// Get current condition string
          /// </summary>
          public string GetCondition()
          {
              return currentWeatherCondition;
          }

          /// <summary>
          /// Check if weather is online
          /// </summary>
          public bool IsWeatherOnline()
          {
              return weatherOnline;
          }

          // =================================================================
          // VRC STRING DOWNLOADER CALLBACKS
          // =================================================================

          public override void OnStringLoadSuccess(IVRCStringDownload result)
          {
              int previewLength = Mathf.Min(100, result.Result.Length);
              string preview = result.Result.Substring(0, previewLength);
              LogDebug("🌐 String download received: " + preview + "...");

              if (result.Result.Contains("temperature") && result.Result.Contains("condition"))
              {
                  LogDebug("🌤️ Weather data detected and processing...");
                  ProcessWeatherData(result.Result);
                  weatherOnline = true;
                  lastWeatherUpdate = Time.time;

                  // Schedule next update with platform-specific interval
                  float nextUpdate = isOnQuest ? questWeatherInterval : desktopWeatherInterval;
                  SendCustomEventDelayedSeconds(nameof(FetchWeatherData), nextUpdate);
              }
              else
              {
                  LogDebug($"📡 Non-weather data received (length: {result.Result.Length})");
              }
          }

          public override void OnStringLoadError(IVRCStringDownload result)
          {
              LogDebug("❌ Weather fetch failed, using cached data");
              weatherOnline = false;

              // Retry with platform-specific interval
              float retryInterval = isOnQuest ? questWeatherInterval : desktopWeatherInterval;
              SendCustomEventDelayedSeconds(nameof(FetchWeatherData), retryInterval);
          }

          // =================================================================
          // WEATHER DATA PROCESSING
          // =================================================================

          /// <summary>
          /// Parse weather JSON and update internal state
          /// Triggers rain shader updates and achievement hooks
          /// </summary>
          private void ProcessWeatherData(string jsonData)
          {
              LogDebug($"🌤️ Processing weather JSON: {jsonData}");

              string cleanJson = jsonData.Replace("\n", "").Replace("\r", "").Replace(" ", "");
              LogDebug($"🧹 Cleaned JSON: {cleanJson.Substring(0, Mathf.Min(50, cleanJson.Length))}...");

              // Extract temperature
              string tempSearch = "\"temperature\":\"";
              int tempStart = cleanJson.IndexOf(tempSearch);
              if (tempStart >= 0)
              {
                  tempStart += tempSearch.Length;
                  int tempEnd = cleanJson.IndexOf("\"", tempStart);
                  if (tempEnd > tempStart)
                  {
                      string newTemp = cleanJson.Substring(tempStart, tempEnd - tempStart);
                      LogDebug($"🌡️ Extracted temperature: {newTemp} (was: {currentWeatherTemp})");
                      currentWeatherTemp = newTemp;
                  }
              }

              // Extract condition
              string conditionSearch = "\"condition\":\"";
              int conditionStart = cleanJson.IndexOf(conditionSearch);
              if (conditionStart >= 0)
              {
                  conditionStart += conditionSearch.Length;
                  int conditionEnd = cleanJson.IndexOf("\"", conditionStart);
                  if (conditionEnd > conditionStart)
                  {
                      string newCondition = cleanJson.Substring(conditionStart, conditionEnd - conditionStart);
                      string normalizedCondition = NormalizeWeatherCondition(newCondition);

                      currentWeatherCondition = normalizedCondition;

                      // Update rain shader via module
                      if (rainShaderModule != null)
                      {
                          rainShaderModule.UpdateRain(normalizedCondition);
                      }
                      else
                      {
                          LogDebug("⚠️ RainShaderModule not assigned");
                      }

                      // Trigger weather achievement notification when condition indicates rain-like weather
                      {
                          string _lc = normalizedCondition.ToLower();
                          bool _isRainy = _lc.Contains("rain") || _lc.Contains("storm") || _lc.Contains("drizzle");

                          if (_isRainy)
                          {
                              // Notify AchievementTracker to check Heavy Rain achievement
                              if (achievementTracker != null)
                              {
                                  achievementTracker.CheckWeatherAchievements(normalizedCondition);
                              }

                              // Immediate feedback in logs
                              LogDebug("🌧️ Rainy weather detected, checking Heavy Rain achievement");
                          }
                      }
                  }
              }

              LogDebug($"🌤️ Weather updated successfully: {currentWeatherTemp} {currentWeatherCondition}");
          }

          /// <summary>
          /// Normalize weather condition strings from various formats
          /// Handles common weather API condition names and formats them for display
          /// </summary>
          private string NormalizeWeatherCondition(string rawCondition)
          {
              if (string.IsNullOrEmpty(rawCondition)) return rawCondition;

              string conditionKey = rawCondition.Replace(" ", "").ToLower();

              switch (conditionKey)
              {
                  case "thunderyoutbreaksinnearby":
                      return "Thunder";
                  case "patchylightdrizzle":
                      return "Light Drizzle";
                  case "moderaterainattimes":
                      return "Moderate Rain";
                  case "partlycloudy":
                      return "Partly Cloudy";
                  case "clearsky":
                      return "Clear";
                  default:
                      // Insert spaces before capital letters for camelCase strings
                      System.Text.StringBuilder sb = new System.Text.StringBuilder();
                      foreach (char c in rawCondition)
                      {
                          if (char.IsUpper(c) && sb.Length > 0) sb.Append(' ');
                          sb.Append(c);
                      }
                      return sb.ToString();
              }
          }

          // =================================================================
          // DEBUG LOGGING
          // =================================================================

          private void LogDebug(string message)
          {
              if (enableDebugLogging)
              {
                  Debug.Log($"[DT_WeatherModule] {message}");
              }
          }
      }
  }