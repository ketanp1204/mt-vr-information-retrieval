using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MenuArea : XRSimpleInteractable
{
    /* Public Variables */
    [Space(20)]
    [Header("Interaction Properties")]
    public InputActionReference controllerTrigger;
    public Tooltip gripHoldTooltip;
    public GameObject menuSpherePrefab;
    public List<GameObject> menuActionPrefabs;
    public GameObject linePrefab;
    public float pullDistance = 0.3f;


    /* Private Variables */
    [SerializeField] private float circleRadius = 0.2f;                   
    private TooltipHandler tooltipHandler;
    private Vector3 interactionInitialPos;
    private GameObject menuSphere;
    private Transform controllerTransform;
    private GameObject menuLine;
    private LineRenderer lR;
    private List<GameObject> menuActions = new List<GameObject>();
    private List<Vector3> menuActionFinalPositions = new List<Vector3>();
    private Vector3 sphereMaxScale = new Vector3(0.14f, 0.14f, 0.14f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Scale the sphere after it is created
        if (isSelected && menuSphere != null && menuSphere.transform.localScale.magnitude < sphereMaxScale.magnitude && menuActions.Count > 0)
        {
            // Calculate the direction where the player is moving the controller
            float moveZ = controllerTransform.InverseTransformPoint(interactionInitialPos).z;

            // Scale the sphere if pulling in
            if (moveZ > 0)
            {
                float displacement = Vector3.Distance(controllerTransform.position, interactionInitialPos) / pullDistance;
                float menuSphereScale = math.remap(0f, 1f, 0f, 0.14f, displacement);
                float menuActionScale = math.remap(0f, 1f, 0f, 0.04f, displacement);
                menuSphere.transform.localScale = new Vector3(menuSphereScale, menuSphereScale, menuSphereScale);

                for (int i = 0; i < menuActions.Count; i++)
                {
                    menuActions[i].transform.localPosition = Vector3.Lerp(Vector3.zero, menuActionFinalPositions[i], displacement);
                    menuActions[i].transform.localScale = new Vector3(menuActionScale, menuActionScale, menuActionScale);
                }
            }
        }

        if (isSelected && controllerTransform != null && lR != null)
        {
            lR.SetPosition(1, controllerTransform.position);
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
        menuSphere = GameObject.Instantiate(menuSpherePrefab, interactionInitialPos, rotation);
        menuSphere.transform.localScale = Vector3.zero;

        // Create a line from the center of interaction to the controller's current position
        menuLine = GameObject.Instantiate(linePrefab, menuSphere.transform);
        lR = menuLine.GetComponent<LineRenderer>();
        lR.positionCount = 2;
        lR.SetPosition(0, interactionInitialPos);

        // Set the position of the parent of menu items to the center of interaction
        transform.Find("Menus").position = interactionInitialPos;

        // Get all menu item GOs
        foreach(Transform child in transform.Find("Menus").GetComponentInChildren<Transform>())
        {
            menuActions.Add(child.gameObject);
            child.GetComponent<FaceCamera>().SetUserCamera();
        }

        // Save the end positions of the menu items
        for(int i = 0; i < menuActions.Count; i++)
        {
            Vector3 endPos = CalculateChildPosition(i, menuActions.Count, circleRadius);
            menuActionFinalPositions.Add(endPos);
        }
    }

    private Vector3 CalculateChildPosition(int childIndex, int totalChildren, float radius)
    {
        float angle = (float)childIndex / (float)totalChildren * Mathf.PI * 2f;
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        return new Vector3(x, y, pullDistance);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        Destroy(menuLine);
    }
}
