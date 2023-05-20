using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageBoxResizer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RectTransform parentRect = GetComponentInParent<RectTransform>();
        Vector3 scale = new Vector3(parentRect.rect.width, parentRect.rect.height, transform.localScale.z);
        transform.localScale = scale;
    }
}
