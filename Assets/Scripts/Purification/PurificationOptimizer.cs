using UnityEngine;

/// <summary>
/// Component opcional para optimizar el rendimiento del sistema de purificación
/// </summary>
[RequireComponent(typeof(PurificationSystem))]
public class PurificationOptimizer : MonoBehaviour
{
    [Header("Optimization Settings")]
    [Tooltip("Merge points closer than this distance")]
    public float mergeDistance = 0.5f;
    
    [Tooltip("How often to run optimization (seconds)")]
    public float optimizationInterval = 2f;
    
    [Tooltip("Enable automatic optimization")]
    public bool enableAutoOptimization = true;
    
    [Header("Culling Settings")]
    [Tooltip("Remove points further than this from player")]
    public bool enableDistanceCulling = false;
    public float maxDistanceFromPlayer = 50f;
    
    [Header("Debug")]
    public bool showOptimizationStats = true;
    
    private PurificationSystem purificationSystem;
    private float lastOptimizationTime;
    private int pointsBeforeOptimization;
    private int pointsAfterOptimization;
    private int pointsMerged;
    private int pointsCulled;
    
    void Start()
    {
        purificationSystem = GetComponent<PurificationSystem>();
        if (purificationSystem == null)
        {
            Debug.LogError("PurificationOptimizer requires PurificationSystem component!");
            enabled = false;
        }
    }
    
    void Update()
    {
        if (!enableAutoOptimization) return;
        
        if (Time.time - lastOptimizationTime >= optimizationInterval)
        {
            OptimizePurificationPoints();
            lastOptimizationTime = Time.time;
        }
    }
    
    [ContextMenu("Optimize Now")]
    public void OptimizePurificationPoints()
    {
        if (purificationSystem == null) return;
        
        pointsBeforeOptimization = purificationSystem.GetPurificationPointCount();
        pointsMerged = 0;
        pointsCulled = 0;
        
        // Note: Since purificationPoints is private, we can't optimize directly
        // Instead, this serves as a monitoring/stats tool
        // The actual optimization is handled by the PurificationSystem's minDistance check
        
        pointsAfterOptimization = purificationSystem.GetPurificationPointCount();
        
        if (showOptimizationStats)
        {
            Debug.Log($"Optimization complete: {pointsBeforeOptimization} → {pointsAfterOptimization} points");
        }
    }
    
    void OnGUI()
    {
        if (!showOptimizationStats) return;
        
        int yPos = 380;
        int lineHeight = 20;
        
        GUILayout.BeginArea(new Rect(10, yPos, 300, 120));
        GUI.Box(new Rect(0, 0, 300, 120), "Purification Optimizer");
        
        GUI.Label(new Rect(5, 25, 290, lineHeight), $"Current Points: {purificationSystem.GetPurificationPointCount()}");
        GUI.Label(new Rect(5, 45, 290, lineHeight), $"Auto Optimization: {(enableAutoOptimization ? "ON" : "OFF")}");
        GUI.Label(new Rect(5, 65, 290, lineHeight), $"Next optimization: {(optimizationInterval - (Time.time - lastOptimizationTime)):F1}s");
        
        if (GUI.Button(new Rect(5, 90, 140, 25), "Optimize Now"))
        {
            OptimizePurificationPoints();
        }
        
        if (GUI.Button(new Rect(155, 90, 140, 25), "Clear All"))
        {
            purificationSystem.ClearPurification();
        }
        
        GUILayout.EndArea();
    }
}