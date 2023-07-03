using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MenuElement : MonoBehaviour
{
    // Public Variables 
     
    public MenuArea menuArea;

    [Space(20)]
    public MenuArea.Menu menuLayer;

    [Space(20)]
    public Tooltip actionSelectTooltip;
    
    [Space(20)]
    public UnityEvent selectActions;

    [Space(20)]
    [Header("Hover Scaling")]
    public bool resizeOnHover = true;
    [Range(0.1f, 0.3f)]
    public float scaleAnimDuration = 0.1f;    
    [Range(0.01f, 0.05f)]
    public float itemScale = 0.03f;
    [Range(0.01f, 0.05f)]
    public float itemHoverScale = 0.05f;



    // Private Variables  

    private Collider col;
    private TooltipHandler tooltipHandler;
    private bool isAnimating = false;



    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
        DisableCollider();
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
            menuArea.SetHoveredMenuItem(gameObject);
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
            menuArea.UnsetHoveredMenuItem();
        }
    }

    private IEnumerator ScaleObject(bool scaleUp)
    {
        isAnimating = true;
        float t = 0f;

        Vector3 scale = Vector3.one * itemScale;
        Vector3 hoverScale = Vector3.one * itemHoverScale;

        while (t < scaleAnimDuration)
        {
            if (scaleUp)
                transform.localScale = Vector3.Lerp(scale, hoverScale, t / scaleAnimDuration);
            else
                transform.localScale = Vector3.Lerp(hoverScale, scale, t / scaleAnimDuration);

            t += Time.deltaTime;
            yield return null;
        }

        isAnimating = false;
    }

    public void SelectMenuAction()
    {
        menuArea.OnMenuItemSelect(this);
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
