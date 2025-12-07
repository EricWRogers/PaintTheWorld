using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemEffectsManager : MonoBehaviour
{
    Inventory inv;
    PlayerManager pm;

    void OnEnable()
    {
        StartCoroutine(InitAndWire());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (inv) inv.onChanged.RemoveListener(Reapply);
    }

    IEnumerator InitAndWire()
    {
        // Wait until PlayerManager and Player are ready
        while ((pm = PlayerManager.instance) == null || pm.player == null) yield return null;

        // Wire inventory change
        inv = pm.inventory;
        if (inv != null) inv.onChanged.AddListener(Reapply);

        
        yield return null; 
        Reapply();
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        
        StartCoroutine(InitAndWire());
    }

    public void Reapply()
    {
        pm = PlayerManager.instance;
        if (!pm || !pm.player) { Debug.Log("[ItemEffectsManager] Reapply skipped: no player."); return; }

        
        var scaler = pm.player.GetComponentInChildren<PlayerPaintWidthScaler>(true);
        if (scaler) { scaler.widthMultiplier = 1f; Debug.Log($"[ItemEffectsManager] Reset widthMultiplier on {scaler.gameObject.name}"); }



        // Re-apply every owned item’s OnEquipped
        if (pm.inventory == null || pm.inventory.items == null) return;

        var ctx = pm.GetContext();
        foreach (var s in pm.inventory.items)
        {
            if (s?.item == null || s.count <= 0) continue;
            try {
                s.item.OnEquipped(ctx, s.count);
                Debug.Log($"[ItemEffectsManager] Equipped {s.item.id} x{s.count}");
            } catch (System.Exception ex) {
                Debug.LogError($"[ItemEffectsManager] OnEquipped error for {s.item.name}: {ex}");
            }
        }

        
        if (scaler) Debug.Log($"[ItemEffectsManager] widthMultiplier now = {scaler.widthMultiplier}");
    }
}
