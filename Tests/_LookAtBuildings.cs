using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class _LookAtBuildings {

        public InitGame Game;

        EnviBuilding envibuilding;
        HybridBuilding hybridbuilding;

        int lastBuildingDelay = 0;
        IdleNum zeroIdle = new IdleNum(0);



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
            Globals.Game.currentUser.wasSignedIn = false;

            // Wait for one Frame until Component is loaded
            yield return null;

            // Delete all PlayerPrefs and start a new Game
            SavingSystem.saveOrLoadPlayfab = false;
            Game.resetGameForAdmins();

            Globals.UICanvas.uiElements.PopUpQuests.SetActive(false);
            Globals.UICanvas.uiElements.SaveGamePopUp.SetActive(false);


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
            Globals.KaloaSettings.skipTutorial = false;
            Globals.KaloaSettings.preventSaving = false;

            yield return null;
        }




        // Test for: BuildingProgress
        [UnityTest]
        public IEnumerator TestLevelUpBuildingProgress() {

            // Inits
            Building NextBuilding = null, LastBuilding = null;
            int timeToUnlock = 2;

            // Eliminate OfflineCoinPopUp
            Globals.UICanvas.uiElements.PopUpBG.SetActive(false);
            Globals.UICanvas.uiElements.PopUpOfflineCoins.SetActive(false);

            // Turn UnlockProcess to X Sec
            Game.adminUnlockProcessSec = timeToUnlock;
            Globals.KaloaSettings.adminUnlockProcessSec = timeToUnlock;

            int i = 0;
            Globals.Game.currentWorld.setCoins(new IdleNum(999, "CC"));

            Globals.UICanvas.uiElements.MainCamera.transform.position = new Vector3(237f, 50f, 65f);

            foreach (Building currentBuilding in Globals.Game.currentWorld.buildingsProgressArray) {

                string buildingName = currentBuilding.getName();
                currentBuilding.level0To1DelaySeconds = timeToUnlock;


                if (i == 0) {
                    // First Building in Array (no Last Building)
                    LastBuilding = null;
                    NextBuilding = Globals.Game.currentWorld.buildingsProgressArray[i + 1];
                    Assert.IsNull(currentBuilding.getLastBuildingInBuildingProgress());
                } else if (i == Globals.Game.currentWorld.buildingsProgressArray.Length - 1) {
                    // Last Building in Array (has no Next Building)
                    LastBuilding = Globals.Game.currentWorld.buildingsProgressArray[i - 1];
                    NextBuilding = null;
                } else {
                    // Any Other Building
                    LastBuilding = Globals.Game.currentWorld.buildingsProgressArray[i - 1];
                    NextBuilding = Globals.Game.currentWorld.buildingsProgressArray[i + 1];
                }

                // Check baseCost of NextBuilding
                if (NextBuilding != null) {
                    Assert.IsTrue(currentBuilding.baseCost <= NextBuilding.baseCost,
                        buildingName + ": Current Buildings baseCost must be lower or equal than from the next");
                }

                // - Check status on LvL 0
                if (LastBuilding != null) {
                    Assert.IsTrue(LastBuilding.getCurrentConstructionPhase() == Building.ConstructionPhases.Finished,
                        buildingName + ": Last Building must currently be in ConstructionPhase Finished, but was " + LastBuilding.getCurrentConstructionPhase());
                    Assert.AreEqual(LastBuilding, currentBuilding.getLastBuildingInBuildingProgress());
                }
                Assert.IsTrue(currentBuilding.getCurrentConstructionPhase() == Building.ConstructionPhases.ConstructionSite);
                if (NextBuilding != null) {
                    Assert.AreEqual(true, currentBuilding.getIsNextInBuildingProgress());
                    Assert.IsFalse(NextBuilding.gameObject.activeSelf);
                    Assert.IsFalse(NextBuilding.isLevelUppable());
                }

                yield return null;

                // Level Up Once
                currentBuilding.levelUp(1);

                yield return new WaitForSeconds(timeToUnlock + 1);

                // - Check status on  lvl 1
                Assert.IsTrue(currentBuilding.getCurrentConstructionPhase() == Building.ConstructionPhases.Finished);
                if (NextBuilding != null) {
                    Assert.IsTrue(NextBuilding.gameObject.activeSelf);
                    Assert.IsTrue(NextBuilding.getCurrentConstructionPhase() == Building.ConstructionPhases.ConstructionSite, 
                        buildingName + ": Next Building must currently be in ConstructionPhase ConstructionSite, but was " + NextBuilding.getCurrentConstructionPhase());
                    Assert.IsFalse(NextBuilding.isLevelUppable());
                }

                yield return null;

                // Level Up Building until one level to unlock next Building
                currentBuilding.levelUp(currentBuilding.getLevelsToUnlockNextBuilding() - 2);

                yield return null;

                // - Check Status
                if (LastBuilding != null) {
                    Assert.IsTrue(currentBuilding.getCurrentConstructionPhase() == Building.ConstructionPhases.Finished);
                }
                Assert.IsTrue(currentBuilding.getCurrentConstructionPhase() == Building.ConstructionPhases.Finished);
                if (NextBuilding != null) {
                    Assert.IsTrue(NextBuilding.gameObject.activeSelf);
                    Assert.IsTrue(NextBuilding.getCurrentConstructionPhase() == Building.ConstructionPhases.ConstructionSite
                        , buildingName + ": Next Building must currently be in ConstructionModus ConstructionSite, but was " + NextBuilding.getCurrentConstructionPhase());
                    Assert.IsFalse(NextBuilding.isLevelUppable());
                    Assert.IsFalse(NextBuilding.isLevelUppable(), buildingName + "(" + currentBuilding.getLevel() + "): Next Building '" + NextBuilding.getName() + "(" + NextBuilding.getLevel() + ")' should NOT be levelUppable when currentBuilding not reached its Progresslevel");
                }

                yield return null;

                currentBuilding.levelUp(1);

                yield return new WaitForSeconds(timeToUnlock - 1 );

                if (NextBuilding != null) {
                    Assert.IsTrue(NextBuilding.isLevelUppable(), buildingName + "("+currentBuilding.getLevel()+"): Next Building '"+ NextBuilding.getName() + "(" + NextBuilding.getLevel() + ")' should be now levelUppable");
                }

                i++;

                yield return null;
            }

            yield return null;

        }


    }
}
