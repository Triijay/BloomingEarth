using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectAnchorpoints : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform contentPanel;

    public List<RectTransform> anchorPoints;

    private void SnapTo(RectTransform target) {
        Canvas.ForceUpdateCanvases();

        contentPanel.anchoredPosition = (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position) -
            (Vector2)scrollRect.transform.InverseTransformPoint(target.position) - new Vector2(0f, 50f);
    }

    public void SnapToFirstAnchorPoint() {
        SnapTo(anchorPoints[0]);
    }

    public void SnapToSecondAnchorPoint() {
        SnapTo(anchorPoints[1]);
    }

    public void SnapToThirdAnchorPoint() {
        SnapTo(anchorPoints[2]);
    }
}

