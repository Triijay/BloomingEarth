using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// InitGame is a Class where funtions can be called, which have to be called only once at the start of the game
/// This is the first script loaded when the game starts
/// </summary>
public class InitGame : MonoBehaviour {

    #region Vars

    /// <summary>
    /// Amount how long the User can get OfflineCoins<br></br>
    /// If maximum is reached, the game is not producing offline coins until he opens the game
    /// </summary>
    [Tooltip("Amount how long the User can get OfflineCoins. If maximum is reached, the game is not producing offline coins until he opens the game")]
    public int limitOfflineCoinsHours = 24;

    /// <summary>
    /// Seconds how long the DelayUnlock-Process goes for the Admins and for Unity Editor
    /// </summary>
    [Tooltip("Seconds how long the DelayUnlock-Process goes for the Admins and for Unity Editor")]
    public int adminUnlockProcessSec = 5;


    /// <summary>
    /// Defines the maximum of seconds how long the Boost of a World can filled with
    /// </summary>
    [Tooltip("Defines the maximum of seconds how long the Boost of a World can filled with")]
    public int boostMultiplier = 2;

    /// <summary>
    /// Defines the maximum of seconds how long the Boost of a World can filled with
    /// </summary>
    [Tooltip("Defines the maximum of seconds how long the Boost of a World can filled with")]
    public int boostSecPerVid = 7200;

    /// <summary>
    /// Defines the maximum of seconds how long the Boost of a World can filled with
    /// </summary>
    [Tooltip("Defines the maximum of seconds how long the Boost of a World can filled with")]
    public int boostMaxSeconds = 36000;

    /// <summary>
    /// Minimum Seconds the User has to be Offline to show the Offline Coins PopUp<br></br>
    /// -> A/B Test later
    /// </summary>
    [Tooltip("Minimum Seconds the User has to be Offline to show the Offline Coins PopUp")]
    public int PopupMinOfflineTime = 60;

    /// <summary>
    /// Test Notification will be sended if true
    /// </summary>
    [Tooltip("Test Notification will be sended if true")]
    public bool testNotificationOn = false;

    /// <summary>
    /// OnApplicationPause und OnApplicationQuit are fired at the same to
    /// to ensure, the things are not done twice, we have this var
    /// </summary>
    bool gameEnding_not_saved = true;

   
    // Performance Optimization
    Language languageWrapper;
    string userID;

    #endregion


    /*-------------------------------------------------------------- 
    ------------------- What happens when ... ----------------------
    ----------------------------------------------------A-----------*/

    private void Awake() {
        // Init the Game on Awake
        Debug.Log("InitGame Awake");

        // Target FPS
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;


        if (Globals.Game.isGameStarting || Globals.Game.isSceneChanging) {
            initGameStartUp(false, DateTime.Now);
            Globals.Game.isGameStarting = false;
            Globals.Game.isSceneChanging = false;
        } else {
            initGameStartUp(false, DateTime.Now, true);
        }
    }

    /// <summary>
    /// On Application Quit Save the important things
    /// </summary>
    void OnApplicationQuit() {
        try {
            if (gameEnding_not_saved) {
                if (!Globals.KaloaSettings.preventSaving) {
                    _ = SavingSystem.saveGameData(false); 
                }
                Globals.Game.currentWorld.CoinIncomeManager.stopProduceIncome();
                gameEnding_not_saved = false;
            }
        } catch { }
        NotificationSystem.sendNotifications();
    }


    /// <summary>
    /// On Application Destroy Save the important things
    /// </summary>
    void OnDestroy() {

    }


    /// <summary>
    /// If the Game is paused, the important things are saved, because in paused game, there is no production
    /// Android: You can kill the game, if you click on the Phones Button for "All running apps", that causes the OnApplicationPause(true)
    /// Android: If you start the game initially, Awake() is triggered and then OnApplicationPause(false) is triggered too
    /// Iphone: No Clue
    /// </summary>
    private void OnApplicationPause(bool pause) {
        if (pause) {
            if (gameEnding_not_saved) {
                if (!Globals.KaloaSettings.preventSaving) {
                    _ = SavingSystem.saveGameData(false);
                }
                Globals.Game.currentWorld.CoinIncomeManager.stopProduceIncome();
                gameEnding_not_saved = false;
            }
            Globals.HelperFunctions.initialGameStart = false;

            NotificationSystem.sendNotifications();

        } else if (!Globals.HelperFunctions.initialGameStart) {
            gameEnding_not_saved = true;
            NotificationSystem.initNotifications();
            OfflineCoins.initOfflineCoins(true, DateTime.Now);
            Globals.Game.currentWorld.CoinIncomeManager.startProduceIncome();
        }
    }



