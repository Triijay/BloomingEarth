using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    #region Vars
    public static LoadingScreen screen;
    public CanvasGroup LoadingSceneLoadingCircle;
    private const int minLoadingScreenDuration = 4;
    private const int maxLoadingScreenDuration = 10;

    public Animator transition;

    public static bool loadingScreenIsActive = false;

    public static bool playFabInitialized = false;
    public static bool googleServicesInitialized = false;
    public static bool firebaseInitialized = false;
    public static bool admobInitialized = false;
    public static bool everythingInitialized = false;

    private static bool minLoadingScreenDurationReached = false;
    #endregion

    private void Awake() {
        // Init the Game on Awake
        Debug.Log("Loading Screen Awake");

        // Target FPS
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;

        // Choose Quality Settings
        Globals.KaloaSettings.setQualitySettings();

        screen = this.GetComponent<LoadingScreen>();
        loadingScreenIsActive = true;

        Globals.UICanvas.uiElements = GameObject.Find("Canvas").GetComponent<UIElements>();
        Globals.UICanvas.translatedTMProElements = Globals.UICanvas.uiElements.gameObject.GetComponent<TranslatedElements>();

        StartCoroutine(MinLoadingScreenCheck());
        StartCoroutine(forceGameStartAfterXSec());

        connectServices();
    }


    public static void connectServices() {
        // Connect to services
        try
        {
            Globals.Game.saveGame = new SaveGame();

            // Load User specific Settings from PlayerPrefs
            SavingSystem.loadUserSettings();
        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText("ConnectServices-- " + e.ToString(), true);
        }

        try {
            // Load Google Services
            SavingSystem.loadGoogleServices();
        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText("ConnectServices-- " + e.ToString(), true);
        }

        try {
            // Connect Playfab
            if (!Globals.KaloaSettings.preventPlayfabCommunication) {
                PlayFabAccountMngmt.connectPlayFab();
            }
        }
        catch (Exception e)
        {
            Globals.UICanvas.DebugLabelAddText("ConnectServices-- " + e.ToString(), true);
        }
    }



    /*-----------------------------------------------------------
     --------------------INIT CALLBACKS--------------------------
     -----------------------------------------------------------*/

    public static void setPlayFabInitialized() {
        playFabInitialized = true;
        Debug.LogWarning("Playfab initialized");
        checkEverythingInitialized();
    }

    public static void setGoogleServicesInitialized() {
        googleServicesInitialized = true;
        Debug.LogWarning("Goolge Play initialized");
        checkEverythingInitialized();
    }

    public static void setFirebaseInitialized() {
        firebaseInitialized = true;
        Debug.LogWarning("Firebase initialized");
        checkEverythingInitialized();
    }

    public static void setAdmobInitialized() {
        admobInitialized = true;
        Debug.LogWarning("Admob initialized");
        checkEverythingInitialized();
    }
    

    /// <summary>
    /// Check if everything is initialized <br></br>
    /// Between min and max loading it will load the Main Scene instantly
    /// </summary>
    private static void checkEverythingInitialized() {

        everythingInitialized = playFabInitialized && googleServicesInitialized && firebaseInitialized && admobInitialized;

        // If the Min Loading Screen Duration is already reached
        if (everythingInitialized && minLoadingScreenDurationReached) {
            // Load MainScene immediately
            loadMainScene();
        } else {
            Debug.LogWarning("Not Everything Initialized or " + minLoadingScreenDuration + " Seconds are not over");
        }
    }

    public IEnumerator MinLoadingScreenCheck() {
        yield return new WaitForSeconds(minLoadingScreenDuration);

        minLoadingScreenDurationReached = true;

        // If everything is Initialized we load the MainScene after the Min Loading Screen Time
        if (everythingInitialized) {
            loadMainScene();
        } else {
            Debug.LogWarning(minLoadingScreenDuration + " Seconds are over, but not everything Initialized");
        }

        yield return null;
    }

    public IEnumerator forceGameStartAfterXSec() {
        yield return new WaitForSeconds(maxLoadingScreenDuration);

        loadMainScene();

        Debug.LogWarning("Forcing to MainScene, everythingInitialized=" + everythingInitialized);

        yield return null;
    }

    /// <summary>
    /// Loads the MainScene
    /// </summary>
    public static void loadMainScene() {
        // Important so the AdWrapper Object doesn´t get destroyed on Loading a new Scene
        DontDestroyOnLoad(Globals.Controller.Ads);
        screen.StartCoroutine(screen.animateToMainScene());
    }

    /// <summary>
    /// Animates to the MainScene
    /// </summary>
    private IEnumerator animateToMainScene() {
        LoadingSceneLoadingCircle.alpha = 1;

        if (SceneManager.GetActiveScene().buildIndex == 0) {

            transition.SetTrigger("StartShowMainscene");

            yield return new WaitForSeconds(1f);

            loadingScreenIsActive = false;

            // Check which World should be loaded (last World Player played -> Default is Village1)
            string currentWorldString = Globals.KaloaSettings.defaultWorldName;

            // Check if last setted World exists in SavingSystem and as Scene
            if (SavingSystem.worldExists(Globals.Game.saveGame.currentWorldName) 
                && Application.CanStreamedLevelBeLoaded("WorldScene_" + Globals.Game.saveGame.currentWorldName)) {
                // World exists -> override default
                currentWorldString = Globals.Game.saveGame.currentWorldName;
            }
            // Load MainScene
            SceneManager.LoadScene("WorldScene_" + currentWorldString);
        }

        // Stop the Coroutines of LoadingScreen.cs
        StopAllCoroutines();

        yield return null;
    }




    /// <summary>
    /// For our User Experience: How many seconds of LoadingScreen are accepted
    /// </summary>
    private void OnApplicationQuit() {
        // Log Firebase Event
        KeyValuePair<string, object>[] valuePairArray = {
            new KeyValuePair<string, object>("loadingScreenTime", (int)Time.time),
        };

        Globals.Controller.Firebase.IncrementFirebaseEventWithParameters(
        "user_quit_in_loadingscreen", valuePairArray);
    }
}
