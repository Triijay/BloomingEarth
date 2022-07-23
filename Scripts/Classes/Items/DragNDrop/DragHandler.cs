using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IPointerExitHandler {

    public static GameObject itemBeingDragged;

    public static bool isCustomerDragged;

    [SerializeField] private Canvas canvas;
    public Transform customerScrollRect;
    public Transform dragParent;

    public float holdTime;
    public float maxScrollVelocityInDrag;

    private Transform startParent;

    private ScrollRect scrollRect;

    private float timer;

    private bool isHolding;
    private bool canDrag;
    private bool isPointerOverGameObject;

    private CanvasGroup canvasGroup;

    private Vector3 startPos;

    public Transform StartParent {
        get { return startParent; }
    }

    public Vector3 StartPos {
        get { return startPos; }
    }

    void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Use this for initialization
    void Start() {
        timer = holdTime;
    }

    // Update is called once per frame
    void Update() {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) {
            if (EventSystem.current.currentSelectedGameObject == gameObject) {
                Debug.Log("Pointer Down");
                scrollRect = customerScrollRect.GetComponent<ScrollRect>();
                isPointerOverGameObject = true;
                isHolding = true;
                StartCoroutine(Holding());
            }
        }

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended) {
            if (EventSystem.current.currentSelectedGameObject == gameObject) {
                Debug.Log("Pointer Up");
                isHolding = false;

                if (canDrag) {
                    itemBeingDragged = null;
                    isCustomerDragged = false;
                    if (transform.parent == dragParent) {
                        canvasGroup.blocksRaycasts = true;
                        canvasGroup.alpha = 1f;
                        transform.SetParent(startParent);
                        transform.localPosition = startPos;
                    }
                    scrollRect.vertical = true;
                    canDrag = false;
                    timer = holdTime;
                }
            }
        }

        if (Input.touchCount == 1) {
            if (EventSystem.current.currentSelectedGameObject == gameObject) {
                if (canDrag) {
                    Debug.Log("Item dragged");
                    RectTransform rect = transform as RectTransform;
                    //rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
                    rect.anchoredPosition += Input.GetTouch(0).deltaPosition / canvas.scaleFactor;
                }
                else {
                    if (!isPointerOverGameObject) {
                        isHolding = false;
                    }
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        isPointerOverGameObject = false;
    }

    IEnumerator Holding() {
        Debug.Log("Coroutine Holding : " + timer);
        while (timer > 0) {
            if (scrollRect.velocity.x >= maxScrollVelocityInDrag) {
                isHolding = false;
            }

            if (!isHolding) {
                timer = holdTime;
                yield break;
            }

            timer -= Time.deltaTime;
            Debug.Log("Time : " + timer);
            yield return null;
        }

        // Set dragged Item
        isCustomerDragged = true;
        itemBeingDragged = gameObject;
        // Save starting position and parent
        startPos = transform.localPosition;
        startParent = transform.parent;
        // Set vars for dragged item
        canDrag = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = .6f;
        transform.SetParent(dragParent);
        // disable scrollrect while dragging
        scrollRect.vertical = false;
    }

    public void Reset() {
        isHolding = false;
        canDrag = false;
        isPointerOverGameObject = false;
    }




    // Interface:  IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler

    //// Use this for initialization
    //[SerializeField] private Canvas canvas;

    //private RectTransform rect;
    //private CanvasGroup canvasGroup;
    //Vector3 origPosition = Vector3.zero;
    //public bool dropped = false;

    //float pointerDownTime = 0;
    //float dragDelay = 4f;
    //bool pointerDown;

    //private void Awake() {
    //    rect = GetComponent<RectTransform>();
    //    canvasGroup = GetComponent<CanvasGroup>();
    //}

    //public void OnBeginDrag(PointerEventData eventData) {
    //    origPosition = transform.localPosition;
    //    // So the Raycast goes through this object and can target the item slot
    //    canvasGroup.blocksRaycasts = false;

    //    canvasGroup.alpha = .6f;
    //}

    //public void OnDrag(PointerEventData eventData) {
    //    rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    //}

    //public void OnEndDrag(PointerEventData eventData) {
    //    Debug.Log("OnEndDrag");
    //    if (!dropped) {
    //        transform.localPosition = origPosition;
    //    } else {
    //        // Reset bool
    //        dropped = false;
    //    }
    //    // So the Raycast can now again hit the item
    //    canvasGroup.blocksRaycasts = true;
    //    canvasGroup.alpha = 1f;
    //}

    //public void OnPointerDown(PointerEventData eventData) {
    //    Debug.Log("OnPointerDown");
    //    pointerDownTime = 0;
    //    pointerDown = true;
    //}

    //public void OnPointerUp(PointerEventData eventData) {
    //    Debug.Log("OnPointerUp");
    //    pointerDown = false;
    //    pointerDownTime = 0;
    //    canvasGroup.interactable = false;
    //}

    //public void Update() {
    //    if (pointerDown) {
    //        pointerDownTime += Time.deltaTime;
    //        Debug.Log("PointerDownTime: " + pointerDownTime);

    //        if (pointerDownTime > dragDelay) {
    //            Debug.Log("PointerDownTime greater dragdelay ");
    //            // User wants to drag
    //            canvasGroup.interactable = true;
    //        }
    //    }
    //}

}