using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager Instance;

    private HashSet<(int, int, int, int)> blockedPaths = new HashSet<(int, int, int, int)>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // prevent duplicates
            return;
        }
        Instance = this;
       // Debug.Log("ObstacleManager initialized");
    }

    public void RegisterObstacle(int row1, int col1, int row2, int col2)
    {
        blockedPaths.Add((row1, col1, row2, col2));
        blockedPaths.Add((row2, col2, row1, col1));
        //Debug.Log($"Registered obstacle between ({row1},{col1}) and ({row2},{col2})");
       // Debug.Log($"Registered obstacle between ({row2},{col2}) and ({row1},{col2})");
        foreach (var p in blockedPaths)
        {
            //Debug.Log($"Blocked path: {p}");
        }
    }


    public bool IsBlocked(int fromRow, int fromCol, int toRow, int toCol)
    {
        bool blocked = blockedPaths.Contains((fromRow, fromCol, toRow, toCol));
        //Debug.Log($"Checking blocked from ({fromRow},{fromCol}) to ({toRow},{toCol}): {blocked}");
        return blocked;
    }

}
