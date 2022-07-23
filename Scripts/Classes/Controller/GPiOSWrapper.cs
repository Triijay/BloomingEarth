using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// Wrapper for Google Play Services and iOS Services<br></br>
/// Call a Function and it will decide what to do on any device
/// </summary>
public class GPiOSWrapper {

    /// <summary>
    /// User is logged in
    /// </summary>
    private bool loggedIn = false;


    /// <summary>
    /// User is logged in
    /// </summary>
    /// <returns></returns>
    public bool isloggedIn() {
        return loggedIn;
    }

    /// <summary>
    /// is GooglePlayGames activated?
    /// </summary>
    private bool PlayGamesPlatformActivated = false;


    /// <summary>
    /// Storage for Events that are Called before connecting to Google. After connect, they will be called again
    /// </summary>
    private static List<KeyValuePair<string, int>> IncrementEventsAfterActivate = new List<KeyValuePair<string, int>>();

    /// <summary>
    /// Should never be turned on, variable for testing that
    /// </summary>
    public bool testerHasDebugLabelOn = false;

    //* -- Login


    /// <summary>
    /// Connect to Google Play Games
    /// </summary>
    public void getGooglePlayGames() {

        // Create client configuration
        PlayGamesClientConfiguration config;

        // Check if the user already linked his Google Account to playfab
        if (Globals.UserSettings.linkedGoogleAccountToPlayfab || PlayFabAccountMngmt.tryingToLinkGoogle) {
            Globals.UICanvas.DebugLabelAddText("Google using profile scope");
            // Create client configuration with profile scope and ServerAuthcode
            config = new PlayGamesClientConfiguration.Builder()
                .EnableSavedGames() // enables saving game progress.
                .AddOauthScope("profile") // necessary for profile information
                .RequestServerAuthCode(false) // necessary for linking Google to Playfab
                .Build();
        } else {
            Globals.UICanvas.DebugLabelAddText("Google NOT using profile scope");
            // Create client configuration
            config = new PlayGamesClientConfiguration.Builder()          
                .EnableSavedGames() // enables saving game progress.
                .Build();
        }


        //Initialize and activate the platform
        PlayGamesPlatform.InitializeInstance(config);

        // Enable debugging output (recommended)
        PlayGamesPlatform.DebugLogEnabled = true;

        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();
        PlayGamesPlatformActivated = true;

        // Try silent authentication (second parameter is isSilent)
        if (!Social.localUser.authenticated) {

            if (Globals.Game.currentUser.wasSignedIn) {
                //PlayGamesPlatform.Instance.Authenticate(signInCallback, false);
                Social.localUser.Authenticate(signInCallback);
            }

        } else {
            // Already Signed In
            signInCallback(true, "");
        }

    }

    /// <summary>
    /// Signs in or out in Gooogle Play
    /// </summary>
    public void signInOut() {
        if(Application.platform == RuntimePlatform.WindowsEditor) {

            Debug.Log("TryingToSignInGooglePlay");

        } else if (Application.platform == RuntimePlatform.Android) {
            if (!Social.localUser.authenticated) {
                signIn();
            } else {
                // Sign out of play games
                PlayGamesPlatform.Instance.SignOut();

                // Deactivate Adm Things
                Globals.UICanvas.uiElements.DebugLabelObj.SetActive(false);

                // Reset sign-in status
                loggedIn = false;
                Globals.Game.currentUser.wasSignedIn = false;

                // Translate Settings
                if (Globals.Controller.Language != null) {
                    Globals.Controller.Language.translateSettingsGoogle();
                }
            }
        } else if (Application.platform == RuntimePlatform.IPhonePlayer) {

            Debug.Log("Iphone");
        }
    }

