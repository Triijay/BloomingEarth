using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
//using UnityEngine.Purchasing;

namespace Tests {
    public class TestSuiteShop {

        public InitGame Game;


        [UnitySetUp]
        public IEnumerator UnitySetUp() {
            // TestSettings
            Globals.KaloaSettings.preventPlayfabCommunication = true;
            Globals.KaloaSettings.preventIAPCommunication = false;
            Globals.KaloaSettings.preventGoogleCommunication = false;
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
            Globals.UICanvas.uiElements.PopUpReconnectCloud.SetActive(false);

            // Reset the main Controller
            Globals.Controller.IAP.resetStoreController();

            // Shop Test specifix
            Globals.UICanvas.uiElements.ShopPopUp.SetActive(true);

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




        // Test for: Check if Shop is initiated correctly
        [UnityTest]
        public IEnumerator CheckShopBasics() {

            yield return new WaitForSeconds(3);

            Assert.IsNotNull(Globals.Controller.IAP.getPurchasableItems());
            Assert.Less(0, Globals.Controller.IAP.getPurchasableItems().Count); // Bug: fails if other tests where running

            Assert.IsNotNull(Globals.Controller.IAP.shopPopUp);

            // Check if all IAP GameObjects are in the purchasable Items List
            Assert.AreEqual(Globals.Controller.IAP.getPurchasableItems().Count, Globals.Controller.IAP.shopPopUp.GetComponentsInChildren<IAPItem>().Length);

            yield return new WaitForSeconds(5);
        }


        // Test for: Check if Shop Items are correct
        [UnityTest]
        public IEnumerator CheckShopItems() {

            yield return null;

            // Add Enough Emeralds, so that Buttons are interactible
            Globals.Game.currentUser.raiseEmeralds(1300);

            yield return new WaitForSeconds(0.5f);

            foreach (IAPItem IAPitem in Globals.Controller.IAP.shopPopUp.GetComponentsInChildren<IAPItem>()) {

                // Check if everything is correctly dragged in in Unity
                Assert.IsNotEmpty(IAPitem.productIDGoogle, "Item " + IAPitem.gameObject.name + " has no productIDGoogle");
                Assert.IsNotNull(IAPitem.ItemTitleField, "Item " + IAPitem.gameObject.name + " has no ItemTitleField");
                Assert.IsNotNull(IAPitem.ItemDescriptionField, "Item " + IAPitem.gameObject.name + " has no ItemDescriptionFields");

                // At the Moment we have only Consumables
                // Assert.AreEqual(ProductType.Consumable, item.productType);
                // Cannot Test because assemby reference problem

                // At Least One Button must be active and enabled
                Assert.IsTrue(IAPitem.BuyButton != null || IAPitem.EmeraldButton != null);

                if (IAPitem.BuyButton != null) {
                    Assert.IsNotNull(IAPitem.BuyButton.GetComponent<Button>());
                    Assert.IsTrue(IAPitem.BuyButton.GetComponent<Button>().interactable);
                }
                if (IAPitem.EmeraldButton != null) {
                    Assert.IsNotNull(IAPitem.EmeraldButton.GetComponent<Button>());
                    Assert.IsTrue(IAPitem.EmeraldButton.GetComponent<Button>().interactable);
                    Assert.Less(-1, IAPitem.emeraldCost, "Item " + IAPitem.gameObject.name + " has EmeraldButton, but no EmeraldCost");
                }

                // Check every free Item has no BuyButton
                if (IAPitem.emeraldCost == 0 && IAPitem.EmeraldButton != null) {
                    Assert.IsNull(IAPitem.BuyButton, "Free Items must not have a Buy Button, because we cannot set them to free in Google PlayStore");
                }

                // Check Items Special Types, because there are no IAPitem (its virtual) -> only Children of Item
                if (IAPitem as IAPItemBoost) {
                    IAPItemBoost miau1 = (IAPItemBoost)IAPitem;
                    Assert.Less(1, miau1.multiplier, "Component-config not correct: " + miau1.name);
                    Assert.Less(1, miau1.hours);
                }

                if (IAPitem as IAPItemEmeraldBuy) {
                    IAPItemEmeraldBuy miau2 = (IAPItemEmeraldBuy)IAPitem;
                    Assert.Less(1, miau2.emeraldsToUser, "Component-config not correct: " + miau2.name);
                }

                if (IAPitem as IAPItemToInventory) {
                    IAPItemToInventory miau3 = (IAPItemToInventory)IAPitem;
                    Assert.IsNotNull(miau3.buyableItem, "Component-config not correct: " + miau3.name);
                }

            }

            yield return null;
        }
    }
}