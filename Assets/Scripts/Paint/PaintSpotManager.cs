using System.Collections.Generic;
using UnityEngine;

public class PaintSpotManager : MonoBehaviour
{
    public static PaintSpotManager instance;

    [Header("Tracked dirty spots")]
    public List<Vector3> dirtySpots = new List<Vector3>();

    [Header("Settings")]
    public float minDistanceBetweenSpots = 1.25f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterDirtySpot(Vector3 worldPos)
    {
        for (int i = 0; i < dirtySpots.Count; i++)
        {
            if (Vector3.Distance(dirtySpots[i], worldPos) < minDistanceBetweenSpots)
            {
                return;
            }
        }

        dirtySpots.Add(worldPos);
    }

    public bool TryGetNearestDirtySpot(Vector3 fromPos, out Vector3 result)
    {
        result = Vector3.zero;

        if (dirtySpots.Count == 0)
            return false;

        float bestDistance = Mathf.Infinity;
        int bestIndex = -1;

        for (int i = 0; i < dirtySpots.Count; i++)
        {
            float dist = Vector3.Distance(fromPos, dirtySpots[i]);

            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestIndex = i;
            }
        }

        if (bestIndex < 0)
            return false;

        result = dirtySpots[bestIndex];
        return true;
    }

    public void ClearDirtySpotsNear(Vector3 worldPos, float radius)
    {
        for (int i = dirtySpots.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(dirtySpots[i], worldPos) <= radius)
            {
                dirtySpots.RemoveAt(i);
            }
        }
    }
}