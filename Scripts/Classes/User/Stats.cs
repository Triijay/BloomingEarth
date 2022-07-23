using Bayat.Json;
using System;

/// <summary>
/// User Stats of the Game (In Globals are ONLY the non-resettables)<br></br>
/// NOTICE that there are two kind of Statistics: those who resets with Game-Reset and those who not<br></br>
/// If we want stats for eg building-Progress, how long do users need to get to Building X, we want to use resettables<br></br>
/// If we want to know how long the users global playtime, we want to use a non-resettable stat
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Stats {

// Things calculated on the very first Game Start

    /// <summary>
    /// Date when the Game was opened the first time ever
    /// </summary>
    [JsonProperty(PropertyName = "FirstGameLoad")]
    public DateTime FirstGameLoad = new DateTime();

    
// Things we have to calculate at the Start of the Game

    /// <summary>
    /// Counts how often the User opened the game since firstGameLoad
    /// </summary>
    [JsonProperty(PropertyName = "GameOpenings")]
    public int GameOpeningsRaw = 0;

    /// <summary>
    /// Date when the Game was last opened by user -> important for DaysWithGameOpening
    /// </summary>
    [JsonProperty(PropertyName = "LastDateGameOpen")]
    public DateTime LastDateGameOpen = new DateTime();

    /// <summary>
    /// Count of Days when the user opened out app at least once (since firstGameLoad)
    /// </summary>
    [JsonProperty(PropertyName = "DaysWithGameOpening")]
    public int DaysWithGameOpening = 0;

    /// <summary>
    /// Count of how many Days the User opened the Game without one day offline
    /// </summary>
    [JsonProperty(PropertyName = "DaysWithConsecutiveGameOpening")]
    public int DaysWithConsecutiveGameOpening = 0;


// Things we can save at the End of the Game


    /// <summary>
    /// Date when the Game was last opened by user 
    /// </summary>
    [JsonProperty(PropertyName = "LastDateGameClosed")]
    public DateTime LastDateGameClosed = new DateTime();

    /// <summary>
    /// User Seconds played overall
    /// </summary>
    [JsonProperty(PropertyName = "SecondsPlayedOverall")]
    public int SecondsPlayedOverall = 0;

    /// <summary>
    /// User Seconds played overall
    /// </summary>
    [JsonProperty(PropertyName = "SecondsPlayedBeforeLastSession")]
    public int SecondsPlayedBeforeLastSession = 0;

    /// <summary>
    /// Date when the User last claimed the Offline Coins
    /// </summary>
    [JsonProperty(PropertyName = "LastOfflineCoinsClaimed")]
    public DateTime LastOfflineCoinsClaimed = new DateTime();



    /// <summary>
    /// How much Ads the User has watched
    /// </summary>
    [JsonProperty(PropertyName = "adsWatched")]
    public int adsWatched = 0;

    /// <summary>
    /// How much Ads the User has watched - Boost Income
    /// </summary>
    [JsonProperty(PropertyName = "adsWatchedBoost")]
    public int adsWatchedBoost = 0;

    /// <summary>
    /// How much Ads the User has watched - Delay Unlock Building Progress
    /// </summary>
    [JsonProperty(PropertyName = "adsWatchedBuildingProgress")]
    public int adsWatchedBuildingProgress = 0;

    /// <summary>
    /// How much Ads the User has watched - Offline Coins
    /// </summary>
    [JsonProperty(PropertyName = "adsWatchedOfflineCoins")]
    public int adsWatchedOfflineCoins = 0;



    /// <summary>
    /// Users Highscore in Minigame: Catch The Nuts
    /// </summary>
    [JsonProperty(PropertyName = "HighscoreCatchTheNuts")]
    public int HighscoreCatchTheNuts = 0;

}