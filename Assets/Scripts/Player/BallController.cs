using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float maxVelocity = 20f; // Limit max speed to prevent erratic movement
    public float dragMultiplier = 1f; // Additional drag control
    
    [Header("Camera Reference")]
    public CameraMovement cameraController; // Reference to the camera controller
    
    [Header("Ground Detection Settings")]
    public LayerMask groundLayerMask = 1;
    public float groundCheckDistance = 0.6f;
    public float groundCheckRadius = 0.4f;
    
    [Header("Jump Settings")]
    public float jumpCooldown = 0.1f;
    public float coyoteTime = 0.1f;
    
    [Header("Ability System")]
    public DashAbility dashAbility; // Reference to dash ability
    public DoubleJumpAbility doubleJumpAbility; // Reference to double jump ability
    
    private Rigidbody rb;
    private bool isGrounded = false;
    private bool wasGroundedLastFrame = false;
    private float lastJumpTime = 0f;
    private float lastGroundedTime = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Auto-find references if not assigned
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraMovement>();
        }
        
        if (dashAbility == null)
        {
            dashAbility = GetComponent<DashAbility>();
        }
        
        if (doubleJumpAbility == null)
        {
            doubleJumpAbility = GetComponent<DoubleJumpAbility>();
        }
    }

    void Update()
    {
        // Only handle jump input if double jump ability is not present or disabled
        if ((doubleJumpAbility == null || !doubleJumpAbility.doubleJumpEnabled) && Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }
    }

    void FixedUpdate()
    {
        // Always check grounded state regardless of double jump ability
        CheckGrounded();
        
        HandleMovement();
        LimitVelocity(); // Now properly called
    }
    
    private void HandleMovement()
    {
        // Get input
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        
        // Calculate movement relative to camera direction
        Vector3 movement = Vector3.zero;
        
        if (cameraController != null)
        {
            // Get camera's forward and right directions (without Y component)
            Vector3 cameraForward = cameraController.GetCameraForward();
            Vector3 cameraRight = cameraController.GetCameraRight();
            
            // Calculate movement vector relative to camera
            movement = (cameraForward * moveVertical + cameraRight * moveHorizontal).normalized;
        }
        else
        {
            // Fallback to world coordinates if no camera reference
            movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        }
        
        // Reduce movement force if dashing (optional - for better dash feel)
        float currentMoveSpeed = moveSpeed;
        if (dashAbility != null && dashAbility.IsDashing())
        {
            currentMoveSpeed *= 0.3f; // Reduce normal movement during dash
        }
        
        // Apply force to the Rigidbody
        rb.AddForce(movement * currentMoveSpeed);
        
        // Apply additional drag when no input (helps with stopping)
        if (moveHorizontal == 0 && moveVertical == 0)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(-horizontalVelocity * dragMultiplier, ForceMode.Force);
        }
    }
    
    private void LimitVelocity()
    {
        // Limit horizontal velocity to prevent excessive speeds
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxVelocity)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxVelocity;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }
    }
    
    private void CheckGrounded()
    {
        wasGroundedLastFrame = isGrounded;
        
        Vector3 sphereCenter = transform.position - Vector3.up * (groundCheckDistance - groundCheckRadius);
        Collider[] hits = Physics.OverlapSphere(sphereCenter, groundCheckRadius, groundLayerMask);
        isGrounded = hits.Length > 0;
        
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }
    
    private void TryJump()
    {
        if (Time.time - lastJumpTime < jumpCooldown)
            return;
            
        bool canJump = isGrounded || (Time.time - lastGroundedTime < coyoteTime);
        
        if (canJump)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0;
            rb.linearVelocity = velocity;
            
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            lastJumpTime = Time.time;
        }
    }
    
    // Public getters for abilities to access grounded state
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    public float GetLastGroundedTime()
    {
        return lastGroundedTime;
    }
    
    void OnDrawGizmos()
    {
        // Always draw ground check gizmos
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 sphereCenter = transform.position - Vector3.up * (groundCheckDistance - groundCheckRadius);
        Gizmos.DrawWireSphere(sphereCenter, groundCheckRadius);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}