using UnityEngine.EventSystems;
using UnityEngine;

public class CreditsOtherHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Transform holder;

    public void OnPointerEnter(PointerEventData eventData)
    {
        holder.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        holder.gameObject.SetActive(false);
    }
}
