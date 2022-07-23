using UnityEngine;

/// <summary>
/// Class for Animating a Spinner Icon
/// </summary>
public class LoadingCircle : MonoBehaviour {
    public float rotateSpeed = 250f;
    private RectTransform rectComponent;

    private void Start() {
        rectComponent = GetComponent<RectTransform>();
    }

    private void Update() {
        rectComponent.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}