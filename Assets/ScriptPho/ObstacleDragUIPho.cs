using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using static UnityEngine.Rendering.DebugUI.Table;

public class ObstacleDragUIPho : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameObject draggedObject;
    public GameObject obstacle3DPrefab;
    public Material highlightMaterial;
    private Material originalMaterial;
    private Renderer objectRenderer;
  
    bool isVertical = false;
    bool isPlaced = true;

    private Vector3? finalSnapPosition = null;
    private Quaternion finalSnapRotation = Quaternion.identity;

    public AudioClip obstacleBuiltClip;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var isPlayer1 = PhotonNetwork.IsMasterClient;
        var currentPlayer = isPlayer1 ? TurnManagerPho.PlayerTurn.Player1 : TurnManagerPho.PlayerTurn.Player2;
        isPlaced = false;
        if (!TurnManagerPho.Instance.IsMyTurn(currentPlayer)) return;
        if (CameraSwipeController.Instance != null)
            CameraSwipeController.Instance.canMove = false;

        draggedObject = Instantiate(obstacle3DPrefab, transform.position, Quaternion.identity);
        draggedObject.transform.localScale = new Vector3(1f, 0.8f, 0.2f);
        draggedObject.transform.position = Input.mousePosition;

        objectRenderer = draggedObject.GetComponentInChildren<Renderer>();
        if (objectRenderer != null && highlightMaterial != null)
        {
            originalMaterial = objectRenderer.material;
            objectRenderer.material = highlightMaterial;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedObject == null) return;
        isPlaced = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            draggedObject.transform.position = hit.point;
            TrySnapPreview(draggedObject.transform.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (CameraSwipeController.Instance != null)
            CameraSwipeController.Instance.canMove = true;

        if (!TurnManagerPho.Instance.CanPlaceObstacle())
        {
            Debug.Log("expand the value");
            Destroy(draggedObject);  // Clean up
            isPlaced = true;
            draggedObject = null;
            finalSnapPosition = null;
            return;
        }

       

        if (draggedObject == null) return;

        if (finalSnapPosition.HasValue)
        {
           
            draggedObject.transform.position = finalSnapPosition.Value;
            draggedObject.transform.rotation = finalSnapRotation;
            draggedObject.transform.localScale = new Vector3(1f, 0.8f, 0.2f);
            PhotonView photonView = PhotonView.Get(this);
            //photonView.RPC("TryPlaceObstacleNetwork", RpcTarget.AllBuffered, finalSnapPosition.Value);
            photonView.RPC("TryPlaceObstacleNetwork", RpcTarget.AllBuffered, finalSnapPosition.Value);

        }
        else
        {
            Destroy(draggedObject);
        }
        isPlaced = true;
        draggedObject = null;
        finalSnapPosition = null;
    }

    private void TrySnapPreview(Vector3 dragPos)
    {
        float snapThreshold = (GridSpawnerPho.Instance.boxSize + GridSpawnerPho.Instance.spacing) * 0.6f;

        for (int row = 0; row < GridSpawnerPho.Instance.rows; row++)
        {
            for (int col = 0; col < GridSpawnerPho.Instance.columns; col++)
            {
                // Horizontal
                if (col + 1 < GridSpawnerPho.Instance.columns)
                {
                    Vector3 p1 = GridSpawnerPho.Instance.boxPositions[row, col];
                    Vector3 p2 = GridSpawnerPho.Instance.boxPositions[row, col + 1];
                    Vector3 mid = (p1 + p2) / 2;

                    if (Vector3.Distance(dragPos, mid) < snapThreshold)
                    {
                        finalSnapPosition = new Vector3(mid.x, mid.y + 1, mid.z + 0.5f);
                        finalSnapRotation = Quaternion.identity;
                        isVertical = false;
                        draggedObject.transform.position = finalSnapPosition.Value;
                        draggedObject.transform.rotation = finalSnapRotation;
                        return;
                    }
                }

                // Vertical
                if (row + 1 < GridSpawnerPho.Instance.rows)
                {
                    Vector3 p1 = GridSpawnerPho.Instance.boxPositions[row, col];
                    Vector3 p2 = GridSpawnerPho.Instance.boxPositions[row + 1, col];
                    Vector3 mid = (p1 + p2) / 2;

                    if (Vector3.Distance(dragPos, mid) < snapThreshold)
                    {
                        finalSnapPosition = new Vector3(mid.x + 0.5f, mid.y + 1, mid.z);
                        finalSnapRotation = Quaternion.Euler(0, 90, 0);
                        isVertical = true;
                        draggedObject.transform.position = finalSnapPosition.Value;
                        draggedObject.transform.rotation = finalSnapRotation;
                        return;
                    }
                }
            }
        }
    }

    

    [PunRPC]
    private void TryPlaceObstacleNetwork(Vector3 dropPos)
    {
        if (!TurnManagerPho.Instance.CanPlaceObstacle()) { return; }

        float snapThreshold = (GridSpawnerPho.Instance.boxSize + GridSpawnerPho.Instance.spacing) * 0.6f;

        for (int row = 0; row < GridSpawnerPho.Instance.rows; row++)
        {
            for (int col = 0; col < GridSpawnerPho.Instance.columns; col++)
            {
                if (col + 1 < GridSpawnerPho.Instance.columns && !isVertical && !isPlaced)
                {
                    Vector3 mid = (GridSpawnerPho.Instance.boxPositions[row, col] + GridSpawnerPho.Instance.boxPositions[row, col + 1]) / 2;
                    mid = new Vector3(mid.x, mid.y + 1, mid.z + 0.5f);
                    Debug.Log("Horizontal");
                    if (Vector3.Distance(dropPos, mid) < snapThreshold)
                    {
                        if (ObstacleManagerPho.Instance.IsBlocked(row + 1, col + 1, row, col + 1) || ObstacleManagerPho.Instance.IsBlocked(row + 1, col, row, col)) { }
                            

                        PhotonView photonView = PhotonView.Get(this);
                        Vector3 spawnPos = dropPos;
                        Quaternion spawnRot = finalSnapRotation;

                        photonView.RPC("PlaceObstacleRPC", RpcTarget.AllBuffered, row, col, isVertical, dropPos, finalSnapRotation,isPlaced);
                        draggedObject = null;
                        finalSnapPosition = null;
                        isPlaced = true;

                    }
                }

                if (row + 1 < GridSpawnerPho.Instance.rows && isVertical && !isPlaced)
                {
                    Vector3 mid = (GridSpawnerPho.Instance.boxPositions[row, col] + GridSpawnerPho.Instance.boxPositions[row + 1, col]) / 2;
                    mid = new Vector3(mid.x + 0.5f, mid.y + 1, mid.z);
                    Debug.Log("Vertical");
                    if (Vector3.Distance(dropPos, mid) < snapThreshold)
                    {
                        if (ObstacleManagerPho.Instance.IsBlocked(row, col, row, col + 1) || ObstacleManagerPho.Instance.IsBlocked(row + 1, col, row + 1, col + 1))
                        { }

                        PhotonView photonView = PhotonView.Get(this);
                        Vector3 spawnPos = dropPos;
                        Quaternion spawnRot = finalSnapRotation;

                        photonView.RPC("PlaceObstacleRPC", RpcTarget.AllBuffered, row, col, isVertical, dropPos, finalSnapRotation,isPlaced);
                        isPlaced = true;
                        draggedObject = null;
                        finalSnapPosition = null;
                    }
                }
            }
        }

    }

    [PunRPC]
    public void PlaceObstacleRPC(int row, int col, bool vertical, Vector3 position, Quaternion rotation,bool IsPlaced)
    {
        GameObject obstacle = Instantiate(obstacle3DPrefab, position, rotation);
        obstacle.transform.localScale = new Vector3(1f, 0.8f, 0.2f);

        audioSource.PlayOneShot(obstacleBuiltClip);
        TurnManagerPho.Instance.ObstaclePlaced();

        if (!vertical && !IsPlaced)
        {
            ObstacleManagerPho.Instance.RegisterObstacle(row + 1, col + 1, row, col + 1);
            ObstacleManagerPho.Instance.RegisterObstacle(row + 1, col, row, col);
        }
        else if(vertical && !IsPlaced)
        {
            ObstacleManagerPho.Instance.RegisterObstacle(row, col, row, col + 1);
            ObstacleManagerPho.Instance.RegisterObstacle(row + 1, col, row + 1, col + 1);
        }
        IsPlaced = true;
    }


}
