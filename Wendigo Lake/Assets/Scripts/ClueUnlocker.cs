using UnityEngine;

public class ClueUnlocker : MonoBehaviour
{
    [SerializeField] ClueSO clue;

    public void UnlockClue()
    {
        EventManager.UI.UnlockClue(clue);
    }
}
