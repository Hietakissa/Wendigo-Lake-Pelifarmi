using HietakissaUtils;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Sound Collection", fileName = "New Sound Collection")]
public class SoundCollectionSO : ScriptableObject
{
    [SerializeField] Sound[] sounds;

    public bool TryGetSound(out Sound sound)
    {
        sound = sounds.RandomElement();

        if (sound != null) return true;
        else return false;
    }
}
