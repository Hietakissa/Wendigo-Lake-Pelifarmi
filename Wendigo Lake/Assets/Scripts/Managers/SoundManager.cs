using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using UnityEngine;

public class SoundManager : Manager
{
    Queue<AudioSource> audioSourceQueue = new Queue<AudioSource>();

    [SerializeField][Range(0f, 1f)] float pitchDeviation;

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


    float GetPitch()
    {
        const float CONST_BASEPITCH = 1f;

        float deviation = CONST_BASEPITCH * pitchDeviation;
        return CONST_BASEPITCH + (Random.Range(-deviation, deviation));
    }

    void EventManager_OnPlaySoundAtPosition(SoundCollectionSO soundCollection, Vector3 position, bool positional)
    {
        if (soundCollection.TryGetAudioClip(out AudioClip clip))
        {
            AudioSource source = GetAudioSource();
            source.transform.position = position;
            source.spatialBlend = positional ? 1f : 0f;

            source.pitch = GetPitch();
            source.clip = clip;
            
            source.Play();


            StartCoroutine(ReturnToQueueCor(source, soundCollection.LongestClipLength));
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
