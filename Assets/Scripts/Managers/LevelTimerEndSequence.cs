using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SuperPupSystems.Helper;
using Unity.Cinemachine;

public class LevelTimerEndSequence : MonoBehaviour
{
    [Header("References")]
    public Timer timerLogic;
    public Currency moneyLogic;

    [Header("UI")]
    public GameObject panelRoot;
    public CanvasGroup panelCanvasGroup;
    public TMP_Text GoalText;
    public string holdPrefix = "Time of posters held:\n";
    public string capturePrefix = "Number of posters painted\n";
    public TMP_Text finalMoneyText;
    public Button nextButton;
    public string moneyPrefix = "Final Money: ";
    public string buttonWinText = "Go to Shop";
    public string buttonLoseText = "Back to main menu";

    [Header("Scene Load")]
    public string nextSceneName = "ShopScene";

    [Header("Cinemachine")]
    public CinemachineCamera endCamera;
    public int endCameraPriority = 50;

    [Header("Camera Positioning")]
    public float distanceInFront = 3.0f;
    public float heightOffset = 1.4f;
    public float lookAtHeight = 1.2f;

    [Header("Sequence Timing")]
    public float cameraPanDuration = 0.75f;
    public float panelFadeDuration = 0.35f;

    private bool ended = false;
    private Transform player;

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(GoToNextScene);
        }

        if (timerLogic != null)
        {
            timerLogic.timeout.AddListener(OnTimerEnded);
        }

        if (endCamera != null)
        {
            endCamera.Priority = 0;
        }

        ended = false;
    }

    void OnDestroy()
    {
        if (timerLogic != null)
            timerLogic.timeout.RemoveListener(OnTimerEnded);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(GoToNextScene);
    }

    public void OnTimerEnded()
    {
        if (ended) return;
        ended = true;

        player = PlayerManager.instance != null && PlayerManager.instance.player != null
            ? PlayerManager.instance.player.transform
            : null;

        StartCoroutine(EndSequenceRoutine());
    }

    IEnumerator EndSequenceRoutine()
    {
       
        if (GameManager.instance != null && GameManager.instance.pauseMenu != null)
        {
            GameManager.instance.pauseMenu.SetActive(false);
        }
        if(GameManager.instance.currentGamemode == GameManager.gameModes.HoldPoints)
        {
            GoalText.text = holdPrefix + string.Format("{0:D2}:{1:D2}", TimeSpan.FromSeconds(GameManager.instance.timeHeld).TotalMinutes, TimeSpan.FromSeconds(GameManager.instance.timeHeld).TotalSeconds) + 
            "/" + string.Format("{0:D2}:{1:D2}", TimeSpan.FromSeconds(GameManager.instance.heldGoal).TotalMinutes, TimeSpan.FromSeconds(GameManager.instance.heldGoal).TotalSeconds);
        }
        else if(GameManager.instance.currentGamemode == GameManager.gameModes.CapturePoints)
        {
            GoalText.text = capturePrefix + GameManager.instance.amountCaptured.ToString() + "/" +GameManager.instance.captureAmountToClear;
        }

        if (GameManager.instance.goalComplete)
        {
            nextButton.GetComponentInChildren<TMP_Text>().text = buttonWinText;
        }
        else
        {
            nextButton.GetComponentInChildren<TMP_Text>().text = buttonLoseText;
        }
        
        if (finalMoneyText != null && moneyLogic != null)
        {
            finalMoneyText.text = moneyPrefix + moneyLogic.amount.ToString();
        }

       
        SetupEndCamera();

        float elapsed = 0f;
        while (elapsed < cameraPanDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        
        if (GameManager.instance != null)
        {
            GameManager.instance.PauseGame();
        }
        else
        {
            Time.timeScale = 0f;
        }

        // Show panel
        if (panelRoot != null)
            panelRoot.SetActive(true);

        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;

            float fade = 0f;
            while (fade < panelFadeDuration)
            {
                fade += Time.unscaledDeltaTime;
                panelCanvasGroup.alpha = Mathf.Clamp01(fade / panelFadeDuration);
                yield return null;
            }

            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }
    }

    void SetupEndCamera()
    {
        if (endCamera == null || player == null) return;

        Vector3 playerCenter = player.position + Vector3.up * lookAtHeight;

        Vector3 flatForward = player.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = Vector3.forward;
        flatForward.Normalize();

        Vector3 camPos = playerCenter + flatForward * distanceInFront;
        camPos.y += (heightOffset - lookAtHeight);

        endCamera.transform.position = camPos;
        endCamera.transform.rotation = Quaternion.LookRotation((playerCenter - camPos).normalized, Vector3.up);

        endCamera.Priority = endCameraPriority;
    }

    public void GoToNextScene()
{
    GameManager.instance.ResumeGame();
    if (panelCanvasGroup != null)
    {
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
    }

    if (panelRoot != null)
        panelRoot.SetActive(false);

    
    if (endCamera != null)
        endCamera.Priority = 0;

    ended = false;
    Time.timeScale = 1f;
    SceneManager.LoadScene(GameManager.instance.nextScene);
}
}