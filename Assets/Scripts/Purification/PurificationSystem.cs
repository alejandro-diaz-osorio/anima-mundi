using UnityEngine;
using System.Collections.Generic;

public class PurificationSystem : MonoBehaviour
{
    [Header("Purification Settings")]
    public float purificationRadius = 2f; // Radio del área que purifica
    public float updateInterval = 0.05f; // Qué tan seguido actualiza (menor = más suave)
    public int maxPurificationPoints = 3929; // Máximo de puntos almacenados (límite del shader)
    
    [Header("Ground Detection")]
    public bool onlyPurifyOnGround = true; // Solo purificar cuando está en el suelo
    public LayerMask groundLayerMask = 1; // Qué capas cuentan como suelo
    public float groundCheckDistance = 0.8f; // Distancia para detectar suelo
    
    [Header("Visual Settings")]
    public bool showDebugSpheres = true;
    public Color purificationColor = Color.green;
    
    [Header("References")]
    public Transform playerTransform; // La esfera del jugador
    
    // Private variables
    private List<Vector4> purificationPoints;
    private Vector4[] purificationPointsArray; // Fixed size array for shader
    private float lastUpdateTime;
    private static PurificationSystem instance;
    private List<Material> registeredMaterials; // Cache of materials
    
    // Material property IDs for optimization
    private static readonly int PurificationPointsID = Shader.PropertyToID("_PurificationPoints");
    private static readonly int PurificationCountID = Shader.PropertyToID("_PurificationCount");
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Validate and clamp max points to shader limit
        const int SHADER_ARRAY_LIMIT = 3929;
        if (maxPurificationPoints > SHADER_ARRAY_LIMIT)
        {
            Debug.LogWarning($"maxPurificationPoints ({maxPurificationPoints}) exceeds shader limit ({SHADER_ARRAY_LIMIT}). Clamping to limit.");
            maxPurificationPoints = SHADER_ARRAY_LIMIT;
        }
        
        purificationPoints = new List<Vector4>();
        purificationPointsArray = new Vector4[maxPurificationPoints];
        registeredMaterials = new List<Material>();
        
