using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public GameObject timerPanel;
    public TextMeshProUGUI timeUpText;
    
    [Header("Timer Settings")]
    public bool countDown = true;
    public float warningTime = 10f;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    
    [Header("Animation")]
    public bool pulseWhenLow = true;
    public float pulseSpeed = 2f;
    
    private float currentTime;
    private float timeLimit;
    private bool isRunning = false;
    private bool hasExpired = false;
    private PillarSystem pillarSystem;
    
    void Start()
    {
        pillarSystem = FindFirstObjectByType<PillarSystem>();
        
        if (timerPanel != null) timerPanel.SetActive(false);
        if (timeUpText != null) timeUpText.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (!isRunning) return;
        
        if (countDown)
        {
            currentTime -= Time.deltaTime;
            
            if (currentTime <= 0 && !hasExpired)
            {
                currentTime = 0;
                TimerExpired();
            }
        }
        else
        {
            currentTime += Time.deltaTime;
        }
        
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(currentTime);
            
            // Change color when low
            if (currentTime <= warningTime && currentTime > 0)
            {
                if (pulseWhenLow)
                {
                    float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                    timerText.color = Color.Lerp(warningColor, normalColor, pulse);
                }
                else
                {
                    timerText.color = warningColor;
                }
            }
            else
            {
                timerText.color = normalColor;
            }
        }
    }
    
    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
    
    private void TimerExpired()
    {
        hasExpired = true;
        isRunning = false;
        
        if (timeUpText != null)
        {
            timeUpText.gameObject.SetActive(true);
            timeUpText.text = "TIME'S UP!";
            Invoke(nameof(HideTimeUpText), 2f);
        }
        
        if (pillarSystem != null)
        {
            pillarSystem.OnTimerExpired();
        }
    }
    
    private void HideTimeUpText()
    {
        if (timeUpText != null)
        {
            timeUpText.gameObject.SetActive(false);
        }
    }
    
    public void StartTimer(float duration)
    {
        timeLimit = duration;
        currentTime = countDown ? duration : 0;
        isRunning = true;
        hasExpired = false;
        
        // CRÍTICO: Mostrar UI solo cuando inicia
        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
        }
        
        // Asegurar que TimeUp esté oculto
        if (timeUpText != null)
        {
            timeUpText.gameObject.SetActive(false);
        }
    }
    
    public void StopTimer()
    {
        isRunning = false;
        
        // CRÍTICO: Ocultar UI cuando se detiene
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
    }
    
    public void PauseTimer()
    {
        isRunning = false;
    }
    
    public void ResumeTimer()
    {
        if (!hasExpired) isRunning = true;
    }
    
    public void ResetTimer()
    {
        currentTime = countDown ? timeLimit : 0;
        isRunning = false;
        hasExpired = false;
        
        // CRÍTICO: Ocultar toda la UI
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
        
        if (timeUpText != null)
        {
            timeUpText.gameObject.SetActive(false);
        }
    }
    
    public float GetTimeRemaining()
    {
        return currentTime;
    }
    
    public float GetTimeElapsed()
    {
        return countDown ? (timeLimit - currentTime) : currentTime;
    }
    
    public bool IsRunning()
    {
        return isRunning;
    }
}