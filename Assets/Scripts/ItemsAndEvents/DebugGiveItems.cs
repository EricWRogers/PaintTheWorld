// DebugGiveItems.cs
using UnityEngine;

public class DebugGiveItems : MonoBehaviour
{
    public Inventory inv;
    public Currency wallet;
    public ItemDefinition paintLauncher, reactiveArmor, explosivePaint, siphoningPaint, giftOfLife, fountain;

    private void Awake()
    {
        if (!inv) inv = FindObjectOfType<Inventory>();
        if (!wallet) wallet = FindObjectOfType<Currency>();
    }

    private void Update()
    {
        if (!inv) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && paintLauncher != null)    inv.Add(paintLauncher, 1);
        if (Input.GetKeyDown(KeyCode.Alpha2) && reactiveArmor != null)    inv.Add(reactiveArmor, 1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && explosivePaint != null)   inv.Add(explosivePaint, 1);
        if (Input.GetKeyDown(KeyCode.Alpha4) && siphoningPaint != null)   inv.Add(siphoningPaint, 1);
        if (Input.GetKeyDown(KeyCode.Alpha5) && giftOfLife != null)       inv.Add(giftOfLife, 1);
        if (Input.GetKeyDown(KeyCode.Alpha6) && fountain != null)         inv.Add(fountain, 1);

        if (Input.GetKeyDown(KeyCode.G) && wallet != null) wallet.Add(100);
    }
}

