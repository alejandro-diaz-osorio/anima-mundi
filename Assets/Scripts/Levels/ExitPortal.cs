using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ExitPortal : MonoBehaviour
{
    [Header("Portal Settings")]
    public bool isActive = false;
    public float rotationSpeed = 30f;
    
    [Header("Visual Effects")]
    public GameObject portalMesh;
    public ParticleSystem portalParticles;
    public Light portalLight;
    
    [Header("Medal Display UI")]
    public GameObject rankPanel;
    public Image goldMedalImage;
    public Image silverMedalImage;
    public Image bronzeMedalImage;
    public Image noMedalImage;
    public TextMeshProUGUI rankText;
    
    [Header("Buttons")]
    public Button restartButton;
    public Button menuButton;
    
    [Header("Scene Names")]
    public string menuSceneName = "MainMenu";
    
    [Header("Audio")]
    public AudioClip enterPortalSound;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private bool playerEntered = false;
    private PillarSystem pillarSystem; // Guardar referencia
    
    void Start()
    {
        if (showDebugLogs) Debug.Log("=== ExitPortal Start ===");
        
        SetPortalActive(false);
        
        if (rankPanel != null)
        {
            rankPanel.SetActive(false);
        }
        
        HideAllMedals();
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonPressed);
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuButtonPressed);
        }
        
        // Guardar referencia al PillarSystem
        pillarSystem = FindFirstObjectByType<PillarSystem>();
        if (pillarSystem != null)
        {
            pillarSystem.OnPillarDestroyed += OnPillarDestroyed;
            Debug.Log("✓ ExitPortal subscribed to PillarSystem.OnPillarDestroyed");
        }
        else
        {
            Debug.LogError("✗ PillarSystem NOT FOUND in scene!");
        }
    }
    
    void Update()
    {
        if (isActive && portalMesh != null)
        {
            portalMesh.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void OnPillarDestroyed()
    {
        Debug.Log("=== OnPillarDestroyed - Activating portal ===");
        SetPortalActive(true);
        playerEntered = false; // Reset para permitir entrada
    }
    
    private void SetPortalActive(bool active)
    {
        if (showDebugLogs) Debug.Log($"SetPortalActive: {active}");
        
        isActive = active;
        
        if (portalMesh != null)
        {
            portalMesh.SetActive(active);
        }
        
        if (portalParticles != null)
        {
            if (active)
                portalParticles.Play();
            else
                portalParticles.Stop();
        }
        
        if (portalLight != null)
        {
            portalLight.enabled = active;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"=== OnTriggerEnter: {other.gameObject.name}, Tag: {other.tag} ===");
        Debug.Log($"isActive: {isActive}, playerEntered: {playerEntered}");
        
        if (other.CompareTag("Player") && isActive && !playerEntered)
        {
            Debug.Log("=== PLAYER ENTERED PORTAL ===");
            playerEntered = true;
            OnPlayerEnteredPortal();
        }
    }
    
    private void OnPlayerEnteredPortal()
    {
        if (enterPortalSound != null)
        {
            AudioSource.PlayClipAtPoint(enterPortalSound, transform.position);
        }
        
        // CRÍTICO: Calcular medalla AQUÍ usando el TimerController
        PillarSystem.MedalRank medal = CalculateMedalFromTimer();
        
        Debug.Log($"✓ Level completed with {medal} medal!");
        
        ShowMedalScreen(medal);
    }
    
    // NUEVO MÉTODO: Calcular medalla directamente del timer
    private PillarSystem.MedalRank CalculateMedalFromTimer()
    {
        if (pillarSystem == null)
        {
            Debug.LogWarning("PillarSystem is null, returning None medal");
            return PillarSystem.MedalRank.None;
        }
        
        // Obtener el TimerController
        TimerController timer = FindFirstObjectByType<TimerController>();
        if (timer == null)
        {
            Debug.LogWarning("TimerController not found, returning None medal");
            return PillarSystem.MedalRank.None;
        }
        
        float timeRemaining = timer.GetTimeRemaining();
        Debug.Log($"Time remaining: {timeRemaining:F2}s");
        
        // Usar los mismos umbrales que PillarSystem
        if (timeRemaining >= pillarSystem.goldMedalTime)
        {
            Debug.Log($"Gold medal! ({timeRemaining:F2}s >= {pillarSystem.goldMedalTime}s)");
            return PillarSystem.MedalRank.Gold;
        }
        else if (timeRemaining >= pillarSystem.silverMedalTime)
        {
            Debug.Log($"Silver medal! ({timeRemaining:F2}s >= {pillarSystem.silverMedalTime}s)");
            return PillarSystem.MedalRank.Silver;
        }
        else if (timeRemaining >= pillarSystem.bronzeMedalTime)
        {
            Debug.Log($"Bronze medal! ({timeRemaining:F2}s >= {pillarSystem.bronzeMedalTime}s)");
            return PillarSystem.MedalRank.Bronze;
        }
        else
        {
            Debug.Log($"No medal ({timeRemaining:F2}s < {pillarSystem.bronzeMedalTime}s)");
            return PillarSystem.MedalRank.None;
        }
    }
    
    // MODIFICADO: Ahora recibe la medalla como parámetro
    private void ShowMedalScreen(PillarSystem.MedalRank medal)
    {
        Debug.Log($"=== ShowMedalScreen called with medal: {medal} ===");
        
        if (rankPanel == null)
        {
            Debug.LogError("Rank Panel is NULL!");
            return;
        }
        
        rankPanel.SetActive(true);
        
        HideAllMedals();
        
        // Mostrar la medalla correspondiente
        Image medalToShow = medal switch
        {
            PillarSystem.MedalRank.Gold => goldMedalImage,
            PillarSystem.MedalRank.Silver => silverMedalImage,
            PillarSystem.MedalRank.Bronze => bronzeMedalImage,
            _ => noMedalImage
        };
        
        if (medalToShow != null)
        {
            medalToShow.gameObject.SetActive(true);
            Debug.Log($"Showing medal image: {medal}");
        }
        else
        {
            Debug.LogError($"Medal image for {medal} is NULL!");
        }
        
        if (rankText != null)
        {
            rankText.text = medal switch
            {
                PillarSystem.MedalRank.Gold => "World Purified: A Rank",
                PillarSystem.MedalRank.Silver => "World Purified: B Rank",
                PillarSystem.MedalRank.Bronze => "World Purified: C Rank",
                _ => "World Purified: D Rank"
            };
        }
        
        // Detener el timer
        TimerController timer = FindFirstObjectByType<TimerController>();
        if (timer != null)
        {
            timer.StopTimer();
        }
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    private void HideAllMedals()
    {
        if (goldMedalImage != null) goldMedalImage.gameObject.SetActive(false);
        if (silverMedalImage != null) silverMedalImage.gameObject.SetActive(false);
        if (bronzeMedalImage != null) bronzeMedalImage.gameObject.SetActive(false);
        if (noMedalImage != null) noMedalImage.gameObject.SetActive(false);
    }
    
    public void OnRestartButtonPressed()
    {
        Debug.Log("Restart button pressed");
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void OnMenuButtonPressed()
    {
        Debug.Log("Menu button pressed");
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(menuSceneName);
    }
    
    void OnDestroy()
    {
        if (pillarSystem != null)
        {
            pillarSystem.OnPillarDestroyed -= OnPillarDestroyed;
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartButtonPressed);
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(OnMenuButtonPressed);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}