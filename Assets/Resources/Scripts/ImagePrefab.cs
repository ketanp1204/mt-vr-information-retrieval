using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImagePrefab : MonoBehaviour
{
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetImage(Texture2D image)
    {
        // Set and resize image on the child
        Transform child = transform.GetChild(0);
        RawImage rI = child.GetComponent<RawImage>();
        rI.texture = image;
        rI.SetNativeSize();

        // Resize box collider
        BoxCollider c = child.GetComponent<BoxCollider>();
        RectTransform rt = child.GetComponent<RectTransform>();
        c.size = new Vector3(rt.rect.width, rt.rect.height, c.size.z);

    }
}
