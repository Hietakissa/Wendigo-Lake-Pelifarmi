using System.Collections.Generic;
using HietakissaUtils;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Sound Collection", fileName = "New Sound Collection")]
public class SoundCollectionSO : ScriptableObject
{
    [field: SerializeField] public SoundCollectionType SoundCollectionType = SoundCollectionType.Random;
    [SerializeField] Sound[] sounds;

    public bool TryGetSound(out Sound sound)
    {
        sound = sounds.RandomElement();

        if (sound != null) return true;
        else return false;
    }

    public bool TryGetSounds(out Sound[] sounds)
    {
        Debug.Log($"Get sounds");
        //if (soundList == null)
        //{
        //    List<Sound> tempSoundList = new List<Sound>();
        //    foreach (Sound sound in this.sounds) if (sound != null) tempSoundList.Add(sound);
        //    soundList = tempSoundList.ToArray();
        //
        //    Debug.Log($"first time, created array with length of {soundList.Length}");
        //}

        sounds = this.sounds;
        if (sounds.Length == 0) return false;
        else return true;
    }
}

public enum SoundCollectionType
{
    None,
    Random,
    All
}
