using Bayat.SaveSystem;
using GooglePlayGames.BasicApi.SavedGame;
using Firebase;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Bayat.SaveSystem.Storage;
using System.Threading.Tasks;
using Firebase.Crashlytics;
using System.IO;

/// <summary>
/// Manages the saving and loading of the game <br></br>
/// is able to upload the encrypted playerdata to the playfab cloud <br></br>
/// also saves a local encrypted save file to disk
/// </summary>
public static class SavingSystem {

    #region Vars

    /// <summary>
    /// Google Firebase DependencyStatus
    /// </summary>
    private static DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

    /// <summary>
    /// If false, the SavingSystem will not try to save/upload Data from Playfab
    /// </summary>
    public static bool saveOrLoadPlayfab = true;



    // Performance Optimization
    private static DateTime now, yesterday;
   
    public static SaveSystemSettingsPreset saveLocalPreset;

    private static SaveGame localSave = null;
    private static SaveGame cloudSave = null;
    private static SaveGame loadedSave = new SaveGame();

    
    /// <summary>
    /// The locations where to save the data
    /// </summary>
    public enum SaveLocation {
        local, 
        playfab
    }

    // Folder and Filenames
    public const string fileNameSaveGameRootFolder = "bin/"; // DONT Change until we want to lose the Progress at all Users!
    public const string fileNameSaveGameFolderLocal = "LOC/"; // DONT Change until we want to lose the Progress at all Users!
    public const string fileNameSaveGameFolderPlayfab = "PF/"; // DONT Change until we want to lose the Progress at all Users!
    public const string fileNameSaveGameSubFolderWorld = "wrld/"; // DONT Change until we want to lose the Progress at all Users!

    public const string fileNameSaveGameFileName = "bs_vKGwtktsgs1604x0"; // DONT Change until we want to lose the Progress at all Users!
    public const string fileNameSaveGameFileType = ".json"; // DONT Change until we want to lose the Progress at all Users!
    public const string fileNameSaveGameFileNameWorld = "w_uwWjdPJldDMDx0"; // DONT Change until we want to lose the Progress at all Users!
    public const string fileNameZipfile = "data_BE"; // DONT Change until we want to lose the Progress at all Users!

    /// <summary>
    /// Level since last game state saved 
    /// </summary>
    private static int levelSinceLastSave = 0;
    /// <summary>
    /// Level Intervall to save after e.g. 
    /// Save every X levels
    /// </summary>
    private const int levelSavingIntervall = 10;

    #endregion

    /* ------------------------------------------------------------- 
     --------------------------- SAVE ------------------------------
     -------------------------------------------------------------- */

    /// <summary>
    /// Save current game data to local disk <br></br>
    /// If <paramref name="savePlayFab"/> is true, 
    /// the data will also be uploaded to playfab cloud
    /// </summary>
    /// <param name="savePlayFab"></param>
    public static async Task<bool> saveGameData(bool savePlayFab = true) {
        savePlayerPrefs(System.DateTime.Now);

        // Save Last Session SecondsPlayed
        Globals.Game.currentUser.stats.SecondsPlayedBeforeLastSession = Globals.Game.currentUser.stats.SecondsPlayedOverall;
        // Save new SecondsPlayed, with adding Time-Running
        Globals.Game.currentUser.stats.SecondsPlayedOverall = (int)Time.time + Globals.Game.currentUser.stats.SecondsPlayedOverall;

        // Tutorial Stage
        Globals.Controller.Tutorial.saveTutorialIDAndStageID();

        // User
        Globals.Game.saveGame.user = Globals.Game.currentUser;

        // Update Leaderboard Total Level Score
        Globals.Controller.GPiOS.ScoreToLeaderBoard(Globals.Game.currentUser.scoreTotalLevels, GPGSIds.leaderboard_mighty_constructors_building_levels);


        /*---------------------- LOCAL -----------------------*/

        // Decide which Save System Settings should be used
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            saveLocalPreset = Resources.Load("Bayat/Kaloa Save System Settings") as SaveSystemSettingsPreset;
        } else {
            saveLocalPreset = Resources.Load("Bayat/Local Save System Settings") as SaveSystemSettingsPreset;
        }

        // Save the data local
        await saveWithSaveSystem("", SaveLocation.local, Globals.Game.saveGame); // Base
        await saveWithSaveSystem(fileNameSaveGameSubFolderWorld, SaveLocation.local, Globals.Game.currentWorld); // World


        Globals.UICanvas.DebugLabelAddText("-- Saved Game Local --");

        /*---------------------- PlayFab -----------------------*/
        if (saveOrLoadPlayfab && savePlayFab && !Globals.KaloaSettings.preventPlayfabCommunication) {
            PlayFabFileSync.UploadFile();
        }


        Debug.Log("InitGame: User played our game for " + Globals.Game.currentUser.stats.SecondsPlayedOverall + " Seconds overall (was " + Globals.Game.currentUser.stats.SecondsPlayedBeforeLastSession + "s before gamestart)");
        Debug.Log("InitGame: Application ending after " + Time.time + " seconds");

