using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SVDragHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public InputActionProperty ScrollActionProperty;
    private int pointerID = int.MinValue;

    [SerializeField]
    private Scrollbar VerticalScrollbar;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pointerID != int.MinValue) return;

        // Store the pointer ID as reference
        pointerID = eventData.pointerId;
        ScrollActionProperty.action?.Enable();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Only if the stored pointer ID is the same as this one
        if (eventData.pointerId != pointerID) return;

        // Reset pointer ID reference
        pointerID = int.MinValue;
        ScrollActionProperty.action?.Disable();
    }

    public void Update()
    {
        if (pointerID == int.MinValue)
            return;

        if (ScrollActionProperty.action is null or { enabled: false })
            return;

        var scrollDelta = ScrollActionProperty.action.ReadValue<Vector2>();
        AddScrollDelta(VerticalScrollbar, scrollDelta.y);
    }

    private void AddScrollDelta(Scrollbar bar, float value)
    {
        if (bar == null) return;
        bar.value = Mathf.Clamp(bar.value + value, 0f, 1f);
    }
}

/*
public class SVDragHandler : MonoBehaviour //, IPointerExitHandler
{
    public float scrollTime = 3f;
    public float restartScrollDelay = 1f;

    private ScrollRect scrollRect;
    private bool isScrolling = true;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 1f;
        StartCoroutine(AutoScroll());
    }

    private IEnumerator AutoScroll()
    {
        while (isScrolling)
        {
            StartCoroutine(ScrollDown());

            yield return new WaitForSeconds(scrollTime);

            yield return new WaitForSeconds(restartScrollDelay);

            scrollRect.normalizedPosition = Vector2.zero;
        }
    }

    private IEnumerator ScrollDown()
    {
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = new Vector2(0f, 1f);

        float t = 0f;
        while (t < scrollTime)
        {
            
            scrollRect.normalizedPosition = Vector2.Lerp(startPos, endPos, t / scrollTime);

            t += Time.deltaTime;
            yield return null;
        }
    }
}
*/