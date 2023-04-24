using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XROffsetGrabInteractable : XRGrabInteractable
{
    private Vector3 initialAttachLocalPos;
    private Quaternion initialAttachLocalRot;

    private MenuSphere menuSphere;

    public bool isWithinSnapZone { get; set; }   

    private Vector3 initialPosition;
    private Vector3 userCameraForward;
    private GameObject userCamera;



    [SerializeField] private float maxDistance = 2.0f;          // Max distance the sphere can be moved in the user direction 
    [SerializeField] private float snapThreshold = 0.2f;        // Threshold upto which the menu sphere snaps back into its original position
    [SerializeField] private float menuActionRadius = 1f;       // Radius of the circle arrangement of the child spheres


    private Vector3 initialGrabPosition;
    private Vector3 startPos;
    private Vector3 endPos;

    private float grabDistance;

    private bool isGrabbing;
    private bool isAtMaxDistance;

    private List<GameObject> childSpheres = new List<GameObject>();
    private GameObject userController;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;

        foreach (Transform child in transform)
        {
            childSpheres.Add(child.gameObject);
        }

        
        // Create attach point
        if (!attachTransform)
        {
            GameObject grab = new GameObject("Grab Pivot");
            grab.transform.SetParent(transform, false);
            attachTransform = grab.transform;
        }

        /*
        initialAttachLocalPos = attachTransform.localPosition;
        initialAttachLocalRot = attachTransform.localRotation;
        
        isWithinSnapZone = true;

        menuSphere = GetComponent<MenuSphere>();
        */
    }

    private void Update()
    {

        /*
        if (isSelected)
        {
            if (userCamera != null)
            {
                // Movement distance along the axis between the sphere and user camera
                Vector3 camPos = userCamera.transform.position;
                Vector3 sphereToCam = camPos - transform.position;
                float movement = Vector3.Dot(sphereToCam, userCameraForward);

                // Clamp movement distance within the maximum distance
                movement = Mathf.Clamp(movement, -maxDistance, maxDistance);

                // Move the sphere along the axis between it and the user camera
                Vector3 newPos = transform.position + userCameraForward * movement;
                transform.position = newPos;

                // Update the position of the child spheres
                // UpdateChildPositions();
            }
            
        }
        */

        if (isGrabbing)
        {
            
        }

    }

    

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        attachTransform.position = args.interactorObject.transform.position;
        attachTransform.rotation = args.interactorObject.transform.rotation;

        base.OnSelectEntered(args);

        if (args.interactorObject is XRDirectInteractor)
        {
            // initialPosition = transform.position;
            // userCamera = Vrsys.Utility.FindRecursive(args.interactorObject.transform.parent.parent.gameObject, "Main Camera");
            // userCameraForward = userCamera.transform.forward;

            startPos = transform.position;
            endPos = transform.position + transform.forward * maxDistance;

            userController = args.interactorObject.transform.gameObject;
            initialGrabPosition = transform.localPosition;
            isGrabbing = true;
            isAtMaxDistance = false;
        }
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        if (args.interactorObject is XRDirectInteractor)
        {
            // transform.position = initialPosition;
            // UpdateChildPositions();

            float displacement = Vector3.Distance(transform.position, startPos);
            Debug.Log(displacement);
            float normalizedDisplacement = displacement / Vector3.Distance(startPos, endPos);
            Debug.Log(normalizedDisplacement);

            grabDistance = Vector3.Distance(initialGrabPosition, transform.localPosition);
            if (grabDistance > maxDistance * snapThreshold)
            {
                // Animate the parent sphere towards its maximum distance position
                StartCoroutine(AnimateToMaxDistance());

                // Animate the child spheres to their circle position
                StartCoroutine(AnimateChildrenToCircle());
            }
            else
            {
                // Move the menu sphere and the child spheres back to their initial positions
                transform.position = initialGrabPosition;
                for (int i = 0; i < childSpheres.Count; i++)
                {
                    childSpheres[i].transform.localPosition = Vector3.zero;
                }

                isGrabbing = false;
            }

        }

        base.OnSelectExiting(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        /*
        if (args.interactorObject is XRDirectInteractor)
        {
            // transform.position = initialPosition;
            // UpdateChildPositions();

            Debug.Log(transform.localPosition);
            grabDistance = Vector3.Distance(initialGrabPosition, transform.localPosition);
            Debug.Log(grabDistance);
            if (grabDistance > maxDistance * snapThreshold)
            {
                // Animate the parent sphere towards its maximum distance position
                StartCoroutine(AnimateToMaxDistance());

                // Animate the child spheres to their circle position
                StartCoroutine(AnimateChildrenToCircle());
            }
            else
            {
                // Move the menu sphere and the child spheres back to their initial positions
                transform.position = initialGrabPosition;
                for (int i = 0; i < childSpheres.Count; i++)
                {
                    childSpheres[i].transform.localPosition = Vector3.zero;
                }

                isGrabbing = false;
            }

        }
        */
    }

    private IEnumerator AnimateToMaxDistance()
    {
        Vector3 currentPos = transform.localPosition;
        Vector3 maxPos = initialGrabPosition + maxDistance * transform.forward;

        float t = 0f;
        while (t < 0.4f)        // 0.4f - Duration of remaining animation
        {
            transform.localPosition = Vector3.Lerp(currentPos, maxPos, t / 0.4f);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = maxPos;
        isGrabbing = false;
    }

    private IEnumerator AnimateChildrenToCircle()
    {
        for (int i = 0; i < childSpheres.Count; i++)
        {
            Vector3 startPos = childSpheres[i].transform.localPosition;
            Vector3 endPos = CalculateChildPosition(i, childSpheres.Count, menuActionRadius);

            float t = 0f;
            while (t < 0.4f)     // 0.4 - Duration of remaining animation
            {
                childSpheres[i].transform.localPosition = Vector3.Lerp(startPos, endPos, t / 0.4f);
                t += Time.deltaTime;
                yield return null;
            }

            childSpheres[i].transform.localPosition = endPos;
            isGrabbing = false;
        }
    }

    private Vector3 CalculateChildPosition(int childIndex, int totalChildren, float radius)
    {
        float angle = (float)childIndex / (float)totalChildren * Mathf.PI * 2f;
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        return new Vector3(x, y, 0f);
    }
}
