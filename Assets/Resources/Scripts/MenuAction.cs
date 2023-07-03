using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MenuAction : MonoBehaviour
{
    /* Public Variables */
    public GestureMenu gestureMenu;
    [Space(20)]
    public GestureMenu.Menu menuLayer;

    // public UnityEvent actionHoverEnteredEvents;
    // public UnityEvent actionHoverExitedEvents;
    [Space(20)]
    public Tooltip actionSelectTooltip;
    public bool resizeOnHover = true;
    [Space(20)]
    [Range(0f, 0.3f)]
    public float scaleAnimDuration = 0.1f;
    [Space(20)]
    public UnityEvent selectActions;
    [Space(20)]
    public Vector3 menuActionScale;
    public Vector3 menuActionHoverScale;

    /* Private Variables */
    // Interaction animation duration 
    [SerializeField, Range(0f, 0.3f)] private float animDuration = 0.1f;

    // Radius of the circle arrangement of the child spheres
    [SerializeField] private float circleRadius = 2f;

    private Collider col;
    private bool isSelected = false;
    private GameObject parent;
    private Vector3 actionPosition;
    private TooltipHandler tooltipHandler;

    private bool isAnimating = false;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
        DisableCollider();

        // parent = transform.parent.gameObject;
        // StartCoroutine(TranslateToZeroPosition());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<XRDirectInteractor>() != null)
        {
            // Show select action tooltip
            tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
            tooltipHandler.ShowTooltip(actionSelectTooltip);

            // Scale the sphere up
            if (!isAnimating && resizeOnHover)
                StartCoroutine(ScaleObject(true));

            // Set currently hovered item in GestureMenu
            gestureMenu.SetHoveredMenuItem(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<XRDirectInteractor>() != null)
        {
            // Hide select action tooltip
            tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
            tooltipHandler.HideTooltip(actionSelectTooltip);

            // Scale the sphere down
            if (!isAnimating && resizeOnHover)
                StartCoroutine(ScaleObject(false));

            // Unset currently hovered item in GestureMenu
            gestureMenu.UnsetHoveredMenuItem();
        }
    }

    private IEnumerator ScaleObject(bool scaleUp)
    {
        isAnimating = true;
        float t = 0f;

        while (t < scaleAnimDuration)
        {
            if (scaleUp)
                transform.localScale = Vector3.Lerp(menuActionScale, menuActionHoverScale, t / scaleAnimDuration);
            else
                transform.localScale = Vector3.Lerp(menuActionHoverScale, menuActionScale, t / scaleAnimDuration);

            t += Time.deltaTime;
            yield return null;
        }

        isAnimating = false;
    }

    public void EnableCollider()
    {
        if (col == null)
            col = GetComponent<Collider>();

        if (!col.enabled)
            col.enabled = true;
    }

    public void DisableCollider()
    {
        if (col == null)
            col = GetComponent<Collider>();

        if (col.enabled)
            col.enabled = false;
    }

    public void DisableMenuItem()
    {
        StartCoroutine(ScaleMenuItemToZero());

        DisableCollider();
    }

    private IEnumerator ScaleMenuItemToZero()
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;

        float t = 0f;
        while (t < scaleAnimDuration)
        {
            // Animate scale
            transform.localScale = Vector3.Lerp(startScale, endScale, t / scaleAnimDuration);

            t += Time.deltaTime;
            yield return null;
        }
    }
}
