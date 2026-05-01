using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using DG.Tweening;

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
    public Vector3 startScale = new Vector3(1.75f, 1.75f, 1f);

    void Update()
    {

    }
    void Awake()
    {
    
        startPos = rectTransform.anchoredPosition;
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
        if (scene.name == "TutorialLevel")
        {
            objectiveText.text = " ";
            return;
        }
        
            rectTransform.DOKill(); // stop any previous tweens

        // Set correct state immediately
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = startPos;
        rectTransform.localScale = startScale;
    
        objectiveText.alignment = TextAlignmentOptions.Center;
        objectiveText.text = GameManager.instance.currentGamemode == GameManager.gameModes.CapturePoints ? captureText : holdText;
    
        // Build sequence
        Sequence seq = DOTween.Sequence();
    
        Debug.Log("Start Position: " + startPos);
        seq.AppendInterval(0.01f); // wait 1 frame (~)

        // begining slam 
        seq.Append(
            rectTransform
                .DOScale(startScale, 1f)
                .From(startScale * 3f)
                .SetEase(Ease.InOutBack)
        );
    
        //move /shrink to side
        seq.AppendInterval(2f); // wait 1 second

        seq.AppendCallback(() => {
            objectiveText.alignment = TextAlignmentOptions.TopRight;
        });

        seq.Append(
            rectTransform
                .DOAnchorPos(targetPosition, duration)
                .SetEase(Ease.OutQuint)
        );
        

        seq.Join(
            rectTransform
                .DOScale(targetScale, duration)
        );


    }





    void Start()
    {
        //startPos = rectTransform.position;
        startPos = new Vector2(-741,368);

            
    
    }

}

