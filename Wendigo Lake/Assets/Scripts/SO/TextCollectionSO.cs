using UnityEngine;

[CreateAssetMenu(menuName = "Game/Dialogue", fileName = "New Dialogue")]
public class TextCollectionSO : ScriptableObject
{
    [field: SerializeField] public TextCollectionMode mode;
    [field: SerializeField] public DialogueElement[] dialogue;
}

[System.Serializable]
public class DialogueElement
{
    [field: SerializeField] public Person speaker { get; private set; }
    [field: SerializeField] [TextArea(1, 5)] string text;
    public string Text => text;
}

public enum TextCollectionMode
{
    Random,
    Sequential
}

public enum Person
{
    None,
    Me,
    Delilah
}
