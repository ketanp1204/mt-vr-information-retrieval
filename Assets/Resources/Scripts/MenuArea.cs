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
using Unity.VisualScripting;

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
    [Header("Exhibit Information")]
    public ExhibitInformation exhibitInfo;
    [Space(20)]
    [Header("Menu Properties")]
    public Menu menuLayer;
    public Vector3 menuRotationOffset;
    public GameObject imageContainer;
    public GameObject exitSphere;
    public float sphereMaxScale = 0.3f;
    public float exitSphereScale = 0.03f;
    public float menuItemLinearSpacing = 0.1f;
    [Space(20)]
    [Header("Tooltips")]
    public Tooltip gripHoldTooltip;
    public Tooltip exitMenuTooltip;
    [Space(20)]
    [Header("Prefabs")]
    public GameObject guidePrefab;
    public GameObject linePrefab;
    [Space(20)]
    [Header("Animation Properties")]
    public float pullDistance = 0.3f;
    public float menuItemAnimDuration = 0.1f;
    public float layerLoadDuration = 0.05f;
    public CanvasGroup gripHoldPrompt;


    // Private variables

    private BoxCollider col;
    private bool menuOpen = false;
    
    private float currentPullDistance = 0f;
    private TooltipHandler tooltipHandler;
    private Vector3 interactionInitialPos;
    private GameObject contentSphere;
    private Transform controllerTransform;
    private GameObject menuLine;
    private LineRenderer lR;
    private List<Vector3> menuItemFinalPositions = new List<Vector3>();
    
    private Menu currentMenuLayer = new();
    private string menuSpherePrefabLoc = "UtilityPrefabs/MenuSphere";
    private bool firstLayerOpen = false;
    private float menuSphereInitialZ = 0f;
    private object[] contentSphereInfo;
    private string infoVisibilityText = "Information will stay private inside the transparent sphere";
    private string menuOpenGuideText = "Open Menu\nHold the Grip Button and Pull Back";
    private string imagePrefabLoc = "UtilityPrefabs/3DMenuPrefabs/ImagePrefab3D";
    private string descBoxPrefabLoc = "UtilityPrefabs/3DMenuPrefabs/DescBoxPrefab3D";
    private string audioPrefabLoc = "UtilityPrefabs/3DMenuPrefabs/AudioPrefab3D";
    private float movementZ = 0f;

    private void Start()
    {
        col = GetComponent<BoxCollider>();
    }
    
    private void Update()
    {
        if (menuOpen)
        {
            movementZ = controllerTransform.InverseTransformPoint(interactionInitialPos).z;
            if (movementZ > 0f)
            {
                if (currentPullDistance < 1f)
                {
                    if (currentMenuLayer.items.Count > 0)
                    {
                        // Calculate the distance that the controller has moved
                        currentPullDistance = Vector3.Distance(controllerTransform.position, interactionInitialPos) / pullDistance;
                        Vector3 differenceDir = controllerTransform.position - interactionInitialPos;
                        float differenceZ = differenceDir.magnitude;
                        //Debug.Log(differenceZ);

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
                                Vector3 exitSphereFinalPos = new Vector3(-0.03f, -0.1f, pullDistance);
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

                        // Show information visibility guide
                        LoadGuide(infoVisibilityText, exitSphere.transform.position, Quaternion.identity, verticalOffset: -0.05f, destroyAfterDelay: 3f);

                    }


                    if (!currentMenuLayer.isAtFinalPos)
                    {
                        currentMenuLayer.isAtFinalPos = true;

                        // Enable item colliders
                        if (currentMenuLayer.items.Count > 0)
                        {
                            foreach (GameObject item in currentMenuLayer.items)
                            {
                                if (item.GetComponent<Collider>() != null)
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
    }

    private void LoadGuide(string guideText, Vector3 position, Quaternion rotation, float verticalOffset = 0f, float horizontalOffset = 0f, float destroyAfterDelay = 0f)
    {
        GameObject guide = Instantiate(Resources.Load("UtilityPrefabs/GuideCanvas") as GameObject,
                                                    position + new Vector3(horizontalOffset, verticalOffset, 0f),
                                                    rotation);

        guide.transform.Find("Panel/GuideText").GetComponent<TextMeshProUGUI>().text = guideText;
        StartCoroutine(FadeCanvasGroup(guide.GetComponent<CanvasGroup>(), 0f, 1f, menuItemAnimDuration, destroyAfterDelay));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cG, float startAlpha, float endAlpha, float duration, float destroyAfterDelay = 0f)
    {
        float t = 0f;
        while (t < duration)
        {
            cG.alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);

            t += Time.deltaTime;
            yield return null;
        }

        cG.alpha = endAlpha;

        if (destroyAfterDelay > 0f)
        {
            yield return new WaitForSeconds(destroyAfterDelay);
            Destroy(cG.gameObject);
        }
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);

        // Check if menu is opened
        if (!isSelected)
        {
            // Show the tooltip if not selected
            tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
            tooltipHandler.ShowTooltip(gripHoldTooltip);

            // float hOffset = args.interactorObject.transform.name.Contains("Right") ? 0.1f : -0.1f;
            // LoadGuide(menuOpenGuideText, args.interactorObject.transform.position, args.interactorObject.transform.rotation, horizontalOffset: hOffset, destroyAfterDelay: 3f);
            StartCoroutine(FadeCanvasGroup(gripHoldPrompt, 0f, 1f, menuItemAnimDuration));
        }
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);

        // Check if menu is opened
        if (!isSelected)
        {
            // Hide the tooltip if not selected
            tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
            tooltipHandler.HideTooltip(gripHoldTooltip);

            StartCoroutine(FadeCanvasGroup(gripHoldPrompt, 1f, 0f, menuItemAnimDuration));
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
            StartCoroutine(FadeCanvasGroup(gripHoldPrompt, 1f, 0f, menuItemAnimDuration));

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

            // Create a line from the center of interaction to the controller's current position
            menuLine = Instantiate(linePrefab, contentSphere.transform);
            lR = menuLine.GetComponent<LineRenderer>();
            lR.positionCount = 2;
            lR.SetPosition(0, interactionInitialPos);

            UpdateCurrentMenuItems(menuLayer);

            // Menu facing in the direction of the user
            menuLayer.parent.transform.rotation = rotation;

            // Get the tooltip handler reference
            tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
            tooltipHandler.HideTooltip(gripHoldTooltip);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (menuOpen)
        {
            if (!firstLayerOpen)
            {
                ResetMenu();

                exitSphere.transform.localPosition = Vector3.zero;
                exitSphere.transform.localScale = Vector3.zero;
                exitSphere.GetComponent<SphereCollider>().enabled = false;
            }
        }
    }

    public void OnMenuItemSelect(MenuElement menuElement)
    {
        if (!menuElement.isSelected)
        {
            menuElement.isSelected = true;
            Quaternion rot = Quaternion.identity;

            // Share content sphere as object info
            contentSphereInfo = new object[] { contentSphere.transform.parent.GetComponent<PhotonView>().ViewID };
            
            switch (menuElement.name)
            {
                case "Info":
                    
                    /*
                    // Animate all items to the zero position
                    for (int i = 0; i < currentMenuLayer.items.Count; i++)
                    {
                       StartCoroutine(AnimateMenuItemToZero(i, true));         // Scaling to zero
                    }
                    */

                    // Animate menu item to zero position
                    for (int i = 0; i  < currentMenuLayer.items.Count; i++)
                    {
                        if (currentMenuLayer.items[i] == menuElement.gameObject)
                        {
                            StartCoroutine(AnimateMenuItemToZero(i, true));  
                        }
                    }

                    // Calculate description box rotation 
                    rot = menuRotationOffset.y == 180 ? menuElement.transform.localRotation :
                                                                    menuElement.transform.localRotation;// * Quaternion.Euler(menuRotationOffset);
                    // Create description box
                    GameObject descGO = PhotonNetwork.Instantiate(descBoxPrefabLoc,
                                                                    menuElement.transform.position + new Vector3(0.04f, 0f, 0f),
                                                                    rot,
                                                                    data: contentSphereInfo);
                    
                    // Set display text
                    DescBoxPrefab dBP = descGO.GetComponent<DescBoxPrefab>();
                    // dBP.SetText(exhibitInfo.basicInfoText.text);
                    dBP.SetInfoFromExhibitInfo(exhibitInfo.exhibitName);

                    // Set removable via button
                    descGO.GetComponent<RemoveObject>().SetRemovableStatus(true);

                    // Set as sharable
                    descGO.GetComponent<ContentSharing>().SetSharable(true);

                    // ExitMenu();

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
                    for (int i = 0; i < exhibitInfo.basicInfoImages.Length; i++)
                    {
                        // Instantiate and rename image
                        GameObject imageGO = PhotonNetwork.Instantiate(imagePrefabLoc, menuElement.menuLayer.parent.transform.position,
                            menuElement.menuLayer.parent.transform.rotation, data: contentSphereInfo);
                        imageGO.transform.SetParent(menuElement.menuLayer.parent.transform);
                        imageGO.name = "Image" + (i + 1);

                        // Set and resize image
                        ImagePrefab iP = imageGO.GetComponent<ImagePrefab>();
                        // iP.SetImage(exhibitInfo.basicInfoImages[i].image);
                        // iP.SetText(exhibitInfo.basicInfoImages[i].imageText.text);
                        iP.SetInfoFromExhibitInfo(exhibitInfo.exhibitName, i, 0);

                        // MenuElement imageMA = imageGO.GetComponent<MenuElement>();
                        // imageMA.menuArea = this;
                        var interactable = imageGO.GetComponent<XRGrabInteractable>();

                        // Remove from menu list after dragging
                        interactable.selectEntered.AddListener((SelectEnterEventArgs) => { menuElement.menuLayer.items.Remove(imageGO); });

                        // Remove face camera after dragging
                        interactable.selectEntered.AddListener((SelectEnterEventArgs) => { Destroy(imageGO.GetComponent<FaceCamera>()); });

                        // Set removable via button
                        interactable.selectEntered.AddListener((SelectEnterEventArgs) => { imageGO.GetComponent<RemoveObject>().SetRemovableStatus(true); });

                        // Set as sharable
                        interactable.selectEntered.AddListener((SelectEnterEventArgs) => { imageGO.GetComponent<ContentSharing>().SetSharable(true); });

                        menuElement.menuLayer.items.Add(imageGO);
                    }

                    StartCoroutine(LoadNextMenuLayer(menuElement));

                    break;

                case "Audio":

                    /*
                    // Animate all items to the zero position
                    for (int i = 0; i < currentMenuLayer.items.Count; i++)
                    {
                        StartCoroutine(AnimateMenuItemToZero(i, true));         // Scaling to zero
                    }
                    */

                    // Animate menu item to zero position
                    for (int i = 0; i < currentMenuLayer.items.Count; i++)
                    {
                        if (currentMenuLayer.items[i] == menuElement.gameObject)
                        {
                            StartCoroutine(AnimateMenuItemToZero(i, true));
                        }
                    }

                    // Create audio box
                    rot = menuRotationOffset.y == 180 ? menuElement.transform.localRotation :
                                                                    menuElement.transform.localRotation;// * Quaternion.Euler(menuRotationOffset);

                    GameObject audioGO = PhotonNetwork.Instantiate(audioPrefabLoc, 
                                                                    menuElement.transform.position + new Vector3(0f, 0.09f, 0f), 
                                                                    rot, 
                                                                    data: contentSphereInfo);
                    audioGO.name = gameObject.name.Replace("MA_", "AB_");

                    // Set audio clip
                    AudioSource audioSource = GetChildWithName(audioGO, "Audio Source").GetComponent<AudioSource>();
                    audioSource.clip = exhibitInfo.basicInfoAudio;
                    audioSource.Play();

                    // Set removable via button
                    audioGO.GetComponent<RemoveObject>().SetRemovableStatus(true);

                    // ExitMenu();

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

        yield return new WaitForSeconds(layerLoadDuration);

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
        Vector3 startPosition = -0.5f * totalLength * parent.InverseTransformDirection(parent.right);


        Vector3 localPos = startPosition + (totalChildren - childIndex - 1) * spacing * parent.InverseTransformDirection(parent.right);
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
        if (currentMenuLayer.items[index].GetComponent<Collider>() != null) 
            currentMenuLayer.items[index].GetComponent<Collider>().enabled = false;
    }

    public void HandleExitMenuHoverEntered(HoverEnterEventArgs args)
    {
        // Check for controller
        if (args.interactorObject.transform.GetComponent<XRDirectInteractor>() != null)
        {
            tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
            tooltipHandler.ShowTooltip(exitMenuTooltip);
        }
    }

    public void HandleExitMenuHoverExited(HoverExitEventArgs args)
    {
        // Check for controller
        if (args.interactorObject.transform.GetComponent<XRDirectInteractor>() != null)
        {
            tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
            tooltipHandler.HideTooltip(exitMenuTooltip);
        }
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

    private void ResetMenu()
    {
        // Reset Variables
        menuOpen = false;
        firstLayerOpen = false;
        currentPullDistance = 0f;
        menuItemFinalPositions.Clear();

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

    private Transform GetChildWithName(GameObject gO, string childName)
    {
        Transform child = null;
        foreach (Transform t in gO.GetComponentsInChildren<Transform>())
        {
            if (t.name == childName)
            {
                child = t;
                break;
            }
        }
        return child;
    }
}