    public void signIn() {
        if (Application.platform == RuntimePlatform.WindowsEditor) {

            Debug.Log("TryingToSignInGooglePlay");

        } else if (Application.platform == RuntimePlatform.Android) {
            // Sign in with Play Game Services, showing the consent dialog 
            // by setting the second parameter to isSilent=false
            //PlayGamesPlatform.Instance.Authenticate(signInCallback, false);
            Social.localUser.Authenticate(signInCallback);
        } 
    }

    /// <summary>
    /// Gives a Callback when signing in
    /// </summary>
    /// <param name="success"></param>
    private void signInCallback(bool success, string error) {
        try {

            if (LoadingScreen.loadingScreenIsActive) {
                LoadingScreen.setGoogleServicesInitialized();
            }

            if (success) {

                PlayFabAccountMngmt.connectPlayFab();

                Globals.UICanvas.DebugLabelAddText("-- GP Signed in!");
                // Change sign-in status
                loggedIn = true;
                Globals.Game.currentUser.wasSignedIn = true;

                // Resent Events if stored
                if (IncrementEventsAfterActivate != null) {
                    foreach (KeyValuePair<string, int> EventToSentPair in IncrementEventsAfterActivate) {
                        //Globals.UICanvas.DebugLabelAddText("ResendingEvent " + EventToSentPair.Key + " with value " + EventToSentPair.Value + ".");
                        IncrementEvent(EventToSentPair.Key, (uint)EventToSentPair.Value, false);
                        //Globals.UICanvas.DebugLabelAddText(EventToSentPair.Key + " - event resent");
                    }
                    IncrementEventsAfterActivate.Clear();
                }

                if (ButtonScripts.achievementClickedwithoutLogin) {
                    showAchievments();
                    ButtonScripts.achievementClickedwithoutLogin = false;
                }

                // Check if Adm Signed in
                checkAndExecuteAdmUserSignIn();

            } else {
                Globals.UICanvas.DebugLabelAddText("-- Sign-in failed...");
                Globals.UICanvas.DebugLabelAddText("Error: " + error);
                // Change sign-in status
                loggedIn = false;
                Globals.Game.currentUser.wasSignedIn = false;
                PlayFabAccountMngmt.reloadGoogleSave = false;
            }
            // Translate Settings
            if (Globals.Controller.Language != null) {
                Globals.Controller.Language.translateSettingsGoogle();
            }

        } catch(Exception e) {
            Globals.UICanvas.DebugLabelAddText(e);
        }
    }

