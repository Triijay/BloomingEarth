using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OverlayMenu : MonoBehaviour
{
    private Building building;
    
    public Button levelUpButton;
    public TMPro.TextMeshProUGUI level;
    public TMPro.TextMeshProUGUI buildingName;

    public UnityEvent onOpen;

    private int y_Offset = 0;
    private int x_Offset = 0;

    public void openOverlayMenu(Building hittedBuilding) {
        triggerOverlayMenu(hittedBuilding, true);
    }

    public void triggerOverlayMenu(Building hittedBuilding, bool forceOpen = false) {


        building = hittedBuilding;
        updateOverlayInfos();

        checkLevelUpButtonState(building, building.isLevelUppable());

        // get OverlayMenu in Position
        var rect = (RectTransform)this.transform;
        rect.anchoredPosition = getCalculatedMenuPosition();


        // Change visibility of the OverlayMenu
        this.gameObject.SetActive(!this.gameObject.activeSelf || forceOpen);

        onOpen.Invoke();
    }

    public void closeOverlayMenu() {
        // Change visibility of the OverlayMenu
        this.gameObject.SetActive(false);
        building = null;
    }

    public void updateOverlayInfos() {
        if(building != null) {
            if (level != null) {
                level.text = building.getLevel().ToString();
            }
            if (buildingName != null) {
                buildingName.text = Globals.Controller.Language.translateString("building_name_" + building.getName());
            }
        }
    }

    public void checkLevelUpButtonState(Building checkedBuilding, bool isActive) {
        if (building != null) {
            if (building == checkedBuilding) {
                if (levelUpButton != null) {
                    levelUpButton.interactable = isActive;
                }
            }
        }    
    }

    private Vector3 getCalculatedMenuPosition() {

        // Reset offset
        x_Offset = 0;
        y_Offset = 0;

        if (Input.touchCount >= 1) {

            // Check if Touch was left or right of screen
            if (Input.GetTouch(0).position.x >= Screen.currentResolution.width / 2) {
                // we are on the right side
                x_Offset += -(Screen.currentResolution.width / 5);
            } else {
                // we are on the left side
                x_Offset += Screen.currentResolution.width / 5;
            }

            return new Vector2(Input.GetTouch(0).position.x + x_Offset, Input.GetTouch(0).position.y + y_Offset);
        }
        else {
            return new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2);
        }
    }
    

    public void levelUpButton_Click() {
        building.triggerLevelUp();
    }

    public void infoButton_Click() {
        Globals.UICanvas.uiElements.InitGameObject.GetComponent<BuildingMenu>().openBuildingMenu(building);
    }

    public void itemButton_Click() {
        Globals.UICanvas.uiElements.InitGameObject.GetComponent<BuildingMenu>().openBuildingMenu(building, BuildingMenu.MenuTab.ItemTab);
    }

}
