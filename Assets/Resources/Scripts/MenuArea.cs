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
using Unity.XR.CoreUtils;
using TMPro;

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
        public GameObject parentSelectedItem;
    }

    // Public Variables


    [Space(20)]
    public ExhibitInformation exhibitInfo;
    [Space(20)]
    [Header("Interaction Properties")]
    public Menu menuLayer;
    public Tooltip gripHoldTooltip;
    public GameObject menuSpherePrefab;
    public GameObject linePrefab;
    public GameObject imagePrefab;
    public GameObject descBoxPrefab;
    public string imagePrefabLoc = "UtilityPrefabs/ImagePrefab";
    public string descBoxPrefabLoc = "UtilityPrefabs/DescBoxPrefab";
    public GameObject imageContainer;    
    public GameObject exitSphere;
    public float pullDistance = 0.3f;
    public float menuItemAnimDuration = 0.1f;
    public float nextLayerLoadDelay = 0.05f;



    // Private variables

    private BoxCollider col;
    private bool menuOpen = false;
    [SerializeField] private float menuItemCircleRadius = 0.08f;
    [SerializeField] private float menuItemLinearSpacing = 0.1f;
    private float currentPullDistance = 0f;
    private TooltipHandler tooltipHandler;
    private Vector3 interactionInitialPos;
    private GameObject contentSphere;
    private Transform controllerTransform;
    private GameObject menuLine;
    private LineRenderer lR;
    private List<Vector3> menuItemFinalPositions = new List<Vector3>();
    [SerializeField] private float sphereMaxScale = 0.3f;
    [SerializeField] private float exitSphereScale = 0.03f;
    private Menu currentMenuLayer = new();
    private string menuSpherePrefabLoc = "UtilityPrefabs/MenuSphere";
    private bool firstLayerOpen = false;
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
                if (currentMenuLayer.items.Count > 0)
                {
                    // Calculate the distance that the controller has moved
                    currentPullDistance = Vector3.Distance(controllerTransform.position, interactionInitialPos) / pullDistance;

                    // Calculate the direction where the player is moving the controller
                    float moveZ = controllerTransform.InverseTransformPoint(interactionInitialPos).z;

                    // Move the menu items if pulling in
                    if (moveZ > 0)
                    {
                        // Animation for the first menu layer
                        if (!firstLayerOpen)
                        {
                            // Scaling the content sphere
                            float currentSphereScale = math.remap(0f, 1f, 0f, sphereMaxScale, currentPullDistance);
                            contentSphere.transform.localScale = Vector3.one * currentSphereScale;

                            // Moving the content sphere
                            float sphereZ = Mathf.Lerp(menuSphereInitialZ, menuSphereInitialZ + pullDistance, currentPullDistance);
                            contentSphere.transform.localPosition = new Vector3(contentSphere.transform.localPosition.x, contentSphere.transform.localPosition.y, sphereZ);

                            // Scaling the exit sphere
                            exitSphere.transform.localScale = Vector3.one * exitSphereScale;
                            Vector3 exitSphereFinalPos = new Vector3(0f, -0.1f, pullDistance);
                            exitSphere.transform.localPosition = Vector3.Lerp(Vector3.zero, exitSphereFinalPos, currentPullDistance);
                        }

                        // Calculating menu item scale
                        float menuItemScale = math.remap(0f, 1f, 0f, currentMenuLayer.maxScaleValue, currentPullDistance);

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
                if (!firstLayerOpen)
                {
                    firstLayerOpen = true;

                    // Enable exit sphere collider
                    exitSphere.GetComponent<SphereCollider>().enabled = true;
                }
                    

                if (!currentMenuLayer.isAtFinalPos)
                {
                    currentMenuLayer.isAtFinalPos = true;

                    // Enable item colliders
                    if (currentMenuLayer.items.Count > 0)
                    {
                        foreach (GameObject item in currentMenuLayer.items)
                        {
                            item.GetComponent<Collider>().enabled = true;
                        }
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

            // Hide Interaction Guide
            var iG = GetComponent<InteractionGuide>();
            iG.HideGuide();
            iG.isMenuOpen = true;

            // Disable Menu Area Collider
            DisableCollider();

            // Save position of controller as center of interaction
            controllerTransform = args.interactorObject.transform;
            interactionInitialPos = controllerTransform.position;

            // Calculating direction facing the user
            Vector3 directionToHead = Vrsys.NetworkUser.localHead.transform.position - interactionInitialPos;
            Quaternion rotation = Quaternion.LookRotation(directionToHead, Vector3.up);

            // Create a sphere at center of interaction
            contentSphere = PhotonNetwork.Instantiate(menuSpherePrefabLoc, interactionInitialPos, rotation);
            contentSphere = contentSphere.transform.Find("Sphere").gameObject;
            contentSphere.transform.localScale = Vector3.zero;
            menuSphereInitialZ = contentSphere.transform.localPosition.z;
            // menuSphere.AddComponent<FaceCamera>();

            // Create a line from the center of interaction to the controller's current position
            menuLine = GameObject.Instantiate(linePrefab, contentSphere.transform);
            lR = menuLine.GetComponent<LineRenderer>();
            lR.positionCount = 2;
            lR.SetPosition(0, interactionInitialPos);

            UpdateCurrentMenuItems(menuLayer);

            // Menu facing in the direction of the user
            contentSphere.transform.parent.localRotation = rotation;
            menuLayer.parent.transform.localRotation = rotation;
            exitSphere.AddComponent<FaceCamera>();

            // Get the tooltip handler reference
            tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
        }
    }

    public void OnMenuItemSelect(MenuElement menuElement)
    {
        if (!menuElement.isSelected)
        {
            menuElement.isSelected = true;

            switch (menuElement.name)
            {
                case "Description":

                    // Animate all items to the zero position
                    for (int i = 0; i < currentMenuLayer.items.Count; i++)
                    {
                       StartCoroutine(AnimateMenuItemToZero(i, true));         // Scaling to zero
                    }

                    // Share content sphere as object info
                    object[] info = new object[] { contentSphere.transform.parent.GetComponent<PhotonView>().ViewID };

                    // Create description box
                    menuElement.menuLayer.items.Clear();
                    GameObject descGO = PhotonNetwork.Instantiate(descBoxPrefabLoc, menuElement.transform.position, menuElement.transform.rotation, data: info);
                    descGO.name = gameObject.name.Replace("MA_", "DB_");

                    // Set display text
                    TextMeshProUGUI displayText = descGO.transform.Find("Panel/Scroll View/Viewport/Text").GetComponent<TextMeshProUGUI>();
                    displayText.text = exhibitInfo.description.text;

                    // Set removable via button
                    descGO.GetComponent<RemoveObject>().SetRemovableStatus(true);

                    // Set as sharable
                    descGO.GetComponent<ContentSharing>().SetSharable(true);

                    ExitMenu();

                    break;

                case "Images":

                    menuElement.menuLayer.parentSelectedItem = menuElement.gameObject;

                    // Animate all items to the zero position
                    for (int i = 0; i < currentMenuLayer.items.Count; i++)
                    {
                        if (menuElement.gameObject.Equals(currentMenuLayer.items[i]))
                            StartCoroutine(AnimateMenuItemToZero(i, false));        // Not scaling to zero
                        else
                            StartCoroutine(AnimateMenuItemToZero(i, true));         // Scaling to zero
                    }

                    // Populate images
                    menuElement.menuLayer.items.Clear();
                    for (int i = 0; i < exhibitInfo.images.Length; i++)
                    {
                        // Share content sphere as object info
                        object[] sphereInfo = new object[] { contentSphere.transform.parent.GetComponent<PhotonView>().ViewID };

                        // Instantiate and rename image
                        GameObject imageGO = PhotonNetwork.Instantiate(imagePrefabLoc, menuElement.menuLayer.parent.transform.position, menuElement.menuLayer.parent.transform.rotation, data: sphereInfo);
                        imageGO.transform.SetParent(menuElement.menuLayer.parent.transform);
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

                        // MenuElement imageMA = imageGO.GetComponent<MenuElement>();
                        // imageMA.menuArea = this;
                        var interactable = imageGO.GetComponent<XRGrabInteractable>();

                        // Remove from menu list after dragging
                        interactable.selectEntered.AddListener((SelectEnterEventArgs) => { menuElement.menuLayer.items.Remove(imageGO); });

                        // Set removable via button
                        interactable.selectEntered.AddListener((SelectEnterEventArgs) => { imageGO.GetComponent<RemoveObject>().SetRemovableStatus(true); });

                        // Set as sharable
                        interactable.selectEntered.AddListener((SelectEnterEventArgs) => { imageGO.GetComponent<ContentSharing>().SetSharable(true); });

                        menuElement.menuLayer.items.Add(imageGO);
                    }

                    StartCoroutine(LoadNextMenuLayer(menuElement));

                    break;

                case "Audio":

                    SelectMenuAction(menuElement);

                    break;

                case "DetailView":

                    SelectMenuAction(menuElement);

                    break;

                default:

                    SelectMenuAction(menuElement);

                    break;
            }
        }
    }

    private IEnumerator LoadNextMenuLayer(MenuElement mE)
    {
        yield return new WaitForSeconds(menuItemAnimDuration);

        yield return new WaitForSeconds(nextLayerLoadDelay);

        // Reset item final positions array
        menuItemFinalPositions.Clear();        

        UpdateCurrentMenuItems(mE.menuLayer);

        // Reset pull distance temporarily for animation
        currentPullDistance = 0f;
    }

    private void SelectMenuAction(MenuElement menuElement)
    {
        // Animate all items to the zero position
        for (int i = 0; i < currentMenuLayer.items.Count; i++)
        {
            if (menuElement.gameObject.Equals(currentMenuLayer.items[i]))
                StartCoroutine(AnimateMenuItemToZero(i, false));       // Not scaling to zero
            else
                StartCoroutine(AnimateMenuItemToZero(i, true));        // Scaling to zero
        }

        UnityEvent selectActions = menuElement.selectActions;
        selectActions.Invoke();
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
        while (t < menuItemAnimDuration)
        {
            // Animate position
            currentMenuLayer.items[index].transform.localPosition = Vector3.Lerp(startPos, endPos, t / menuItemAnimDuration);

            // Animate scale
            if (scaleToZero)
                currentMenuLayer.items[index].transform.localScale = Vector3.Lerp(startScale, endScale, t / menuItemAnimDuration);

            t += Time.deltaTime;
            yield return null;
        }

        // Set final position
        currentMenuLayer.items[index].transform.localPosition = endPos;

        // Set final scale
        if (scaleToZero)
            currentMenuLayer.items[index].transform.localScale = endScale;

        // Disable collider
        currentMenuLayer.items[index].GetComponent<Collider>().enabled = false;
    }

    public void ExitMenu()
    {
        for (int i = 0; i < currentMenuLayer.items.Count; i++)
        {
            if (currentMenuLayer.items[i].GetComponent<MenuElement>() != null)
            {
                currentMenuLayer.items[i].GetComponent<MenuElement>().ResetParameters();
            }
            StartCoroutine(AnimateMenuItemToZero(i, true));
        }

        StartCoroutine(AnimateExitSphereToZero());        
    }

    private void ResetMenu()
    {
        // Reset Variables
        menuOpen = false;
        firstLayerOpen = false;
        GetComponent<InteractionGuide>().isMenuOpen = false;
        currentPullDistance = 0f;

        // Reset top layer menu options
        foreach (GameObject gO in menuLayer.items)
        {
            gO.transform.localPosition = Vector3.zero;
            gO.transform.localScale = Vector3.zero;
            gO.GetComponent<MenuElement>().DisableCollider();
            if (gO.GetComponent<FaceCamera>() != null)
                Destroy(gO.GetComponent<FaceCamera>());
        }
        menuLayer.isAtFinalPos = false;

        // Reset top layer menu parent local position
        menuLayer.parent.transform.localPosition = Vector3.zero;

        // Reset current selected parent option
        if (currentMenuLayer.parentSelectedItem != null)
        {
            currentMenuLayer.parentSelectedItem.transform.localScale = Vector3.zero;
            currentMenuLayer.parentSelectedItem.GetComponent<MenuElement>().ResetParameters();
            currentMenuLayer.parentSelectedItem.GetComponent<MenuElement>().DisableCollider();
            if (currentMenuLayer.parentSelectedItem.GetComponent<FaceCamera>() != null)
                Destroy(currentMenuLayer.parentSelectedItem.GetComponent<FaceCamera>());
        }

        foreach (Transform child in imageContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Destroy content sphere
        contentSphere = contentSphere.transform.parent.gameObject;
        contentSphere.GetComponent<ContentSphere>().DestroySphere();

        // Re-enable collider for new menu interaction
        EnableCollider();
    }

    private IEnumerator AnimateExitSphereToZero()
    {
        // Get start and end position values
        Vector3 startPos = exitSphere.transform.localPosition;
        Vector3 endPos = Vector3.zero;

        // Get start and end scale values
        Vector3 startScale = Vector3.one * exitSphereScale;
        Vector3 endScale = Vector3.zero;

        float t = 0f;
        while (t < menuItemAnimDuration)
        {
            // Animate position
            exitSphere.transform.localPosition = Vector3.Lerp(startPos, endPos, t / menuItemAnimDuration);

            // Animate scale
            exitSphere.transform.localScale = Vector3.Lerp(startScale, endScale, t / menuItemAnimDuration);

            t += Time.deltaTime;
            yield return null;
        }

        exitSphere.transform.localPosition = endPos;
        exitSphere.transform.localScale = endScale;
        exitSphere.GetComponent<SphereCollider>().enabled = false;

        yield return new WaitForSeconds(menuItemAnimDuration);

        ResetMenu();
    }
}
