using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    /*
    public InputActionReference uiClickAction;

    public static GameObject itemBeingDragged;

    public static bool isCustomerDragged;

    public Transform customerScrollRect;
    public Transform dragParent;

    public float holdTime;
    public float maxScrollVelocityInDrag;

    private Transform startParent;

    private ScrollRect scrollRect;

    private float timer;

    private bool isHolding;
    private bool canDrag;
    private bool isPointerOverGameObject;

    private CanvasGroup canvasGroup;

    private Vector3 startPos;

    public Transform StartParent
    {
        get { return startParent; }
    }

    public Vector3 StartPos
    {
        get { return startPos; }
    }

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Use this for initialization
    void Start()
    {
        timer = holdTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (uiClickAction.action.WasPressedThisFrame())
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                //Debug.Log("Mouse Button Down");
                scrollRect = customerScrollRect.GetComponent<ScrollRect>();
                isPointerOverGameObject = true;
                isHolding = true;
                StartCoroutine(Holding());
            }
        }

        if (uiClickAction.action.WasReleasedThisFrame())
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                //Debug.Log("Mouse Button Up");
                isHolding = false;

                if (canDrag)
                {
                    itemBeingDragged = null;
                    isCustomerDragged = false;
                    if (transform.parent == dragParent)
                    {
                        canvasGroup.blocksRaycasts = true;
                        transform.SetParent(startParent);
                        transform.localPosition = startPos;
                    }
                    canDrag = false;
                    timer = holdTime;
                }
            }
        }

        if (uiClickAction.action.WasPerformedThisFrame())
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                if (canDrag)
                {
                    //Debug.Log("Mouse Button");
                    transform.position = Input.mousePosition;
                }
                else
                {
                    if (!isPointerOverGameObject)
                    {
                        isHolding = false;
                    }
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOverGameObject = false;
    }

    IEnumerator Holding()
    {
        while (timer > 0)
        {
            if (scrollRect.velocity.x >= maxScrollVelocityInDrag)
            {
                isHolding = false;
            }

            if (!isHolding)
            {
                timer = holdTime;
                yield break;
            }

            timer -= Time.deltaTime;
            //Debug.Log("Time : " + timer);
            yield return null;
        }

        isCustomerDragged = true;
        itemBeingDragged = gameObject;
        startPos = transform.localPosition;
        startParent = transform.parent;
        canDrag = true;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(dragParent);
    }

    public void Reset()
    {
        isHolding = false;
        canDrag = false;
        isPointerOverGameObject = false;
    }
    */
}
