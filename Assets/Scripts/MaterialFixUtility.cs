using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Utility script to fix corrupted shared materials
/// </summary>
public class MaterialFixUtility : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Material Restoration")]
    [Tooltip("Check this to reset all URP default materials")]
    public bool resetURPMaterials = false;
    
    [Header("Scene Cleanup")]
    [Tooltip("Find all renderers in scene and check their materials")]
    public bool scanScene = false;
    
    [ContextMenu("Reset All URP Default Materials")]
    public void ResetURPDefaultMaterials()
    {
        Debug.Log("Starting URP material reset...");
        
        // Force Unity to reimport URP materials
        string[] urpMaterialPaths = new string[]
        {
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat",
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/SimpleLit.mat",
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Unlit.mat",
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesLit.mat",
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesSimpleLit.mat",
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat"
        };
        
        foreach (string path in urpMaterialPaths)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        
        AssetDatabase.Refresh();
        Debug.Log("URP materials have been reimported. If issue persists, you may need to reinstall URP package.");
    }
    
    [ContextMenu("Scan Scene For Affected Objects")]
    public void ScanSceneForAffectedObjects()
    {
        Debug.Log("Scanning scene for objects with corrupted materials...");
        
        MeshRenderer[] allRenderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int affectedCount = 0;
        
        foreach (MeshRenderer renderer in allRenderers)
        {
            Material[] materials = renderer.sharedMaterials;
            
            foreach (Material mat in materials)
            {
                if (mat != null)
                {
                    // Check if material has the corrupted texture
                    if (mat.HasProperty("_CorruptedTex"))
                    {
                        Texture corruptedTex = mat.GetTexture("_CorruptedTex");
                        if (corruptedTex != null)
                        {
                            Debug.LogWarning($"Found affected object: {renderer.gameObject.name} with material: {mat.name}", renderer.gameObject);
                            affectedCount++;
                        }
                    }
                }
            }
        }
        
        Debug.Log($"Scan complete. Found {affectedCount} affected materials.");
    }
    
    [ContextMenu("Fix All Scene Materials")]
    public void FixAllSceneMaterials()
    {
        Debug.Log("Attempting to fix all scene materials...");
        
        MeshRenderer[] allRenderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int fixedCount = 0;
        
        foreach (MeshRenderer renderer in allRenderers)
        {
            // Skip objects that SHOULD have PurifiableObject
            if (renderer.GetComponent<PurifiableObject>() != null)
                continue;
            
            Material[] materials = renderer.sharedMaterials;
            bool needsFixing = false;
            
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                if (mat != null && mat.shader.name == "Custom/PurificationShader")
                {
                    // This material shouldn't be using the purification shader
                    // Create a new standard material
                    Material newMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    newMat.name = mat.name + "_Fixed";
                    
                    // Try to preserve the main texture if it's not the corrupted one
                    if (mat.mainTexture != null)
                    {
                        newMat.mainTexture = mat.mainTexture;
                    }
                    
                    materials[i] = newMat;
                    needsFixing = true;
                    fixedCount++;
                    
                    Debug.Log($"Fixed material on: {renderer.gameObject.name}", renderer.gameObject);
                }
            }
            
            if (needsFixing)
            {
                renderer.sharedMaterials = materials;
            }
        }
        
        Debug.Log($"Fixed {fixedCount} materials in scene.");
        EditorUtility.DisplayDialog("Material Fix Complete", 
            $"Fixed {fixedCount} materials.\n\nObjects with PurifiableObject component were left untouched.", 
            "OK");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(MaterialFixUtility))]
public class MaterialFixUtilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MaterialFixUtility utility = (MaterialFixUtility)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("Use these buttons to fix corrupted materials in your scene.", MessageType.Info);
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("1. Scan Scene", GUILayout.Height(30)))
        {
            utility.ScanSceneForAffectedObjects();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("2. Fix All Scene Materials", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Fix Materials", 
                "This will reset materials on objects WITHOUT PurifiableObject component.\n\nContinue?", 
                "Yes", "Cancel"))
            {
                utility.FixAllSceneMaterials();
            }
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("3. Reset URP Default Materials", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Reset URP Materials", 
                "This will reimport all URP default materials.\n\nContinue?", 
                "Yes", "Cancel"))
            {
                utility.ResetURPDefaultMaterials();
            }
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("After fixing, save your scene and restart Unity if needed.", MessageType.Warning);
    }
}
#endif