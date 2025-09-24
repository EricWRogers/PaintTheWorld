
using System.Collections.Generic;
using UnityEngine;

public static class EnemyFinder
{
    public static List<GameObject> FindClosest(Vector3 from, int max, LayerMask enemyLayer)
    {
        var list = new List<GameObject>();
        foreach (var c in Physics.OverlapSphere(from, 100f, enemyLayer))
        {
            var go = c.attachedRigidbody ? c.attachedRigidbody.gameObject : c.gameObject;
            if (go) list.Add(go);
        }
        list.Sort((a,b)=> Vector3.SqrMagnitude(a.transform.position - from)
                         .CompareTo(Vector3.SqrMagnitude(b.transform.position - from)));
        if (list.Count > max) list.RemoveRange(max, list.Count - max);
        return list;
    }
}
