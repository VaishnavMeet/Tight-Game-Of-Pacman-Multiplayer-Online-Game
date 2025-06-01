using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GridSpawnerPho : MonoBehaviourPunCallbacks
{
    public GameObject boxPrefab;
    public GameObject playerPrefab;
    public GameObject player2Prefab;
    public int rows = 9;
    public int columns = 9;
    public float boxSize = 0.7f;
    public float spacing = 0.1f;
    public float spawnDelay = 0.01f;

    [HideInInspector] public GameObject[,] boxGrid;
    [HideInInspector] public Vector3[,] boxPositions;

    public static GridSpawnerPho Instance;

    void Awake()
    {
        Instance = this;
        boxGrid = new GameObject[rows, columns];
        boxPositions = new Vector3[rows, columns];
    }

    void Start()
    {
        if (boxPrefab == null || playerPrefab == null)
        {
            Debug.LogError("Assign the boxPrefab and playerPrefab.");
            return;
        }

        
            StartCoroutine(SpawnBoxes());
        
    }

    IEnumerator SpawnBoxes()
    {
        float totalWidth = columns * (boxSize + spacing) - spacing;
        float totalHeight = rows * (boxSize + spacing) - spacing;

        Vector3 startPosition = transform.position - new Vector3(totalWidth / 2, 0, totalHeight / 2) + new Vector3(boxSize / 2, 0, boxSize / 2);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 pos = startPosition + new Vector3(col * (boxSize + spacing), 0, row * (boxSize + spacing));
                GameObject box = Instantiate(boxPrefab, pos, Quaternion.identity, transform);
                boxGrid[row, col] = box;
                boxPositions[row, col] = pos;

                yield return new WaitForSeconds(spawnDelay);
            }
        }

        // Only Master Client spawns players
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3 player1Pos = boxPositions[8, 4];
            GameObject player1 = PhotonNetwork.Instantiate(playerPrefab.name, player1Pos, Quaternion.identity);
            player1.GetComponent<PlayerMovementPho>().SetStartPosition(8, 4);
            player1.GetComponent<PlayerMovementPho>().myTurn = TurnManagerPho.PlayerTurn.Player1;

            
        }
        else
        {
            Vector3 player2Pos = boxPositions[0, 4];
            GameObject player2 = PhotonNetwork.Instantiate(player2Prefab.name, player2Pos, Quaternion.identity);
            player2.GetComponent<PlayerMovementPho>().SetStartPosition(0, 4);
            player2.GetComponent<PlayerMovementPho>().myTurn = TurnManagerPho.PlayerTurn.Player2;
        }
    }

    public void RestartGame()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
    }
}
