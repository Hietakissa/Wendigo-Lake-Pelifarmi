using UnityEngine;

[CreateAssetMenu(menuName = "Game/Monologue", fileName = "New TextCollection")]
public class TextCollectionSO : ScriptableObject
{
    [field: SerializeField] public TextCollectionMode mode;
    [field: SerializeField] [TextArea(1, 5)] public string[] texts;
}

public enum TextCollectionMode
{
    Random,
    Sequential
}
