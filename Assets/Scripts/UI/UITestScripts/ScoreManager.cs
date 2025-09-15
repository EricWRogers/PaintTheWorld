using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class ScoreChangedEvent : UnityEvent<int> {}
[System.Serializable] public class MultiplierChangedEvent : UnityEvent<bool, float> {}

public class ScoreManager : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private int score = 0;
    [SerializeField] private bool multiplierActive = false;
    [SerializeField] private float multiplier = 1f;

    [Header("Events")]
    public ScoreChangedEvent onScoreChanged;
    public MultiplierChangedEvent onMultiplierChanged;

    public int CurrentScore => score;
    public float CurrentMultiplier => multiplierActive ? multiplier : 1f;
    public bool MultiplierActive => multiplierActive;

    private void Awake()
    {
        if (onScoreChanged == null) onScoreChanged = new ScoreChangedEvent();
        if (onMultiplierChanged == null) onMultiplierChanged = new MultiplierChangedEvent();
    }

    private void Start()
    {
        onScoreChanged.Invoke(score);
        onMultiplierChanged.Invoke(multiplierActive, CurrentMultiplier);
    }

    public void AddScore(int basePoints)
    {
        int final = Mathf.RoundToInt(basePoints * CurrentMultiplier);
        score += final;
        onScoreChanged.Invoke(score);
    }

    public void SetMultiplier(bool active, float value = 1f)
    {
        multiplierActive = active;
        multiplier = Mathf.Max(0.1f, value);
        onMultiplierChanged.Invoke(multiplierActive, CurrentMultiplier);
    }

    public void IncreaseMultiplier(float delta)
    {
        multiplierActive = true;
        multiplier = Mathf.Max(0.1f, multiplier + delta);
        onMultiplierChanged.Invoke(multiplierActive, CurrentMultiplier);
    }

    public void ResetMultiplier()
    {
        multiplierActive = false;
        multiplier = 1f;
        onMultiplierChanged.Invoke(multiplierActive, CurrentMultiplier);
    }
}
