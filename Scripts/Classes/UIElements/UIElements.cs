using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIElements : MonoBehaviour {


    /// <summary>
    /// GameObject: Canvas
    /// </summary>
    public GameObject Canvas;

    /// <summary>
    /// GameObject: Sounds
    /// </summary>
    public GameObject Sounds;

    /// <summary>
    /// GameObject: Items
    /// </summary>
    public GameObject Items;

    /// <summary>
    /// GameObject: PopUps
    /// </summary>
    public GameObject PopUps;

    /// <summary>
    /// GameObject: mainCamera
    /// </summary>
    public GameObject MainCamera;

    /// <summary>
    /// GameObject: uiCamera
    /// </summary>
    public GameObject uiCamera;

    /// <summary>
    /// GameObject: the InitGame GameObject with the Init-Functions
    /// </summary>
    public GameObject InitGameObject;

    /// <summary>
    /// GameObject: the PopUpBG
    /// </summary>
    public GameObject PopUpBG;


    /// <summary>
    /// The slider for the Amount of LevelUps
    /// </summary>
    public Slider LevelUpAmountSlider;



    /// <summary>
    /// GameObject: PopUpOfflineCoins
    /// </summary>
    public GameObject PopUpOfflineCoins;

    /// <summary>
    /// GameObject: PopUpOfflineCoinsButtonOK
    /// </summary>
    public GameObject PopUpOfflineCoinsButtonOK;

    /// <summary>
    /// GameObject: PopUpOfflineCoinsAdButtonX2
    /// </summary>
    public GameObject PopUpOfflineCoinsAdButtonX2;


    /// <summary>
    /// GameObject: buildingMenu
    /// </summary>
    public GameObject BuildingMenu;

    /// <summary>
    /// GameObjectText: buildingName
    /// </summary>
    public TMPro.TextMeshProUGUI BuildingMenuBuildingName;

    /// <summary>
    /// GameObject: the BuildingMenuButtonLvlUp
    /// </summary>
    public GameObject BuildingMenuButtonLvlUp;

    /// <summary>
    /// GameObject: the BuildingMenuButtonMinus30
    /// </summary>
    public GameObject BuildingMenuAdButtonMinus30;

    /// <summary>
    /// UI_Inventory: the BuildingMenuUI_Inventory
    /// </summary>
    public UI_Inventory BuildingMenuUI_Inventory;

    /// <summary>
    /// OverlayMenu: the OverlayMenu
    /// </summary>
    public OverlayMenu OverlayMenu;



    /// <summary>
    /// GameObject: CoinsPerSecond
    /// </summary>
    public GameObject PopUpCoinsPerSecond;



    /// <summary>
    /// GameObject: the DebugLabelObject
    /// </summary>
    public GameObject DebugLabelObj;

    /// <summary>
    /// GameObjectText: the DebugLabel
    /// </summary>
    public TMPro.TextMeshProUGUI DebugLabelText;



    /// <summary>
    /// GameObject: the BoostBatteryStatus
    /// </summary>
    public GameObject BoostButtonHUD;

    /// <summary>
    /// GameObject: the SettingsMenu
    /// </summary>
    public GameObject SettingsMenu;

    /// <summary>
    /// GameObject: the SettingsMenu
    /// </summary>
    public GameObject SettingsMenuContent;


    /// <summary>
    /// GameObject: the SettingsMenu
    /// </summary>
    public GameObject LinkingConfirmationPopUp;



    /// <summary>
    /// GameObject: the PopUpEnviGlass
    /// </summary>
    public GameObject PopUpEnviGlass;

    /// <summary>
    /// GameObject: the PopUpEnviGlass
    /// </summary>
    public TMPro.TextMeshProUGUI PopUpEnviGlassStatus;

    /// <summary>
    /// GameObject: the PopUpEnviGlass
    /// </summary>
    public TMPro.TextMeshProUGUI PopUpEnviGlassStatusConsequences;

    /// <summary>
    /// GameObject: the PopUpEnviGlass
    /// </summary>
    public TMPro.TextMeshProUGUI PopUpEnviGlassTips;



    /// <summary>
    /// GameObject: the QuestPopUp
    /// </summary>
    public GameObject PopUpQuests;

    /// <summary>
    /// GameObject: the QuestFinishedPopUp
    /// </summary>
    public GameObject QuestInfoCircle;



    /// <summary>
    /// GameObject: the PopUpFeedback
    /// </summary>
    public GameObject PopUpFeedback;



    /// <summary>
    /// GameObject: the PrestigePopUp
    /// </summary>
    public GameObject PopUpPrestige;

    /// <summary>
    /// GameObject: the ButtonPrestige
    /// </summary>
    public GameObject ButtonPrestige;



    /// <summary>
    /// GameObject: the BoostPopUp
    /// </summary>
    public GameObject BoostPopUp;

    /// <summary>
    /// GameObject: the BoostBatteryPopUpFull
    /// </summary>
    public GameObject BoostPopUpBatteryStatus;

    /// <summary>
    /// GameObjectText: the BoostPopUpTextRemaining
    /// </summary>
    public TMPro.TextMeshProUGUI BoostPopUpTextRemaining;

    /// <summary>
    /// GameObjectText: the BoostPopUpWarningText
    /// </summary>
    public TMPro.TextMeshProUGUI BoostPopUpWarningText;

    /// <summary>
    /// GameObject: the BoostPopUpAdButtonInvokeBoost
    /// </summary>
    public GameObject BoostPopUpAdButtonInvokeBoost;

    /// <summary>
    /// GameObject: the ShopPopUp
    /// </summary>
    public GameObject ShopPopUp;

    /// <summary>
    /// GameObjectText: the EmeraldDisplay in Shop
    /// </summary>
    public TMPro.TextMeshProUGUI ShopEmeraldDisplay;



    /// <summary>
    /// GameObjectText: the HUD_PanelTopMenu
    /// </summary>
    public GameObject HUD_PanelTopMenu;

    /// <summary>
    /// GameObjectText: the HUD_PanelRightMenu
    /// </summary>
    public GameObject HUD_PanelRightMenu;

    /// <summary>
    /// GameObjectText: the HUD_PanelLeftMenu
    /// </summary>
    public GameObject HUD_PanelLeftMenu;

    /// <summary>
    /// GameObjectText: the HUD_PanelEnviGlass
    /// </summary>
    public GameObject HUD_PanelEnviGlass;

    /// <summary>
    /// GameObjectText: the HUD_PanelLevelUpAmountSlider
    /// </summary>
    public GameObject HUD_PanelLevelUpAmountSlider;


    /// <summary>
    /// GameObjectText: the MainCoinDisplay
    /// </summary>
    public TMPro.TextMeshProUGUI HUDCoinDisplay;

    /// <summary>
    /// GameObjectText: the EmeraldDisplay
    /// </summary>
    public TMPro.TextMeshProUGUI HUDEmeraldDisplay;



    /// <summary>
    /// GameObject: the CatchTheNuts-PopUp
    /// </summary>
    public GameObject MiniGamePopUp;

    /// <summary>
    /// GameObject: the MiniGamePopUpFinished-PopUp
    /// </summary>
    public GameObject MiniGamePopUpFinished;

    /// <summary>
    /// GameObject: the SaveGamePopUp-PopUp
    /// </summary>
    public GameObject SaveGamePopUp;

    /// <summary>
    /// GameObject: the SaveGamePopUp-PopUp Button local
    /// </summary>
    public GameObject SaveGameButtonLocal;

    /// <summary>
    /// GameObject: the SaveGamePopUp-PopUp Button cloud
    /// </summary>
    public GameObject SaveGameButtonCloud;

    /// <summary>
    /// GameObject: the EmeraldEffects
    /// </summary>
    public ParticleEffects ParticleEffects;

    /// <summary>
    /// GameObject: the PopupOverlayMenu
    /// </summary>
    public GameObject PopUpOverlayMenu;

    /// <summary>
    /// GameObject: the enviGlassContainer
    /// </summary>
    public GameObject enviGlassContainer;

    /// <summary>
    /// GameObject: the Element to Display envi value
    /// </summary>
    public GameObject enviNeedle;

    /// <summary>
    /// GameObject: the PopUpReconnectCloud
    /// </summary>
    public GameObject PopUpReconnectCloud;
    public GameObject PopUpReconnectCloudBtnConnectCloud;
    public GameObject PopUpReconnectCloudBtnNewGame;


    /// <summary>
    /// GameObject: CreativeCenter
    /// </summary>
    public GameObject CreativeCenter;



    public TMPro.TextMeshProUGUI SaveGameLocalLastClosed;
    public TMPro.TextMeshProUGUI SaveGameLocalScoreTotalLevels;
    public TMPro.TextMeshProUGUI SaveGameLocalItemCount;
    public TMPro.TextMeshProUGUI SaveGameLocalEmeralds;
    public TMPro.TextMeshProUGUI SaveGameCloudLastClosed;
    public TMPro.TextMeshProUGUI SaveGameCloudScoreTotalLevels;
    public TMPro.TextMeshProUGUI SaveGameCloudItemCount;
    public TMPro.TextMeshProUGUI SaveGameCloudEmeralds;



    public static void setHUDVisibility_BuildingMenu(bool visible) {
        Globals.UICanvas.uiElements.HUD_PanelTopMenu.SetActive(visible);
        Globals.UICanvas.uiElements.HUD_PanelRightMenu.SetActive(visible);
        Globals.UICanvas.uiElements.HUD_PanelLeftMenu.SetActive(visible);
        Globals.UICanvas.uiElements.HUD_PanelEnviGlass.SetActive(visible);
    }

    public static void setHUDVisibility(bool isVisible) {
        Globals.UICanvas.uiElements.HUD_PanelTopMenu.SetActive(isVisible);
        Globals.UICanvas.uiElements.HUD_PanelRightMenu.SetActive(isVisible);
        Globals.UICanvas.uiElements.HUD_PanelLeftMenu.SetActive(isVisible);
        Globals.UICanvas.uiElements.HUD_PanelEnviGlass.SetActive(isVisible);
        Globals.UICanvas.uiElements.HUD_PanelLevelUpAmountSlider.SetActive(isVisible);
    }

    public static void setHUDVisibility_LeftAndRight(bool isVisible) {
        Globals.UICanvas.uiElements.HUD_PanelRightMenu.SetActive(isVisible);
        Globals.UICanvas.uiElements.HUD_PanelLeftMenu.SetActive(isVisible);
        Globals.UICanvas.uiElements.HUD_PanelEnviGlass.SetActive(isVisible);
        Globals.UICanvas.uiElements.HUD_PanelLevelUpAmountSlider.SetActive(isVisible);
    }

}
