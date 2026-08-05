using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public GameObject highlightImage;

    void Start()
    {
        if (highlightImage != null)
            highlightImage.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightImage != null)
            highlightImage.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightImage != null)
            highlightImage.SetActive(false);
    }
}