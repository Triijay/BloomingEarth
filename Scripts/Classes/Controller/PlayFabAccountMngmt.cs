using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using GooglePlayGames;
using UnityEngine;
using UnityEngine.UI;

public class PlayFabAccountMngmt
{

    public static bool tryingToLinkGoogle = false;
    public static bool reloadGoogleSave = false;
    private static bool tryingToLogin = false;
    private static int sessionLinkCountGoogle = 0;
    

    /*--------------------------------------------------------------
      -------------------------- LOGIN -----------------------------
      --------------------------------------------------------------*/

    public static void connectPlayFab() {

        Globals.UICanvas.DebugLabelAddText("Trying to connect playfab...");
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId)) {
            /* You can set the value in the Playfab Editor Extensions
             * to be sure the ID is set, it will be set here (again) */
            PlayFabSettings.TitleId = "FB218";
        }

        if(Application.internetReachability != NetworkReachability.NotReachable) {

            // if not logged in, log in
            if (!PlayFabClientAPI.IsClientLoggedIn() && !tryingToLogin) {
                tryingToLogin = true;

                if (Globals.UserSettings.linkedGoogleAccountToPlayfab) {
                    LoginWithGoogle();
                    sessionLinkCountGoogle++;
                } else {
                    LoginWithAndroidDevice();
                }
            } 

            // Used when User starts the game and wants to restore his game data
            if (reloadGoogleSave) {
                Globals.UICanvas.DebugLabelAddText("Forced Google Login");
                LoginWithGoogle();
            }

            if (tryingToLinkGoogle) {
                LinkGoogleToPlayfab();
            }
        }


    }

    private static void LoginWithAndroidDevice() {    
        Globals.UICanvas.DebugLabelAddText("PF trying to login with Android Device ID...");

        // Connect with Android Device ID
        var request = new LoginWithAndroidDeviceIDRequest {
            AndroidDevice = SystemInfo.deviceModel,
            AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            TitleId = PlayFabSettings.TitleId
        };

        PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginSuccess, OnSharedFailure);
    }


    private static void LoginWithGoogle() {
        Globals.UICanvas.DebugLabelAddText("PF trying to login with Google...");
        
        // Connect to playfab with Google Account
        var request = new LoginWithGoogleAccountRequest {
            TitleId = PlayFabSettings.TitleId,
            ServerAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode(),
            CreateAccount = true
        };

        if (Social.localUser.authenticated) {
            PlayFabClientAPI.LoginWithGoogleAccount(request, OnLoginSuccess, OnSharedFailure);
        } else {
            Globals.UICanvas.DebugLabelAddText("Google not logged in...");
            tryingToLogin = false;
        }
    }


    private static void OnLoginSuccess(LoginResult result) {

        Globals.UICanvas.DebugLabelAddText("PF Signed In as " + result.PlayFabId);

        // Saving the user Login Entity
        PlayFabFileSync.playFabEntityID = result.EntityToken.Entity.Id;
        PlayFabFileSync.playFabEntityType = result.EntityToken.Entity.Type;

        tryingToLogin = false;

        PlayFabFileSync.LoadAllFiles();
    }


    /*--------------------------------------------------------------
      ------------------------- LINKING ----------------------------
      --------------------------------------------------------------*/

    public static void PrepareLinking() {
        if (Globals.UserSettings.linkedGoogleAccountToPlayfab) {
            // Link Android Device ID
            LinkAndroidDeviceToPlayfab();
            
        } else if(!Globals.UserSettings.linkedGoogleAccountToPlayfab && sessionLinkCountGoogle == 0) {
            // Link Google Account
            tryingToLinkGoogle = true;
            // To make sure the script tries to authenticate
            Globals.Game.currentUser.wasSignedIn = true;
            Globals.Controller.GPiOS.getGooglePlayGames();
        } else {
            DeactivateLinkButton();
        }
    }

    private static void LinkGoogleToPlayfab() {
        Globals.UICanvas.DebugLabelAddText("Trying to link Google to Playfab...");  

        if (Social.localUser.authenticated) {
            Globals.UICanvas.DebugLabelAddText("ServerAuthCode: " + PlayGamesPlatform.Instance.GetServerAuthCode());
            // Try to link Google Account
            var request = new LinkGoogleAccountRequest {
                ServerAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode(),
                ForceLink = true,
            };

            if (PlayFabClientAPI.IsClientLoggedIn()) {
                Globals.UICanvas.DebugLabelAddText("Linking Google...");
                PlayFabClientAPI.LinkGoogleAccount(request, OnLinkSuccess, OnLinkFailure);
            } 
        } 
    }

    private static void LinkAndroidDeviceToPlayfab() {
        Globals.UICanvas.DebugLabelAddText("Trying to link Android Device to Playfab...");

        var request = new LinkAndroidDeviceIDRequest {
            AndroidDevice = SystemInfo.deviceModel,
            AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
            ForceLink = true,
        };

        if (PlayFabClientAPI.IsClientLoggedIn()) {
            PlayFabClientAPI.LinkAndroidDeviceID(request, OnLinkSuccess, OnSharedFailure);
        }
    }


    /*--------------------------------------------------------------
      ----------------------- LINK RESULTS --------------------------
      --------------------------------------------------------------*/


    /// <summary>
    /// Successfully linked Google Account
    /// </summary>
    /// <param name="result"></param>
    private static void OnLinkSuccess(LinkGoogleAccountResult result) {
        Globals.UICanvas.DebugLabelAddText("Google successfully linked to PF Account");

        Globals.UserSettings.linkedGoogleAccountToPlayfab = true;
        sessionLinkCountGoogle++;

        Globals.Controller.Language.executeCloudLinking();
        Globals.UICanvas.translatedTMProElements.LinkingPopUpHint.text = Globals.Controller.Language.translateString("linking_confirmation_hint_linkSuccess");

        var request = new UnlinkAndroidDeviceIDRequest {
            AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
        };

        // Unlink Android Device ID
        PlayFabClientAPI.UnlinkAndroidDeviceID(request, OnUnlinkSuccess, OnSharedFailure);
    }

    /// <summary>
    /// Successfully linked Android Device
    /// </summary>
    /// <param name="result"></param>
    private static void OnLinkSuccess(LinkAndroidDeviceIDResult result) {
        Globals.UICanvas.DebugLabelAddText("Android Device successfully linked to PF Account: ");

        var request = new UnlinkGoogleAccountRequest();

        // Unlink Google Account
        PlayFabClientAPI.UnlinkGoogleAccount(request, OnUnlinkSuccess, OnSharedFailure);
    }


    /// <summary>
    /// Successfully unlinked Google Account
    /// </summary>
    /// <param name="result"></param>
    private static void OnUnlinkSuccess(UnlinkGoogleAccountResult result) {
        Globals.UserSettings.linkedGoogleAccountToPlayfab = false;
        Globals.Controller.Language.executeCloudLinking();
        Globals.UICanvas.translatedTMProElements.LinkingPopUpHint.text = Globals.Controller.Language.translateString("linking_confirmation_hint_unlinkSuccess");
        Globals.UICanvas.DebugLabelAddText("Successfully unlinked Google from PF Account");
    }

    /// <summary>
    /// Successfully unlinked Android Device
    /// </summary>
    /// <param name="result"></param>
    private static void OnUnlinkSuccess(UnlinkAndroidDeviceIDResult result) {
        Globals.UICanvas.DebugLabelAddText("Successfully unlinked Android Device from PF Account");
    }


    private static void OnLinkFailure(PlayFabError error) {
        Globals.UserSettings.linkedGoogleAccountToPlayfab = false;
        Globals.UICanvas.translatedTMProElements.LinkingPopUpHint.text = Globals.Controller.Language.translateString("linking_confirmation_hint_linkSuccessFailure"); 
        Globals.UICanvas.DebugLabelAddText(error.GenerateErrorReport());
    }



    /*--------------------------------------------------------------
      -------------------- HELPER FUNCTIONS ------------------------
      --------------------------------------------------------------*/

    private static void OnSharedFailure(PlayFabError error) {
        tryingToLogin = false;
        reloadGoogleSave = false;
        Debug.LogError(error.GenerateErrorReport());
    }

    private static void DeactivateLinkButton() {
        Globals.UICanvas.translatedTMProElements.LinkingPopUpHint.text = Globals.Controller.Language.translateString("linking_confirmation_hint_exceededLinkLimit");
        Globals.UICanvas.translatedTMProElements.LinkingPopUpBtn.gameObject.GetComponent<Button>().interactable = false;
    }
}
