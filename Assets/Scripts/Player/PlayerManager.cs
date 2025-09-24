using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public GameObject player;
    public Stats playerStats;
    public List<ItemStack> playerItems = new();

    public void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void SavePlayer()
    {
        playerStats = player.GetComponent<Stats>();
        playerItems = player.GetComponent<Inventory>().items;
    }

    public void LoadPlayer()
    {
        player.GetComponent<Stats>().health = playerStats.health;
        player.GetComponent<Stats>().movementSpeed = playerStats.movementSpeed;
        player.GetComponent<Stats>().attackSpeed = playerStats.attackSpeed;
        player.GetComponent<Stats>().damage = playerStats.damage;
        player.GetComponent<Stats>().defense = playerStats.defense;
        player.GetComponent<Stats>().level = playerStats.level;
        
        player.GetComponent<Inventory>().items = playerItems;
    }
}
