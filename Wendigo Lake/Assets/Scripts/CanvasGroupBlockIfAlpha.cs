using UnityEngine;

public class CanvasGroupBlockIfAlpha : MonoBehaviour
{
    CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (Mathf.Approximately(canvasGroup.alpha, 0f)) canvasGroup.blocksRaycasts = false;
        else canvasGroup.blocksRaycasts = true;
    }
}
