// AudioManager.cs - Sistema central de audio
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Music")]
    public AudioClip ambientMusic;
    public AudioClip escapeMusic;
    [Range(0f, 1f)] public float musicVolume = 0.3f;
    public float musicFadeSpeed = 1f;
    
    [Header("Player Sounds")]
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip rollingSound; // Loop
    [Range(0f, 1f)] public float jumpVolume = 0.5f;
    [Range(0f, 1f)] public float landVolume = 0.6f;
    [Range(0f, 1f)] public float rollingMaxVolume = 0.4f;
    
    [Header("Rolling Settings")]
    public float minSpeedForRolling = 2f;
    public float maxSpeedForFullVolume = 15f;
    
    private AudioSource musicSource;
    private AudioSource rollingSource;
    private Transform player;
    private Rigidbody playerRb;
    private bool isEscapeMode = false;
    private static AudioManager instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Setup music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;
        
        // Setup rolling sound source
        rollingSource = gameObject.AddComponent<AudioSource>();
        rollingSource.loop = true;
        rollingSource.volume = 0f;
        rollingSource.playOnAwake = false;
        rollingSource.clip = rollingSound;
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody>();
        }
        
        // Subscribe to pillar events
        PillarSystem pillar = FindFirstObjectByType<PillarSystem>();
        if (pillar != null)
        {
            pillar.OnPillarDestroyed += OnEscapeStart;
            pillar.OnTimeExpired += PlayAmbientMusic;
        }
        
        PlayAmbientMusic();
    }
    
    void Update()
    {
        UpdateRollingSound();
    }
    
    private void UpdateRollingSound()
    {
        if (playerRb == null) return;
        
        // Get horizontal speed
        Vector3 horizontalVelocity = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;
        
        // Calculate volume based on speed
        if (speed > minSpeedForRolling)
        {
            if (!rollingSource.isPlaying)
            {
                rollingSource.Play();
            }
            
            float volumePercent = Mathf.InverseLerp(minSpeedForRolling, maxSpeedForFullVolume, speed);
            float targetVolume = volumePercent * rollingMaxVolume;
            rollingSource.volume = Mathf.Lerp(rollingSource.volume, targetVolume, Time.deltaTime * 5f);
            
            // Also adjust pitch slightly for more dynamic sound
            rollingSource.pitch = Mathf.Lerp(0.8f, 1.2f, volumePercent);
        }
        else
        {
            // Fade out
            rollingSource.volume = Mathf.Lerp(rollingSource.volume, 0f, Time.deltaTime * 3f);
            
            if (rollingSource.volume < 0.01f && rollingSource.isPlaying)
            {
                rollingSource.Stop();
            }
        }
    }
    
    private void PlayAmbientMusic()
    {
        if (ambientMusic != null && musicSource != null && !isEscapeMode)
        {
            musicSource.Stop();
            musicSource.clip = ambientMusic;
            musicSource.Play();
            isEscapeMode = false;
        }
    }
    
    private void OnEscapeStart()
    {
        if (escapeMusic != null && !isEscapeMode && musicSource.isPlaying)
        {
            musicSource.Stop();
            StartCoroutine(CrossfadeMusic(escapeMusic));
            isEscapeMode = true;
        }
    }
    
    private System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        // Fade out current
        while (musicSource.volume > 0.01f)
        {
            musicSource.volume -= musicFadeSpeed * Time.deltaTime;
            yield return null;
        }
        
        // Switch clip
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();
        
        // Fade in new
        while (musicSource.volume < musicVolume)
        {
            musicSource.volume += musicFadeSpeed * Time.deltaTime;
            yield return null;
        }
        
        musicSource.volume = musicVolume;
    }
    
    // Public methods for other scripts
    public static void PlayJumpSound()
    {
        if (instance != null && instance.jumpSound != null && instance.player != null)
        {
            AudioSource.PlayClipAtPoint(instance.jumpSound, instance.player.position, instance.jumpVolume);
        }
    }
    
    public static void PlayLandSound()
    {
        if (instance != null && instance.landSound != null && instance.player != null)
        {
            AudioSource.PlayClipAtPoint(instance.landSound, instance.player.position, instance.landVolume);
        }
    }
}