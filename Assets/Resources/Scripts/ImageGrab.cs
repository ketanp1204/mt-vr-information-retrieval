using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageGrab : MonoBehaviour
{

    public void StartGrabbing(GameObject controller)
    {
        // Debug.Log(controller.name);

        var rect = Instantiate(gameObject, controller.transform.Find("ImageCanvas")).GetComponent<RectTransform>();
        rect.localRotation = Quaternion.Euler(0f, 180f, 0f);
        rect.anchoredPosition3D = Vector3.zero;
    }

    public void StopGrabbing()
    {
        
    }
}