        return true;
    }

    

    /// <summary>
    /// Saves the PlayerPrefs (Variables that should be stored after Game ends)
    /// </summary>
    public static void savePlayerPrefs(DateTime currentTime) {

        // Save Last Opening Timestamp
        Globals.Game.currentUser.stats.LastDateGameClosed = currentTime;

        PlayerPrefs.SetString(Globals.UniquePlayerPrefs.global_settings_language, Globals.UserSettings.language);
        PlayerPrefs.SetString(Globals.UniquePlayerPrefs.global_settings_hasRated, Globals.UserSettings.hasRated.ToString());
        PlayerPrefs.SetString(Globals.UniquePlayerPrefs.global_settings_hasSound, Globals.UserSettings.hasSound.ToString());
        PlayerPrefs.SetString(Globals.UniquePlayerPrefs.global_settings_hasMusic, Globals.UserSettings.hasMusic.ToString());
        PlayerPrefs.SetString(Globals.UniquePlayerPrefs.global_settings_hasNotifications, Globals.UserSettings.hasNotifications.ToString());
        PlayerPrefs.SetString(Globals.UniquePlayerPrefs.global_settings_hasVibration, Globals.UserSettings.hasVibration.ToString());
        PlayerPrefs.SetString(Globals.UniquePlayerPrefs.global_settings_wasSignedIn, Globals.Game.currentUser.wasSignedIn.ToString());
        PlayerPrefs.SetString(Globals.UniquePlayerPrefs.global_settings_linkedGoogleAccountToPlayfab, Globals.UserSettings.linkedGoogleAccountToPlayfab.ToString());


        PlayerPrefs.Save();
    }



    /* ------------------------------------------------------------- 
     --------------------------- LOAD ------------------------------
     -------------------------------------------------------------- */


    /// <summary>
    /// Load game data (cloud and local) <br></br>
    /// Decide which file should be used,
    /// if unclear, ask user
    /// </summary>
    public static async void loadGameData() {

        Debug.Log(Application.persistentDataPath);

        try {

            // Local Save is always on
            if (Application.platform == RuntimePlatform.WindowsEditor) {
                saveLocalPreset = Resources.Load("Bayat/Kaloa Save System Settings") as SaveSystemSettingsPreset;
            } else {
                saveLocalPreset = Resources.Load("Bayat/Local Save System Settings") as SaveSystemSettingsPreset;
            }

            // Load back the saved data
            try {
                bool localExists = await existsInSaveSystem("", SaveLocation.local);

                if (localExists) {
                    // Load back the saved data from local storage
                    localSave = await loadWithSaveSystem("", SaveLocation.local);
                    Globals.Game.currentWorld = await loadWorldWithSaveSystem(fileNameSaveGameSubFolderWorld, SaveLocation.local, localSave.currentWorldName);

                    // Set the user temporary, so we don´t have a empty user when the user quits the app
                    // before selecting a game via Popup (if needed)
                    Globals.Game.currentUser = localSave.user;
                } else {
                    localSave = null;
                }
            }
            catch (Exception e) {
                // Error while loading local save
                // -> continue loading cloud save
                Debug.LogWarning(e);
                Debug.LogWarning("Error while loading SaveSystem Local Data. Continue loading cloud save.");
                Globals.UICanvas.DebugLabelAddText("Error while loading SaveSystem Local Data. Continue loading cloud save.");
                localSave = null;
            } 


            if (saveOrLoadPlayfab) {
                // Check if CloadSave exists
                bool cloudExists = await existsInSaveSystem("", SaveLocation.playfab);

                if (cloudExists) {
                    try {
                        // Load back the saved data from playfab
                        cloudSave = await loadWithSaveSystem("", SaveLocation.playfab);
                        Globals.Game.currentWorld = await loadWorldWithSaveSystem(fileNameSaveGameSubFolderWorld, SaveLocation.playfab, cloudSave.currentWorldName);

                        // Set the user temporary, so we don´t have a empty user when the user quits the app
                        // before selecting a game via Popup (if needed)
                        Globals.Game.currentUser = cloudSave.user;
                    }
                    catch (Exception e) {
                        // Error while loading cloud save
                        // -> use local save
                        Debug.LogWarning(e);
                        Debug.LogWarning("Error while loading SaveSystem Cloud Data. Comparing SaveSystem Data.");
                        Globals.UICanvas.DebugLabelAddText("Error while loading SaveSystem Cloud Data. Comparing SaveSystem Data.");
                        cloudSave = null;

                    }
                } else {
                    cloudSave = null;
                }
            }
            
            compareSaveGame(localSave, cloudSave);

        }
        catch (Exception e) {
            Debug.LogWarning(e);
            Debug.LogWarning("No SaveSystem Data found. Creating a new Game");
            PlayFabAccountMngmt.reloadGoogleSave = false;
        }


    }

    /// <summary>
    /// Compares two SaveGames and decides which SaveGame is the best for our User<br></br>
    /// if no "best" can be found, it activates a Pop-Up wich lets the User decide<br></br>
    /// (Only User Values can be compared here, because Worlds is a Monobehaviour and will be overwritten by the last SaveSystemAPI.LoadAsync)
    /// </summary>
    /// <param name="local"></param>
    /// <param name="cloud"></param>
    public static string compareSaveGame(SaveGame local, SaveGame cloud) {
        string result = "";

        if (cloud == null && local == null) {
            // --------- Both Savegames are null, creating new game ---------
            Globals.UICanvas.DebugLabelAddText("No SaveSystem Data found. Creating a new Game");
            Debug.Log("No SaveSystem Data found. Creating a new Game");
            result = "bothNull_newGame";

        } else if (cloud == null && local != null) {
            // --------- Just Cloud Savegame is null, using local ---------
            try {
                // Decide which save should be used
                setSaveGame(SaveLocation.local);

                // Save selected SaveGame local & to Playfab
                if (saveOrLoadPlayfab && !Globals.KaloaSettings.preventPlayfabCommunication) {
                    PlayFabFileSync.UploadFile();
                }

                Globals.UICanvas.DebugLabelAddText("Used Local Save Game");
                result = "cloudNull_localGame";
            }
            catch (Exception e) {
                Crashlytics.SetCustomKey("CrashlyticsSavingSystem", "Error loading local Save, cloud is null: " + e);
                Globals.UICanvas.DebugLabelAddText("Error while loading Local Savegame: " + e);
                result = "cloudNull_error";
            }

        } else if (cloud != null && local == null) {
            // --------- Just Local Savegame is null, using cloud ---------
            try {
                // Decide which save should be used
                setSaveGame(SaveLocation.playfab);

                // Update save file local
                if (!Globals.KaloaSettings.preventSaving) {
                    _ = saveGameData(false);
                }

                Globals.UICanvas.DebugLabelAddText("Used Cloud Save Game");
                result = "localNull_cloudGame";

            } catch (Exception e) {
                Crashlytics.SetCustomKey("CrashlyticsSavingSystem", "Error loading cloud Save, local is null: " + e);
                Globals.UICanvas.DebugLabelAddText("Error while loading Cloud Savegame: " + e);
                result = "localNull_error";
            }

        } else {

            Globals.UICanvas.DebugLabelAddText("Compare local and cloud...");

            try {
                // Compare save games

                if ((local.user.stats.LastDateGameClosed >= cloud.user.stats.LastDateGameClosed)
                    && (local.user.scoreTotalLevels >= cloud.user.scoreTotalLevels)) {
                    /* Add when reactivating prestige 
                    // && (local.user.prestige.prestigeBonus >= cloud.user.prestige.prestigeBonus)
                    // && (local.user.prestige.getPossiblePrestigeBonus() >= cloud.user.prestige.getPossiblePrestigeBonus())
                    */

                    // Use local save because of more levels and last opened
                    setSaveGame(SaveLocation.local);

                    // Save selected SaveGame local & to Playfab
                    if (saveOrLoadPlayfab && !Globals.KaloaSettings.preventPlayfabCommunication) {
                        PlayFabFileSync.UploadFile();
                    }

                    Globals.UICanvas.DebugLabelAddText("Used Local Save Game");
                    Debug.Log("Used Local Save Game");
                    result = "compare_localGame";

                } else if ((local.user.stats.LastDateGameClosed <= cloud.user.stats.LastDateGameClosed)
                  && (local.user.scoreTotalLevels <= cloud.user.scoreTotalLevels)) {
                    /* Add when reactivating prestige 
                    // && (local.user.prestige.prestigeBonus <= cloud.user.prestige.prestigeBonus)
                    // && (local.user.prestige.getPossiblePrestigeBonus() <= cloud.user.prestige.getPossiblePrestigeBonus())
                    */

                    // Use cloud save because of more levels and last opened
                    setSaveGame(SaveLocation.playfab);

                    // Update save file local
                    if (!Globals.KaloaSettings.preventSaving) {
                        _ = saveGameData(false);
                    }

                    Globals.UICanvas.DebugLabelAddText("Used Cloud Save Game");
                    Debug.Log("Used Cloud Save Game");
                    result = "compare_cloudGame";

                } else {
                    // User should decide which game he want to continue
                    // if not sure which one to choose, show popup
                    Globals.UICanvas.DebugLabelAddText("Savegame not clear, user should decide...");
                    fillAndShowPopUp(local, cloud);
                    result = "compare_popUp";

                }
            }
            catch (Exception e) {
                Crashlytics.SetCustomKey("CrashlyticsSavingSystem", "Error while comparing Savegames: " + e);
                Globals.UICanvas.DebugLabelAddText("Error while comparing Savegames: " + e);
                result = "compare_error";
            }
      
        }

        return result;
    }

    /// <summary>
    /// Load saveGame after comparison is done
    /// </summary>
    /// <param name="saveLocation"></param>
    private static async void setSaveGame(SaveLocation saveLocation) {

        // Load back the saved data
        try {
            // Load back the saved data from local storage
            loadedSave = await loadWithSaveSystem("", saveLocation);
            Globals.Game.currentWorld = await loadWorldWithSaveSystem(fileNameSaveGameSubFolderWorld, saveLocation, loadedSave.currentWorldName);
        }
        catch (Exception e) {
            // Error while loading local save
            // -> continue loading cloud save
            Debug.LogWarning(e);
            Debug.LogWarning("No SaveSystem Data found. Creating a new Game");
            loadedSave = new SaveGame();
        }

        Globals.Game.saveGame = loadedSave;
        Globals.Game.currentUser = Globals.Game.saveGame.user;

        try {
            // Load Tutorial Stage
            Globals.Controller.Tutorial.desiredTutorialID = Globals.Game.currentUser.tutorialIDAndStageID.Key;
            Globals.Controller.Tutorial.desiredStageID = Globals.Game.currentUser.tutorialIDAndStageID.Value;
            if (Globals.Controller.Tutorial.isInitialized) {
                Globals.Controller.Tutorial.startGameWithSpecificTutorialStage(Globals.Controller.Tutorial.desiredTutorialID, Globals.Controller.Tutorial.desiredStageID);
            }
        }
        catch (Exception e) {
            // Error while loading local save
            Crashlytics.SetCustomKey("SaveGameEror", "Tutorial Controller wasn't loaded before Savegame!");
            Debug.LogError("Tutorial Controller wasn't loaded before Savegame!" );
            Debug.LogError(e);
        }

        // Finish loading Save
        if (PlayFabAccountMngmt.reloadGoogleSave) {
            Globals.Controller.worldController.changeWorld(Globals.Game.currentWorld.worldName);
            PlayFabAccountMngmt.reloadGoogleSave = false;
        }
    }
  

    /// <summary>
    /// Shows the popup where the user can decide which save game he wants to use
    /// </summary>
    /// <param name="local"></param>
    /// <param name="cloud"></param>
    public static void fillAndShowPopUp(SaveGame local, SaveGame cloud) {

        // Get Components
        TMPro.TextMeshProUGUI local_LastDateGameClosed = Globals.UICanvas.uiElements.SaveGameLocalLastClosed;
        TMPro.TextMeshProUGUI local_scoreTotalLevels = Globals.UICanvas.uiElements.SaveGameLocalScoreTotalLevels;
        TMPro.TextMeshProUGUI local_itemCount = Globals.UICanvas.uiElements.SaveGameLocalItemCount;
        TMPro.TextMeshProUGUI local_PremiumCurrency = Globals.UICanvas.uiElements.SaveGameLocalEmeralds;
        TMPro.TextMeshProUGUI cloud_LastDateGameClosed = Globals.UICanvas.uiElements.SaveGameCloudLastClosed;
        TMPro.TextMeshProUGUI cloud_scoreTotalLevels = Globals.UICanvas.uiElements.SaveGameCloudScoreTotalLevels;
        TMPro.TextMeshProUGUI cloud_itemCount = Globals.UICanvas.uiElements.SaveGameCloudItemCount;
        TMPro.TextMeshProUGUI cloud_PremiumCurrency = Globals.UICanvas.uiElements.SaveGameCloudEmeralds;

        // Fill Local
        local_LastDateGameClosed.text = local.user.stats.LastDateGameClosed.ToString();
        local_scoreTotalLevels.text = local.user.scoreTotalLevels.ToString();
        local_itemCount.text = local.user.inventory.getPropertyList().Count.ToString(); // local.user.prestige.prestigeBonus.ToString();
        local_PremiumCurrency.text = local.user.Emeralds.ToString();

        // Fill Cloud
        cloud_LastDateGameClosed.text = cloud.user.stats.LastDateGameClosed.ToString();
        cloud_scoreTotalLevels.text = cloud.user.scoreTotalLevels.ToString();
        cloud_itemCount.text = cloud.user.inventory.getPropertyList().Count.ToString(); //cloud.user.prestige.prestigeBonus.ToString();
        cloud_PremiumCurrency.text = cloud.user.Emeralds.ToString();

        Color32 green = new Color32(130, 170, 35, 255);

        // Highlight higher values
        if (local.user.stats.LastDateGameOpen > cloud.user.stats.LastDateGameOpen) {
            local_LastDateGameClosed.color = green;
            local_LastDateGameClosed.fontStyle = TMPro.FontStyles.Bold;
        } else if (local.user.stats.LastDateGameOpen < cloud.user.stats.LastDateGameOpen) {
            cloud_LastDateGameClosed.color = green;
            cloud_LastDateGameClosed.fontStyle = TMPro.FontStyles.Bold;
        }

        if (local.user.scoreTotalLevels > cloud.user.scoreTotalLevels) {
            local_scoreTotalLevels.color = green;
            local_scoreTotalLevels.fontStyle = TMPro.FontStyles.Bold;
        } else if (local.user.scoreTotalLevels < cloud.user.scoreTotalLevels) {
            cloud_scoreTotalLevels.color = green;
            cloud_scoreTotalLevels.fontStyle = TMPro.FontStyles.Bold;
        }

        //if (local.user.prestige.prestigeBonus > cloud.user.prestige.prestigeBonus) {
        //    local_itemCount.color = green;
        //    local_itemCount.fontStyle = TMPro.FontStyles.Bold;
        //} else if (local.user.prestige.prestigeBonus < cloud.user.prestige.prestigeBonus) {
        //    cloud_itemCount.color = green;
        //    cloud_itemCount.fontStyle = TMPro.FontStyles.Bold;
        //}

        if (local.user.inventory.getPropertyList().Count > cloud.user.inventory.getPropertyList().Count) {
            local_itemCount.color = green;
            local_itemCount.fontStyle = TMPro.FontStyles.Bold;
        }
        else if (local.user.inventory.getPropertyList().Count < cloud.user.inventory.getPropertyList().Count) {
            cloud_itemCount.color = green;
            cloud_itemCount.fontStyle = TMPro.FontStyles.Bold;
        }

        if (local.user.Emeralds > cloud.user.Emeralds) {
            local_PremiumCurrency.color = green;
            local_PremiumCurrency.fontStyle = TMPro.FontStyles.Bold;
        } else if (local.user.Emeralds < cloud.user.Emeralds) {
            cloud_PremiumCurrency.color = green;
            cloud_PremiumCurrency.fontStyle = TMPro.FontStyles.Bold;
        }

        Globals.UICanvas.uiElements.SaveGamePopUp.SetActive(true);
    }


    /// <summary>
    /// User clicks on Popup button to load local save game
    /// </summary>
    public static void OnButtonClickLocal() {
        setSaveGame(SaveLocation.local);
        // Save selected SaveGame to Playfab
        if (saveOrLoadPlayfab && !Globals.KaloaSettings.preventPlayfabCommunication) {
            PlayFabFileSync.UploadFile();
        }

        Globals.Game.initGame.initGameStartUp(false, System.DateTime.Now, true);

        Globals.UICanvas.uiElements.SaveGamePopUp.SetActive(false);
    }

    /// <summary>
    /// User clicks on Popup button to load cloud save game
    /// </summary>
    public static void OnButtonClickCloud() {
        setSaveGame(SaveLocation.playfab);
        if (!Globals.KaloaSettings.preventSaving) {
            _ = saveGameData(false);
        }
        Globals.Game.initGame.initGameStartUp(false, System.DateTime.Now, true);
        Globals.UICanvas.uiElements.SaveGamePopUp.SetActive(false);
    }

    /// <summary>
    /// Updates the level count since last save <br></br>
    /// Saves the game every <see cref="levelSavingIntervall"/> levelups 
    /// </summary>
    /// <param name="amount"></param>
    public static void updateSaveCount(int amount) {
        levelSinceLastSave += amount;

        // Check if level border is reached for saving
        if(levelSinceLastSave >= levelSavingIntervall) {
            // Save game local and to playfab
            if (!Globals.KaloaSettings.preventSaving) {
                _ = saveGameData();
            }
            // Reset counter
            levelSinceLastSave = 0;
        }
    }


    /// <summary>
    /// Load Google Services
    /// </summary>
    public static void loadGoogleServices() {
        // Init before AdmobServices, else there is a NullReference if Google Communication is prevented
        Globals.Controller.Ads = GameObject.Find("AdWrapper").GetComponent<AdWrapper>();

        if (!Globals.KaloaSettings.preventGoogleCommunication) {
            loadFirebaseServices();
            loadGooglePlayGamesServices();
            loadAdmobServices();
        }

    }

    /// <summary>
    /// Connect the Firebase Services
    /// </summary>
    public static void loadFirebaseServices() {
        try {
            /// -----------------------------------------------------------------
            /// -------------------- Get Google Firebase ----------------------
            /// -----------------------------------------------------------------

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available) {
                    Globals.Controller.Firebase.InitializeFirebase();
                    Globals.UICanvas.DebugLabelAddText("Firebase correctly Initialized");
                } else {
                    Globals.UICanvas.DebugLabelAddText(
                      "Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e.ToString());
        }
        
    }

    /// <summary>
    /// Connect Google Play Games Services
    /// </summary>
    public static void loadGooglePlayGamesServices() {
        try {
            /// -----------------------------------------------------------------
            /// --------------- Get Google Play Games Services -----------------
            /// -----------------------------------------------------------------

            Globals.Controller.GPiOS.getGooglePlayGames();

            // Send initial Level Score
            if (Social.localUser.authenticated) {
                GooglePlayGames.PlayGamesPlatform.Instance.ReportScore(
                    Globals.Game.currentUser.scoreTotalLevels,
                    GPGSIds.leaderboard_mighty_constructors_building_levels,
                    (bool success) => {
                        Debug.Log("(Blooming Earth) Leaderboard update success: " + success);
                    });
            }
        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e.Message);
        }
    }

    /// <summary>
    /// Connect Google Admob Services
    /// </summary>
    public static void loadAdmobServices() {
        try {
            /// -----------------------------------------------------------------
            /// -------------------- Get Google Mobile Ads ----------------------
            /// -----------------------------------------------------------------
            //Globals.UICanvas.DebugLabelAddText("Init Google Mobile Ads");
            Globals.Controller.Ads.initMobileAds();

        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e.Message);
        }
    }



    /// <summary>
    /// Load the Settings the User have done in the SettingsMenu
    /// </summary>
    public static void loadUserSettings() {

        // Get UserSettings from PlayerPrefs
        bool global_settings_hasRated = bool.Parse(PlayerPrefs.GetString(Globals.UniquePlayerPrefs.global_settings_hasRated, "False"));
        bool global_settings_hasSound = bool.Parse(PlayerPrefs.GetString(Globals.UniquePlayerPrefs.global_settings_hasSound, "True"));
        bool global_settings_hasMusic = bool.Parse(PlayerPrefs.GetString(Globals.UniquePlayerPrefs.global_settings_hasMusic, "True"));
        bool global_settings_hasNotifications = bool.Parse(PlayerPrefs.GetString(Globals.UniquePlayerPrefs.global_settings_hasNotifications, "True"));
        bool global_settings_hasVibration = bool.Parse(PlayerPrefs.GetString(Globals.UniquePlayerPrefs.global_settings_hasVibration, "True"));
        bool global_settings_wasSignedIn = bool.Parse(PlayerPrefs.GetString(Globals.UniquePlayerPrefs.global_settings_wasSignedIn, "True"));
        bool global_settings_linkedGoogleAccountToPlayfab = bool.Parse(PlayerPrefs.GetString(Globals.UniquePlayerPrefs.global_settings_linkedGoogleAccountToPlayfab, "False"));

        // Set the Global UserSettings
        Globals.UserSettings.hasRated = global_settings_hasRated;
        Globals.UserSettings.hasSound = global_settings_hasSound;
        Globals.UserSettings.hasMusic = global_settings_hasMusic;
        Globals.UserSettings.hasNotifications = global_settings_hasNotifications;
        Globals.UserSettings.hasVibration = global_settings_hasVibration;
        Globals.Game.currentUser.wasSignedIn = global_settings_wasSignedIn;
        Globals.UserSettings.linkedGoogleAccountToPlayfab = global_settings_linkedGoogleAccountToPlayfab;

    }



    /// <summary>
    /// Loads the necessary UI Elements into our Globals.UICanvas wrapper
    /// </summary>
    public static void loadUIElements() {
        // Set some important things to invisible
        Globals.UICanvas.uiElements.DebugLabelObj.SetActive(false);
    }

    /// <summary>
    /// Loads the Language<br></br>
    /// If Users First Gamestart-> check what Language is Phone has<br></br>
    /// else it should be saved in PlayerPrefs, because User can switch Language like he want<br></br>
    /// if nothing in PlayerPrefs, the FallbackLanguage in LanguageWrapper is used
    /// </summary>
    public static void loadLanguage() {
        string global_settings_language = Language.FALLBACK_LANGUAGE;

        if (Globals.Game.currentUser.stats.FirstGameLoad == new DateTime()) {
            // First Gamestart -> Try to get Language from Mobile Device
            switch (Application.systemLanguage) {
                case SystemLanguage.German:
                    global_settings_language = "de-DE"; break;
                case SystemLanguage.English:
                    global_settings_language = "en-US"; break;
                default:
                    global_settings_language = Language.FALLBACK_LANGUAGE; break;

            }
        } else {
            // Other Gamestarts -> Try to get Language from PlayerPrefs
            //global_settings_language = PlayerPrefs.GetString(Globals.UniquePlayerPrefs.global_settings_language, Language.FALLBACK_LANGUAGE);
            global_settings_language = "en-US";
        }

        // Save to Globals
        Globals.UserSettings.language = global_settings_language;
    }


    /// <summary>
    /// Initiates the Buildings and itsdependencies
    /// </summary>
    /// <param name="cleanUp">if true the Building will be resetted</param>
    public static void loadBuildings(bool cleanUp) {

        try {

            // Load Achievements for Buildings
            Globals.Game.currentWorld.initAchievementsBuildings();

            // Go Over all Buildings and Init them
            foreach (Building building in Globals.Game.currentWorld.buildingsProgressArray) {
                building.loadBuilding();
                building.initIntegratedUIElements();
                building.initUpgradeSlots();

                if (cleanUp) {
                    building.updateLabel();
                    // Cancel all ongoing unlocking processes
                    building.CancelInvoke("checkDelayUnlockProcess");

                    // Reset DelayUnlock-Process Bars
                    building.transform.Find("DelayUnlock-Process/Bar/PercentBarPivot").localScale = new Vector3(1, 1, 0);
                    building.transform.Find("DelayUnlock-Process").gameObject.SetActive(false);
                }
            }

        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e, true);
        }

    }

    /// <summary>
    /// Loads the Referenced Boost, activates the TimeLeft-Boost (if Offline Time didn't consume the whole Boost) <br></br>
    /// and returns the Seconds the Boost ran while Offline
    /// </summary>
    /// <param name="TimeDifferenceGameOff"></param>
    /// <param name="boostTimer"></param>
    /// <param name="multiplierBoostSeconds"></param>
    /// <param name="multiplierBoost"></param>
    public static int loadBoost(TimeSpan TimeDifferenceGameOff, BoostTimer boostTimer) {
        int secondsTimeLeftBoost = boostTimer.getTimeLeftSec() - (int)TimeDifferenceGameOff.TotalSeconds; // Time how much Seconds are left from the Boost after going offline

        int secondsBoostWasActiveWhileOffline = boostTimer.getTimeLeftSec() - secondsTimeLeftBoost; // Time how much Seconds the Boost affected the offline-Coins

        // Globals.UICanvas.DebugLabelAddText(Globals.Game.currentWorld.CoinIncomeManager.boostTimer.isTimerStarted().ToString());
        // Globals.UICanvas.DebugLabelAddText(secondsTimeLeftBoost.ToString());

        // If Timer not Started, start it, with the Last Remaining-Time in PlayerPrefs
        if (secondsTimeLeftBoost > 0) {
            // Load the Boost if seconds left (Boost is reduced by offline Time seconds)
            boostTimer.startBoostTimer();
            boostTimer.reduceTimeLeftSec(secondsBoostWasActiveWhileOffline);
        } else {
            // Reset the Boost to 1x
            boostTimer.stopBoostTimer();
        }

        return secondsBoostWasActiveWhileOffline;
    }


    /// <summary>
    /// processes ad Save the Statistics of the Player
    /// </summary>
    public static void loadAndProcessStatistics(DateTime now) {

        yesterday = now.AddDays(-1);


        // Update Statistics

        // Statistic - Save players starting timestamp (since when he is playing our game)
        if (Globals.Game.currentUser.stats.FirstGameLoad == new DateTime()) {
            if (!Globals.KaloaSettings.preventSaving) {
                Globals.UICanvas.uiElements.PopUpReconnectCloud.SetActive(true);
            }
            Globals.Game.currentUser.stats.FirstGameLoad = now;
        }

        Globals.Game.currentUser.stats.GameOpeningsRaw += 1;

        // Statistic - DaysWithGameOpening
        if (Globals.Game.currentUser.stats.LastDateGameOpen == new DateTime() // if its never been setted
            || Globals.Game.currentUser.stats.LastDateGameOpen.Date != now.Date) {

            // Days with consecutice Game Openings
            if (Globals.Game.currentUser.stats.LastDateGameOpen.Date == yesterday.Date) {
                Globals.Game.currentUser.stats.DaysWithConsecutiveGameOpening += 1;
            } else {
                // reset the Consecutive Days
                Globals.Game.currentUser.stats.DaysWithConsecutiveGameOpening = 1;
            }

            // When LastDateGameOpen changed
            // Raise the DaysWithGameOpening + 1
            Globals.Game.currentUser.stats.DaysWithGameOpening += 1;
        }

        // Update LastDateGameOpen
        Globals.Game.currentUser.stats.LastDateGameOpen = now;


        // Save Everything to PlayerPrefs (to Buffer it if Game-Reset happens)
        savePlayerPrefs(now);

    }


    /// <summary>
    /// loads the currentQuest if necessary
    /// </summary>
    public static void loadQuests(bool deletePlayerPrefs = false) {

        try {
            // Find the Quests component on the currentWorld
            if (Globals.Game.currentWorld.QuestsComponent == null) {
                Globals.Game.currentWorld.QuestsComponent = Globals.Game.currentWorld.gameObject.GetComponent<Quests>();
            }

            if (deletePlayerPrefs) {
                Globals.Game.currentWorld.QuestsComponent.setCurrentQuestState(Quest.QuestStates.NotStarted);
                // Set all Quests in the QuestList to NotStarted
                foreach (Quest aQuest in Globals.Game.currentWorld.QuestsComponent.questList) {
                    aQuest.questState = Quest.QuestStates.NotStarted;
                }
            }

            // Get currentQuestStatus out of the PlayerPrefs
            Globals.Game.currentWorld.QuestsComponent.loadCurrentQuest(true);

        }
        catch (Exception e) {
            if (Globals.Game.currentWorld.QuestsComponent == null) {
                Debug.LogWarning("World " + Globals.Game.currentWorld.worldName + " has no Quests-Component!");
            }
            Debug.LogError(e);
        }
    }


    /* ------------------------------------------------------------- 
     ------------------- Actual Data Read/Write --------------------
     -------------------------------------------------------------- */

    /// <summary>
    /// Saves the SaveGameFile into the desired Place, needs an Object what should be saved
    /// </summary>
    /// <param name="subFolder"></param>
    /// <param name="saveLocation"></param>
    /// <param name="objectToSave"></param>
    /// <returns></returns>
    public static async Task saveWithSaveSystem(string subFolder, SaveLocation saveLocation, object objectToSave) {

        await SaveSystemAPI.SaveAsync(
            getSaveSystemFolder(saveLocation,  subFolder) + getSaveSystemFilename(subFolder),
            objectToSave,
            saveLocalPreset.CustomSettings
            );
    }


    public static async Task<SaveGame> loadWithSaveSystem(string subFolder, SaveLocation saveLocation) {

        return await SaveSystemAPI.LoadAsync<SaveGame>(
                    getSaveSystemFolder(saveLocation, subFolder) + getSaveSystemFilename(subFolder),
                    saveLocalPreset.CustomSettings);
    }

    public static async Task<World> loadWorldWithSaveSystem(string subFolder, SaveLocation saveLocation, string worldName) {
        
        return await SaveSystemAPI.LoadAsync<World>(
                    getSaveSystemFolder(saveLocation, subFolder) + getSaveSystemFilename(subFolder, worldName),
                    saveLocalPreset.CustomSettings);

    }

    public static async Task<bool> existsInSaveSystem(string subFolder, SaveLocation saveLocation) {

        return await SaveSystemAPI.ExistsAsync(
                getSaveSystemFolder(saveLocation, subFolder) + getSaveSystemFilename(subFolder));
    }

    public static bool worldExists(string worldName, SaveLocation saveLocation = SaveLocation.local) {
        return File.Exists(getSaveSystemFolder(saveLocation, fileNameSaveGameSubFolderWorld) + getSaveSystemFilename(fileNameSaveGameSubFolderWorld, worldName));
    }


    public static async Task<StorageDeleteOperationResult> deleteWithSaveSystem(string subFolder, SaveLocation saveLocation) {
        return await SaveSystemAPI.DeleteAsync(
            getSaveSystemFolder(saveLocation, subFolder) + getSaveSystemFilename(subFolder),
            saveLocalPreset.CustomSettings);

    }

    /// <summary>
    /// Get the fileName of a SavingSystem-File. Looks if the File is a Base-File or a World-File, with checking the desired subFolder
    /// </summary>
    /// <param name="saveLocation"></param>
    /// <param name="isWorld"></param>
    /// <returns></returns>
    public static string getSaveSystemFilename(string subFolder, string desiredWorldName = "") {

        if (subFolder == fileNameSaveGameSubFolderWorld) {
            // Subfolder is "Worlds", get the World fileName
            if (desiredWorldName == "") {
                // No desiredWorldName - get fileName from currentWorld
                if (Globals.Game.saveGame.currentWorldName == "") {
                    Globals.Game.saveGame.currentWorldName = Globals.KaloaSettings.defaultWorldName;
                }
                return fileNameSaveGameFileNameWorld + "_" + Globals.Game.saveGame.currentWorldName + fileNameSaveGameFileType;
            } else {
                // get fileName for desired World
                return fileNameSaveGameFileNameWorld + "_" + desiredWorldName + fileNameSaveGameFileType;
            }
        } else {
            // Get base fileName
            return fileNameSaveGameFileName + fileNameSaveGameFileType;
        }
    }


    public static string getSaveSystemFolder(SaveLocation saveLocation, string subFolder) {

        switch (saveLocation) {
            case SaveLocation.local:
            default:
                return fileNameSaveGameRootFolder + fileNameSaveGameFolderLocal + subFolder;

            case SaveLocation.playfab:
                return fileNameSaveGameRootFolder + fileNameSaveGameFolderPlayfab + subFolder;
        }
        
    }

    public static string getRootSaveGameLocation() {
        return Application.persistentDataPath + "/" + SavingSystem.fileNameSaveGameRootFolder;
    }




    /* ------------------------------------------------------------- 
     -------------------- RESET (only Kaloa) -----------------------
     -------------------------------------------------------------- */

    public static async void DeleteGameData() {
        try {
            // Check if Saving Preset is set
            // if not (because saveGameData was never executed)
            // deleteWithSaveSystem will fail
            if(saveLocalPreset != null) {
                // Delete SaveGames
                await deleteWithSaveSystem("", SaveLocation.local);
                await deleteWithSaveSystem("", SaveLocation.playfab);
            }

            // Delete PlayerPref Settings
            PlayerPrefs.DeleteAll();

        } catch (Exception e) {
            Debug.LogWarning("WARNING: Not able to Delete Game Data !" + e);
        }


    }

}
