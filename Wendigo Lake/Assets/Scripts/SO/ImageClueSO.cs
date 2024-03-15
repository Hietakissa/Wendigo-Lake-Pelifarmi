using UnityEngine;

[CreateAssetMenu(menuName = "Game/Image Clue", fileName = "New Image Clue")]
public class ImageClueSO : ClueSO
{
    public override ClueType GetClueType => ClueType.Image;
    [SerializeField] float imageTest;
}
