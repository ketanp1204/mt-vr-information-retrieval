using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using Photon.Pun;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEditor;
using System;

public class MenuArea : XRSimpleInteractable
{
    [System.Serializable]
    public struct Menu
    {
        public GameObject parent;
        public List<GameObject> items;
        public float maxScaleValue;
        [HideInInspector]
        public bool isAtFinalPos;
        [HideInInspector]
        public bool isSelected;
        [HideInInspector]
        public GameObject parentSelectedItem;
    }

    // Public Variables

    
    [Space(20)]
    public ExhibitInformation exhibitInfo;
    [Space(20)]
    [Header("Interaction Properties")]
    public Menu menuLayer;
    public InputActionReference controllerTrigger;
    public InputActionReference controllerPrimaryButton;
    public Tooltip gripHoldTooltip;
    public GameObject menuSpherePrefab;
    public GameObject linePrefab;
    public GameObject imagePrefab;
    public GameObject exitSphere;
    public float pullDistance = 0.3f;
    public float selectItemAnimDuration = 0.1f;
    


    // Private variables

    private BoxCollider col;
    private bool menuOpen = false;
    [SerializeField] private float menuItemCircleRadius = 0.08f;
    [SerializeField] private float menuItemLinearSpacing = 0.1f;
    private float currentPullDistance = 0f;
    private TooltipHandler tooltipHandler;
    private Vector3 interactionInitialPos;
    private GameObject menuSphere;
    private Transform controllerTransform;
    private GameObject menuLine;
    private LineRenderer lR;
    private List<Vector3> menuItemFinalPositions = new List<Vector3>();
    [SerializeField] private float sphereMaxScale = 0.3f;
    private GameObject currentlyHoveredMenuItem;
    private Menu currentMenuLayer = new();
    private string menuSpherePrefabLoc = "UtilityPrefabs/MenuSphere";

    private float menuSphereInitialZ = 0f;


