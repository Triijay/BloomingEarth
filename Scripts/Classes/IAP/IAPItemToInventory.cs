using UnityEngine;
using UnityEngine.Purchasing;

public class IAPItemToInventory : IAPItem {

    public ItemTemplate buyableItem;

    public bool oneTimeRedeemable = false;

    // Performance Optimization
    private GameObject cloneItem;


    /// <summary>
    /// Gets the Translated Texts out of Google
    /// </summary>
    public override void translateShopItem(IStoreController m_StoreController) {

        base.translateShopItem(m_StoreController);

        // Item should be displayed, no texts
        //ItemTitleField.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "";
        ItemDescriptionField.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "";
        //ItemDescriptionField.transform.localPosition = new Vector3(ItemDescriptionField.transform.localPosition.x, 120, ItemDescriptionField.transform.localPosition.z);

        // Display Item
        cloneItem = Instantiate(Globals.UICanvas.uiElements.BuildingMenuUI_Inventory.inventoryItemTemplateSingle.gameObject, transform);
        buyableItem.fillItemTemplateWithInfos(cloneItem.transform);

        // Set Item Position to the Middle of the Slot
        cloneItem.transform.localPosition = new Vector2(0, 0);
        cloneItem.SetActive(true);

        // In Shop we dont want the Item to be Dragged
        cloneItem.GetComponent<DragHandler>().enabled = false;

    }

    /// <summary>
    /// Post-Inits the Shop Item by InternetConnection<br></br>
    /// Locks Buttons, Descriptions etc, when no Connection
    /// </summary>
    public override void reactToInternetConnection() {
        base.reactToInternetConnection();
        // Prevent no-internet Users to get oneTimeRedeemer-Items
        if (Application.internetReachability == NetworkReachability.NotReachable && oneTimeRedeemable) {
            gameObject.SetActive(false);
        }
    }


    public override void ExecuteBuyedItem() {
        Globals.Game.currentUser.inventory.createProperty(buyableItem);

        if (oneTimeRedeemable) {
            if (EmeraldButton != null) {
                EmeraldButtonComponent.interactable = false;
            }
            if (BuyButton != null) {
                BuyButtonComponent.interactable = false;
            }
        }
    }

}
