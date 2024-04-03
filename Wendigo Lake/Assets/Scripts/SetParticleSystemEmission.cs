using UnityEngine;

public class SetParticleSystemEmission : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;
    float startEmission;

    void Awake()
    {
        startEmission = ps.emission.rateOverTime.constant;

        SetEmission(false);
    }

    public void SetEmission(bool state)
    {
        ParticleSystem.EmissionModule emission = ps.emission;
        //emission.enabled = state;
        if (state) emission.rateOverTime = startEmission;
        else emission.rateOverTime = startEmission * 0.1f;
    }
}
