using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Grid Settings")]
    [LunaPlaygroundField("Grid Width", 1, "Grid")]
    [SerializeField] private int gridWidth = 6;

    [LunaPlaygroundField("Grid Height", 2, "Grid")]
    [SerializeField] private int gridHeight = 6;

    [LunaPlaygroundField("Dot Spacing", 3, "Grid")]
    [SerializeField] private float dotSpacing = 1.0f;

    [Header("Colors")]
    [LunaPlaygroundField("Red Dot Color", 1, "Colors")]
    [SerializeField] private Color redDotColor = new Color(1f, 0.2f, 0.2f);

    [LunaPlaygroundField("Blue Dot Color", 2, "Colors")]
    [SerializeField] private Color blueDotColor = new Color(0.2f, 0.6f, 1f);

    [LunaPlaygroundField("Green Dot Color", 3, "Colors")]
    [SerializeField] private Color greenDotColor = new Color(0.2f, 0.8f, 0.2f);

    [LunaPlaygroundField("Yellow Dot Color", 4, "Colors")]
    [SerializeField] private Color yellowDotColor = new Color(1f, 0.8f, 0.2f);

    [LunaPlaygroundField("Purple Dot Color", 5, "Colors")]
    [SerializeField] private Color purpleDotColor = new Color(0.8f, 0.4f, 1f);

    [LunaPlaygroundField("Number of Colors", 6, "Colors")]
    [Range(3, 5)]
    [SerializeField] private int activeColorCount = 5;

    [Header("Game Settings")]
    [LunaPlaygroundField("Points Per Dot", 1, "Gameplay")]
    [SerializeField] private int pointsPerDot = 10;

    [LunaPlaygroundField("Target Score", 2, "Gameplay")]
    [SerializeField] private int targetScore = 200;

    [LunaPlaygroundField("Game Duration (seconds)", 3, "Gameplay")]
    [SerializeField] private float gameDuration = 30f;

    [Header("References")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject endCardPanel;

    // Internal variables
    private Dot[,] dots;
    private List<Dot> selectedDots = new List<Dot>();
    private bool isGameActive = false;
    private int currentScore = 0;
    private float remainingTime;
    private LineRenderer connectionLine;
    private Color[] activeColors;
    private bool tutorialShown = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize components
        connectionLine = GetComponent<LineRenderer>();
        if (connectionLine == null)
        {
            connectionLine = gameObject.AddComponent<LineRenderer>();
            connectionLine.startWidth = 0.1f;
            connectionLine.endWidth = 0.1f;
            connectionLine.positionCount = 0;
            connectionLine.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Initialize game state
        remainingTime = gameDuration;
        dots = new Dot[gridWidth, gridHeight];

        // Set up color array
        SetupColors();
    }

    private void Start()
    {
        // Show tutorial first
        ShowTutorial();
    }

    private void Update()
    {
        if (isGameActive)
        {
            // Update timer
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0)
            {
                remainingTime = 0;
                EndGame();
            }

            // Update UI
            UpdateUI();

            // Handle input
            if (Input.GetMouseButtonUp(0))
            {
                HandleMouseUp();
            }
            else if (Input.GetMouseButton(0))
            {
                HandleMouseDrag();
            }

            // Update connection line
            UpdateConnectionLine();
        }
    }

    private void SetupColors()
    {
        // Create main color array
        Color[] allColors = new Color[]
        {
            redDotColor,
            blueDotColor,
            greenDotColor,
            yellowDotColor,
            purpleDotColor
        };

        // Limit to active color count
        activeColors = new Color[Mathf.Clamp(activeColorCount, 3, 5)];
        for (int i = 0; i < activeColors.Length; i++)
        {
            activeColors[i] = allColors[i];
        }
    }

    public void StartGame()
    {
        // Hide tutorial if showing
        if (tutorialPanel.activeSelf)
        {
            tutorialPanel.SetActive(false);
        }

        // Log tutorial complete event
#if UNITY_LUNA
        Luna.Unity.Analytics.LogEvent(Luna.Unity.Analytics.EventType.TutorialComplete);
#endif

        // Reset game state
        currentScore = 0;
        remainingTime = gameDuration;
        isGameActive = true;

        // Initialize grid
        CreateGrid();

        // Update UI
        UpdateUI();
    }

    private void EndGame()
    {
        isGameActive = false;

        // Log end card shown event
#if UNITY_LUNA
        Luna.Unity.Analytics.LogEvent(Luna.Unity.Analytics.EventType.EndCardShown);
#endif

        // Show end card
        ShowEndCard();
    }

    private void CreateGrid()
    {
        // Clear existing grid
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // Set up Grid Layout Group
        GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            // Set cell size based on container dimensions
            float cellSize = Mathf.Min(
                gridContainer.rect.width / gridWidth,
                gridContainer.rect.height / gridHeight
            ) - gridLayout.spacing.x;

            gridLayout.cellSize = new Vector2(cellSize, cellSize);
            gridLayout.constraintCount = gridWidth;
        }

        // Create dots
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // Just instantiate the dot - Grid Layout Group will position it
                GameObject dotObj = Instantiate(dotPrefab, gridContainer);

                // Initialize Dot component
                Dot dot = dotObj.GetComponent<Dot>();
                if (dot == null)
                {
                    dot = dotObj.AddComponent<Dot>();
                }

                // Assign a random color
                int colorIndex = Random.Range(0, activeColors.Length);
                dot.SetColor(activeColors[colorIndex], colorIndex);

                // Set grid position
                dot.SetGridPosition(x, y);

                // Store in grid array
                dots[x, y] = dot;
            }
        }

        // Force immediate layout update
        Canvas.ForceUpdateCanvases();
        gridLayout.enabled = false;
        gridLayout.enabled = true;
    }

    private void CreateDot(int x, int y, float startX, float startY)
    {
        Debug.Log($"Creating dot at grid position ({x}, {y})");
        // Instantiate a new dot
        GameObject dotObj = Instantiate(dotPrefab, gridContainer);

        if (dotObj == null)
        {
            Debug.LogError("Failed to instantiate dot prefab!");
            return;
        }

        Debug.Log("Dot instantiated successfully");

        // Position the dot
        RectTransform rectTransform = dotObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(
            startX + x * dotSpacing,
            startY + y * dotSpacing
        );

        // Set size
        rectTransform.sizeDelta = new Vector2(dotSpacing * 0.8f, dotSpacing * 0.8f);

        // Initialize Dot component
        Dot dot = dotObj.GetComponent<Dot>();
        if (dot == null)
        {
            dot = dotObj.AddComponent<Dot>();
        }

        // Assign a random color
        int colorIndex = Random.Range(0, activeColors.Length);
        dot.SetColor(activeColors[colorIndex], colorIndex);

        // Set grid position
        dot.SetGridPosition(x, y);

        // Store in grid array
        dots[x, y] = dot;
    }

    private void HandleMouseDrag()
    {
        // Convert mouse position to world space
        Vector2 mousePosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridContainer,
            mousePosition,
            null,
            out Vector2 localPoint
        );

        // Find closest dot
        Dot closestDot = FindClosestDot(localPoint);

        // If we found a valid dot
        if (closestDot != null)
        {
            // If it's the first dot selected
            if (selectedDots.Count == 0)
            {
                // Start a new selection
                selectedDots.Add(closestDot);
                closestDot.Select();
            }
            // If it's not already selected
            else if (!selectedDots.Contains(closestDot))
            {
                Dot lastDot = selectedDots[selectedDots.Count - 1];

                // Check if it's the same color
                if (closestDot.GetColorId() == lastDot.GetColorId())
                {
                    // Check if it's adjacent
                    if (AreDotsAdjacent(lastDot, closestDot))
                    {
                        // Add to selection
                        selectedDots.Add(closestDot);
                        closestDot.Select();
                    }
                }
            }
            // If it's the second-last dot (to allow backtracking)
            else if (selectedDots.Count > 1 && closestDot == selectedDots[selectedDots.Count - 2])
            {
                // Remove the last dot (backtracking)
                Dot lastDot = selectedDots[selectedDots.Count - 1];
                lastDot.Deselect();
                selectedDots.RemoveAt(selectedDots.Count - 1);
            }
        }
    }

    private void HandleMouseUp()
    {
        // If we have a valid selection (2+ dots)
        if (selectedDots.Count >= 2)
        {
            // Process the match
            ProcessMatch();

            // Log event
#if UNITY_LUNA
            Luna.Unity.Analytics.LogEvent("match_made", selectedDots.Count);
#endif
        }

        // Clear selection
        ClearSelection();
    }

    private void ProcessMatch()
    {
        // Calculate points
        int points = selectedDots.Count * pointsPerDot;
        currentScore += points;

        // Get the last dot's position
        Vector2 lastPosition = new Vector2(
            selectedDots[selectedDots.Count - 1].GetGridX(),
            selectedDots[selectedDots.Count - 1].GetGridY()
        );

        // Get the color for the merged dot
        int lastColorId = selectedDots[selectedDots.Count - 1].GetColorId();
        int newColorId = (lastColorId + 1) % activeColors.Length;

        // Remove matched dots
        foreach (Dot dot in selectedDots)
        {
            int x = dot.GetGridX();
            int y = dot.GetGridY();

            // Mark for destruction
            Destroy(dot.gameObject);
            dots[x, y] = null;
        }

        // Collapse dots and refill
        StartCoroutine(CollapseAndRefillGrid(lastPosition, newColorId));
    }

    private IEnumerator CollapseAndRefillGrid(Vector2 mergePosition, int mergeColorId)
    {
        // Small delay for visual effect
        yield return new WaitForSeconds(0.2f);

        // Collapse columns
        for (int x = 0; x < gridWidth; x++)
        {
            int emptySpaces = 0;

            // Bottom to top
            for (int y = 0; y < gridHeight; y++)
            {
                if (dots[x, y] == null)
                {
                    emptySpaces++;
                }
                else if (emptySpaces > 0)
                {
                    // Move dot down by emptySpaces
                    int newY = y - emptySpaces;
                    dots[x, newY] = dots[x, y];
                    dots[x, y] = null;

                    // Update dot's position
                    dots[x, newY].SetGridPosition(x, newY);

                    // Move visually
                    StartCoroutine(MoveDotToPosition(dots[x, newY], x, newY));
                }
            }
        }

        // Small delay
        yield return new WaitForSeconds(0.3f);

        // Create new merged dot at merge position
        int mergeX = Mathf.RoundToInt(mergePosition.x);
        int mergeY = Mathf.RoundToInt(mergePosition.y);

        // Ensure position is valid
        mergeX = Mathf.Clamp(mergeX, 0, gridWidth - 1);
        mergeY = Mathf.Clamp(mergeY, 0, gridHeight - 1);

        // If position is empty
        if (dots[mergeX, mergeY] == null)
        {
            // Create the merged dot
            float startX = -(gridWidth * dotSpacing) / 2 + dotSpacing / 2;
            float startY = -(gridHeight * dotSpacing) / 2 + dotSpacing / 2;
            CreateDot(mergeX, mergeY, startX, startY);

            // Set merged color
            dots[mergeX, mergeY].SetColor(activeColors[mergeColorId], mergeColorId);
        }

        // Refill the grid
        RefillGrid();

        // Check win condition
        CheckWinCondition();
    }

    private IEnumerator MoveDotToPosition(Dot dot, int x, int y)
    {
        RectTransform rectTransform = dot.GetComponent<RectTransform>();
        Vector2 startPos = rectTransform.anchoredPosition;

        float startX = -(gridWidth * dotSpacing) / 2 + dotSpacing / 2;
        float startY = -(gridHeight * dotSpacing) / 2 + dotSpacing / 2;
        Vector2 targetPos = new Vector2(startX + x * dotSpacing, startY + y * dotSpacing);

        float duration = 0.2f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
    }

    private void RefillGrid()
    {
        float startX = -(gridWidth * dotSpacing) / 2 + dotSpacing / 2;
        float startY = -(gridHeight * dotSpacing) / 2 + dotSpacing / 2;

        // Fill empty spaces
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (dots[x, y] == null)
                {
                    CreateDot(x, y, startX, startY);
                }
            }
        }
    }

    private void CheckWinCondition()
    {
        if (currentScore >= targetScore)
        {
            // Player reached target score
            EndGame();
        }
    }

    private Dot FindClosestDot(Vector2 position)
    {
        float gridWidth_px = gridContainer.rect.width;
        float gridHeight_px = gridContainer.rect.height;

        // Convert position to grid coordinates
        float x = (position.x + gridWidth_px / 2) / gridWidth_px * gridWidth;
        float y = (position.y + gridHeight_px / 2) / gridHeight_px * gridHeight;

        // Round to nearest grid position
        int gridX = Mathf.RoundToInt(x);
        int gridY = Mathf.RoundToInt(y);

        // Check bounds
        if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
        {
            // Check if dot exists at position
            return dots[gridX, gridY];
        }

        return null;
    }

    private bool AreDotsAdjacent(Dot dot1, Dot dot2)
    {
        int x1 = dot1.GetGridX();
        int y1 = dot1.GetGridY();
        int x2 = dot2.GetGridX();
        int y2 = dot2.GetGridY();

        // Check if dots are horizontally or vertically adjacent
        return (Mathf.Abs(x1 - x2) == 1 && y1 == y2) || (Mathf.Abs(y1 - y2) == 1 && x1 == x2);
    }

    private void UpdateConnectionLine()
    {
        if (selectedDots.Count > 0)
        {
            // Set the number of positions
            connectionLine.positionCount = selectedDots.Count;

            // Set color to match the selected dots
            connectionLine.startColor = selectedDots[0].GetColor();
            connectionLine.endColor = selectedDots[0].GetColor();

            // Set positions
            for (int i = 0; i < selectedDots.Count; i++)
            {
                // Get dot position
                Vector3 dotPosition = selectedDots[i].transform.position;
                dotPosition.z = -1; // Slightly in front of the dots
                connectionLine.SetPosition(i, dotPosition);
            }
        }
        else
        {
            // Clear line
            connectionLine.positionCount = 0;
        }
    }

    private void ClearSelection()
    {
        // Deselect all dots
        foreach (Dot dot in selectedDots)
        {
            dot.Deselect();
        }

        // Clear list
        selectedDots.Clear();

        // Clear line
        connectionLine.positionCount = 0;
    }

    private void UpdateUI()
    {
        // Update score text
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }

        // Update timer text
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(remainingTime);
            timerText.text = $"Time: {seconds}s";
        }
    }

    private void ShowTutorial()
    {
        // Show tutorial panel
        tutorialPanel.SetActive(true);

        // Game is not active during tutorial
        isGameActive = false;

        // Log event
#if UNITY_LUNA
        Luna.Unity.Analytics.LogEvent("tutorial_shown");
#endif
        StartGame();
    }

    private void ShowEndCard()
    {
        // Show end card panel
        endCardPanel.SetActive(true);

        // Game is not active during end card
        isGameActive = false;

        // Log event
#if UNITY_LUNA
        Luna.Unity.Analytics.LogEvent(Luna.Unity.Analytics.EventType.EndCardShown);
#endif
    }

    public void OnInstallButtonClicked()
    {
        // Trigger store redirect
#if UNITY_LUNA
        Luna.Unity.Playable.InstallFullGame();
#endif
    }

    // Helper methods for orientation support
    public void OnOrientationChanged(bool isLandscape)
    {
        // Recalculate grid size based on new orientation
        if (isGameActive)
        {
            StartCoroutine(RebuildGridDelayed());
        }
    }

    private IEnumerator RebuildGridDelayed()
    {
        // Allow time for layout to update
        yield return new WaitForEndOfFrame();

        // Rebuild the grid
        CreateGrid();
    }
}