    private void Start()
    {
        col = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        // Menu items animation
        if (menuOpen)
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
                        menuSphere.transform.localScale = Vector3.one * currentSphereScale;

                        // Calculating menu item scale
                        float menuItemScale = math.remap(0f, 1f, 0f, currentMenuLayer.maxScaleValue, currentPullDistance);

                        // Scaling and moving the exit sphere
                        exitSphere.transform.localScale = Vector3.one * menuItemScale;
                        Vector3 exitSphereFinalPos = new Vector3(0f, -0.1f, pullDistance);
                        exitSphere.transform.localPosition = Vector3.Lerp(Vector3.zero, exitSphereFinalPos, currentPullDistance);

                        // Move the menu sphere
                        float sphereZ = Mathf.Lerp(menuSphereInitialZ, menuSphereInitialZ + pullDistance, currentPullDistance);
                        menuSphere.transform.localPosition = new Vector3(menuSphere.transform.localPosition.x, menuSphere.transform.localPosition.y, sphereZ);

                        // Move and scale the menu items
                        for (int i = 0; i < currentMenuLayer.items.Count; i++)
                        {
                            currentMenuLayer.items[i].transform.localPosition = Vector3.Lerp(Vector3.zero, menuItemFinalPositions[i], currentPullDistance);
                            currentMenuLayer.items[i].transform.localScale = Vector3.one * menuItemScale;
                        }
                    }
                }
            }
            else if (currentPullDistance > 1f)
            {
                if (!currentMenuLayer.isAtFinalPos)
                {
                    currentMenuLayer.isAtFinalPos = true;

                    // Enable item colliders
                    if (currentMenuLayer.items.Count > 0)
                    {
                        foreach (GameObject item in currentMenuLayer.items)
                        {
                            item.GetComponent<MenuElement>().EnableCollider();
                        }
                    }

                    // Enable exit sphere collider
                    exitSphere.GetComponent<SphereCollider>().enabled = true;
                }
            }

            // Set line end position to the controller's position
            if (controllerTransform != null && lR != null)
            {
                lR.SetPosition(1, controllerTransform.position);
            }
        }
    }

    private IEnumerator LoadNextMenuLayer(MenuElement mA)
    {
        yield return new WaitForSeconds(selectItemAnimDuration);

        // Reset item final positions array
        menuItemFinalPositions.Clear();

        // Reset pull distance temporarily for animation
        currentPullDistance = 0f;

        UpdateCurrentMenuItems(mA.menuLayer);
    }

    private void SelectMenuAction(MenuElement mA)
    {
        // Animate all other items to the zero position
        for (int i = 0; i < currentMenuLayer.items.Count; i++)
        {
            if (!currentlyHoveredMenuItem.Equals(currentMenuLayer.items[i]))
                StartCoroutine(AnimateMenuItemToZero(i, true));         // Scaling to zero
        }

        UnityEvent selectActions = mA.selectActions;
        selectActions.Invoke();
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
            menuSphere.GetComponentInParent<GrabSphere>().SetVisibility(visible);
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

    private void EnableCollider()
    {
        if (col != null)
            col.enabled = true;
    }

    private void DisableCollider()
    {
        if (col != null)
            col.enabled = false;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (!menuOpen)
        {
            menuOpen = true;

            // Disable Menu Area Collider
            DisableCollider();

            // Save position of controller as center of interaction
            controllerTransform = args.interactorObject.transform;
            interactionInitialPos = controllerTransform.position;

            // Make the sphere face the user
            Vector3 directionToHead = Vrsys.NetworkUser.localHead.transform.position - interactionInitialPos;
            Quaternion rotation = Quaternion.LookRotation(directionToHead, Vector3.up);

            // Create a sphere at center of interaction
            menuSphere = PhotonNetwork.Instantiate(menuSpherePrefabLoc, interactionInitialPos, rotation);
            menuSphere = menuSphere.transform.Find("Sphere").gameObject;
            menuSphere.transform.localScale = Vector3.zero;
            menuSphereInitialZ = menuSphere.transform.localPosition.z;
            menuSphere.AddComponent<FaceCamera>();

            // Show the menu sphere
            SetMenuSphereVisibility(true);

            // Create a line from the center of interaction to the controller's current position
            menuLine = GameObject.Instantiate(linePrefab, menuSphere.transform);
            lR = menuLine.GetComponent<LineRenderer>();
            lR.positionCount = 2;
            lR.SetPosition(0, interactionInitialPos);

            UpdateCurrentMenuItems(menuLayer);

            // Menu facing in the direction of the user
            menuSphere.transform.parent.localRotation = rotation;
            menuLayer.parent.transform.localRotation = rotation;
            exitSphere.AddComponent<FaceCamera>();

            // Get the tooltip handler reference
            tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
        }
    }

    public void OnMenuItemSelect(MenuElement mA)
    {
        Debug.Log(currentlyHoveredMenuItem.name);
        currentMenuLayer.isSelected = true;

        switch (currentlyHoveredMenuItem.name)
        {
            case "Description":

                SelectMenuAction(mA);

                break;

            case "Images":

                mA.menuLayer.parentSelectedItem = currentlyHoveredMenuItem;

                // Animate all items to the zero position
                for (int i = 0; i < currentMenuLayer.items.Count; i++)
                {
                    if (currentlyHoveredMenuItem.Equals(currentMenuLayer.items[i]))
                        StartCoroutine(AnimateMenuItemToZero(i, false));        // Not scaling the selected object to zero
                    else
                        StartCoroutine(AnimateMenuItemToZero(i, true));         // Scaling to zero
                }

                // Populate images
                mA.menuLayer.items.Clear();
                for (int i = 0; i < exhibitInfo.images.Length; i++)
                {
                    // Instantiate and rename image
                    GameObject imageGO = Instantiate(imagePrefab, mA.menuLayer.parent.transform);
                    imageGO.name = "Image" + (i + 1);
                    GameObject child = imageGO.transform.Find("Image").gameObject;

                    // Set and resize image
                    RawImage rI = child.GetComponent<RawImage>();
                    rI.texture = exhibitInfo.images[i].texture;
                    rI.SetNativeSize();

                    // Resize box collider
                    BoxCollider c = child.GetComponent<BoxCollider>();
                    RectTransform rt = child.GetComponent<RectTransform>();
                    c.size = new Vector3(rt.rect.width, rt.rect.height, c.size.z);

                    MenuElement imageMA = imageGO.GetComponent<MenuElement>();
                    imageMA.menuArea = this;
                    imageMA.selectActions.AddListener(mA.DisableMenuItem);
                    mA.menuLayer.items.Add(imageGO);
                }

                StartCoroutine(LoadNextMenuLayer(mA));

                break;

            case "Audio":

                SelectMenuAction(mA);

                break;

            case "DetailView":

                SelectMenuAction(mA);

                break;

            default:

                SelectMenuAction(mA);

                break;
        }
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
            //Vector3 endPos = CalculateItemCircularPosition(i, menuLayer.items.Count, menuItemCircleRadius, menuLayer.parent.transform.localScale.x);
            Vector3 endPos = CalculateItemLinearPosition(i, menuLayer, menuItemLinearSpacing);
            menuItemFinalPositions.Add(endPos);
        }

        currentMenuLayer = menuLayer;
    }

    private Vector3 CalculateItemCircularPosition(int childIndex, int totalChildren, float radius, float parentScaleValue)
    {
        float angle = (float)childIndex / (float)totalChildren * Mathf.PI * 2f;
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        return new Vector3(x * (1f / parentScaleValue), y * (1f / parentScaleValue), pullDistance * (1f / parentScaleValue));
    }

    private Vector3 CalculateItemLinearPosition(int childIndex, Menu menuLayer, float spacing)
    {
        int totalChildren = menuLayer.items.Count;
        Transform parent = menuLayer.parent.transform;
        float parentScaleValue = menuLayer.parent.transform.localScale.x;

        float totalLength = (totalChildren - 1) * spacing;
        Vector3 startPosition = -0.5f * totalLength * parent.right;

        Vector3 localPos = startPosition + (totalChildren - childIndex - 1) * spacing * parent.right;
        return new Vector3(localPos.x * (1f / parentScaleValue), localPos.y * (1f / parentScaleValue), pullDistance * (1f / parentScaleValue));
    }

    private IEnumerator AnimateMenuItemToZero(int index, bool scaleToZero)
    {
        // Get start and end positions
        Vector3 startPos = currentMenuLayer.items[index].transform.localPosition;
        Vector3 endPos = Vector3.zero;

        // Get start and end scale values
        Vector3 startScale = currentMenuLayer.items[index].transform.localScale;
        Vector3 endScale = Vector3.zero;

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
        currentMenuLayer.items[index].GetComponent<MenuElement>().DisableCollider();
    }

    public void ExitMenu()
    {
        for (int i = 0; i < currentMenuLayer.items.Count; i++)
        {
            StartCoroutine(AnimateMenuItemToZero(i, true));
        }

        StartCoroutine(AnimateExitSphereToZero());

        ResetParameters();
    }

    private void ResetParameters()
    {
        menuOpen = false;
        EnableCollider();
        currentPullDistance = 0f;
        if (currentMenuLayer.parentSelectedItem != null)
        {
            currentMenuLayer.parentSelectedItem.transform.localScale = Vector3.zero;
            currentMenuLayer.parentSelectedItem.GetComponent<MenuElement>().DisableCollider();
        }
        Destroy(menuSphere);
        Destroy(currentMenuLayer.parent.GetComponent<FaceCamera>());
    }

    private IEnumerator AnimateExitSphereToZero()
    {
        // Get start and end scale values
        Vector3 startScale = exitSphere.transform.localScale;
        Vector3 endScale = Vector3.zero;

        float t = 0f;
        while (t < selectItemAnimDuration)
        {

            // Animate scale
            exitSphere.transform.localScale = Vector3.Lerp(startScale, endScale, t / selectItemAnimDuration);

            t += Time.deltaTime;
            yield return null;
        }

        exitSphere.transform.localScale = endScale;
        exitSphere.GetComponent<SphereCollider>().enabled = false;
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
            /*
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
            */
        }
    }
}
