using UnityEngine;

public class PhotographableObject : MonoBehaviour, IPhotographable
{
    void Start()
    {
        GameManager.Instance.RegisterPhotographableObject(this);
    }

    public void CapturedInImage(int objectID)
    {
        Destroy(gameObject);
        GameManager.Instance.UnregisterPhotographableObject(objectID);
    }
}
