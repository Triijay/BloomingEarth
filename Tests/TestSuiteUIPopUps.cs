using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests {
    public class TestSuiteUIPopUps {

        public InitGame Game;

        /// <summary>
        /// Notice that this SetUp dont resets the game at start
        /// </summary>
        /// <returns></returns>
        [UnitySetUp]
        public IEnumerator UnitySetUp() {
            // TestSettings
            Globals.KaloaSettings.preventPlayfabCommunication = true;
            Globals.KaloaSettings.preventIAPCommunication = true;
            Globals.KaloaSettings.preventGoogleCommunication = true;
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




        // Test for: Check all Panels and PopUps loaded
        // -> Fails if one or more GameObjects not found. e.G. if someone renamed Objects or restructured the hierarchy
        [UnityTest]
        public IEnumerator CheckAllPanelsAndPopUpsLoaded() {

            // Check if InitGame has loaded the Global GameObjects
            Assert.IsNotNull(Globals.UICanvas.uiElements.MainCamera, "Main Camera not initialized");
            Assert.IsNotNull(Globals.UICanvas.uiElements.Canvas, "Canvas not initialized");
            Assert.IsNotNull(Globals.UICanvas.uiElements.PopUps, "PopUps not initialized");
            Assert.IsNotNull(Globals.UICanvas.uiElements.LevelUpAmountSlider, "LevelUpAmountSlider not initialized");
            Assert.IsNotNull(Globals.UICanvas.uiElements.PopUpOfflineCoins, "PopUpOfflineCoins not initialized");
            Assert.IsNotNull(Globals.UICanvas.uiElements.BuildingMenu, "BuildingMenu not initialized");
            Assert.IsNotNull(Globals.UICanvas.uiElements.BuildingMenuBuildingName, "BuildingName not initialized");
            Assert.IsNotNull(Globals.UICanvas.uiElements.SettingsMenu, "SettingsMenu not initialized");
            Assert.IsNotNull(Globals.UICanvas.uiElements.SettingsMenuContent, "SettingsMenuContent not initialized");
            Assert.IsNotNull(Globals.UICanvas.uiElements.BoostPopUp, "BoostPopUp not initialized");

            Assert.IsTrue(Globals.UICanvas.uiElements.PopUps.activeSelf);

            yield return null;
        }


        // Test for: Check all PopUps inactive
        // -> Fails if one or more PopUp is setted to active (happens through development)
        [UnityTest]
        public IEnumerator CheckAllPopUpsInactive() {

            // If Cloud save is available, don´t show popup for test
            Globals.UICanvas.uiElements.SaveGamePopUp.SetActive(false);

            Transform[] children = Globals.UICanvas.uiElements.PopUps.GetComponentsInChildren<Transform>();

            Assert.IsFalse(children.Length < 1, "PopUps not found");
            Assert.AreEqual(1, children.Length, "One or More PopUps are setted active in unity, please set them to inactive");

            yield return null;
        }


    }
}
