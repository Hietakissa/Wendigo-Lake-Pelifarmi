using UnityEngine;

public abstract class ClueSO : ScriptableObject
{
    public abstract ClueType GetClueType { get; }
    [field: SerializeField] public int ID { get; private set; }
}

public enum ClueType
{
    Image,
    Text
}