        // Initialize array with zeros
        for (int i = 0; i < maxPurificationPoints; i++)
        {
            purificationPointsArray[i] = Vector4.zero;
        }
    }
    
    void Start()
    {
        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("Player not found! Please assign playerTransform or tag player as 'Player'");
            }
        }
        
        // Find and register all purifiable objects
        RegisterAllPurifiableObjects();
    }
    
    void Update()
    {
        if (playerTransform == null) return;
        
        // Update at intervals for performance
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            // Check if player is grounded before adding purification point
            if (!onlyPurifyOnGround || IsPlayerGrounded())
            {
                AddPurificationPoint(playerTransform.position);
                UpdatePurificationMaterials();
            }
            lastUpdateTime = Time.time;
        }
    }
    
    private void AddPurificationPoint(Vector3 position)
    {
        // Check if this position is too close to existing points
        bool tooClose = false;
        float minDistance = purificationRadius * 0.3f; // 30% of radius
        
        foreach (Vector4 point in purificationPoints)
        {
            Vector3 pointPos = new Vector3(point.x, point.y, point.z);
            if (Vector3.Distance(position, pointPos) < minDistance)
            {
                tooClose = true;
                break;
            }
        }
        
        if (!tooClose)
        {
            // Add new purification point
            Vector4 newPoint = new Vector4(position.x, position.y, position.z, purificationRadius);
            purificationPoints.Add(newPoint);
            
            // Remove oldest points if we exceed the limit
            if (purificationPoints.Count > maxPurificationPoints)
            {
                purificationPoints.RemoveAt(0);
            }
            
            // Update the fixed array
            UpdatePointsArray();
        }
    }
    
    private void UpdatePointsArray()
    {
        // Clear array first
        for (int i = 0; i < maxPurificationPoints; i++)
        {
            if (i < purificationPoints.Count)
            {
                purificationPointsArray[i] = purificationPoints[i];
            }
            else
            {
                purificationPointsArray[i] = Vector4.zero;
            }
        }
    }
    
    private bool IsPlayerGrounded()
    {
        if (playerTransform == null) return false;
        
        // Check if player has BallController and use its grounded state
        BallController ballController = playerTransform.GetComponent<BallController>();
        if (ballController != null)
        {
            return ballController.IsGrounded();
        }
        
        // Fallback: Do our own ground check using spherecast
        Vector3 checkPosition = playerTransform.position;
        float checkRadius = 0.5f; // Slightly smaller than player for accuracy
        
        // Cast a sphere downward to detect ground
        bool isGrounded = Physics.CheckSphere(
            checkPosition - Vector3.up * groundCheckDistance, 
            checkRadius, 
            groundLayerMask
        );
        
        return isGrounded;
    }
    
    private void RegisterAllPurifiableObjects()
    {
        PurifiableObject[] purifiableObjects = FindObjectsByType<PurifiableObject>(FindObjectsSortMode.None);
        
        foreach (PurifiableObject obj in purifiableObjects)
        {
            RegisterMaterial(obj);
        }
        
        Debug.Log($"Registered {registeredMaterials.Count} purifiable materials");
    }
    
    public void RegisterMaterial(PurifiableObject obj)
    {
        if (obj != null && obj.IsInitialized())
        {
            Material mat = obj.GetMaterial();
            if (mat != null && !registeredMaterials.Contains(mat))
            {
                registeredMaterials.Add(mat);
                
                // Initialize the material with empty arrays
                InitializeMaterial(mat);
            }
        }
    }
    
    private void InitializeMaterial(Material mat)
    {
        if (mat == null) return;
        
        // Set initial count to 0
        mat.SetInt(PurificationCountID, 0);
        
        // Initialize with the full array (even if empty)
        mat.SetVectorArray(PurificationPointsID, purificationPointsArray);
    }
    
    private void UpdatePurificationMaterials()
    {
        // Update all registered materials
        foreach (Material mat in registeredMaterials)
        {
            if (mat != null)
            {
                UpdateMaterial(mat);
            }
        }
    }
    
    private void UpdateMaterial(Material mat)
    {
        if (mat == null) return;
        
        // Always send the full array to avoid the warning
        mat.SetVectorArray(PurificationPointsID, purificationPointsArray);
        mat.SetInt(PurificationCountID, purificationPoints.Count);
    }
    
    // Public methods for external control
    public static PurificationSystem GetInstance()
    {
        return instance;
    }
    
    public void ClearPurification()
    {
        purificationPoints.Clear();
        UpdatePointsArray();
        UpdatePurificationMaterials();
    }
    
    public void SetPurificationRadius(float radius)
    {
        purificationRadius = Mathf.Max(0.1f, radius);
    }
    
    public int GetPurificationPointCount()
    {
        return purificationPoints.Count;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showDebugSpheres || purificationPoints == null) return;
        
        Gizmos.color = purificationColor;
        foreach (Vector4 point in purificationPoints)
        {
            Vector3 position = new Vector3(point.x, point.y, point.z);
            float radius = point.w;
            Gizmos.DrawWireSphere(position, radius);
        }
        
        // Draw ground check sphere for player
        if (playerTransform != null && onlyPurifyOnGround)
        {
            bool isGrounded = IsPlayerGrounded();
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 checkPos = playerTransform.position - Vector3.up * groundCheckDistance;
            Gizmos.DrawWireSphere(checkPos, 0.5f);
        }
    }
    
    void OnGUI()
    {
        if (showDebugSpheres)
        {
            GUILayout.BeginArea(new Rect(10, 250, 250, 120));
            GUILayout.Label($"Purification Points: {purificationPoints.Count}/{maxPurificationPoints}");
            GUILayout.Label($"Purification Radius: {purificationRadius:F1}");
            GUILayout.Label($"Registered Materials: {registeredMaterials.Count}");
            
            if (onlyPurifyOnGround)
            {
                bool grounded = playerTransform != null && IsPlayerGrounded();
                GUI.color = grounded ? Color.green : Color.red;
                GUILayout.Label($"Grounded: {(grounded ? "Yes" : "No")}");
                GUI.color = Color.white;
            }
            
            if (GUILayout.Button("Clear Purification"))
            {
                ClearPurification();
            }
            GUILayout.EndArea();
        }
    }
}