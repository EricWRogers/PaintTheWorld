using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T instance => _instance;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.Log($"[Singleton] Duplicate {typeof(T).Name} destroyed.");
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        DontDestroyOnLoad(gameObject);
    }
}
