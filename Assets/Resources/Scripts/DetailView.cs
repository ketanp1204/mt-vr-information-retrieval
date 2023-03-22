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

    public void Start()
    {
        col = GetComponent<SphereCollider>();
    }

    public void HandleSelect()
    {
        if (!isInDetailView)
        {
            // Get screen fade component of user
            screenFade = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "Main Camera").GetComponentInChildren<ScreenFade>();

            // Enter detail view
            StartCoroutine(EnterDetailView());

            isInDetailView = true;
        }
        else
        {
            // Get screen fade component of user
            screenFade = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "Main Camera").GetComponentInChildren<ScreenFade>();

            // Exit detail view
            StartCoroutine(ExitDetailView());

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
