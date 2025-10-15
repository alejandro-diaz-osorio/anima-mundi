using UnityEngine;

public class DashAbility : MonoBehaviour
{
    [Header("Dash Settings")]
    public bool dashEnabled = true; // Toggle for debugging
    public float dashForce = 15f; // Strength of the dash
    public float dashCooldown = 1f; // Time between dashes in seconds
    public float dashDuration = 0.2f; // How long the dash effect lasts
    
    [Header("Input Settings")]
    public KeyCode dashKey = KeyCode.LeftShift;
    
    [Header("Y Velocity Control")]
    public bool cancelVerticalVelocity = true; // Cancel Y momentum when dashing
    public float verticalVelocityReduction = 0.8f; // How much to reduce Y velocity (0 = full cancel, 1 = no cancel)
    
    [Header("Visual Feedback")]
    public bool showDebugInfo = true;
    public Color dashTrailColor = Color.cyan;
    
    [Header("References")]
    public BallController ballController; // Reference to main controller
    public CameraMovement cameraController; // Reference to camera for direction
    
    // Private variables
    private Rigidbody rb;
    private float lastDashTime = 0f;
    private bool isDashing = false;
    private float dashStartTime;
    private TrailRenderer trailRenderer;
    
    // Events (optional - for other systems to listen to)
    public System.Action OnDashStart;
    public System.Action OnDashEnd;
    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        
        // Auto-find references if not assigned (consistent with other scripts)
        if (ballController == null)
            ballController = GetComponent<BallController>();
        
        if (cameraController == null)
            cameraController = FindFirstObjectByType<CameraMovement>();
        
        // Setup trail renderer for visual feedback
        SetupTrailRenderer();
    }
    
    void Update()
    {
        if (!dashEnabled) return;
        
        // Check for dash input
        if (Input.GetKeyDown(dashKey) && CanDash())
        {
            PerformDash();
        }
        
        // Update dash state
        UpdateDashState();
    }
    
    private bool CanDash()
    {
        if (!dashEnabled) return false;
        if (Time.time - lastDashTime < dashCooldown) return false;
        return true;
    }
    
    private void PerformDash()
    {
        // Calculate dash direction based on camera or input
        Vector3 dashDirection = GetDashDirection();
        
        if (dashDirection == Vector3.zero) return; // No direction to dash
        
        // Cancel or reduce vertical velocity if enabled
        if (cancelVerticalVelocity)
        {
            Vector3 currentVelocity = rb.linearVelocity;
            currentVelocity.y *= (1f - verticalVelocityReduction);
            rb.linearVelocity = currentVelocity;
        }
        
        // Apply dash force
        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
        
        // Update dash state
        isDashing = true;
        dashStartTime = Time.time;
        lastDashTime = Time.time;
        
        // Visual effects
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
            trailRenderer.emitting = true;
        }
        
        // Trigger events
        OnDashStart?.Invoke();
        
        // Debug info
        if (showDebugInfo)
        {
            Debug.Log($"Dash performed! Direction: {dashDirection}, Force: {dashForce}");
        }
    }
    
    private Vector3 GetDashDirection()
    {
        // Get current movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 direction = Vector3.zero;
        
        if (cameraController != null)
        {
            // Use camera-relative direction if there's input
            if (horizontal != 0 || vertical != 0)
            {
                Vector3 cameraForward = cameraController.GetCameraForward();
                Vector3 cameraRight = cameraController.GetCameraRight();
                direction = (cameraForward * vertical + cameraRight * horizontal).normalized;
            }
            else
            {
                // If no input, dash in camera forward direction
                direction = cameraController.GetCameraForward();
            }
        }
        else
        {
            // Fallback: use world coordinates
            if (horizontal != 0 || vertical != 0)
            {
                direction = new Vector3(horizontal, 0, vertical).normalized;
            }
            else
            {
                direction = transform.forward; // Default forward
            }
        }
        
        return direction;
    }
    
    private void UpdateDashState()
    {
        // Check if dash duration is over
        if (isDashing && Time.time - dashStartTime >= dashDuration)
        {
            EndDash();
        }
    }
    
    private void EndDash()
    {
        isDashing = false;
        
        // Disable visual effects
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
        
        // Trigger events
        OnDashEnd?.Invoke();
    }
    
    private void SetupTrailRenderer()
    {
        // Check if trail renderer already exists
        trailRenderer = GetComponent<TrailRenderer>();
        
        if (trailRenderer == null)
        {
            // Create trail renderer for dash effect
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }
        
        // Configure trail renderer
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = dashTrailColor;
        trailRenderer.startWidth = 0.5f;
        trailRenderer.endWidth = 0.1f;
        trailRenderer.time = 0.3f;
        trailRenderer.enabled = false;
        trailRenderer.emitting = false;
    }
    
    // Public methods for external access
    public bool IsDashing()
    {
        return isDashing;
    }
    
    public float GetDashCooldownRemaining()
    {
        return Mathf.Max(0f, dashCooldown - (Time.time - lastDashTime));
    }
    
    public bool IsOnCooldown()
    {
        return GetDashCooldownRemaining() > 0f;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showDebugInfo || !dashEnabled) return;
        
        // Draw dash direction
        Vector3 dashDir = GetDashDirection();
        if (dashDir != Vector3.zero)
        {
            Gizmos.color = isDashing ? Color.red : Color.yellow;
            Gizmos.DrawRay(transform.position, dashDir * 3f);
        }
        
        // Draw dash availability indicator
        if (CanDash())
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.2f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.1f);
        }
    }
    
    // Optional: GUI for debugging
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        GUILayout.Label($"Dash Enabled: {dashEnabled}");
        GUILayout.Label($"Cooldown: {GetDashCooldownRemaining():F1}s");
        GUILayout.Label($"Is Dashing: {isDashing}");
        if (GUILayout.Button("Toggle Dash"))
        {
            dashEnabled = !dashEnabled;
        }
        GUILayout.EndArea();
    }
}