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

    [System.Serializable]
    public struct MenuItems
    {
        public GameObject parent;
        public List<GameObject> items;
        public float maxScaleValue;
        [HideInInspector]
        public bool isSelected;
    }


    [Space(20)]
    [Header("Interaction Properties")]
    public MenuItems menus;
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
    private List<Vector3> menuItemFinalPositions = new List<Vector3>();
    private float sphereMaxScale = 0.3f;
    private float menuActionScaleValue = 0.03f;
    private float menuActionHoverScaleValue = 0.05f;
    private GameObject currentlyHoveredMenuItem;
    private bool menuItemSelected = false;
    private MenuItems currentMenuItems = new MenuItems();
    

    

    // Update is called once per frame
    void Update()
    {
        // Menu items animation
        if (isSelected)
        {
            

            if (currentPullDistance < 1f)
            {
                if (currentMenuItems.items.Count > 0 && !currentMenuItems.isSelected)
                {
                    // Calculate the distance that the controller has moved 
                    currentPullDistance = Vector3.Distance(controllerTransform.position, interactionInitialPos) / pullDistance;

                    // Calculate the direction where the player is moving the controller
                    float moveZ = controllerTransform.InverseTransformPoint(interactionInitialPos).z;

                    // Move the menu items if pulling in
                    if (moveZ > 0)
                    {
                        // Scaling the menu sphere based on the displacement
                        float currentSphereScale = math.remap(0f, 1f, 0f, sphereMaxScale, currentPullDistance);
                        menuSphere.transform.localScale = new Vector3(currentSphereScale, currentSphereScale, currentSphereScale);

                        // Scaling the menu items based on the displacement
                        float currentActionScale = math.remap(0f, 1f, 0f, currentMenuItems.maxScaleValue, currentPullDistance);

                        // Move and scale the menu items
                        for (int i = 0; i < currentMenuItems.items.Count; i++)
                        {
                            currentMenuItems.items[i].transform.localPosition = Vector3.Lerp(Vector3.zero, menuItemFinalPositions[i], currentPullDistance);
                            currentMenuItems.items[i].transform.localScale = new Vector3(currentActionScale, currentActionScale, currentActionScale);
                        }
                    }
                }

                /*
                // Calculate the direction where the player is moving the controller
                float moveZ = controllerTransform.InverseTransformPoint(interactionInitialPos).z;

                // Move the child items if pulling in
                if (moveZ > 0)
                {
                    // Scaling the menu sphere based on the displacement
                    float currentSphereScale = math.remap(0f, 1f, 0f, sphereMaxScale, currentPullDistance);
                    menuSphere.transform.localScale = new Vector3(currentSphereScale, currentSphereScale, currentSphereScale);

                    // Scaling the menu items based on the displacement
                    float currentActionScale = math.remap(0f, 1f, 0f, menuActionScaleValue, currentPullDistance);

                    // Move and scale the child menu items
                    for (int i = 0; i < menuItems.Count; i++)
                    {
                        menuItems[i].transform.localPosition = Vector3.Lerp(Vector3.zero, menuActionFinalPositions[i], currentPullDistance);
                        menuItems[i].transform.localScale = new Vector3(currentActionScale, currentActionScale, currentActionScale);
                    }
                }
                */
            }
            else if (currentPullDistance > 1f)
            {
                if (currentMenuItems.items.Count > 0)
                {
                    // Enable item colliders
                    foreach (GameObject item in menuItems)
                    {
                        item.GetComponent<MenuAction>().EnableCollider();
                    }
                }
            }

            // Select action on menu item
            if (currentlyHoveredMenuItem != null && controllerPrimaryButton.action.WasPressedThisFrame())
            {
                menuItemSelected = true;

                currentMenuItems.isSelected = true;

                // Animate all items to the zero position
                for (int i = 0; i < currentMenuItems.items.Count; i++)
                {
                    if (currentlyHoveredMenuItem.Equals(currentMenuItems.items[i]))
                        StartCoroutine(AnimateMenuItemToZero(i, false));        // Not scaling the selected object to zero
                    else
                        StartCoroutine(AnimateMenuItemToZero(i, true));         // Scaling to zero
                }

                /*
                // Animate all items to the zero position
                for (int i = 0; i < menuItems.Count; i++)
                {
                    if (currentlyHoveredMenuItem.Equals(menuItems[i]))
                        StartCoroutine(AnimateMenuItemToZero(i, false));        // Not scaling the selected object to zero
                    else
                        StartCoroutine(AnimateMenuItemToZero(i, true));         // Scaling to zero
                }
                */

                // Check whether there is another layer of menus
                var mA = currentlyHoveredMenuItem.GetComponent<MenuAction>();
                if (mA.menus.items.Count > 0)
                {
                    StartCoroutine(LoadNextMenuLayer(mA));
                }
                else
                {
                    currentlyHoveredMenuItem.GetComponent<MenuAction>().selectActions.Invoke();
                }
            }



            // Set line end position to the controller's position
            if (controllerTransform != null && lR != null)
            {
                lR.SetPosition(1, controllerTransform.position);
            }
        }
    }

    private IEnumerator LoadNextMenuLayer(MenuAction mA)
    {
        yield return new WaitForSeconds(selectItemAnimDuration);
        
        // Reset item final positions array
        menuItemFinalPositions.Clear();

        // Reset pull distance temporarily for animation
        currentPullDistance = 0f;

        UpdateCurrentMenuItems(mA.menus);
    }

    public void SetHoveredMenuItem(GameObject gO)
    {
        currentlyHoveredMenuItem = gO;
    }

    public void UnsetHoveredMenuItem()
    {
        currentlyHoveredMenuItem = null;
    }

    public void SetMenuSphereVisibility(bool visible)
    {
        if (menuSphere != null)
            menuSphere.GetComponent<MeshRenderer>().enabled = visible;
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

        UpdateCurrentMenuItems(menus);

        /*
        // Set the position of the parent of menu items to the center of interaction
        transform.Find("Menus").position = interactionInitialPos;

        foreach (GameObject item in menuItems)
        {
            item.GetComponent<FaceCamera>().SetUserCamera();
        }

        // Calculate the end positions of the menu items
        for (int i = 0; i < menuItems.items.Count; i++)
        {
            Vector3 endPos = CalculateItemPosition(i, menuItems.items.Count, circleRadius);
            menuItemFinalPositions.Add(endPos);
        }
        */

        // Get the tooltip handler reference
        tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
    }

    private void UpdateCurrentMenuItems(MenuItems menuItems)
    {
        // Set the position of the parent of menu items to the center of interaction
        menuItems.parent.transform.position = interactionInitialPos;

        // Set user camera on the items
        foreach (GameObject item in menuItems.items)
        {
            item.GetComponent<FaceCamera>().SetUserCamera();
        }

        // Calculate the end positions of the menu items
        for (int i = 0; i < menuItems.items.Count; i++)
        {
            Vector3 endPos = CalculateItemPosition(i, menuItems.items.Count, circleRadius, menuItems.parent.transform.localScale.x);
            menuItemFinalPositions.Add(endPos);
        }

        currentMenuItems = menuItems;
    }

    private Vector3 CalculateItemPosition(int childIndex, int totalChildren, float radius, float parentScaleValue)
    {
        float angle = (float)childIndex / (float)totalChildren * Mathf.PI * 2f;
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        return new Vector3(x * (1f / parentScaleValue), y * (1f / parentScaleValue), pullDistance * (1f / parentScaleValue));
    }

    private IEnumerator AnimateMenuItemToZero(int index, bool scaleToZero)
    {
        // Get start and end positions
        Vector3 startPos = currentMenuItems.items[index].transform.localPosition;
        Vector3 endPos = Vector3.zero;

        // Get start and end scale values
        Vector3 startScale = currentMenuItems.items[index].transform.localScale;
        Vector3 endScale = Vector3.zero;

        // Disable menu item collider
        currentMenuItems.items[index].GetComponent<MenuAction>().DisableCollider();

        float t = 0f;
        while (t < selectItemAnimDuration)
        {
            // Animate position
            currentMenuItems.items[index].transform.localPosition = Vector3.Lerp(startPos, endPos, t / selectItemAnimDuration);

            // Animate scale
            if (scaleToZero)
                currentMenuItems.items[index].transform.localScale = Vector3.Lerp(startScale, endScale, t / selectItemAnimDuration);

            t += Time.deltaTime;
            yield return null;
        }

        // Set final position
        currentMenuItems.items[index].transform.localPosition = endPos;

        // Set final scale
        if (scaleToZero)
            currentMenuItems.items[index].transform.localScale = endScale;
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
