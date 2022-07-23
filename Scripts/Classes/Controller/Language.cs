/*
The Lang Class adds easy to use multiple language support to any Unity project by parsing an XML file
containing numerous strings translated into any languages of your choice.  Refer to UMLS_Help.html and lang.xml
for more information.
 
Created by Adam T. Ryder
C# version by O. Glorieux
 
*/

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Linq;

using UnityEngine;
using System.Collections.Generic;

public class Language {

    private Hashtable XmlFileStrings;

    /// <summary>
    /// Language where its guaranteed that there is a Valid LanguageFile
    /// </summary>
    public const string FALLBACK_LANGUAGE = "en-US";

    /// <summary>
    /// Current Setted Language
    /// </summary>
    public string currentLanguage;

    /// <summary>
    /// The Current Setted Language as an Index (int Keys for <see cref="supportedLanguages"/> )
    /// </summary>
    public int currentLanguageIndex;

    private string returnString;

    private const string languageFileFolder = "LanguageTextFiles/lang";

    List<string> langKeyList;

    string nextLanguage;
    int nextLanguageIndex = 0;



    /// <summary>
    /// Supported Languages in our Game
    /// </summary>
    public static Dictionary<string, string> supportedLanguages = new Dictionary<string, string>{
        { "en-US", "English" },
        { "de-DE", "Deutsch" }
    };


    /// <summary>
    /// Initialize Lang class<br></br>
    /// path = path to XML resource example(you must not give the Resources Folder in the path and you must not write the extension of the File) :  "languageFiles/lang" (see InitGame.cs)<br></br>
    /// language = language to use example:  "en-US" or "de-DE"<br></br>
    /// </summary>
    /// <param name="language">Language-Code in the File</param>
    public Language(string language) {
        langKeyList = supportedLanguages.Keys.ToList();
        setLanguage(language);
    }


    /// <summary>
    /// Use the setLanguage function to swap languages after the Lang class has been initialized.<br></br>
    /// This function is called automatically when the Lang class is initialized.<br></br>
    /// path = path to XML resource example(you must not give the Resources Folder in the path and you must not write the extension of the File) :  "languageFiles/lang" (see InitGame.cs)<br></br>
    /// language = language to use example:  "en-US" or "de-DE"<br></br>
    /// </summary>
    /// <param name="language">Language-Code in the File</param>
    public void setLanguage(string language) {

        // Sets the language only if the new Language differs from the currentLanguage
        if (currentLanguage != language) {
            TextAsset textAsset = (TextAsset)Resources.Load(languageFileFolder + "_"+language, typeof(TextAsset));

            if (supportedLanguages.ContainsKey(language) && textAsset != null ) {
                currentLanguage = language;
                loadLanguageFile(textAsset, language);
            } else {
                // Fallback to English
                textAsset = (TextAsset)Resources.Load(languageFileFolder + "_" + FALLBACK_LANGUAGE, typeof(TextAsset));
                currentLanguage = FALLBACK_LANGUAGE;
                loadLanguageFile(textAsset, FALLBACK_LANGUAGE);
            }

            // Write Language to Usersettings
            Globals.UserSettings.language = currentLanguage;

            // Set the Index of the new Language
            currentLanguageIndex = langKeyList.IndexOf(currentLanguage);

            // Check whats the next Language in the supportedLanguages
            if (currentLanguageIndex < langKeyList.Count - 1) {
                nextLanguageIndex = currentLanguageIndex + 1;
            } else {
                nextLanguageIndex = 0;
            }
            nextLanguage = langKeyList[nextLanguageIndex];

            // Change the UI Elements
            executeUILanguage();

            // Change the Tutorial Language
            Globals.Controller.Tutorial.setTutorialLanguage(currentLanguageIndex);

        }

    }


    /// <summary>
    /// loads the desired Language File
    /// </summary>
    /// <param name="textAsset"></param>
    /// <param name="language"></param>
    private void loadLanguageFile(TextAsset textAsset, string language) {
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(textAsset.text);

        XmlFileStrings = new Hashtable();
        XmlElement element = xmldoc.DocumentElement[language];
        if (element != null) {
            readLanguageXML(element);
        } else {
            Debug.Log("Language Code in File is not the same as in Filename: " + language);
        }
    }

    /// <summary>
    /// Goes through the lines of the language Files and reads the keys and Values
    /// </summary>
    /// <param name="element"></param>
    public void readLanguageXML(XmlElement element) {
        IEnumerator elemEnum = element.GetEnumerator();
        while (elemEnum.MoveNext()) {
            XmlElement xmlItem = (XmlElement)elemEnum.Current;
            // Add Text if the Strings doesn't already contain the Key
            if (!XmlFileStrings.ContainsKey(xmlItem.GetAttribute("name"))) {
                XmlFileStrings.Add(xmlItem.GetAttribute("name"), xmlItem.InnerText);
            } else {
                Debug.LogWarning("Warning: Duplicate Key in Language Files: " + xmlItem.GetAttribute("name"));
            }
        }
    }


