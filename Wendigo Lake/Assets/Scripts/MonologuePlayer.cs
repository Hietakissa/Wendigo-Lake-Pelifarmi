using UnityEngine;

public class MonologuePlayer : MonoBehaviour
{
    [SerializeField] TextCollectionSO monologueText;

    public void PlayMonologue()
    {
        EventManager.PlayMonologue(monologueText);
    }
}
