using UnityEngine;

[CreateAssetMenu(menuName = "Game/Text Clue", fileName = "New Text Clue")]
public class TextClueSO : ClueSO
{
    public override ClueType GetClueType => ClueType.Text;
    [SerializeField] float textTest;
}
