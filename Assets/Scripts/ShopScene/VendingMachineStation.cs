using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Cinemachine;

public class VendingMachineStation : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;
    public RectTransform contentParent;
    public VendingMachineRow rowPrefab;
    public KioskListNavigator navigator;
    public TMP_Text toastText;
    public TMP_Text currencyText;

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

    void Awake()
    {
        if (panelRoot) panelRoot.SetActive(false);
        ShowToast("");
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

        // If open, only handle kiosk inputs
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
        if (!pm || pm.stats == null || pm.wallet == null)
        {
            ShowToast("PlayerManager missing Stats/Wallet.");
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

        BuildList();
        UpdateCurrencyLabel();
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

        if (usedThisVisit)
        {
            ShowToast("Vending machine already used this visit.");
            navigator.SetRows(new List<SelectableRow>());
            DarkenAllRows(true);
            return;
        }

      
        var candidates = new List<int>();
        for (int i = 0; i < pm.stats.skills.Count; i++)
        {
            var s = pm.stats.GetSkill(i);
            if (s == null) continue;

            int cost = s.CostForNextLevel();
            if (cost >= 0) candidates.Add(i);
        }

        if (candidates.Count == 0)
        {
            ShowToast("All stats are maxed.");
            navigator.SetRows(new List<SelectableRow>());
            return;
        }

        // Pick random options
        int n = Mathf.Min(optionsToShow, candidates.Count);
        for (int k = 0; k < n; k++)
        {
            int r = Random.Range(0, candidates.Count);
            offeredSkillIndices.Add(candidates[r]);
            candidates.RemoveAt(r);
        }

        // Build UI rows
        var selectables = new List<SelectableRow>();
        for (int i = 0; i < offeredSkillIndices.Count; i++)
        {
            int skillIndex = offeredSkillIndices[i];
            var s = pm.stats.GetSkill(skillIndex);

            var row = Instantiate(rowPrefab, contentParent);
            liveRows.Add(row);

            row.Bind(s, skillIndex, pm.wallet);

            int localRowIndex = i;
            row.selectable.Bind(() => TryBuy(localRowIndex), true);
            selectables.Add(row.selectable);
        }

        navigator.SetRows(selectables);
    }

        void TryBuy(int rowIndex)
    {
        if (usedThisVisit) { ShowToast("Already used this visit."); DarkenAllRows(true); return; }
        if (!pm || pm.stats == null || pm.wallet == null) return;
        if (rowIndex < 0 || rowIndex >= offeredSkillIndices.Count) return;

        int skillIndex = offeredSkillIndices[rowIndex];
        var s = pm.stats.GetSkill(skillIndex);
        if (s == null) return;

        int cost = s.CostForNextLevel();
        if (cost < 0) { ShowToast("That stat is already maxed."); RefreshRows(); return; }

        if (pm.wallet.amount < cost) { ShowToast("Not enough currency."); return; }

        bool success = pm.stats.TryUpgradeSkill(skillIndex, pm.wallet);
        if (!success) { ShowToast("Upgrade failed."); RefreshRows(); return; }

        usedThisVisit = true;
        ShowToast($"Upgraded: {s.displayName}!");
        RefreshRows();
        DarkenAllRows(true);

        FindObjectOfType<ItemEffectsManager>()?.Reapply();

        // AUTO EXIT AFTER PURCHASE:
        Close();
    }

    void RefreshRows()
    {
        for (int i = 0; i < liveRows.Count; i++)
        {
            int skillIndex = offeredSkillIndices[i];
            var s = pm.stats.GetSkill(skillIndex);
            liveRows[i].Bind(s, skillIndex, pm.wallet);
        }
        UpdateCurrencyLabel();
    }

    void DarkenAllRows(bool darken)
    {
        foreach (var r in liveRows)
            if (r) r.SetDimmed(darken);
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