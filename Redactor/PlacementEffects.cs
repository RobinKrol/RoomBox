using UnityEngine;

public class PlacementEffects : MonoBehaviour
{
    [Header("Визуальные эффекты")]
    public GameObject placementParticleEffect;
    public float particleEffectDuration = 2f;
    
    [Header("Анимация")]
    public float placementAnimationDuration = 0.5f;
    public AnimationCurve placementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Vector3 originalScale;
    private bool isAnimating = false;
    
    void Awake()
    {
        originalScale = transform.localScale;
    }
    
    public void PlayPlacementEffect()
    {
        // Анимация появления
        StartCoroutine(PlacementAnimation());
        
        // Частицы
        if (placementParticleEffect != null)
        {
            GameObject particles = Instantiate(placementParticleEffect, transform.position, transform.rotation);
            Destroy(particles, particleEffectDuration);
        }
    }
    
    private System.Collections.IEnumerator PlacementAnimation()
    {
        if (isAnimating) yield break;
        
        isAnimating = true;
        float elapsed = 0f;
        
        // Начинаем с нулевого размера
        transform.localScale = Vector3.zero;
        
        while (elapsed < placementAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / placementAnimationDuration;
            float curveValue = placementCurve.Evaluate(progress);
            
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, curveValue);
            
            yield return null;
        }
        
        transform.localScale = originalScale;
        isAnimating = false;
    }
} 