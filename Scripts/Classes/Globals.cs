using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Global Custom Attribute to mark methods for the Custom Editor
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Method)]
public class ExposeMethodInEditorAttribute : System.Attribute { }



/// <summary>
/// Global reachable Classes
/// </summary>
namespace Globals {

    /// <summary>
    /// Overarching Game Settings
    /// </summary>
    public static class Game {

        public static InitGame initGame;

        /// <summary>
        /// Says, if the Game is just starting (true at start, after it will alway be false)
        /// </summary>
        public static bool isGameStarting = true;

        /// <summary>
        /// Says, if a new Scene is loades (false at start, after click on world change, it will be true)
        /// </summary>
        public static bool isSceneChanging = true;

        /// <summary>
        /// SaveGame of Blooming Earth
        /// </summary>
        public static SaveGame saveGame;

        /// <summary>
        /// Quick Reference to the current loaded World
        /// </summary>
        public static World currentWorld;

        /// <summary>
        /// Represents the User with his ID, Stats etc
        /// </summary>
        public static User currentUser = new User();

        public static void getWorldBuildings() {

            // Get the Worlds Component from the game
            GameObject world = GameObject.Find("World");

            World worldComp = world.gameObject.GetComponent<World>();

            // Get all buildings from this world
            Building[] buildings = world.GetComponentsInChildren<Building>(true);
            // Save the Progress-Order of the Buildings by their baseCost
            buildings = World.SortBuildingsByBaseCost(buildings);

            // Save sorted Building into the world Array
            worldComp.buildingsProgressArray = buildings;

            // Set currentWorld
            currentWorld = worldComp;
            currentWorld.isCurrentWorld = true;
            currentWorld.gameObject.SetActive(true);

        }
    }


    /// <summary>
    /// All Playerpref Strings used in the game
    /// </summary>
    public static class UniquePlayerPrefs {
        // User Settings
        public const string global_settings_hasRated = "global_settings_hasRated";
        public const string global_settings_hasSound = "global_settings_hasSound";
        public const string global_settings_hasMusic = "global_settings_hasMusic";
        public const string global_settings_hasNotifications = "global_settings_hasNotifications";
        public const string global_settings_hasVibration = "global_settings_hasVibration";
        public const string global_settings_wasSignedIn = "global_settings_wasSignedIn";
        public const string global_settings_language = "global_settings_language";
        public const string global_1tenMax = "global_1tenMax";
        public const string global_settings_linkedGoogleAccountToPlayfab = "global_settings_linkedGoogleAccountToPlayfab";

    }


    /// <summary>
    /// User Settings of the Game
    /// </summary>
    public static class UserSettings {

        /// <summary>
        /// Menu: What language had the User setted
        /// </summary>
        public static string language;

        /// <summary>
        /// Menu: Has the User rated our game yet?
        /// </summary>
        public static bool hasRated = false;

        /// <summary>
        /// Menu: Has the user activated sounds?
        /// </summary>
        public static bool hasSound = true;

        /// <summary>
        /// Menu: Has the user activated the background music?
        /// </summary>
        public static bool hasMusic = true;

        /// <summary>
        /// Menu: Has the user activated the Notifications?
        /// </summary>
        public static bool hasNotifications = true;

        /// <summary>
        /// Menu: Has the user activated the Vibration Function?
        /// </summary>
        public static bool hasVibration = true;

        /// <summary>
        /// Documents if the User linked his Google Account to Playfab <br></br>
        /// Neccessary to decide how the user should login to Playfab
        /// </summary>
        public static bool linkedGoogleAccountToPlayfab = false;

    }


    /// <summary>
    /// Wrapper/Controller Classes
    /// </summary>
    public static class Controller {
        /// <summary>
        /// Wrapper for Google Play Services and iOS Services
        /// </summary>
        public static GPiOSWrapper GPiOS = new GPiOSWrapper();

        /// <summary>
        /// Wrapper for Firebase
        /// </summary>
        public static FirebaseWrapper Firebase = new FirebaseWrapper();

        /// <summary>
        /// Wrapper for Firebase
        /// </summary>
        public static IAPWrapper IAP;

