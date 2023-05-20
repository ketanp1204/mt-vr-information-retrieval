using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;
using Vrsys;
using System.Linq;

public class DetailView : MonoBehaviour
{
    /* Public Variables */
    public List<GameObject> detailViewingAreaGOs;
    public string itemName;

    public GameObject dVAWrapperGO;

    /* Private Variables */
    [SerializeField] private List<GameObject> focusObjects;
    public DetailViewManager detailViewManager;
    private bool isInDetailView = false;
    private SphereCollider col;
    private ScreenFade screenFade;
    private GameObject userGO;
    private GameObject displayGO;
    private Transform detailViewAreaTransform;

    public void Start()
    {
        col = GetComponent<SphereCollider>();
        detailViewManager = GetComponent<DetailViewManager>();
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


    private IEnumerator StartDetailView()
    {
        // Fade the screen out
        Vrsys.NetworkUser.localNetworkUser.FadeOutScreen();

        // Disable collider
        col.enabled = false;

        // Wait for screen fade
        yield return new WaitForSeconds(screenFade.fadeDuration);


        // Set focused objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        focus.SetFocused(focusObjects);
        
        // Enable collider
        col.enabled = true;

        // Fade the screen in
        Vrsys.NetworkUser.localNetworkUser.FadeInScreen();
    }
    
    
    private IEnumerator EnterDetailViewingArea()
    {
        // Fade the screen out 
        Vrsys.NetworkUser.localNetworkUser.FadeOutScreen();

        // Wait for screen fade
        yield return new WaitForSeconds(Vrsys.NetworkUser.localNetworkUser.GetScreenFadeDuration());

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
