using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerMovementPho : MonoBehaviourPun
{
    public TurnManagerPho.PlayerTurn myTurn;

    private int currentRow;
    private int currentCol;
    private float moveSpeed = 5f;
    private bool isMoving = false;
    private bool isSelecting = false;
    private TurnManagerPho.PlayerTurn lastKnownTurn;

    private List<Vector2Int> highlightedPositions = new List<Vector2Int>();
    private AudioSource audioSource;

    public AudioClip movementClip;
    public AudioClip tapClip;

    void Start()
    {
        if (!photonView.IsMine)
        {
            return; // disable input for remote players
        }

        audioSource = GetComponent<AudioSource>();
        lastKnownTurn = TurnManagerPho.Instance.currentTurn;
    }

    void OnEnable()
    {
        if (photonView.IsMine)
        {
            TurnManagerPho.Instance.OnTurnSwitched.AddListener(OnTurnChanged);
        }
    }

    void OnDisable()
    {
        if (photonView.IsMine && TurnManagerPho.Instance != null)
        {
            TurnManagerPho.Instance.OnTurnSwitched.RemoveListener(OnTurnChanged);
        }
    }

    void OnTurnChanged(TurnManagerPho.PlayerTurn newTurn)
    {
        ResetHighlightedBoxes();
    }

    public void SetStartPosition(int row, int col)
    {
        currentRow = row;
        currentCol = col;
        transform.position = GridSpawnerPho.Instance.boxPositions[row, col];
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (lastKnownTurn != TurnManagerPho.Instance.currentTurn)
        {
            lastKnownTurn = TurnManagerPho.Instance.currentTurn;
            ResetHighlightedBoxes();
        }

        if (!TurnManagerPho.Instance.IsMyTurn(myTurn)) return;

        if (isSelecting && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                foreach (Vector2Int pos in highlightedPositions)
                {
                    if (hit.transform == GridSpawnerPho.Instance.boxGrid[pos.x, pos.y].transform)
                    {
                        photonView.RPC("MoveToBoxRPC", RpcTarget.AllBuffered, pos.x, pos.y);
                        break;
                    }
                }
            }
        }
    }

    void CheckForWin()
    {
        bool won = false;

        if (myTurn == TurnManagerPho.PlayerTurn.Player1 && currentRow == 0)
            won = true;
        else if (myTurn == TurnManagerPho.PlayerTurn.Player2 && currentRow == GridSpawnerPho.Instance.rows - 1)
            won = true;

        if (won && photonView.IsMine)
        {
            if (TurnManagerPho.Instance.winText != null)
            {
                TurnManagerPho.Instance.winText.gameObject.SetActive(true);
                TurnManagerPho.Instance.winText.text = $"{myTurn} Wins!";
            }

            enabled = false;
        }
    }

    void OnMouseDown()
    {
        if (!photonView.IsMine || isMoving || isSelecting) return;
        if (!TurnManagerPho.Instance.IsMyTurn(myTurn)) return;

        PlaySound(tapClip);
        ShowNeighborBoxes();
    }

    void ShowNeighborBoxes()
    {
        highlightedPositions.Clear();

        TryHighlight(currentRow - 1, currentCol);
        TryHighlight(currentRow + 1, currentCol);
        TryHighlight(currentRow, currentCol - 1);
        TryHighlight(currentRow, currentCol + 1);

        isSelecting = true;
    }

    void TryHighlight(int row, int col)
    {
        if (row >= 0 && row < GridSpawnerPho.Instance.rows && col >= 0 && col < GridSpawnerPho.Instance.columns)
        {
            bool blocked = ObstacleManagerPho.Instance.IsBlocked(currentRow, currentCol, row, col);
            if (blocked) return;

            GameObject box = GridSpawnerPho.Instance.boxGrid[row, col];
            if (box.transform.childCount > 1)
                box.transform.GetChild(1).gameObject.SetActive(true);

            highlightedPositions.Add(new Vector2Int(row, col));
        }
    }

    void ResetHighlightedBoxes()
    {
        foreach (Vector2Int pos in highlightedPositions)
        {
            GameObject box = GridSpawnerPho.Instance.boxGrid[pos.x, pos.y];
            if (box.transform.childCount > 1)
                box.transform.GetChild(1).gameObject.SetActive(false);
        }
        highlightedPositions.Clear();
        isSelecting = false;
    }

    [PunRPC]
    void MoveToBoxRPC(int row, int col)
    {
        Vector3 targetPos = GridSpawnerPho.Instance.boxPositions[row, col];
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

        transform.position = target;

        if (photonView.IsMine)
        {
            CheckForWin();
            TurnManagerPho.Instance.SwitchTurn();
        }

        isMoving = false;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
