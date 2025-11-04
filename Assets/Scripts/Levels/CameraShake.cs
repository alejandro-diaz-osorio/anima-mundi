// Reemplazar CameraShake.cs con esta versión corregida:
using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float defaultIntensity = 0.3f;
    public float defaultDuration = 0.2f;
    public bool useRandomRotation = true;
    
    [Header("Advanced Settings")]
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float positionShakeMultiplier = 1f;
    public float rotationShakeMultiplier = 0.1f;
    
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private bool isShaking = false;
    private CameraMovement cameraMovement;
    
    void Start()
    {
        // CRÍTICO: Usar localPosition, no position
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        cameraMovement = GetComponent<CameraMovement>();
        
        if (cameraMovement == null)
        {
            Debug.LogWarning("CameraShake works best with CameraMovement component");
        }
    }
    
    void LateUpdate()
    {
        if (!isShaking)
        {
            // Smoothly return to zero offset (CameraMovement handles actual position)
            if (cameraMovement == null)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, Time.deltaTime * 5f);
                
                if (!useRandomRotation)
                {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, originalLocalRotation, Time.deltaTime * 5f);
                }
            }
        }
    }
    
    public void Shake()
    {
        Shake(defaultIntensity, defaultDuration);
    }
    
    public void Shake(float intensity, float duration)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(intensity, duration));
        }
    }
    
    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        isShaking = true;
        
        // Store current local position at start of shake
        Vector3 startLocalPosition = transform.localPosition;
        Quaternion startLocalRotation = transform.localRotation;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percentComplete = elapsed / duration;
            float currentIntensity = intensity * intensityCurve.Evaluate(percentComplete);
            
            // CRITICAL FIX: Add offset to current position, don't set absolute
            Vector3 randomOffset = Random.insideUnitSphere * currentIntensity * positionShakeMultiplier;
            
            if (cameraMovement != null)
            {
                // With CameraMovement, just apply small local offset
                transform.localPosition = randomOffset;
            }
            else
            {
                // Without CameraMovement, offset from start position
                transform.localPosition = startLocalPosition + randomOffset;
            }
            
            // Rotation shake
            if (useRandomRotation)
            {
                float rotationIntensity = currentIntensity * rotationShakeMultiplier * 10f;
                Vector3 randomRotation = new Vector3(
                    Random.Range(-rotationIntensity, rotationIntensity),
                    Random.Range(-rotationIntensity, rotationIntensity),
                    Random.Range(-rotationIntensity, rotationIntensity)
                );
                transform.localRotation = startLocalRotation * Quaternion.Euler(randomRotation);
            }
            
            yield return null;
        }
        
        // Reset to zero offset
        transform.localPosition = cameraMovement != null ? Vector3.zero : startLocalPosition;
        
        if (useRandomRotation)
        {
            transform.localRotation = startLocalRotation;
        }
        
        isShaking = false;
    }
    
    public void StopShake()
    {
        StopAllCoroutines();
        transform.localPosition = cameraMovement != null ? Vector3.zero : originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        isShaking = false;
    }
    
    public bool IsShaking()
    {
        return isShaking;
    }
}