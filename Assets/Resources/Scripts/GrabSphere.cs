using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Mathematics;
using Photon.Pun;

public class GrabSphere : MonoBehaviourPunCallbacks
{
    // Private Variables
    private MeshRenderer rend;

    public void SetVisibility(bool visible)
    {
        rend = GetComponent<MeshRenderer>();
        photonView.RPC(nameof(SetVisibilityRPC), RpcTarget.All, visible);
    }


    [PunRPC]
    void SetVisibilityRPC(bool visible)
    {
        if (!photonView.IsMine)
        {
            rend.enabled = visible;
            Debug.Log("not mine");
        }
        
    }

    /*
    // Public Variables 
    public float grabSnapLimit = 0.35f;

    // Private Variables
    private Vector3 initialPosition;
    private Vector3 controllerInitPos;
    private Vector3 controllerExitPos;
    private bool faceCameraSet = false;
    private Transform controllerTransform;
    private Rigidbody rb;
    private float jointLinearLimit;
    private bool isAtInitialPosition = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        jointLinearLimit = GetComponent<ConfigurableJoint>().linearLimit.limit;
        // transform.position -= new Vector3(0f, 0f, -jointLinearLimit);
        initialPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (isSelected && controllerTransform != null && isAtInitialPosition)
        {
            // Move the sphere in the controller direction
            rb.velocity = ((initialPosition + controllerTransform.position - controllerInitPos) - initialPosition) / Time.deltaTime;

            // Scale the sphere as it moves
            float displacement = Mathf.Abs(transform.position.z - initialPosition.z) / (jointLinearLimit * 2);
            float scale = math.remap(0f, 1f, 0.07f, 0.14f, displacement);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public void HandleSelectEntered()
    {

    }

    public void HandleSelectExited()
    {

    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        //Debug.Log(args.interactorObject.transform.gameObject);
        //Debug.Log(args.interactorObject.transform.position); 

        controllerTransform = args.interactorObject.transform;
        controllerInitPos = controllerTransform.position;

        if (transform.position.z == initialPosition.z)
        {
            isAtInitialPosition = true;
        }
        else
        {
            isAtInitialPosition = false;
            StartCoroutine(MoveSphereToInitialPosition(0.3f));
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (isAtInitialPosition)
        {
            //Debug.Log(args.interactorObject.transform.position);
            controllerExitPos = controllerTransform.position;

            float displacement = Mathf.Abs(transform.position.z - initialPosition.z) / (jointLinearLimit * 2);

            if (displacement < grabSnapLimit)
            {
                transform.position = initialPosition;
                rb.velocity = Vector3.zero;
                transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
            }
            else
            {
                StartCoroutine(ScaleSphereToFinalPosition());
            }
        }
    }

    private IEnumerator MoveSphereToInitialPosition(float duration)
    {
        Vector3 startPos = transform.position;
        Vector3 finalPos = initialPosition;

        float t = 0f;

        while (t < duration)
        {
            transform.position = Vector3.Lerp(startPos, finalPos, (t / duration));

            float scale = math.remap(0f, duration, 0.14f, 0.07f, t);
            transform.localScale = new Vector3(scale, scale, scale);

            t += Time.deltaTime;
            yield return null;
        }

        transform.position = initialPosition;
        transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
    }

    private IEnumerator ScaleSphereToFinalPosition()
    {
        float displacement = Mathf.Abs(transform.position.z - initialPosition.z) / (jointLinearLimit * 2);

        while (displacement <= jointLinearLimit * 2 - 0.1f)
        {
            yield return new WaitForEndOfFrame();
            displacement = Mathf.Abs(transform.position.z - initialPosition.z) / (jointLinearLimit * 2);
            float scale = math.remap(0f, 1f, 0.07f, 0.14f, displacement);
            transform.localScale = new Vector3(scale, scale, scale);
        }

        transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
    }

    public void SetFaceCamera()
    {
        if (!faceCameraSet)
        {
            Debug.Log("setfacecamera");
            FaceCamera faceCamera = GetComponent<FaceCamera>();
            if (faceCamera != null && !faceCamera.isInitialized)
            {
                faceCamera.cam = Vrsys.NetworkUser.localNetworkUser.GetCamera().gameObject;
                faceCamera.isInitialized = true;
                faceCameraSet = true;
            }
        }
    }
    */
}
