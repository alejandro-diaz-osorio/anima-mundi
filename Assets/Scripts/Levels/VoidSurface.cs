using UnityEngine;

public class VoidSurface : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float respawnHeight = -10f;
    public float checkInterval = 0.1f; // Check every 0.1 seconds instead of every frame
    
    [Header("Effects")]
    public ParticleSystem respawnEffect;
    public AudioClip fallSound;
    [Range(0f, 1f)]
    public float fallSoundVolume = 0.5f;
    
    [Header("References")]
    public CheckpointManager checkpointManager;

    private float nextCheckTime = 0f;

    void Start()
    {
        // Find CheckpointManager if not assigned
        if (checkpointManager == null)
        {
            checkpointManager = FindFirstObjectByType<CheckpointManager>();
            if (checkpointManager == null)
            {
                Debug.LogError("CheckpointManager not found! Please add one to the scene.");
            }
        }
    }

    private void Update()
    {
        // Optimize by checking at intervals instead of every frame
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        // Find all players and check if they fell
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.transform.position.y < respawnHeight)
            {
                RespawnPlayer(player);
            }
        }
    }

    private void RespawnPlayer(GameObject player)
    {
        if (checkpointManager == null)
        {
            Debug.LogError("Cannot respawn: CheckpointManager is missing!");
            return;
        }

        // Get respawn position from CheckpointManager
        Vector3 respawnPosition = checkpointManager.GetRespawnPosition();

        // Reset physics
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Teleport player
        player.transform.position = respawnPosition;
        player.transform.rotation = Quaternion.identity;

        // Visual effects
        if (respawnEffect != null)
        {
            ParticleSystem effect = Instantiate(respawnEffect, respawnPosition, Quaternion.identity);
            Destroy(effect.gameObject, 3f); // Clean up after 3 seconds
        }

        // Audio effects
        if (fallSound != null)
        {
            AudioSource.PlayClipAtPoint(fallSound, respawnPosition, fallSoundVolume);
        }

        // Reset abilities
        ResetPlayerAbilities(player);

        // Debug info
        string checkpointInfo = checkpointManager.HasActiveCheckpoint() 
            ? $"at checkpoint '{checkpointManager.GetActiveCheckpoint().gameObject.name}'" 
            : "at default spawn point";
        
        Debug.Log($"Player respawned {checkpointInfo} at position: {respawnPosition}");
    }

    private void ResetPlayerAbilities(GameObject player)
    {
        // Reset double jump
        DoubleJumpAbility doubleJump = player.GetComponent<DoubleJumpAbility>();
        if (doubleJump != null)
        {
            doubleJump.ResetDoubleJump();
        }

        // You can add more ability resets here as needed
        // For example:
        // DashAbility dash = player.GetComponent<DashAbility>();
        // if (dash != null) { dash.ResetDash(); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RespawnPlayer(other.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the death plane
        Gizmos.color = Color.red;
        float lineLength = 100f;
        
        // Draw cross pattern
        Gizmos.DrawLine(
            new Vector3(-lineLength, respawnHeight, 0), 
            new Vector3(lineLength, respawnHeight, 0)
        );
        Gizmos.DrawLine(
            new Vector3(0, respawnHeight, -lineLength), 
            new Vector3(0, respawnHeight, lineLength)
        );
        
        // Draw a grid for better visualization
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        int gridSize = 10;
        float spacing = lineLength / gridSize;
        
        for (int i = -gridSize; i <= gridSize; i++)
        {
            float offset = i * spacing;
            Gizmos.DrawLine(
                new Vector3(offset, respawnHeight, -lineLength),
                new Vector3(offset, respawnHeight, lineLength)
            );
            Gizmos.DrawLine(
                new Vector3(-lineLength, respawnHeight, offset),
                new Vector3(lineLength, respawnHeight, offset)
            );
        }
    }
}