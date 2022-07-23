using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This Class plays very well with the Event Trigger Component.
/// To get 3D Button-Press Effekt, you have to:
/// - Add Button3D Class as Component
/// </summary>
public class Button3D : MonoBehaviour {

    private float translationDown;

    private List<Transform> childrenList = new List<Transform>();

    private Button buttonComponent;

    /// <summary>
    /// On Start the Class is preparing everything to get the Button into 3D
    /// </summary>
    void Start() {

        RectTransform rt = GetComponent<RectTransform>();
        translationDown = rt.rect.height / 14;

        foreach (Transform childTransform in gameObject.GetComponentsInChildren<Transform>(true)) {
            if (childTransform != this.transform) {
                childrenList.Add(childTransform);
            }
        }

        // Get Button Component
        try {
            buttonComponent = GetComponent<Button>();
        } catch { }

        // Add EventTrigger
        EventTrigger trigger = gameObject.AddComponent(typeof(EventTrigger)) as EventTrigger;

        // Add PointerDown Event
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((eventData) => { ButtonPress(); });
        trigger.triggers.Add(entry);

        // Add PointerUp Event
        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerUp;
        entry2.callback.AddListener((eventData) => { ButtonRelease(); });
        trigger.triggers.Add(entry2);
    }

    /// <summary>
    /// On ButtonPress all Elements of this Button have to go Down
    /// </summary>
    public void ButtonPress() {
        try {
            TranslateButtonChilds(-translationDown);
        }
        catch { }
    }

    /// <summary>
    /// On ButtonRelease all Elements of this Button have to go Up
    /// </summary>
    public void ButtonRelease() {
        TranslateButtonChilds(translationDown);
    }

    /// <summary>
    /// Translates all the Button-Childs Up or Down belong the Y Axis
    /// </summary>
    /// <param name="amount"></param>
    private void TranslateButtonChilds(float amount) {
        if (buttonComponent.interactable) {
            foreach (RectTransform child in childrenList) {
                child.anchoredPosition = new Vector2(child.anchoredPosition.x, child.anchoredPosition.y + amount);
            }
        }
    }

}
