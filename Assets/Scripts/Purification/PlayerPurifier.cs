using UnityEngine;

public class PlayerPurifier : MonoBehaviour
{
    [Header("Purification Settings")]
    public bool enablePurification = true;
    public float purificationRadiusMultiplier = 1f; // Multiplica el radio base
    
    [Header("Visual Effects")]
    public ParticleSystem purificationParticles;
    public bool showPurificationRadius = true;
    public Color radiusColor = new Color(0, 1, 0, 0.3f);
    
    [Header("Audio")]
    public AudioClip purificationSound;
    public float soundVolume = 0.3f;
    public float soundCooldown = 0.5f;
    
    private PurificationSystem purificationSystem;
    private float lastSoundTime;
    private float currentRadius;
    
    void Start()
    {
        // Find or wait for PurificationSystem
        purificationSystem = PurificationSystem.GetInstance();
        
        if (purificationSystem == null)
        {
            Debug.LogWarning("PurificationSystem not found! Purification will not work.");
        }
        
        // Setup particles if assigned
        if (purificationParticles != null)
        {
            var main = purificationParticles.main;
            main.loop = true;
            main.playOnAwake = true;
            
            if (!enablePurification)
            {
                purificationParticles.Stop();
            }
        }
    }
    
    void Update()
    {
        // Update radius from system
        if (purificationSystem != null)
        {
            currentRadius = purificationSystem.purificationRadius * purificationRadiusMultiplier;
        }
        
        // Control particle effects
        if (purificationParticles != null)
        {
            if (enablePurification && !purificationParticles.isPlaying)
            {
                purificationParticles.Play();
            }
            else if (!enablePurification && purificationParticles.isPlaying)
            {
                purificationParticles.Stop();
            }
            
            // Update particle size based on radius
            var shape = purificationParticles.shape;
            shape.radius = currentRadius;
        }
        
        // Play sound occasionally
        if (enablePurification && purificationSound != null && Time.time - lastSoundTime > soundCooldown)
        {
            PlayPurificationSound();
            lastSoundTime = Time.time;
        }
    }
    
    private void PlayPurificationSound()
    {
        if (purificationSound != null)
        {
            AudioSource.PlayClipAtPoint(purificationSound, transform.position, soundVolume);
        }
    }
    
    // Public methods
    public void SetPurificationEnabled(bool enabled)
    {
        enablePurification = enabled;
    }
    
    public void SetRadiusMultiplier(float multiplier)
    {
        purificationRadiusMultiplier = Mathf.Max(0.1f, multiplier);
    }
    
    public float GetCurrentRadius()
    {
        return currentRadius;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showPurificationRadius) return;
        
        Gizmos.color = radiusColor;
        
        // Draw purification radius sphere
        if (purificationSystem != null)
        {
            currentRadius = purificationSystem.purificationRadius * purificationRadiusMultiplier;
        }
        else if (currentRadius == 0)
        {
            currentRadius = 2f; // Default preview
        }
        
        Gizmos.DrawWireSphere(transform.position, currentRadius);
        
        // Draw solid sphere with transparency
        Gizmos.color = new Color(radiusColor.r, radiusColor.g, radiusColor.b, 0.1f);
        Gizmos.DrawSphere(transform.position, currentRadius);
    }
}