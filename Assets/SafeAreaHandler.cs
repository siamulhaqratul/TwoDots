using UnityEngine;

public class SafeAreaHandler : MonoBehaviour
{
    private RectTransform rectTransform;

    void Awake()
    {
        // Check if RectTransform exists
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogWarning("SafeAreaHandler requires a RectTransform component. Adding one now.");
            gameObject.AddComponent<RectTransform>();
            rectTransform = GetComponent<RectTransform>();
        }

        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}