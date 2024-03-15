using UnityEngine.EventSystems;
using UnityEngine;

public class DraggableClue : MonoBehaviour, IDragHandler, IBeginDragHandler,  IEndDragHandler
{
    [field: SerializeField] public ClueSO clue { get; private set; }
    Vector2 offset;


    void Start()
    {
        EventManager.UI.RegisterDraggableClue(this);
    }


    void SetPosition(Vector2 mousePos)
    {
        transform.position = mousePos + offset;
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetPosition(eventData.position);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = (Vector2)transform.position - eventData.position;
        SetPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EventManager.UI.EndDrag(this);
    }
}