    /// <summary>
    /// Detects wether an Admin is Online on Build and gives him Debug Logs<br></br>
    /// forceIsTester is to Unit Test that the Line with the DebugLabel is commented out at the Tester
    /// </summary>
    public void checkAndExecuteAdmUserSignIn() {

        if(Globals.UICanvas.uiElements.DebugLabelObj != null) {
            Globals.UICanvas.uiElements.DebugLabelObj.SetActive(false);

            if (Application.platform == RuntimePlatform.Android) {

                if (isAdm()) {
                    Globals.UICanvas.uiElements.DebugLabelObj.SetActive(true);
                    Globals.UICanvas.DebugLabelAddText("Welcome Adm:\n " + Social.localUser.userName);
                    //Globals.UICanvas.uiElements.SettingsMenuReset.SetActive(true);
                    //Globals.Wrapper.FirebaseWrapper.IncrementFirebaseEventOnce("admin_loggedin", "times");
                    //Globals.UICanvas.DebugLabelAddText("Playin' on Quality " + QualitySettings.GetQualityLevel().ToString());
                }

            } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
                Debug.Log("Iphone");
            } else if (Application.platform == RuntimePlatform.WindowsEditor) {
                Debug.Log("WindowsEditor");
            }
        } 

    }

    /// <summary>
    /// Detects wether an Admin is loggedin
    /// </summary>
    /// <returns></returns>
    public bool isAdm() {
        if (Application.platform == RuntimePlatform.Android) {
            return (Social.localUser.userName == "BIGScream92" || Social.localUser.userName == "Triijay");
        } else {
            return false;
        }
    }


    //* -- Achievements


    /// <summary>
    /// Sets an Achievement to 100 Percent
    /// </summary>
    /// <param name="eventName"></param>
    public void sentAchievement100Percent(string eventName) {

        Firebase.Analytics.FirebaseAnalytics.LogEvent("unlock_achievement");

        if (Application.platform == RuntimePlatform.Android) {

            if (Social.localUser.authenticated) {
                PlayGamesPlatform.Instance.ReportProgress(
                    eventName, 100.0f,
                    (bool success) => { 
                        Globals.UICanvas.DebugLabelAddText("Achievement unlocked: " + eventName); 
                    }
                );
            }

        } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            Debug.Log("Iphone");
        }
    }

    /// <summary>
    /// Sets an string array of Achievements to 100 Percent<br></br>
    /// Casts the Achievements with a good Time-Between
    /// usage: sentAchievements100Percent
    /// </summary>
    /// <param name="achievementIDs"></param>
    public void sentAchievements100Percent(string[] achievementIDs) {
        Globals.Game.initGame.GetComponent<GPiOSHelper>().triggerAchievementsArray(achievementIDs);
    }

    /// <summary>
    /// Shows the Achievements
    /// </summary>
    public void showAchievments() {
        if (Application.platform == RuntimePlatform.Android) {

            if (Social.localUser.authenticated) {
                PlayGamesPlatform.Instance.ShowAchievementsUI();
            } else {
                Debug.Log("Cannot show Achievements, not logged in");
            }

        } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            Debug.Log("Iphone");
        }
    }





    //* -- Events


    /// <summary>
    /// Increments a Tracking Event with a specific Integer
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="incrementInt"></param>
    /// <param name="chanceToResend">if this is true, a not sended Event will be stored in a List and will be attempted to resend on Service-Activation</param>
    public void IncrementEvent(string eventName, uint incrementInt, bool chanceToResend = true) {
        if (Application.platform == RuntimePlatform.Android) {

            if (PlayGamesPlatformActivated && Social.localUser.authenticated) {
                //Globals.UICanvas.DebugLabelAddText("Event " + eventName + " sent");
                PlayGamesPlatform.Instance.Events.IncrementEvent(eventName, incrementInt);
            } else if (chanceToResend) {
                //Globals.UICanvas.DebugLabelAddText("Event " + eventName + " saved for later");
                IncrementEventsAfterActivate.Add(new KeyValuePair<string, int>(eventName, (int)incrementInt));
            }

        } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            Debug.Log("Iphone");
        }
    }

    /// <summary>
    /// Increments a Tracking Event on one
    /// </summary>
    /// <param name="eventName"></param>
    public void IncrementEventOnce(string eventName) {
        IncrementEvent(eventName, 1);
    }    



    //* -- Leaderboards


    /// <summary>
    /// Posts a Score to a specific Leaderboard
    /// </summary>
    /// <param name="score"></param>
    /// <param name="leaderboardName"></param>
    public void ScoreToLeaderBoard(long score, string leaderboardName) {
        if (Application.platform == RuntimePlatform.Android) {

            if (Social.localUser.authenticated && !isAdm()) {
                Social.ReportScore(
                    score,
                    leaderboardName,
                    (bool success) => { Globals.UICanvas.DebugLabelAddText("(Blooming Earth) Leaderboard " + leaderboardName + " update success: " + success); }
                );
            }

        } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            Debug.Log("Iphone");
        }
    }

    /// <summary>
    /// Opens the Leaderboard
    /// </summary>
    public void showLeaderboards(string leaderboardID) {
        if (Application.platform == RuntimePlatform.Android) {
            if (Social.localUser.authenticated) {
                PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardID);
            } else {
                Debug.Log("Cannot show leaderboard: not authenticated");
            }

        } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            Debug.Log("Iphone");
        }
    }

}