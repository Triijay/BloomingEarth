using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Important: never rename this Class - Item Tutorial will not work anymore
public class BuildingUpgradeSlotHandler : DropHandler
{
    public int slot;
    private Building building;
    public Transform upgradeHolder;

    public override void HandleDrop(GameObject droppedItem) {
        base.HandleDrop(droppedItem);

        // Update Inventory
        Property connectedProp = droppedItem.GetComponent<UI_Item>().getProperty();
        if (connectedProp.ReferencedItem as BuildingUpgrade != false) {

            building = Globals.Game.initGame.GetComponent<BuildingMenu>().getCurrentBuilding();

            // try adding Item
            if (building.addBuildingUpgrade(connectedProp, slot)) {

                // Item was added if addBuildingUpgrade returns true

                connectedInventory.RefreshInventoryItems();
                // Set position in the middle of the slot
                droppedItem.transform.SetParent(upgradeHolder);
                droppedItem.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position;

                Globals.Game.initGame.GetComponent<BuildingMenu>().droppedItemIntoBuildingSlot.Invoke();
            }

        }

    }
}
