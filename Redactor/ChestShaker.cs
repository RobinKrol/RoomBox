using UnityEngine;
using System.Collections;

public class ChestShaker : MonoBehaviour
{
    public ChestClickHandler chestClickHandler;
    public void ShakeChest(float duration = 0.7f, float magnitude = 3f)
    {
        if (chestClickHandler != null)
        chestClickHandler.DisableChest(duration);

    StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        Quaternion originalRotation = transform.localRotation;

        while (elapsed < duration)
        {
        float damper = 1f - (elapsed / duration);
        float z = Mathf.Sin(elapsed * 20f) * magnitude * damper;
        transform.localRotation = originalRotation * Quaternion.Euler(0, 0, z);
        elapsed += Time.deltaTime;
        yield return null;
        }

        Vector3 angles = transform.localEulerAngles;
        angles.z = -6.63f;
        transform.localEulerAngles = angles;
    }
}
