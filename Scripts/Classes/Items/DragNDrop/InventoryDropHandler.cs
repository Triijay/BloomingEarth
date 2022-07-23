using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryDropHandler : DropHandler {

    public Transform itemContainer;
    public override void HandleDrop(GameObject droppedItem) {
        base.HandleDrop(droppedItem);

        // Set parent transform
        droppedItem.transform.SetParent(itemContainer);

        // Update Inventory
        Property connectedProperty = droppedItem.GetComponent<UI_Item>().getProperty();
        if (connectedProperty.ReferencedItem as BuildingUpgrade != false) {
            Debug.Log(connectedProperty);
            // Remove from building
            Globals.Game.initGame.GetComponent<BuildingMenu>().getCurrentBuilding().removeBuildingUpgrade(connectedProperty);
            // update Item positions
            connectedInventory.RefreshInventoryItems();
        }

    }
}

