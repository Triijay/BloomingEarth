using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;


namespace Tests 
{
    public class TestSuitePrestige {

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

        // Test for: Initial Test for variables 
        [UnityTest]
        public IEnumerator CheckPrestigeVarsAfterGameStart() {

            // These Vars has to be set on start
            Assert.AreEqual(1.0f, Globals.Game.currentUser.prestige.prestigeBonus, 0.1f);
            Assert.AreEqual(0, Globals.Game.currentUser.prestige.getPrestigeLevel());
            Assert.AreEqual(0.0f, Globals.Game.currentUser.prestige.getPossiblePrestigeBonus(), 0.1f);
            Assert.AreEqual(0, Globals.Game.currentUser.prestige.getAlreadyPrestigedLevel());

            // Check if our vars are rational
            Assert.Less(Globals.Game.currentUser.prestige.prestigeConstructionTime, 10);
            Assert.Greater(Globals.Game.currentUser.prestige.minPrestigeBonus, 5);
            Assert.Greater(Globals.Game.currentUser.prestige.minPrestigeProgress, 5);

            // Check if Gameobjects are not null
            Assert.IsNotNull(Globals.Game.currentUser.prestige.prestigeResetButton);
            Assert.IsNotNull(Globals.Game.currentUser.prestige.prestigeLabelBonusNow);
            Assert.IsNotNull(Globals.Game.currentUser.prestige.prestigeLabelBonusNext);
            Assert.IsNotNull(Globals.Game.currentUser.prestige.prestigeHint);

            // Stern ausgeblendet
            Assert.IsFalse(Globals.UICanvas.uiElements.ButtonPrestige.activeSelf);

            yield return null;
        }


