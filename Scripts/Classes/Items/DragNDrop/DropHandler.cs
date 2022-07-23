using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropHandler : MonoBehaviour {

    public UI_Inventory connectedInventory;

    void Update() {
        if (Input.touchCount == 1) {
            // Check if finger entered module
            Debug.Log(IsPointerOverUIObject());
            if (IsPointerOverUIObject()) {
                if (Input.GetTouch(0).phase == TouchPhase.Ended) {
                    if (DragHandler.itemBeingDragged) {
                        GameObject draggedItem = DragHandler.itemBeingDragged;
                        HandleDrop(draggedItem);
                    }
                }
            }
        }
    }

    private bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach(RaycastResult rs in results) {
            if(rs.gameObject == this.gameObject) {
                Debug.Log("Object over " + this.gameObject.name);
                return true;
            }
        }
        return false;
    }

    public virtual void HandleDrop(GameObject droppedItem) {
        DragHandler dragHandler = droppedItem.GetComponent<DragHandler>();
        dragHandler.GetComponent<CanvasGroup>().blocksRaycasts = true;
        dragHandler.GetComponent<CanvasGroup>().alpha = 1f;
        Debug.Log("Dropped");
    }
}
