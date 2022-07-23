using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class TestSuiteWorlds {

        public InitGame Game;

        IdleNum zeroIdle = new IdleNum(0);
        HybridBuilding hybridbuilding;


        [UnitySetUp]
        public IEnumerator UnitySetUp() {
            // TestSettings
            Globals.KaloaSettings.preventPlayfabCommunication = true;
            Globals.KaloaSettings.preventIAPCommunication = true;
            Globals.KaloaSettings.preventSaving = true;
            Globals.KaloaSettings.preventGoogleCommunication = true;
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
            Globals.Game.currentUser.wasSignedIn = false; PlayerPrefs.SetString("global_stat_firstGameLoad", "222");


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





        // Test for: World has all Things a World should have
        [UnityTest]
        public IEnumerator CheckWorldBasics() {

            //foreach (World aWorld in Globals.Game.saveGame.WorldsDictionary.Values) {

                // CoinIncome
                Assert.IsNotNull(Globals.Game.currentWorld.gameObject.GetComponent<CoinIncome>());

                //LevelUpIndicator
                Assert.IsNotNull(Globals.Game.currentWorld.levelUpIndicator_transparent);
                Assert.IsNotNull(Globals.Game.currentWorld.levelUpIndicator_updateable);
                Assert.IsNotNull(Globals.Game.currentWorld.levelUpIndicator_upgradeable);

                // Monuments
                Assert.IsNotNull(Globals.Game.currentWorld.monumentSlot);

            //}

            yield return null;
        }



    }
}
