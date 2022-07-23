using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Positions all Items in a grid Layout
/// based on the width and height of the container (dependent on screen size)
/// </summary>
public class UI_Inventory : MonoBehaviour {

    // reference to the user inventory
    private Inventory inventory;
    public Transform inventoryItemContainer;
    public Transform inventoryItemTemplate;
    public Transform inventoryItemTemplateSingle;

    // Performance Optimzation
    private Building currentBuilding;
    private RectTransform itemSlotRect;
    private RectTransform containerRect;
    private ItemTemplate item;
    private RectTransform inventoryItemRectTransform;

    // bool compatible item
    bool itemCompatible = false;
    // start position
    float startPosition_x = 85;
    float startPosition_y = 85;
    // item counts
    int cnt_items_x = 0;
    int cnt_rows_y = 0;
    // item max count
    int max_items_x = 0;
    // cell size
    private float itemSlotCellSize;
    //padding
    public float padding;
    


    /// <summary>
    /// Set the inventory reference
    /// </summary>
    public void setInventory(Inventory inventory) {
        this.inventory = inventory;
    }

    /// <summary>
    /// Get the inventory reference
    /// </summary>
    public Inventory getInventory() {
        return inventory;
    }

    /// <summary>
    /// Refreshes the UI Layout on screen
    /// and orders the items in grid layout 
    /// based on the container size
    /// </summary>
    public void RefreshInventoryItems() {

        // get Current building from Building Menu Script

        //Globals.Game.initGame.GetComponent<BuildingMenu>().getCurrentBuilding();
        currentBuilding = Globals.UICanvas.uiElements.InitGameObject.GetComponent<BuildingMenu>().getCurrentBuilding();

        setInventory(Globals.Game.currentUser.inventory);

        // Clear all previous items
        foreach (Transform child in inventoryItemContainer) {
            if(child != inventoryItemTemplate) {
                Destroy(child.gameObject);
            }
        }

        // reset item counts and start position
        cnt_items_x = 0;
        cnt_rows_y = 0;

        // get the item size
        itemSlotRect = inventoryItemTemplate as RectTransform;
        startPosition_x = (int)itemSlotRect.rect.width / 2;
        startPosition_y = (int)itemSlotRect.rect.height / 2;
        itemSlotCellSize = (int)itemSlotRect.rect.width;
        // add padding right
        itemSlotCellSize += padding;


        // get the Container Size
        containerRect = inventoryItemContainer as RectTransform;
        float width = containerRect.rect.width;
        float height = containerRect.rect.height;

        // set the item counts for x and y 
        max_items_x = (int)(width / itemSlotCellSize);

        // center the items
        startPosition_x += (width - (itemSlotCellSize * max_items_x)) / 2;
        startPosition_y += padding*2;

        // position all items
        foreach (Property prop in inventory.getPropertyListOnlyUnused()) {

            // Get referenced ItemTemplate
            item = prop.ReferencedItem;

            // Instiate the itemTemplate
            inventoryItemRectTransform = Instantiate(inventoryItemTemplate, inventoryItemContainer).GetComponent<RectTransform>();
            // connect UI item with item
            inventoryItemRectTransform.gameObject.GetComponent<UI_Item>().setProperty(prop);
            // fill in the item infos
            item.fillItemTemplateWithInfos(inventoryItemRectTransform);
            // position the item
            inventoryItemRectTransform.anchoredPosition = new Vector2((cnt_items_x * itemSlotCellSize) + startPosition_x + padding/2, -(cnt_rows_y * itemSlotCellSize) - startPosition_y - padding / 2);
            cnt_items_x++;

            // Check if item is compatible and disable it if necessary
            itemCompatible = (item as BuildingUpgrade != false) && (item as BuildingUpgrade).isBuildingUpgradeCompatibleTo(currentBuilding);

            if (!itemCompatible) {
                // Deactivate item
                inventoryItemRectTransform.GetComponent<CanvasGroup>().alpha = 0.5f;
                inventoryItemRectTransform.GetComponent<Button>().interactable = false;
            }

            // if line is filled, start next row
            if (cnt_items_x >= max_items_x) {
                cnt_items_x = 0;
                cnt_rows_y++;
            }

            // set the cloned item active
            inventoryItemRectTransform.gameObject.SetActive(true);

        }

        // Resize the content height
        // If the items fit exactly in full rows, there should not be unnecessary space for the next row at the bottom
        if (max_items_x != 0 && (cnt_items_x % max_items_x) == 0) {
            height = (padding * 2) * 2 + (cnt_rows_y * itemSlotCellSize);
        } else {
            height = (padding * 2) * 2 + ((cnt_rows_y + 1) * itemSlotCellSize);
        }
        containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, height);

        // reset current building
        currentBuilding = null;
    }
}
