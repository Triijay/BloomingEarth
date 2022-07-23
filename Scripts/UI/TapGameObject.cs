using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// TapGameObject is controlling the behaviour when GameObjects are tapped by the User
/// </summary>
public class TapGameObject : MonoBehaviour {
    // Used to lock the current touch, so it would not be executed more than 1 time
    protected bool lockedTouch = false;
    protected Vector2 pos;
    protected float timePressStarted;
    public float durationLongTouch = 1.0f;
    public int moveThresholdLongTouch = 100;


    // Performance Optimization
    private PointerEventData pointer;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    private RaycastHit raycastHit;
    private Ray touchRay;
    private Building hittedBuilding;
    private HybridBuilding coinRelatedBuilding;
    private BuildingMenu buildingMenuScript;
    private CameraControls MainCamera;
    float magnitude;

    Touch touchZero;
    int touchCount;


    /// <summary>
    /// When a Gameobject is touched, check if its a building and handle levelUp 
    /// OnGUI is called once per frame, of a GUI Input exists <br></br>
    /// </summary>
    void OnGUI() {

        // Check if UI was touched
        pointer = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        raycastResults.Clear();

        EventSystem.current.RaycastAll(pointer, raycastResults);
        if (raycastResults.Count > 0) {
            return;
        }

        touchCount = Input.touchCount;

        if (touchCount > 0) {
            touchZero = Input.GetTouch(0);

            if (touchCount == 1 && touchZero.phase == TouchPhase.Began) {
                pos = touchZero.position;
                lockedTouch = false;
                timePressStarted = Time.time;
            }

            /* LONG TOUCH - Function saved for fishing */

            //if (touchCount == 1 && is_close(pos, touchZero.position, moveThresholdLongTouch)) {
            //    if(Time.time - timePressStarted >= durationLongTouch && !lockedTouch) {
            //        // Lock the Touch
            //        lockedTouch = true;

            //        // Initiate a Ray from
            //        touchRay = Camera.main.ScreenPointToRay(touchZero.position);

            //        // if Raycast hitted something
            //        if (Physics.Raycast(touchRay, out raycastHit)) {

            //            // Get the Building Component
            //            hittedBuilding = raycastHit.collider.GetComponent<Building>();

            //            // Check if it has that Component
            //            if (hittedBuilding != null) {
                          
            //            }
            //        }
            //    }
            //}
        
            // Check if it was the initial Touch
            if (touchCount == 1 && touchZero.phase == TouchPhase.Ended && is_close(pos, touchZero.position, 10) && !lockedTouch) {

                // Lock the Touch
                lockedTouch = true;

                // Initiate a Ray from
                touchRay = Camera.main.ScreenPointToRay(touchZero.position);

                // if Raycast hitted something
                if (Physics.Raycast(touchRay, out raycastHit)) {

                    // Get the Building Component
                    hittedBuilding = raycastHit.collider.GetComponent<Building>();

                    // Play Sound if the desired Object should Play a Sound (needs Collider) 
                    if (raycastHit.collider.gameObject.GetComponent<TapToPlaySound>()) {
                        raycastHit.collider.gameObject.GetComponent<TapToPlaySound>().playSoundOnTab.Play();
                    }

                    // Check if it has that Component
                    if (hittedBuilding != null) {

                        // Prevent OverlayMenu when BM is open
                        if (!Globals.UICanvas.uiElements.BuildingMenu.activeSelf) {
                            Globals.UICanvas.uiElements.OverlayMenu.triggerOverlayMenu(hittedBuilding);
                        }
                        
                    } else {
                        // It was no Building
                        if (raycastHit.collider.gameObject.name == "TapCoin") {
                            coinRelatedBuilding = raycastHit.collider.gameObject.GetComponentInParent<HybridBuilding>();
                            if (coinRelatedBuilding != null) {
                                coinRelatedBuilding.handleCoinTap();
                            }
                        } else if (raycastHit.collider.gameObject.name == "DeerAchievement") {
                            // Debug.Log("Deer Achievement sent");
                            Globals.Controller.GPiOS.sentAchievement100Percent(GPGSIds.achievement_where_are_you_deer);
                        }

                        // Close overlayMenu
                        Globals.UICanvas.uiElements.OverlayMenu.closeOverlayMenu();

                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks if 2 vectors are within a magnitude of 10 of each other
    /// </summary>
    private bool is_close(Vector2 pos1, Vector2 pos2, int maxMagnitude) {
        magnitude = (pos1 - pos2).magnitude;

        return (magnitude <= maxMagnitude);
    }


}
