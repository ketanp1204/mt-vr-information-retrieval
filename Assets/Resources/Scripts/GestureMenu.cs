using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using static MenuArea;
using Photon.Pun;
using UnityEngine.Rendering;

public class GestureMenu : XRSimpleInteractable
{
    [System.Serializable]
    public struct Menu
    {
        public GameObject parent;
        public List<GameObject> items;
        public float maxScaleValue;
        [HideInInspector]
        public bool isSelected;
        [HideInInspector]
        public GameObject parentSelectedItem;
    }

    // Public Variables

    [Space(20)]
    [Header("Interaction Properties")]
    public Menu menuLayer;
    public InputActionReference controllerTrigger;
    public InputActionReference controllerPrimaryButton;
    public Tooltip gripHoldTooltip;
    public GameObject menuSpherePrefab;
    public GameObject linePrefab;
    public float pullDistance = 0.3f;
    public float selectItemAnimDuration = 0.1f;


    // Private variables
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
    private GameObject currentlyHoveredMenuItem;
    private Menu currentMenuLayer = new();
    private string menuSpherePrefabLoc = "UtilityPrefabs/MenuSphere";
    private float menuSphereInitialZ = 0f;

    private void Update()
    {
        // Menu items animation
        if (isSelected)
        {
            if (currentPullDistance < 1f)
            {
                if (currentMenuLayer.items.Count > 0 && !currentMenuLayer.isSelected)
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
                        float currentActionScale = math.remap(0f, 1f, 0f, currentMenuLayer.maxScaleValue, currentPullDistance);

                        // Move the menu sphere
                        float sphereZ = Mathf.Lerp(menuSphereInitialZ, menuSphereInitialZ + 0.15f, currentPullDistance);
                        menuSphere.transform.position = new Vector3(menuSphere.transform.position.x, menuSphere.transform.position.y, sphereZ);

                        // Move and scale the menu items
                        for (int i = 0; i < currentMenuLayer.items.Count; i++)
                        {
                            currentMenuLayer.items[i].transform.localPosition = Vector3.Lerp(Vector3.zero, menuItemFinalPositions[i], currentPullDistance);
                            currentMenuLayer.items[i].transform.localScale = new Vector3(currentActionScale, currentActionScale, currentActionScale);
                        }
                    }
                }
            }
            else if (currentPullDistance > 1f)
            {
                if (currentMenuLayer.items.Count > 0)
                {
                    // Enable item colliders
                    foreach (GameObject item in currentMenuLayer.items)
                    {
                        item.GetComponent<MenuAction>().EnableCollider();
                    }
                }
            }
            
