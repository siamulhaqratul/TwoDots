using UnityEngine;

[CreateAssetMenu(fileName = "DotColorPalette", menuName = "Two Dots/Color Palette", order = 1)]
public class DotColorPalette : ScriptableObject
{
    [System.Serializable]
    public class DotColorInfo
    {
        public string colorName;
        public Color color;
        [Tooltip("Optional sprite for dots with custom shapes per color")]
        public Sprite customSprite;
    }

    [Header("Game Colors")]
    [SerializeField] private DotColorInfo[] dotColors;

    // Get total number of colors
    public int ColorCount => dotColors.Length;

    // Get color by index
    public Color GetColor(int index)
    {
        if (index >= 0 && index < dotColors.Length)
            return dotColors[index].color;

        return Color.white; // Default fallback
    }

    // Get color info by index
    public DotColorInfo GetColorInfo(int index)
    {
        if (index >= 0 && index < dotColors.Length)
            return dotColors[index];

        return null;
    }

    // Get a random color index
    public int GetRandomColorIndex()
    {
        return Random.Range(0, dotColors.Length);
    }

    // Get the next color in sequence (for merging)
    public int GetNextColorIndex(int currentIndex)
    {
        return (currentIndex + 1) % dotColors.Length;
    }
}