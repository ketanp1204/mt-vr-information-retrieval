using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RemoveObject : MonoBehaviour
{

    // Public Variables
    
    public bool removable = false;
    public bool enableRemoveTooltip = true;
    public Tooltip removeObjectTooltip;
    public InputActionReference removeInputAction;


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
            removeInputAction.action.Disable();
            removeInputAction.action.performed -= RemoveGameObject;
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
                if (other.gameObject.name.Contains("Right"))
                {
                    isHovering = true;

                    // Show select action tooltip
                    if (enableRemoveTooltip)
                    {
                        // Show tooltip
                        tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                        tooltipHandler.ShowTooltip(removeObjectTooltip);

                        // Enable input action
                        removeInputAction.action.Enable();
                        removeInputAction.action.performed += RemoveGameObject;
                    }
                }
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
                // Check for controller
                if (other.GetComponent<XRDirectInteractor>() != null)
                {
                    isHovering = false;

                    // Show select action tooltip
                    if (enableRemoveTooltip)
                    {
                        // Hide tooltip
                        tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                        tooltipHandler.HideTooltip(removeObjectTooltip);

                        // Disable input action
                        removeInputAction.action.Disable();
                        removeInputAction.action.performed -= RemoveGameObject;
                    }
                }
            }
        }        
    }
}
