using UnityEngine;

public class PlayerInit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerManager.instance.player = gameObject;
        PlayerManager.instance.InitializeComponents();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
