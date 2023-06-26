using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class XROffsetGrabInteractable : XRGrabInteractable
{
    private void Start()
    {
        if (!attachTransform)
        {
            GameObject attachPoint = new GameObject("Offset Grab Pivot");
            attachPoint.transform.SetParent(transform, false);
            attachTransform = attachPoint.transform;
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        attachTransform.position = args.interactorObject.transform.position;
        attachTransform.rotation = args.interactorObject.transform.rotation;

        base.OnSelectEntered(args);
    }

    /*
    // Public Variables

    
    public GameObject menuParent;
    public List<GameObject> menuItems;
    public float menuItemMaxScaleValue;
    [HideInInspector]
    public bool menuSelected;

    [Space(20)]
    [Header("Interaction Properties")]
    public InputActionReference controllerTrigger;
    public InputActionReference controllerPrimaryButton;
    public Tooltip gripHoldTooltip;
    public GameObject menuSpherePrefab;
    public GameObject linePrefab;
    public float pullDistance = 0.3f;
    public float selectItemAnimDuration = 0.1f;



    // Private Variables

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
    private string menuSpherePrefabLoc = "UtilityPrefabs/MenuSphere";
    private float menuSphereInitialZ = 0f;


    private void Update()
    {
        // Menu items animation
        if (isSelected)
        {
            if (currentPullDistance < 1f)
            {
                if (menuItems.Count > 0 && !menuSelected)
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
                        float currentActionScale = math.remap(0f, 1f, 0f, menuItemMaxScaleValue, currentPullDistance);

                        // Move the menu sphere
                        float sphereZ = Mathf.Lerp(menuSphereInitialZ, menuSphereInitialZ + 0.15f, currentPullDistance);
                        menuSphere.transform.position = new Vector3(menuSphere.transform.position.x, menuSphere.transform.position.y, sphereZ);

                        // Move and scale the menu items
                        for (int i = 0; i < menuItems.Count; i++)
                        {
                            menuItems[i].transform.localPosition = Vector3.Lerp(Vector3.zero, menuItemFinalPositions[i], currentPullDistance);
                            menuItems[i].transform.localScale = new Vector3(currentActionScale, currentActionScale, currentActionScale);
                        }
                    }
                }
            }
            else if (currentPullDistance > 1f)
            {
                if (menuItems.Count > 0)
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
                menuSelected = true;

                // Check whether there is a sub menu
                var mE = currentlyHoveredMenuItem.GetComponent<MenuElement>();
                if (mE.hasSubMenu)
                {

                }
            }

            // Set line end position to the controller's position
            if (controllerTransform != null && lR != null)
            {
                lR.SetPosition(1, controllerTransform.position);
            }
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

    */
}
