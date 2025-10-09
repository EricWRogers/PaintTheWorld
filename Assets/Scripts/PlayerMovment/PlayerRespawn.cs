using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform spawnPos;

    public void RespawnPlayer()
    {
        transform.position = spawnPos.position;
    }
}
