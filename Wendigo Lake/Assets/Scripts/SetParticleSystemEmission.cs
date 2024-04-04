using UnityEngine;

public class SetParticleSystemEmission : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;
    float startEmission = 4f;

    void Awake()
    {
        //startEmission = ps.emission.rateOverTime.constant;

        SetEmission(false);
    }

    public void SetEmission(bool state)
    {
        ParticleSystem.EmissionModule emission = ps.emission;
        //emission.enabled = state;
        if (state)
        {
            emission.rateOverTime = startEmission;
            Debug.Log($"setting rateovertime to: {startEmission}");
        }
        else
        {
            emission.rateOverTime = startEmission * 0.1f;
            Debug.Log($"setting rateovertime to: {startEmission * 0.1f}");
        }

        Debug.Log($"Set emission to: {emission.rateOverTime.constant}, startemission: {startEmission}, {gameObject.name}, parent: {transform.parent.name}");
    }
}
