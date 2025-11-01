using UnityEngine;

public class ParticleSystemManager : MonoBehaviour
{
    [Header("Jump Particles")]
    public ParticleSystem jumpParticles;
    public ParticleSystem doubleJumpParticles;
    public ParticleSystem landingParticles;
    
    [Header("Dash Particles")]
    public ParticleSystem dashParticles;
    public ParticleSystem dashTrailParticles;
    
    [Header("Purification Particles")]
    public ParticleSystem purificationAuraParticles;
    public ParticleSystem purificationGroundParticles;
    
    [Header("Environment Particles")]
    public ParticleSystem limboFogParticles;
    public ParticleSystem limboFloatingDustParticles;
    
    [Header("Colors")]
    public Color jumpColor = Color.white;
    public Color doubleJumpColor = Color.cyan;
    public Color dashColor = new Color(1f, 0.5f, 0f); // Orange
    public Color purificationColor = Color.green;
    public Color limboColor = new Color(0.5f, 0.5f, 0.5f); // Gray
    
    private static ParticleSystemManager instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static ParticleSystemManager GetInstance()
    {
        return instance;
    }
    
    // Play particles at position
    public void PlayJumpParticles(Vector3 position)
    {
        if (jumpParticles != null)
        {
            jumpParticles.transform.position = position;
            var main = jumpParticles.main;
            main.startColor = jumpColor;
            jumpParticles.Play();
        }
    }
    
    public void PlayDoubleJumpParticles(Vector3 position)
    {
        if (doubleJumpParticles != null)
        {
            doubleJumpParticles.transform.position = position;
            var main = doubleJumpParticles.main;
            main.startColor = doubleJumpColor;
            doubleJumpParticles.Play();
        }
    }
    
    public void PlayLandingParticles(Vector3 position)
    {
        if (landingParticles != null)
        {
            landingParticles.transform.position = position;
            landingParticles.Play();
        }
    }
    
    public void PlayDashParticles(Vector3 position)
    {
        if (dashParticles != null)
        {
            dashParticles.transform.position = position;
            var main = dashParticles.main;
            main.startColor = dashColor;
            dashParticles.Play();
        }
    }
    
    // Instantiate particle at position (for one-shot effects)
    public void InstantiateParticleEffect(ParticleSystem prefab, Vector3 position, Quaternion rotation, float destroyAfter = 3f)
    {
        if (prefab != null)
        {
            ParticleSystem instance = Instantiate(prefab, position, rotation);
            Destroy(instance.gameObject, destroyAfter);
        }
    }
}