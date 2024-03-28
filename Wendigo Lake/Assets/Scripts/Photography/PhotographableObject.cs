using UnityEngine.Events;
using UnityEngine;

public class PhotographableObject : MonoBehaviour, IPhotographable
{
    [field: SerializeField] public Transform[] LOSChecks { get; private set; }
    [SerializeField] UnityEvent<ImageParams> onPhotographed;
    //public int ID { get; private set; }


    void Start()
    {
        GameManager.Instance.RegisterPhotographableObject(this);
    }

    public void CapturedInImage(/*int objectID, */in ImageParams imageParams)
    {
        //ID = objectID;
        //Destroy(gameObject);
        //GameManager.Instance.UnregisterPhotographableObject(objectID);
        onPhotographed?.Invoke(imageParams);
    }
}
