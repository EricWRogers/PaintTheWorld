using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ObjectiveUI : MonoBehaviour
{
    public TMP_Text objectiveText;
    public string captureText = "Capture Posters";
    public string holdText = "Hold Posters";


    public RectTransform rectTransform;
    public float duration = 1.5f;

    public Vector2 targetPosition; // where it ends (top-left, etc.)
    public Vector3 targetScale = new Vector3(0.5f, 0.5f, 1f);

    private Vector2 startPos;
    public Vector3 startScale;

    void Update()
    {
        if(GameManager.instance.currentGamemode == GameManager.gameModes.HoldPoints)
        {
            objectiveText.text = holdText;
        }
        if(GameManager.instance.currentGamemode == GameManager.gameModes.CapturePoints)
        {
            objectiveText.text = captureText;
        }
    }


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        rectTransform.position = startPos;
        rectTransform.localScale = startScale;
        objectiveText.alignment = TextAlignmentOptions.Top;

        StartCoroutine(AnimateObjective());
    }




    void Start()
    {
        startPos = rectTransform.anchoredPosition + new Vector2(-rectTransform.position.x, rectTransform.position.y + 368);

        StartCoroutine(AnimateObjective());
    }

    IEnumerator AnimateObjective()
    {
        float time = 0;


        while (time < duration)
        {
            float t = time / duration;
            objectiveText.alignment = TextAlignmentOptions.TopRight;
            // smooth easing
            t = Mathf.SmoothStep(0, 1, t);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        rectTransform.localScale = targetScale;
    }
}

