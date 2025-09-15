using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntEvent : UnityEvent<int> {}

public class Currency : MonoBehaviour
{
    public int amount = 0;
    public IntEvent changed;

    private void Start()
    {
        if (changed == null) changed = new IntEvent();
        changed.Invoke(amount); // initialize UI
    }

    public void Add(int delta)
    {
        amount += delta;
        if (changed != null) changed.Invoke(amount);
    }

    public bool Spend(int cost)
    {
        if (amount < cost) return false;
        amount -= cost;
        if (changed != null) changed.Invoke(amount);
        return true;
    }
}
