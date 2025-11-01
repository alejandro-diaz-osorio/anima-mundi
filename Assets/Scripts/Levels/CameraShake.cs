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
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isShaking = false;
    private CameraMovement cameraMovement;
    
    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        cameraMovement = GetComponent<CameraMovement>();
    }
    
    void LateUpdate()
    {
        if (!isShaking)
        {
            // Smoothly return to original position
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * 5f);
            
            if (!useRandomRotation)
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, Time.deltaTime * 5f);
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
        
        // Store original position if camera movement exists
        bool hadCameraMovement = false;
        if (cameraMovement != null)
        {
            hadCameraMovement = true;
            originalPosition = transform.localPosition;
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percentComplete = elapsed / duration;
            float currentIntensity = intensity * intensityCurve.Evaluate(percentComplete);
            
            // Position shake
            Vector3 randomOffset = Random.insideUnitSphere * currentIntensity * positionShakeMultiplier;
            transform.localPosition = originalPosition + randomOffset;
            
            // Rotation shake
            if (useRandomRotation)
            {
                float rotationIntensity = currentIntensity * rotationShakeMultiplier * 10f;
                Vector3 randomRotation = new Vector3(
                    Random.Range(-rotationIntensity, rotationIntensity),
                    Random.Range(-rotationIntensity, rotationIntensity),
                    Random.Range(-rotationIntensity, rotationIntensity)
                );
                transform.localRotation = originalRotation * Quaternion.Euler(randomRotation);
            }
            
            yield return null;
        }
        
        // Reset
        transform.localPosition = originalPosition;
        if (useRandomRotation)
        {
            transform.localRotation = originalRotation;
        }
        
        isShaking = false;
    }
    
    public void StopShake()
    {
        StopAllCoroutines();
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        isShaking = false;
    }
    
    public bool IsShaking()
    {
        return isShaking;
    }
}