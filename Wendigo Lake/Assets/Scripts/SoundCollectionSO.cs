using UnityEngine.Audio;
using HietakissaUtils;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Sound Collection", fileName = "New Sound Collection")]
public class SoundCollectionSO : ScriptableObject
{
    [field: SerializeField] public SoundCollectionType SoundCollectionType { get; private set; } = SoundCollectionType.Random;
    [field: SerializeField] public AudioMixerGroup MixerGroup { get; private set; }
    [SerializeField] Sound[] sounds;

    public bool TryGetSound(out Sound sound)
    {
        sound = sounds.RandomElement();

        if (sound != null) return true;
        else return false;
    }

    public bool TryGetSounds(out Sound[] sounds)
    {
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
