using UnityEngine;
using UnityEngine.UI;

public class Dot : MonoBehaviour
{
    [Header("Properties")]
    private Color dotColor;
    private int colorId;
    private bool isSelected = false;
    private int gridX;
    private int gridY;

    [Header("References")]
    private Image image;
    private RectTransform rectTransform;
    private Animator animator;

    void Awake()
    {
        // Get components
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        animator = GetComponent<Animator>();

        // Initial visual update
        UpdateVisual();
    }

    public void SetColor(Color newColor, int newColorId)
    {
        dotColor = newColor;
        colorId = newColorId;
        UpdateVisual();
    }

    public void SetGridPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    public int GetGridX()
    {
        return gridX;
    }

    public int GetGridY()
    {
        return gridY;
    }

    public Color GetColor()
    {
        return dotColor;
    }

    public int GetColorId()
    {
        return colorId;
    }

    public void Select()
    {
        if (!isSelected)
        {
            isSelected = true;
            UpdateVisual();

            // Play selection animation if available
            if (animator != null)
            {
                animator.SetBool("Selected", true);
            }
        }
    }

    public void Deselect()
    {
        if (isSelected)
        {
            isSelected = false;
            UpdateVisual();

            // Play deselection animation if available
            if (animator != null)
            {
                animator.SetBool("Selected", false);
            }
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public void PlayMatchAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Match");
        }
    }

    public void PlayMergeAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Merge");
        }
    }

    private void UpdateVisual()
    {
        if (image == null) image = GetComponent<Image>();

        // Set the dot color
        image.color = dotColor;

        // Visual feedback for selection
        if (isSelected)
        {
            // If not using an animator, scale up slightly
            if (animator == null)
            {
                transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            }

            // Add a glow effect (optional)
            image.material = new Material(Shader.Find("UI/Default"));
            image.material.SetFloat("_ColorMask", 15);
        }
        else
        {
            // Reset scale if not using animator
            if (animator == null)
            {
                transform.localScale = Vector3.one;
            }

            // Reset material
            image.material = null;
        }
    }

    // Called when dots collapse down
    public void AnimateMove(Vector2 targetPosition, float duration = 0.2f)
    {
        StartCoroutine(AnimateMoveCoroutine(targetPosition, duration));
    }

    private System.Collections.IEnumerator AnimateMoveCoroutine(Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Use easing function for smoother movement
            float smoothT = Mathf.SmoothStep(0, 1, t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, smoothT);

            yield return null;
        }

        // Ensure final position is exact
        rectTransform.anchoredPosition = targetPosition;
    }

    // Called when new dots appear
    // Called when new dots appear
    public void AnimateAppear()
    {
        StartCoroutine(AnimateAppearCoroutine());
    }

    private System.Collections.IEnumerator AnimateAppearCoroutine()
    {
        // Start with zero scale
        transform.localScale = Vector3.zero;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Use easing function for bounce effect (similar to easeOutBack)
            float smoothT = Mathf.Sin(t * Mathf.PI * 0.5f);
            float overshoot = 1f + 0.1f * Mathf.Sin(t * Mathf.PI);

            transform.localScale = Vector3.one * smoothT * overshoot;

            yield return null;
        }

        // Ensure final scale is exactly one
        transform.localScale = Vector3.one;
    }

    // For touch detection
    private void OnMouseDown()
    {
        // Optional direct handling of input
        // You might want to use this instead of raycasting from GameManager
        // if you prefer component-based input handling
    }
}