using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public abstract class IAPItem : MonoBehaviour {

    public string productIDGoogle;

    public int emeraldCost = -1;

    public ProductType productType;

    public GameObject ItemTitleField;
    public GameObject ItemDescriptionField;

    public GameObject BuyButton;
    public GameObject EmeraldButton;

    [HideInInspector]
    public Button BuyButtonComponent;
    [HideInInspector]
    public Button EmeraldButtonComponent;

    public float originalBuyPriceEUR = 0;
    public float originalBuyPriceUSD = 0;
    private bool strikepriceInserted = false;

    private bool isInitialized = false;


    /// <summary>
    /// Inits the Basics of an IAPItem
    /// </summary>
    public void initItemBasics() {
        if (BuyButton != null) {
            BuyButtonComponent = BuyButton.GetComponent<Button>();
        }
        if (EmeraldButton != null) {
            EmeraldButtonComponent = EmeraldButton.GetComponent<Button>();
        }
        reactToInternetConnection();
    }


    /// <summary>
    /// Inits the Shop Item - Pulls the Description and Price from Google Shop
    /// </summary>
    public void initializeShopPresentation() {

        // BuyButton enable when Internet Connection
        if (Application.internetReachability != NetworkReachability.NotReachable) {
            // Connected
            if (BuyButtonComponent != null) {
                BuyButtonComponent.interactable = true;
            }
        }

        var m_StoreController = Globals.Controller.IAP.getStoreController();

        if (!m_StoreController.products.WithID(productIDGoogle).metadata.localizedTitle.Contains("inactive")) {

            //Globals.UICanvas.DebugLabelAddText(productIDGoogle + " available");
            translateShopItem(m_StoreController);

        } else {
            Globals.UICanvas.DebugLabelAddText(productIDGoogle + " not available -> hiding GameObject");
            this.gameObject.SetActive(false);
        }

        isInitialized = true;

    }

    /// <summary>
    /// Post-Inits the Shop Item by InternetConnection<br></br>
    /// Locks Buttons, Descriptions etc, when no Connection
    /// </summary>
    public virtual void reactToInternetConnection() {
        if (Application.internetReachability == NetworkReachability.NotReachable) {
            // Not Connected
            if (BuyButtonComponent != null) {
                BuyButtonComponent.interactable = false;
            }

            ItemDescriptionField.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Globals.Controller.Language.translateString("shop_no_internet");
        }
    }

    /// <summary>
    /// Gets the Translated Texts out of Google
    /// </summary>
    public virtual void translateShopItem(IStoreController m_StoreController) {

        // Inits
        strikepriceInserted = false;

        ItemTitleField.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = m_StoreController.products.WithID(productIDGoogle).metadata.localizedTitle.Replace(" (Blooming Earth Idle)", "").Replace(" (Blooming Earth)", "");
        ItemDescriptionField.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = m_StoreController.products.WithID(productIDGoogle).metadata.localizedDescription;

        if (BuyButton != null) {
            BuyButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "";

            // Strike-Trough-Price
            if (EmeraldButton == null) {
                // When Emerald Button is not active -> otherwise there wouldnt be enough space
                if (originalBuyPriceEUR > 0 && m_StoreController.products.WithID(productIDGoogle).metadata.isoCurrencyCode == "EUR") {
                    BuyButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text += "<s> " + originalBuyPriceEUR.ToString("N2", System.Globalization.CultureInfo.CreateSpecificCulture("de-DE")) + " € </s> ";
                    strikepriceInserted = true;
                } else if (originalBuyPriceUSD > 0 && m_StoreController.products.WithID(productIDGoogle).metadata.isoCurrencyCode == "USD") {
                    BuyButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text += "<s> " + originalBuyPriceUSD.ToString("N2", System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) + " $ </s>";
                    strikepriceInserted = true;
                }
                // if we want to show % -> originalBuyPriceEUR / (float)m_StoreController.products.WithID(productIDGoogle).metadata.localizedPrice
            }

            // Make new price Strong, if reduced
            if (strikepriceInserted) {
                BuyButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text += " <b>" + m_StoreController.products.WithID(productIDGoogle).metadata.localizedPriceString + "</b>";
            } else {
                BuyButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text += " " + m_StoreController.products.WithID(productIDGoogle).metadata.localizedPriceString;
            }


        }

        if (EmeraldButton != null) {
            if (emeraldCost < 0) {
                Debug.LogError("Error: Product " + productIDGoogle + "EmeraldCost < 0 AND EmeraldButton there");
            }
            if (emeraldCost == 0) {
                EmeraldButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Globals.Controller.Language.translateString("Free") + "!";
            } else {
                EmeraldButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "<sprite=\"Icon_Emerald\" name=\"Icon_Emerald\"> " + emeraldCost;
            }
        }
    }


    /// <summary>
    /// Executes a Buy
    /// </summary>
    public virtual void ExecuteBuyedItem() {

    }

    /// <summary>
    /// User gets the Information, that he can only redeem this Item once<br></br>
    /// Only for IAPItemToInventory!
    /// </summary>
    public void InformUserAlreadyRedeemed() {
        ItemDescriptionField.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "You can only redeem this Item once!";
        ItemDescriptionField.GetComponentInChildren<TMPro.TextMeshProUGUI>().color = new Color32(169, 44, 44, 255);
    }

    /// <summary>
    /// Returns if the IAP Item has a valid Emerald Button
    /// </summary>
    /// <returns></returns>
    public bool hasEmeraldButton() {
        return EmeraldButton != null && EmeraldButtonComponent != null;
    }

    /// <summary>
    /// Returns if the IAP Item has a valid Real Money Button
    /// </summary>
    /// <returns></returns>
    public bool hasRealMoneyBuyButton() {
        return BuyButton != null && BuyButtonComponent != null;
    }
}
