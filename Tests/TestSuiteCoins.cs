using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class TestSuiteCoins {

        public InitGame Game;

        IdleNum zeroIdle = new IdleNum(0);
        HybridBuilding hybridbuilding;


        [UnitySetUp]
        public IEnumerator UnitySetUp() {
            // TestSettings
            Globals.KaloaSettings.preventPlayfabCommunication = true;
            Globals.KaloaSettings.preventIAPCommunication = true;
            Globals.KaloaSettings.preventGoogleCommunication = true;
            Globals.KaloaSettings.preventSaving = false;
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

            Globals.UICanvas.uiElements.SaveGamePopUp.SetActive(false);
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



        // Test for: CoinIncome Objects
        [UnityTest]
        public IEnumerator TestsCoinIncomeBasics() {

            // Boost Timer
            Assert.IsNotNull(Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer);
            Assert.IsNotNull(Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer);

            // Income Interval (ms) ok
            Assert.Less(49, Globals.Game.currentWorld.CoinIncomeManager.incomeInterval);
            Assert.Less(Globals.Game.currentWorld.CoinIncomeManager.incomeInterval, 500);

            yield return null;
        }

        // Test for: Coin producing
        [UnityTest]
        public IEnumerator TestsCoinProducingIsOK() {

            // get first Building
            Building firstBuilding = Globals.Game.currentWorld.buildingsProgressArray[0];

            Globals.UICanvas.uiElements.PopUpOfflineCoins.SetActive(false);
            Globals.UICanvas.uiElements.PopUpCoinsPerSecond.SetActive(true);
            Globals.UICanvas.uiElements.PopUps.GetComponent<CoinsPerSecond>().startUpdating();

            // First Building must be HybridBuilding
            Assert.IsTrue((firstBuilding as HybridBuilding != false), "First Building must be HybridBuilding");
            if (firstBuilding as HybridBuilding != false) {

                string originalTextCoinDisplay = Globals.UICanvas.uiElements.HUDCoinDisplay.text;

                // Building is an HybridBuilding
                hybridbuilding = (HybridBuilding)firstBuilding;

                Assert.IsTrue(hybridbuilding.baseIncome > zeroIdle, "First Buildings Income must be greater than one");

                IdleNum firstBuildingIncome = hybridbuilding.baseIncome;
                // Set Coins to 0
                Globals.Game.currentWorld.setCoins(new IdleNum(0));
                Assert.AreEqual(new IdleNum(0).toRoundedString(), Globals.Game.currentWorld.getCoins().toRoundedString());

                // Level Up Building
                firstBuilding.levelUp(1, true);
                Assert.AreEqual(firstBuilding.getLevel(), 1);
                Assert.AreEqual(Globals.Game.currentWorld.CoinIncomeManager.getIncomeBuildings().toRoundedString(), firstBuildingIncome.toRoundedString());
                Assert.AreEqual(Globals.Game.currentWorld.CoinIncomeManager.getTotalIncome().toRoundedString(), firstBuildingIncome.toRoundedString());

                // Wait X Seconds
                int waitforsec = 6;
                yield return new WaitForSeconds(waitforsec);

                // Test if World Coins are correct after the time
                Assert.AreEqual((firstBuildingIncome * waitforsec).toRoundedString() , Globals.Game.currentWorld.getCoins().toRoundedString());

                // Test if the UI Shows the change
                Assert.AreNotEqual(Globals.UICanvas.uiElements.HUDCoinDisplay.text, originalTextCoinDisplay, "CoinDisplay in HUD didn't show the coins");

                //* Boost
                int boostintensity = 2;
                Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.activateBoost(boostintensity, waitforsec, false);
                Assert.AreEqual(Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.getTimeLeftSec(), waitforsec);

                // Set Coins to 0
                Globals.Game.currentWorld.setCoins(new IdleNum(0));
                yield return new WaitForSeconds(waitforsec);

                // Test if World Coins are correct after the time
                Assert.AreEqual((firstBuildingIncome * waitforsec * boostintensity).toRoundedString(), Globals.Game.currentWorld.getCoins().toRoundedString());
                Assert.Less(Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.getTimeLeftSec(), waitforsec);


                //* ItemBoost
                Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer.activateBoost(boostintensity, waitforsec);

                // Set Coins to 0
                Globals.Game.currentWorld.setCoins(new IdleNum(0));
                yield return new WaitForSeconds(waitforsec);

                // Test if World Coins are correct after the time
                Assert.AreEqual((firstBuildingIncome * waitforsec * boostintensity).toRoundedString(), Globals.Game.currentWorld.getCoins().toRoundedString());



                //* both Boosts (must stack)
                Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.activateBoost(boostintensity, waitforsec, false);
                Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer.activateBoost(boostintensity, waitforsec);


                yield return null;

                // Safe PopUpTexts
                string boostTextOR = Globals.UICanvas.uiElements.PopUps.GetComponent<CoinsPerSecond>().BoostText.text;
                string itemBoostTextOR = Globals.UICanvas.uiElements.PopUps.GetComponent<CoinsPerSecond>().ItemBoostText.text;

                Globals.Game.currentWorld.setCoins(new IdleNum(0));
                // Set Coins to 0
                Globals.Game.currentWorld.setCoins(new IdleNum(0));

                yield return new WaitForSeconds(1);

                // Check if PopUp Boost Texts changed
                Assert.AreNotEqual(Globals.UICanvas.uiElements.PopUps.GetComponent<CoinsPerSecond>().BoostText.text, boostTextOR);
                Assert.AreNotEqual(Globals.UICanvas.uiElements.PopUps.GetComponent<CoinsPerSecond>().ItemBoostText.text, itemBoostTextOR);

                yield return new WaitForSeconds(waitforsec - 1);

                // Test if World Coins are correct after the time
                Assert.AreEqual((firstBuildingIncome * waitforsec * boostintensity * boostintensity).toRoundedString(), Globals.Game.currentWorld.getCoins().toRoundedString());
            }


            yield return null;
        }

        // Test for: Coin producing
        [UnityTest]
        public IEnumerator TestTapCoin() {

            // get first Building
            Building firstBuilding = Globals.Game.currentWorld.buildingsProgressArray[0];

            Globals.UICanvas.uiElements.PopUpOfflineCoins.SetActive(false);
            Globals.UICanvas.uiElements.PopUpCoinsPerSecond.SetActive(true);
            Globals.UICanvas.uiElements.PopUps.GetComponent<CoinsPerSecond>().startUpdating();

            // First Building must be HybridBuilding
            Assert.IsTrue((firstBuilding as HybridBuilding != false), "First Building must be HybridBuilding");
            if (firstBuilding as HybridBuilding != false) {

                HybridBuilding firstHybrid = (HybridBuilding)firstBuilding;

                // Twice Level Up
                firstHybrid.levelUp(1, true);
                firstHybrid.levelUp(1, true);

                yield return null;

                // Check if its Level 2
                Assert.AreEqual(2, firstHybrid.getLevel());

                IdleNum currentCoins = Globals.Game.currentWorld.getCoins();
                Debug.Log("-- Current Coins" + Globals.Game.currentWorld.getCoins().toRoundedString());

                firstHybrid.handleCoinTap();

                Assert.AreEqual(Globals.Game.currentWorld.getCoins(), currentCoins + firstHybrid.getCurrentIncome() * firstHybrid.coinRespawnTime);
                Debug.Log("-- Current Coins" + Globals.Game.currentWorld.getCoins().toRoundedString());

            }


            yield return null;
        }


        // Test for: Offline Coins
        [UnityTest]
        public IEnumerator TestsOfflineCoinsWorking() {

            Debug.Log("-- Beginning OfflineCoinCheck\n");

            Game.PopupMinOfflineTime = 1;
            System.DateTime l8r;

            IdleNum producingIdleNum = new IdleNum(10, "K");
            int secondsOffline = 5 * 60; // 5 Minutes

            // Add CoinIncome, so that the PopUp will show 
            Globals.Game.currentWorld.CoinIncomeManager.addAffector( new Affector<IdleNum>("income_producer", producingIdleNum));

            Debug.Log(Globals.Game.currentUser.stats.LastOfflineCoinsClaimed);

            yield return null;

            // Manipulate Time, so that Offline Coins working properly
            l8r = System.DateTime.Now;
            l8r = l8r.AddSeconds(+secondsOffline);

            // Start new Game with PlayerPrefs
            Game.initGameStartUp(false, l8r);
            Globals.UICanvas.uiElements.PopUpQuests.SetActive(false);

            yield return null;

            // Check if OfflineCoins are loaded
            Assert.IsTrue(Globals.UICanvas.uiElements.PopUpOfflineCoins.activeSelf, "PopUpOfflineCoins not shown");

            yield return null;

            // Test Offline Coins
            IdleNum coinsOnStart = Globals.Game.currentWorld.getCoins();
            Globals.UICanvas.uiElements.PopUpOfflineCoinsButtonOK.GetComponent<Button>().onClick.Invoke();

            yield return null;

            Debug.Log("TestSuite: Coins before ButtonClick: " + coinsOnStart.toRoundedString());
            Debug.Log("TestSuite: Coins after ButtonClick: " + Globals.Game.currentWorld.getCoins().toRoundedString());

            Assert.IsTrue(coinsOnStart <= Globals.Game.currentWorld.getCoins(), "Coins did not raise after OfflineCoin-ButtonPress");
            Assert.AreEqual( (producingIdleNum * secondsOffline).toRoundedString(1), Globals.Game.currentWorld.getCoins().toRoundedString(1), 
                "Coins did not a specific precise Amount: actual: " + Globals.Game.currentWorld.getCoins().toRoundedString(1) + "; expected " + (producingIdleNum * secondsOffline).toRoundedString(1)); ;


            yield return null;

        }

        // Test for: Offline Coins
        [UnityTest]
        public IEnumerator TestsOfflineCoinsDoubleItButton() {

            Debug.Log("-- Beginning OfflineCoinCheck\n");

            Game.PopupMinOfflineTime = 1;
            System.DateTime l8r;

            IdleNum producingIdleNum = new IdleNum(10, "K");
            int secondsOffline = 5 * 60; // 5 Minutes

            // Add CoinIncome, so that the PopUp will show 
            Globals.Game.currentWorld.CoinIncomeManager.addAffector(new Affector<IdleNum>("income_producer", producingIdleNum));

            yield return null;

            // Manipulate Time, so that Offline Coins working properly
            l8r = System.DateTime.Now;
            l8r = l8r.AddSeconds(+secondsOffline);

            // Start new Game with PlayerPrefs
            Game.initGameStartUp(false, l8r);
            Globals.UICanvas.uiElements.PopUpQuests.SetActive(false);

            yield return null;

            // Check if OfflineCoins are loaded
            Assert.IsTrue(Globals.UICanvas.uiElements.PopUpOfflineCoins.activeSelf, "PopUpOfflineCoins not shown");

            yield return null;

            // Test Offline Coins x2 Button
            IdleNum coinsOnStart = Globals.Game.currentWorld.getCoins();
            Globals.UICanvas.uiElements.PopUpOfflineCoinsAdButtonX2.GetComponent<Button>().onClick.Invoke();

            yield return null;

            Debug.Log("TestSuite: Coins before ButtonClick: " + coinsOnStart.toRoundedString(1));
            Debug.Log("TestSuite: Coins after ButtonClick: " + Globals.Game.currentWorld.getCoins().toRoundedString(1));

            Assert.IsTrue(coinsOnStart <= Globals.Game.currentWorld.getCoins(), "Coins did not raise after OfflineCoin-ButtonPress");
            Assert.AreEqual((producingIdleNum * secondsOffline * 2).toRoundedString(1), Globals.Game.currentWorld.getCoins().toRoundedString(1),
                "Coins Doubled did not a specific precise Amount: actual: " + Globals.Game.currentWorld.getCoins().toRoundedString(1) + "; expected " + (producingIdleNum * secondsOffline * 2).toRoundedString(1)); ;


            yield return null;

        }


        // Test for: Offline Coins with Boost
        [UnityTest]
        public IEnumerator TestsOfflineCoinsWithBoostWorking() {

            Debug.Log("-- Beginning OfflineCoinCheck with Boost\n");

            Game.PopupMinOfflineTime = 1;
            System.DateTime l8r;

            // Options
            IdleNum producingIdleNum = new IdleNum(10, "K");
            int secondsOffline = 5 * 60; // 5 Minutes
            float boostIntensity = 2;

            // Add CoinIncome, so that the PopUp will show 
            Globals.Game.currentWorld.CoinIncomeManager.addAffector(new Affector<IdleNum>("income_producer", producingIdleNum));

            Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.activateBoost(boostIntensity, secondsOffline * 2, false);

            yield return null;

            // Manipulate Time, so that Offline Coins working properly
            l8r = System.DateTime.Now;
            l8r = l8r.AddSeconds(+secondsOffline);

            // Start new Game with PlayerPrefs
            Game.initGameStartUp(false, l8r);
            Globals.UICanvas.uiElements.PopUpQuests.SetActive(false);

            yield return null;

            // Check if OfflineCoins are loaded
            Assert.IsTrue(Globals.UICanvas.uiElements.PopUpOfflineCoins.activeSelf, "PopUpOfflineCoins not shown");

            yield return null;

            // Test Offline Coins
            IdleNum coinsOnStart = Globals.Game.currentWorld.getCoins();
            Globals.UICanvas.uiElements.PopUpOfflineCoinsButtonOK.GetComponent<Button>().onClick.Invoke();

            yield return null;

            Debug.Log("TestSuite: Coins before ButtonClick: " + coinsOnStart.toRoundedString(1));
            Debug.Log("TestSuite: Coins after ButtonClick: " + Globals.Game.currentWorld.getCoins().toRoundedString(1));

            Assert.IsTrue(coinsOnStart <= Globals.Game.currentWorld.getCoins(), "Coins did not raise after OfflineCoin-ButtonPress");
            Assert.AreEqual((producingIdleNum * secondsOffline * boostIntensity).toRoundedString(1), Globals.Game.currentWorld.getCoins().toRoundedString(1), 
                "Coins did not a specific precise Amount: actual- " + Globals.Game.currentWorld.getCoins().toRoundedString(1) + "; expected " + (producingIdleNum * secondsOffline * boostIntensity).toRoundedString(1)); ;


            yield return null;

        }

        // Test for: Offline Coins
        [UnityTest]
        public IEnumerator TestsOfflineCoinsLimit() {

            Debug.Log("-- Beginning OfflineCoinCheck\n");

            Game.PopupMinOfflineTime = 1;
            Game.limitOfflineCoinsHours = 2; // Manipulating the Limit of the OfflineCoins
            int limitOfflineSeconds = Game.limitOfflineCoinsHours * 60 * 60;
            System.DateTime l8r;

            IdleNum producingIdleNum = new IdleNum(10, "K");
            int secondsOffline = (Game.limitOfflineCoinsHours+55) * 60 * 60; // many Hours (more than limitOfflineCoinsHours) 

            // Add CoinIncome, so that the PopUp will show 
            Globals.Game.currentWorld.CoinIncomeManager.addAffector(new Affector<IdleNum>("income_producer", producingIdleNum));

            yield return null;

            // Manipulate Time, so that Offline Coins working properly
            l8r = System.DateTime.Now;
            l8r = l8r.AddSeconds(+secondsOffline);

            // Start new Game with PlayerPrefs
            Game.initGameStartUp(false, l8r);
            Globals.UICanvas.uiElements.PopUpQuests.SetActive(false);

            yield return null;

            // Check if OfflineCoins are loaded
            Assert.IsTrue(Globals.UICanvas.uiElements.PopUpOfflineCoins.activeSelf, "PopUpOfflineCoins not shown");

            yield return null;

            // Test Offline Coins
            IdleNum coinsOnStart = Globals.Game.currentWorld.getCoins();
            Globals.UICanvas.uiElements.PopUpOfflineCoinsButtonOK.GetComponent<Button>().onClick.Invoke();

            yield return null;

            Debug.Log("TestSuite: Coins before ButtonClick: " + coinsOnStart.toRoundedString());
            Debug.Log("TestSuite: Coins after ButtonClick: " + Globals.Game.currentWorld.getCoins().toRoundedString());

            Assert.IsTrue(coinsOnStart <= Globals.Game.currentWorld.getCoins(), "Coins did not raise after OfflineCoin-ButtonPress");
            Assert.AreEqual((producingIdleNum * limitOfflineSeconds).toRoundedString(1), Globals.Game.currentWorld.getCoins().toRoundedString(1),
                "Coins did not a specific precise limited Amount: actual: " + Globals.Game.currentWorld.getCoins().toRoundedString(1) + "; expected " + (producingIdleNum * limitOfflineSeconds).toRoundedString(1)); ;


            yield return null;

        }



    }
}
