using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.15f;

    private float currentShakeTime = 0f;
    private Vector3 shakeOrigin;
    private bool isShaking;

    void LateUpdate()
    {
        if (!isShaking)
            return;

        currentShakeTime -= Time.unscaledDeltaTime;

        if (currentShakeTime <= 0f)
        {
            transform.position = shakeOrigin;
            isShaking = false;
            return;
        }

        float fade = currentShakeTime / shakeDuration;
        Vector3 offset = Random.insideUnitSphere * shakeMagnitude * fade;
        offset.z = 0f;

        transform.position = shakeOrigin + offset;
    }

    public void Shake()
    {
        if (isShaking)
            transform.position = shakeOrigin;

        shakeOrigin = transform.position;
        currentShakeTime = Mathf.Max(0f, shakeDuration);
        isShaking = currentShakeTime > 0f;
    }
}
