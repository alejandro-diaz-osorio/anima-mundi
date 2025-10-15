using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class PurifiableObject : MonoBehaviour
{
    [Header("Texture Settings")]
    public Texture2D corruptedTexture; // Textura A (carne, etc)
    public Texture2D purifiedTexture; // Textura B (pasto, etc)
    
    [Header("Material Settings")]
    public float transitionSmoothness = 0.5f; // Suavidad de la transición
    
    [Header("Safety Settings")]
    [Tooltip("ALWAYS keep this TRUE to avoid corrupting shared materials!")]
    public bool alwaysCreateInstance = true;
    
    private Material materialInstance;
    private MeshRenderer meshRenderer;
    private bool isInitialized = false;
    
    // Material property IDs
    private static readonly int CorruptedTexID = Shader.PropertyToID("_CorruptedTex");
    private static readonly int PurifiedTexID = Shader.PropertyToID("_PurifiedTex");
    private static readonly int SmoothnessID = Shader.PropertyToID("_TransitionSmoothness");
    
    void Awake()
    {
        // Initialize early to prevent shared material modification
        meshRenderer = GetComponent<MeshRenderer>();
    }
    
    void Start()
    {
        InitializeMaterial();
        
        // Register with PurificationSystem after initialization
        PurificationSystem system = PurificationSystem.GetInstance();
        if (system != null)
        {
            system.RegisterMaterial(this);
        }
        else
        {
            Debug.LogWarning($"PurificationSystem not found! Add PurificationSystem to scene for {gameObject.name} to work.");
        }
    }
    
    [ContextMenu("Force Reinitialize Material")]
    public void ForceReinitialize()
    {
        // Clean up old material if exists
        if (materialInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(materialInstance);
            }
            else
            {
                DestroyImmediate(materialInstance);
            }
        }
        
        isInitialized = false;
        InitializeMaterial();
        
        // Re-register with system
        PurificationSystem system = PurificationSystem.GetInstance();
        if (system != null)
        {
            system.RegisterMaterial(this);
        }
        
        Debug.Log($"Material reinitialized on {gameObject.name}");
    }
    
    private void InitializeMaterial()
    {
        if (meshRenderer == null)
        {
            Debug.LogError($"MeshRenderer not found on {gameObject.name}!");
            return;
        }
        
        // CRITICAL: Always create an instance to avoid corrupting shared materials
        if (!alwaysCreateInstance)
        {
            Debug.LogError($"alwaysCreateInstance is FALSE on {gameObject.name}! This WILL corrupt shared materials. Setting to TRUE.");
            alwaysCreateInstance = true;
        }
        
        // Load the purification shader
        Shader purificationShader = Shader.Find("Custom/PurificationShader");
        
        if (purificationShader == null)
        {
            Debug.LogError($"Custom/PurificationShader not found! Please create the shader for {gameObject.name}");
            return;
        }
        
        // Create a NEW material instance (never modify shared materials!)
        materialInstance = new Material(purificationShader);
        materialInstance.name = $"{gameObject.name}_PurificationMaterial (Instance)";
        
        // IMPORTANT: Use renderer.material (creates instance) not renderer.sharedMaterial!
        meshRenderer.material = materialInstance;
        
        // Validate textures
        if (corruptedTexture == null)
        {
            Debug.LogError($"No corrupted texture assigned on {gameObject.name}! Shader will show magenta.");
            return;
        }
        
        if (purifiedTexture == null)
        {
            Debug.LogWarning($"No purified texture assigned on {gameObject.name}! Using corrupted texture as fallback.");
            purifiedTexture = corruptedTexture;
        }
        
        // Set textures
        materialInstance.SetTexture(CorruptedTexID, corruptedTexture);
        materialInstance.SetTexture(PurifiedTexID, purifiedTexture);
        materialInstance.SetFloat(SmoothnessID, transitionSmoothness);
        
        // Set main texture for fallback compatibility
        materialInstance.mainTexture = corruptedTexture;
        
        isInitialized = true;
        
        Debug.Log($"PurifiableObject initialized on {gameObject.name} with instance material");
    }
    
    // Public getters
    public Material GetMaterial()
    {
        return materialInstance;
    }
    
    public bool IsInitialized()
    {
        return isInitialized && materialInstance != null;
    }
    
    // Update textures at runtime
    public void SetCorruptedTexture(Texture2D texture)
    {
        corruptedTexture = texture;
        if (materialInstance != null && texture != null)
        {
            materialInstance.SetTexture(CorruptedTexID, texture);
            materialInstance.mainTexture = texture;
        }
    }
    
    public void SetPurifiedTexture(Texture2D texture)
    {
        purifiedTexture = texture;
        if (materialInstance != null && texture != null)
        {
            materialInstance.SetTexture(PurifiedTexID, texture);
        }
    }
    
    public void SetTransitionSmoothness(float smoothness)
    {
        transitionSmoothness = Mathf.Clamp01(smoothness);
        if (materialInstance != null)
        {
            materialInstance.SetFloat(SmoothnessID, transitionSmoothness);
        }
    }
    
    void OnDestroy()
    {
        // CRITICAL: Clean up material instance to prevent memory leaks
        if (materialInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(materialInstance);
            }
            else
            {
                DestroyImmediate(materialInstance);
            }
        }
    }
    
    void OnValidate()
    {
        // Safety check in editor
        if (!alwaysCreateInstance)
        {
            Debug.LogWarning($"alwaysCreateInstance should ALWAYS be TRUE on {gameObject.name}!");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Show object bounds
        if (meshRenderer != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(meshRenderer.bounds.center, meshRenderer.bounds.size);
        }
        
        // Show texture status
        if (meshRenderer != null)
        {
            Vector3 pos = transform.position + Vector3.up * 2f;
            
#if UNITY_EDITOR
            UnityEditor.Handles.Label(pos, 
                $"Corrupted: {(corruptedTexture != null ? "✓" : "✗")}\n" +
                $"Purified: {(purifiedTexture != null ? "✓" : "✗")}");
#endif
        }
    }
}