using UnityEngine.Events;
using UnityEngine;

public class PhotographableObject : MonoBehaviour, IPhotographable
{
    [SerializeField] UnityEvent onPhotographed;
    public int ID { get; private set; }

    void Start()
    {
        GameManager.Instance.RegisterPhotographableObject(this);
    }

    public void CapturedInImage(int objectID)
    {
        ID = objectID;
        //Destroy(gameObject);
        //GameManager.Instance.UnregisterPhotographableObject(objectID);
        onPhotographed?.Invoke();
    }
}
