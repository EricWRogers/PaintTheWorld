using UnityEngine;

public class EnemySpawning : ObjectSpawner
{
    new void Start()
    {
        base.Start();
        EnemyManager.instance.AddSpawners(this);
    }
    void Update()
    {

    }
}