    /*-------------------------------------------------------------- 
     ---------------------- Init Functions -------------------------
     ---------------------------------------------------------------*/


    /// <summary>
    /// Initialize the Game<br></br>
    /// <param name="deletePlayerPrefs">param deletePlayerPrefs: if true, the game will be resetted completely</param>
    /// </summary>
    public void initGameStartUp(bool deletePlayerPrefs, DateTime CurrentTime, bool reload = false) {
              
        
        
        // ** IMPORTANT:  BE CAREFUL if you want to change the order of the functions in initGameStartUp, there are various dependencies in that order ! ** 
       
        
        
        try {

            //-- Basics

            Globals.UICanvas.uiElements = GameObject.Find("CanvasMain").GetComponent<UIElements>();
            Globals.UICanvas.translatedTMProElements = Globals.UICanvas.uiElements.gameObject.GetComponent<TranslatedElements>();



            //-- CreativeCenter
            CreativeCenter CreativeCenter = Globals.UICanvas.uiElements.CreativeCenter.GetComponent<CreativeCenter>();
            if (CreativeCenter.letsBeCreative) {
                // We want to make Creatives
                CreativeCenter.gameObject.SetActive(true);
            } else {
                // We dont want to make Creatives
                CreativeCenter.gameObject.SetActive(false);
                CreativeCenter.enabled = false;
            }


            if (!reload) {
                Globals.Game.initGame = this;
                // Load UI Elements
                SavingSystem.loadUIElements();
                Globals.HelperFunctions.enableGoogleAdButtonsInEditor();

                // Load Tutorial Controller
                try {
                    Crashlytics.SetCustomKey("CrashlyticsInitGame", "Load Tutorial Controller");
                    Globals.Controller.Tutorial = GameObject.Find("TutorialManager").GetComponent<TutorialController>();
                }
                catch (Exception e) {
                    Globals.UICanvas.DebugLabelAddText("InitGameBasics TutorialLoading Failed--" + e.ToString(), true);
                }
            }

            // KaloaSettings.logBufferLoadingScreen
            Globals.UICanvas.DebugLabelAddText("-- Debug in LoadingScreen:");
            Globals.UICanvas.DebugLabelAddText(Globals.KaloaSettings.logBufferLoadingScreen);
            Globals.KaloaSettings.logBufferLoadingScreen = "";
            Globals.UICanvas.DebugLabelAddText("---------------------------");

            Debug.Log(Globals.Controller.Firebase.getUserID());

            // Update Emerald Display
            Globals.Game.currentUser.Emeralds = Globals.Game.currentUser.Emeralds;

        }
        catch (Exception e) {
            Crashlytics.SetCustomKey("CrashlyticsInitGameError", "Basics");
            Globals.UICanvas.DebugLabelAddText("CrashlyticsInitGameError -- Basics \n" + e, true);
        }
        
        
        try {

            //-- Services

            if (!reload) {

                Crashlytics.SetCustomKey("CrashlyticsInitGame", "Connect Services");

                if (Application.platform == RuntimePlatform.WindowsEditor && Globals.Game.saveGame == null) {
                    // Load Data when starting from our MainScene directly per Unity
                    LoadingScreen.connectServices();
                } else {
                    // We check if all services are initialized from Loading Screen
                    // if not we need to reload
                    checkServiceInitStatus();
                }

                // Check the AdButton-Stati
                AdWrapper.checkButtonLoadingStatus();
                DontDestroyOnLoad(Globals.Controller.Ads);



                // Load Data when LoadingScreen was successful
                if (!deletePlayerPrefs) {
                    Crashlytics.SetCustomKey("CrashlyticsInitGame", "Load Game Data");
                    SavingSystem.loadGameData();                
                }

                // Load InventoryItems
                try {
                    Crashlytics.SetCustomKey("CrashlyticsInitGame", "Init Items");
                    Globals.Game.currentUser.inventory.InitItemTemplates();
                    Globals.Game.currentUser.inventory.InitProperties();
                }
                catch (Exception e) {
                    Globals.UICanvas.DebugLabelAddText("InitGame2 ItemLoading Failed--" + e.ToString(), true);
                }

                // Check Adm Sign In
                Crashlytics.SetCustomKey("CrashlyticsInitGame", "AdmUserSignIn");
                Globals.Controller.GPiOS.checkAndExecuteAdmUserSignIn();


                // Delete all PlayerPref Data before everything is loaded, so the standard values will be used
                if (deletePlayerPrefs) {
                    PlayerPrefs.DeleteAll();
                }

            }


        }
        catch (Exception e) {
            Crashlytics.SetCustomKey("CrashlyticsInitGameError", "Services");
            Globals.UICanvas.DebugLabelAddText("CrashlyticsInitGameError -- Services \n" + e, true);
        }



        try {

            //-- Worlds

            // Get last World loaded from SaveSystem (Default "World1")
            Crashlytics.SetCustomKey("CrashlyticsInitGame", "Set Current World");
            Globals.Game.getWorldBuildings();

            Globals.Game.saveGame.currentWorldName = Globals.Game.currentWorld.worldName;
            Debug.Log("InitGame: Loading " + Globals.Game.currentWorld.worldName);

            

            // if Reset -> Close SettingsMenu
            if (deletePlayerPrefs) {
                Globals.UICanvas.uiElements.SettingsMenu.SetActive(false);
                Globals.UICanvas.uiElements.PopUpBG.SetActive(false);
            }

            // Load some InitGame Variables
            Globals.KaloaSettings.boostMultiplier = boostMultiplier;
            Globals.KaloaSettings.boostSecPerVid = boostSecPerVid;

        }
        catch (Exception e) {
            Crashlytics.SetCustomKey("CrashlyticsInitGameError", "Worlds");
            Globals.UICanvas.DebugLabelAddText("CrashlyticsInitGameError -- Worlds \n" + e, true);
        }



        try {

            //-- Modules

            if (!reload) {

                // Load World Controller
                try {
                    Crashlytics.SetCustomKey("CrashlyticsInitGame", "Load WorldController");
                    Globals.Controller.worldController = GameObject.Find("WorldController").GetComponent<WorldController>();
                }
                catch (Exception e) {
                    Globals.UICanvas.DebugLabelAddText("InitGame2 WorldController Failed--" + e.ToString(), true);
                }

                // Load Language / Locale
                try {
                    Crashlytics.SetCustomKey("CrashlyticsInitGame", "Load Language");
                    SavingSystem.loadLanguage();
                    Globals.Controller.Language = languageWrapper = new Language(Globals.UserSettings.language);
                } catch (Exception e) { 
                    Globals.UICanvas.DebugLabelAddText("InitGame2 LanguageLoading Failed--" + e.ToString(), true); 
                }

                // Load Sounds
                try {
                    Crashlytics.SetCustomKey("CrashlyticsInitGame", "Load Sounds");
                    Globals.Controller.Sound = Globals.UICanvas.uiElements.Sounds.GetComponent<Sound>();
                    Globals.Controller.Sound.InitAudioFiles();
                }
                catch (Exception e) {
                    Globals.UICanvas.DebugLabelAddText("InitGame2 SoundLoading Failed--" + e.ToString(), true);
                }

                // Load IAP System
                try {
                    Crashlytics.SetCustomKey("CrashlyticsInitGame", "Load IAP");
                    Globals.Controller.IAP = gameObject.GetComponent<IAPWrapper>();
                }
                catch (Exception e) {
                    Globals.UICanvas.DebugLabelAddText("InitGame2 ItemLoading Failed--" + e.ToString(), true);
                }


            }

            // Initialize Prestige
            Globals.Game.currentUser.prestige = Globals.UICanvas.uiElements.InitGameObject.GetComponent<Prestige>();
            //Globals.Game.currentUser.prestige.loadPrestige();
            //Globals.Game.currentUser.prestige.checkButtonState();
            Globals.Game.currentUser.prestige.ResetPrestigeValuesForAdmins();

        }
        catch (Exception e) {
            Crashlytics.SetCustomKey("CrashlyticsInitGameError", "Modules");
            Globals.UICanvas.DebugLabelAddText("CrashlyticsInitGameError -- Modules \n" + e, true);
        }



        try {
            //-- Quests

            // Load Quests
            Crashlytics.SetCustomKey("CrashlyticsInitGame", "Load Quests");
            SavingSystem.loadQuests(deletePlayerPrefs);

        }
        catch (Exception e) {
            Crashlytics.SetCustomKey("CrashlyticsInitGameError", "Quests");
            Globals.UICanvas.DebugLabelAddText("CrashlyticsInitGameError -- Quests \n" + e, true);
        }



        try {

            //-- Coins

            // Init CoinIncome
            Crashlytics.SetCustomKey("CrashlyticsInitGame", "Load CoinIncome");
            Globals.Game.currentWorld.CoinIncomeManager = Globals.Game.currentWorld.gameObject.GetComponent<CoinIncome>();
            Globals.Game.currentWorld.CoinIncomeManager.InitCoinIncome();
            Globals.Game.currentWorld.CoinIncomeManager.startProduceIncome();
            Globals.UICanvas.uiElements.PopUps.GetComponent<CoinsPerSecond>().InitCoinsPerSecondsPopup();

            // Init Boosts
            Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.initBoostTimer();
            Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer.initBoostTimer();

        } catch (Exception e) {
            Crashlytics.SetCustomKey("CrashlyticsInitGameError", "Coins");
            Globals.UICanvas.DebugLabelAddText("CrashlyticsInitGameError -- Coins \n" + e, true);
        }
 


        try {

            //-- Envi
            Globals.Game.currentWorld.enviGlass = new EnviGlass();

            if (deletePlayerPrefs) {
                // Admin wants to reset his Game
                Globals.Game.currentWorld.enviGlass.updateEnviPopUp();
                Globals.Game.currentWorld.setCoins(new IdleNum(10));
                Globals.Game.currentWorld.CoinIncomeManager.reset();
                Globals.Game.currentUser.scoreTotalLevels = 0;
                // Reset all LevelUpExtensions
                Globals.HelperFunctions.resetLevelUpExtensions();
                // Reset Quests
                if (Globals.Game.currentWorld.QuestsComponent != null) {
                    Globals.Game.currentWorld.QuestsComponent.resetQuests();
                }
            }

            // Edit and Save Statistics of Player
            SavingSystem.loadAndProcessStatistics(CurrentTime);


            /// -----------------------------------------------------------------
            /// -------------Get the basic Components for the game---------------
            /// -----------------------------------------------------------------

            if (!reload) {

                // BoostPopUp-Texts
                //Globals.UICanvas.uiElements.BoostPopUpTextRemaining.text = "0s";

                // Load the LevelUpAmount Sliders last value
                //Globals.UICanvas.uiElements.LevelUpAmountSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(Globals.UniquePlayerPrefs.global_1tenMax, 1));

                // Add a Listener and check if the value had changed
                Globals.UICanvas.uiElements.LevelUpAmountSlider.onValueChanged.AddListener(delegate { Globals.HelperFunctions.SliderValueChanged(); });

                // Make sure the Globals.UICanvas.uiElements.BuildingMenuButtonMinus30 is deactivated
                Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.SetActive(false);

                // Get InitGame Settings
                Globals.KaloaSettings.adminUnlockProcessSec = adminUnlockProcessSec;

                // Notifications will be cancelled and initiated
                NotificationSystem.initNotifications();

            }

        }
        catch (Exception e) {
            Crashlytics.SetCustomKey("CrashlyticsInitGameError", "Envi");
            Globals.UICanvas.DebugLabelAddText("CrashlyticsInitGameError -- Envi \n" + e, true);
        }



        try {

            //-- Buildings and Offline Coins


            // Initiates the Buildings
            SavingSystem.loadBuildings(deletePlayerPrefs);

            // Init OfflineCoins
            if (deletePlayerPrefs) {
                OfflineCoins.initOfflineCoins(false, CurrentTime);
                if (Globals.Game.currentWorld.CoinIncomeManager != null) {
                    Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.stopBoostTimer();
                    Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer.stopBoostTimer();
                }
            } else {
                OfflineCoins.initOfflineCoins(true, CurrentTime);
            }

            // Update the Building Progress
            Globals.Game.currentWorld.checkNextBuildingProgress();

        }
        catch (Exception e) {
            Crashlytics.SetCustomKey("CrashlyticsInitGameError", "Buildings and Offline Coins");
            Globals.UICanvas.DebugLabelAddText("CrashlyticsInitGameError -- Buildings and Offline Coins \n" + e, true);
        }



        try {

            //-- Various


            // Check Level Indicators once
            Globals.Game.currentWorld.checkLevelUpIndicatorsForced();
            // Invoke Check Level Indicators
            StartCoroutine(Globals.HelperFunctions.checkLevelIndicatorsForwardFunction());

            // Update the Prestige Levels
            //Globals.Game.currentUser.prestige.updatePrestigeLevels();

            // Test for Items
            if (false && Globals.Game.currentUser.inventory.getPropertyList().Count == 0) {
                Globals.Game.currentUser.inventory.createProperty("SolarSystem");
                Globals.Game.currentUser.inventory.createProperty("PelletHeating");
                Globals.Game.currentUser.inventory.createProperty("IndustrialOven");
                Globals.Game.currentUser.inventory.createProperty("TesterBuildingUpgrades");
                Globals.Game.currentUser.inventory.createProperty("TesterOnlyForHouse");
                Globals.Game.currentUser.inventory.createProperty("SolarSystem");
            }

        } catch (Exception e) {
            Crashlytics.SetCustomKey("CrashlyticsInitGameError", "Various");
            Globals.UICanvas.DebugLabelAddText("CrashlyticsInitGameError -- Various \n" + e, true);
        }


    }





