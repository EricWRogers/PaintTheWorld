using UnityEngine;
using TMPro;

public class FloatingMultiplierUI : MonoBehaviour
{
    [Header("Refs")]
    public ScoreManager scoreManager;       // scoremanager connected to player or game object
    public Transform target;                // Use player's transform for now

    [Header("Positioning")]
    public Vector3 offset = new Vector3(0f, 2.0f, 0f);
    public float bobAmplitude = 0.12f;
    public float bobSpeed = 2.2f;

    [Header("Scaling vs Score")]
    [Tooltip("Base local scale when score = 0")]
    public float baseScale = 0.4f;
    [Tooltip("How much size increases per log10(score+1)")]
    public float growthPerLog = 0.35f;
    public float maxScale = 2.5f;
    public float scaleLerpSpeed = 6f;

    [Header("Visibility")]
    public bool showOnlyWhenActive = true;

    private TMP_Text tmp;
    private Camera cam;
    private float t;
    private float currentScale;

    private bool active;
    private float currentMultiplier;
    private int currentScore;

    private void Awake()
    {
        tmp = GetComponent<TMP_Text>();
        cam = Camera.main;
        currentScale = baseScale;
        SetVisible(false); // start hidden until events init
    }

    private void OnEnable()
    {
        if (!scoreManager)
            scoreManager = FindObjectOfType<ScoreManager>();

        if (scoreManager)
        {
            scoreManager.onScoreChanged.AddListener(OnScoreChanged);
            scoreManager.onMultiplierChanged.AddListener(OnMultiplierChanged);

            // initialize from manager
            OnScoreChanged(scoreManager.CurrentScore);
            OnMultiplierChanged(scoreManager.MultiplierActive, scoreManager.CurrentMultiplier);
        }
    }

    private void OnDisable()
    {
        if (scoreManager)
        {
            scoreManager.onScoreChanged.RemoveListener(OnScoreChanged);
            scoreManager.onMultiplierChanged.RemoveListener(OnMultiplierChanged);
        }
    }

    private void Update()
    {
        if (!target) return;

        // Follow with a gentle bob
        t += Time.deltaTime * bobSpeed;
        Vector3 bob = Vector3.up * Mathf.Sin(t) * bobAmplitude;
        transform.position = target.position + offset + bob;

        // billboard
        if (!cam) cam = Camera.main;
        if (cam)
        {
            Vector3 toCam = transform.position - cam.transform.position;
            toCam.y = 0f; // y-lockedd
            if (toCam.sqrMagnitude > 0.001f)
                transform.forward = toCam.normalized;
        }

        // Scale with score to smooth it
        float targetScale = baseScale + growthPerLog * Mathf.Log10(currentScore + 1f);
        targetScale = Mathf.Clamp(targetScale, baseScale, maxScale);
        currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * scaleLerpSpeed);
        transform.localScale = Vector3.one * currentScale;
    }

    private void OnScoreChanged(int newScore)
    {
        currentScore = Mathf.Max(0, newScore);
        RefreshText();
    }

    private void OnMultiplierChanged(bool isActive, float mult)
    {
        active = isActive;
        currentMultiplier = mult;
        RefreshText();
        if (showOnlyWhenActive)
            SetVisible(active);
        else
            SetVisible(true);
    }

    private void RefreshText()
    {
        if (!tmp) return;
        if (!active && showOnlyWhenActive)
        {
            tmp.text = "";
            return;
        }
        tmp.text = $"x{currentMultiplier:0.##}";
    }

    private void SetVisible(bool vis)
    {
        if (tmp) tmp.enabled = vis;
    }
}
