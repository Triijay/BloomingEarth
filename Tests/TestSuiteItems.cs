using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class TestSuiteItems {

        public InitGame Game;

        IdleNum zeroIdle = new IdleNum(0);



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
            Globals.UICanvas.uiElements.PopUpFeedback.SetActive(false);

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




       
        // Test for: Inventory with Items
        [UnityTest]
        public IEnumerator CheckInventoryBasics() {

            List<Property> propertyList = Globals.Game.currentUser.inventory.getPropertyList();

            // Test Inventory
            Assert.IsNotNull(propertyList);

            // One Item into inv
            Globals.Game.currentUser.inventory.createProperty("TesterBuildingUpgrades");
            Assert.AreEqual(1, propertyList.Count);

            // same item twice
            Globals.Game.currentUser.inventory.createProperty("TesterBuildingUpgrades");
            Assert.AreEqual(2, propertyList.Count);

            // third item (other)
            Globals.Game.currentUser.inventory.createProperty("TesterOnlyForHouse");
            Assert.AreEqual(3, propertyList.Count);

            // Contains Function
            Assert.IsTrue(Globals.Game.currentUser.inventory.containsItem((BuildingUpgrade)propertyList[0].ReferencedItem));

            yield return null;
        }


        // Test for: Inventory To Building
        [UnityTest]
        public IEnumerator TestAttachItemToBuilding() {

            List<Property> propertyList = Globals.Game.currentUser.inventory.getPropertyList();

            // get House at level 1
            HybridBuilding house = (HybridBuilding)Globals.Game.currentWorld.buildingsProgressArray[0];
            house.levelUp(1, true);

            // Test House is unaffected by Environment-Effects
            Assert.AreEqual(house.environmentFactorOR, house.getCurrentEnvironmentFactor());

            // Test Inventory
            Assert.IsNotNull(propertyList);


            // One Item into inv
            Globals.Game.currentUser.inventory.createProperty("TesterBuildingUpgrades");
            Assert.AreEqual(1, propertyList.Count);

            // Test: Other Slots working
            Globals.Game.currentUser.inventory.createProperty("TesterBuildingUpgrades");
            Assert.AreEqual(2, propertyList.Count);
            Globals.Game.currentUser.inventory.createProperty("TesterBuildingUpgrades");
            Assert.AreEqual(3, propertyList.Count);

            // Test unused Function
            Assert.AreEqual(3, Globals.Game.currentUser.inventory.getPropertyListOnlyUnused().Count);


            // Test Slot is locked
            house.addBuildingUpgrade(propertyList[1], 2);
            LogAssert.Expect(LogType.Error, "Item Slot is not unlocked yet");
            // Check if there are still 3 unused Items
            Assert.AreEqual(3, Globals.Game.currentUser.inventory.getPropertyListOnlyUnused().Count); 

            // Unlock slots
            house.levelUp(1000, true);
            float enviFactorWith0Item = house.getCurrentEnvironmentFactor();
            IdleNum incomeWith0Item = house.getCurrentIncome();


            // Test: add Items to Building
            house.addBuildingUpgrade(propertyList[0], 1);

            // Test: Item should be marked as Used at inventory
            Assert.AreEqual(2, Globals.Game.currentUser.inventory.getPropertyListOnlyUnused().Count);
            Assert.NotNull(house.getUpgradeSlot(1));

            // Test: Item affects building -> Envi
            Assert.Less(house.getCurrentEnvironmentFactor(), enviFactorWith0Item);
            Assert.AreEqual(enviFactorWith0Item * ((BuildingUpgrade)propertyList[0].ReferencedItem).factorEnvironmentAffection, house.getCurrentEnvironmentFactor());
            float enviFactorWith1Item = house.getCurrentEnvironmentFactor();

            // Test: Item affects building -> Coins
            Assert.IsTrue(house.getCurrentIncome() < incomeWith0Item);
            IdleNum expectedIncome = incomeWith0Item * ((BuildingUpgrade)propertyList[0].ReferencedItem).factorCoinProducing;
            Assert.AreEqual(expectedIncome.toRoundedString(), house.getCurrentIncome().toRoundedString(), "Building was not correctly affected by Item in coin producing " +
                "-> expected " + expectedIncome.toRoundedString() + 
                " - actual: " + house.getCurrentIncome().toRoundedString() );
            IdleNum incomeWith1Item = house.getCurrentIncome();


            // Testing slot 2
            house.addBuildingUpgrade(propertyList[1], 2);
            Assert.AreEqual(1, Globals.Game.currentUser.inventory.getPropertyListOnlyUnused().Count);

            // Test: Items in slot 2 affecting building -> Envi
            Assert.Less(house.getCurrentEnvironmentFactor(), enviFactorWith1Item);
            float enviFactorWith2Items = house.getCurrentEnvironmentFactor();

            // Test: Items in slot 2 affecting building -> Coins
            Assert.IsTrue(house.getCurrentIncome() < incomeWith1Item);
            IdleNum incomeWith2Items = house.getCurrentIncome();



            // Testing slot 3
            house.addBuildingUpgrade(propertyList[2], 3);
            Assert.AreEqual(0, Globals.Game.currentUser.inventory.getPropertyListOnlyUnused().Count);

            // Test: Items in slot 3 affecting building -> Envi
            Assert.Less(house.getCurrentEnvironmentFactor(), enviFactorWith2Items);
            float enviFactorWith3Items = house.getCurrentEnvironmentFactor();

            // Test: Items in slot 3 affecting building -> Coins
            Assert.IsTrue(house.getCurrentIncome() < incomeWith2Items);
            IdleNum incomeWith3Items = house.getCurrentIncome();



            yield return null;
        }


        // Test for: Attaching an Item to a uncompatible Building shouldnt work
        [UnityTest]
        public IEnumerator TestAttachItemToBuildingNotCompatible() {

            List<Property> propertyList = Globals.Game.currentUser.inventory.getPropertyList();

            // get House at level 1
            HybridBuilding house = (HybridBuilding)Globals.Game.currentWorld.buildingsProgressArray[0];

            // Test Inventory
            Assert.IsNotNull(propertyList);

            // One Item into inv
            Globals.Game.currentUser.inventory.createProperty("TesterBuildingUpgrades");
            Assert.AreEqual(1, propertyList.Count);

            ((BuildingUpgrade)propertyList[0].ReferencedItem).compatibleBuildingTypes = BuildingUpgrade.CompatibleBuildingTypes.EnviBuildingOnly;

            // Test: add Item to Building -> But the Item is only for EnviBuildings
            house.addBuildingUpgrade(propertyList[0], 1);

            // Test: Item should stay in inventory and Slot on Building is still empty
            LogAssert.Expect(LogType.Error, "Item is not a compatible with Building");
            Assert.AreEqual(1, Globals.Game.currentUser.inventory.getPropertyListOnlyUnused().Count);
            Assert.AreEqual(null, house.getUpgradeSlot(1));

            yield return null;
        }


        // Test for: BuildingItem Compatibility
        [UnityTest]
        public IEnumerator TestBuildingUpgradeCompatibility() {

            List<Property> propertyList = Globals.Game.currentUser.inventory.getPropertyList();

            // get Buildings
            HybridBuilding house = (HybridBuilding)Globals.Game.currentWorld.buildingsProgressArray[0];
            EnviBuilding windturbine = (EnviBuilding)Globals.Game.currentWorld.buildingsProgressArray[2];
            HybridBuilding beekeeper = (HybridBuilding)Globals.Game.currentWorld.buildingsProgressArray[7];
            HybridBuilding windmill = (HybridBuilding)Globals.Game.currentWorld.buildingsProgressArray[3];

            // Check if Buildings meets conditions for test, to ensure test is working correctly
            Assert.AreEqual("House", house.gameObject.name);
            Assert.AreEqual("Windturbine", windturbine.gameObject.name);
            Assert.AreEqual("Beekeeper", beekeeper.gameObject.name);
            Assert.AreEqual("Windmill", windmill.gameObject.name);
            Assert.Less(house.environmentFactorOR, 0);
            Assert.Greater(beekeeper.environmentFactorOR, 0);
            Assert.AreEqual(windmill.environmentFactorOR, 0);

            // One Item into inv
            Globals.Game.currentUser.inventory.createProperty("TesterBuildingUpgrades");
            Assert.AreEqual(1, propertyList.Count);


            // Test AllBuildings
            ((BuildingUpgrade)propertyList[0].ReferencedItem).compatibleBuildingTypes = BuildingUpgrade.CompatibleBuildingTypes.AllBuildings;

            Assert.IsTrue(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(house));
            Assert.IsTrue(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(windturbine));
            Assert.IsTrue(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(beekeeper));
            Assert.IsTrue(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(windmill));


            // Test EnviBuildingOnly
            ((BuildingUpgrade)propertyList[0].ReferencedItem).compatibleBuildingTypes = BuildingUpgrade.CompatibleBuildingTypes.EnviBuildingOnly;

            Assert.IsFalse(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(house));
            Assert.IsTrue(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(windturbine));
            Assert.IsFalse(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(beekeeper));
            Assert.IsFalse(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(windmill));


            // Test HybridBuildingOnly
            ((BuildingUpgrade)propertyList[0].ReferencedItem).compatibleBuildingTypes = BuildingUpgrade.CompatibleBuildingTypes.HybridBuilding;

            Assert.IsTrue(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(house));
            Assert.IsFalse(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(windturbine));
            Assert.IsTrue(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(beekeeper));
            Assert.IsTrue(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(windmill));


            // Test HybridBuildingOnlyNegative
            ((BuildingUpgrade)propertyList[0].ReferencedItem).compatibleBuildingTypes = BuildingUpgrade.CompatibleBuildingTypes.HybridBuildingOnlyNegativeForEnvironment;

            Assert.IsTrue(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(house));
            Assert.IsFalse(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(windturbine));
            Assert.IsFalse(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(beekeeper));
            Assert.IsFalse(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(windmill));


            // Test HybridBuildingOnlyPositive
            ((BuildingUpgrade)propertyList[0].ReferencedItem).compatibleBuildingTypes = BuildingUpgrade.CompatibleBuildingTypes.HybridBuildingPositiveOrNeutralForEnvironment;

            Assert.IsFalse(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(house));
            Assert.IsFalse(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(windturbine));
            Assert.IsTrue(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(beekeeper));
            Assert.IsTrue(((BuildingUpgrade)propertyList[0].ReferencedItem).isBuildingUpgradeCompatibleTo(windmill));


            // Test: compatibility List
            Globals.Game.currentUser.inventory.createProperty("TesterOnlyForHouse");
            Assert.AreEqual(2, propertyList.Count);

            // Check if Tester is OK
            Assert.AreEqual(((BuildingUpgrade)propertyList[1].ReferencedItem).compatibleBuildingTypes, BuildingUpgrade.CompatibleBuildingTypes.AllBuildings);
            Assert.AreEqual(((BuildingUpgrade)propertyList[1].ReferencedItem).compatibleBuildings.Length, 1);
            Assert.AreEqual(((BuildingUpgrade)propertyList[1].ReferencedItem).compatibleBuildings[0], house);

            Assert.IsTrue(((BuildingUpgrade)propertyList[1].ReferencedItem).isBuildingUpgradeCompatibleTo(house));
            Assert.IsFalse(((BuildingUpgrade)propertyList[1].ReferencedItem).isBuildingUpgradeCompatibleTo(windturbine));
            Assert.IsFalse(((BuildingUpgrade)propertyList[1].ReferencedItem).isBuildingUpgradeCompatibleTo(beekeeper));
            Assert.IsFalse(((BuildingUpgrade)propertyList[1].ReferencedItem).isBuildingUpgradeCompatibleTo(windmill));

            yield return null;
        }


        // Test for: Every Item has translation
        [UnityTest]
        public IEnumerator CheckEveryItemHasTranslation() {

            foreach (ItemTemplate anItem in Globals.Game.currentUser.inventory.availableItems.Values) {
                Assert.IsTrue(Globals.Controller.Language.getXmlFileStrings().ContainsKey("itemname_" + anItem.name),
                    "Item " + anItem.name + " has no translation");
                Assert.IsNotEmpty(anItem.getTranslatedItemName(), "Item " + anItem.name + ": getTranslatedItemName returns empty string");
            }

            yield return null;
        }


        // Test for: Every Item in "Items" has an unique Name
        [UnityTest]
        public IEnumerator CheckEveryItemHasUniqueName() {

            List<string> itemNames = new List<string>();

            foreach (ItemTemplate anItem in GameObject.FindObjectsOfType<ItemTemplate>()) {

                string anItemName = anItem.getItemName();

                // Check if ItemName wasnt there
                Assert.IsFalse(itemNames.Contains(anItemName));

                // Add itemName to itemNames
                itemNames.Add(anItemName);
            }

            yield return null;
        }

        // Test for: Check every BuildingUpgradeSlotHandler has an assigned Inventory
        [UnityTest]
        public IEnumerator CheckEveryBuildingUpgradeSlotHandlerHasInventory() {


            foreach (BuildingUpgradeSlotHandler anBuildingUpgradeSlotHandler in GameObject.FindObjectsOfType<BuildingUpgradeSlotHandler>()) {

                // Check
                Assert.IsNotNull(anBuildingUpgradeSlotHandler.connectedInventory);
            }

            yield return null;
        }


        // Test for: Check every UI_Inventory has an assigned basics
        [UnityTest]
        public IEnumerator CheckUI_InventoryBasics() {

            foreach (UI_Inventory anUI_Inventory in GameObject.FindObjectsOfType<UI_Inventory>()) {

                // Check
                Assert.IsNotNull(anUI_Inventory.inventoryItemContainer);
                Assert.IsNotNull(anUI_Inventory.inventoryItemTemplate);
            }

            yield return null;
        }


        // Test for: Check BuildingUpgrade Basics
        [UnityTest]
        public IEnumerator CheckBuildingUpgradeBasics() {

            foreach (BuildingUpgrade anBuildingUpgrade in GameObject.FindObjectsOfType<BuildingUpgrade>()) {

                // Check
                Assert.IsNotNull(anBuildingUpgrade.factorCoinProducing);
                Assert.GreaterOrEqual(anBuildingUpgrade.factorCoinProducing, 0);

                Assert.IsNotNull(anBuildingUpgrade.factorEnvironmentAffection);
                Assert.GreaterOrEqual(anBuildingUpgrade.factorEnvironmentAffection, 0);

            }

            yield return null;
        }


        // Test for: MonumentsOK
        [UnityTest]
        public IEnumerator CheckMonumentsValid() {

            Assert.IsNotNull(Globals.UICanvas.uiElements.Items.transform.Find("Monuments"), "Monument-Items has been removed, dragged out or renamed.");

            foreach (Monument aMonument in Globals.UICanvas.uiElements.Items.transform.Find("Monuments").GetComponentsInChildren<Monument>()) {
                Assert.IsNotNull(aMonument.monumentObject);
                aMonument.placeMonumentInMonumentSlot();

                yield return new WaitForSeconds(1);
            }


            yield return null;
        }


        // Test for: Out Test Items in Initgame must be inactive
        [UnityTest]
        public IEnumerator CheckTestItemsNotActivated() {

            Assert.AreEqual(0, Globals.Game.currentUser.inventory.getPropertyList().Count);
            
            yield return null;
        }



    }
}
