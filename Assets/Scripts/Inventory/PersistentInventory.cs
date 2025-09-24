
using UnityEngine;

//also put this on player object or same object as inventory to keep inventory persistent across scenes

[DefaultExecutionOrder(-1000)]
public class PersistentInventory : MonoBehaviour
{
    private static PersistentInventory _instance;
    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

