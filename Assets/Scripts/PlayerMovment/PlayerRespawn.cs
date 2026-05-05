using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerRespawn : MonoBehaviour
{
    public Vector3 spawnPos;
    public void Awake()
    {
        spawnPos = transform.position;
    }
    public void RespawnPlayer()
    {
        transform.position = spawnPos;
    }
}
