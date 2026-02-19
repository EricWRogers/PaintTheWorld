using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    }

    void Start()
    {
        player = PlayerManager.instance ? PlayerManager.instance.player.transform : null;
        if (promptUI) promptUI.SetActive(false);
    }

    void Update()
    {
       
        if (!panelRoot || !rowPrefab) return;

        if (!player) player = PlayerManager.instance ? PlayerManager.instance.player.transform : null;

        if (panelRoot.activeSelf)
        {
            // Backspace to exit
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

        KioskCameraController.I.EnterKiosk(cameraTarget, () =>
        {
            if (panelRoot) panelRoot.SetActive(true);
            if (navigator) navigator.active = true;

            BuildList();
            UpdateCurrencyLabel();
        });
    }

    public void Close()
    {
        if (navigator) navigator.active = false;
        if (panelRoot) panelRoot.SetActive(false);

        ClearRows();
        offeredSkillIndices.Clear();
        ShowToast("");

        
        KioskCameraController.I.ExitKiosk();
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
            return;
        }

       
        var candidates = new List<int>();
        for (int i = 0; i < pm.stats.skills.Count; i++)
        {
            var s = pm.stats.GetSkill(i);
            if (s == null) continue;

            int cost = s.CostForNextLevel();
            bool isMax = cost < 0;
            if (!isMax) candidates.Add(i);
        }

        if (candidates.Count == 0)
        {
            ShowToast("All stats are maxed.");
            navigator.SetRows(new List<SelectableRow>());
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
        if (cost < 0)
        {
            ShowToast("That stat is already maxed.");
            RefreshRows();
            return;
        }

        if (pm.wallet.amount < cost)
        {
            ShowToast("Not enough currency.");
            return;
        }

        
        bool success = pm.stats.TryUpgradeSkill(skillIndex, pm.wallet);
        if (!success)
        {
            ShowToast("Upgrade failed.");
            RefreshRows();
            return;
        }

        usedThisVisit = true;
        ShowToast($"Upgraded: {s.displayName}!");
        RefreshRows();
        DarkenAllRows(true);

       
        FindObjectOfType<ItemEffectsManager>()?.Reapply();
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