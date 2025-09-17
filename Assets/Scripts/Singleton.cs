using UnityEngine;

public class Singleton<T> : MonoBehaviour where T: MonoBehaviour{
    
    public static bool verbose = false;
    public static bool keepAlive = true;

    private static T m_instance = null;
    public static T instance {
        get { 
            if(m_instance == null){
                m_instance = GameObject.FindObjectOfType<T>();
                if(m_instance == null){
                    var singletonObj = new GameObject();
                    singletonObj.name = typeof(T).ToString();
                    m_instance = singletonObj.AddComponent<T>();
                }
            }
            return m_instance;
        }
    }

    static public bool isInstanceAlive{
        get { return m_instance != null; }
    }

    public virtual void Awake(){
        if (m_instance != null){
            if(verbose)
                Debug.Log("SingleAccessPoint, Destroy duplicate instance " + name + " of " + instance.name);
            Destroy(gameObject);
            return;
        }

        m_instance = GetComponent<T>();
        
        if(keepAlive){
            DontDestroyOnLoad(gameObject);
        }
        
        if (m_instance == null){
            if(verbose)
                Debug.LogError("SingleAccessPoint<" + typeof(T).Name + "> Instance null in Awake");
            return;
        }

        if(verbose)
            Debug.Log("SingleAccessPoint instance found " + instance.GetType().Name);

    }

}