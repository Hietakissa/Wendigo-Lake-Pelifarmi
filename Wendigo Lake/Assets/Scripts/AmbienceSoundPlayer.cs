using UnityEngine;

public class AmbienceSoundPlayer : MonoBehaviour
{
    [SerializeField] SoundCollectionSO soundCollection;
    [SerializeField] float minDelay = 1f;
    [SerializeField] float maxDelay = 3f;
    float currentTime;
    float delay;

    void Awake() => RandomizeDelay();
    void RandomizeDelay() => delay = Random.Range(minDelay, maxDelay);


    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= delay)
        {
            currentTime -= delay;

            PlaySound();
            RandomizeDelay();
        }
    }

    void PlaySound()
    {
        EventManager.PlaySoundAtPosition(soundCollection, transform.position);
    }
}
