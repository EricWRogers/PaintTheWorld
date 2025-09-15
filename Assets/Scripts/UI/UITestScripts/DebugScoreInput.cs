using UnityEngine;

public class DebugScoreInput : MonoBehaviour
{
    public ScoreManager score;

    public int addScoreAmount = 100;
    public float multiplierStep = 0.5f;
    public float defaultActivateValue = 2f;

    private void Awake()
    {
        if (!score) score = GetComponent<ScoreManager>();
    }

    private void Update()
    {
        if (!score) return;

        // Add score
        if (Input.GetKeyDown(KeyCode.N))
            score.AddScore(addScoreAmount);

        // Toggle multiplier on
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (score.MultiplierActive)
                score.ResetMultiplier();
            else
                score.SetMultiplier(true, defaultActivateValue); 
        }

        // Increase multiplier step 
        if (Input.GetKeyDown(KeyCode.B))
            score.IncreaseMultiplier(multiplierStep); 

        // reset
        if (Input.GetKeyDown(KeyCode.R))
            score.ResetMultiplier();
    }
}
