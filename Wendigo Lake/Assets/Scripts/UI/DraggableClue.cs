using UnityEngine.EventSystems;
using UnityEngine;

public class DraggableClue : MonoBehaviour, IDragHandler, IBeginDragHandler,  IEndDragHandler
{
    [field: SerializeField] public ClueSO clueData { get; private set; }
    Vector2 dragOffset;


    void Start()
    {
        EventManager.UI.RegisterDraggableClue(this);
    }


    void SetPosition(Vector2 mousePos)
    {
        transform.position = mousePos + dragOffset;
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetPosition(eventData.position);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragOffset = (Vector2)transform.position - eventData.position;
        SetPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EventManager.UI.EndDrag(this);
    }
}
