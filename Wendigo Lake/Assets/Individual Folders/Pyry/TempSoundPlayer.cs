using UnityEngine;

public class TempSoundPlayer : MonoBehaviour
{
    [SerializeField] SoundCollectionSO sounds;
    [SerializeField] float delay;

    float time;

    void Update()
    {
        time += Time.deltaTime;
        if (time > delay)
        {
            EventManager.PlaySoundAtPosition(sounds, transform.position);
            time -= delay;
        }
    }
}
