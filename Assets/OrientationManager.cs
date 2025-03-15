using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    public RectTransform gameBoard;
    public RectTransform UIContainer;

    private float screenRatio;

    void Update()
    {
        // Check for orientation changes
        screenRatio = (float)Screen.width / Screen.height;

        if (screenRatio >= 1)
        {
            // Landscape layout
            ApplyLandscapeLayout();
        }
        else
        {
            // Portrait layout
            ApplyPortraitLayout();
        }
    }

    void ApplyPortraitLayout()
    {
        // Example adjustments for portrait
        gameBoard.anchorMin = new Vector2(0, 0.1f);
        gameBoard.anchorMax = new Vector2(1, 0.9f);

        // Reposition UI elements
        UIContainer.anchorMin = new Vector2(0, 0);
        UIContainer.anchorMax = new Vector2(1, 0.1f);
    }

    void ApplyLandscapeLayout()
    {
        // Example adjustments for landscape
        gameBoard.anchorMin = new Vector2(0.1f, 0);
        gameBoard.anchorMax = new Vector2(0.9f, 1);

        // Reposition UI elements
        UIContainer.anchorMin = new Vector2(0.9f, 0);
        UIContainer.anchorMax = new Vector2(1, 1);
    }
}