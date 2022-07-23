using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

namespace Tests
{
    public class TestSuiteGoogle {

        public InitGame Game;


        [UnitySetUp]
        public IEnumerator UnitySetUp() {
            // TestSettings
            Globals.KaloaSettings.preventPlayfabCommunication = true;
            Globals.KaloaSettings.preventIAPCommunication = true;
            Globals.KaloaSettings.preventGoogleCommunication = true;
            Globals.KaloaSettings.preventSaving = true;
            Globals.KaloaSettings.skipTutorial = true;

            // Load the MainScene
            SceneManager.LoadScene("WorldScene_Village1");

            // Wait one Frame until Scene is loaded
            yield return null;

            // Get Game-Object and Init the Game
            Game = GameObject.Find("/_BaseObjects/InitGame").GetComponent<InitGame>();
            Globals.Game.currentUser.wasSignedIn = false;

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

        // Test for: Google Ads Loaded
        // Works only if Smartphone is Online
        [UnityTest]
        public IEnumerator CheckGoogleAdsLoaded() {

            yield return new WaitForSeconds(10);

            // Check if Controller is loaded properly
            Assert.IsNotNull(GameObject.Find("AdWrapper"));
            Assert.IsNotNull(GameObject.Find("AdWrapper").GetComponent<AdWrapper>());
            Assert.IsNotNull(Globals.Controller.Ads);

            // Check if controller working
            Assert.IsTrue(Globals.Controller.Ads.isBoostAdLoaded());
            Assert.IsTrue(Globals.Controller.Ads.isOfflineCoinsAdLoaded());
            Assert.IsTrue(Globals.Controller.Ads.isForwardBuildingProgressAdLoaded());

            yield return null;
        }

        // Test for: Google Firebase
        // Works only if Smartphone is Online
        [UnityTest]
        public IEnumerator CheckGoogleFireBase() {

            yield return new WaitForSeconds(4);
            Assert.IsNotNull(Globals.Controller.Firebase);
            
        }
        

    }
}