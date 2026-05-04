using UnityEngine;

public class ControllerCheck : MonoBehaviour
{

    public bool controllerConnected;

    // Update is called once per frame
    void Update()
    {
        controllerConnected = PlayerManager.instance.ConnectedToController;
    }
}
