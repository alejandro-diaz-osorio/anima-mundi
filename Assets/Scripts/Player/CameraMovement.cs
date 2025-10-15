using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    
    [Header("Camera Settings")]
    public float distance = 5f;
    public float height = 2f;
    public float smoothSpeed = 2f;
    
    [Header("Mouse Controls")]
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -40f;
    public float maxVerticalAngle = 80f;
    
    [Header("Zoom Settings")]
    public float minZoom = 2f;
    public float maxZoom = 10f;
    public float zoomSpeed = 2f;
    
    private float currentX = 0f;
    private float currentY = 20f; // Start with slight downward angle
    private float currentDistance;
    
    void Start()
    {
        currentDistance = distance;
        
        // Lock cursor to center of screen for better camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Set initial camera position
        UpdateCameraPosition();
    }
    
    void FixedUpdate()
    {
        HandleMouseInput();
        HandleZoom();
        UpdateCameraPosition();
    }
    
    void HandleMouseInput()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Update rotation angles
        currentX += mouseX;
        currentY -= mouseY; // Inverted for natural feel
        
        // Clamp vertical rotation
        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        
        // Optional: Toggle cursor lock with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    void HandleZoom()
    {
        // Handle zoom with mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minZoom, maxZoom);
        }
    }
    
    void UpdateCameraPosition()
    {
        if (player == null) return;
        
        // Calculate the desired position based on rotation and distance
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = player.position + rotation * new Vector3(0, height, -currentDistance);
        
        // Use SmoothDamp for more stable following (alternative to Lerp)
        Vector3 velocity = Vector3.zero;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / smoothSpeed);
        
        // Alternative: Use direct assignment for immediate following (no smoothing)
        // transform.position = desiredPosition;
        
        // Look at target with slight offset for better viewing angle
        Vector3 lookTarget = player.position + Vector3.up * (height * 0.5f);
        transform.LookAt(lookTarget);
    }
    
    // Public method to get camera's forward direction (useful for player movement)
    public Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0; // Remove vertical component
        return forward.normalized;
    }
    
    // Public method to get camera's right direction (useful for player movement)
    public Vector3 GetCameraRight()
    {
        Vector3 right = transform.right;
        right.y = 0; // Remove vertical component
        return right.normalized;
    }
}