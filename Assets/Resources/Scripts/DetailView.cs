using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;
using Vrsys;

public class DetailView : MonoBehaviour
{
    /* Public Variables */
    public GameObject detailViewGO;

    /* Private Variables */
    [SerializeField] private List<GameObject> focusObjects;
    private bool isInDetailView = false;
    private SphereCollider col;
    private ScreenFade screenFade;
    private GameObject userGO;
    private GameObject displayGO;
    private Transform detailViewAreaTransform;

    public void Start()
    {
        col = GetComponent<SphereCollider>();
        detailViewAreaTransform = GameObject.Find("DetailViewingArea").transform;
    }

    public void HandleSelect()
    {
        if (!isInDetailView)
        {
            // Get screen fade component of user
            screenFade = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "Main Camera").GetComponentInChildren<ScreenFade>();

            // Get user GameObject
            userGO = Vrsys.NetworkUser.localNetworkUser.gameObject;

            // Enter detail view
            StartCoroutine(EnterDetailViewingArea());

            isInDetailView = true;
        }
        else
        {
            // Get screen fade component of user
            screenFade = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "Main Camera").GetComponentInChildren<ScreenFade>();

            // Exit detail view
            StartCoroutine(ExitDetailViewingArea());

            isInDetailView = false;
        }
    }

    private IEnumerator EnterDetailView()
    {
        // Fade the screen out
        screenFade.FadeOut();

        // Disable collider
        col.enabled = false;

        // Wait for screen fade
        yield return new WaitForSeconds(screenFade.fadeDuration);

        // Load detail view objects
        detailViewGO.SetActive(true);

        // Set focused objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        focus.SetFocused(focusObjects);
        
        // Enable collider
        col.enabled = true;

        // Fade the screen in
        screenFade.FadeIn();
    }

    private IEnumerator EnterDetailViewingArea()
    {
        // Fade the screen out 
        screenFade.FadeOut();

        // Disable Collider
        col.enabled = false;

        // Wait for screen fade
        yield return new WaitForSeconds(screenFade.fadeDuration);

        // Show the representation of the player in the original location
        userGO.GetComponent<NetworkUser>().ShowUserDisplay();
        
        
        /*
        displayGO.transform.SetParent(userGO.transform.parent);
        displayGO.SetActive(true);
        displayGO.GetComponent<UserDisplaySync>().SyncUserDisplay(true);
        */

        // TODO: Teleport the player to the detail viewing area
        var player = Vrsys.NetworkUser.localNetworkUser.gameObject.transform;
        player.position = new Vector3(player.localPosition.x + detailViewAreaTransform.localPosition.x,
                                      player.localPosition.y,
                                      player.localPosition.z + detailViewAreaTransform.localPosition.z);

        // Set focused objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        focus.SetFocused(focusObjects);

        // Enable collider
        col.enabled = true;

        // Fade the screen in
        screenFade.FadeIn();
    }

    private IEnumerator ExitDetailViewingArea()
    {
        // Fade the screen out
        screenFade.FadeOut();

        // Disable collider
        col.enabled = false;

        // Wait for screen fade
        yield return new WaitForSeconds(screenFade.fadeDuration);

        // TODO: Teleport the player back to the original location
        var player = Vrsys.NetworkUser.localNetworkUser.gameObject.transform;
        player.position = new Vector3(player.localPosition.x - detailViewAreaTransform.localPosition.x,
                                      player.localPosition.y,
                                      player.localPosition.z - detailViewAreaTransform.localPosition.z);


        // Remove the representation of the user from the original location
        userGO.GetComponent<NetworkUser>().DestroyUserDisplay();

        /*
        // Remove the representation of the user from the original location
        displayGO.transform.SetParent(userGO.transform);
        displayGO.GetComponent<UserDisplaySync>().SyncUserDisplay(false); 
        displayGO.SetActive(false);
        */


        // Unset focused objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        List<GameObject> n = null;
        focus.SetFocused(n);

        // Enable collider
        col.enabled = true;

        // Fade the screen in
        screenFade.FadeIn();
    }

    private IEnumerator ExitDetailView()
    {
        // Fade the screen out
        screenFade.FadeOut();

        // Disable collider
        col.enabled = false;

        // Wait for screen fade
        yield return new WaitForSeconds(screenFade.fadeDuration);

        // Hide detail view objects
        detailViewGO.SetActive(false);

        // Unset focused objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        List<GameObject> n = null;
        focus.SetFocused(n);

        // Enable collider
        col.enabled = true;

        // Fade the screen in
        screenFade.FadeIn();
    }
}