    private void checkServiceInitStatus() {
        if (!LoadingScreen.everythingInitialized) {
            // Init before AdmobServices, else there is a NullReference if Google Communication is prevented
            Globals.Controller.Ads = GameObject.Find("AdWrapper").GetComponent<AdWrapper>();

            if (!Globals.KaloaSettings.preventGoogleCommunication) {
                // Check status of each service and try reconnecting
                if (!LoadingScreen.googleServicesInitialized) {
                    // Connect Google Play Games
                    SavingSystem.loadGooglePlayGamesServices();
                }

                // Connect Firebase
                SavingSystem.loadFirebaseServices();

                if (!LoadingScreen.admobInitialized) {
                    // Connect Admob
                    SavingSystem.loadAdmobServices();
                }
            }

            if (!LoadingScreen.playFabInitialized) {
                // Connect Playfab
                if (!Globals.KaloaSettings.preventPlayfabCommunication) {
                    PlayFabAccountMngmt.connectPlayFab();
                }
            }
        }
    }

    /// <summary>
    /// If LoadingScreen does not fade out correctly, this is the Backup whats hide it completely
    /// </summary>
    /// <returns></returns>
    public IEnumerator fadeOutLoadingScreenCrossfadeBackup() {

        yield return new WaitForSeconds(5);

        GameObject.Find("LoadingScreenCrossfade").SetActive(false);

        yield return null;
    }

    /// <summary>
    /// SHOULD ONLY BE CALLED BY ADMINS/Tests - NOT FOR USERS (They would lose buyed Items and Emeralds)
    /// </summary>
    /// <param name="loadGoogleServices"></param>
    public void resetGameForAdmins() {
        Globals.Game.currentUser.prestige.usualResetGame();

        // Stop the current running Coroutines
        Globals.HelperFunctions.stopAllRunningCoroutines();

        // Delete User without deleting his Buying List
        List<IAPBuyedItem> tempList = Globals.Game.currentUser.buyedItemsList;// Save Buyed Items from User
        Globals.Game.currentUser = new User(); // delete User Information
        Globals.Game.currentUser.buyedItemsList = tempList;


        //Globals.Game.currentUser.prestige.ResetPrestigeValuesForAdmins();
        SavingSystem.DeleteGameData();

        // Activate HUD Buttons
        UIElements.setHUDVisibility(true);


        // Change to World 1
        if(Globals.Game.currentWorld.worldName != Globals.KaloaSettings.defaultWorldName) {
            Globals.Controller.worldController.changeWorld(Globals.KaloaSettings.defaultWorldName);
        }

        // Restart the Game
        initGameStartUp(true, DateTime.Now, false);

    }
}
