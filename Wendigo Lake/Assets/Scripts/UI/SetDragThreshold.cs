using UnityEngine.EventSystems;
using HietakissaUtils;
using UnityEngine;

public class SetDragThreshold : MonoBehaviour
{
    [SerializeField] float dragThreshold;
    [SerializeField] float referenceDPI;

    EventSystem eventSystem;

    void Awake()
    {
        SetThreshold();
    }

    void OnValidate()
    {
        SetThreshold();
    }

    void SetThreshold()
    {
        if (!eventSystem) eventSystem = GetComponent<EventSystem>();

        float DPIMultiplier = referenceDPI / Screen.dpi;
        int correctedDragThreshold = (DPIMultiplier * dragThreshold).RoundDown();

        eventSystem.pixelDragThreshold = correctedDragThreshold;
    }
}
