using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSlideshow : MonoBehaviour
{
    /* Public Variables */
    public Texture[] images;

    /* Private Variables */
    [SerializeField] private float delay = 1.5f;
    private int currentImageIndex = 0;
    private RawImage imageComponent;
    private float lastImageTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        imageComponent = GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= delay + lastImageTime)
        {
            lastImageTime = Time.time;
            currentImageIndex = (currentImageIndex + 1) % images.Length;
            imageComponent.texture = images[currentImageIndex];
        }
    }
}
