using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Header("Default Spawn Settings")]
    public Transform defaultSpawnPoint; // Fallback spawn if no checkpoint is active
    public Vector3 defaultSpawnPosition = Vector3.zero; // Used if defaultSpawnPoint is null
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private Checkpoint currentActiveCheckpoint;
    
    void Start()
    {
        // Validate default spawn point
        if (defaultSpawnPoint == null && defaultSpawnPosition == Vector3.zero)
        {
            Debug.LogWarning("No default spawn point set! Using world origin (0,0,0).");
        }
    }
    
    /// <summary>
    /// Sets a checkpoint as active and deactivates all others
    /// </summary>
    public void SetActiveCheckpoint(Checkpoint newCheckpoint)
    {
        if (newCheckpoint == null)
        {
            Debug.LogError("Trying to set null checkpoint as active!");
            return;
        }
        
        // Deactivate previous checkpoint
        if (currentActiveCheckpoint != null && currentActiveCheckpoint != newCheckpoint)
        {
            currentActiveCheckpoint.DeactivateCheckpoint();
        }
        
        // Set new active checkpoint
        currentActiveCheckpoint = newCheckpoint;
        
        if (showDebugInfo)
        {
            Debug.Log($"Active checkpoint changed to: {newCheckpoint.gameObject.name}");
        }
    }
    
    /// <summary>
    /// Gets the current respawn position
    /// </summary>
    public Vector3 GetRespawnPosition()
    {
        // Use active checkpoint if available
        if (currentActiveCheckpoint != null)
        {
            return currentActiveCheckpoint.GetRespawnPosition();
        }
        
        // Fall back to default spawn point
        if (defaultSpawnPoint != null)
        {
            return defaultSpawnPoint.position;
        }
        
        // Last resort: use default position
        return defaultSpawnPosition + Vector3.up * 2f;
    }
    
    /// <summary>
    /// Checks if there's an active checkpoint
    /// </summary>
    public bool HasActiveCheckpoint()
    {
        return currentActiveCheckpoint != null;
    }
    
    /// <summary>
    /// Gets the current active checkpoint
    /// </summary>
    public Checkpoint GetActiveCheckpoint()
    {
        return currentActiveCheckpoint;
    }
    
    /// <summary>
    /// Clears the current checkpoint (useful for testing)
    /// </summary>
    public void ClearActiveCheckpoint()
    {
        if (currentActiveCheckpoint != null)
        {
            currentActiveCheckpoint.DeactivateCheckpoint();
            currentActiveCheckpoint = null;
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw default spawn position
        Vector3 spawnPos = defaultSpawnPoint != null ? defaultSpawnPoint.position : defaultSpawnPosition;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(spawnPos + Vector3.up * 2f, 0.5f);
        Gizmos.DrawLine(spawnPos, spawnPos + Vector3.up * 2f);
    }
}