    /// <summary>
    /// Access strings in the currently selected language by supplying this getString function with
    /// the name identifier for the string used in the XML resource.<br></br>
    /// Is able to replace variables in Text marked with {0}, {1}, {2}, ...
    /// </summary>
    /// <param name="name"></param>
    /// <param name="arrayOfVariables">Array of Strings</param>
    /// <returns></returns>
    public string translateString(string name, string[] arrayOfVariables) {
        if (!XmlFileStrings.ContainsKey(name)) {
            Debug.LogError("The specified string does not exist: " + name);

            return "";
        }

        returnString = (string)XmlFileStrings[name];

        if (arrayOfVariables != null) {
            for (int i = 0; i < arrayOfVariables.Length; i++) {
                returnString = returnString.Replace("{" + i + "}", arrayOfVariables[i]);
            }
        }

        return returnString;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string translateString(string name) {
        return translateString(name, null);
    }


    public Hashtable getXmlFileStrings() {
        return XmlFileStrings;
    }


    public void changeToNextLanguage() {
        setLanguage(nextLanguage);
    }





    /// <summary>
    /// Changes the UI Texts to the desired Language
    /// </summary>
    private void executeUILanguage() {
        try {
            // BuildingMenu
            executeUIBuildingMenu();

            // BoostMenu
            executeUIBoostMenu();

            // SettingsMenu
            executeUISettings();

            // OfflineCoins
            executeOfflineCoins();

            // CoinsPerSecond
            executeCoinsPerSecond();

            // Quests
            executeQuests();

            // Feedback
            executeFeedback();

            // EnviGlass
            executeEnviGlass();

            // Google Playfab Link Confirmation
            executeCloudLinking();

            // Game start - Reconnect to Cloud to get SaveGame Data
            executeReconnectCloud();

            // Coming Soon Elements
            executeUIComingSoon();

            // Shop Popup
            executeShop();

        } catch (Exception e) {
            // Language System is completely catched, if all Static Textes in Unity GameObjects are already in english
            Globals.UICanvas.DebugLabelAddText(e.ToString(), true);
        }

    }

    /// <summary>
    /// Changes the UI Texts of BuildingMenu to the desired Language
    /// </summary>
    private void executeUIBuildingMenu() {
        Globals.UICanvas.translatedTMProElements.BuildingMenuLevel.text = translateString("buildingmenu_level") + ":";
        Globals.UICanvas.translatedTMProElements.BuildingMenuIncome.text = translateString("buildingmenu_income") + ":";
        Globals.UICanvas.translatedTMProElements.BuildingMenuEnvi.text = translateString("buildingmenu_envi") + ":";
        Globals.UICanvas.translatedTMProElements.BuildingMenuCoinRespawnTime.text = translateString("buildingmenu_coinRespawnTime") + ":";
        Globals.UICanvas.translatedTMProElements.BuildingMenuCoinTapCash.text = translateString("buildingmenu_coinTapCash") + ":";
        Globals.UICanvas.translatedTMProElements.BuildingMenuItemDescription.text = translateString("buildingmenu_item_description");
        Globals.UICanvas.translatedTMProElements.BuildingMenuItemDragHint.text = translateString("buildingmenu_item_drag_hint");
        Globals.UICanvas.translatedTMProElements.BuildingMenuAdButtonHint.text =
            translateString("buildingmenu_adbutton_why_grey", new string[] { Building.DelayUnlockProcessMaxReducePercent.ToString() });
    }


    /// <summary>
    /// Changes the UI Texts of BoostMenu to the desired Language
    /// </summary>
    private void executeUIBoostMenu() {

        Globals.UICanvas.uiElements.BoostPopUpWarningText.text = "";
        Globals.UICanvas.translatedTMProElements.BoostPopUpHeading.text = translateString("boostmenu_header");
        Globals.UICanvas.translatedTMProElements.BoostPopUpDescriptionTimeLeft.text = translateString("boostmenu_timeleft");
        Globals.UICanvas.translatedTMProElements.BoostPopUpSubtext.text = translateString("boostmenu_button_subtext");
        // In Description of ButtonClick there has to be some Variables
        Globals.UICanvas.translatedTMProElements.BoostPopUpDescriptionButtonClick.text = translateString(
                    "boostmenu_button_description",
                    new string[] {
                        Globals.KaloaSettings.boostMultiplier.ToString(),
                        translateString(Globals.Game.currentWorld.worldName),
                        ((int)(Globals.KaloaSettings.boostSecPerVid / 60 / 60)).ToString() });
    }


    /// <summary>
    /// Changes the UI Texts of SettingsMenu to the desired Language
    /// </summary>
    private void executeUISettings() {

        // Translate 
        translateSettings();
        translateSettingsRateUs();
        translateSettingsSound();
        translateSettingsMusic();
        translateSettingsNotifications();
        translateSettingsVibration();
        translateSettingsGoogle();

    }

    public void executeCloudLinking() {
        if (Globals.UserSettings.linkedGoogleAccountToPlayfab) {
            Globals.UICanvas.translatedTMProElements.LinkingPopUpHeading.text = translateString("linking_confirmation_header_unlink");
            Globals.UICanvas.translatedTMProElements.LinkingPopUpDescription.text = translateString("linking_confirmation_description_unlink");          
            Globals.UICanvas.translatedTMProElements.LinkingPopUpBtn.text = translateString("linking_confirmation_btn_unlink");
        } else {
            Globals.UICanvas.translatedTMProElements.LinkingPopUpHeading.text = translateString("linking_confirmation_header_link");
            Globals.UICanvas.translatedTMProElements.LinkingPopUpDescription.text = translateString("linking_confirmation_description_link");
            Globals.UICanvas.translatedTMProElements.LinkingPopUpBtn.text = translateString("linking_confirmation_btn_link");
        }
        Globals.UICanvas.translatedTMProElements.LinkingPopUpHint.text = "";
    }


    private void executeReconnectCloud() {
        Globals.UICanvas.translatedTMProElements.ReconnectCloudPopUpHeading.text = translateString("reconnect_cloud_header");
        Globals.UICanvas.translatedTMProElements.ReconnectCloudPopUpDescription.text = translateString("reconnect_cloud_description");
        Globals.UICanvas.translatedTMProElements.ReconnectCloudPopUpBtnConnectCloud.text = translateString("reconnect_cloud_btn_loadCloud");
        Globals.UICanvas.translatedTMProElements.ReconnectCloudPopUpBtnNewGame.text = translateString("reconnect_cloud_btn_newGame");
        Globals.UICanvas.translatedTMProElements.ReconnectCloudPopUpHint.text = "";
    }


    /// <summary>
    /// Changes the UI Texts of Coming Soon PopUps to the desired Language
    /// </summary>
    private void executeUIComingSoon() {
        Globals.UICanvas.translatedTMProElements.MapComingSoon.text = translateString("comingSoon_map");
    }


    /// <summary>
    /// Changes the UI Texts to the desired Language
    /// </summary>
    public void executeOfflineCoins() {
        Globals.UICanvas.translatedTMProElements.PopUpOfflineCoinsHeading.text = translateString("offlinecoins_header");
        Globals.UICanvas.translatedTMProElements.PopUpOfflineCoinsButtonOKText.text = translateString("offlinecoins_button_claim");
        // InThisTime Text contains the Variable of the Worldname, which has to be Translated too
        Globals.UICanvas.translatedTMProElements.PopUpOfflineCoinsInThisTime.text = 
            translateString("offlinecoins_inthistime", 
            new string[] { translateString(Globals.Game.currentWorld.worldName + "_possessive")  }
            );
    }

    /// <summary>
    /// Changes the UI Texts to the desired Language
    /// </summary>
    public void executeCoinsPerSecond() {
        Globals.UICanvas.translatedTMProElements.CoinsPerSecondHeading.text = translateString("buildingmenu_income");
    }
    

    /// <summary>
    /// Changes the UI Texts to the desired Language
    /// </summary>
    public void executeQuests() {
        Globals.UICanvas.translatedTMProElements.PopUpQuestsStart.text 
            = translateString("quest_button_startquest");
    }

    /// <summary>
    /// Changes the UI Texts to the desired Language
    /// </summary>
    public void executeFeedback() {
        Globals.UICanvas.translatedTMProElements.PopUpFeedbackHeading.text = translateString("feedbackPopUp_heading");
        Globals.UICanvas.translatedTMProElements.PopUpFeedbackDescription.text = translateString("feedbackPopUp_description");
        Globals.UICanvas.translatedTMProElements.PopUpFeedbackBtnFeedback.text = translateString("feedbackPopUp_button_text");
        Globals.UICanvas.translatedTMProElements.PopUpFeedbackRewardInfo.text = translateString("quest_noReward");
    }

    /// <summary>
    /// Changes the UI Texts to the desired Language
    /// </summary>
    public void executeEnviGlass() {
        Globals.UICanvas.translatedTMProElements.PopUpEnviGlassHeading.text
            = translateString("enviglass_heading");
    }

    /// <summary>
    /// Changes the UI Texts to the desired Language
    /// </summary>
    public void executeShop() {
        Globals.UICanvas.translatedTMProElements.ShopPopUpHeadingBoosts.text = translateString("shop_heading_boosts");
        Globals.UICanvas.translatedTMProElements.ShopPopUpHeadingItems.text = translateString("shop_heading_items");
        Globals.UICanvas.translatedTMProElements.ShopPopUpHeadingEmeralds.text = translateString("emeralds_plural");
    }

    /// <summary>
    /// Changes the UI Texts to the desired Language
    /// </summary>
    public void translateSettings() {
        // Set the Buttons in the Menu

        Globals.UICanvas.translatedTMProElements.SettingsMenuHeading.text = translateString("settingsmenu_header");

        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("H_Community").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_community_header");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Like/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_community_like");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Idea/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_community_idea");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Idea/Button/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_community_idea_btn");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Feedback/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_community_report");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Feedback/Button/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_community_report_btn");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Instagram/Button/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_open");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("GooglePlayGames/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_googleplay_login");

        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("H_GooglePlayGames").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_googleplay_header");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("GooglePlayGames/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_googleplay_login");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("GoogleServices/ButtonAchievements/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_googleplay_btn_achievements");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("GoogleServices/ButtonLeaderboard/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_googleplay_btn_leaderboard");

        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("H_Settings").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_settings_header");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Sound/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_settings_sound");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Music/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_settings_music");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Vibration/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_settings_vibration");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Notifications/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_settings_notifications");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Language/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_settings_languages");

        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("H_LegalInformation").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_legalInformation_header");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("PrivacyPolicy/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_legalInformation_privacyPolicy");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("PrivacyPolicy/Button/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_open");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("AGB/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_legalInformation_termsOfService");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("AGB/Button/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_open");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Impressum/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_legalInformation_imprint");
        Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Impressum/Button/Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_open");

        // Language Button
        Globals.UICanvas.translatedTMProElements.SettingsMenuLanguageButton.text = supportedLanguages[currentLanguage];

        // VersionNumber
        Globals.UICanvas.translatedTMProElements.SettingsVersionNumber.text = "Build: " + "v" + Application.version;
    }

    public void translateSettingsRateUs() {

        if (PlayerPrefs.GetInt("global_stat_GameOpenings") > 20 && !Globals.UserSettings.hasRated) {
            // Wenn mehrmals im Spiel gewesen und hat noch nicht eine Google Bewertung gemacht
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Like/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_community_like_btn_rate");
        } else {
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Like/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_community_like_btn_share");
        }
    }

    public void translateSettingsSound() {
        if (Globals.UserSettings.hasSound) {
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Sound/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_on");
        } else {
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Sound/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_off");
        }
    }

    public void translateSettingsMusic() {
        if (Globals.UserSettings.hasMusic) {
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Music/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_on");
        } else {
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Music/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_off");
        }
    }

    public void translateSettingsNotifications() {
        if (Globals.UserSettings.hasNotifications) {
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Notifications/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_on");
        } else {
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Notifications/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_off");
        }
    }

    public void translateSettingsVibration() {
        if (Globals.UserSettings.hasVibration) {
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Vibration/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_on");
        } else {
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Vibration/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translateString("settingsmenu_btn_off");
        }
    }

    public void translateSettingsGoogle() {
        
        if(Globals.UICanvas.uiElements.SettingsMenuContent != null) {
            
            if (Globals.Controller.GPiOS.isloggedIn()) {
                Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("GooglePlayGames/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text =
                    translateString("settingsmenu_googleplay_login_btn_sign_out");
                Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("GooglePlayGames/Text").GetComponent<TMPro.TextMeshProUGUI>().text =
                    translateString("settingsmenu_googleplay_signed_in_as", new string[] { Social.localUser.userName });
            } else {
                Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("GooglePlayGames/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text =
                    translateString("settingsmenu_googleplay_login_btn_sign_in");
                Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("GooglePlayGames/Text").GetComponent<TMPro.TextMeshProUGUI>().text =
                    translateString("settingsmenu_googleplay_login");
            }


            // Google linked with Cloud
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("LinkGoogleToCloud/Text").GetComponent<TMPro.TextMeshProUGUI>().text =
                    translateString("settingsmenu_googleplay_link");
            Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("LinkGoogleToCloud/Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text =
                    translateString("settingsmenu_googleplay_link_btn");
        }
    }


}
