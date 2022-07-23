using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class TestSuiteBuildings {

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


        // Test for: BuildingsArrays bigger than Size 6
        [UnityTest]
        public IEnumerator CheckBuildingArraysBiggerThanX() {
            Assert.Greater(Globals.Game.currentWorld.buildingsProgressArray.Length, 6, "currentWorld Buildings are only " + Globals.Game.currentWorld.buildingsProgressArray.Length);

            //ToDo Test other Worlds

            yield return null;
        }



        // Test for: Buildings are functional
        [UnityTest]
        public IEnumerator CheckBuildingsAreFunctional() {

            // That would be a lower € Outcome for us
            int moreThanOneBuildingHas5SecDelayUnlockTime = 0;

            foreach (Building building in Globals.Game.currentWorld.buildingsProgressArray) {

                string buildingName = building.getName();
                string bID = building.buildingNameIdentifier();

                Assert.IsNotEmpty(buildingName);
                Assert.IsTrue(bID.Contains("World"), "WorldName of Building not OK - Building must be Child of a Folder WorldNameObject->Buildings! - " + buildingName);

                Assert.AreEqual(0, building.getLevel()); // Without PlayerPrefs level must be 0

                // Building would be able to LevelUp for no Coins
                Assert.IsTrue(building.baseCost > zeroIdle, buildingName + ": Buildings Base Cost are wrong");

                // Building UpgradeCost would be the same or cheaper every Level
                Assert.IsTrue(building.getUpgradeCost(1) > zeroIdle);
                Assert.IsTrue(building.getUpgradeCost(2) > building.getUpgradeCost(1));
                Assert.Greater(building.costFactor, 1, buildingName + ": Buildings Cost Factor must be greater than one");

                if (building.level0To1DelaySeconds <= 5) {
                    moreThanOneBuildingHas5SecDelayUnlockTime++;
                    Debug.Log("--- " + buildingName + " Seconds in DelayUnlockProcess: " + building.level0To1DelaySeconds);
                }

                // Check if building.level0To1DelaySeconds are raising every Building in BuildingProgress
                Assert.GreaterOrEqual(building.level0To1DelaySeconds, lastBuildingDelay, buildingName + ": Building Delay has to be more or equal than the last Building");
                lastBuildingDelay = building.level0To1DelaySeconds;

                // DelayUnlock-Process
                Transform DelayUnlockProcess = building.transform.Find("DelayUnlock-Process");
                Assert.IsNotNull(DelayUnlockProcess, buildingName + ": DelayUnlock-Process not in Building");

                // EnviBuilding
                if (building as EnviBuilding != false) {
                    // Building is an EnviBuilding
                    envibuilding = (EnviBuilding)building;
                    Assert.IsNotNull(envibuilding.getCurrentEnvironmentFactor());

                    if (envibuilding as HybridBuilding == false) {
                        // Building is an EnviBuilding and not a HybridBuilding
                        Debug.Log("Building " + buildingName + " is an EnviBuilding");
                        Assert.Less(0, envibuilding.getCurrentEnvironmentFactor(), "Building " + buildingName + " negative EnviBuilding makes no sense, fix that!");
                    }

                    if (envibuilding as HybridBuilding != false) {
                        // Building is an HybridBuilding
                        hybridbuilding = (HybridBuilding)envibuilding;
                        Debug.Log("Building " + buildingName + " is an HybridBuilding");

                        Assert.IsTrue(hybridbuilding.baseIncome > zeroIdle, buildingName + ": Buildings Income must be greater than one");
                    }
                }

            }

            // Only the First Building should have a Mini-UnlockTime
            Assert.LessOrEqual(moreThanOneBuildingHas5SecDelayUnlockTime, 1, "More than one Building has 5orLess Seconds Unlock Time - See Debug.Log of this test");

            yield return null;

        }




        // Test for: Buildings has all it UI Elements Ingame
        [UnityTest]
        public IEnumerator CheckBuildingsHasIntegratedUIElements() {

            foreach (Building building in Globals.Game.currentWorld.buildingsProgressArray) {

                string buildingName = building.getName();

                // Sign
                Transform Sign = building.transform.Find("Sign");
                Assert.IsNotNull(Sign, buildingName + ": Sign not in Building");

                // LevelUpIndicator
                Transform LevelUpIndicator = building.transform.Find("LevelUpIndicator");
                Assert.IsNotNull(LevelUpIndicator, buildingName + ": LevelUpIndicator not in Building");
                // LevelUpIndicator Material
                MeshRenderer LevelUpIndicatorMeshRenderer = LevelUpIndicator.GetComponent<MeshRenderer>();
                Assert.IsTrue(LevelUpIndicatorMeshRenderer.enabled);
                Assert.IsNotNull(LevelUpIndicatorMeshRenderer, buildingName + ": LevelUpIndicator Material is not setted up correctly");

                // DelayUnlock-Process
                Assert.IsNotNull(building.getDelayUnlockProcessIndicator());
                Assert.IsNotNull(building.getPercentBar());
                Assert.IsNotNull(building.getTimeLeftLabel());
                Assert.AreEqual(0, building.getPercentBar().transform.localScale.z, "PercentBarPivot from DelayUnlock-Process GameObject is not on z-scale 0: " + buildingName);

                // Spinning Coin if Hybridbuilding
                if (building as HybridBuilding != false) {
                    // Building is an HybridBuilding
                    Transform SpinningCoin = building.transform.Find("TapCoin");
                    Assert.IsNotNull(SpinningCoin, buildingName + ": TapCoin not in Hybridbuilding");
                }
                
            }

            yield return null;
        }


        // Test for: Every Building has its translated Keywords, like Name or Description
        [UnityTest]
        public IEnumerator CheckBuildingsHasLanguageKeys() {

            Hashtable languageKeys = Globals.Controller.Language.getXmlFileStrings();

            foreach (Building building in Globals.Game.currentWorld.buildingsProgressArray) {

                string buildingName = building.getName();

                Assert.IsTrue(languageKeys.Contains("building_name_" + buildingName));
                Assert.IsTrue(languageKeys.Contains("buildingmenu_building_description_" + buildingName));
                
            }

            yield return null;
        }

        // Test for: Every Building has its Achievements and Events from Google
        [UnityTest]
        public IEnumerator CheckBuildingsHasAchievements() {

            foreach (Building building in Globals.Game.currentWorld.buildingsProgressArray) {

                string buildingName = building.getName();

                Assert.IsTrue(Globals.Game.currentWorld.achievementsAndEventsLevel1.ContainsKey(buildingName));

            }

            yield return null;
        }


        // Test for: ConstructionModus
        [UnityTest]
        public IEnumerator CheckBuildingsConstructionModus() {

            foreach (Building building in Globals.Game.currentWorld.buildingsProgressArray) {

                string buildingName = building.getName();

                // LevelUpExtensions built in 3D Model
                Transform ConstructionLevelObject = building.transform.Find("ConstructionLevels");
                Assert.IsNotNull(ConstructionLevelObject, buildingName + ": ConstructionLevels not in Building (should be built in Cinema4D)");

                Transform[] ConstructionLevelsTransforms = ConstructionLevelObject.GetComponentsInChildren<Transform>(true);
                foreach (Transform ConstructionLevelEngineTransform in ConstructionLevelsTransforms) {
                    if (ConstructionLevelEngineTransform.parent == ConstructionLevelObject) {
                        Assert.IsTrue(System.Enum.TryParse<Building.ConstructionPhases>(ConstructionLevelEngineTransform.name, true, out Building.ConstructionPhases tempPhase), 
                            buildingName + ": The potential ConstructionLevel-Object '" + ConstructionLevelEngineTransform.name + "' is not named like a Building.ConstructionLevel");
                    }
                }

                // ConstructionLevels built in Engine
                Transform ConstructionLevelEngineObject = building.transform.Find("ConstructionLevelsEngine");
                Assert.IsNotNull(ConstructionLevelEngineObject, buildingName + ": ConstructionLevelsEngine not in Building (should be built in Engine)");
                // Check the Elements right under the ConstructionLevels GameObject
                Transform[] ConstructionLevelsEngineTransforms = ConstructionLevelEngineObject.GetComponentsInChildren<Transform>(true);
                foreach (Transform ConstructionLevelTransform in ConstructionLevelsEngineTransforms) {
                    if (ConstructionLevelTransform.parent == ConstructionLevelEngineObject) {
                        Assert.IsTrue(System.Enum.TryParse<Building.ConstructionPhases>(ConstructionLevelTransform.name, true, out Building.ConstructionPhases tempPhase),
                            buildingName + ": The potential ConstructionLevel-Object '" + ConstructionLevelTransform.name + "' is not named like a Building.ConstructionLevel");
                    }
                }

                // TODO Test LevelUp -> Meshes fading in

                // TODO Test InitLevelUpExtensions when Building is already on desired Level

            }

            yield return null;
        }



        // Test for: LevelUpExtensions
        [UnityTest]
        public IEnumerator CheckBuildingsLevelUpExtensions() {

            foreach (Building building in Globals.Game.currentWorld.buildingsProgressArray) {

                string buildingName = building.getName();
                int levelUntilUnlockExtension;

                // LevelUpExtensions built in 3D Model
                Transform LevelUpExtensionObject = building.transform.Find("LevelUpExtensions");
                Assert.IsNotNull(LevelUpExtensionObject, buildingName + ": LevelUpExtensions not in Building (should be built in Cinema4D)");

                Transform[] LevelUpExtensionsTransforms = LevelUpExtensionObject.GetComponentsInChildren<Transform>(true);
                foreach (Transform LevelUpExtensionEngineTransform in LevelUpExtensionsTransforms) {
                    if (LevelUpExtensionEngineTransform.parent == LevelUpExtensionObject) {
                        try {
                            levelUntilUnlockExtension = int.Parse(LevelUpExtensionEngineTransform.name);
                        }
                        catch {
                            Assert.IsTrue(false, buildingName + ": The potential LevelExtension-Object '" + LevelUpExtensionEngineTransform.name + "' is not named like an int - eg '30', '55' ...");
                        }
                    }
                }


                // LevelUpExtensions built in Engine
                Transform LevelUpExtensionEngineObject = building.transform.Find("LevelUpExtensionsEngine");
                Assert.IsNotNull(LevelUpExtensionEngineObject, buildingName + ": LevelUpExtensionsEngine not in Building (should be built in Engine)");
                // Check the Elements right under the LevelUpExtensions GameObject
                Transform[] LevelUpExtensionsEngineTransforms = LevelUpExtensionEngineObject.GetComponentsInChildren<Transform>(true);
                foreach (Transform LevelUpExtensionTransform in LevelUpExtensionsEngineTransforms) {
                    if (LevelUpExtensionTransform.parent == LevelUpExtensionEngineObject) {
                        try {
                            levelUntilUnlockExtension = int.Parse(LevelUpExtensionTransform.name);
                        } catch {
                            Assert.IsTrue(false, buildingName + ": The potential LevelExtension-Object '" + LevelUpExtensionTransform.name + "' is not named like an int - eg '30', '55' ...");
                        }
                    }
                }

                if (building.getLevelUpExtensions().Count > 0) {
                    foreach (KeyValuePair<int, List<Transform>> LevelUpExtensionList in building.getLevelUpExtensions()) {
                        foreach (Transform LevelUpExtension in LevelUpExtensionList.Value) {
                            Debug.Log(LevelUpExtension.gameObject.activeSelf);
                            Assert.IsFalse(LevelUpExtension.gameObject.activeSelf, buildingName + " a LevelUpExtension should not be activated from the Start");
                        }
                    }
                }

                // TODO Test LevelUp -> Meshes fading in

                // TODO Test InitLevelUpExtensions when Building is already on desired Level

            }

            yield return null;
        }


        // Test for: CalcNextUpgradeCost and setUpgradeCostNext
        [UnityTest]
        public IEnumerator TestCalcNextUpgradeCost() {
            yield return null;
        }



        // Test for: LevelUp and CurrentWorld Coins are raising
        [UnityTest]
        public IEnumerator TestLevelUpBuildingAndCoinProducing() {

            // get first Building
            Building firstBuilding = Globals.Game.currentWorld.buildingsProgressArray[0];

            // Check if at least one Building is in buildingsList
            Assert.IsNotNull(firstBuilding, "No Building initialized");

            // Save the LevelUp-relevant Values of the Building
            int initLevel = firstBuilding.getLevel();
            Assert.AreEqual(0, initLevel); // Without PlayerPrefs the Level of the Building must be 0
            IdleNum initUpgradeCost1 = firstBuilding.getUpgradeCost(1);
            long initGlobalLevels = Globals.Game.currentUser.scoreTotalLevels;

            yield return null;

            // Turn off the UnlockProcess
            Game.adminUnlockProcessSec = 0;

            // levelUp 1
            firstBuilding.levelUpDebug1();

            yield return null;

            // Test the Effects of the LevelUp
            Assert.Less(initLevel, firstBuilding.getLevel(), "LevelUp isn't working");
            Assert.AreEqual(initLevel + 1, firstBuilding.getLevel());
            Assert.IsTrue(initUpgradeCost1 < firstBuilding.getUpgradeCost(1));
            Assert.Less(initGlobalLevels, Globals.Game.currentUser.scoreTotalLevels);


            yield return null;

            // After at least one LevelUp we can test if the CurrentWorld-Buildings are producing coins
            IdleNum CurrentWorldCoins = Globals.Game.currentWorld.getCoins();

            // test if global Coins are initialized
            Assert.IsNotNull(CurrentWorldCoins, "Global Coins CurrentWorld not initialized");

            yield return new WaitForSeconds(1f);

            Assert.IsTrue(CurrentWorldCoins <= Globals.Game.currentWorld.getCoins());
        }


        // Test for: User has not enough Money for LevelUp is Building, but clicks on it
        [UnityTest]
        public IEnumerator TestLevelUpBuildingLevelUpBlockCoinsNotEnough() {

            // get first Building
            Building firstBuilding = Globals.Game.currentWorld.buildingsProgressArray[0];
            int initLevel = firstBuilding.getLevel();

            yield return null;

            // Set Coins to 0, so that the User has not enough Money for LevelUp
            Globals.Game.currentWorld.setCoins(new IdleNum(0));

            // Turn off the UnlockProcess for this Building
            Game.adminUnlockProcessSec = 0;
            Globals.KaloaSettings.adminUnlockProcessSec = Game.adminUnlockProcessSec;
            firstBuilding.level0To1DelaySeconds = 0;
            firstBuilding.setDelayUnlockProcessDelay(0);

            firstBuilding.levelUp(1);

            yield return null;

            Assert.AreEqual(initLevel, firstBuilding.getLevel(), "User had NOT enough coins, but leveled up his Building");

            yield return null;

            // Counter-Test
            Globals.Game.currentWorld.setCoins(new IdleNum(999, "K"));

            firstBuilding.levelUp(1);

            yield return null;

            Assert.AreNotEqual(initLevel, firstBuilding.getLevel(), "User had enough coins, but Building was not leveled up");

        }


        // Test for: DelayUnlock-Process
        [UnityTest]
        public IEnumerator TestLevelUpBuildingDelayUnlockProcess() {

            Globals.UICanvas.uiElements.PopUpQuests.SetActive(false);

            // Eliminate OfflineCoinPopUp
            Globals.UICanvas.uiElements.PopUpBG.SetActive(false);
            Globals.UICanvas.uiElements.PopUpOfflineCoins.SetActive(false);

            yield return null;

            int timeToUnlock = 3; // Min: 3

            // Move Cam
            Globals.UICanvas.uiElements.MainCamera.transform.position = new Vector3(162.0577f, 39.7f, 101.0166f);

            // get first Building
            Building firstBuilding = Globals.Game.currentWorld.buildingsProgressArray[0];
            int initLevel = firstBuilding.getLevel();
            Game.GetComponent<BuildingMenu>().openBuildingMenu(firstBuilding);

            // Check if firstBuilding has DelayUnlock-Process
            Transform DelayUnlockProcess = firstBuilding.transform.Find("DelayUnlock-Process");
            Assert.IsNotNull(DelayUnlockProcess, firstBuilding + ": DelayUnlock-Process not in Building");
            Transform DelayUnlockProcessBarFill = DelayUnlockProcess.Find("Bar/PercentBarPivot");

            Assert.IsFalse(firstBuilding.getIsCheckingDelayUnlockProcess());
            Assert.IsTrue(firstBuilding.getCurrentConstructionPhase() == Building.ConstructionPhases.ConstructionSite);
            Assert.IsFalse(Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.activeSelf);
            Assert.IsTrue(Globals.UICanvas.uiElements.BuildingMenuButtonLvlUp.GetComponent<Button>().interactable);
            Assert.IsFalse(DelayUnlockProcess.gameObject.activeSelf, firstBuilding.getName() + ": DelayUnlock-ProcessBar must be invisible at start");
            Assert.AreEqual(0, DelayUnlockProcessBarFill.localScale.z);

            yield return null;

            // Turn UnlockProcess to X Sec
            Game.adminUnlockProcessSec = timeToUnlock;
            Globals.KaloaSettings.adminUnlockProcessSec = timeToUnlock;
            firstBuilding.level0To1DelaySeconds = timeToUnlock;

            Globals.Game.currentWorld.setCoins(new IdleNum(999, "K")); // Give enough Money

            yield return null;

            // Level Up from Level 0 to 1 -> DelayUnlock-Process starts
            firstBuilding.levelUp(1);

            yield return null;

            Assert.IsTrue(firstBuilding.getIsCheckingDelayUnlockProcess());
            Assert.IsTrue(firstBuilding.getCurrentConstructionPhase() == Building.ConstructionPhases.InConstruction);
            Assert.AreEqual(initLevel, firstBuilding.getLevel(), "Building should be in DelayUnlock-Process but level up instant");
            Assert.IsTrue(Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.activeSelf);
            Assert.IsFalse(Globals.UICanvas.uiElements.BuildingMenuButtonLvlUp.GetComponent<Button>().interactable);
            Assert.IsTrue(DelayUnlockProcess.gameObject.activeSelf, firstBuilding.getName() + ": DelayUnlock-ProcessBar must be visible in the Process");
            Assert.AreEqual(0, DelayUnlockProcessBarFill.localScale.z, 0.01f);

            yield return new WaitForSeconds(timeToUnlock/2 + 0.1f);

            // Building should be on lvl 1, -> DelayUnlock-Process should be in process

            Assert.IsTrue(firstBuilding.getIsCheckingDelayUnlockProcess());
            Assert.IsTrue(firstBuilding.getCurrentConstructionPhase() == Building.ConstructionPhases.InConstruction);
            Assert.AreEqual(initLevel, firstBuilding.getLevel(), "Building should be in DelayUnlock-Process but level up after to few time");
            Assert.IsTrue(Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.activeSelf);
            Assert.IsFalse(Globals.UICanvas.uiElements.BuildingMenuButtonLvlUp.GetComponent<Button>().interactable);
            Assert.IsTrue(DelayUnlockProcess.gameObject.activeSelf, firstBuilding.getName() + ": DelayUnlock-ProcessBar must be visible in the Process");
            Assert.Less(0.2, DelayUnlockProcessBarFill.localScale.z);

            yield return new WaitForSeconds(timeToUnlock*2);

            // Building should be on lvl 1, -> DelayUnlock-Process should be ended

            Assert.AreEqual(initLevel+1, firstBuilding.getLevel(), "Building should finished DelayUnlock-Process, but didnt level up");
            Assert.IsFalse(Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.activeSelf);
            Assert.IsTrue(firstBuilding.getCurrentConstructionPhase() == Building.ConstructionPhases.Finished);
            Assert.IsTrue(Globals.UICanvas.uiElements.BuildingMenuButtonLvlUp.GetComponent<Button>().interactable);
            Assert.IsFalse(DelayUnlockProcess.gameObject.activeSelf, firstBuilding.getName() + ": DelayUnlock-ProcessBar must be invisible after the Process");
            Assert.Less(0.5, DelayUnlockProcessBarFill.localScale.z);

            yield return null;

            // LvlUp Test over BuildingMenu Button
            Globals.UICanvas.uiElements.BuildingMenuButtonLvlUp.GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.AreEqual(initLevel + 2, firstBuilding.getLevel(), "Building should didnt lvl up after click on LvlUpButton in BuildingMenu");

        }



    }
}
