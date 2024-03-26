using UnityEngine;

[CreateAssetMenu(menuName = "Game/Expression", fileName = "New Expression")]
public class ExpressionSO : ScriptableObject
{
    [field: SerializeField] public Sprite ExpressionSprite;
}
