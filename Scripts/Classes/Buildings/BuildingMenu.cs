using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuildingMenu : MonoBehaviour {
    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    private Building building;

    private CameraControls MainCamera;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public GameObject popUpWindow;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI buildingname;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI buildingnameTranslated;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI buildingdescription;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI level;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI enviAffect;
   
    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI income;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI coinRespawnTime;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI coinTapCash;

    /// <summary>
    /// The Layout Group for the enviAffect
    /// </summary>
    public Transform enviAffectLayoutGroup;

    /// <summary>
    /// The Layout Group for the income
    /// </summary>
    public Transform incomeLayoutGroup;

    /// <summary>
    /// The Layout Group for the coinRespawnTime
    /// </summary>
    public Transform coinRespawnTimeLayoutGroup;

    /// <summary>
    /// The Layout Group for the coinTapCash
    /// </summary>
    public Transform coinTapCashLayoutGroup;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI extension;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI buildingProgressInfo;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI LvlUpButtonSmallText;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI LvlUpButtonBigText;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI CoinDisplayLandscape;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI CoinDisplayPortrait;

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI ItemCurrentStats;

    /// <summary>
    /// Level Up Button
    /// </summary>
    public Button LvlUpButton;

    /// <summary>
    /// The Inventory-Holder for the Items
    /// </summary>
    public Transform inventoryItems;

    /// <summary>
    /// Upgrade-Slots
    /// </summary>
    public Transform upgradeSlot1;
    public Transform upgradeSlot2;
    public Transform upgradeSlot3;

    /// <summary>
    /// LockSymbol with text for the slot
    /// </summary>
    public GameObject UpgradeSlotUnlockLvl1stLock;
    public GameObject UpgradeSlotUnlockLvl2ndLock;
    public GameObject UpgradeSlotUnlockLvl3rdLock;


    // The Image tabs
    public Image InfoTab;
    public Image ItemTab;

    // The Layout Objects
    public GameObject InfoLayout;
    public GameObject ItemLayout;
    private GameObject cloneItem;

    // Colors
    private Color32 colorTabActive = new Color32(255, 255, 255, 255);
    private Color32 colorTabInactive = new Color32(175, 175, 175, 255);

    public enum MenuTab {
        InfoTab,
        ItemTab,
    }

    // UnityEvents
    public UnityEvent droppedItemIntoBuildingSlot;
    public UnityEvent clickedBuildingMenuTab;

    // Performance Optimization
    Dictionary<string, string> buildingInfos;

    /// <summary>
    /// Open the Building PopUp in Info Tab
    /// </summary>
    public void openBuildingMenu(Building assignBuilding) {
        if (!popUpWindow.activeSelf) {
            openBuildingMenu(assignBuilding, MenuTab.InfoTab);
        }
    }

    /// <summary>
    /// Open the Building PopUp in specific Tab
    /// </summary>
    public void openBuildingMenu(Building assignBuilding, MenuTab tab) {


        building = assignBuilding;

        setActiveTab(tab);

        // Hide UI
        UIElements.setHUDVisibility_BuildingMenu(false);

        // Get Building Name of referenced GameObject
        buildingname.text = building.getName();

        // Get Building Name-Textfield
        buildingnameTranslated.text = Globals.Controller.Language.translateString("building_name_" + building.getName());
        // Get Building Description
        buildingdescription.text = Globals.Controller.Language.translateString("buildingmenu_building_description_" + building.getName());

        if (popUpWindow != null) {
            // set the Panel Visibility to the opposite
            popUpWindow.SetActive(true);

            InvokeRepeating("updateBuildingMenu", 0, 0.2f);
        }

        Vibration.Vibrate(25);

        // Also move the camera to the building
        MainCamera = Globals.UICanvas.uiElements.MainCamera.GetComponent<CameraControls>();

        // Get in front of the building
        MainCamera.moveToBuilding(building, true);

        // Close overlayMenu
        Globals.UICanvas.uiElements.OverlayMenu.closeOverlayMenu();
    }


    /// <summary>
    /// Updates the BuildingMenu Information if BuildingMenu is open
    /// search for InvokeRepeating("UpdateBuildingMenu" ...
    /// </summary>
    public void updateBuildingMenu() {

        // Check what Option is selected on LevelUpAmount Slider
        if (Globals.UICanvas.uiElements.LevelUpAmountSlider.value == 1) {
            buildingInfos = building.getInfo(1);
        } else if (Globals.UICanvas.uiElements.LevelUpAmountSlider.value == 2) {
            buildingInfos = building.getInfo(5);
        } else {
            buildingInfos = building.getInfo(10);
        }

        level.text = buildingInfos["Level"];
        LvlUpButtonSmallText.text = "<sprite=\"Icon_Coins\" name=\"Icon_Coins\"> " + buildingInfos["UpgrCost"];

        if (buildingInfos.ContainsKey("Envi")) {
            enviAffect.text = buildingInfos["Envi"];
            enviAffectLayoutGroup.gameObject.SetActive(true);
        } else {
            enviAffect.text = "0";
            enviAffectLayoutGroup.gameObject.SetActive(false);
        }
        if (buildingInfos.ContainsKey("Income")) {
            income.text = "<sprite=\"Icon_Coins\" name=\"Icon_Coins\"> " + buildingInfos["Income"] + " / s";
            incomeLayoutGroup.gameObject.SetActive(true);
        } else {
            income.text = "0";
            incomeLayoutGroup.gameObject.SetActive(false);
        }
        if (buildingInfos.ContainsKey("CoinTapCash")) {
            coinTapCash.text = "<sprite=\"Icon_Coins\" name=\"Icon_Coins\"> " + buildingInfos["CoinTapCash"];
            coinTapCashLayoutGroup.gameObject.SetActive(true);
        } else {
            coinTapCash.text = "0";
            coinTapCashLayoutGroup.gameObject.SetActive(false);
        }
        if (buildingInfos.ContainsKey("CoinRespawnTime")) {
            coinRespawnTime.text = buildingInfos["CoinRespawnTime"] + " s";
            coinRespawnTimeLayoutGroup.gameObject.SetActive(true);
        } else {
            coinRespawnTime.text = "0";
            coinRespawnTimeLayoutGroup.gameObject.SetActive(false);
        }
        



        // BuildingProgress
        if (buildingInfos["LastBuildingLevelLock"] != "False") {
            // Building is in Construction Modus and can not be upgraded yet
            buildingProgressInfo.text = Globals.Controller.Language.translateString(
                "buildingmenu_progressinfo_need_to_upgrade",
                new string[] { buildingInfos["LastBuildingName"], buildingInfos["LastBuildingLevel"] }
                );
        } else if (building.getLevel() == 0 && building.getUpgradeCost(1) > Globals.Game.currentWorld.getCoins() && !building.getIsCheckingDelayUnlockProcess()) {
            // If Building Level 0 and not enough Money to upgrade
            buildingProgressInfo.text = Globals.Controller.Language.translateString(
                "buildingmenu_progressinfo_not_enough_money");
        } else if (building.getLevel() == 0 && building.getIsCheckingDelayUnlockProcess() ) {
            // DelayUnlock-Process is ongoing
            buildingProgressInfo.text = Globals.Controller.Language.translateString(
                "buildingmenu_progressinfo_time_until_unlocked",
                new string[] {" " + (DateTime.Parse(buildingInfos["level0to1TimestampUntilUnlock"]) - DateTime.Now).ToString(@"hh\:mm\:ss", new CultureInfo("en-US")) });
        } else if (building.getLevel() > 0 && building.getLevel() < int.Parse(buildingInfos["LevelsToUnlockNextBuilding"])) {
            // Building is above level 0, but is able to unlock next Building
            buildingProgressInfo.text = Globals.Controller.Language.translateString(
                "buildingmenu_progressinfo_upgrade_this",
                new string[] { buildingInfos["LevelsToUnlockNextBuilding"] }
                );
        } else {
            buildingProgressInfo.text = "";
        }

        // BENEFITS for LevelUp
        // If LevelUpAmount > 0 we can show what the Upgrades benefits are
        if (int.Parse(buildingInfos["LevelUpAmount"]) > 0) {
            // Show Textual Benefits of LevelUp
            level.text += "  <color=#AFC918>(+" + buildingInfos["LevelUpAmount"] + ")</color>";
            if (buildingInfos.ContainsKey("Envi") && buildingInfos["EnviNext"] != "0") {
                if (buildingInfos["EnviPlus"] == "") {
                    enviAffect.text += "  <color=#DD4400>(" + buildingInfos["EnviPlus"] + buildingInfos["EnviNext"] + ")</color>";
                } else {
                    enviAffect.text += "  <color=#AFC918>(" + buildingInfos["EnviPlus"] + buildingInfos["EnviNext"] + ")</color>";
                }
            }
            if (buildingInfos.ContainsKey("Income")) {
                income.text += "  <color=#AFC918>(+ <sprite=\"Icon_Coins\" name=\"Icon_Coins\"> " + buildingInfos["IncomeNext"] + " / s)</color>";
            }
            if (buildingInfos.ContainsKey("CoinTapCash")) {
                coinTapCash.text += "  <color=#AFC918>(+ <sprite=\"Icon_Coins\" name=\"Icon_Coins\"> " + buildingInfos["CoinTapCashNext"] + " / s)</color>";
            }
        }

        // Item Affection
        ItemCurrentStats.text = "";
        if (buildingInfos.ContainsKey("currentItemIncomeFactor")) {
            // Color it, when its good or bad
            if (float.Parse(buildingInfos["currentItemIncomeFactor"]) > 0) {
                ItemCurrentStats.text += "<color=#AFC918>" + Globals.Controller.Language.translateString("buildingmenu_currentItemIncomeFactor") + " +" + buildingInfos["currentItemIncomeFactor"] + "%</color>";
            } else if (float.Parse(buildingInfos["currentItemIncomeFactor"]) < 0) {
                ItemCurrentStats.text += "<color=#DD4400>" + Globals.Controller.Language.translateString("buildingmenu_currentItemIncomeFactor") + " " + buildingInfos["currentItemIncomeFactor"] + "%</color>";
            } else {
                ItemCurrentStats.text += Globals.Controller.Language.translateString("buildingmenu_currentItemIncomeFactor") + " " + buildingInfos["currentItemIncomeFactor"] + "%";
            }
        }
        ItemCurrentStats.text += "<br>";
        if (buildingInfos.ContainsKey("currentItemEnvironmentFactor")) {
            // Color it, when its good or bad
            if (float.Parse(buildingInfos["currentItemEnvironmentFactor"]) > 0) {
                ItemCurrentStats.text += "<color=#AFC918>" + Globals.Controller.Language.translateString("buildingmenu_currentItemEnvironmentFactor") + " +" + buildingInfos["currentItemEnvironmentFactor"] + "%</color>";
            } else if (float.Parse(buildingInfos["currentItemEnvironmentFactor"]) < 0) {
                ItemCurrentStats.text += "<color=#DD4400>" + Globals.Controller.Language.translateString("buildingmenu_currentItemEnvironmentFactor") + " " + buildingInfos["currentItemEnvironmentFactor"] + "%</color>";
            } else {
                ItemCurrentStats.text += Globals.Controller.Language.translateString("buildingmenu_currentItemEnvironmentFactor") + " " + buildingInfos["currentItemEnvironmentFactor"] + "%";
            }
        }
        // ItemSlots Locking
        if (UpgradeSlotUnlockLvl1stLock != null && building.getLevel() >= int.Parse(buildingInfos["upgradeSlotUnlockLvl1st"])) {
            UpgradeSlotUnlockLvl1stLock.SetActive(false);
        }
        if (UpgradeSlotUnlockLvl2ndLock != null && building.getLevel() >= int.Parse(buildingInfos["upgradeSlotUnlockLvl2nd"])) {
            UpgradeSlotUnlockLvl2ndLock.SetActive(false);
        }
        if (UpgradeSlotUnlockLvl1stLock != null && building.getLevel() >= int.Parse(buildingInfos["upgradeSlotUnlockLvl3rd"])) {
            UpgradeSlotUnlockLvl3rdLock.SetActive(false);
        }

        // LevelUpExtensions
        if (building.getLevel() > 0 && buildingInfos["nextLevelUpExtensionsLevel"] != "0") {
            extension.text = "<color=#FF4400>" + Globals.Controller.Language.translateString("buildingmenu_nextLevelUpExtensionsLevel", new string[] { buildingInfos["nextLevelUpExtensionsLevel"] }) + "</color>";
        } else {
            extension.text = "";
        }


        // DelayUnlockProcess
        if (Globals.Game.currentWorld.buildingInDelayUnlock == building) {
            // When in DelayUnlock show the Button
            Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.SetActive(true);
            // Make the Button Interactible, when it can be reduced
            Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.GetComponent<Button>().interactable = building.delayUnlockCanBeReduced() && Globals.Controller.Ads.isForwardBuildingProgressAdLoaded();
        } else {
            Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.SetActive(false);
        }
        
        if (Globals.Game.currentWorld.buildingInDelayUnlock == building && !building.delayUnlockCanBeReduced()) {
            // If this Building is in DelayUnlock-Process, but the waiting time cannot reduced anymore
            Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.transform.Find("WhyGrey").gameObject.SetActive(true);
        } else {
            Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.transform.Find("WhyGrey").gameObject.SetActive(false);
        }

        // if we cannot lvlUp this building, the Button shows that
        if (int.Parse(buildingInfos["LevelUpAmount"]) <= 0 || !bool.Parse(buildingInfos["UpgrCostHasEnoughMoney"]) ||
            buildingInfos["LastBuildingLevelLock"] != "False" || building.getIsCheckingDelayUnlockProcess()) {
            LvlUpButton.interactable = false;
        } else {
            LvlUpButton.interactable = true;
        }

        // Update CoinDisplays
        CoinDisplayLandscape.text = Globals.Game.currentWorld.getCoins().toRoundedString();
        CoinDisplayPortrait.text = Globals.Game.currentWorld.getCoins().toRoundedString();

        // If BuildingMenu was closed by User, cancel the Update-Repeating
        if (!popUpWindow.activeSelf) {
            CancelInvoke();
        }

    }
    
    public void closeBuildingMenu() {

        // Hide PopUp
        Globals.UICanvas.uiElements.BuildingMenu.SetActive(false);

        // Zoom Out
        Globals.UICanvas.uiElements.MainCamera.GetComponent<CameraControls>().moveOutOfBuilding();

        // Show UI
        UIElements.setHUDVisibility_BuildingMenu(true);
    }


    public Building getCurrentBuilding() {
        return building;
    }

    public void setActiveTab(MenuTab tab) {

        // Open desired Tab
        switch (tab) {
            case MenuTab.InfoTab:
                default:
                // Set active
                InfoTab.color = colorTabActive;
                InfoLayout.SetActive(true);

                // Set inactive
                ItemTab.color = colorTabInactive;
                ItemLayout.SetActive(false);
                break;

            case MenuTab.ItemTab:


                // Delete old Item clones
                foreach (Transform child in upgradeSlot1) {
                    Destroy(child.gameObject);
                }
                foreach (Transform child in upgradeSlot2) {
                    Destroy(child.gameObject);
                }
                foreach (Transform child in upgradeSlot3) {
                    Destroy(child.gameObject);
                }

                // Show Items in Building
                if (building.getUpgradeSlot(1) != null) {
                    displaySlotItem(building.getUpgradeSlot(1), upgradeSlot1);
                }
                if (building.getUpgradeSlot(2) != null) {
                    displaySlotItem(building.getUpgradeSlot(2), upgradeSlot2);
                }
                if (building.getUpgradeSlot(3) != null) {
                    displaySlotItem(building.getUpgradeSlot(3), upgradeSlot3);
                }

                // Set active
                ItemTab.color = colorTabActive;
                ItemLayout.SetActive(true);

                // Set inactive
                InfoTab.color = colorTabInactive;
                InfoLayout.SetActive(false);

                clickedBuildingMenuTab.Invoke();
                break;
        }
        // Start Update/Show Inventory
        StartCoroutine(updateInventoryItemLayout());
    }


    public void displaySlotItem(Property prop, Transform targetObject) {

        // Make new UI Item
        cloneItem = Instantiate(Globals.UICanvas.uiElements.BuildingMenuUI_Inventory.inventoryItemTemplateSingle.gameObject, targetObject);
        // connect UI item with item
        cloneItem.gameObject.GetComponent<UI_Item>().setProperty(prop);
        prop.ReferencedItem.fillItemTemplateWithInfos(cloneItem.transform);

        // Set Item Position to the Middle of the Slot
        cloneItem.transform.localPosition = new Vector2();
        cloneItem.SetActive(true);
    }

    public IEnumerator updateInventoryItemLayout() {
        // Wait a frame, so the flex width can be detect correctly
        yield return null;
        // Refresh inventory
        Globals.UICanvas.uiElements.BuildingMenuUI_Inventory.RefreshInventoryItems();
    }
}

