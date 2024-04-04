using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // very bad code but no time left
        if (other.gameObject.name == "Player" && GameManager.Instance.HasCollectedClues) EventManager.WonGame();
    }
}
