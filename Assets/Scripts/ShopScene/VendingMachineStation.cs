using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Cinemachine;

public class VendingMachineStation : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;

    [Tooltip("List container root (the screen that shows rows).")]
    public GameObject listRoot;

    [Tooltip("Result container root (the screen shown after upgrade).")]
    public GameObject resultRoot;

    public RectTransform contentParent;
    public VendingMachineRow rowPrefab;
    public KioskListNavigator navigator;

    public TMP_Text toastText;
    public TMP_Text currencyText;

    [Tooltip("Text shown on resultRoot after upgrade.")]
    public TMP_Text resultText;

    [Header("Camera")]
    public CinemachineCamera kioskCam;
    public Transform cameraTarget;

    [Header("Rules")]
    public bool usedThisVisit = false;
    public int optionsToShow = 3;

    [Header("Interact")]
    public float interactRadius = 2f;
    public GameObject promptUI;

    private PlayerManager pm;
    private Transform player;

    private readonly List<VendingMachineRow> liveRows = new();
    private readonly List<int> offeredSkillIndices = new();

    private string _lastResultMessage = "";

    void Awake()
    {
        if (panelRoot) panelRoot.SetActive(false);
        ShowToast("");
        ShowResult(false);
        if (promptUI) promptUI.SetActive(false);
    }

    void Start()
    {
        pm = PlayerManager.instance;
        player = pm && pm.player ? pm.player.transform : null;
    }

    void Update()
    {
        if (!pm) pm = PlayerManager.instance;
        if (!player && pm && pm.player) player = pm.player.transform;

        if (panelRoot && panelRoot.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
                Close();

            UpdateCurrencyLabel();
            return;
        }

        if (!player) return;

        bool close = Vector3.Distance(transform.position, player.position) <= interactRadius;
        if (promptUI) promptUI.SetActive(close);

        if (close && Input.GetKeyDown(KeyCode.E))
            Open();
    }

    public void Open()
    {
        pm = PlayerManager.instance;
        if (!pm || pm.stats == null)
        {
            ShowToast("PlayerManager missing Stats.");
            return;
        }

        if (!kioskCam || !cameraTarget)
        {
            ShowToast("Missing kioskCam or cameraTarget on vending machine.");
            return;
        }

        KioskCameraController.I.EnterKiosk(kioskCam, cameraTarget);

        if (panelRoot) panelRoot.SetActive(true);
        if (navigator) navigator.active = true;

        UpdateCurrencyLabel();

        if (usedThisVisit)
        {
            // Stay in result state if already used this visit
            ShowResult(true);
            if (resultText)
                resultText.text = string.IsNullOrEmpty(_lastResultMessage)
                    ? "Upgrade already used this visit."
                    : _lastResultMessage;

            if (navigator) navigator.SetRows(new List<SelectableRow>());
            return;
        }

        ShowResult(false);
        BuildList();
    }

    public void Close()
    {
        if (navigator) navigator.active = false;
        if (panelRoot) panelRoot.SetActive(false);

        ClearRows();
        offeredSkillIndices.Clear();
        ShowToast("");

        KioskCameraController.I.ExitKiosk();
        if (promptUI) promptUI.SetActive(false);
    }

    void BuildList()
    {
        ClearRows();
        offeredSkillIndices.Clear();
        ShowToast("");

        if (!pm || pm.stats == null) return;

        //pick any skill not maxed
        var candidates = new List<int>();
        for (int i = 0; i < pm.stats.skills.Count; i++)
        {
            var s = pm.stats.GetSkill(i);
            if (s == null) continue;
            if (s.level < s.maxLevel) candidates.Add(i);
        }

        if (candidates.Count == 0)
        {
            ShowToast("All stats are maxed.");
            if (navigator) navigator.SetRows(new List<SelectableRow>());
            return;
        }

        int n = Mathf.Min(optionsToShow, candidates.Count);
        for (int k = 0; k < n; k++)
        {
            int r = Random.Range(0, candidates.Count);
            offeredSkillIndices.Add(candidates[r]);
            candidates.RemoveAt(r);
        }

        var selectables = new List<SelectableRow>();
        for (int i = 0; i < offeredSkillIndices.Count; i++)
        {
            int skillIndex = offeredSkillIndices[i];
            var s = pm.stats.GetSkill(skillIndex);

            var row = Instantiate(rowPrefab, contentParent);
            liveRows.Add(row);

            
            row.Bind(s, skillIndex, pm.wallet);

            int localRowIndex = i;
            row.selectable.Bind(() => TryFreeUpgrade(localRowIndex), true);
            selectables.Add(row.selectable);
        }

        if (navigator) navigator.SetRows(selectables);
    }

    void TryFreeUpgrade(int rowIndex)
    {
        if (usedThisVisit) return;
        if (!pm || pm.stats == null) return;
        if (rowIndex < 0 || rowIndex >= offeredSkillIndices.Count) return;

        int skillIndex = offeredSkillIndices[rowIndex];
        var s = pm.stats.GetSkill(skillIndex);
        if (s == null) return;

        if (s.level >= s.maxLevel)
        {
            ShowToast("That stat is already maxed.");
            return;
        }

        // Free upgrade 
        s.currentMult += s.levelGrowth;
        s.level++;
        pm.stats.onSkillChanged?.Invoke(skillIndex, s);

        usedThisVisit = true;
        _lastResultMessage = $"Upgrade Used! Upgraded: {s.displayName} (Level {s.level}/{s.maxLevel})";
        ShowToast("");

        //swap to results
        ShowResult(true);
        if (resultText) resultText.text = _lastResultMessage;

        if (navigator) navigator.SetRows(new List<SelectableRow>());
        ClearRows();

        FindObjectOfType<ItemEffectsManager>()?.Reapply();
    }

    void ShowResult(bool on)
    {
        if (listRoot) listRoot.SetActive(!on);
        if (resultRoot) resultRoot.SetActive(on);
    }

    void ClearRows()
    {
        foreach (var r in liveRows)
            if (r) Destroy(r.gameObject);
        liveRows.Clear();
    }

    void ShowToast(string msg)
    {
        if (toastText) toastText.text = msg;
    }

    void UpdateCurrencyLabel()
    {
        
        if (!currencyText) return;
        var w = PlayerManager.instance ? PlayerManager.instance.wallet : null;
        currencyText.text = w ? $"$ {w.amount}" : "$ 0";
    }
}