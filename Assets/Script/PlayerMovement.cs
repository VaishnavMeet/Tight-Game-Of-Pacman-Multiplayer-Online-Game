using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public TurnManager.PlayerTurn myTurn;
    private int currentRow;
    private int currentCol;
    private float moveSpeed = 5f;
    private bool isMoving = false;
    private bool isSelecting = false;
    private TurnManager.PlayerTurn lastKnownTurn;
    private List<Vector2Int> highlightedPositions = new List<Vector2Int>();
    private AudioSource audioSource;
    public AudioClip movementClip;
    public AudioClip tapClip;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lastKnownTurn = TurnManager.Instance.currentTurn;
    }
    void OnEnable()
    {
        TurnManager.Instance.OnTurnSwitched.AddListener(OnTurnChanged);
    }

    void OnDisable()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnSwitched.RemoveListener(OnTurnChanged);
    }

    void OnTurnChanged(TurnManager.PlayerTurn newTurn)
    {
        ResetHighlightedBoxes();
    }


    public void SetStartPosition(int row, int col)
    {
        currentRow = row;
        currentCol = col;
    }

    void Update()
    {
        if (lastKnownTurn != TurnManager.Instance.currentTurn)
        {
            lastKnownTurn = TurnManager.Instance.currentTurn;
            ResetHighlightedBoxes(); // Ensure old highlights are removed when turn changes
        }

        if (!TurnManager.Instance.IsMyTurn(myTurn)) return;

        if (isSelecting && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                foreach (Vector2Int pos in highlightedPositions)
                {
                    if (hit.transform == GridSpawner.Instance.boxGrid[pos.x, pos.y].transform)
                    {
                        MoveToBox(pos.x, pos.y);
                        break;
                    }
                }
            }
        }
    }

    void CheckForWin()
    {
        bool won = false;

        // Assuming Player1 starts at bottom (row 8) and wants to reach top (row 0)
        // Player2 starts at top (row 0) and wants to reach bottom (row 8)
        if (myTurn == TurnManager.PlayerTurn.Player1 && currentRow == 0)
        {
            won = true;
        }
        else if (myTurn == TurnManager.PlayerTurn.Player2 && currentRow == GridSpawner.Instance.rows - 1)
        {
            won = true;
        }

        if (won)
        {
            // Show winner message
            if (TurnManager.Instance.winText != null)
            {

                TurnManager.Instance.winText.gameObject.SetActive(true);
                TurnManager.Instance.winText.text = $"{myTurn} Wins!";
            }

          
            enabled = false;  // disables this PlayerMovement script

            // Optionally, you can add other game end logic here, like stopping turns, showing UI, etc.
        }
    }


    void OnMouseDown()
    {
        if (isMoving || isSelecting) return;
        if (!TurnManager.Instance.IsMyTurn(myTurn)) return;
        PlaySound(tapClip);
        ShowNeighborBoxes();
    }

    void ShowNeighborBoxes()
    {
        highlightedPositions.Clear();

        TryHighlight(currentRow - 1, currentCol); // Up
        TryHighlight(currentRow + 1, currentCol); // Down
        TryHighlight(currentRow, currentCol - 1); // Left
        TryHighlight(currentRow, currentCol + 1); // Right

        isSelecting = true;
    }

    void TryHighlight(int row, int col)
    {
        if (row >= 0 && row < GridSpawner.Instance.rows && col >= 0 && col < GridSpawner.Instance.columns)
        {
            bool blocked = ObstacleManager.Instance.IsBlocked(currentRow, currentCol, row, col);
            //Debug.Log($"Checking neighbor ({row},{col}) from ({currentRow},{currentCol}): blocked={blocked}");

            if (blocked)
            {
                return;
            }

            GameObject box = GridSpawner.Instance.boxGrid[row, col];
            if (box.transform.childCount > 1)
            {
                box.transform.GetChild(1).gameObject.SetActive(true);
            }
            highlightedPositions.Add(new Vector2Int(row, col));
        }
    }



    void ResetHighlightedBoxes()
    {
        foreach (Vector2Int pos in highlightedPositions)
        {
            GameObject box = GridSpawner.Instance.boxGrid[pos.x, pos.y];

            if (box.transform.childCount > 1)
            {
                box.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
        highlightedPositions.Clear();
        isSelecting = false;
    }


    void MoveToBox(int row, int col)
    {
        Vector3 targetPos = GridSpawner.Instance.boxPositions[row, col];
        currentRow = row;
        currentCol = col;
        StartCoroutine(MoveToPosition(targetPos));
    }

    IEnumerator MoveToPosition(Vector3 target)
    {
        PlaySound(movementClip);
        isMoving = true;
        ResetHighlightedBoxes();

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        CheckForWin();

        TurnManager.Instance.SwitchTurn();
        transform.position = target;
        isMoving = false;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }


}