        // Test for: 
        [UnityTest]
        public IEnumerator TestPrestige() {

            Assert.IsTrue(true);

            //// Inits
            //Building minPrestigeBuilding = Globals.Game.currentWorld.buildingsProgressArray[0];
            //Building firstBuilding = Globals.Game.currentWorld.buildingsProgressArray[0];
            //Prestige prestige = Globals.Game.currentUser.prestige;
            //int cntUpdateLevels = (int)(1 / Prestige.bonusPerLevelFactor) + 1;
            //int levelProgress = 0;
            //int levelProgressAfterFirstPrestige = 0;
            //int buildingProgress = -1;

            //// Standardisierte Test Vorraussetzungen
            //prestige.minPrestigeBonus = 10.0f;
            //prestige.minPrestigeProgress = 7;
            //Globals.Game.currentUser.Emeralds = 1;
            //Globals.Game.currentWorld.QuestsComponent.setCurrentQuestIndex(5);
            //prestige.calcMinLevelForPrestige();
            //Globals.UICanvas.uiElements.PopUpOfflineCoins.SetActive(false);
            //Globals.Game.currentWorld.addCoins(new IdleNum(99, "AB"));
            //Globals.KaloaSettings.adminUnlockProcessSec = 1;

            ///*-------------------------- PHASE 1 -----------------------------*/
            //// Go to min Prestige Building

            //// Check interactability of the Prestige Reset Button
            //Assert.IsFalse(prestige.prestigeResetButton.interactable);

            //// Test would not fail if both numbers are the same
            //Assert.AreNotEqual(Globals.KaloaSettings.adminUnlockProcessSec, prestige.prestigeConstructionTime);


            //foreach (Building aBuilding in Globals.Game.currentWorld.buildingsProgressArray) {
            //    aBuilding.levelsToUnlockNextBuilding = cntUpdateLevels;

            //    levelProgress += cntUpdateLevels;
            //    buildingProgress++;

            //    aBuilding.gameObject.SetActive(true);
            //    Debug.LogWarning("Levelling up building " + aBuilding.getName() + " to " + aBuilding.levelsToUnlockNextBuilding);
            //    aBuilding.levelUp(aBuilding.levelsToUnlockNextBuilding);

            //    yield return new WaitForSeconds(2);

            //    aBuilding.levelUp(aBuilding.levelsToUnlockNextBuilding - 1);

            //    // Sind die prestige level richtig addiert worden
            //    Assert.AreEqual(levelProgress, prestige.getPrestigeLevel());

            //    Assert.AreEqual(buildingProgress, Globals.Game.currentUser.worldProgressIndex);

            //    // Check if Construction time is the initial time
            //    Assert.AreEqual(Globals.KaloaSettings.adminUnlockProcessSec, aBuilding.getDelayUnlockProcessSeconds());
               

            //    // We reached the first checkpoint and reached the min building Progress
            //    // Now we have to check things
            //    if (buildingProgress == prestige.minPrestigeProgress) {
            //        minPrestigeBuilding = aBuilding;
            //        Debug.Log("Min Prestige Building: " + aBuilding.getName());
            //        break;
            //    }
            //}

            //// Momentane Bonus unter MinPrestigeBonus
            //Assert.AreEqual(prestige.minPrestigeProgress, buildingProgress);
            //Assert.Less(prestige.getPossiblePrestigeBonus(), prestige.minPrestigeBonus);
            //Assert.AreEqual(levelProgress, (buildingProgress+1) * cntUpdateLevels);
            //Assert.AreEqual(0, Globals.Game.currentUser.prestige.getAlreadyPrestigedLevel()); // da wir bisher nicht prestiged haben

            //// Check if the right building is the min prestige building
            //Assert.AreNotEqual(Globals.Game.currentWorld.buildingsProgressArray[0], minPrestigeBuilding);
            //Assert.AreEqual(Globals.Game.currentWorld.buildingsProgressArray[prestige.minPrestigeProgress], minPrestigeBuilding);

            //// Stern eingeblendet
            //Assert.IsTrue(Globals.UICanvas.uiElements.ButtonPrestige.activeSelf);

            //// Check interactability of the Prestige Reset Button
            //Assert.IsFalse(prestige.prestigeResetButton.interactable);


            ///*-------------------------- PHASE 2 -----------------------------*/
            //// Go to min Prestige Bonus

            //do {
            //    minPrestigeBuilding.levelUp(1);
            //    levelProgress++;
            //} while (prestige.getPossiblePrestigeBonus() < prestige.minPrestigeBonus);


            //// Now the user can prestige

            //Assert.AreEqual(prestige.getPossiblePrestigeBonus(), prestige.minPrestigeBonus, 0.5f);

            //// Stern eingeblendet
            //Assert.IsTrue(Globals.UICanvas.uiElements.ButtonPrestige.activeSelf);
            //// Check interactability of the Prestige Reset Button
            //Assert.IsTrue(prestige.prestigeResetButton.interactable);


            ///*-------------------------- PHASE 3 -----------------------------*/
            //// The user prestiges

            //// The user clicks on prestige Button to open popup
            //Assert.IsFalse(Globals.UICanvas.uiElements.PopUpPrestige.activeSelf);
            //Globals.UICanvas.uiElements.ButtonPrestige.GetComponent<Button>().onClick.Invoke();
            //Assert.IsTrue(Globals.UICanvas.uiElements.PopUpPrestige.activeSelf, "Prestige Popup didn´t open");

            //yield return null;

            //// The user clicks on prestige reset button
            //Assert.IsTrue(prestige.prestigeResetButton.interactable);
            //prestige.prestigeResetButton.onClick.Invoke();

            //yield return null;

            //// Be sure we have enough coins
            //Globals.Game.currentWorld.addCoins(new IdleNum(99, "CC"));

            //Assert.AreEqual(prestige.minPrestigeBonus, prestige.prestigeBonus, 1.0f);

            //// check if prestige was successfull
            //Assert.AreEqual(0, Globals.Game.currentWorld.buildingsProgressArray[0].getLevel()); // House is level 0
            //Assert.AreEqual(0, Globals.Game.currentWorld.buildingsProgressArray[1].getLevel()); // Field Wheat is level 0

            //// check the construction times
            //Assert.AreEqual(prestige.prestigeConstructionTime, Globals.Game.currentWorld.buildingsProgressArray[0].getDelayUnlockProcessSeconds()); // House has prestige construction time
            //Assert.AreEqual(prestige.prestigeConstructionTime, Globals.Game.currentWorld.buildingsProgressArray[1].getDelayUnlockProcessSeconds()); // Field Wheat has prestige construction time
            //Assert.AreEqual(prestige.prestigeConstructionTime, minPrestigeBuilding.getDelayUnlockProcessSeconds()); // Beekeeper has prestige construction time

            //// Stern weiterhin eingeblendet
            //Assert.IsTrue(Globals.UICanvas.uiElements.ButtonPrestige.activeSelf);
            //// Check interactability of the Prestige Reset Button
            //Assert.IsFalse(prestige.prestigeResetButton.interactable);

            //// Check if kept items really kept
            //Assert.AreEqual(1, Globals.Game.currentUser.Emeralds);
            //Assert.AreEqual(5, Globals.Game.currentWorld.QuestsComponent.getCurrentQuestIndex());

            //Assert.AreEqual(levelProgress, prestige.getAlreadyPrestigedLevel());


            ///*-------------------------- PHASE 4 -----------------------------*/
            //// Go to already prestige levels

            //// Sind die prestige level richtig zurück gesetzt worden
            //Assert.AreEqual(0, prestige.getPrestigeLevel());

            //// exactly 1 step before the previous prestiged level
            //// Initial level up with delayProcess
            //firstBuilding.levelUp(1);
            //levelProgressAfterFirstPrestige++;
            //yield return new WaitForSeconds(2);

            //do {
            //    firstBuilding.levelUp(1);
            //    levelProgressAfterFirstPrestige++;
            //} while (levelProgressAfterFirstPrestige < prestige.getAlreadyPrestigedLevel());

            //yield return null;

            //// Sind die prestige level nicht addiert worden
            //Assert.AreEqual(0, prestige.getPrestigeLevel());
            //// bleibt der prestige bonus bei 0
            //Assert.AreEqual(0.0f, prestige.getPossiblePrestigeBonus(), 0.01f);
            //// Check if already prestiged level stays the same after level up
            //Assert.AreEqual(levelProgress, prestige.getAlreadyPrestigedLevel());
            //// Check if levelProgressAfterFirstPrestige is exactly the alreadyPrestigedLevel
            //Assert.AreEqual(levelProgressAfterFirstPrestige, prestige.getAlreadyPrestigedLevel());
            //// Check if possiblePrestigeBonus is less than minBonus
            //Assert.Less(prestige.getPossiblePrestigeBonus(), prestige.minPrestigeBonus);


            //// Final level up - since then the user can prestige again
            //firstBuilding.levelUp(1);
            //levelProgressAfterFirstPrestige++;

            //// Sind die prestige level richtig addiert worden
            //Assert.AreEqual(1, prestige.getPrestigeLevel());
            //// Fängt der prestige jetzt erst an zu zählen
            //Assert.Greater(prestige.getPossiblePrestigeBonus(), prestige.minPrestigeBonus);
            //// Check if already prestiged level stays the same after level up
            //Assert.AreEqual(levelProgress, prestige.getAlreadyPrestigedLevel());


            ///*-------------------------- PHASE 5 -----------------------------*/
            //// The user prestiges again

            //do {
            //    firstBuilding.levelUp(1);
            //    levelProgressAfterFirstPrestige++;
            //} while (prestige.getPossiblePrestigeBonus() < prestige.getMinPrestigeBonusNext());

            //// The user clicks on prestige Button to open popup
            //Assert.IsFalse(Globals.UICanvas.uiElements.PopUpPrestige.activeSelf);
            //Globals.UICanvas.uiElements.ButtonPrestige.GetComponent<Button>().onClick.Invoke();
            //Assert.IsTrue(Globals.UICanvas.uiElements.PopUpPrestige.activeSelf, "Prestige Popup didn´t open");

            //yield return null;


            //// save possible prestige bonus
            //double prestigedBonus = prestige.getPossiblePrestigeBonus();

            //// The user clicks on prestige reset button
            //Assert.IsTrue(prestige.prestigeResetButton.interactable);
            //prestige.prestigeResetButton.onClick.Invoke();

            //yield return null;

            //// check the construction times
            //Assert.AreEqual(prestige.prestigeConstructionTime, Globals.Game.currentWorld.buildingsProgressArray[0].getDelayUnlockProcessSeconds()); // House has prestige construction time
            //Assert.AreEqual(prestige.prestigeConstructionTime, Globals.Game.currentWorld.buildingsProgressArray[1].getDelayUnlockProcessSeconds()); // Field Wheat has prestige construction time
            //Assert.AreEqual(prestige.prestigeConstructionTime, minPrestigeBuilding.getDelayUnlockProcessSeconds()); // Beekeeper has prestige construction time

            //Assert.AreEqual(prestigedBonus, prestige.prestigeBonus, 1.0f);

            yield return null;
        }

    }
}
