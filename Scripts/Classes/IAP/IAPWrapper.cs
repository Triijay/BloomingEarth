using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;


// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
public class IAPWrapper : MonoBehaviour, IStoreListener {

    public GameObject shopPopUp;

    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

    // Product identifiers for all products capable of being purchased: 
    // "convenience" general identifiers for use with Purchasing, and their store-specific identifier 
    // counterparts for use with and outside of Unity Purchasing. Define store-specific identifiers 
    // also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)

    Dictionary<string, IAPItem> purchasableItems = new Dictionary<string, IAPItem>();


    // Performance Optimization
    bool itemIsOneTimeItemAlreadyRedeemed;

    void Start() {
        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null) {
            // Begin to configure our connection to Purchasing
            if (!Globals.KaloaSettings.preventIAPCommunication) {
                InitializePurchasing();
            }
        }
    }

    public IStoreController getStoreController() {
        return m_StoreController;
    }

    /// <summary>
    /// ONLY FOR TEST PURPOSES
    /// </summary>
    public void resetStoreController() {
        m_StoreController = null;
    }

    public void InitializePurchasing() {
        // If we have already connected to Purchasing ...
        if (IsInitialized()) {
            // ... we are done here.
            return;
        }

        // Create a builder, first passing in a suite of Unity provided stores.
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (IAPItem shopIAPItem in shopPopUp.GetComponentsInChildren<IAPItem>()) {

            if (shopIAPItem.productType == ProductType.Subscription) {
                // And finish adding the subscription product. Notice this uses store-specific IDs, illustrating
                // if the Product ID was configured differently between Apple and Google stores. Also note that
                // one uses the general kProductIDSubscription handle inside the game - the store-specific IDs 
                // must only be referenced here. 
                //builder.AddProduct(kProductIDSubscription, ProductType.Subscription, new IDs(){
                //    { kProductNameAppleSubscription, AppleAppStore.Name },
                //    { kProductNameGooglePlaySubscription, GooglePlay.Name },
                //});

                shopIAPItem.gameObject.SetActive(false);
            } else {
                // Add a product to sell / restore by way of its identifier, associating the general identifier
                // with its store-specific identifiers.

                shopIAPItem.initItemBasics();

                try {
                    builder.AddProduct(shopIAPItem.productIDGoogle, shopIAPItem.productType);
                    purchasableItems.Add(shopIAPItem.productIDGoogle, shopIAPItem);
                }
                catch {
                    shopIAPItem.gameObject.SetActive(false);
                }

            }
        }

        // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
        // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
        //UnityPurchasing.Initialize(this, builder);
    }


    private bool IsInitialized() {
        // Only say we are initialized if both the Purchasing references are set.
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }






    public void BuyProduct(IAPItem item) {
        // Buy the consumable product using its general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.

        if (oneTimeItemAlreadyRedeemed(item)) {
            return;
        }

        if (Application.platform == RuntimePlatform.WindowsEditor || Social.localUser.authenticated) {
            StartCoroutine(DisableAllShopButtonsFor1Sec());
            BuyProductID(item);
        } else {
            Globals.Controller.GPiOS.signIn();
        }
    }

    /// <summary>
    /// Boosts the current World with Emeralds
    /// </summary>
    public void buyItemForEmeralds(IAPItem item) {

        if (oneTimeItemAlreadyRedeemed(item)) {
            return;
        }

        // If User has enough Emeralds
        if (Globals.Game.currentUser.Emeralds >= item.emeraldCost) {

            StartCoroutine(DisableAllShopButtonsFor1Sec());

            // Activate Boost
            item.ExecuteBuyedItem();


            // Log Firebase Event
            if (!Social.localUser.authenticated) {
                KeyValuePair<string, object>[] valuePairArray = {
                    new KeyValuePair<string, object>("item", item.productIDGoogle),
                    new KeyValuePair<string, object>("itemEmeraldCost", item.emeraldCost),
                    new KeyValuePair<string, object>("FirstGameStart", Globals.Game.currentUser.stats.FirstGameLoad.ToString()),
                    new KeyValuePair<string, object>("DaysWithGameOpening", Globals.Game.currentUser.stats.DaysWithGameOpening),
                    new KeyValuePair<string, object>("MinutesPlayedOverall", (int)(Globals.Game.currentUser.stats.SecondsPlayedOverall + Time.time)/60),
                    new KeyValuePair<string, object>("GoogleLogin", "false"),
                };
                Globals.Controller.Firebase.IncrementFirebaseEventWithParameters(
                "purchased_item_for_emeralds_notloggedin", valuePairArray);

            } else {
                KeyValuePair<string, object>[] valuePairArray = {
                    new KeyValuePair<string, object>("item", item.productIDGoogle),
                    new KeyValuePair<string, object>("itemEmeraldCost", item.emeraldCost),
                    new KeyValuePair<string, object>("FirstGameStart", Globals.Game.currentUser.stats.FirstGameLoad.ToString()),
                    new KeyValuePair<string, object>("DaysWithGameOpening", Globals.Game.currentUser.stats.DaysWithGameOpening),
                    new KeyValuePair<string, object>("MinutesPlayedOverall", (int)(Globals.Game.currentUser.stats.SecondsPlayedOverall + Time.time)/60),
                    new KeyValuePair<string, object>("GoogleLogin", Social.localUser.userName),
                };
                Globals.Controller.Firebase.IncrementFirebaseEventWithParameters(
                "purchased_item_for_emeralds_loggedin", valuePairArray);

            }

            // Reduce Users Emeralds
            Globals.Game.currentUser.reduceEmeralds(item.emeraldCost);
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
              Firebase.Analytics.FirebaseAnalytics.EventSpendVirtualCurrency,
              new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                  Firebase.Analytics.FirebaseAnalytics.ParameterItemName, item.productIDGoogle),
                new Firebase.Analytics.Parameter(
                  Firebase.Analytics.FirebaseAnalytics.ParameterValue, item.emeraldCost),
                new Firebase.Analytics.Parameter(
                  Firebase.Analytics.FirebaseAnalytics.ParameterVirtualCurrencyName, "Emeralds"),
              }
            );

            // Save Selling-Information in User
            Globals.Game.currentUser.buyedItemsList.Add(
                new IAPBuyedItem(item.productIDGoogle,
                "Google PlayStore",
                "EmeraldSell",
                System.DateTime.UtcNow.ToString(),
                item.emeraldCost
                ));
        } else {
            // User "Not Enough Emeralds"
        }

    }

    /// <summary>
    /// Check, if the Item is a one-time-redeemer and if the User already owns it<br></br>
    /// Returns false if its not a InventoryItem, its not a oneTimeRedeemable or its one of these but the User doesn't own it
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool oneTimeItemAlreadyRedeemed(IAPItem item) {
        // Some Items are only redeemable once
        if (item as IAPItemToInventory != null) {
            if (((IAPItemToInventory)item).oneTimeRedeemable) {
                // Check if its already in inventory
                if (Globals.Game.currentUser.inventory.containsItem(((IAPItemToInventory)item).buyableItem)) {
                    return true;
                }
            }
        }
        return false;
    }



    void BuyProductID(IAPItem item) {
        // If Purchasing has been initialized ...
        if (IsInitialized()) {
            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            Product product = m_StoreController.products.WithID(item.productIDGoogle);

            // If the look up found a product for this device's store and that product is ready to be sold ... 
            if (product != null && product.availableToPurchase) {
                Globals.UICanvas.DebugLabelAddText(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                // asynchronously.
                m_StoreController.InitiatePurchase(product);
            }
            // Otherwise ...
            else {
                // ... report the product look-up failure situation  
                Globals.UICanvas.DebugLabelAddText("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        // Otherwise ...
        else {
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            Globals.UICanvas.DebugLabelAddText("BuyProductID FAIL. Not initialized.");
        }
    }


    // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
    // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
    public void RestorePurchases() {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized()) {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Globals.UICanvas.DebugLabelAddText("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer) {
            // ... begin restoring purchases
            Globals.UICanvas.DebugLabelAddText("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) => {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                Globals.UICanvas.DebugLabelAddText("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        // Otherwise ...
        else {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Globals.UICanvas.DebugLabelAddText("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }


    //  
    // --- IStoreListener
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Globals.UICanvas.DebugLabelAddText("Shop initialized");

        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;


        foreach (IAPItem shopIAPItem in shopPopUp.GetComponentsInChildren<IAPItem>()) {
            try {
                shopIAPItem.initializeShopPresentation();
            }
            catch (Exception e) {
                Globals.UICanvas.DebugLabelAddText("Err on InitIAPItem " + shopIAPItem.productIDGoogle + ": " + e);
                throw;
            }
        }

        // Check Buttons
        checkButtonsShouldBeInteractible();
    }


    public void OnInitializeFailed(InitializationFailureReason error) {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Globals.UICanvas.DebugLabelAddText("OnInitializeFailed InitializationFailureReason:" + error);
    }


    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) {
        // A consumable product has been purchased by this user.
        if (purchasableItems.ContainsKey(args.purchasedProduct.definition.id)) {

            // Item is known by purchasable Items
            // Execute its function
            IAPItem purchasedItem = null;
            purchasableItems.TryGetValue(args.purchasedProduct.definition.id, out purchasedItem);
            purchasedItem.ExecuteBuyedItem();

            // Log Firebase Event
            KeyValuePair<string, object>[] valuePairArray = {
                new KeyValuePair<string, object>("item", purchasedItem.productIDGoogle),
                new KeyValuePair<string, object>("itemPrice", m_StoreController.products.WithID(purchasedItem.productIDGoogle).metadata.localizedPriceString),
                new KeyValuePair<string, object>("FirstGameStart", Globals.Game.currentUser.stats.FirstGameLoad.ToString()),
                new KeyValuePair<string, object>("DaysWithGameOpening", Globals.Game.currentUser.stats.DaysWithGameOpening),
                new KeyValuePair<string, object>("MinutesPlayedOverall", (int)(Globals.Game.currentUser.stats.SecondsPlayedOverall + Time.time)/60),
                new KeyValuePair<string, object>("GoogleLogin", Social.localUser.userName),
            };

            Globals.Controller.Firebase.IncrementFirebaseEventWithParameters(
                "purchased_item_for_money", valuePairArray);

            // Save Selling-Information in User
            Globals.Game.currentUser.buyedItemsList.Add(
                new IAPBuyedItem(purchasedItem.productIDGoogle,
                "Google PlayStore",
                m_StoreController.products.WithID(purchasedItem.productIDGoogle).metadata.localizedPriceString,
                System.DateTime.UtcNow.ToString(),
                -1
                ));

        } else {
            // Or ... an unknown product has been purchased by this user. Fill in additional products here....
            Globals.UICanvas.DebugLabelAddText(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
        }

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed. 
        return PurchaseProcessingResult.Complete;
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
        // this reason with the user to guide their troubleshooting actions.
        Globals.UICanvas.DebugLabelAddText(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }









    /// <summary>
    /// Checks for each Shop Item, if the User has enough Emeralds or has already buyed a one-time-redeemable
    /// </summary>
    public void checkButtonsShouldBeInteractible() {

        try {

            foreach (IAPItem shopIAPItem in shopPopUp.GetComponentsInChildren<IAPItem>()) {

                itemIsOneTimeItemAlreadyRedeemed = false;

                if (oneTimeItemAlreadyRedeemed(shopIAPItem)) {
                    shopIAPItem.InformUserAlreadyRedeemed();
                    itemIsOneTimeItemAlreadyRedeemed = true;
                }


                if (shopIAPItem.hasEmeraldButton()) {
                    // Check if Emeralds of User are enough
                    if (Globals.Game.currentUser.Emeralds >= shopIAPItem.emeraldCost && !itemIsOneTimeItemAlreadyRedeemed) {
                        shopIAPItem.EmeraldButtonComponent.interactable = true;
                    } else {
                        shopIAPItem.EmeraldButtonComponent.interactable = false;
                    }
                }

                if (shopIAPItem.hasRealMoneyBuyButton()) {
                    if (!itemIsOneTimeItemAlreadyRedeemed) {
                        shopIAPItem.BuyButtonComponent.interactable = true;
                    } else {
                        shopIAPItem.BuyButtonComponent.interactable = false;
                    }
                }

            }

        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e, true);
        }

    }


    /// <summary>
    /// Disables all Shop Buttons for 1 Sec and enable them then again (to prevent double purchases)
    /// </summary>
    private IEnumerator DisableAllShopButtonsFor1Sec() {

        foreach (IAPItem shopIAPItem in shopPopUp.GetComponentsInChildren<IAPItem>()) {
            if (shopIAPItem.hasEmeraldButton()) {
                shopIAPItem.EmeraldButtonComponent.interactable = false;
            }
            if (shopIAPItem.hasRealMoneyBuyButton()) {
                shopIAPItem.BuyButtonComponent.interactable = false;
            }
        }

        yield return new WaitForSeconds(1.5f);

        checkButtonsShouldBeInteractible();

        yield return null;
    }


    /// <summary>
    /// Returns the purchasable items List
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, IAPItem> getPurchasableItems() {
        return purchasableItems;
    }

}