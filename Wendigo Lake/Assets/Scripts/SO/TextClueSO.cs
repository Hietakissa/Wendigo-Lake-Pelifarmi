using UnityEngine;

[CreateAssetMenu(menuName = "Game/Text Clue", fileName = "New Text Clue")]
public class TextClueSO : ClueSO
{
    public override ClueType GetClueType => ClueType.Text;
    public string Text => text;
    [SerializeField] [TextArea(1, 5)] string text;
}
