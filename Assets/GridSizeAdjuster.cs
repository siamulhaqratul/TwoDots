using UnityEngine;
using UnityEngine.UI;

public class GridSizeAdjuster : MonoBehaviour
{
    public GridLayoutGroup gridLayout;
    public int columns = 6;
    public int rows = 6;

    void Update()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float screenRatio = (float)Screen.width / Screen.height;

        if (screenRatio >= 1) // Landscape
        {
            // Make cells square based on height
            float cellSize = rect.rect.height / rows;
            gridLayout.cellSize = new Vector2(cellSize, cellSize);
        }
        else // Portrait
        {
            // Make cells square based on width
            float cellSize = rect.rect.width / columns;
            gridLayout.cellSize = new Vector2(cellSize, cellSize);
        }
    }
}