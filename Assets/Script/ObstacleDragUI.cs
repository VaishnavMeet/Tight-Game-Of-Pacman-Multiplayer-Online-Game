using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI.Table;

public class ObstacleDragUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameObject draggedObject;
    public GameObject obstacle3DPrefab;
    public Material highlightMaterial;
    private Material originalMaterial;
    private Renderer objectRenderer;
    public TurnManager.PlayerTurn myTurn = TurnManager.PlayerTurn.Player1; // Set in Inspector for each UI button
    bool isVertical = false;
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

        if (draggedObject == null) return;

        if (finalSnapPosition.HasValue)
        {
            // Set position and rotation once here based on cached snap info
            draggedObject.transform.position = finalSnapPosition.Value;
            draggedObject.transform.rotation = finalSnapRotation;
            draggedObject.transform.localScale = new Vector3(1f, 0.8f, 0.2f);

            // Only validate placement (no transform changes inside)
            if (TryPlaceObstacle(finalSnapPosition.Value))
            {
                if (objectRenderer != null && originalMaterial != null)
                    objectRenderer.material = originalMaterial;
            }
            else
            {
                Destroy(draggedObject);
            }
        }
        else
        {
            Destroy(draggedObject);
        }

        draggedObject = null;
        finalSnapPosition = null;
    }



    private void TrySnapPreview(Vector3 dragPos)
    {
        float snapThreshold = (GridSpawner.Instance.boxSize + GridSpawner.Instance.spacing) * 0.6f;

        for (int row = 0; row < GridSpawner.Instance.rows; row++)
        {
            for (int col = 0; col < GridSpawner.Instance.columns; col++)
            {
                // Horizontal
                if (col + 1 < GridSpawner.Instance.columns)
                {
                    Vector3 p1 = GridSpawner.Instance.boxPositions[row, col];
                    Vector3 p2 = GridSpawner.Instance.boxPositions[row, col + 1];
                    Vector3 mid = (p1 + p2) / 2;

                    if (Vector3.Distance(dragPos, mid) < snapThreshold)
                    {
                        finalSnapPosition = new Vector3(mid.x, mid.y + 1, mid.z + 0.5f);
                        finalSnapRotation = Quaternion.identity;
                        isVertical = false;
                        //Debug.Log("hh");
                        draggedObject.transform.position = finalSnapPosition.Value;
                        draggedObject.transform.rotation = finalSnapRotation;
                        return;
                    }

                }

                // Vertical
                if (row + 1 < GridSpawner.Instance.rows)
                {
                    Vector3 p1 = GridSpawner.Instance.boxPositions[row, col];
                    Vector3 p2 = GridSpawner.Instance.boxPositions[row + 1, col];
                    Vector3 mid = (p1 + p2) / 2;

                    if (Vector3.Distance(dragPos, mid) < snapThreshold)
                    {
                        finalSnapPosition = new Vector3(mid.x + 0.5f, mid.y + 1, mid.z);
                        finalSnapRotation = Quaternion.Euler(0, 90, 0);
                        //Debug.Log("vv");
                        isVertical = true;
                        draggedObject.transform.position = finalSnapPosition.Value;
                        draggedObject.transform.rotation = finalSnapRotation;
                        return;
                    }

                }
            }
        }
    }

    private bool TryPlaceObstacle(Vector3 dropPos)
    {
        if (!TurnManager.Instance.CanPlaceObstacle())
        {
            //Debug.Log("Obstacle limit reached.");
            return false;
        }

        float snapThreshold = (GridSpawner.Instance.boxSize + GridSpawner.Instance.spacing) * 0.6f;

        for (int row = 0; row < GridSpawner.Instance.rows; row++)
        {
            for (int col = 0; col < GridSpawner.Instance.columns; col++)
            {
                // Check Horizontal snap positions
                if (col + 1 < GridSpawner.Instance.columns && !isVertical)
                {
                    Vector3 p1 = GridSpawner.Instance.boxPositions[row, col];
                    Vector3 p2 = GridSpawner.Instance.boxPositions[row, col + 1];
                    Vector3 mid = (p1 + p2) / 2;
                    mid = new Vector3(mid.x, mid.y + 1, mid.z + 0.5f);

                    if (Vector3.Distance(dropPos, mid) < snapThreshold)
                    {
                        if (ObstacleManager.Instance.IsBlocked(row + 1, col + 1, row, col + 1) || ObstacleManager.Instance.IsBlocked(row + 1, col, row, col))
                        {
                            //Debug.LogWarning("Attempted to place a duplicate obstacle.");
                            return false;
                        }

                        //Debug.Log("Horizontal");
                        TurnManager.Instance.ObstaclePlaced();
                        ObstacleManager.Instance?.RegisterObstacle(row+1, col+1, row , col + 1 ); //  Correct
                        ObstacleManager.Instance?.RegisterObstacle(row+1, col, row , col ); //  Correct
                        audioSource.PlayOneShot(obstacleBuiltClip);

                        return true;
                    }

                }

                // Check Vertical snap positions
                if (row + 1 < GridSpawner.Instance.rows)
                {
                    Vector3 p1 = GridSpawner.Instance.boxPositions[row, col];
                    Vector3 p2 = GridSpawner.Instance.boxPositions[row + 1, col];
                    Vector3 mid = (p1 + p2) / 2;
                    //Debug.Log(row);
                    mid = new Vector3(mid.x + 0.5f, mid.y + 1, mid.z);

                    if (Vector3.Distance(dropPos, mid) < snapThreshold)
                    {
                        if (ObstacleManager.Instance.IsBlocked(row, col, row, col + 1) || ObstacleManager.Instance.IsBlocked(row + 1, col, row + 1, col + 1))
                        {
                            //Debug.LogWarning("Attempted to place a duplicate obstacle.");
                            return false;
                        }
                        TurnManager.Instance.ObstaclePlaced();
                            ObstacleManager.Instance?.RegisterObstacle(row , col , row, col + 1); //  Correct
                            ObstacleManager.Instance?.RegisterObstacle(row + 1, col, row+1, col+1); //  Correct

                            //Debug.Log("vertical");
                            isVertical= false;
                        audioSource.PlayOneShot(obstacleBuiltClip);

                        return true;
                        
                    }


                }
            }
        }

        return false;
    }


}