        /// <summary>
        /// Wrapper for Google AdMob Services
        /// </summary>
        public static AdWrapper Ads;

        /// <summary>
        /// Wrapper for Language Strings
        /// </summary>
        public static Language Language;

        /// <summary>
        /// Wrapper for Sounds
        /// </summary>
        public static Sound Sound;

        /// <summary>
        /// Controller for the Tutorial
        /// </summary>
        public static TutorialController Tutorial;

        /// <summary>
        /// Controller for the Tutorial
        /// </summary>
        public static WorldController worldController;

    }


    /// <summary>
    /// User Settings of the Game
    /// </summary>
    public static class KaloaSettings {

        /// <summary>
        /// public variable from InitGame
        /// </summary>
        public static int adminUnlockProcessSec;

        /// <summary>
        /// boostMultiplier from InitGame Object
        /// </summary>
        public static int boostMultiplier;

        /// <summary>
        /// boostSecPerVid from InitGame Object
        /// </summary>
        public static int boostSecPerVid;

        /// <summary>
        /// Some Tools are better to debug: Currently:<br></br>
        /// Catch The Nuts, 
        /// </summary>
        public static bool debugMode = false;

        /// <summary>
        /// Test Ads will be loaded if true
        /// </summary>
        public static bool preventRealAds = false;

        /// <summary>
        /// Playfab is not going to be addressed if true,
        /// Used in tests
        /// </summary>
        public static bool preventPlayfabCommunication = false;

        /// <summary>
        /// Game will not save if true
        /// Used in tests
        /// </summary>
        public static bool preventSaving = false;

        /// <summary>
        /// Shop is not going to be addressed if true,
        /// Used in tests
        /// </summary>
        public static bool preventIAPCommunication = false;

        /// <summary>
        /// Google is not going to be addressed if true,
        /// Used in tests
        /// </summary>
        public static bool preventGoogleCommunication = false;

        /// <summary>
        /// Playing the Ambient Music in Windows if true,
        /// Used in tests
        /// </summary>
        public static bool playAmbientMusicInEditor = false;

        /// <summary>
        /// Test the Tutorial -> most of the Times we dont want the Tutorial on at developing
        /// </summary>
        public static bool skipTutorial = false;

        /// <summary>
        /// To Debug LoadingScreen
        /// </summary>
        public static string logBufferLoadingScreen = "";

        /// <summary>
        /// 
        /// </summary>
        public static string defaultWorldName = "Village1";

        /// <summary>
        /// enum for different AdTypes
        /// </summary>
        public enum AdType {
            Boost = 0,
            OfflineCoins = 1,
            ForwardBuildingProgress = 2
        }


        public const string linkPlayStore = "https://play.google.com/store/apps/details?id=com.KaloaGames.BloomingEarth";

        public const string linkInstagram = "https://www.instagram.com/blooming_earth_idle";

        public const string linkFacebook = "https://www.facebook.com/bloomingearthidle";

        public const string linkWebsite = "https://www.kaloagames.com/";

        public const string linkWebsitePrivacy = "https://www.kaloagames.com/privacy-policy-games";

        public const string linkWebsiteTOS = "https://www.kaloagames.com/terms-conditions";

        public const string linkWebsiteImprint = "https://www.kaloagames.com/imprint";

        public const string linkEmailInfo = "info@kaloagames.com";



        /// <summary>
        /// Downgrades the Quality Settings, if nessecary<br></br>
        /// BE SURE THAT THE HIGHEST QUALITY SETTING IS SET AS DEFAULT IN: Edit > Project Settings > Quality
        /// </summary>
        public static void setQualitySettings() {

            // Generally choose high Quality
            QualitySettings.SetQualityLevel(2);

            if (SystemInfo.systemMemorySize <= 2048 || SystemInfo.graphicsMemorySize < 1024 || SystemInfo.graphicsMemorySize < 1536) {
                // If Graphics Memory Size is not really high, lower the Quality Setting
                QualitySettings.SetQualityLevel(1);
            }
            if (!SystemInfo.supportsInstancing || SystemInfo.graphicsMemorySize < 512 || SystemInfo.systemMemorySize <= 1024) {
                // If our Mats/Shadows cannot be instanced, set the Quality UltraLow
                QualitySettings.SetQualityLevel(0);
            }
        }

    }


