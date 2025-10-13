using UnityEngine;

public class KillZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerManager.instance.player.GetComponent<PlayerRespawn>().RespawnPlayer();
        }
    }
}
