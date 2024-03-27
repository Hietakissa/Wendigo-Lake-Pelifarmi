using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [field: SerializeField] public Transform PlayerCameraTransform { get; private set; }
    [field: SerializeField] public Transform PlayerTransform { get; private set; }
    public List<PhotographableObject> photographableObjects { get; private set; }

    public void SetPlayerVisibility(float visibility) => PlayerVisibility = visibility;
    public float PlayerVisibility { get; private set; }

    public bool Paused { get; private set; }

    void Awake()
    {
        Instance = this;

        photographableObjects = new List<PhotographableObject>();

        Manager[] managers = GetComponents<Manager>();
        foreach (Manager manager in managers)
        {
            manager.Initialize();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Paused) EventManager.UnPause();
            else EventManager.Pause();
        }
    }


    public static void ShowMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void HideMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void EventManager_OnPause()
    {
        Time.timeScale = float.Epsilon;
        Paused = true;
        ShowMouse();
    }

    void EventManager_OnUnPause()
    {
        Time.timeScale = 1f;
        Paused = false;
        HideMouse();
    }


    public void RegisterPhotographableObject(PhotographableObject photographableObject) => photographableObjects.Add(photographableObject);
    //public void UnregisterPhotographableObject(int objectID) => photographableObjects.RemoveAt(objectID);

    void OnEnable()
    {
        EventManager.OnPause += EventManager_OnPause;
        EventManager.OnUnPause += EventManager_OnUnPause;
    }

    void OnDisable()
    {
        EventManager.OnPause -= EventManager_OnPause;
        EventManager.OnUnPause -= EventManager_OnUnPause;
    }
}
