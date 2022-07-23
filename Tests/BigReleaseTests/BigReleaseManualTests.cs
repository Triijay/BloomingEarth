using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class BigReleaseManualTests {

        public InitGame Game;

        [UnitySetUp]
        public IEnumerator UnitySetUp() {
            // Load the MainScene
            SceneManager.LoadScene("WorldScene_Village1");

            // Wait one Frame until Scene is loaded
            yield return null;

            // Get Game-Object and Init the Game
            Game = GameObject.Find("/InitGame").GetComponent<InitGame>();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown() {
            // Destroy the GameObject to not affect other tests
            Object.Destroy(Game.gameObject);

            yield return null;
        }




        // Test for: All Things we cant automatically test
        [UnityTest]
        public IEnumerator AllManualTestsOK() {

            // If something here can tested automatically, make a test in other Test class and remove it here


            // 1 Offline Coins

            // 1.1 - Close App (pause Mode) - wait 60 Seconds 
            // -> Offline Coin PopUp comes up
            // -> You are getting coins on Claim Button

            // 1.2 - Close App - Force the Game to quit completely - wait 60 Seconds 
            // -> Offline Coin PopUp comes up
            // -> You are getting coins on Claim Button



            // 2 Notifications
            // Turn on TestNotification in InitGame Object - Close App - Wait 10 Seconds
            // -> Test Notification should come


            // 3 AdButtons

        // Turn on Debug Mode in Globals.KaloaSettings OFF !

            // 3.1 - Close App (pause Mode) - wait 60 Seconds 
            // -> Offline Coin PopUp comes up
            // -> You are getting Ad on 2x Button and after it double-Coins

            // 3.2 - Restart App - Build House - At start of DelayUnlockProcess go to BuildingMenu and Press the -30min Button
            // -> Ad should come
            // -> House should be instant builded after Ad

            // 3.3 - Check current Income and Boost Information when you click on the Current Coins - Go to Boost Menu + Press Boost Button
            // -> Ad should come
            // -> PopUp should come saying that Boost is now running (when its implemented)
            // -> in Current Coins Information now should be Info that is boosted and the current Income should be doubled


            // 4 Camera Test
            // 

        // IMPORTANT: Turn on Debug Mode in InitGame-Object ON before Release



            Assert.IsTrue(false);


            yield return null;

        }


    }
}
