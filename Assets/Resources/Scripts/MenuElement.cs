using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MenuElement : MonoBehaviour
{
    // Public Variables

    public XROffsetGrabInteractable xrOGI;
    public ExhibitInformation exhibitInfo;
    public bool hasSubMenu = false;
    [Space(20)]
    public Tooltip actionSelectTooltip;
    [Space(20)]
    [Range(0f, 0.3f)]
    public float scaleAnimDuration = 0.1f;
    [Space(20)]
    public Vector3 menuActionScale;
    public Vector3 menuActionHoverScale;


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

    public void EnableCollider()
    {
        if (!col.enabled)
            col.enabled = true;
    }

    public void DisableCollider()
    {
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<XRDirectInteractor>() != null)
        {
            // Show select action tooltip
            tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
            tooltipHandler.ShowTooltip(actionSelectTooltip);

            // Scale the sphere up
            if (!isAnimating)
                StartCoroutine(ScaleObject(true));

            // Set currently hovered item in GestureMenu
            // xrOGI.SetHoveredMenuItem(gameObject);
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
            if (!isAnimating)
                StartCoroutine(ScaleObject(false));

            // Unset currently hovered item in GestureMenu
            // xrOGI.UnsetHoveredMenuItem();
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
}