    /// <summary>
    /// UICanvas<br></br>
    /// UI to access from anywhere 
    /// </summary>
    public static class UICanvas {

        public static UIElements uiElements;
        public static TranslatedElements translatedTMProElements;

        /// <summary>
        ///  Adds a Log to the Build-DebugLabel
        /// </summary>
        /// <param name="anything"></param>
        /// <param name="isCritical"></param>
        public static void DebugLabelAddText(object anything, bool isCritical = false) {
            if (anything != null) {

                if (isCritical) {
                    KaloaSettings.logBufferLoadingScreen += "\n" + "CRITICAL:" + "\n";
                }

                if (isCritical) {
                    Debug.LogError(anything.ToString() + "\n");
                } else {
                    Debug.Log(anything.ToString() + "\n");
                }
                if (uiElements.DebugLabelText != null) {
                    uiElements.DebugLabelText.text += anything.ToString() + "\n";
                } else {
                    KaloaSettings.logBufferLoadingScreen += anything.ToString() + "\n";
                }

                if (isCritical) {
                    KaloaSettings.logBufferLoadingScreen += "--END CRITICAL:" + "\n";
                }
            } else {
                Debug.LogError("NULL\n");
                uiElements.DebugLabelText.text += "NULL\n";
            }
        }

    }



    /// <summary>
    /// Some Helper Functions and Vars - used in different scenarios
    /// </summary>
    public static class HelperFunctions {

        /// <summary>
        /// Indicates wether the Game was started initially or was only paused
        /// </summary>
        public static bool initialGameStart = true;

        //Permformance Optimization
        static Material mat;

        /// <summary>
        /// Shows if the Player has already claimed his OfflineCoins
        /// </summary>
        [HideInInspector]
        public static bool claimedOfflineCoins = false;

        /// <summary>
        /// Swap the Materials of a Building from the first to the second Color
        /// </summary>
        /// <param name="toChange"></param>
        /// <param name="target"></param>
        public static void swapMaterials(GameObject targetObject, Material toChange, Material target) {
            foreach (Renderer rend in targetObject.GetComponentsInChildren<Renderer>(true)) {
                for (var j = 0; j < rend.materials.Length; j++) {
                    // If Rendering Mode of the Material == Opaque
                    mat = rend.materials[j];
                    if (mat.HasProperty("_Color") && mat.color == toChange.color) {
                        mat.SetColor("_Color", target.color);
                    }
                }
            }
        }

        public static void resetLevelUpExtensions() {
            // Reset CurrentWorld
            foreach (Building building in Game.currentWorld.buildingsProgressArray) {
                LevelUpExtension[] extensions = building.gameObject.GetComponentsInChildren<LevelUpExtension>(true);
                foreach (LevelUpExtension ext in extensions) {
                    ext.setExecuted(false);
                }
            }
        }

