using UnityEngine;

public class MonologuePlayer : MonoBehaviour
{
    [SerializeField] TextCollectionSO monologueText;

    public void PlayMonologue()
    {
        EventManager.UI.PlayDialogue(monologueText);
    }
}
