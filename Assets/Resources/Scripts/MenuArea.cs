using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MenuArea : XRSimpleInteractable
{
    /* Public Variables */
    [Space(20)]
    [Header("Interaction Properties")]
    public List<GameObject> menuItems = new List<GameObject>();
    public InputActionReference controllerTrigger;
    public InputActionReference controllerPrimaryButton;
    public Tooltip gripHoldTooltip;
    public GameObject menuSpherePrefab;
    public List<GameObject> menuActionPrefabs;
    public GameObject linePrefab;
    public float pullDistance = 0.3f;
    public float selectItemAnimDuration = 0.1f;

    /* Private Variables */
    [SerializeField] private float circleRadius = 0.08f;
    private float currentPullDistance = 0f;
    private TooltipHandler tooltipHandler;
    private Vector3 interactionInitialPos;
    private GameObject menuSphere;
    private Transform controllerTransform;
    private GameObject menuLine;
    private LineRenderer lR;
    private List<Vector3> menuActionFinalPositions = new List<Vector3>();
    private Vector3 sphereMaxScale = new Vector3(0.14f, 0.14f, 0.14f);
    private float menuActionScaleValue = 0.03f;
    private float menuActionHoverScaleValue = 0.05f;
    private GameObject currentlyHoveredMenuItem;
    private bool menuItemSelected = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Menu items animation
        if (isSelected)
        {
            if (currentPullDistance < 1f && menuItems.Count > 0)
            {
                // Calculate the distance that the controller has moved 
                currentPullDistance = Vector3.Distance(controllerTransform.position, interactionInitialPos) / pullDistance;

                // Calculate the direction where the player is moving the controller
                float moveZ = controllerTransform.InverseTransformPoint(interactionInitialPos).z;

                // Move the child items if pulling in
                if (moveZ > 0)
                {
                    // Calculate the scale of the child items based on the current displacement
                    float currentActionScale = math.remap(0f, 1f, 0f, menuActionScaleValue, currentPullDistance);

                    // Move and scale the child menu items
                    for (int i = 0; i < menuItems.Count; i++)
                    {
                        menuItems[i].transform.localPosition = Vector3.Lerp(Vector3.zero, menuActionFinalPositions[i], currentPullDistance);
                        menuItems[i].transform.localScale = new Vector3(currentActionScale, currentActionScale, currentActionScale);
                    }
                }
            }
            else if (currentPullDistance > 1f && menuItems.Count > 0)
            {
                // Enable item colliders
                foreach (GameObject item in menuItems)
                {
                    item.GetComponent<MenuAction>().EnableCollider();
                }
            }
        }

        if (isSelected && currentPullDistance > 1f && menuItems.Count > 0)
        {
            /* Controller position w.r.t. menu items 
            Vector3 actionLocalPos = menuActions[0].transform.localPosition;
            Transform temp = menuActions[0].transform;
            Vector3 localPos = temp.localPosition;
            localPos.x = 0f;
            localPos.y = 0f;
            temp.localPosition = localPos;
            Vector3 centerWorldPos = temp.position;
            localPos.x = actionLocalPos.x;
            localPos.y = actionLocalPos.y;
            temp.localPosition = localPos;

            if (Vector3.Distance(centerWorldPos, controllerTransform.position) > circleRadius * 2)
            {
                //Debug.Log("outside circle");
            }
            else
            {
                //Debug.Log("inside circle");
            }
           

            // Hover over action 
            for (int i = 0; i < menuActions.Count; i++)
            {
                float distanceToController = Vector3.Distance(menuActions[i].transform.position, controllerTransform.position);

                if (distanceToController < 0.06f)
                {
                    menuActions[i].transform.localScale = new Vector3(menuActionHoverScaleValue, menuActionHoverScaleValue, menuActionHoverScaleValue);
                    currentlyHoveredMenuItem = menuActions[i];
                }
                else
                {
                    menuActions[i].transform.localScale = new Vector3(menuActionScaleValue, menuActionScaleValue, menuActionScaleValue);
                }
            }
            */
        }

        // Select action on menu item
        if (currentlyHoveredMenuItem != null && controllerPrimaryButton.action.WasPressedThisFrame())
        {
            menuItemSelected = true;

            for(int i = 0; i < menuItems.Count; i++)
            {
                if (currentlyHoveredMenuItem.Equals(menuItems[i]))
                    StartCoroutine(AnimateMenuItemToZero(i, false));
                else
                    StartCoroutine(AnimateMenuItemToZero(i, true));
            }
        }

        // Set line end position to the controller's position
        if (isSelected && controllerTransform != null && lR != null)
        {
            lR.SetPosition(1, controllerTransform.position);
        }
    }

    public void SetHoveredMenuItem(GameObject gO)
    {
        currentlyHoveredMenuItem = gO;
    }

    public void UnsetHoveredMenuItem()
    {
        currentlyHoveredMenuItem = null;
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
        menuSphere = GameObject.Instantiate(menuSpherePrefab, interactionInitialPos, rotation);
        menuSphere.transform.localScale = Vector3.zero;

        // Create a line from the center of interaction to the controller's current position
        menuLine = GameObject.Instantiate(linePrefab, menuSphere.transform);
        lR = menuLine.GetComponent<LineRenderer>();
        lR.positionCount = 2;
        lR.SetPosition(0, interactionInitialPos);

        // Set the position of the parent of menu items to the center of interaction
        transform.Find("Menus").position = interactionInitialPos;

        // Set user camera on the items
        foreach(GameObject item in menuItems)
        {
            item.GetComponent<FaceCamera>().SetUserCamera();
        }

        // Save the end positions of the menu items
        for(int i = 0; i < menuItems.Count; i++)
        {
            Vector3 endPos = CalculateChildPosition(i, menuItems.Count, circleRadius);
            menuActionFinalPositions.Add(endPos);
        }

        // Get the tooltip handler reference
        tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
    }

    private Vector3 CalculateChildPosition(int childIndex, int totalChildren, float radius)
    {
        float angle = (float)childIndex / (float)totalChildren * Mathf.PI * 2f;
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        return new Vector3(x, y, pullDistance);
    }

    private IEnumerator AnimateMenuItemToZero(int index, bool scaleToZero)
    {
        // Get start and end positions
        Vector3 startPos = menuItems[index].transform.localPosition;
        Vector3 endPos = Vector3.zero;

        // Get start and end scale values
        Vector3 startScale = menuItems[index].transform.localScale;
        Vector3 endScale = Vector3.zero;

        // Disable menu item collider
        menuItems[index].GetComponent<MenuAction>().DisableCollider();

        float t = 0f;
        while (t < selectItemAnimDuration)
        {
            // Animate position
            menuItems[index].transform.localPosition = Vector3.Lerp(startPos, endPos, t / selectItemAnimDuration);

            // Animate scale
            if (scaleToZero)
                menuItems[index].transform.localScale = Vector3.Lerp(startScale, endScale, t / selectItemAnimDuration);

            t += Time.deltaTime;
            yield return null;
        }

        // Set final position
        menuItems[index].transform.localPosition = endPos;

        // Set final scale
        if (scaleToZero)
            menuItems[index].transform.localScale = endScale;
    }

    private IEnumerator AnimateMenuItemsToZero(bool scaleToZero)
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            // Get start and end positions
            Vector3 startPos = menuItems[i].transform.localPosition;
            Vector3 endPos = Vector3.zero;

            // Get start and end scale values
            Vector3 startScale = menuItems[i].transform.localScale;
            Vector3 endScale = Vector3.zero;

            // Disable menu item collider
            menuItems[i].GetComponent<MenuAction>().DisableCollider();

            float t = 0f;
            while (t < selectItemAnimDuration)
            {
                // Animate position
                menuItems[i].transform.localPosition = Vector3.Lerp(startPos, endPos, t / selectItemAnimDuration);

                // Animate scale
                if (scaleToZero)
                    menuItems[i].transform.localScale = Vector3.Lerp(startScale, endScale, t / selectItemAnimDuration);

                t += Time.deltaTime;
                yield return null;
            }

            // Set final position
            menuItems[i].transform.localPosition = endPos;

            // Set final scale
            menuItems[i].transform.localScale = endScale;
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        
        Destroy(menuLine);

        if (menuItemSelected)
        {

        }
        else
        {
            for (int i = 0; i < menuItems.Count; i++)
            {
                StartCoroutine(AnimateMenuItemToZero(i, true));
            }

            // Reset parameters
            currentPullDistance = 0f;
            Destroy(menuSphere);
        }
    }
}
