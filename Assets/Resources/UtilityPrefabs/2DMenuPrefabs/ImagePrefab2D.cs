using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImagePrefab2D : MonoBehaviour
{

    // Public Variables //

    public Image imageComp;
    public Button imageButton;


    // Private Variables //

    private float imageHeight = 22f;


    public void SetData(Sprite image, string imageText, TextBox textBox)
    {
        // Set and resize image on the child
        imageComp.sprite = image;
        imageComp.SetNativeSize();
        float aspectRatio = imageComp.rectTransform.rect.width / imageComp.rectTransform.rect.height;

        // Resize parent rect
        float rectWidth = imageHeight * aspectRatio;
        RectTransform rT = (RectTransform)transform;
        rT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectWidth);
        rT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageHeight);

        // Rescale image component gameObject rect
        imageComp.GetComponent<RectTransform>().localScale = Vector3.one * (imageHeight / imageComp.rectTransform.rect.height);

        // Set display text on button click
        imageButton.onClick.AddListener(() => { textBox.DisplayText(imageText); });
    }
}
