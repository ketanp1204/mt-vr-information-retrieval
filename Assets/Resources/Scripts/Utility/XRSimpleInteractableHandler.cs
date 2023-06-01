using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class XRSimpleInteractableHandler : MonoBehaviour
{
    public UnityEvent hoverEnterEvents;
    public UnityEvent hoverExitEvents;
    public UnityEvent selectEnterEvents;
    public UnityEvent selectExitEvents;

    private bool isSelected = false;

    public void HandleHoverEntered()
    {
        if (!isSelected)
        {
            hoverEnterEvents.Invoke();
        }
    }

    public void HandleHoverExited()
    {
        if (!isSelected)
        {
            hoverExitEvents.Invoke();
        }
    }

    public void HandleSelect()
    {
        if (isSelected)
        {
            isSelected = false;

            selectExitEvents.Invoke();
        }
        else
        {
            isSelected = true;

            selectEnterEvents.Invoke();
        }
    }
}
