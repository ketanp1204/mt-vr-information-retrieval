using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaRealObject : MonoBehaviour
{
    public string objectName;
    public string authorName;
    public string knowledgeID;
    public GameObject labelGO;
    public GameObject basicInfoGO;
    public GameObject detailViewOptionGO;
    public GameObject actionMenuGO;
    public List<MetaRealInteractable> mrInteractables = new List<MetaRealInteractable>();

    public bool isInDetailView = false;

    // Start is called before the first frame update
    void Start()
    {
        FindGO("Label");
        FindGO("BasicInfo");
        FindGO("DetailViewDrag");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowLabel()
    {
        FindGO("Label");
        
        if (labelGO != null)
        {
            labelGO.SetActive(true);
            FaceCamera faceCamera = labelGO.GetComponent<FaceCamera>();
            if (faceCamera != null && !faceCamera.isInitialized)
            {
                faceCamera.cam = Vrsys.NetworkUser.localNetworkUser.GetCamera().gameObject;
                faceCamera.isInitialized = true;
            }
        }
    }

    public void HideLabel()
    {
        labelGO.SetActive(false);
    }

    public void FadeLabel(int afterXSeconds)
    {
        StartCoroutine(HideLabel(afterXSeconds));
    }

    IEnumerator HideLabel(int secs)
    {
        yield return new WaitForSeconds(secs);
        labelGO.SetActive(false);
    }

    public void ShowBasicInfo()
    {
        FindGO("BasicInfo");

        if (basicInfoGO != null)
        {
            if (!basicInfoGO.activeSelf)
            {
                basicInfoGO.SetActive(true);
                FaceCamera faceCamera = basicInfoGO.GetComponent<FaceCamera>();
                if (faceCamera != null && !faceCamera.isInitialized)
                {
                    faceCamera.cam = Vrsys.NetworkUser.localNetworkUser.GetCamera().gameObject;
                    faceCamera.isInitialized = true;
                }
            }
        }
    }

    public void HideBasicInfo()
    {
        basicInfoGO.SetActive(false);
    }

    public void ShowDetailViewOption()
    {
        FindGO("DetailViewDrag");

        if(detailViewOptionGO != null)
        {
            if (!detailViewOptionGO.activeSelf)
            {
                detailViewOptionGO.SetActive(true);
                FaceCamera faceCamera = detailViewOptionGO.GetComponent<FaceCamera>();
                if (faceCamera != null && !faceCamera.isInitialized)
                {
                    faceCamera.cam = Vrsys.NetworkUser.localNetworkUser.GetCamera().gameObject;
                    faceCamera.isInitialized = true;
                }
            }
        }
    }

    public void HideDetailViewOption()
    {
        detailViewOptionGO.SetActive(false);
    }

    private void FindGO(string gameObjectName)
    {
        switch (gameObjectName)
        {
            case "Label":
                if (labelGO == null)
                {
                    labelGO = Vrsys.Utility.FindRecursive(this.transform.gameObject, gameObjectName);
                }
                break;
            case "BasicInfo":
                if (basicInfoGO == null)
                {
                    basicInfoGO = Vrsys.Utility.FindRecursive(this.transform.gameObject, gameObjectName);
                }
                break;
        }
    }

}
