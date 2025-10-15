using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visual Settings")]
    public Material activeMaterial;
    public Material inactiveMaterial;
    public ParticleSystem activationEffect;

    [Header("Checkpoint Settings")]
    public bool isActive = false;

    private CheckpointManager checkpointManager;
    private MeshRenderer meshRenderer;

    void Start()
    {
        // Find CheckpointManager instead of VoidSurface
        checkpointManager = FindFirstObjectByType<CheckpointManager>();
        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager not found in scene! Please add one.");
        }

        meshRenderer = GetComponent<MeshRenderer>();
        UpdateVisuals();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActivateCheckpoint();
        }
    }

    public void ActivateCheckpoint()
    {
        if (isActive) return; // Already active, no need to do anything

        isActive = true;
        UpdateVisuals();

        // Play activation effect
        if (activationEffect != null)
        {
            activationEffect.Play();
        }

        // Notify manager to deactivate other checkpoints and set this as current
        if (checkpointManager != null)
        {
            checkpointManager.SetActiveCheckpoint(this);
        }

        Debug.Log($"Checkpoint activated: {gameObject.name} at position: {transform.position}");
    }

    public void DeactivateCheckpoint()
    {
        isActive = false;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (meshRenderer != null)
        {
            if (isActive && activeMaterial != null)
            {
                meshRenderer.material = activeMaterial;
            }
            else if (!isActive && inactiveMaterial != null)
            {
                meshRenderer.material = inactiveMaterial;
            }
        }
    }

    // Public getter for checkpoint position
    public Vector3 GetRespawnPosition()
    {
        return transform.position + Vector3.up * 2f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, transform.localScale.x * 0.5f);
        
        // Draw respawn position indicator
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
    }
}