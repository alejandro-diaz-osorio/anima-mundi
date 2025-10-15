using UnityEngine;

[ExecuteInEditMode]
public class ShaderDebugger : MonoBehaviour
{
    [Header("Debug Info")]
    public bool showDebugInfo = true;
    
    private Material material;
    private MeshRenderer meshRenderer;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        UpdateMaterial();
    }
    
    void Update()
    {
        // Update material reference in case it changes
        if (meshRenderer != null && material == null)
        {
            UpdateMaterial();
        }
    }
    
    void UpdateMaterial()
    {
        if (meshRenderer != null)
        {
            material = meshRenderer.sharedMaterial;
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo || material == null) return;
        
        int yPos = 500;
        int lineHeight = 20;
        
        GUI.Box(new Rect(10, yPos, 400, 220), "Shader Debug Info");
        yPos += 25;
        
        GUI.Label(new Rect(15, yPos, 390, lineHeight), $"Material: {material.name}");
        yPos += lineHeight;
        
        GUI.Label(new Rect(15, yPos, 390, lineHeight), $"Shader: {material.shader.name}");
        yPos += lineHeight;
        
        // Check if using purification shader
        bool isPurificationShader = material.shader.name == "Custom/PurificationShader";
        
        if (isPurificationShader)
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(15, yPos, 390, lineHeight), "✓ Using Purification Shader");
            yPos += lineHeight;
            GUI.color = Color.white;
            
            // Check for textures (only if shader has these properties)
            if (material.HasProperty("_CorruptedTex"))
            {
                Texture corruptedTex = material.GetTexture("_CorruptedTex");
                GUI.Label(new Rect(15, yPos, 390, lineHeight), 
                    $"Corrupted Texture: {(corruptedTex != null ? corruptedTex.name : "MISSING!")}");
                yPos += lineHeight;
                
                if (corruptedTex == null)
                {
                    GUI.color = Color.red;
                    GUI.Label(new Rect(15, yPos, 390, lineHeight), "⚠ Assign texture in PurifiableObject!");
                    yPos += lineHeight;
                    GUI.color = Color.white;
                }
            }
            
            if (material.HasProperty("_PurifiedTex"))
            {
                Texture purifiedTex = material.GetTexture("_PurifiedTex");
                GUI.Label(new Rect(15, yPos, 390, lineHeight), 
                    $"Purified Texture: {(purifiedTex != null ? purifiedTex.name : "MISSING!")}");
                yPos += lineHeight;
                
                if (purifiedTex == null)
                {
                    GUI.color = Color.red;
                    GUI.Label(new Rect(15, yPos, 390, lineHeight), "⚠ Assign texture in PurifiableObject!");
                    yPos += lineHeight;
                    GUI.color = Color.white;
                }
            }
            
            // Check for purification data
            if (material.HasProperty("_PurificationCount"))
            {
                int count = material.GetInt("_PurificationCount");
                GUI.Label(new Rect(15, yPos, 390, lineHeight), $"Purification Count: {count}");
                yPos += lineHeight;
            }
            
            if (material.HasProperty("_TransitionSmoothness"))
            {
                float smoothness = material.GetFloat("_TransitionSmoothness");
                GUI.Label(new Rect(15, yPos, 390, lineHeight), $"Transition Smoothness: {smoothness:F2}");
                yPos += lineHeight;
            }
        }
        else
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(15, yPos, 390, lineHeight), "⚠ NOT using Purification Shader");
            yPos += lineHeight;
            GUI.color = Color.white;
            
            GUI.Label(new Rect(15, yPos, 390, lineHeight), "This object won't be purifiable!");
            yPos += lineHeight;
            
            // Check if has PurifiableObject component
            PurifiableObject purifiable = GetComponent<PurifiableObject>();
            if (purifiable != null)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(15, yPos, 390, lineHeight), "Has PurifiableObject but wrong shader!");
                yPos += lineHeight;
                GUI.Label(new Rect(15, yPos, 390, lineHeight), "Material may have been reset. Check component.");
                GUI.color = Color.white;
            }
        }
        
        // Show shader keywords
        string[] keywords = material.shaderKeywords;
        if (keywords.Length > 0)
        {
            yPos += 5;
            GUI.Label(new Rect(15, yPos, 390, lineHeight), $"Keywords: {string.Join(", ", keywords)}");
        }
    }
}