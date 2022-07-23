using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests {
    public class TestSuiteSavingSystem {

        public InitGame Game;

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
            UnityEngine.Object.Destroy(Game.gameObject);
            // Reset outside communication
            Globals.KaloaSettings.preventPlayfabCommunication = false;
            Globals.KaloaSettings.preventIAPCommunication = false;
            Globals.KaloaSettings.preventGoogleCommunication = false;
            Globals.KaloaSettings.preventSaving = false;
            Globals.KaloaSettings.skipTutorial = false;

            yield return null;
        }


        // Test for: LevelUp and CurrentWorld Coins are raising
        [UnityTest]
        public IEnumerator CheckSaveGameCompare() {

            // Inits
            SaveGame localSave = null;
            SaveGame cloudSave = null;

            /* The Compare Function has different result strings for the 9 states tha can occur 
             * 
             - NULL States: bothNull_newGame, cloudNull_localGame, localNull_cloudGame
             - COMPARE States: compare_popUp, compare_localGame, compare_cloudGame, 
             - ERROR States: compare_error, localNull_error, cloudNull_error

             */

            // ------------- Check NULL States -------------
            Assert.AreEqual("bothNull_newGame", SavingSystem.compareSaveGame(localSave, cloudSave));

            localSave = new SaveGame();
            cloudSave = null;
            Assert.AreEqual("cloudNull_localGame", SavingSystem.compareSaveGame(localSave, cloudSave));

            localSave = null;
            cloudSave = new SaveGame();
            Assert.AreEqual("localNull_cloudGame", SavingSystem.compareSaveGame(localSave, cloudSave));



            // ------------- Check COMPARE States -------------
            localSave = new SaveGame();
            cloudSave = new SaveGame();
            System.DateTime localTime_GameClosed = DateTime.Now;
            System.DateTime cloudTime_GameClosed = DateTime.Now;

            // Manipulate Time
            localTime_GameClosed = localTime_GameClosed.AddHours(24); // 1 day

            // Set SaveGame Stats
            localSave.user.stats.LastDateGameClosed = localTime_GameClosed;
            localSave.user.scoreTotalLevels = 123;
            cloudSave.user.stats.LastDateGameClosed = cloudTime_GameClosed;
            cloudSave.user.scoreTotalLevels = 1;

            Assert.AreEqual("compare_localGame", SavingSystem.compareSaveGame(localSave, cloudSave));




            // Manipulate Time
            localTime_GameClosed = DateTime.Now;
            cloudTime_GameClosed = localTime_GameClosed.AddHours(24); // 1 day

            // Set SaveGame Stats
            localSave.user.stats.LastDateGameClosed = localTime_GameClosed;
            localSave.user.scoreTotalLevels = 1;
            cloudSave.user.stats.LastDateGameClosed = cloudTime_GameClosed;
            cloudSave.user.scoreTotalLevels = 123;

            Assert.AreEqual("compare_cloudGame", SavingSystem.compareSaveGame(localSave, cloudSave));




            // Set SaveGame Stats
            localSave.user.stats.LastDateGameClosed = localTime_GameClosed;
            localSave.user.scoreTotalLevels = 123;
            cloudSave.user.stats.LastDateGameClosed = cloudTime_GameClosed;
            cloudSave.user.scoreTotalLevels = 1;

            Assert.AreEqual("compare_popUp", SavingSystem.compareSaveGame(localSave, cloudSave));



            yield return null;
        }


    }
}
