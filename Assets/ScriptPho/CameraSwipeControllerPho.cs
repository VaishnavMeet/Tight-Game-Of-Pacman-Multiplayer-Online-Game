using UnityEngine;

public class CameraSwipeControllerPho : MonoBehaviour
{
    public Transform target;                   // Target to orbit
    public float distance = 10f;               // Starting distance
    public float minDistance = 3f;             // Closest zoom
    public float maxDistance = 20f;            // Farthest zoom

    public float rotationSensitivity = 0.2f;   // Swipe X sensitivity
    public float pitchSensitivity = 0.2f;      // Swipe Y sensitivity
    public float zoomSensitivity = 0.05f;      // Pinch zoom sensitivity

    public float minPitch = -10f;              // Down tilt limit
    public float maxPitch = 60f;               // Up tilt limit

    public float lerpSpeed = 5f;               // Smoothness

    private Vector2 lastTouchPosition;
    private float currentYaw = 0f;
    private float targetYaw = 0f;

    private float currentPitch = 20f;
    private float targetPitch = 20f;

    private float currentDistance;
    private float targetDistance;

    private bool isDragging = false;

    public static CameraSwipeControllerPho Instance;
    public bool canMove = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraSwipeController: Target not assigned!");
            enabled = false;
            return;
        }

        currentDistance = targetDistance = distance;
        UpdateCameraPosition(true);
    }

    void Update()
    {
        if (!canMove) return;
        HandleInput();
        UpdateCameraPosition(false);
    }

    void HandleInput()
    {
        // --- Touch Input (Mobile) ---
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.position - lastTouchPosition;
                lastTouchPosition = touch.position;

                targetYaw += delta.x * rotationSensitivity;
                targetPitch -= delta.y * pitchSensitivity;
                targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            // Pinch to zoom
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float prevDist = Vector2.Distance(prev0, prev1);
            float currDist = Vector2.Distance(t0.position, t1.position);

            float delta = prevDist - currDist;
            targetDistance += delta * zoomSensitivity;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

#if UNITY_EDITOR
        // --- Mouse Input (Editor) ---
        if (Input.GetMouseButtonDown(0))
        {
            lastTouchPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastTouchPosition;
            lastTouchPosition = Input.mousePosition;

            targetYaw += delta.x * rotationSensitivity;
            targetPitch -= delta.y * pitchSensitivity;
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // Scroll to zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetDistance -= scroll * 10f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
#endif
    }

    void UpdateCameraPosition(bool instant)
    {
        currentYaw = instant ? targetYaw : Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * lerpSpeed);
        currentPitch = instant ? targetPitch : Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * lerpSpeed);
        currentDistance = instant ? targetDistance : Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * lerpSpeed);

        // Calculate offset from rotation and distance
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -currentDistance);
        Vector3 desiredPosition = target.position + offset;

        // Prevent camera from going below target's Y position
        if (desiredPosition.y < target.position.y)
        {
            desiredPosition.y = target.position.y;
        }

        transform.position = desiredPosition;
        transform.LookAt(target);
    }
}
