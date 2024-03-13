using UnityEngine;

public class SetParticleSystemEmission : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;

    public void SetEmission(bool state)
    {
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.enabled = state;
    }
}
