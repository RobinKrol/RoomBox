using UnityEngine;
using System.Collections;

public class ChestLidAnimator : MonoBehaviour
{
    public GameObject glowEffect;
    public float openAngle = 50f;      
    public float openSpeed = 2f;       
    public bool isOpen = false;        

    private float currentAngle = 0f;   

    void Update()
    {
        float targetAngle = isOpen ? openAngle : 0f;
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * openSpeed);

        transform.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);
    }
    public void OpenLid()
    {
        isOpen = true;
        if (glowEffect != null)
            glowEffect.SetActive(true);
    }

    public void CloseLid()
    {
        isOpen = false;
        if (glowEffect != null)
            glowEffect.SetActive(false);
    }

    public void ShakeChest(float duration = 0.5f, float magnitude = 10f)
    {
    StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
{
    float elapsed = 0f;
    Vector3 originalRotation = transform.localEulerAngles;

    while (elapsed < duration)
    {
        float z = Random.Range(-magnitude, magnitude);
        transform.localEulerAngles = originalRotation + new Vector3(0, 0, z);
        elapsed += Time.deltaTime;
        yield return null;
    }

    transform.localEulerAngles = originalRotation;
}
}

