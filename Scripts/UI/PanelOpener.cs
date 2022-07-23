using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelOpener : MonoBehaviour
{
    public GameObject Panel;

    /// <summary>
    /// Opens the referenced PopUp
    /// </summary>
    public void openPanel() {

        // Close Overlay Menus
        Globals.UICanvas.uiElements.OverlayMenu.closeOverlayMenu();

        // PopUpOffline Coins blocks the other PopUps to open
        if (! (Globals.UICanvas.uiElements.PopUpOfflineCoins.activeSelf || Globals.UICanvas.uiElements.MiniGamePopUpFinished.activeSelf) ) {

            Globals.Controller.Sound.PlaySound("ButtonClick1");

            // check if Panel is visible or not
            bool isActive = Panel.activeSelf;

            if (Panel == Globals.UICanvas.uiElements.ShopPopUp) {
                Globals.Controller.IAP.checkButtonsShouldBeInteractible();
            }

            if (Panel == Globals.UICanvas.uiElements.PopUpEnviGlass) {
                Globals.Game.currentWorld.enviGlass.updateEnviPopUp(true);
            }

            if (Panel == Globals.UICanvas.uiElements.PopUpCoinsPerSecond) {
                Globals.UICanvas.uiElements.PopUps.GetComponent<CoinsPerSecond>().startUpdating();
            }

            // set the Panel Visibility to the opposite
            Panel.SetActive(!isActive);
            Globals.UICanvas.uiElements.PopUpBG.SetActive(!isActive);

            foreach (Transform child in Globals.UICanvas.uiElements.PopUps.transform) {
                // Close all other Panels
                if (Panel.transform != child) {
                    child.gameObject.SetActive(false);
                    Debug.Log(child.name + " autoclosed");
                }
            }       

        }

    }

    public void openPanelAndHideUI() {
        openPanel();
        UIElements.setHUDVisibility(false);
    }

    public void openPanelAndHideSideMenus() {
        openPanel();
        UIElements.setHUDVisibility_LeftAndRight(false);
    }

    /// <summary>
    /// Closes the Panel and the PopUpBG
    /// </summary>
    public void closePanel() {
        closePanel(false);
    }

    /// <summary>
    /// Closes the Panel and the PopUpBG
    /// </summary>
    public void closePanel(bool silent) {
        if(Panel != null) {
            // Fade out the Panel
            Panel.SetActive(false);
            Globals.UICanvas.uiElements.PopUpBG.SetActive(false);

            Globals.UICanvas.uiElements.PopUps.GetComponent<CoinsPerSecond>().stopUpdating();

            if (!silent) {
                Globals.Controller.Sound.PlaySound("ClosePanel");
            }

            // Activate HUD Buttons
            UIElements.setHUDVisibility(true);
        }
    }


    public void triggerPanel() {
        if (Panel != null) {
            if (Panel.activeSelf) {
                closePanel();
            } else {
                openPanel();
            }
        }
    }


    /// <summary>
    /// Close all open PopUps
    /// </summary>
    public static void closeAllPopUpsStatic() {
        
        // deactivate all PopUps
        foreach (Transform child in Globals.UICanvas.uiElements.PopUps.transform) {
        // Check if the active PopUp is not the OfflineCoin Menu
            if (child.gameObject.transform.name != Globals.UICanvas.uiElements.PopUpOfflineCoins.transform.name) {
                child.gameObject.SetActive(false);
            }
        }

        // Activate HUD Buttons
        UIElements.setHUDVisibility(true);

        Globals.UICanvas.uiElements.PopUps.GetComponent<CoinsPerSecond>().stopUpdating();

        Globals.UICanvas.uiElements.PopUpBG.SetActive(false);
    }

    /// <summary>
    /// Close all open PopUps<br></br>
    /// Hint: Used in Unity Inspector
    /// </summary>
    public void closeAllPopUps() {
        closeAllPopUpsStatic();
    }

    /// <summary>
    /// returns the active PopUp - if no PopUp is active, it returns an empty string
    /// </summary>
    /// <returns></returns>
    public string checkWhatPopUpIsActive() {
        foreach (Transform child in Globals.UICanvas.uiElements.PopUps.transform) {
            if (child.gameObject.activeSelf) {
                return child.name;
            }
        }
        return "";
    }
}
