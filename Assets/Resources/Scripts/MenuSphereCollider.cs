using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuSphereCollider : MonoBehaviour
{

    // Public Variables //

    public InputActionReference anyPrimaryButton;
    public Tooltip showInfoTooltip;

    // Private Variables //

    private MenuSphere mS;
    private TooltipHandler tooltipHandler;


    // Start is called before the first frame update
    void Start()
    {
        mS.GetComponentInParent<MenuSphere>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Show tooltip
        tooltipHandler = collision.transform.root.GetComponent<TooltipHandler>();
        tooltipHandler.ShowTooltip(showInfoTooltip);

        // Handle hover action on parent script
        mS.OnHoverEntered();

        // Enable input action
        anyPrimaryButton.action.Enable();
        anyPrimaryButton.action.performed += mS.OnSelectEntered;
    }

    private void OnCollisionExit(Collision collision)
    {
        // Hide tooltip
        tooltipHandler = collision.transform.root.GetComponent<TooltipHandler>();
        tooltipHandler.HideTooltip(showInfoTooltip);

        // Handle hover action on parent script
        mS.OnHoverExited();

        // Disable input action
        anyPrimaryButton.action.performed -= mS.OnSelectEntered;
        anyPrimaryButton.action.Disable();
    }
}
