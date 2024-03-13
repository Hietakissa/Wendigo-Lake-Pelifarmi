using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [field: SerializeField] public Transform PlayerCameraTransform { get; private set; }
    public List<PhotographableObject> photographableObjects { get; private set; }


    void Awake()
    {
        Instance = this;

        photographableObjects = new List<PhotographableObject>();
    }

    public void RegisterPhotographableObject(PhotographableObject photographableObject) => photographableObjects.Add(photographableObject);
    public void UnregisterPhotographableObject(int objectID) => photographableObjects.RemoveAt(objectID);
}
