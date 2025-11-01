using UnityEngine;

public class LimboWorldManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public bool disableDash = true;
    public bool disableDoubleJump = true;
    public bool disablePurification = true;
    
    [Header("Environment")]
    public Color fogColor = new Color(0.5f, 0.5f, 0.5f);
    public float fogDensity = 0.07f;
    
    void Start()
    {
        ConfigurePlayerAbilities();
        ConfigureEnvironment();
    }
    
    void ConfigurePlayerAbilities()
    {
        // Deshabilitar habilidades avanzadas
        var dash = FindFirstObjectByType<DashAbility>();
        if (dash != null && disableDash) 
        {
            dash.dashEnabled = false;
            Debug.Log("Dash disabled for Limbo tutorial");
        }
        
        var doubleJump = FindFirstObjectByType<DoubleJumpAbility>();
        if (doubleJump != null && disableDoubleJump) 
        {
            doubleJump.doubleJumpEnabled = false;
            Debug.Log("Double Jump disabled for Limbo tutorial");
        }
        
        var purifier = FindFirstObjectByType<PlayerPurifier>();
        if (purifier != null && disablePurification) 
        {
            purifier.enablePurification = false;
            Debug.Log("Purification disabled for Limbo tutorial");
        }
    }
    
    void ConfigureEnvironment()
    {
        // Configure fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
    }
}