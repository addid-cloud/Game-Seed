using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("UI")]
    public Image background;
    public Image itemIcon;

    [Header("Sprites")]
    public Sprite emptySprite;
    public Sprite filledSprite;
    public Sprite hoverSprite;

    [Header("Hover Animation")]
    public float hoverScale = 1.08f;
    public float hoverMoveY = 8f;
    public float animationSpeed = 8f;

    private bool hasItem = false;

    private Vector3 originalScale;
    private Vector3 originalPosition;

    private Vector3 targetScale;
    private Vector3 targetPosition;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        targetScale = originalScale;
        targetPosition = originalPosition;

        background.sprite = emptySprite;
        itemIcon.enabled = false;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * animationSpeed);

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * animationSpeed);
    }

    public void SetItem(Sprite item)
    {
        hasItem = true;

        background.sprite = filledSprite;

        itemIcon.sprite = item;
        itemIcon.enabled = true;
    }

    public void RemoveItem()
    {
        hasItem = false;

        background.sprite = emptySprite;

        itemIcon.enabled = false;

        targetScale = originalScale;
        targetPosition = originalPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!hasItem) return;

        background.sprite = hoverSprite;

        targetScale = originalScale * hoverScale;
        targetPosition = originalPosition + Vector3.up * hoverMoveY;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!hasItem) return;

        background.sprite = filledSprite;

        targetScale = originalScale;
        targetPosition = originalPosition;
    }
}