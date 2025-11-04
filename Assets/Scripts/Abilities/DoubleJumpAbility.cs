using UnityEngine;

public class DoubleJumpAbility : MonoBehaviour
{
    [Header("Double Jump Settings")]
    public bool doubleJumpEnabled = true; // Toggle for debugging
    public float secondJumpForce = 8f; // Force for the second jump (usually less than first jump)
    public float secondJumpForceMultiplier = 0.8f; // Multiplier based on original jump force
    public bool useMultiplier = true; // Use multiplier instead of fixed force
    
    [Header("Input Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    
    [Header("Jump Control")]
    public float jumpCooldown = 0.1f; // Minimum time between jumps
    public bool resetVerticalVelocity = true; // Reset Y velocity before second jump
    public float velocityResetAmount = 0.5f; // How much to reduce current Y velocity (0 = full reset, 1 = no reset)
    
    [Header("Visual Feedback")]
    public bool showDebugInfo = true;
    public Color doubleJumpParticleColor = Color.white;
    public bool createJumpParticles = true;
    
    [Header("References")]
    public BallController ballController; // Reference to main controller
    
    // Private variables
    private Rigidbody rb;
    private bool hasDoubleJump = false; // Can we still double jump?
    private bool isGrounded = false;
    private float lastJumpTime = 0f;
    private int jumpCount = 0; // Track how many jumps we've done
    private ParticleSystem jumpParticles;
    
    // Events
    public System.Action OnFirstJump;
    public System.Action OnSecondJump;
    public System.Action OnLanded;
    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        
        // Auto-find references if not assigned
        if (ballController == null)
            ballController = GetComponent<BallController>();
        
        // Setup particle system for jump effects
        if (createJumpParticles)
            SetupParticleSystem();
        
        // Initialize double jump availability
        hasDoubleJump = true;
    }
    
    void Update()
    {
        if (!doubleJumpEnabled) return;
        
        // Use BallController's ground detection when possible
        UpdateGroundedState();
        
        // Handle jump input
        if (Input.GetKeyDown(jumpKey) && CanJump())
        {
            PerformJump();
        }
    }
    
    private void UpdateGroundedState()
{
    bool wasGrounded = isGrounded;
    
    if (ballController != null)
    {
        isGrounded = ballController.IsGrounded();
    }
    else
    {
        Vector3 sphereCenter = transform.position - Vector3.up * (ballController.groundCheckDistance - ballController.groundCheckRadius);
        isGrounded = Physics.CheckSphere(sphereCenter, ballController.groundCheckRadius, ballController.groundLayerMask);
    }
    
    // Reset double jump when landing
    if (!wasGrounded && isGrounded)
    {
        hasDoubleJump = true;
        jumpCount = 0;
        OnLanded?.Invoke();
        
        AudioManager.PlayLandSound();
        
        if (showDebugInfo)
        {
            Debug.Log("Landed - Double jump reset");
        }
    }
}
    
    private bool CanJump()
    {
        if (!doubleJumpEnabled) return false;
        if (Time.time - lastJumpTime < jumpCooldown) return false;
        
        // Can jump if grounded OR if we still have our double jump
        return isGrounded || hasDoubleJump;
    }
    
    private void PerformJump()
    {
        float jumpForceToApply;
        bool isSecondJump = false;
        
        if (isGrounded)
        {
            // First jump
            if (ballController != null)
            {
                jumpForceToApply = ballController.jumpForce;
            }
            else
            {
                jumpForceToApply = 10f;
            }
            
            jumpCount = 1;
            OnFirstJump?.Invoke();
            
            // NUEVO: Sonido de salto
            AudioManager.PlayJumpSound();
            
            if (showDebugInfo)
            {
                Debug.Log("First jump executed");
            }
        }
        else if (hasDoubleJump)
        {
            if (useMultiplier && ballController != null)
            {
                jumpForceToApply = ballController.jumpForce * secondJumpForceMultiplier;
            }
            else
            {
                jumpForceToApply = secondJumpForce;
            }
            
            jumpCount = 2;
            hasDoubleJump = false;
            isSecondJump = true;
            OnSecondJump?.Invoke();
            
            AudioManager.PlayJumpSound();
            
            if (showDebugInfo)
            {
                Debug.Log("Second jump executed");
            }
        }
        else
        {
            return;
        }
        
        // Apply jump logic
        if (resetVerticalVelocity || isSecondJump)
        {
            // Reset or reduce vertical velocity for consistent jump height
            Vector3 velocity = rb.linearVelocity;
            if (isSecondJump)
            {
                velocity.y *= (1f - velocityResetAmount);
            }
            else
            {
                velocity.y = 0; // Always reset for first jump
            }
            rb.linearVelocity = velocity;
        }
        
        // Apply jump force
        rb.AddForce(new Vector3(0, jumpForceToApply, 0), ForceMode.Impulse);
        
        // Update timing
        lastJumpTime = Time.time;
        
        // Visual effects
        if (createJumpParticles && jumpParticles != null)
        {
            var main = jumpParticles.main;
            main.startColor = isSecondJump ? doubleJumpParticleColor : Color.gray;
            jumpParticles.Play();
        }
    }
    
    private void SetupParticleSystem()
    {
        // Check if particle system already exists
        jumpParticles = GetComponentInChildren<ParticleSystem>();
        
        if (jumpParticles == null)
        {
            // Create a new particle system
            GameObject particleObj = new GameObject("JumpParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;
            jumpParticles = particleObj.AddComponent<ParticleSystem>();
        }
        
        // Configure particle system
        var main = jumpParticles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 5f;
        main.startSize = 0.2f;
        main.startColor = doubleJumpParticleColor;
        main.maxParticles = 20;
        
        var emission = jumpParticles.emission;
        emission.enabled = false; // We'll trigger manually
        
        var shape = jumpParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
        
        // Stop playing by default
        jumpParticles.Stop();
    }
    
    // Public methods for external access
    public bool HasDoubleJump()
    {
        return hasDoubleJump;
    }
    
    public int GetJumpCount()
    {
        return jumpCount;
    }
    
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    public float GetJumpCooldownRemaining()
    {
        return Mathf.Max(0f, jumpCooldown - (Time.time - lastJumpTime));
    }
    
    // Force reset double jump (useful for other abilities or power-ups)
    public void ResetDoubleJump()
    {
        hasDoubleJump = true;
        jumpCount = 0;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showDebugInfo || !doubleJumpEnabled) return;
        
        // Draw ground check sphere only if we're doing our own ground detection
        if (ballController == null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 sphereCenter = transform.position - Vector3.up * 0.6f;
            Gizmos.DrawWireSphere(sphereCenter, 0.4f);
        }
        
        // Draw double jump availability indicator
        Vector3 indicatorPos = transform.position + Vector3.up * 2.5f;
        if (hasDoubleJump)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(indicatorPos, 0.3f);
        }
        else
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(indicatorPos, 0.15f);
        }
        
        // Draw jump count
        if (jumpCount > 0)
        {
            Gizmos.color = jumpCount == 1 ? Color.yellow : Color.magenta;
            for (int i = 0; i < jumpCount; i++)
            {
                Gizmos.DrawWireCube(transform.position + Vector3.up * 3f + Vector3.right * (i * 0.5f - 0.25f), Vector3.one * 0.2f);
            }
        }
    }
    
    // Optional: GUI for debugging
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 120, 200, 120));
        GUILayout.Label($"Double Jump Enabled: {doubleJumpEnabled}");
        GUILayout.Label($"Has Double Jump: {hasDoubleJump}");
        GUILayout.Label($"Jump Count: {jumpCount}");
        GUILayout.Label($"Is Grounded: {isGrounded}");
        if (GUILayout.Button("Toggle Double Jump"))
        {
            doubleJumpEnabled = !doubleJumpEnabled;
        }
        if (GUILayout.Button("Reset Double Jump"))
        {
            ResetDoubleJump();
        }
        GUILayout.EndArea();
    }
}