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
        Debug.Log("[LevelTimerEndSequence] Timer ended event received.");

        if (ended) return;
        ended = true;

        GameManager.instance.canPause = false;
        GameManager.instance.TurnOnCursor();

        player = PlayerManager.instance != null && PlayerManager.instance.player != null
            ? PlayerManager.instance.player.transform
            : null;

        StartCoroutine(EndSequenceRoutine());
    }

    IEnumerator EndSequenceRoutine()
    {
        Time.timeScale = 0f;

        if (GameManager.instance != null)
        {
            GameManager.instance.canPause = false;
            GameManager.instance.TurnOnCursor();

            if (GameManager.instance.pauseMenu != null)
                GameManager.instance.pauseMenu.SetActive(false);
        }

        if (GoalText != null && GameManager.instance != null)
        {
            if (GameManager.instance.currentGamemode == GameManager.gameModes.HoldPoints)
            {
                GoalText.text = holdPrefix + GameManager.instance.timeHeld + "/" + GameManager.instance.heldGoal;
            }
            else if (GameManager.instance.currentGamemode == GameManager.gameModes.CapturePoints)
            {
                GoalText.text = capturePrefix + GameManager.instance.amountCaptured + "/" + GameManager.instance.captureAmountToClear;
            }
        }

        if (nextButton != null && GameManager.instance != null)
        {
            TMP_Text buttonText = nextButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text = GameManager.instance.goalComplete ? buttonWinText : buttonLoseText;
            }
        }

        if (finalMoneyText != null && moneyLogic != null)
        {
            finalMoneyText.text = moneyPrefix + moneyLogic.amount;
        }

        SetupEndCamera();

        float elapsed = 0f;
        while (elapsed < cameraPanDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (panelRoot != null)
            panelRoot.SetActive(true);
        else
            Debug.LogWarning("[LevelTimerEndSequence] panelRoot is not assigned.");

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
        else
        {
            Debug.LogWarning("[LevelTimerEndSequence] panelCanvasGroup is not assigned.");
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
        Time.timeScale = 1f;

        if (GameManager.instance != null)
        {
            GameManager.instance.canPause = true;
            GameManager.instance.ResumeGame();

            if (GameManager.instance.goalComplete)
                SceneManager.LoadScene(GameManager.instance.nextScene);
            else
                SceneManager.LoadScene("Start Menu");
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}