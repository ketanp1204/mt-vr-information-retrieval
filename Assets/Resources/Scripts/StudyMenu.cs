using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class StudyMenu : MonoBehaviour
{
    // Public Variables //

    [Space(20)]
    [Header("Interaction Properties")]
    public CanvasGroup studyMenuPanelCG;
    public CanvasGroup studySphereTextPanelCG;
    public CanvasGroup topCG;
    public CanvasGroup controllerInstructionsCG;
    public CanvasGroup nextButtonCG;
    public CanvasGroup skipButtonCG;
    public CanvasGroup studySelectorCG;
    public CanvasGroup startButtonCG;
    public CanvasGroup practiceButtonCG;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.1f;
    public float beginInstructionsDelay = 3f;
    public Tooltip showStudyMenuTooltip;

    [Space(20)]
    [Header("Tutorial Content")]
    public Sprite[] controllerTutorialSprites;
    public string[] controllerTutorialTexts;

    [Space(20)]
    [Header("Study Selection")]
    public GameObject study1PracticeGO;
    public GameObject study2PracticeGO;
    public GameObject study1GO;
    public GameObject study2GO;
    public GameObject studySphere;


    // Private Variables //

    private bool playerJoinedRoom = false;
    private int tutorialLayer = 1;
    private int selectedStudy = 0;
    private TooltipHandler tooltipHandler;

    

    private void Update()
    {
        if (Vrsys.NetworkUser.localNetworkUser != null && !playerJoinedRoom)
        {
            playerJoinedRoom = true;

            StartCoroutine(FadeCanvasGroup(studyMenuPanelCG, 0f, 1f, fadeInDuration, 1f, true));

            StartCoroutine(FadeCanvasGroup(topCG, 0f, 1f, fadeInDuration, 2f));

            StartCoroutine(BeginInstructions());
        }
    }

    private IEnumerator BeginInstructions()
    {
        yield return new WaitForSeconds(beginInstructionsDelay);

        // Show controller primary button controls
        controllerInstructionsCG.GetComponentInChildren<TextMeshProUGUI>().text = controllerTutorialTexts[0];
        controllerInstructionsCG.GetComponentInChildren<Image>().sprite = controllerTutorialSprites[0];
        StartCoroutine(FadeCanvasGroup(controllerInstructionsCG, 0f, 1f, fadeInDuration));

        // Show skip controls option
        StartCoroutine(FadeCanvasGroup(skipButtonCG, 0f, 1f, fadeInDuration, 0.6f, true));

        // Show next button
        StartCoroutine(FadeCanvasGroup(nextButtonCG, 0f, 1f, fadeInDuration, 0.6f, true));

        yield return new WaitForSeconds(2f);
    }

    public void SkipControls()
    {
        // Disable skip controls button
        skipButtonCG.gameObject.SetActive(false);
        
        // Disable next button
        nextButtonCG.gameObject.SetActive(false);

        // Hide Controls Tutorial
        controllerInstructionsCG.alpha = 0f;
        controllerInstructionsCG.gameObject.SetActive(false);

        // Show tooltips on the player's controllers
        TooltipHandler tH = Vrsys.NetworkUser.localNetworkUser.GetComponent<TooltipHandler>();
        tH.SetShowTooltipsBool(true);
        tH.InitializeTooltips();
        tH.ShowTriggerTooltip();

        // Show study selector 
        studySelectorCG.gameObject.SetActive(true);
        StartCoroutine(FadeCanvasGroup(studySelectorCG, 0f, 1f, fadeInDuration, enableInteraction: true));

        // Show start and practice buttons
        StartCoroutine(FadeCanvasGroup(startButtonCG, 0f, 1f, fadeInDuration, 0.6f, true));
        StartCoroutine(FadeCanvasGroup(practiceButtonCG, 0f, 1f, fadeInDuration, 0.6f, true));
    }

    public void HandleNext(Button nextButton)
    {
        nextButton.interactable = false;
        tutorialLayer += 1;

        if (tutorialLayer == 2)
        {
            // Show controller primary button controls
            controllerInstructionsCG.alpha = 0f;
            controllerInstructionsCG.GetComponentInChildren<TextMeshProUGUI>().text = controllerTutorialTexts[1];
            controllerInstructionsCG.GetComponentInChildren<Image>().sprite = controllerTutorialSprites[1];
            StartCoroutine(FadeCanvasGroup(controllerInstructionsCG, 0f, 1f, fadeInDuration));

            // Show tooltips on the player's controllers
            TooltipHandler tH = Vrsys.NetworkUser.localNetworkUser.GetComponent<TooltipHandler>();
            tH.SetShowTooltipsBool(true);
            tH.InitializeTooltips();

            // Set button interactable after delay
            StartCoroutine(SetButtonInteractableAfterDelay(nextButton, 1f));
        }
        if (tutorialLayer == 3)
        {
            // Show controller trigger button controls
            controllerInstructionsCG.alpha = 0f;
            controllerInstructionsCG.GetComponentInChildren<TextMeshProUGUI>().text = controllerTutorialTexts[2];
            controllerInstructionsCG.GetComponentInChildren<Image>().sprite = controllerTutorialSprites[2];
            StartCoroutine(FadeCanvasGroup(controllerInstructionsCG, 0f, 1f, fadeInDuration));

            // Show trigger tooltip on the player's controllers
            TooltipHandler tH = Vrsys.NetworkUser.localNetworkUser.GetComponent<TooltipHandler>();
            tH.ShowTriggerTooltip();

            // Set button interactable after delay
            StartCoroutine(SetButtonInteractableAfterDelay(nextButton, 1f));
        }
        if (tutorialLayer == 4) 
        {
            // Hide Controls Tutorial
            controllerInstructionsCG.alpha = 0f;
            controllerInstructionsCG.gameObject.SetActive(false);

            // Show study selector 
            studySelectorCG.gameObject.SetActive(true);
            StartCoroutine(FadeCanvasGroup(studySelectorCG, 0f, 1f, fadeInDuration, enableInteraction: true));

            // Hide next button
            nextButton.gameObject.SetActive(false);

            // Hide skip controls button
            skipButtonCG.gameObject.SetActive(false);

            // Show start and practice buttons
            StartCoroutine(FadeCanvasGroup(startButtonCG, 0f, 1f, fadeInDuration, 0.6f, true));
            StartCoroutine(FadeCanvasGroup(practiceButtonCG, 0f, 1f, fadeInDuration, 0.6f, true));
        }
    }

    public void SelectStudy(int studyIndex)
    {
        selectedStudy = studyIndex;
    }

    public void StartStudyPractice()
    {
        if (selectedStudy == 0)
        {
            // Show no selection warning
            CanvasGroup noSelectionWarningCG = studySelectorCG.transform.Find("NoSelectionWarning").GetComponent<CanvasGroup>();
            StartCoroutine(FadeCanvasGroup(noSelectionWarningCG, 0f, 1f, 0.1f));
            StartCoroutine(FadeCanvasGroup(noSelectionWarningCG, 1f, 0f, 0.1f, 2f));
        }
        else if (selectedStudy == 1)
        {
            // Enable GameObject
            study1PracticeGO.SetActive(true);

            // Hide study menu
            HideStudyMenu();
        }
        else if (selectedStudy == 2)
        {
            // Enable GameObject
            study2PracticeGO.SetActive(true);

            // Hide study menu
            HideStudyMenu();
        } 
    }

    public void StartStudy()
    {
        if (selectedStudy == 0)
        {
            // Show no selection warning
            CanvasGroup noSelectionWarningCG = studySelectorCG.transform.Find("NoSelectionWarning").GetComponent<CanvasGroup>();
            StartCoroutine(FadeCanvasGroup(noSelectionWarningCG, 0f, 1f, 0.1f));
            StartCoroutine(FadeCanvasGroup(noSelectionWarningCG, 1f, 0f, 0.1f, 2f));
        }
        else if (selectedStudy == 1)
        {
            // Enable GameObjects
            study1GO.SetActive(true);

            // Hide study menu
            HideStudyMenu();
        }
        else if (selectedStudy == 2)
        {
            // Enable GameObjects
            study2GO.SetActive(true);

            // Hide study menu
            HideStudyMenu();
        }
    }

    private void HideStudyMenu()
    {
        StartCoroutine(FadeCanvasGroup(studyMenuPanelCG, 1f, 0f, fadeOutDuration, enableInteraction: false));

        StartCoroutine(EnableGOAfterDelay(studySphere, 3f));
    }

    private IEnumerator EnableGOAfterDelay(GameObject gO, float delay)
    {
        yield return new WaitForSeconds(delay);

        gO.SetActive(true);
    }

    private IEnumerator SetButtonInteractableAfterDelay(Button button, float delay)
    {
        yield return new WaitForSeconds(delay);

        button.interactable = true;
    }

    public void OnSphereHoverEntered(HoverEnterEventArgs args)
    {
        // Show  tooltip
        tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
        tooltipHandler.ShowTooltip(showStudyMenuTooltip);

        // Show text panel
        StartCoroutine(FadeCanvasGroup(studySphereTextPanelCG, 0f, 1f, fadeOutDuration));
    }

    public void OnSphereHoverExited(HoverExitEventArgs args)
    {
        // Hide  tooltip
        tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
        tooltipHandler.HideTooltip(showStudyMenuTooltip);

        // Hide text panel
        StartCoroutine(FadeCanvasGroup(studySphereTextPanelCG, 1f, 0f, fadeOutDuration));
    }

    public void OnSphereSelectEntered(SelectEnterEventArgs args)
    {
        // Hide  tooltip
        tooltipHandler = args.interactorObject.transform.root.GetComponent<TooltipHandler>();
        tooltipHandler.HideTooltip(showStudyMenuTooltip);

        // Hide text panel
        StartCoroutine(FadeCanvasGroup(studySphereTextPanelCG, 1f, 0f, fadeOutDuration));

        // Show study menu
        StartCoroutine(FadeCanvasGroup(studyMenuPanelCG, 0f, 1f, fadeInDuration, enableInteraction: true));

        // Hide Sphere
        studySphere.SetActive(false);

        // Disable all menu GOs
        study1PracticeGO.SetActive(false);
        study2PracticeGO.SetActive(false);
        study1GO.SetActive(false);
        study2GO.SetActive(false);
    }


    private IEnumerator FadeCanvasGroup(CanvasGroup cG, float startAlpha, float endAlpha, float duration, float startDelay = 0f, bool enableInteraction = false)
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        float t = 0f;
        while (t < duration)
        {
            cG.alpha = Mathf.Lerp(startAlpha, endAlpha, t /  duration);

            t += Time.deltaTime;
            yield return null;
        }

        cG.alpha = endAlpha;

        if (enableInteraction)
        {
            cG.interactable = true;
            cG.blocksRaycasts = true;
        }
        else
        {
            cG.interactable = false;
            cG.blocksRaycasts = false;
        }
    }
}
