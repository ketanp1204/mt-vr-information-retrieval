using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SelectiveBlur : MonoBehaviour
{

    /* Private Variables */

    private DepthOfField depthOfField;
    private Transform selectedObject;
    [SerializeField] private float blurSize = 5f;
    [SerializeField] private float focusSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
         var postProcessVolume = GetComponent<PostProcessVolume>();
         postProcessVolume.profile.TryGetSettings(out depthOfField);
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedObject == null)
            return;

        float focusDistance = Vector3.Distance(transform.position, selectedObject.position);
        depthOfField.focusDistance.value = Mathf.Lerp(depthOfField.focusDistance.value, focusDistance, Time.deltaTime * focusSpeed);
        depthOfField.aperture.value = blurSize / Mathf.Max(focusDistance, 0.001f);
    }

    public void SetSelectedObject(Transform newObject)
    {
        selectedObject = newObject;
    }
}
