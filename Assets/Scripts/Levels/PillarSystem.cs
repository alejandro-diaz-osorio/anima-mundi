using UnityEngine;

public class PillarSystem : MonoBehaviour
{
    [Header("Pillar Settings")]
    public float minimumImpactSpeed = 8f;
    public bool pillarDestroyed = false;
    
    [Header("Visual Effects")]
    public ParticleSystem destructionParticles;
    public GameObject pillarMesh;
    
    [Header("Audio")]
    public AudioClip impactSound;
    public AudioClip destructionSound;
    [Range(0f, 1f)] public float soundVolume = 0.8f;
    
    [Header("Camera Shake")]
    public float shakeIntensity = 0.5f;
    public float shakeDuration = 0.3f;
    
    [Header("Timer Settings")]
    public float timeLimit = 60f;
    public Transform returnTarget;
    public float returnRadius = 5f;
    
    [Header("Medals/Ranks")]
    public float goldMedalTime = 45f;
    public float silverMedalTime = 30f;
    public float bronzeMedalTime = 15f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private TimerController timerController;
    private CameraShake cameraShake;
    private bool timerStarted = false;
    private Collider pillarCollider;
    
    public System.Action OnPillarDestroyed;
    public System.Action<MedalRank> OnPlayerReturnedInTime;
    public System.Action OnTimeExpired;
    
    public enum MedalRank { None, Bronze, Silver, Gold }
    
    void Start()
    {
        pillarCollider = GetComponent<Collider>();
        timerController = FindFirstObjectByType<TimerController>();
        cameraShake = Camera.main?.GetComponent<CameraShake>();
        
        if (cameraShake == null && Camera.main != null)
        {
            cameraShake = Camera.main.gameObject.AddComponent<CameraShake>();
        }
    }
    
    void Update()
    {
        if (timerStarted)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && returnTarget != null)
            {
                if (Vector3.Distance(player.transform.position, returnTarget.position) <= returnRadius)
                {
                    PlayerReturnedSuccessfully();
                }
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !pillarDestroyed)
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                float impactSpeed = playerRb.linearVelocity.magnitude;
                
                if (impactSpeed >= minimumImpactSpeed)
                {
                    DestroyPillar(collision.contacts[0].point);
                }
                else
                {
                    PlaySound(impactSound, collision.contacts[0].point);
                    Debug.Log($"Impact too weak! Need {minimumImpactSpeed:F2}, got {impactSpeed:F2}");
                }
            }
        }
    }
    
    private void DestroyPillar(Vector3 impactPoint)
    {
        pillarDestroyed = true;
        
        if (pillarMesh != null) pillarMesh.SetActive(false);
        if (destructionParticles != null)
        {
            destructionParticles.transform.position = impactPoint;
            destructionParticles.Play();
        }
        
        PlaySound(destructionSound, impactPoint);
        if (cameraShake != null) cameraShake.Shake(shakeIntensity, shakeDuration);
        if (pillarCollider != null) pillarCollider.enabled = false;
        
        if (timerController != null)
        {
            timerController.StartTimer(timeLimit);
            timerStarted = true;
        }
        
        OnPillarDestroyed?.Invoke();
        Debug.Log("PILLAR DESTROYED! Return to start!");
    }
    
    private void PlayerReturnedSuccessfully()
    {
        if (timerController != null)
        {
            float timeRemaining = timerController.GetTimeRemaining();
            timerController.StopTimer();
            
            MedalRank medal = CalculateMedal(timeRemaining);
            Debug.Log($"Returned with {timeRemaining:F2}s! Medal: {medal}");
            
            OnPlayerReturnedInTime?.Invoke(medal);
            timerStarted = false;
        }
    }
    
    public void OnTimerExpired()
    {
        Debug.Log("TIME'S UP!");
        OnTimeExpired?.Invoke();
        timerStarted = false;
        ResetPlayerToPillar();
    }
    
    private void ResetPlayerToPillar()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = transform.position + Vector3.up * 3f;
            
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            DoubleJumpAbility doubleJump = player.GetComponent<DoubleJumpAbility>();
            if (doubleJump != null) doubleJump.ResetDoubleJump();
        }
    }
    
    private MedalRank CalculateMedal(float timeRemaining)
    {
        if (timeRemaining >= goldMedalTime) return MedalRank.Gold;
        if (timeRemaining >= silverMedalTime) return MedalRank.Silver;
        if (timeRemaining >= bronzeMedalTime) return MedalRank.Bronze;
        return MedalRank.None;
    }
    
    private void PlaySound(AudioClip clip, Vector3 position)
    {
        if (clip != null) AudioSource.PlayClipAtPoint(clip, position, soundVolume);
    }
    
    public void ResetPillar()
    {
        pillarDestroyed = false;
        timerStarted = false;
        if (pillarMesh != null) pillarMesh.SetActive(true);
        if (pillarCollider != null) pillarCollider.enabled = true;
        if (timerController != null) timerController.ResetTimer();
    }
    
    void OnDrawGizmos()
    {
        if (returnTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(returnTarget.position, returnRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, returnTarget.position);
        }
        Gizmos.color = pillarDestroyed ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}
