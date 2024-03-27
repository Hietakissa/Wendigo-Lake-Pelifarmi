using UnityEngine;

public class Campfire : MonoBehaviour
{
    [SerializeField] Light fireLight;
    [SerializeField] float lightMoveSpeed;
    [SerializeField] float lightMoveDistance;
    [SerializeField] float baseIntensity;
    [SerializeField] float intensityFrequency;
    [SerializeField] float intensityMagnitude;

    [Header("Other")]
    [SerializeField] Collider[] colliders;

    Vector3 lightVelocity;
    Vector3 lightTargetPos;
    Vector3 lightOrigin;
    float lightPosThreshold = 0.05f;


    void Awake()
    {
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = true;
        }

        lightOrigin = fireLight.transform.position;
        SetRandomLightTarget();
    }

    void Update()
    {
        AnimateLight();
    }


    void AnimateLight()
    {
        fireLight.transform.position = Vector3.SmoothDamp(fireLight.transform.position, lightTargetPos, ref lightVelocity, lightMoveSpeed);

        if (Vector3.Distance(fireLight.transform.position, lightTargetPos) < lightPosThreshold) SetRandomLightTarget();
        //fireLight.intensity = baseIntensity + Mathf.Sin(intensityFrequency * Time.time) * intensityMagnitude;
        float intensity = baseIntensity + (CalculateIntensity(1) + CalculateIntensity(2) + CalculateIntensity(8));
        fireLight.intensity = intensity;
        fireLight.range = intensity * 0.5f;
    }

    void SetRandomLightTarget()
    {
        Vector3 pos = lightOrigin + Random.insideUnitSphere.normalized * lightMoveDistance;
        pos.y = Mathf.Max(lightOrigin.y, pos.y);
        lightTargetPos = pos;
    }

    float CalculateIntensity(int cascade)
    {
        float cascadeMult = (1f / cascade);
        //return Mathf.Sin(intensityFrequency * cascadeMult * Time.time) * intensityMagnitude * cascade;
        return Mathf.PingPong(intensityFrequency * cascadeMult * Time.time, 1f) * intensityMagnitude / cascade;
    }
}
