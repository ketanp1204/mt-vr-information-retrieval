using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FaceCamera : MonoBehaviour
{
    public GameObject cam;
    public Vector3 rotationOffset = new Vector3(0f, 0f, 0f);
    public bool rotateTowardsUser = true;
    public bool isInitialized = false;
    public bool objectActiveOnStart = false;
    public bool rotateX = true;
    public bool rotateY = true;
    public bool rotateZ = true;

    // Start is called before the first frame update
    void Start()
    {
        if (!objectActiveOnStart)
        {
            if (cam == null)
            {
                cam = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localGameObject, "Main Camera");
            }
            if (cam == null)
            {
                cam = Camera.main.gameObject;
            }
            if (cam != null)
            {
                isInitialized = true;
            }
        }
    }

    public void SetUserCamera()
    {
        if (cam == null)
        {
            cam = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localGameObject, "Main Camera");
            isInitialized = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!isInitialized)
        {
            return;
        }
        
        // update view direction
        if (rotateTowardsUser && cam != null)
        {
            Vector3 directionToCam = transform.position - cam.transform.position;
            Quaternion rotation = Quaternion.LookRotation(directionToCam, Vector3.up);

            if (!rotateX)
            {
                rotation.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, rotation.eulerAngles.z);
            }
            if (!rotateY)
            {
                rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, 0f, rotation.eulerAngles.z);
            }
            if (!rotateZ)
            {
                rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, 0f);
            }

            //transform.LookAt(cam.transform.position);
            //transform.localRotation *= Quaternion.Euler(rotationOffset);
            transform.localRotation = rotation * Quaternion.Euler(rotationOffset);
        }
    }
}
