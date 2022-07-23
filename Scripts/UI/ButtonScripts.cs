using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Mail;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class ButtonScripts : MonoBehaviour {

    // Performance Optimization
    string buildingName;
    Building activeBuilding;
    private AudioSource audioSource;
    string txt;
    public static bool achievementClickedwithoutLogin = false;
    static bool feedbackPopUpAlreadyOpened = false;

    /// <summary>
    /// Load Sounds on Awake of Buttons
    /// </summary>
    public void Awake() {
        audioSource = this.GetComponent<AudioSource>();
    }


    /// <summary>
    /// resets the game and let the User Start from the beginning with all Buildungs lvl 0 and a minimum amount of coins<br></br>
    /// Can Only be triggered by Admins
    /// </summary>
    public void resetWholeGame() {
        Globals.Controller.Sound.PlaySound("ButtonClick1");

        Globals.Game.initGame.resetGameForAdmins();
    }

    /// <summary>
    /// User can reset his game via the Prestige function
    /// </summary>
    public void prestigeReset() {
        Globals.Game.currentUser.prestige.prestigeResetGame();
    }


    /// <summary>
    /// Searches the Building from the active Panel and levels Up the Building
    /// </summary>
    public void triggerLevelUpFromBuildingMenu() {

        if (Globals.UICanvas.uiElements.BuildingMenu.activeSelf) {
            // Get BuildingName out of the Panel
            buildingName = Globals.UICanvas.uiElements.BuildingMenuBuildingName.text;

            // Get Building with the Name
            activeBuilding = GameObject.Find("Buildings/" + buildingName).GetComponent<Building>();

            if (activeBuilding != null) {
                // Level it Up
                activeBuilding.triggerLevelUp();
            } else {
                Debug.LogError("BuildingMenu: Building '" + buildingName + "' cannot be LevelUpped");
            }
        }

    }

    /// <summary>
    /// Searches the Building from the active Panel, Starts a Video and after the Video is fully watched, the DelayUnlock-Process is -30min
    /// </summary>
    public void triggerMinus30MinFromBuildingMenu() {

        Globals.Controller.Sound.PlaySound("ButtonClick1");

        if (Globals.UICanvas.uiElements.BuildingMenu.activeSelf) {
            // Get BuildingName out of the Panel
            buildingName = Globals.UICanvas.uiElements.BuildingMenuBuildingName.text;

            // Get Building with the Name
            activeBuilding = GameObject.Find("Buildings/" + buildingName).GetComponent<Building>();

            if (activeBuilding != null) {
                if (Application.platform == RuntimePlatform.WindowsEditor) {
                    Globals.Controller.Ads.setRewardingBuilding(activeBuilding);
                    Globals.Controller.Ads.HandleUserEarnedForwardBuildingProgressReward(null, null);
                } else {
                    // Start Video
                    Globals.Controller.Ads.ShowRewardedAd(Globals.KaloaSettings.AdType.ForwardBuildingProgress, null , activeBuilding);
                }
            } else {
                Debug.LogError("BuildingMenu: Building '" + buildingName + "' cannot be LevelUpped");
            }
        }

    }


    /// <summary>
    /// Boosts the current World do a double-Bonus in producing coins
    /// </summary>
    public void triggerBoostWorld() {

        Globals.Controller.Sound.PlaySound("ButtonClick1");

        // Start Video
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            Globals.Controller.Ads.HandleUserEarnedBoostReward(null, null);
        } else {
            // Start Video
            Globals.Controller.Ads.ShowRewardedAd(Globals.KaloaSettings.AdType.Boost);
        }

    }

    /// <summary>
    /// TODO - Show the Item Details
    /// </summary>
    public void showItemDetailsPopUp() {

    }


    /// <summary>
    /// AddCoins-Button -> Open Shop Coins Page
    /// </summary>
    public void openShopCoinsPage() {
        Globals.Controller.Sound.PlaySound("ButtonClick1");
        Debug.Log("openShopPageCoins");
    }


    public void LoadCloudSaveGame() {
        Globals.Controller.Sound.PlaySound("ButtonClick1");
        SavingSystem.OnButtonClickCloud();
    }

    public void LoadLocalSaveGame() {
        Globals.Controller.Sound.PlaySound("ButtonClick1");
        SavingSystem.OnButtonClickLocal();
    }

    public void ReconnectGameToCloud() {
        Globals.Controller.Sound.PlaySound("ButtonClick1");

        // Set starting vars
        Globals.Game.isGameStarting = true;
        PlayFabAccountMngmt.reloadGoogleSave = true;
        Globals.Game.currentUser.wasSignedIn = true;
        Globals.UserSettings.linkedGoogleAccountToPlayfab = true;
        SavingSystem.savePlayerPrefs(DateTime.Now);
        
        Globals.UICanvas.uiElements.PopUpReconnectCloud.SetActive(false);
        SceneManager.LoadScene("LoadingScene");
    }


    public void activateBmInfoTab() {
        Globals.UICanvas.uiElements.InitGameObject.GetComponent<BuildingMenu>().setActiveTab(BuildingMenu.MenuTab.InfoTab);
    }

    public void activateBmItemTab() {
        Globals.UICanvas.uiElements.InitGameObject.GetComponent<BuildingMenu>().setActiveTab(BuildingMenu.MenuTab.ItemTab);
    }


    /*-------------------------------------------------------------- 
    --------------------------- Quests -----------------------------
    ---------------------------------------------------------------*/

    /// <summary>
    /// Button -> QuestsPopUp Quests
    /// </summary>
    public void openQuests() {
        triggerQuests(true);
    }

    /// <summary>
    /// Button -> QuestsPopUp Quests, without click
    /// </summary>
    public void openQuestsSilent() {
        triggerQuests(true, true);
    }

    /// <summary>
    /// Button -> Trigger QuestsPopUp on/off, with click
    /// </summary>
    public void triggerQuestsWithClick() {
        triggerQuests(false, false);
    }

    /// <summary>
    /// Open/Trigger QuestsPopUp -> Cannot be used in Inspector, because of 2 Parameters. Use the Other functions there
    /// </summary>
    private static void triggerQuests(bool forceOpen = false, bool silent = false) {

        bool popupActiveState = Globals.UICanvas.uiElements.PopUpQuests.activeSelf;

        // Close OverlayMenu
        Globals.UICanvas.uiElements.OverlayMenu.closeOverlayMenu();

        // Play Sound if not silent
        if (!silent) {
            Globals.Controller.Sound.PlaySound("ButtonClick1");
        }

        // Load Quest Information
        Globals.Game.currentWorld.QuestsComponent.loadCurrentQuest(false);

        // Trigger PopUp
        Globals.UICanvas.uiElements.PopUpQuests.SetActive(!popupActiveState || forceOpen);

        UIElements.setHUDVisibility_LeftAndRight(popupActiveState);

    }

    /// <summary>
    /// Closes the Quest PopUp
    /// </summary>
    public void closeQuestPanelSilent() {
        closeQuestPanel(true);
    }

    /// <summary>
    /// Closes the Quest PopUp
    /// </summary>
    public void closeQuestPanel(bool silent = false) {
        staticCLoseQuestPanel(silent);
    }

    public static void staticCLoseQuestPanel(bool silent = false) {
        // InfoCircle not hidden anymore, because User should be aware that there is a next quest
        if (Globals.Game.currentWorld.QuestsComponent.getCurrentQuest().questState != Quest.QuestStates.InProgress &&
            !Globals.Game.currentWorld.QuestsComponent.currentQuestIsLastQuestAndFinished()
            ) {
            Globals.Game.currentWorld.QuestsComponent.setStatusQuestInfoCircle(true);
        }
        else {
            Globals.Game.currentWorld.QuestsComponent.setStatusQuestInfoCircle(false);
        }

        // Play Sound if not silent
        if (!silent) {
            Globals.Controller.Sound.PlaySound("ButtonClick1");
        }

        Globals.UICanvas.uiElements.PopUpQuests.SetActive(false);

        UIElements.setHUDVisibility(true);
    }


    /// <summary>
    /// Start Quest Button of PopUp Quests
    /// </summary>
    public void QuestStart() {

        Globals.Controller.Sound.PlaySound("ButtonClick1");

        // QuestState
        Globals.Game.currentWorld.QuestsComponent.setCurrentQuestState(Quest.QuestStates.InProgress);

        // InfoCircle hidden
        Globals.Game.currentWorld.QuestsComponent.setStatusQuestInfoCircle(false);    

        // Check Quest alreadyFullfilled
        if (Globals.Game.currentWorld.QuestsComponent.checkCurrentQuestAlreadyFulfilled()) {
            // if its already fullfilled, immediately go to Reward-PopUp
            Globals.Game.currentWorld.QuestsComponent.loadCurrentQuest(false);
            Globals.UICanvas.uiElements.PopUpQuests.SetActive(true);
        } else {
            // User took all Quests that are currently doable
            Globals.Game.currentWorld.QuestsComponent.allCurrentQuestsAccepted.Invoke();
            // Close PopUp
            Globals.UICanvas.uiElements.PopUpQuests.SetActive(false);
            // Activate HUD Buttons
            UIElements.setHUDVisibility(true);
        }
    }

    /// <summary>
    /// Finish Quest Button of PopUp Quests
    /// </summary>
    public void QuestFinish() {

        Globals.Controller.Sound.PlaySound("EmeraldReward");

        // InfoCircle hidden
        Globals.Game.currentWorld.QuestsComponent.setStatusQuestInfoCircle(false);

        // Earn reward
        Globals.Game.currentWorld.QuestsComponent.currentQuestClaimRewards();

        // Set Next Quest
        Globals.Game.currentWorld.QuestsComponent.loadNextQuest();
        
    }


    /*-------------------------------------------------------------- 
    ------------------------- Community ----------------------------
    ---------------------------------------------------------------*/


    /// <summary>
    /// Sends the Logged Information from the User
    /// </summary>
    public void triggerUserIdeas() {
        Globals.UserSettings.hasRated = true;
        SendEmail("Some Feedback and Ideas");
    }

    /// <summary>
    /// Sends the Logged Information from the User
    /// </summary>
    public void triggerUserBugReport() {
        SendEmail("Bug Report", Globals.Controller.Language.translateString("mail_predefined_questions_bugreport"));
    }

    /// <summary>
    ///  TODO Description
    /// </summary>
    /// <param name="subject"></param>
    private void SendEmail(string subject, string predefined_body = "") {
        string email = Globals.KaloaSettings.linkEmailInfo;
        subject = MyEscapeURL(subject);

        string body = MyEscapeURL(
            // Vordefinierte Fragen
            "" + predefined_body + "" +
            
            // System Infos
            "\r\n\r\n\r\n" +
            "--------------------------------------\r\n" +
            "" + Globals.Controller.Language.translateString("mail_please_leave_device_specs") + "" + "\r\n" +
            "Product Name: " +  Application.productName + "\r\n" +
            "Application Version: " + Application.version + "\r\n" +
            "System Language: " + Application.systemLanguage + " -- chosenGameLanguage: " + Globals.Controller.Language.currentLanguage + "\r\n" + "\r\n" +

            "Google was signed in: " + Globals.Game.currentUser.wasSignedIn + "\r\n" +
            "Google linked to Cloud: " + Globals.UserSettings.linkedGoogleAccountToPlayfab + "\r\n" + "\r\n" +

            "Platform: " + Application.platform + ", OS: " + SystemInfo.operatingSystem + "\r\n" +
            "Device: " + SystemInfo.deviceModel + " -- " + SystemInfo.deviceName + " -- " + SystemInfo.deviceType + " \r\n" + " \r\n" +

            "Quality: " + QualitySettings.GetQualityLevel().ToString() + "\r\n" +
            "CPU: " + SystemInfo.processorType + " ("+ SystemInfo.processorCount + "x" + SystemInfo.processorFrequency + "Mhz)" + " \r\n" +
            "RAM: " + SystemInfo.systemMemorySize.ToString() + "MB \r\n" + " \r\n" +

            "Resolution: " + Screen.currentResolution + " @ " + Screen.dpi + "dpi -> " + Screen.orientation + " \r\n" + " \r\n" +

            "Graphics: DeviceType " + SystemInfo.graphicsDeviceType +  " -- DeviceVersion:" + SystemInfo.graphicsDeviceVersion +   
            " -- MemorySize " + SystemInfo.graphicsMemorySize + "MB; MultiThreaded " + SystemInfo.graphicsMultiThreaded + " -- ShaderLevel " + SystemInfo.graphicsShaderLevel +
            " -- supportsInstancing: " + SystemInfo.supportsInstancing + " -- supportsShadows: " + SystemInfo.supportsShadows + ""+

            ""
            );
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }
    private string MyEscapeURL(string url) {
        return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }


    /// <summary>
    /// Activates Feedback-PopUp when neccessary
    /// </summary>
    public static void checkifToActivateFeedbackPopUp() {
        if (!feedbackPopUpAlreadyOpened && !Globals.Game.currentUser.tutorialIsRunning &&
            (Globals.Game.currentUser.stats.GameOpeningsRaw == 10 || Globals.Game.currentUser.stats.DaysWithGameOpening == 10 || Globals.Game.currentUser.stats.DaysWithGameOpening == 30)) {
            Globals.UICanvas.uiElements.PopUps.transform.Find("Feedback").gameObject.SetActive(true);
            feedbackPopUpAlreadyOpened = true;
        }
    }


    /*-------------------------------------------------------------- 
    ------------------------ UserSettings --------------------------
    ---------------------------------------------------------------*/


    /// <summary>
    /// Changes the Setting of Users Sound
    /// </summary>
    public void changeUserSettingSound() {

        Globals.Controller.Sound.PlaySound("ButtonClick1");

        Globals.UserSettings.hasSound = !Globals.UserSettings.hasSound;
        Globals.Controller.Language.translateSettingsSound();
    }

    /// <summary>
    /// Changes the Setting of Users Music
    /// </summary>
    public void changeUserSettingMusic() {

        Globals.Controller.Sound.PlaySound("ButtonClick1");

        Globals.UserSettings.hasMusic = !Globals.UserSettings.hasMusic;
        Globals.Controller.Language.translateSettingsMusic();

        if (Globals.UserSettings.hasMusic) {
            // Start Playing Ambient Sound
            Globals.Controller.Sound.PlaySound(Globals.Controller.Sound.currentAmbientMusic, true);
        } else {
            Globals.Controller.Sound.StopSound(Globals.Controller.Sound.currentAmbientMusic);
        }
    }

    /// <summary>
    /// Changes the Setting of Users Notifications
    /// </summary>
    public void changeUserSettingNotifications() {

        Globals.Controller.Sound.PlaySound("ButtonClick1");

        Globals.UserSettings.hasNotifications = !Globals.UserSettings.hasNotifications;
        Globals.Controller.Language.translateSettingsNotifications();
    }

    /// <summary>
    /// Changes the Setting of Users Notifications
    /// </summary>
    public void changeUserSettingVibration() {

        Globals.Controller.Sound.PlaySound("ButtonClick1");

        Globals.UserSettings.hasVibration = !Globals.UserSettings.hasVibration;
        Globals.Controller.Language.translateSettingsVibration();
    }

    /// <summary>
    /// Changes the Setting of Users Language
    /// </summary>
    public void changeUserSettingLanguage() {

        Globals.Controller.Sound.PlaySound("ButtonClick1");

        Globals.Controller.Language.changeToNextLanguage();

        if (GameObject.Find("Language/Button/Text") != null) {
            GameObject.Find("Language/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text = Language.supportedLanguages[Globals.Controller.Language.currentLanguage];
        }
    }

    public void googleSignIn() {
        Globals.Controller.GPiOS.signInOut();
    }

    public void showAchievements() {
        if (!Social.localUser.authenticated) {
            achievementClickedwithoutLogin = true;
            Globals.Controller.GPiOS.signIn();
        } else {
            Globals.Controller.GPiOS.showAchievments();
        }
    }

    public void showLeaderboardTotalLevel() {
        Globals.Controller.GPiOS.showLeaderboards(GPGSIds.leaderboard_mighty_constructors_building_levels);
    }

    public void showLeaderboardCatchTheNuts() {
        Globals.Controller.GPiOS.showLeaderboards(GPGSIds.leaderboard_catch_the_nuts);
    }

    public void linkAccountToPlayfab() {
        PlayFabAccountMngmt.PrepareLinking();       
    }


    /*-------------------------------------------------------------- 
    ------------------------ LinkButtons --------------------------
    ---------------------------------------------------------------*/


    // Opens the Kaloa Games Play Store
    public void showKaloaShareUsRateUs() {

        if (Globals.Game.currentUser.stats.GameOpeningsRaw > 20 && !Globals.UserSettings.hasRated) {
            // Set User Rated Us
            Globals.UserSettings.hasRated = true;
            // Open Store Page Blooming Earth
            Application.OpenURL(Globals.KaloaSettings.linkPlayStore);
        } else {
            NativeShare ns = new NativeShare();
            Globals.Controller.Firebase.IncrementFirebaseEventOnce("shared_function_settingsmenu");
            ns.AddFile(Application.dataPath + "/Resources/Icons/Logos/AppIcon.png");
            ns.SetTitle("Blooming Earth");
            ns.SetText(Globals.Controller.Language.translateString(
                "settingsmenu_pressed_share", 
                new string[] {Globals.KaloaSettings.linkPlayStore }));
            ns.Share();
        }
        
        
    }

    // Opens the Kaloa Games Instagram Page
    public void showKaloaInstagram() {
        Application.OpenURL(Globals.KaloaSettings.linkInstagram);
    }

    // Opens the Kaloa Games Website "Privacy"
    public void showKaloaPrivacy() {
        Application.OpenURL(Globals.KaloaSettings.linkWebsitePrivacy);
    }

    // Opens the Kaloa Games Website "Terms and Conditions"
    public void showKaloaTOS() {
        Application.OpenURL(Globals.KaloaSettings.linkWebsiteTOS);
    }

    // Opens the Kaloa Games Website "Imprint"
    public void showKaloaImpress() {
        Application.OpenURL(Globals.KaloaSettings.linkWebsiteImprint);
    }

}
