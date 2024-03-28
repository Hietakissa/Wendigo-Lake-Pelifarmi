using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using UnityEngine;
using HietakissaUtils;

public class SoundManager : Manager
{
    Queue<AudioSource> audioSourceQueue = new Queue<AudioSource>();

    int highestID;


    AudioSource GetAudioSource()
    {
        if (audioSourceQueue.Count == 0)
        {
            CreateAudioSource();
        }

        return audioSourceQueue.Dequeue();
    }
    IEnumerator ReturnToQueueCor(AudioSource source, int time)
    {
        yield return QOL.GetWaitForSeconds(time);
        audioSourceQueue.Enqueue(source);
    }
    void CreateAudioSource()
    {
        GameObject go = new GameObject($"Pooled Audio ({highestID++})");
        go.transform.parent = transform;
        audioSourceQueue.Enqueue(go.AddComponent<AudioSource>());
    }


    float GetPitch(float minDeviation, float maxDeviation)
    {
        const float CONST_BASEPITCH = 1f;

        return CONST_BASEPITCH + (Random.Range(CONST_BASEPITCH * minDeviation, CONST_BASEPITCH * maxDeviation));
    }

    void EventManager_OnPlaySoundAtPosition(SoundCollectionSO soundCollection, Vector3 position)
    {
        if (soundCollection.TryGetSound(out Sound sound))
        {
            AudioSource source = GetAudioSource();
            source.transform.position = position;
            source.spatialBlend = sound.Type == AudioType.Positional ? 1f : 0f;

            source.pitch = GetPitch(sound.MinPitchDeviation, sound.MaxPitchDeviation);
            source.volume = sound.Volume * Random.Range(0.95f, 1.05f);
            source.clip = sound.Clip;
            
            source.Play();

            int normalizedClipTime = sound.Clip.length.RoundUp();
            StartCoroutine(ReturnToQueueCor(source, normalizedClipTime));
        }
    }


    void OnEnable()
    {
        EventManager.OnPlaySoundAtPosition += EventManager_OnPlaySoundAtPosition;
    }

    void OnDisable()
    {
        EventManager.OnPlaySoundAtPosition -= EventManager_OnPlaySoundAtPosition;
    }
}



[System.Serializable]
public class Sound
{
    [field: SerializeField] public AudioType Type = AudioType.Positional;
    [field: SerializeField][Range(0f, 1f)] public float Volume = 1f;
    [field: SerializeField][Range(-1f, 0f)] public float MinPitchDeviation = -0.1f;
    [field: SerializeField][Range(0f, 1f)] public float MaxPitchDeviation = 0.1f;
    [field: SerializeField] public AudioClip Clip;

    Sound() { } // Unity can't properly serialize the class with the correct default values without the empty constructor
}

public enum AudioType
{
    NonPositional,
    Positional
}
