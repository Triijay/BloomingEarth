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
    public class BuildTestSuiteGoogle {

        public InitGame Game;


        [UnitySetUp]
        public IEnumerator UnitySetUp() {
            // Load the MainScene
            SceneManager.LoadScene("WorldScene_Village1");

            // Wait one Frame until Scene is loaded
            yield return null;

            // Get Game-Object and Init the Game
            Game = GameObject.Find("/InitGame").GetComponent<InitGame>();
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

            yield return null;
        }


        // Test for: Google Logs In
        // Works only if Smartphone is Online
        [UnityTest]
        public IEnumerator BuildTestGoogleTriesToConnect() {

            Debug.Log("Expecting log from now");
            LogAssert.Expect(LogType.Log, "Error: Not implemented on this platform");

            yield return new WaitForSeconds(4);


            yield return null;
        }

        // Test for: Google Logs In
        // Works only if Smartphone is Online
        [UnityTest]
        public IEnumerator BuildTestGoogleLoggedIn() {

            yield return new WaitForSeconds(4);

            Assert.IsTrue(Social.localUser.authenticated);

            yield return null;
        }



    }
}