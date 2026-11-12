using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro; // REQUIRED for updating TextMeshPro elements

public class playercontrolsys : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public Rigidbody2D playerRb;

    [Header("Physics Trackers")]
    [Tooltip("Drag your wide tilting Base object here.")]
    public Rigidbody2D baseRb;

    [Header("UI Settings")]
    public GameObject gameOverCanvas;
    [Tooltip("Drag your Intro Panel object into this box.")]
    public GameObject introPanel;
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI highScoreText;

    private float horizontalInput = 0f;
    private bool isFacingRight = true;
    private bool isIntroActive = true; // State tracker for the startup screen

    // Scoring Mechanics Variables
    private int currentScore = 0;
    private int highScore = 0;
    private bool scoredOnLeftLimit = false;

    void Start()
    {
        Time.timeScale = 1f;
        Application.targetFrameRate = 60;

        // Load the saved High Score data from PlayerPrefs memory
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateScoreVisuals();

        // Check if the Intro Panel is hooked up
        if (introPanel != null)
        {
            introPanel.SetActive(true);
            isIntroActive = true;
        }
        else
        {
            isIntroActive = false;
        }

        if (gameOverCanvas != null)
        {
            Button restartBtn = gameOverCanvas.GetComponentInChildren<Button>();
            if (restartBtn != null)
            {
                restartBtn.onClick.RemoveAllListeners();
                restartBtn.onClick.AddListener(RestartGame);
            }

            // Clear any raycast block parameters at start
            CanvasGroup canvasGroup = gameOverCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            gameOverCanvas.SetActive(false);
        }
    }

    void Update()
    {
        // 1. INTRO PANEL DISMISSAL TRIGGER
        if (isIntroActive)
        {
            // If the player presses any keyboard key or taps the mouse/touchscreen, dismiss the intro panel
            if ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                (Pointer.current != null && Pointer.current.press.wasPressedThisFrame))
            {
                DismissIntro();
            }
            return; // Skip normal gameplay updates until intro is cleared!
        }

        if (Time.timeScale == 0f) return;

        horizontalInput = 0f;

        // 2. Inputs (Keyboard)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) horizontalInput = -1f;
            else if (Keyboard.current.rKey.isPressed) horizontalInput = 1f;
        }

        // 3. Inputs (Pointer / Touch)
        if (Pointer.current != null && Pointer.current.press.isPressed)
        {
            Vector2 inputPosition = Pointer.current.position.ReadValue();
            if (inputPosition.x < Screen.width / 2f) horizontalInput = -1f;
            else horizontalInput = 1f;
        }

        if (horizontalInput > 0 && !isFacingRight) FlipCharacter();
        else if (horizontalInput < 0 && isFacingRight) FlipCharacter();

        // 4. Track Base Tilt to Award Points
        EvaluateBaseTiltPoints();
    }

    void FixedUpdate()
    {
        // Freeze rigidbodies during introduction card presentation so nothing tips over prematurely
        if (isIntroActive)
        {
            if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
            return;
        }

        if (playerRb != null)
        {
            if (horizontalInput != 0)
            {
                playerRb.linearVelocity = new Vector2(horizontalInput * moveSpeed * 2.5f, playerRb.linearVelocity.y);
            }
            else
            {
                // Momentum air-drift logic preservation
                if (Mathf.Abs(playerRb.linearVelocity.y) < 0.1f)
                {
                    float frictionBrake = Mathf.MoveTowards(playerRb.linearVelocity.x, 0f, moveSpeed * Time.fixedDeltaTime * 15f);
                    playerRb.linearVelocity = new Vector2(frictionBrake, playerRb.linearVelocity.y);
                }
            }
        }
    }

    void DismissIntro()
    {
        isIntroActive = false;
        if (introPanel != null)
        {
            introPanel.SetActive(false); // Turns off the presentation visuals completely
        }
    }

    void EvaluateBaseTiltPoints()
    {
        if (baseRb == null) return;

        float currentAngle = baseRb.transform.localEulerAngles.z;

        // TRUE 180 DEGREE FLIP DETECTION:
        if (currentAngle >= 170f && currentAngle <= 190f)
        {
            if (!scoredOnLeftLimit)
            {
                AddPoints(5);
                scoredOnLeftLimit = true;
            }
        }
        else if (currentAngle < 15f || currentAngle > 345f)
        {
            scoredOnLeftLimit = false;
        }
    }

    void AddPoints(int amount)
    {
        currentScore += amount;

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        UpdateScoreVisuals();
    }

    void UpdateScoreVisuals()
    {
        if (currentScoreText != null) currentScoreText.text = "SCORE: " + currentScore;
        if (highScoreText != null) highScoreText.text = "BEST: " + highScore;
    }

    void FlipCharacter()
    {
        isFacingRight = !isFacingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }

    public void TriggerGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);

            CanvasGroup canvasGroup = gameOverCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameOverCanvas.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            Button restartBtn = gameOverCanvas.GetComponentInChildren<Button>();
            if (restartBtn != null)
            {
                restartBtn.interactable = true;
            }
        }
        yield return new WaitForEndOfFrame();
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