            // Select action on menu item
            if (currentlyHoveredMenuItem != null && controllerPrimaryButton.action.WasPressedThisFrame())
            {
                currentMenuLayer.isSelected = true;

                // Check whether there is another layer of menus
                var mA = currentlyHoveredMenuItem.GetComponent<MenuAction>();
                if (mA.menuLayer.items.Count > 0)
                {
                    mA.menuLayer.parentSelectedItem = currentlyHoveredMenuItem;

                    StartCoroutine(LoadNextMenuLayer(mA));

                    // Animate all items to the zero position
                    for (int i = 0; i < currentMenuLayer.items.Count; i++)
                    {
                        if (currentlyHoveredMenuItem.Equals(currentMenuLayer.items[i]))
                            StartCoroutine(AnimateMenuItemToZero(i, false));        // Not scaling the selected object to zero
                        else
                            StartCoroutine(AnimateMenuItemToZero(i, true));         // Scaling to zero
                    }
                }
                else
                {
                    // Animate all items to the zero position
                    for (int i = 0; i < currentMenuLayer.items.Count; i++)
                    {
                        StartCoroutine(AnimateMenuItemToZero(i, true));         // Scaling to zero
                    }

                    UnityEvent<GameObject> selectActions = currentlyHoveredMenuItem.GetComponent<MenuAction>().selectActions;

                    for (int i = 0; i < selectActions.GetPersistentEventCount(); i++)
                    {
                        ((MonoBehaviour)selectActions.GetPersistentTarget(i)).SendMessage(selectActions.GetPersistentMethodName(i), controllerTransform.gameObject);
                    }
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

        UpdateCurrentMenuItems(mA.menuLayer);
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
            menuSphere.GetComponent<GrabSphere>().SetVisibility(visible);
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
        Vector3 directionToHead = Vrsys.NetworkUser.localHead.transform.position - interactionInitialPos;
        Quaternion rotation = Quaternion.LookRotation(directionToHead, Vector3.up);

        // Create a sphere at center of interaction
        menuSphere = PhotonNetwork.Instantiate(menuSpherePrefabLoc, interactionInitialPos, rotation);
        menuSphere.transform.localScale = Vector3.zero;
        menuSphereInitialZ = menuSphere.transform.position.z;

        // Show the menu sphere
        SetMenuSphereVisibility(true);

        // Create a line from the center of interaction to the controller's current position
        menuLine = GameObject.Instantiate(linePrefab, menuSphere.transform);
        lR = menuLine.GetComponent<LineRenderer>();
        lR.positionCount = 2;
        lR.SetPosition(0, interactionInitialPos);

        UpdateCurrentMenuItems(menuLayer);

        // Menu facing in the direction of the user
        menuLayer.parent.transform.localRotation = rotation;

        // Get the tooltip handler reference
        tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
    }

    private void UpdateCurrentMenuItems(Menu menuLayer)
    {
        // Set the position of the parent of menu items to the center of interaction
        menuLayer.parent.SetActive(true);
        menuLayer.parent.transform.position = interactionInitialPos;

        // Face towards the user
        //if (menuLayer.parent.GetComponent<FaceCamera>() == null)
            //menuLayer.parent.AddComponent<FaceCamera>().rotationOffset = new Vector3(0f, 180f, 0f);

        // Set user camera on the items
        foreach (GameObject item in menuLayer.items)
        {
            if (item.GetComponent<FaceCamera>() == null)
                item.AddComponent<FaceCamera>();
            // item.GetComponent<FaceCamera>().SetUserCamera();
        }

        // Calculate the end positions of the menu items
        for (int i = 0; i < menuLayer.items.Count; i++)
        {
            Vector3 endPos = CalculateItemPosition(i, menuLayer.items.Count, circleRadius, menuLayer.parent.transform.localScale.x);
            menuItemFinalPositions.Add(endPos);
        }

        currentMenuLayer = menuLayer;
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
        Vector3 startPos = currentMenuLayer.items[index].transform.localPosition;
        Vector3 endPos = Vector3.zero;

        // Get start and end scale values
        Vector3 startScale = currentMenuLayer.items[index].transform.localScale;
        Vector3 endScale = Vector3.zero;

        // Disable menu item collider
        currentMenuLayer.items[index].GetComponent<MenuAction>().DisableCollider();

        float t = 0f;
        while (t < selectItemAnimDuration)
        {
            // Animate position
            currentMenuLayer.items[index].transform.localPosition = Vector3.Lerp(startPos, endPos, t / selectItemAnimDuration);

            // Animate scale
            if (scaleToZero)
                currentMenuLayer.items[index].transform.localScale = Vector3.Lerp(startScale, endScale, t / selectItemAnimDuration);

            t += Time.deltaTime;
            yield return null;
        }

        // Set final position
        currentMenuLayer.items[index].transform.localPosition = endPos;

        // Set final scale
        if (scaleToZero)
            currentMenuLayer.items[index].transform.localScale = endScale;

        // Disable collider
        currentMenuLayer.items[index].GetComponent<MenuAction>().DisableCollider();
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        Destroy(menuLine);

        if (currentMenuLayer.isSelected)
        {

        }
        else
        {
            for (int i = 0; i < currentMenuLayer.items.Count; i++)
            {
                StartCoroutine(AnimateMenuItemToZero(i, true));
            }

            // Reset parameters
            currentPullDistance = 0f;
            if (currentMenuLayer.parentSelectedItem != null)
            {
                currentMenuLayer.parentSelectedItem.transform.localScale = Vector3.zero;
                currentMenuLayer.parentSelectedItem.GetComponent<MenuAction>().DisableCollider();
            }
            Destroy(menuSphere);
            Destroy(currentMenuLayer.parent.GetComponent<FaceCamera>());
        }
    }
}
