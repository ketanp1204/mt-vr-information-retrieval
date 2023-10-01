using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class ContentSharing : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    // Public Variables

    public GameObject visibilityObject;
    public Vector3 guideSpawnOffset;
    public string guideText = "Drag out to Share";
    // public GameObject shareTextPrefab;


    // Private Variables

    private GameObject contentSphere;
    private bool isSharable = false;
    private bool shareTextDisplayed = false;
    private float distanceFromSphere = 0f;
    private float animDuration = 0.1f;
    private ParticleSystem particles;

    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        /*
        object[] data = info.photonView.InstantiationData;

        int sphereViewID = (int)data[0];

        contentSphere = PhotonView.Find(sphereViewID).transform.Find("Sphere").gameObject;
        particles = contentSphere.GetComponent<ParticleSystem>();

        // Hide the content for others
        if (!photonView.IsMine)
        {
            visibilityObject.SetActive(false);
        }
        */
    }

    void Update()
    {
        if(isSharable && contentSphere != null)
        {   
            distanceFromSphere = Vector3.Distance(transform.position, contentSphere.transform.position);

            if (distanceFromSphere > (0.6f * (contentSphere.transform.localScale.x / 2)) && !shareTextDisplayed)
            {
                shareTextDisplayed = true;

                StartCoroutine(DisplayShareText());

                particles.Play();
            } 
            else if (distanceFromSphere < (0.6f * (contentSphere.transform.localScale.x / 2)) && shareTextDisplayed)
            {
                particles.Stop();

                shareTextDisplayed = false;
            }
        }        
    }

    private IEnumerator DisplayShareText()
    {
        GameObject shareGuide = Instantiate(Resources.Load("UtilityPrefabs/GuideCanvas") as GameObject,
                                            transform.position + guideSpawnOffset,
                                            Quaternion.identity);

        shareGuide.transform.Find("Panel/GuideText").GetComponent<TextMeshProUGUI>().text = guideText;

        CanvasGroup cG = shareGuide.GetComponent<CanvasGroup>();

        float t = 0f;
        while (t < animDuration)
        {
            cG.alpha = Mathf.Lerp(0f, 1f, t / animDuration);

            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(3f);

        t = 0f;
        while (t < animDuration)
        {
            cG.alpha = Mathf.Lerp(1f, 0f, t / animDuration);

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(shareGuide);
    }

    public void SetSharable(bool sharable)
    {
        StartCoroutine(SetSharableAfterDelay(sharable));
    }

    private IEnumerator SetSharableAfterDelay(bool sharable)
    {
        yield return new WaitForSeconds(0.1f);

        photonView.RPC(nameof(SetSharableRPC), RpcTarget.All, sharable);
    }

    [PunRPC]
    void SetSharableRPC(bool sharable)
    {
        isSharable = sharable;
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == contentSphere && isSharable)
        {
            isSharable = false;
            photonView.RPC(nameof(SetVisibilityRPC), RpcTarget.All, true);
            Debug.Log("visible to all");
            particles.Stop();
        }
    }

    [PunRPC]
    void SetVisibilityRPC(bool visible)
    {
        visibilityObject.SetActive(visible);
    }

    public void HandleSelect(SelectEnterEventArgs args)
    {
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
    }
}
