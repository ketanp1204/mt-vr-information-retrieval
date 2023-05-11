using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MenuArea : XRSimpleInteractable
{
    /* Public Variables */
    public InputActionReference controllerTrigger;
    public Tooltip gripHoldTooltip;
    public GameObject grabMenuPrefab;
    public float pullDistance = 0.3f;

    /* Private Variables */
    private TooltipHandler tooltipHandler;
    private Vector3 interactionInitialPos;
    private GameObject menuSphere;
    private Transform controllerTransform;
    private Vector3 sphereMaxScale = new Vector3(0.14f, 0.14f, 0.14f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Scale the sphere after it is created
        if (isSelected && menuSphere != null && menuSphere.transform.localScale.magnitude < sphereMaxScale.magnitude)
        {
            // Calculate the direction where the player is moving the controller
            float moveZ = controllerTransform.InverseTransformPoint(interactionInitialPos).z;

            // Scale the sphere if pulling in
            if (moveZ > 0)
            {
                float displacement = Vector3.Distance(controllerTransform.position, interactionInitialPos) / pullDistance;
                float scale = math.remap(0f, 1f, 0f, 0.14f, displacement);
                menuSphere.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);

        // Show the tooltip if not selected
        if (!isSelected)
        {
            tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
            tooltipHandler.ShowTooltip(gripHoldTooltip);
        }
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);

        // Hide the tooltip if not selected
        if (!isSelected)
        {
            tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
            tooltipHandler.HideTooltip(gripHoldTooltip);
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // Save position of controller as center of interaction
        controllerTransform = args.interactorObject.transform;
        interactionInitialPos = controllerTransform.position;

        // Make the sphere face the user
        Vector3 directionToHead = interactionInitialPos - Vrsys.NetworkUser.localHead.transform.position;
        Quaternion rotation = Quaternion.LookRotation(directionToHead, Vector3.up);

        // Create a sphere at center of interaction
        menuSphere = GameObject.Instantiate(grabMenuPrefab, interactionInitialPos, rotation);
        menuSphere.transform.localScale = Vector3.zero;

        // TODO: Create child menu spheres at origin of the parent sphere and move them as well
        //       with the parent but towards their respective menu positions in a circle

    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
    }
}
