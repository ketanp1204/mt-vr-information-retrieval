using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RemoveObject : MonoBehaviour
{

    // Public Variables

    [HideInInspector]
    public bool removable = false;
    public bool showRemoveTooltip = true;
    public Tooltip removeObjectTooltip;
    public InputActionReference secondaryButton;


    // Private Variables

    private bool isHovering = false;
    private TooltipHandler tooltipHandler;    


    
    public void SetRemovableStatus(bool status)
    {
        removable = status;
    }

    private void RemoveGameObject(InputAction.CallbackContext obj)
    {
        if (isHovering)
        {
            secondaryButton.action.Disable();
            secondaryButton.action.performed -= RemoveGameObject;
            tooltipHandler.HideTooltip(removeObjectTooltip);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (removable)
        {
            // Check for controller
            if (other.GetComponent<XRDirectInteractor>() != null)
            {
                isHovering = true;

                // Show select action tooltip
                if (showRemoveTooltip)
                {
                    tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                    tooltipHandler.ShowTooltip(removeObjectTooltip);
                }

                secondaryButton.action.Enable();
                secondaryButton.action.performed += RemoveGameObject;

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (removable)
        {
            // Check for controller
            if (other.GetComponent<XRDirectInteractor>() != null)
            {
                isHovering = false;

                // Show select action tooltip
                if (showRemoveTooltip)
                {
                    tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                    tooltipHandler.HideTooltip(removeObjectTooltip);
                }

                secondaryButton.action.Disable();
                secondaryButton.action.performed -= RemoveGameObject;
            }
        }        
    }
}
