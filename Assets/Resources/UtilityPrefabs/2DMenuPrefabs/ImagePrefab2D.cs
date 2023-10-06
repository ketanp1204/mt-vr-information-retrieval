using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImagePrefab2D : MonoBehaviour
{

    // Public Variables //

    public Image imageComp;
    public Button imageButton;



    public void SetData(Sprite image, string imageText, TextBox textBox)
    {
        // Set and resize image on the child
        imageComp.sprite = image;
        imageComp.SetNativeSize();
        float aspectRatio = imageComp.rectTransform.rect.width / imageComp.rectTransform.rect.height;

        if (aspectRatio < 1)
        {
            // Resize parent rect
            float rectWidth = 23f * aspectRatio;
            RectTransform rT = (RectTransform)transform;
            rT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectWidth);
            rT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 23f);

            // Rescale image component gameObject rect
            imageComp.GetComponent<RectTransform>().localScale = Vector3.one * (23f / imageComp.rectTransform.rect.height);
        }
        else
        {
            // Resize parent rect
            float rectWidth = 17f * aspectRatio;
            RectTransform rT = (RectTransform)transform;
            rT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectWidth);
            rT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 17f);

            // Rescale image component gameObject rect
            imageComp.GetComponent<RectTransform>().localScale = Vector3.one * (17f / imageComp.rectTransform.rect.height);
        }

        // Set display text on button click
        imageButton.onClick.AddListener(() => { textBox.DisplayText(imageText); });
    }
}
