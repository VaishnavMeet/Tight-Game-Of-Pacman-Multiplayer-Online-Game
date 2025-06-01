using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObstacleManagerPho : MonoBehaviourPun
{
    public static ObstacleManagerPho Instance;

    private HashSet<(int, int, int, int)> blockedPaths = new HashSet<(int, int, int, int)>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Call this when a player places an obstacle. Only the local player calls this.
    /// </summary>
    public void RegisterObstacle(int row1, int col1, int row2, int col2)
    {
        photonView.RPC("RPC_RegisterObstacle", RpcTarget.AllBuffered, row1, col1, row2, col2);
    }

    /// <summary>
    /// Synchronizes the obstacle registration across the network.
    /// </summary>
    [PunRPC]
    void RPC_RegisterObstacle(int row1, int col1, int row2, int col2)
    {
        blockedPaths.Add((row1, col1, row2, col2));
        blockedPaths.Add((row2, col2, row1, col1));

        // Uncomment for debugging
        // Debug.Log($"[Photon] Registered obstacle between ({row1},{col1}) and ({row2},{col2})");
    }

    public bool IsBlocked(int fromRow, int fromCol, int toRow, int toCol)
    {
        return blockedPaths.Contains((fromRow, fromCol, toRow, toCol));
    }
}
