using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class TestSuiteSettings {

        public InitGame Game;

        [UnitySetUp]
        public IEnumerator UnitySetUp() {

            // TestSettings
            Globals.KaloaSettings.preventPlayfabCommunication = true;
            Globals.KaloaSettings.preventIAPCommunication = true;
            Globals.KaloaSettings.preventGoogleCommunication = false;
            Globals.KaloaSettings.preventSaving = true;
            Globals.KaloaSettings.skipTutorial = true;

            // Ensure that all components will be loaded
            Globals.Game.isGameStarting = true;

            // Load the MainScene
            SceneManager.LoadScene("WorldScene_Village1");

            // Wait one Frame until Scene is loaded
            yield return null;

            // Shut Up Google Sign In
            PlayerPrefs.SetString("global_settings_wasSignedIn", "false"); PlayerPrefs.SetString("global_stat_firstGameLoad", "222");

            // Get Game-Object and Init the Game
            Game = GameObject.Find("/_BaseObjects/InitGame").GetComponent<InitGame>();

            // Wait for one Frame until Component is loaded
            yield return null;

            // Delete all PlayerPrefs and start a new Game
            SavingSystem.saveOrLoadPlayfab = false;
            Game.resetGameForAdmins();

            Globals.UICanvas.uiElements.PopUpQuests.SetActive(false);


            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown() {
            // Destroy the GameObject to not affect other tests
            Object.Destroy(Game.gameObject);
            // Reset outside communication
            Globals.KaloaSettings.preventPlayfabCommunication = false;
            Globals.KaloaSettings.preventIAPCommunication = false;
            Globals.KaloaSettings.preventGoogleCommunication = false;
            Globals.KaloaSettings.preventSaving = false;
            Globals.KaloaSettings.skipTutorial = false;

            yield return null;
        }







        // Test for: LevelUp and CurrentWorld Coins are raising, Offline Coins at the Start should block any other PopUp
        [UnityTest]
        public IEnumerator TestSettingsButtonAndPopUpBlock() {

            GameObject settingsMenu = Globals.UICanvas.uiElements.PopUps.transform.Find("Settings").gameObject;

            // Check if Settings Menu is not shown at Start
            Assert.IsFalse(settingsMenu.activeSelf, "Button Settings is showed at start, thats not ok");

            Button buttonSettings = Globals.UICanvas.uiElements.Canvas.transform.Find("Panel_LeftMenu").Find("ButtonSettings").gameObject.GetComponent<Button>();
            Assert.IsNotNull(buttonSettings, "ButtonSettings is Null");

            // Turn OfflineCoin PopUp on, to ensure, that the OfflineCoinPopUp blocks other PopUps
            Globals.UICanvas.uiElements.PopUpOfflineCoins.SetActive(true);

            // Click Button
            buttonSettings.onClick.Invoke();

            yield return null;

            // Check if Settings Menu is blocked by OfflineCoinPopUp
            Assert.IsFalse(settingsMenu.activeSelf, "SettingsMenu not been blocked by OfflineCoinPopUp");

            // Eliminate OfflineCoinPopUp
            Globals.UICanvas.uiElements.PopUpOfflineCoins.SetActive(false);

            yield return null;

            // Click Button
            Assert.IsNotNull(buttonSettings.GetComponent<Button>(), "ButtonSettings is not a Button");
            buttonSettings.onClick.Invoke();

            // Check if Settings Menu shown after Button Click on Settings
            Assert.IsTrue(settingsMenu.activeSelf);
        }

        // Test for: SettingsMenu - Buttons e.G are functional
        [UnityTest]
        public IEnumerator TestSettingsMenu() {

            GameObject settingsMenu = Globals.UICanvas.uiElements.PopUps.transform.Find("Settings").gameObject;

            // Eliminate OfflineCoinPopUp
            Globals.UICanvas.uiElements.PopUpOfflineCoins.SetActive(false);

            yield return null;

            settingsMenu.SetActive(true);

            // Check if Settings Menu shown
            Assert.IsTrue(settingsMenu.activeSelf);


            // Check UserSettings: Sound
            GameObject soundObject = Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Sound").gameObject;
            Button soundBtn = soundObject.transform.Find("Button").gameObject.GetComponent<Button>();
            string soundBtnString = soundObject.transform.Find("Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text;
            Assert.IsNotNull(soundObject);
            Assert.IsNotNull(soundBtn);
            Assert.IsNotEmpty(soundBtnString);
            soundBtn.onClick.Invoke(); // Invoke the Button
            yield return null;
            Assert.AreNotEqual(soundBtnString, soundObject.transform.Find("Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text, "Button Sound pressed, but Text didn't change");


            // Check UserSettings: Music
            GameObject musicObject = Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Music").gameObject;
            Button musicBtn = musicObject.transform.Find("Button").gameObject.GetComponent<Button>();
            string musicBtnString = musicObject.transform.Find("Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text;
            Assert.IsNotNull(musicObject);
            Assert.IsNotNull(musicBtn);
            Assert.IsNotEmpty(musicBtnString);
            musicBtn.onClick.Invoke(); // Invoke the Button
            yield return null;
            Assert.AreNotEqual(musicBtnString, musicObject.transform.Find("Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text, "Button Music pressed, but Text didn't change");

            // Check UserSettings: Vibration
            GameObject vibrationObject = Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Vibration").gameObject;
            Button vibrationBtn = vibrationObject.transform.Find("Button").gameObject.GetComponent<Button>();
            string vibrationBtnString = vibrationObject.transform.Find("Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text;
            Assert.IsNotNull(vibrationObject);
            Assert.IsNotNull(vibrationBtn);
            Assert.IsNotEmpty(vibrationBtnString);
            vibrationBtn.onClick.Invoke(); // Invoke the Button
            yield return null;
            Assert.AreNotEqual(vibrationBtnString, vibrationObject.transform.Find("Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text, "Button Vibration pressed, but Text didn't change");

            // Check UserSettings: Notifications
            GameObject notificationsObject = Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Notifications").gameObject;
            Button notificationsBtn = notificationsObject.transform.Find("Button").gameObject.GetComponent<Button>();
            string notificationsBtnString = notificationsObject.transform.Find("Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text;
            Assert.IsNotNull(notificationsObject);
            Assert.IsNotNull(notificationsBtn);
            Assert.IsNotEmpty(notificationsBtnString);
            notificationsBtn.onClick.Invoke(); // Invoke the Button
            yield return null;
            Assert.AreNotEqual(notificationsBtnString, notificationsObject.transform.Find("Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text, "Button Notifications pressed, but Text didn't change");

            // LanguageButton
            if (Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Language") != null) {
                GameObject languageObject = Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("Language").gameObject;
                Button languageBtn = languageObject.transform.Find("Button").gameObject.GetComponent<Button>();
                string languageBtnString = languageObject.transform.Find("Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text;
                Assert.IsNotNull(languageObject);
                Assert.IsNotNull(languageBtn);
                Assert.IsNotEmpty(languageBtnString);
                languageBtn.onClick.Invoke(); // Invoke the Button
                yield return null;
                Assert.AreNotEqual(languageBtnString, languageObject.transform.Find("Button/Text").GetComponent<TMPro.TextMeshProUGUI>().text, "Button Language pressed, but Text didn't change");
            }

            // Check Google Play
            GameObject gpgObject = Globals.UICanvas.uiElements.SettingsMenuContent.transform.Find("GooglePlayGames").gameObject;
            Button gpgBtn = gpgObject.transform.Find("Button").gameObject.GetComponent<Button>();
            Assert.IsNotNull(gpgObject);
            Assert.IsNotNull(gpgBtn);
            gpgBtn.onClick.Invoke(); // Invoke the Button
            LogAssert.Expect(LogType.Log, "TryingToSignInGooglePlay");

        }



        // Test for: LevelUp and CurrentWorld Coins are raising
        [UnityTest]
        public IEnumerator CheckUserSettingsLoaded() {

            Assert.IsNotNull(Globals.UserSettings.hasRated, "UserSettings hasRated not Loaded");
            Assert.IsNotNull(Globals.UserSettings.hasSound, "UserSettings hasSound not Loaded");
            Assert.IsNotNull(Globals.UserSettings.hasMusic, "UserSettings hasMusic not Loaded");
            Assert.IsNotNull(Globals.UserSettings.hasNotifications, "UserSettings hasNotifications not Loaded");
            Assert.IsNotNull(Globals.UserSettings.hasVibration, "UserSettings hasVibration not Loaded");

            yield return null;

        }


        // Test for: InitGame Variables
        [UnityTest]
        public IEnumerator CheckInitGamePublicVariablesOK() {

            // our Test-Notification must be false to release
            Assert.IsFalse(Game.testNotificationOn, "CRITICAL WARNING: Test Notification must be turned to FALSE for release");

            //PopupMinOfflineTime greater 60 Seconds
            // This covers up to not Upset the User, especially when he watches Ads or checks his Achievements
            Assert.GreaterOrEqual(Game.PopupMinOfflineTime, 120, "CRITICAL WARNING: PopupMinOfflineTime  must be greater 120 Seconds for release");

            // Limit Offline Coins
            Assert.GreaterOrEqual(Game.limitOfflineCoinsHours, 2, "CRITICAL WARNING: limitOfflineCoinsHours  must be greater than 2 Hours for release");

            // Boost
            Assert.GreaterOrEqual(Game.boostMultiplier, 2, "CRITICAL WARNING: boostMultiplier  must be greater than 2x for release");
            Assert.GreaterOrEqual(Game.boostSecPerVid, 1*60*60, "CRITICAL WARNING: boostSecPerVid  must be greater than 1 Hours for release");
            Assert.GreaterOrEqual(Game.boostMaxSeconds, 2*60*60, "CRITICAL WARNING: boostMaxSeconds  must be greater than 2 Hours for release");

            yield return null;
        }

        // Test for: Crashlytics Test
        [UnityTest]
        public IEnumerator CriticalCheckTesterDisabled() {

            // CrashlyticsTester
            GameObject CrashlyticsObject = Globals.UICanvas.uiElements.InitGameObject.transform.Find("CrashlyticsTester").gameObject;

            Assert.IsNotNull(CrashlyticsObject, "CrashlyticsTester GameObject not found");

            CrashlyticsTester tester = CrashlyticsObject.transform.GetComponentInChildren<CrashlyticsTester>();

            Assert.IsNotNull(tester, "tester Component on CrashlyticsTester not found");

            Assert.IsFalse(tester.isActiveAndEnabled, "CRITICAL ERROR: CrashlyticsTester Component MUST be disabled");


            // FirebaseTester
            GameObject FirebaseObject = Globals.UICanvas.uiElements.InitGameObject.transform.Find("FirebaseTester").gameObject;

            Assert.IsNotNull(FirebaseObject, "FirebaseTester GameObject not found");

            FirebaseTester ftester = FirebaseObject.transform.GetComponentInChildren<FirebaseTester>();

            Assert.IsNotNull(ftester, "tester Component on FirebaseTester not found");

            Assert.IsFalse(ftester.isActiveAndEnabled, "CRITICAL ERROR: FirebaseTester Component MUST be disabled");



            yield return null;
        }


        // Test for: Language Files Valid
        [UnityTest]
        public IEnumerator TestLanguageFiles() {

            // Test if any LanguageFile is loaded
            Assert.IsNotNull(Globals.Controller.Language.getXmlFileStrings());

            // Without PlayerPrefs the Language should always be setted to the FallbackLanguage
            Assert.IsNotEmpty(Language.FALLBACK_LANGUAGE);
            Assert.IsTrue(Language.supportedLanguages.TryGetValue(Globals.Controller.Language.currentLanguage, out string bla), "Language without PlayerPrefs is not a supported Language!");

            // set a valid Language
            Globals.Controller.Language.setLanguage("de-DE");

            Assert.AreEqual("de-DE", Globals.Controller.Language.currentLanguage);

            // set a invalid Language -> FallbackLanguage should be setted
            Globals.Controller.Language.setLanguage("miau-MAUZ");

            Assert.AreEqual(Language.FALLBACK_LANGUAGE, Globals.Controller.Language.currentLanguage);

            yield return null;

            // load FallbackLanguage
            Globals.Controller.Language.setLanguage(Language.FALLBACK_LANGUAGE);
            Hashtable fileFallback = Globals.Controller.Language.getXmlFileStrings();

            // ExampleTexts
            string egText = Globals.UICanvas.translatedTMProElements.SettingsMenuHeading.text;
            string egText2 = Globals.UICanvas.translatedTMProElements.BoostPopUpSubtext.text;
            string egText3 = Globals.UICanvas.translatedTMProElements.BuildingMenuAdButtonHint.text;

            foreach (KeyValuePair<string, string> lang in Language.supportedLanguages) {

                if (lang.Key == Language.FALLBACK_LANGUAGE) { 
                    continue; 
                };

                Globals.Controller.Language.setLanguage(lang.Key);
                Hashtable fileOtherLang = Globals.Controller.Language.getXmlFileStrings();

                Assert.AreEqual(fileFallback.Count, fileOtherLang.Count, "LanguageFiles have not the same size as FallbackLanguage: " + lang.Key + ".");
                
                // Check Keys
                foreach (DictionaryEntry dictionaryEntry in fileFallback) {
                    Assert.IsTrue(fileOtherLang.ContainsKey(dictionaryEntry.Key), "Key: " + dictionaryEntry.Key + " is missing in LanguageFile " + lang.Key);
                }

                yield return null;

                // Check if with our exampleTexts happend something
                Assert.AreNotEqual(egText, Globals.UICanvas.translatedTMProElements.SettingsMenuHeading.text);
                Assert.AreNotEqual(egText2, Globals.UICanvas.translatedTMProElements.BoostPopUpSubtext.text);
                Assert.AreNotEqual(egText3, Globals.UICanvas.translatedTMProElements.BuildingMenuAdButtonHint.text);

            }

        }


        // Test for: Ads Loaded
        [UnityTest]
        public IEnumerator CheckAdsInitialized() {
            // Start new Game with PlayerPrefs
            Game.initGameStartUp(false, System.DateTime.Now);
            // Test if any Ad is loaded
            LogAssert.Expect(LogType.Log, "CreateRewardedAd");

            yield return null;

        }


    }
}
