using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridSpawner : MonoBehaviour
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

    public static GridSpawner Instance;

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
                boxGrid[row, col] = Instantiate(boxPrefab, pos, Quaternion.identity, transform);
                boxPositions[row, col] = pos;

                yield return new WaitForSeconds(spawnDelay);
            }
        }

        GameObject player = Instantiate(playerPrefab, boxPositions[8, 4], Quaternion.identity);
        PlayerMovement pm1 = player.GetComponent<PlayerMovement>();
        pm1.myTurn = TurnManager.PlayerTurn.Player1;
        pm1.SetStartPosition(8, 4);

        GameObject player2 = Instantiate(player2Prefab, boxPositions[0, 4], Quaternion.identity);
        PlayerMovement pm2 = player2.GetComponent<PlayerMovement>();
        pm2.myTurn = TurnManager.PlayerTurn.Player2;
        pm2.SetStartPosition(0, 4);
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
