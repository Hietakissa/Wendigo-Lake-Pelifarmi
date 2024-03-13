using UnityEngine.Events;
using UnityEngine;

public class CameraEnterExitDetector : MonoBehaviour
{
    [SerializeField] UnityEvent onEntered;
    [SerializeField] UnityEvent onExit;


    void EventManager_OnEnteredCamera() => onEntered?.Invoke();
    void EventManager_OnExitCamera() => onExit?.Invoke();


    void OnEnable()
    {
        EventManager.OnEnteredCamera += EventManager_OnEnteredCamera;
        EventManager.OnExitCamera += EventManager_OnExitCamera;
    }

    void OnDisable()
    {
        EventManager.OnEnteredCamera -= EventManager_OnEnteredCamera;
        EventManager.OnExitCamera -= EventManager_OnExitCamera;
    }
}