        /// <summary>
        /// A Listener for waiting for the User to Press an OfflineCoin-PopUp Button and add the Coins to his BankAccount
        /// </summary>
        /// <param name="OfflineCoins"></param>
        /// <param name="vidShown"></param>
        public static void popUpButtonListener(IdleNum OfflineCoins, bool vidShown) {

            // Debug Log
            string coinsBefore = Game.currentWorld.getCoins().toRoundedString();

            if (!claimedOfflineCoins) {
                if (vidShown) {
                    if (Application.platform == RuntimePlatform.WindowsEditor) {
                        Globals.Controller.Ads.setRewardingOfflineCoinAmount(OfflineCoins);
                        Globals.Controller.Ads.HandleUserEarnedOfflineCoinsReward(null, null);
                        Globals.Controller.Ads.HandleRewardedOfflineCoinsAdClosed(null, null);
                    } else {
                        // Start Video
                        Globals.Controller.Ads.ShowRewardedAd(Globals.KaloaSettings.AdType.OfflineCoins, OfflineCoins);
                    }

                    /* DEBUG */
                    Debug.Log("InitGame: User choosed double-OfflineCoins -- CurrentWorld coins before: " + coinsBefore +
                        " -- Will add " + OfflineCoins.toRoundedString() + " coins if Video Reward is fired.");
                } else {
                    // Add coins
                    Globals.UICanvas.uiElements.ParticleEffects.addCoins(OfflineCoins);

                    /* DEBUG */
                    Debug.Log("InitGame: User choosed single-OfflineCoins -- CurrentWorld coins before: " + coinsBefore +
                        " -- Added " + OfflineCoins.toRoundedString() + " coins." +
                        " -- CurrentWorld coins now: " + Game.currentWorld.getCoins().toRoundedString());
                }

                setOfflineCoinsClaimed();
            }

            // Hide popup
            Globals.UICanvas.uiElements.PopUpOfflineCoins.SetActive(false);
            Globals.UICanvas.uiElements.PopUpBG.SetActive(false);

            // TODO Play Reward Sound
            Globals.Controller.Sound.PlaySound("CoinReward");

            // Show Feedback PopUp after X Gamestarts
            ButtonScripts.checkifToActivateFeedbackPopUp();

        }

        /// <summary>
        /// Invoked when the value of the LevelUpAmount slider changes.
        /// </summary>
        public static void SliderValueChanged() {
            Game.currentWorld.checkLevelUpIndicatorsForced();

            // Save the LevelUpAmount Sliders  value
            PlayerPrefs.SetFloat(Globals.UniquePlayerPrefs.global_1tenMax, Globals.UICanvas.uiElements.LevelUpAmountSlider.value);
        }

        /// <summary>
        /// Update the LevelIndicators, but only if its not already upgradable (Performance)
        /// </summary>
        public static IEnumerator checkLevelIndicatorsForwardFunction() {

            while (true) {
                yield return new WaitForSeconds(0.3f);
                Game.currentWorld.checkLevelIndicators();
            }

        }


        /// <summary>
        ///  Enables the Buttons using Google Ads, they should only be enabled in UnityEditor or when its Ad is Loaded
        /// </summary>
        public static void enableGoogleAdButtonsInEditor() {
            if (Application.platform == RuntimePlatform.WindowsEditor) {
                AdWrapper.setButtonLoadingStatus(false, Globals.UICanvas.uiElements.PopUpOfflineCoinsAdButtonX2, false);
                AdWrapper.setButtonLoadingStatus(false, Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30, false);
                AdWrapper.setButtonLoadingStatus(false, Globals.UICanvas.uiElements.BoostPopUpAdButtonInvokeBoost, false);
            }
        }


        /// <summary>
        /// Save Last Offline Coin Claim Time (prevents doubling when game crashes after this)
        /// </summary>
        public static void setOfflineCoinsClaimed() {
            Globals.Game.currentUser.stats.LastOfflineCoinsClaimed = System.DateTime.Now;
            claimedOfflineCoins = true;
        }


        public static void stopAllRunningCoroutines() {
            Game.currentWorld.CoinIncomeManager.StopAllCoroutines();
            UICanvas.uiElements.MainCamera.GetComponent<CameraControls>().StopAllCoroutines();
        }

        public static TimeSpan getGameOfflineTime(DateTime CurrentTime) {
            // Get Time difference for GameOffCoins
            DateTime LastApplicationQuit = new DateTime();

            if (Globals.Game.currentUser.stats.LastOfflineCoinsClaimed != new DateTime()) {
                LastApplicationQuit = Globals.Game.currentUser.stats.LastOfflineCoinsClaimed;
            } else {
                // Happens only at first game start
                LastApplicationQuit = CurrentTime;
                Globals.HelperFunctions.setOfflineCoinsClaimed();
            }


            // Calculate the Game Off Time
            TimeSpan TimeDifferenceGameOff = CurrentTime - LastApplicationQuit;

            Debug.Log("Globals: Last Quit: " + LastApplicationQuit);
            Debug.Log("Globals: Game-Off Time: " + TimeDifferenceGameOff);

            return TimeDifferenceGameOff;
        }

    }

}
