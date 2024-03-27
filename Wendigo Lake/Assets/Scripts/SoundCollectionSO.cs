using HietakissaUtils;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Sound Collection", fileName = "New Sound Collection")]
public class SoundCollectionSO : ScriptableObject
{
    [field: SerializeField] public int LongestClipLength;
    [SerializeField] AudioClip[] Sounds;

    public bool TryGetAudioClip(out AudioClip audioClip)
    {
        audioClip = Sounds.RandomElement();

        if (audioClip != null) return true;
        else return false;
    }
}
