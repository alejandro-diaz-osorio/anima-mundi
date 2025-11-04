using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    [Header("Jump Particles")]
    public ParticleSystem jumpParticles;
    public int jumpBurstAmount = 15;
    public Color jumpParticleColor = Color.white;
    
    [Header("Land Particles")]
    public ParticleSystem landParticles;
    public int landBurstAmount = 20;
    public Color landParticleColor = new Color(0.7f, 0.7f, 0.7f);
    public float minLandVelocity = -3f; // Velocidad mínima para mostrar partículas
    
    [Header("Double Jump Particles")]
    public Color doubleJumpParticleColor = Color.cyan;
    
    private BallController ballController;
    private DoubleJumpAbility doubleJumpAbility;
    private bool wasGrounded = false;
    
    void Start()
    {
        ballController = GetComponent<BallController>();
        doubleJumpAbility = GetComponent<DoubleJumpAbility>();
        
        // Setup jump particles if not assigned
        if (jumpParticles == null)
        {
            jumpParticles = CreateParticleSystem("JumpParticles");
        }
        ConfigureParticleSystem(jumpParticles, jumpParticleColor, 0.2f, 3f);
        
        // Setup land particles if not assigned
        if (landParticles == null)
        {
            landParticles = CreateParticleSystem("LandParticles");
        }
        ConfigureParticleSystem(landParticles, landParticleColor, 0.3f, 5f);
        
        // Subscribe to jump events if double jump exists
        if (doubleJumpAbility != null)
        {
            doubleJumpAbility.OnFirstJump += OnJump;
            doubleJumpAbility.OnSecondJump += OnDoubleJump;
        }
    }
    
    void Update()
    {
        CheckLanding();
    }
    
    private void CheckLanding()
    {
        if (ballController == null) return;
        
        bool isGrounded = ballController.IsGrounded();
        
        // Detect landing
        if (!wasGrounded && isGrounded)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null && rb.linearVelocity.y < minLandVelocity)
            {
                PlayLandParticles();
            }
        }
        
        wasGrounded = isGrounded;
    }
    
    private void OnJump()
    {
        PlayJumpParticles(jumpParticleColor);
    }
    
    private void OnDoubleJump()
    {
        PlayJumpParticles(doubleJumpParticleColor);
    }
    
    private void PlayJumpParticles(Color color)
    {
        if (jumpParticles == null) return;
        
        var main = jumpParticles.main;
        main.startColor = color;
        jumpParticles.transform.position = transform.position - Vector3.up * 0.5f;
        jumpParticles.Emit(jumpBurstAmount);
    }
    
    private void PlayLandParticles()
    {
        if (landParticles == null) return;
        
        landParticles.transform.position = transform.position - Vector3.up * 0.5f;
        landParticles.Emit(landBurstAmount);
    }
    
    private ParticleSystem CreateParticleSystem(string name)
    {
        GameObject particleObj = new GameObject(name);
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;
        return particleObj.AddComponent<ParticleSystem>();
    }
    
    private void ConfigureParticleSystem(ParticleSystem ps, Color color, float size, float speed)
    {
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = speed;
        main.startSize = size;
        main.startColor = color;
        main.gravityModifier = 1f;
        main.maxParticles = 50;
        main.loop = false;
        
        var emission = ps.emission;
        emission.enabled = false; // Manual emission
        
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
        shape.radiusThickness = 0f;
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
    }
    
    void OnDestroy()
    {
        if (doubleJumpAbility != null)
        {
            doubleJumpAbility.OnFirstJump -= OnJump;
            doubleJumpAbility.OnSecondJump -= OnDoubleJump;
        }
    }
}